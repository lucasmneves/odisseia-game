// Dispara uma ferramenta de imagem do PixelLab e espera o job, salvando os PNGs.
//
//   node Tools/pixellab-image.js <ferramenta> <args.json> <prefixo-de-saída>
//
// As ferramentas de imagem são assíncronas: devolvem job_id e o resultado sai por get_image.
// Jobs com erro de validação e jobs falhados NÃO são cobrados — errar o schema é de graça.
const fs = require('fs'), path = require('path');
const px = require('./pixellab.js');

const sleep = (ms) => new Promise(r => setTimeout(r, ms));

async function generate(tool, args, outPrefix, { timeoutMs = 15 * 60000 } = {}) {
  const started = await px.call(tool, args);
  const info = px.jsonOf(started);
  // Casar o UUID inteiro. Um fallback do tipo /job_idD+(w+)/ come os dígitos iniciais do
  // UUID (D é não-dígito, e "af4122ca" começa com letras) e devolve um id truncado que o
  // get_image rejeita — com o job já disparado e cobrado.
  const jobId = info.job_id || info.jobId ||
    (px.textOf(started).match(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i) || [])[0];
  if (!jobId) throw new Error(`sem job_id na resposta: ${px.textOf(started).slice(0, 400)}`);
  console.log(`job ${jobId} disparado (${tool})`);

  const t0 = Date.now();
  for (let i = 0; ; i++) {
    await sleep(i < 3 ? 5000 : 12000);
    const res = await px.call('get_image', { job_id: jobId });
    const images = (res.content || []).filter(c => c.type === 'image');
    const text = px.textOf(res);
    if (images.length) {
      const files = images.map((img, n) => {
        const f = `${outPrefix}${images.length > 1 ? `_${n}` : ''}.png`;
        fs.mkdirSync(path.dirname(f), { recursive: true });
        fs.writeFileSync(f, Buffer.from(img.data, 'base64'));
        return f;
      });
      console.log(`pronto em ${Math.round((Date.now() - t0) / 1000)}s: ${files.join(', ')}`);
      return { jobId, files, text };
    }
    if (/fail|error/i.test(text)) throw new Error(`job falhou: ${text.slice(0, 400)}`);
    if (Date.now() - t0 > timeoutMs) throw new Error(`tempo esgotado após ${Math.round((Date.now() - t0) / 1000)}s`);
    process.stdout.write(`  ...${Math.round((Date.now() - t0) / 1000)}s\r`);
  }
}

if (require.main === module) {
  const [tool, argsFile, outPrefix] = process.argv.slice(2);
  generate(tool, JSON.parse(fs.readFileSync(argsFile, 'utf8')), outPrefix)
    .catch(e => { console.error('ERRO:', e.message); process.exit(1); });
}
module.exports = { generate };
