// Efeitos das Sereias, construídos por código.
//
//   node Tools/build-sereias-fx.js
//
// A tese da fase é "belo, hipnótico, perigo escondido". O perigo já está nos destroços; o que
// falta é o HIPNÓTICO, e ele não pode ser um monstro nem um aviso vermelho — se o jogo avisar,
// a fase deixa de funcionar. Então o canto é luz: arcos concêntricos pálidos, alpha baixo,
// saindo da ilha. Bonito primeiro, errado depois.
//
// Arco concêntrico é geometria pura. Gerar isso seria pagar por algo que uma equação desenha
// melhor e que ladrilha sem emenda.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Sereias');
const ramps = loadPalette(path.join(ENV, 'Palette/SEREIAS_PALETTE.gpl'));
const UN = 42.857143;

function escrever(grupo, nome, img) {
  const dir = path.join(ENV, grupo);
  fs.mkdirSync(dir, { recursive: true });
  p.write(path.join(dir, nome + '.png'), img);
  console.log(`${nome.padEnd(22)} ${grupo.padEnd(10)} ${img.width}x${img.height}  ` +
    `${(img.width / UN).toFixed(2)}x${(img.height / UN).toFixed(2)} un`);
}

/**
 * O canto: arcos concêntricos saindo de um foco na base esquerda.
 *
 * Alpha máximo 150 de 255. O briefing proíbe que o efeito esconda jogador, inimigo ou
 * plataforma, e a veladura fraca é o que separa "atmosfera" de "cortina".
 */
function canto() {
  const W = 512, H = 320;
  const img = p.blank(W, H);
  // Turquesa, e não branco. Os arcos brancos ficaram INVISÍVEIS: o céu e a areia desta fase
  // são creme claro, e branco sobre creme não tem contraste nenhum. O frio da água contra o
  // quente da praia é o único par que aparece sem precisar subir o alpha a ponto de virar
  // cortina — e continua saindo da paleta, sem cor de alerta.
  const claro = ramps['Agua rasa'][0], medio = ramps['Agua rasa'][2], quente = ramps['Espuma'][1];

  const fx = 0, fy = H;                     // foco: a ilha, fora do quadro pela esquerda-baixo
  const passo = 26;                         // distância entre arcos
  for (let y = 0; y < H; y++) {
    for (let x = 0; x < W; x++) {
      const d = Math.hypot(x - fx, (y - fy) * 1.35);
      const fase = (d % passo) / passo;
      // Um arco fino por período: seno estreito, não onda cheia, para o efeito ler como
      // pulso e não como listra.
      const forca = Math.max(0, Math.sin(fase * Math.PI) ** 6);
      if (forca < 0.02) { continue; }

      // Some com a distância: o canto é mais forte perto da fonte.
      const queda = Math.max(0, 1 - d / (W * 1.15));
      const a = Math.round(150 * forca * queda ** 1.4);
      if (a <= 2) { continue; }

      const cor = fase < 0.5 ? claro : (d < W * 0.45 ? quente : medio);
      const o = (y * W + x) * 4;
      img.data[o] = cor[0]; img.data[o + 1] = cor[1]; img.data[o + 2] = cor[2]; img.data[o + 3] = a;
    }
  }
  return img;
}

/**
 * Véu de luz quente para o fundo. Não é névoa: é o ar brilhante de um dia claro de mar, que é
 * o que separa os planos numa fase SEM a perspectiva aérea por tint — aqui as seis famílias de
 * matiz já fazem esse trabalho, e o véu só acrescenta a sensação de calor.
 */
function veu() {
  const W = 512, H = 192;
  const img = p.blank(W, H);
  const cor = ramps['Areia e calcario'][0];
  for (let y = 0; y < H; y++) {
    // Mais forte embaixo, junto ao horizonte, que é de onde a luz vem numa vista de mar.
    const a = Math.round(46 * (y / H) ** 1.8);
    for (let x = 0; x < W; x++) {
      const o = (y * W + x) * 4;
      img.data[o] = cor[0]; img.data[o + 1] = cor[1]; img.data[o + 2] = cor[2]; img.data[o + 3] = a;
    }
  }
  return img;
}

console.log('asset'.padEnd(23) + 'grupo'.padEnd(11) + 'tamanho   unidades');
escrever('Special', 'sereias_song', canto());
escrever('VFX', 'sereias_haze', veu());
