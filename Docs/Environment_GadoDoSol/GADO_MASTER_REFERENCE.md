# GADO_ENVIRONMENT_MASTER — Fase 12_GadoDoSol

**Conceito C — Ilha sagrada — aprovado em 2026-09-09.**
Paleta: `Palette/GADO_PALETTE.gpl` · 10 rampas, 40 cores

> A fase da MENTIRA DA BELEZA. Ela precisa parecer segura para que a decisão de comer o gado
> pese — e é o contrário exato de Cila e Caribdis em toda medida.

---

## 1. Identidade aprovada

| | Cores | Faixas de valor | Famílias de matiz | Veredito |
|---|---|---|---|---|
| A — Ilha | 40 | 9 de 10 | 5 (43% azul) | Fonte do **verde de pasto**, do **mar** e do **gado** |
| B — Vale | 66 | 8 de 10 | 5 | Fonte da **arquitetura** — o templo mais bem desenhado |
| **C — Sagrado** | **88** | **10 de 10** | **6** | **Direção aprovada** |

O Conceito C mede melhor em tudo — é o único que ocupa todas as dez faixas de valor — e é a
progressão da fase desenhada: prado em primeiro plano, colinas com olival no meio, templo na
crista ao fundo. O que ele não tem é verde em quantidade (12%), e é por isso que a rampa
`Pasto` vem do Conceito A.

---

## 2. O contraste com a fase 11 é a decisão de paleta

| | Cila e Caribdis (11) | Gado do Sol (12) |
|---|---|---|
| Matiz dominante | basalto, teal, violeta | céu azul, pasto verde, calcário dourado |
| Luminância | quase tudo abaixo de 0,25 | quase tudo acima de 0,40 |
| Famílias quentes | uma (relâmpago) | três (ouro, calcário, terra) |
| Rampas compartilhadas | **Contorno, Bronze** | **Contorno, Bronze** |

As duas fases só continuam parecendo o mesmo jogo por causa das duas rampas compartilhadas,
que vêm do personagem e atravessam a campanha inteira.

**Não existe rampa de "ouro solar".** O sol, os detalhes dourados do gado e o fogo do altar
usam `Bronze`. Criar uma segunda rampa dourada poria dois amarelos quase iguais na paleta, e a
regra medida em Lestrigões é que cor repetida entre rampas não é detalhe: é o classificador
deixando de saber a que material o pixel pertence.

**O gado tem rampa própria** — `Gado sagrado`, de branco quase puro ao ouro num degradê só. É
isso que os marca como sagrados sem precisar de brilho, partícula ou shader.

---

## 3. O classificador, e os dois erros que ele cometeu

Esta é a primeira fase em muito tempo que pode cortar por MATIZ: seis famílias distintas se
separam sozinhas. Mas dois ramos erraram, e os dois pelo mesmo motivo de fundo.

**Os menires voltaram AZUIS.** A pedra deles é cinza-esverdeada com líquen: matiz ~185,
saturação logo acima de 0,14 — não cinza o bastante para `Muro seco` e bem dentro da faixa do
céu. Saíram pintados de azul-celeste, lendo como cascatas de água no meio do pasto.
**Correção:** o ramo `Ceu` virou opt-in. Nenhum sprite solto desta fase contém céu — céu só
existe nas camadas de fundo, que não são quantizadas. Um ramo que nunca deveria ser alcançado
não pode ficar ligado por padrão.

**Casca de oliveira, terra do tileset e muro seco saíram todos DOURADOS.** O ramo quente-claro
mandava tudo para `Calcario dourado`, na ideia de que "quente e claro = pedra de templo".
**Correção:** `quenteClaro` passou a ser declarado por asset. Casca, terra e mármore ocupam a
mesma região de matiz, saturação e luminância — nenhum limiar os separa. Quem sabe a diferença
é quem chamou.

---

## 4. A escala do rebanho é medida, não estimada

O boi gerado mede **3,22 × 2,19 un** e o Odisseu tem **1,4** de altura. Um boi real tem ~1,5 m
na cernelha contra 1,75 m de um homem: **o boi deve ser mais BAIXO que o personagem.** O fator
0,55 leva-o a 1,20 un; o bezerro vai a 0,42.

É a armadilha já registrada no projeto — escala de prop gerado é sempre grande demais — e aqui
custaria a leitura: um boi maior que o herói deixa de ser gado e vira monstro.

