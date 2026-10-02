// Cliente JSON-RPC para o MCP do PixelLab.
//
// Por que existe: o servidor está registrado em ~/.claude.json sob o escopo C:/Users/luucas
// (a home), não sob a pasta do projeto — então numa sessão aberta aqui as ferramentas do
// PixelLab não aparecem, nem via ToolSearch. O contorno é falar HTTP direto.
// As respostas podem vir como SSE ("data: {...}") ou JSON puro.
const fs = require('fs'), os = require('os'), path = require('path');

// Numa sessão na nuvem não existe o ~/.claude.json da máquina local: a credencial vem do
// ambiente (PIXELLAB_API_KEY, e PIXELLAB_MCP_URL se o endpoint mudar). O host
// api.pixellab.ai também precisa estar liberado na política de rede do ambiente.
function config() {
  if (process.env.PIXELLAB_API_KEY) {
    return {
      url: process.env.PIXELLAB_MCP_URL || 'https://api.pixellab.ai/mcp',
      headers: { Authorization: `Bearer ${process.env.PIXELLAB_API_KEY}` },
    };
  }
  const arquivo = path.join(os.homedir(), '.claude.json');
  if (fs.existsSync(arquivo)) {
    const j = JSON.parse(fs.readFileSync(arquivo, 'utf8'));
    if (j.mcpServers && j.mcpServers.pixellab) return j.mcpServers.pixellab;
    for (const cfg of Object.values(j.projects || {})) {
      const s = cfg.mcpServers && cfg.mcpServers.pixellab;
      if (s) return s;
    }
  }
  throw new Error('servidor pixellab não encontrado: defina PIXELLAB_API_KEY ou registre o MCP em ~/.claude.json');
}

let _session = null;
async function rpc(method, params, { notify = false } = {}) {
  const s = config();
  const headers = { ...s.headers, 'Content-Type': 'application/json', Accept: 'application/json, text/event-stream' };
  if (_session) headers['Mcp-Session-Id'] = _session;
  const body = { jsonrpc: '2.0', method, ...(params ? { params } : {}) };
  if (!notify) body.id = Math.floor(Math.random() * 1e9);

  const res = await fetch(s.url, { method: 'POST', headers, body: JSON.stringify(body) });
  const sid = res.headers.get('mcp-session-id');
  if (sid) _session = sid;
  if (notify) return null;

  const text = await res.text();
  if (!res.ok) throw new Error(`HTTP ${res.status}: ${text.slice(0, 400)}`);
  // SSE: pegar a última linha "data:" que tenha JSON.
  let payload = null;
  if (text.startsWith('event:') || text.includes('\ndata:') || text.startsWith('data:')) {
    for (const line of text.split('\n'))
      if (line.startsWith('data:')) { try { payload = JSON.parse(line.slice(5).trim()); } catch {} }
  } else payload = JSON.parse(text);
  if (!payload) throw new Error(`resposta ilegível: ${text.slice(0, 400)}`);
  if (payload.error) throw new Error(`${method}: ${JSON.stringify(payload.error)}`);
  return payload.result;
}

let _ready = null;
async function connect() {
  if (_ready) return _ready;
  _ready = (async () => {
    await rpc('initialize', {
      protocolVersion: '2024-11-05',
      capabilities: {},
      clientInfo: { name: 'odisseia-tools', version: '1.0.0' },
    });
    await rpc('notifications/initialized', undefined, { notify: true });
  })();
  return _ready;
}

async function listTools() {
  await connect();
  return (await rpc('tools/list', {})).tools;
}

async function call(name, args) {
  await connect();
  const r = await rpc('tools/call', { name, arguments: args });
  if (r && r.isError) throw new Error(`${name} falhou: ${JSON.stringify(r.content).slice(0, 600)}`);
  return r;
}

// O conteúdo textual costuma vir como JSON dentro de content[].text.
function textOf(result) {
  return (result.content || []).filter(c => c.type === 'text').map(c => c.text).join('\n');
}
function jsonOf(result) {
  const t = textOf(result);
  try { return JSON.parse(t); } catch { return t; }
}

module.exports = { listTools, call, textOf, jsonOf, rpc, connect };
