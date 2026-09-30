# CILA_ENVIRONMENT_MASTER — Fase 11_CilaCaribdis

**Conceito A — Estreito de basalto — aprovado em 2026-09-09.**
Paleta: `Palette/CILA_PALETTE.gpl` · 10 rampas, 40 cores

> A fase em que o quadro FECHA. O que a distingue de Cytera (04) e de Sereias (10) não é
> "mais tempestade": é claustrofobia, e ela é medida.

---

## 1. Identidade aprovada, e por que a medição mudou a decisão

Três conceitos gerados, e a leitura a olho apontava para o A. A medição
(`Tools/concept-metrics.js`) contradisse metade dessa leitura:

| | Cores | Faixas de valor | Famílias de matiz | Veredito |
|---|---|---|---|---|
| **A — Estreito** | 65 | **5 de 10** | **5** (única com família quente) | **Direção aprovada** |
| B — Passagem | 50 | **9 de 10** | 3 | **Recusado como direção**, aprovado como fonte de VALOR |
| C — Garganta | 45 | 8 de 10 | 4 | Vocabulário do redemoinho e das ondas |

**O Conceito A tem a identidade certa e a estrutura de valor errada.** Ele mede 5 de 10 faixas
ocupadas, com **27,3% dos pixels num único quase-preto (`#090909`)**. Isso é a falha de
Lestrigões de cabeça para baixo — lá uma massa cinza única, aqui uma massa PRETA única — e o
briefing proíbe explicitamente a fase completamente preta.

**O Conceito B foi recusado como direção** porque voltou em alvenaria de blocos, o defeito já
documentado em Sereias. Mas é ele que tem os tons médios de rocha que o A não tem, e é de lá
que sai a rampa `Rocha jogavel`.

---

## 2. A leitura do gameplay é um problema de VALOR, e o número está travado

| | L médio |
|---|---|
| Tileset jogável (`Rocha jogavel`) | **0,43** |
| Parede de basalto de meio de campo | **0,20** |
| Distância | **0,23** |

É essa distância que faz a plataforma saltar da falésia numa fase inteira de pedra escura.
Nada pode encostar nesses dois valores sem refazer a conta.

**A parede NÃO é quantizada — é escurecida por fator 0,72.** Quantizá-la na rampa `Basalto`
(4 passos, faixa de 0,18) a transformou numa mancha preta sem separação entre colunas, duas
vezes. Uma rampa estreita demais não escurece um asset: ela o APAGA. A medida que resolveu foi
descobrir que a parede crua já media L 0,278 contra 0,432 do chão — ela já era mais escura, e a
quantização não corrigia nada.

---

## 3. Dez rampas, e o ouro é a única família quente

`Contorno` · `Bronze` (as duas do personagem, compartilhadas) · `Basalto` · `Rocha jogavel` ·
`Agua funda` · `Agua agitada` · `Espuma` · `Ceu de tempestade` · `Relampago` · `Madeira do navio`

Numa fase de basalto preto, mar teal e céu violeta tudo é frio. O relâmpago e a madeira do
navio são o único calor — e por isso o navio, que é plano de jogo, se destaca do fundo por
MATIZ e não só por valor.

---

## 4. A geração falhou três vezes no mesmo eixo, e a saída foi parar de gerar

A falésia com boca de caverna e o pináculo voltaram como ALVENARIA em três tentativas —
blocos cortados, arco de aduelas, degraus empilhados —, **inclusive com a formulação que
corrigiu o mesmo defeito em Sereias** (`not built by anyone, no bricks, no masonry`).

O que aquela formulação não cobre: **"boca de caverna", "arco" e "prateleira de rocha" já são
substantivos de arquitetura.** O modelo lê estrutura e desenha estrutura; negar depois não
desfaz.

A saída é a da casa de Ítaca (ESTADO_ATUAL §4): quando a geração falha sempre no mesmo eixo,
construir. `cila_wall_columnar` voltou perfeito, então:

| Asset | Como foi feito | Custo |
|---|---|---|
| `cila_crag_stack` | fatia de 96px da parede | **0** |
| `cila_cave_cliff` | a parede com um vão escavado em dois passos secos | **0** |

O vão é escavado com borda DURA e aro de um passo. A primeira versão usava degradê e leu como
borrão de fuligem: em pixel art o que dá profundidade é o contraste entre dois valores chapados
com borda irregular, não a interpolação entre eles.

---

## 5. O xadrez que o modelo PINTA — e por que o olho não pega

`cila_wall_columnar` voltou com **0% de pixels transparentes**: o canto superior esquerdo tem
576 de 576 opacos, em dois cinzas alternados de ~7px de passo. O modelo desenhou o xadrez de
transparência como conteúdo.

