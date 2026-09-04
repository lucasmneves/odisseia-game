// Reconstrói o Grupo 9 (navios) a partir do casco gerado, e verifica.
// Os PNGs em Ships/ são derivados; a única fonte que não dá para refazer de graça é
// _hull_v3_cut.png, o casco recortado da geração do pixen.
//
//   node Tools/build-ships.js
const { execFileSync } = require('child_process');
const path = require('path');

const S = 'Docs/Environment_Ithaca/Ships';
const run = (script, ...args) =>
  process.stdout.write(execFileSync(process.execPath, [path.join(__dirname, script), ...args], { encoding: 'utf8' }));

console.log('== construindo ==');
run('build-ship.js');              // vela içada — a partida
run('build-ship.js', '--furled');  // vela enrolada — atracado no cais

console.log('\n== verificando ==');
run('palette-check.js', `${S}/ithaca_ship_01.png`, `${S}/ithaca_ship_01_furled.png`);
run('scale-probe.js', `${S}/ithaca_ship_01.png`);
run('ship-proof.js');
