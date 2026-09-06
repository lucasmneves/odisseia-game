// Gera os tilesets Wang de uma fase e baixa folha + JSON.
//
//   node Tools/build-tilesets.js <lote.json> <pasta-de-saida>
//
// O lote é um array de { nome, args }. O primeiro kit vira referência dos demais pelo
// `base_tile_id`, que é o que conecta os kits visualmente. Em Ítaca esse encadeamento NÃO
// aconteceu — o regex da sessão anterior não casou o formato da resposta e os quatro kits
// saíram independentes. Aqui o id é extraído do JSON quando existe e do texto como reserva,
// e o script diz qual dos dois usou.
//
// Custo: 2 ou 3 gerações por kit, nunca 1.
const fs = require('fs'), path = require('path');
const px = require('./pixellab.js');

const sleep = (ms) => new Promise(r => setTimeout(r, ms));

function acharId(res) {
  const dados = px.jsonOf(res);
  if (dados && typeof dados === 'object') {
    for (const chave of ['tileset_id', 'id', 'base_tile_id']) {
      if (typeof dados[chave] === 'string') return { id: dados[chave], via: 'JSON' };
    }
  }
  const m = px.textOf(res).match(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i);
  return m ? { id: m[0], via: 'texto' } : { id: null, via: null };
}

// O id de encadeamento pode ser o do tileset ou o de um tile dele; a resposta do get diz qual.
function acharBaseTileId(texto) {
  const m = texto.match(/base_tile_id[^:]*:\s*([0-9a-f-]{36})/i);
  return m ? m[1] : null;
}

async function esperar(id, saidaBase) {
  for (let i = 0; i < 90; i++) {
    await sleep(8000);
    const g = await px.call('get_sidescroller_tileset', { tileset_id: id });
    const texto = px.textOf(g);
    if (/status: failed|error:/i.test(texto)) throw new Error(texto.slice(0, 200));

    // O get NÃO devolve a folha inline, ao contrário das ferramentas de imagem: manda duas
    // URLs. Esperar imagem inline aqui trava o polling até o tempo esgotar.
    const png = (texto.match(/download_png:\s*(\S+)/) || [])[1];
    const meta = (texto.match(/download_metadata:\s*(\S+)/) || [])[1];
    if (/status: completed/.test(texto) && png) {
      fs.writeFileSync(saidaBase + '.png', Buffer.from(await (await fetch(png)).arrayBuffer()));
      if (meta) fs.writeFileSync(saidaBase + '.json', await (await fetch(meta)).text());
      return { texto, base: acharBaseTileId(texto) };
    }
    process.stdout.write('  ' + texto.split(String.fromCharCode(10))[0].slice(0, 60) + '\r');
  }
  throw new Error('tempo esgotado');
}

async function main() {
  const [loteFile, pasta] = process.argv.slice(2);
  const lote = JSON.parse(fs.readFileSync(loteFile, 'utf8'));
  fs.mkdirSync(pasta, { recursive: true });

  let baseId = null;
  for (const item of lote) {
    const args = { ...item.args };
    if (baseId) args.base_tile_id = baseId;

    const criado = await px.call('create_sidescroller_tileset', args);
    const { id, via } = acharId(criado);
    if (!id) throw new Error('sem tileset_id: ' + px.textOf(criado).slice(0, 300));
    console.log(`${item.nome}: ${id} (id via ${via})${baseId ? ' — encadeado' : ''}`);

    const { base } = await esperar(id, path.join(pasta, item.nome));
    console.log(`  pronto${base ? `, base_tile_id ${base}` : ', sem base_tile_id na resposta'}`);
    if (!baseId && base) baseId = base;
  }
  console.log(`${lote.length} tilesets gerados em ${pasta}`);
}

main().catch(e => { console.error('ERRO:', e.message); process.exit(1); });
