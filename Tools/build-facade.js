// Fachada larga a partir de uma geração do PixelLab: recorta o fundo, quantiza na paleta e
// mede a porta.
//
//   node Tools/build-facade.js <fonte.png> <saida.png> <corteMadeira>
//
// Por que estes prédios precisaram de geração, ao contrário do resto da arquitetura: têm
// 17,8 un de largura e o maior material do banco (o corpo da casa v3) tem 4,1. Compor daria
// uma casa esticada — falta a silhueta própria (colunata e friso no palácio, cornija e
// portões duplos no armazém).
//
// Cada um custou 1 geração no `pixen`. O `pro` acima de 170 px devolve UM candidato por
// 20–40 gerações, e 768 px de largura cairia nesse caso. Os 768 px de lado máximo do `pixen`
// são, aliás, o que define a largura destas fachadas.
//
// O fundo vem opaco: o `no_background` do pixen cai em qualquer prompt com enquadramento.
// Sai por Tools/cutout.js.
const path = require('path');
const C = require('./compose.js');
const { remap, materialArquitetura } = require('./ramp-map.js');
const { cutout } = require('./cutout.js');
const { acharPorta } = require('./scale-proof.js');
const p = C.p;

const [fonte, saida, corteArg] = process.argv.slice(2);
if (!fonte || !saida) {
  console.error('uso: node Tools/build-facade.js <fonte.png> <saida.png> [corteMadeira]');
  process.exit(2);
}

// O corte entre madeira e pedra é PROPRIEDADE DO ASSET, não do pipeline, e por isso é
// argumento. Medido no palácio: porta em 0,60-0,69 e calcário em 0,356-0,43, corte 0,50.
// No armazém: porta em 0,50-0,53 e parede em 0,16-0,26, corte 0,38. Usar o padrão de 0,36 no
// palácio fazia metade da parede virar madeira e o calcário sair mosqueado de malva.
const corte = corteArg ? Number(corteArg) : 0.36;

const bruto = p.read(fonte);
const { img: recortado, removed } = cutout(bruto, 14, { enclosed: true });
const b = p.bounds(recortado);
const final = remap(p.crop(recortado, b.x0, b.y0, b.w, b.h), C.ramps,
  { classify: materialArquitetura(corte) });
p.write(saida, final);

// A busca da porta vem do scale-proof, não de uma cópia local: uma cópia desatualizada aqui
// já reportou meia porta dupla e "reprovou" o palácio por 1 px.
const porta = acharPorta(final);
const UN = C.UN;

console.log(`${path.relative(C.ROOT, saida)}  ${final.width}x${final.height}px = ${C.un(final.width)} x ${C.un(final.height)} un`);
console.log(`  fundo removido ${(100 * removed / (bruto.width * bruto.height)).toFixed(1)}%, corte madeira/pedra ${corte}`);

if (porta) {
  const w = porta.x1 - porta.x0 + 1, h = porta.y1 - porta.y0 + 1;
  const desvio = ((porta.x0 + porta.x1) / 2 - final.width / 2) / UN;
  console.log(`  porta ${w}x${h}px = ${(w / UN).toFixed(2)} x ${(h / UN).toFixed(2)} un, ${desvio.toFixed(2)} un do centro`);
} else {
  console.log('  nenhuma porta de madeira encontrada');
}