**O visualizador de PNG mostra alpha real como xadrez cinza, exatamente igual a um xadrez
pintado.** Olhar não distingue os dois — só contar pixels opacos distingue.

E o `cutout.js` genérico não resolve: os cinzas do xadrez medem o mesmo valor das faces
iluminadas do basalto, então o flood-fill por tolerância atravessou o contorno e **removeu
50,7% da imagem**, devolvendo um esqueleto. A ferramenta certa é `Tools/checker-cut.js`, que
separa por ESTRUTURA — referências só da linha 0, tolerância 6, semeadura pelo topo e laterais.
Removeu 17,5%, que é a área exata dos dois cantos.

---

## 6. Caribdis

Folha de **8 quadros** construída por `Tools/build-cila-fx.js` e tocada pelo `SpriteAnimator`
que já existe — nenhum sistema novo, nenhum shader, nenhum ParticleSystem.

O redemoinho é radialmente simétrico, então girar não precisa de quadros novos: basta
reamostrar o mesmo quadro em coordenadas ELÍPTICAS deslocando o ângulo. Oito quadros do único
asset gerado, ciclo perfeito (o deslocamento total é 2π), custo zero. Amostragem por vizinho
mais próximo — interpolar produziria o anti-aliasing que a direção de arte proíbe.

O recorte dele **não** é `cutout`: a cor do fundo é a mesma da garganta do funil, e o
flood-fill vazou e comeu 64%. O que separa fundo de objeto ali é GEOMETRIA, e a máscara é
elíptica.

---

## 7. Assets

| Grupo | Arquivo | Origem |
|---|---|---|
| Background | `cila_bg_walls_far.png` | gerado (não quantizado) |
| Ocean | `cila_sea_churn.png` | gerado, céu cortado, **espelhado** |
| Midground | `cila_wall_columnar.png` | gerado, xadrez removido, escurecido 0,72 |
| Rocks | `cila_crag_stack.png` | **fatia da parede** |
| Scylla | `cila_cave_cliff.png` | **parede escavada** |
| Charybdis | `cila_charybdis_funnel.png` + `FX_Charybdis.png` (8 quadros) | gerado + girado por código |
| Gameplay | `cila_tiles_rock.png`, `cila_rock_ground_top/body.png` | tileset Wang |
| Shipwrecks | `cila_wreck_ribs.png` | gerado |
| Props | `cila_debris_timbers.png` | gerado |

**Reutilizados de Cytera (04):** `cytera_deck_planks`, `cytera_mast_sail`, `cytera_rain`,
`cytera_storm_cloud`. É a mesma embarcação e a mesma tempestade da viagem — desenhar segundas
seria inventar uma segunda frota e um segundo clima.

**Reutilizado do projeto:** `StormAmbience` (chuva, clarões e tremor), o mesmo componente que a
fase 04 usa.

---

## 8. Montagem na Unity

`CilaCaribdisSceneDresser` — mesmo desenho dos vestidores das fases 02 a 10.

- **5 paredes** de basalto em parallax 0,30, espaçadas para o céu do fundo continuar aparecendo
  como uma fresta. Com sete elas se encavalavam e a fresta sumia; claustrofobia precisa da
  fresta, senão não é corredor, é parede.
- **5 pináculos** em 0,20 com recuo em valor (perspectiva aérea de Lestrigões/Circe).
- As dez peças penduram em **dois contêineres com um `ParallaxLayer` cada**. Antes eram 13
  camadas contra 5 a 8 das outras fases; agora são 5.
- A falésia de Cila é posicionada **pelo marcador**, com o vão alinhado à cabeça dele. Fixada à
  mão, a caverna caía em y≈7,9 — fora do quadro, sem erro nenhum no console.
- A extensão e o início da câmera saem da CENA, não de constantes. Assumir que a câmera começa
  onde o jogador nasce (a Main Camera está em x=0, não em −14) deixava uma faixa cinza chapada
  na borda do quadro **só no objetivo**, a 50 unidades de onde o erro estava.

---

## 9. Verificação

```
Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod PlaceholderProbe.Run \
          -probeScene Assets/Scenes/Levels/Level_11_CilaCaribdis.unity
```

`-probeScene` é obrigatório: sem ele o probe abre a fase 01 e diz OK da fase errada.

| Medida | Valor |
|---|---|
| Sprites na cena | 57 |
| Camadas de parallax | 5 |
| Colisores sólidos | 12 |
| Extensão | 90 un (−35 a 55) |
| Placeholders pendentes | **0** |
| Gerações gastas | **15** (3 conceitos + 8 + 3 correções + 1 tileset) |
