// Dispara vários jobs de imagem em paralelo e espera todos.
//
//   node Tools/pixellab-batch.js <lote.json> <pasta-de-saida>
//
// O lote é um array de { nome, ferramenta, args }. Sequencialmente cinco assets levariam uns
// cinco minutos de espera; em paralelo, pouco mais que o mais lento deles. O limite do
// PixelLab é 8 jobs simultâneos — passar disso devolve erro de validação, que não é cobrado,
// mas custa a rodada.
const fs = require('fs'), path = require('path');
const { generate } = require('./pixellab-image.js');

const LIMITE_DE_JOBS = 8;

async function main() {
  const [loteFile, pasta] = process.argv.slice(2);
  const lote = JSON.parse(fs.readFileSync(loteFile, 'utf8'));
  if (lote.length > LIMITE_DE_JOBS) {
    console.error(`lote de ${lote.length} passa do limite de ${LIMITE_DE_JOBS} jobs simultâneos`);
    process.exit(2);
  }

  fs.mkdirSync(pasta, { recursive: true });
  const resultados = await Promise.allSettled(lote.map(item =>
    generate(item.ferramenta || 'create_image_pixen', item.args, path.join(pasta, item.nome))));

  let falhas = 0;
  for (let i = 0; i < lote.length; i++) {
    const r = resultados[i];
    if (r.status === 'fulfilled') console.log(`  OK   ${lote[i].nome}`);
    else { console.log(`  FALHA ${lote[i].nome}: ${r.reason.message}`); falhas++; }
  }
  console.log(`${lote.length - falhas}/${lote.length} concluídos`);
  process.exit(falhas ? 1 : 0);
}

main().catch(e => { console.error('ERRO:', e.message); process.exit(1); });
