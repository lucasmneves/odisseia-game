// Baixa as 8 animações de personagem JÁ PAGAS do Asset Completion (PXL-012 e PXL-013). NUNCA gera nada.
//
//   node Tools/fetch-paid-anims.js           (precisa de backblaze.pixellab.ai liberado na rede do ambiente)
//
// Lê os URLs na hora com get_character (leitura, sem custo) — os URLs gravados em
// Docs/Art/PixelLab/_pendentes/animacoes_pagas.json levam um carimbo ?t= e podem expirar.
// Destino: o mesmo layout que o build-cast usa — Docs/Characters/Fase<NN>/<Nome>/Attack_east/<Nome>_Attack_NN.png —
// e, para o Odisseu, Docs/Art/PixelLab/PXL-013/_baixados/<Estado>_NN.png (a folha mestra só muda na integração,
// por splice-sheet-state.js). Também marca os quadros no cast.json, como o `build-cast.js fetchanim` faria.
const fs = require('fs'), path = require('path');
const px = require('./pixellab.js');

const ROOT = path.join(__dirname, '..');
const LISTA = JSON.parse(fs.readFileSync(path.join(ROOT, 'Docs/Art/PixelLab/_pendentes/animacoes_pagas.json'), 'utf8'));
const FASE = { Trojan_Soldier: '02', Cicones_Warrior: '03', Circe_Wolf: '08', Shade_Warrior: '09', Suitor: '15', Soldier_Ithaca: '01' };

async function baixar(url, arquivo) {
  const res = await fetch(url);
  if (!res.ok) throw new Error(`HTTP ${res.status} em ${url.slice(0, 80)}`);
  fs.mkdirSync(path.dirname(arquivo), { recursive: true });
  fs.writeFileSync(arquivo, Buffer.from(await res.arrayBuffer()));
}

(async () => {
  const cast = JSON.parse(fs.readFileSync(path.join(ROOT, 'Docs/Characters/cast.json'), 'utf8'));
  let ok = 0;
  for (const a of LISTA) {
    const txt = px.textOf(await px.call('get_character', { character_id: a.character_id, include_preview: false }));
    const L = txt.split('\n');
    // casa pelo GRUPO registrado, não só pelo nome: o Odisseu tem outras animações com nomes parecidos
    const k = L.findIndex(l => l.includes(a.grupo));
    const linha = k >= 0 ? L.slice(k + 1).find(l => /^\s*east:/.test(l)) : null;
    const urls = linha ? linha.match(/https:\/\/[^,\s]+/g) : null;
    if (!urls) { console.log(`  FALTA ${a.personagem} ${a.estado}: grupo ${a.grupo} não encontrado`); continue; }
    const odisseu = a.personagem.startsWith('Odysseus');
    for (let f = 0; f < urls.length; f++) {
      const nn = String(f).padStart(2, '0');
      const arq = odisseu
        ? path.join(ROOT, 'Docs/Art/PixelLab/PXL-013/_baixados', `${a.estado}_${nn}.png`)
        : path.join(ROOT, 'Docs/Characters', 'Fase' + FASE[a.personagem], a.personagem, `${a.estado}_east`, `${a.personagem}_${a.estado}_${nn}.png`);
      await baixar(urls[f], arq);
    }
    if (!odisseu && cast[a.personagem] && cast[a.personagem].anims && cast[a.personagem].anims[a.estado])
      cast[a.personagem].anims[a.estado].frames = urls.length;
    console.log(`  ok ${a.personagem.padEnd(20)} ${a.estado.padEnd(11)} ${urls.length} quadros`);
    ok++;
  }
  fs.writeFileSync(path.join(ROOT, 'Docs/Characters/cast.json'), JSON.stringify(cast, null, 2));
  console.log(`${ok}/${LISTA.length} animações baixadas — 0 gerações`);
})().catch(e => { console.error('ERRO:', e.message); process.exit(1); });
