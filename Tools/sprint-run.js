// Executa o sprint de produção PixelLab: todos os lotes de uma pasta, em ordem de prioridade.
//
//   node Tools/sprint-run.js <pasta-de-lotes> --dry            conta itens e custo, não gera nada
//   node Tools/sprint-run.js <pasta-de-lotes> --ping           lista as ferramentas do MCP (custo 0)
//   node Tools/sprint-run.js <pasta-de-lotes> [--max 300] [--only P0] [--lote 16_final]
//
// Cada lote é { "prioridade": "P0", "saida": "Docs/...", "itens": [ { nome, ferramenta, args, custo? } ] }.
// O formato dos itens é o mesmo de `pixellab-batch.js`; a pasta de saída sai do lote.
//
// Por que existe, e não só o pixellab-batch:
// - **Rodar de novo nunca cobra duas vezes.** Item cujo PNG já está na pasta de saída é pulado;
//   se a sessão cair no meio, basta repetir o comando.
// - **Teto de gerações** (`--max`): o sprint inteiro passa do saldo de propósito, ordenado do
//   mais importante ao menos. O teto corta pelo fim da fila, nunca pelo meio de um lote.
// - **Limite de 8 jobs simultâneos** respeitado por fatia; erro de validação não é cobrado,
//   mas desperdiça a rodada.
// - **Xadrez pintado acusado na hora** (`temXadrezPintado`): olhar não distingue alpha real de
//   xadrez desenhado — só contar pixels opacos distingue.
const fs = require('fs'), path = require('path');

const ROOT = path.join(__dirname, '..');
const LIMITE_DE_JOBS = 8;
// Custo por chamada, do ESTADO_ATUAL §3. O `pro` varia de 20 a 40: conta-se o pior caso.
const CUSTO = { create_image_pixen: 1, create_image_pro: 40, create_image_pixflux: 1 };

function argumentos() {
  const a = process.argv.slice(2);
  const opt = { pasta: a[0], dry: a.includes('--dry'), ping: a.includes('--ping'), max: Infinity, only: null, lote: null };
  const val = (f) => { const i = a.indexOf(f); return i >= 0 ? a[i + 1] : null; };
  if (val('--max')) opt.max = Number(val('--max'));
  opt.only = val('--only');
  opt.lote = val('--lote');
  return opt;
}

function carregarLotes(pasta) {
  return fs.readdirSync(pasta).filter(f => f.endsWith('.json')).sort().map(f => {
    const lote = JSON.parse(fs.readFileSync(path.join(pasta, f), 'utf8'));
    return { arquivo: f, ...lote };
  }).sort((x, y) => (x.prioridade || 'P9').localeCompare(y.prioridade || 'P9'));
}

const custoDe = (item) => item.custo || CUSTO[item.ferramenta || 'create_image_pixen'] || 1;

function jaFeito(saida, nome) {
  const dir = path.join(ROOT, saida);
  if (!fs.existsSync(dir)) return false;
  return fs.readdirSync(dir).some(f => f === `${nome}.png` || (f.startsWith(`${nome}_`) && /_\d+\.png$/.test(f)));
}

async function main() {
  const opt = argumentos();
  if (!opt.pasta) { console.error('uso: node Tools/sprint-run.js <pasta-de-lotes> [--dry|--ping] [--max N] [--only P0] [--lote nome]'); process.exit(2); }

  if (opt.ping) {
    const px = require('./pixellab.js');
    const tools = await px.listTools();
    for (const t of tools) console.log(`  ${t.name}`);
    console.log(`${tools.length} ferramentas — conexão ok, nada foi cobrado`);
    return;
  }

  let lotes = carregarLotes(opt.pasta);
  if (opt.only) lotes = lotes.filter(l => l.prioridade === opt.only);
  if (opt.lote) lotes = lotes.filter(l => l.arquivo.includes(opt.lote));

  // Planejar antes de gastar: o que falta, quanto custa, onde o teto corta.
  let acumulado = 0;
  const fila = [];
  for (const lote of lotes) {
    const pendentes = lote.itens.filter(i => !jaFeito(lote.saida, i.nome));
    const custo = pendentes.reduce((s, i) => s + custoDe(i), 0);
    const cabe = acumulado + custo <= opt.max;
    console.log(`${cabe ? '  ' : '✗ '}${lote.prioridade}  ${lote.arquivo.padEnd(34)} ${String(pendentes.length).padStart(2)}/${lote.itens.length} itens  ${String(custo).padStart(4)} ger.${cabe ? '' : '  (passa do teto)'}`);
    if (!cabe || !pendentes.length) continue;
    acumulado += custo;
    fila.push({ lote, pendentes });
  }
  console.log(`total a gerar: ${acumulado} gerações em ${fila.length} lotes`);
  if (opt.dry) return;

  const { generate } = require('./pixellab-image.js');
  const png = require('./png.js');
  const { temXadrezPintado } = require('./checker-cut.js');
  let falhas = 0, xadrez = [];

  for (const { lote, pendentes } of fila) {
    const dir = path.join(ROOT, lote.saida);
    fs.mkdirSync(dir, { recursive: true });
    console.log(`\n== ${lote.arquivo} → ${lote.saida}`);
    for (let i = 0; i < pendentes.length; i += LIMITE_DE_JOBS) {
      const fatia = pendentes.slice(i, i + LIMITE_DE_JOBS);
      const res = await Promise.allSettled(fatia.map(item =>
        generate(item.ferramenta || 'create_image_pixen', item.args, path.join(dir, item.nome))));
      res.forEach((r, k) => {
        const nome = fatia[k].nome;
        if (r.status !== 'fulfilled') { console.log(`  FALHA ${nome}: ${r.reason.message}`); falhas++; return; }
        for (const f of r.value.files) {
          try { if (temXadrezPintado(png.read(f))) xadrez.push(path.relative(ROOT, f)); } catch {}
        }
        console.log(`  OK    ${nome}`);
      });
    }
  }

  if (xadrez.length) {
    console.log(`\nxadrez pintado (canto opaco) — passar em Tools/checker-cut.js ou Tools/cutout.js:`);
    for (const f of xadrez) console.log(`  ${f}`);
  }
  console.log(`\n${falhas ? `${falhas} falha(s) — job falhado não é cobrado; rodar de novo retoma` : 'sprint concluído sem falhas'}`);
  process.exit(falhas ? 1 : 0);
}

main().catch(e => { console.error('ERRO:', e.message); process.exit(1); });