---

## 5. A transformação gradual

| Trecho | x | O que muda |
|---|---|---|
| Praia e costa | −18 a −2 | céu aberto, mar calmo, nada construído |
| Campos e pastagens | −2 a 18 | olival, muros secos, o rebanho |
| Templo e área sagrada | 18 a 40 | calcário dourado, altar, menires — e o céu fechando |

O escurecimento é **um sprite de 256×8 px com o alpha variando por coluna**, esticado sobre o
último trecho. A primeira montagem usou quatro retângulos de alpha uniforme se sobrepondo:
cada fronteira virou um degrau visível e o fim da fase ficou com quatro painéis translúcidos de
borda reta atravessando a tela — leitura de interface, não de clima. **Alpha uniforme não vira
degradê por empilhamento; o degradê tem de estar na imagem.**

A rampa sobe até 70% da largura e depois fica no teto (0,60), para o véu poder passar do fim da
fase sem que a borda direita dele apareça dentro do quadro.

A troca de material do chão — grama para laje de calcário no `Floor_3` — é o aviso silencioso
de que se entrou em terreno sagrado. O jogador percebe que pisa em pedra lavrada antes de ver o
templo.

---

## 6. Assets

| Grupo | Arquivos |
|---|---|
| Background | `gado_bg_hills.png` (não quantizado, **espelhado**) |
| Ocean | `gado_sea_calm.png` (clareado ×1,55, **espelhado**) |
| Midground | `gado_grove_band.png` (ponta cortada, **espelhado**) |
| Gameplay | `gado_tiles_grass`, `gado_tiles_stone` + as quatro peças de chão |
| Fields | `gado_olive_tree`, `gado_cypress`, `gado_shade_tree_big`, `gado_drystone_wall` |
| Pasture | `gado_shrubs_flowers` |
| SacredCattle | `gado_cattle_idle`, `gado_cattle_grazing`, `gado_calf` |
| Temple | `gado_temple`, `gado_ruined_column` |
| SacredArea | `gado_altar`, `gado_standing_stones` |
| VFX | `gado_storm_gradient.png` (construído por código) |

**Reutilizados de Cytera (04):** `cytera_deck_planks`, `cytera_mast_sail`,
`cytera_storm_cloud`.

O **gado é sprite estático em três variações** (parado, pastando, bezerro), e não animação. O
briefing pede "não criar um sistema complexo de animais caso não exista" — e não existe: o
`SacredCattleZone` é um gatilho de interação, não um NPC com locomoção.

---

## 7. Quatro erros de POSIÇÃO, todos do mesmo tipo

Pivô na base desenha **para cima**. Três objetos foram postos onde ninguém podia vê-los, sem um
único erro no console:

- **O véu da tempestade** em y=4 ocupava de 4 a 44 — inteiramente acima do que a câmera mostra.
- **As nuvens** em y=6,5 ficavam com só a franja de baixo visível.
- **O olival**, em tamanho nativo e assente em y=−3, cobria o quadro inteiro e transformava a
  ilha aberta numa floresta fechada. A primeira correção — encolher o transform para 0,52 —
  resolveu a altura e QUEBROU a largura, porque encolher o transform encolhe as duas dimensões:
  surgiu uma borda vertical dura onde o olival acabava. O que a faixa precisava era estar mais
  BAIXA, não menor.

E um erro de ORDEM, que só aparecia com a câmera baixa: `Ground_Fill` em −58 ficava atrás de
todas as camadas de fundo, e o olival rebaixado reaparecia **debaixo do chão**, com árvores
crescendo dentro da terra. Aquele bloco não é fundo — é a massa de terra, e massa de terra tapa
o que está atrás dela.

O mar tinha o problema simétrico: em −44 ficava atrás do olival e a praia não existia na tela.
Na costa o mar está NA FRENTE do interior da ilha.

---

## 8. Verificação

```
Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod PlaceholderProbe.Run \
          -probeScene Assets/Scenes/Levels/Level_12_GadoDoSol.unity
```

| Medida | Valor |
|---|---|
| Sprites na cena | 65 |
| Camadas de parallax | 6 |
| Colisores sólidos | 5 |
| Extensão | 90 un (−35 a 55) |
| Placeholders pendentes | **0** |
| Gerações gastas | **25** (3 conceitos + 8 + 7 + 2 tilesets, contando o custo por kit) |
