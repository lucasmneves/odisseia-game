// Reconstrói o Grupo 3 inteiro a partir da fonte e verifica tudo.
// Os PNGs em Architecture/ são derivados: podem ser apagados e refeitos por este script.
//
//   node Tools/build-architecture.js
const { execFileSync } = require('child_process');
const path = require('path');

const A = 'Docs/Environment_Ithaca/Architecture';
const assets = ['ithaca_house_small_01', 'ithaca_column_01', 'ithaca_wall_low_01',
  'ithaca_house_odysseus_01', 'ithaca_palace_01', 'ithaca_warehouse_01',
  'ithaca_gate_01', 'ithaca_gatepost_01'];
const run = (script, ...args) => {
  process.stdout.write(execFileSync(process.execPath, [path.join(__dirname, script), ...args], { encoding: 'utf8' }));
};

console.log('== construindo ==');
run('build-house.js');            // casa pequena: v3 recortada + laje plana
run('build-column.js');           // coluna dórica, desenhada por código
run('build-wall.js');             // muro baixo, espelho da parede da v3
run('build-house-odysseus.js');   // casa de Odisseu: parede + porta + janelas + pórtico
  // As duas fachadas largas: recorte do fundo + quantização da geração. O corte madeira/pedra
  // é argumento porque é propriedade do asset — medido em 0,50 no palácio e 0,38 no armazém.
  run('build-facade.js', `${A}/_fonte_palacio_pixen.png`, `${A}/ithaca_palace_01.png`, '0.50');
  run('build-facade.js', `${A}/_fonte_armazem_pixen.png`, `${A}/ithaca_warehouse_01.png`, '0.38');

console.log('\n== verificando ==');
run('palette-check.js', ...assets.map(a => `${A}/${a}.png`));
run('seam-test.js', `${A}/ithaca_wall_low_01.png`);
run('scale-proof.js', `${A}/ithaca_house_small_01.png`, `${A}/_prova_escala.png`);
run('scale-proof.js', `${A}/ithaca_house_odysseus_01.png`, `${A}/_prova_escala_odisseu.png`);
run('preview-architecture.js');
