// Executor do PixelLab para o Asset Completion: dispara, registra o job no livro-razão e espera.
//
//   node Tools/pxl.js gen <ID> <nome> <ferramenta> '<args JSON>'      → Docs/Art/PixelLab/<ID>/_candidatos/<nome>.png
//   node Tools/pxl.js anim <ID> <nome> <quadro.png> '<ação>' [quadros] → <nome>_00.png … (00 = o quadro de entrada)
//   node Tools/pxl.js fetch <ID> <nome> <job_id>                       → recupera um job já cobrado
//
// Por que existe: na Fase 16 duas respostas caíram no polling depois de o job ter sido COBRADO, e o
// pixellab-batch só imprimia o último job_id — o primeiro "recuperado" era de outro asset. Aqui todo
// disparo vai para `Docs/Art/PixelLab/_ledger.tsv` ANTES do polling, e falha de rede no polling repete
// só o polling (o disparo nunca é repetido automaticamente).
const fs = require('fs'), path = require('path');
const px = require('./pixellab.js');

const ROOT = path.join(__dirname, '..');
const BASE = path.join(ROOT, 'Docs/Art/PixelLab');
const LEDGER = path.join(BASE, '_ledger.tsv');
const sleep = (ms) => new Promise(r => setTimeout(r, ms));
const UUID = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i;

function registrar(campos) {
  fs.mkdirSync(BASE, { recursive: true });
  if (!fs.existsSync(LEDGER)) fs.writeFileSync(LEDGER, 'quando\tid\tnome\tferramenta\tjob\tstatus\n');
  fs.appendFileSync(LEDGER, [new Date().toISOString(), ...campos].join('\t') + '\n');
}

async function comRetentativa(fn, vezes = 6) {
  for (let i = 0; ; i++) {
    try { return await fn(); } catch (e) {
      if (i >= vezes || !/fetch failed|ECONNRESET|socket|network|timeout|HTTP 5/i.test(e.message)) throw e;
      await sleep(2000 * (i + 1));
    }
  }
}

async function esperar(id, nome, job, pasta, multiplos) {
  const t0 = Date.now();
  for (let i = 0; ; i++) {
    await sleep(i < 3 ? 5000 : 10000);
    const res = await comRetentativa(() => px.call('get_image', { job_id: job }));
    const imgs = (res.content || []).filter(c => c.type === 'image');
    const texto = px.textOf(res);
    if (imgs.length) {
      fs.mkdirSync(pasta, { recursive: true });
      const arquivos = imgs.map((img, n) => {
        const f = path.join(pasta, `${nome}${multiplos || imgs.length > 1 ? '_' + String(n).padStart(2, '0') : ''}.png`);
        fs.writeFileSync(f, Buffer.from(img.data, 'base64'));
        return f;
      });
      registrar([id, nome, '-', job, `ok ${imgs.length} img ${Math.round((Date.now() - t0) / 1000)}s`]);
      return arquivos;
    }
    if (/fail|error/i.test(texto) && !/processing|pending|queued/i.test(texto)) {
      registrar([id, nome, '-', job, 'falhou']);
      throw new Error(`job ${job} falhou: ${texto.slice(0, 300)}`);
    }
    if (Date.now() - t0 > 15 * 60000) throw new Error(`tempo esgotado no job ${job}`);
  }
}

async function disparar(id, nome, ferramenta, args) {
  const r = await px.call(ferramenta, args);                 // sem retentativa: disparo nunca se repete sozinho
  const info = px.jsonOf(r);
  const job = (info && (info.job_id || info.jobId)) || (px.textOf(r).match(UUID) || [])[0];
  if (!job) throw new Error(`sem job_id: ${px.textOf(r).slice(0, 300)}`);
  registrar([id, nome, ferramenta, job, 'disparado']);
  console.log(`  ${id} ${nome}: job ${job}`);
  return job;
}

async function main() {
  const [cmd, id, nome, ...resto] = process.argv.slice(2);
  const pasta = id ? path.join(BASE, id, '_candidatos') : '';
  if (cmd === 'gen') {
    const [ferramenta, json] = resto;
    const job = await disparar(id, nome, ferramenta, JSON.parse(json));
    const f = await esperar(id, nome, job, pasta, false);
    console.log('  pronto: ' + f.map(x => path.relative(ROOT, x)).join(', '));
  } else if (cmd === 'anim') {
    const [quadro, acao, quadros = '8'] = resto;
    const b64 = fs.readFileSync(quadro).toString('base64');
    const job = await disparar(id, nome, 'animate_image', {
      first_frame_base64: b64, action: acao, frame_count: +quadros, no_background: true });
    const f = await esperar(id, nome, job, pasta, true);
    console.log(`  pronto: ${f.length} quadros em ${path.relative(ROOT, pasta)}`);
  } else if (cmd === 'fetch') {
    const f = await esperar(id, nome, resto[0], pasta, false);
    console.log('  recuperado: ' + f.map(x => path.relative(ROOT, x)).join(', '));
  } else if (cmd === 'saldo') {
    console.log(px.textOf(await comRetentativa(() => px.call('get_balance', {}))).split('\n').slice(1, 3).join(' | '));
  } else {
    console.error('uso: gen | anim | fetch | saldo'); process.exit(2);
  }
}
main().catch(e => { console.error('ERRO:', e.message); process.exit(1); });
