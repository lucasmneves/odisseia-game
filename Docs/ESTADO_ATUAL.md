# Odisseia — estado atual e passagem de contexto

Atualizado em 2026-09-03. Este arquivo existe para retomar o trabalho numa sessão nova.

Documentos de referência, mais detalhados que este resumo:
- `Docs/CharacterMaster_Odysseus/ODYSSEUS_MASTER_REFERENCE.md` — personagem
- `Docs/Environment_Ithaca/ITHACA_ENVIRONMENT.md` — cenário

---

## 1. Personagem — concluído e integrado

**Conceito B (Aventureiro)** aprovado como `ODYSSEUS_MASTER_REFERENCE`.
`character_id` PixelLab: `908b7f60-0624-43bc-a0cd-89ad16416400`

**16 estados, 99 frames**, todos na direção `east` (espelhada por código):
Idle 7 · Run 9 · Jump 8 · Fall 2 · AttackLight 6 · AttackHeavy 8 · Shield 6 ·
ShieldHold 4 · Bow 8 · Damage 6 · Death 8 · Interaction 6 · Victory 8 ·
Crouch 4 · CrouchWalk 3 · Climb 6

Folha em `Assets/Resources/Odisseia/Characters/CHR_Odysseus.png` (756×1344).

### Números travados — não mudar sem recalcular tudo

| | |
|---|---|
| Corpo do Odisseu | 60 px = **1,4 unidades** |
| Pixels por unidade | **42,857143** |
| Canvas de animação | 84 × 84 px |
| Pivô | `alignment: 9`, {0,5 ; 0,142857} — linha dos pés |
| GUID da folha | `71ffcd5a5101b09622667079891272f6` |
| `internalID` do `Idle_00` | `607464041` |

**Ao regerar a folha:** preservar GUID e o `internalID` do `Idle_00` — é o `fileID` que o
`SpriteRenderer` do `Player.prefab` referencia. Trocar deixa o sprite *missing* no Inspector.

### Código de gameplay adicionado

- `PlayerController` — agachar (collider encolhe para baixo; `crouchHeightFactor 0.73` é a
  medida real do sprite agachado, 44px de 60). Levantar exige espaço livre acima.
- `PlayerClimb` (novo) — escalada de beirada por dois raycasts que precisam **discordar**:
  peito acha geometria, cabeça está livre. Usa `MovementSuspended`, não desliga o controller.
- `PlayerCombat` — golpe forte como segundo golpe encadeado (`comboWindow` 0,8 s), mais dano
  e alcance. `Attacked` manteve a assinatura; quem distingue lê `LastAttackWasHeavy`.
- `PlayerBow` — `releaseDelay` 0,15 s separa o disparo do lançamento da flecha, para
  sincronizar com a animação.
- `PlayerAnimator` — todos os 16 estados ligados.
- `OdysseusSheetProbe` (Editor) — valida tudo em batchmode.

### Verificação

```
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod OdysseusSheetProbe.Run -logFile Logs/x.log
```
Também `CampaignValidation.Run`, `PrologueProbe.Run` e `InputBindingsProbe.Check`
(atenção: `Check`, não `Run`). **O Editor precisa estar fechado.**

---

## 2. Cenário de Ítaca — completo e importado

**Conceito A (Rural)** aprovado. Venceu por afinidade de paleta medida contra o personagem:
75% dos pixels na família da paleta do Odisseu, contra 48% (B) e 50% (C).

### Paleta oficial — 35 cores em 9 rampas

`Docs/Environment_Ithaca/Palette/ITHACA_PALETTE.{hex,gpl,png}`

Contorno · Céu · Água · Folhagem · Oliva seca · Terra/caminho · Pedra · Pedra fria · Madeira

Origens: as seis primeiras do Conceito A; **Água do Conceito B** (o A quase não tem mar) e
**dessaturada 0,62** depois que a composição mostrou o oceano com 57% de saturação contra
13–39% do resto; **Pedra fria e Madeira** acrescentadas no Grupo 2, porque o Conceito A não
tem nem madeira nem pedra cinza e a quantização jogava madeira na Folhagem e capstone na Água.

`#050507` é compartilhado com a paleta do personagem.

### Grupos prontos

| Grupo | Estado | Onde |
|---|---|---|
| 1 Background | **completo** — 5 camadas de parallax | `Layers/` |
| 2 Terrain | **completo** — 4 tilesets Wang 32px | `Terrain/` |
| 4 Nature | 9 sprites | `Nature/` |
| 5 Props | 10 sprites | `Props/` |
| 6 Training | 4 sprites | `Training/` |
| 7 Arsenal | 4 sprites | `Arsenal/` |
| 8 Port | 4 sprites | `Port/` |
| 3 Architecture | **completo** — 2 casas, coluna e muro | `Architecture/` |
| 9 Ships | **completo** — navio em dois estados | `Ships/` |

A fonte vive em `Docs/Environment_Ithaca/`; `node Tools/unity-import.js` copia para
`Assets/Art/Environments/Ithaca/` com os `.meta` prontos.

### Escala derivada (42,857 px = 1 un)

Porta **acima de 1,4 un** · barril ≈ 0,5 · muro baixo ≈ 0,8 · casa ≈ 3 · oliveira 3–4 ·
navio 8–12. Tile de 32 px = 0,747 un, e o Odisseu tem 1,88 tiles de altura.

---

## 3. O que aprendi sobre as ferramentas — vale mais que os assets

Este é o conteúdo caro. Sem isso, uma sessão nova repete os mesmos erros.

### Escolha da ferramenta

O `pro` devolve um CONJUNTO de candidatos por chamada — 4 até 170 px e **1 só acima disso**,
com custo por chamada. Acima de 170 px ele deixa de valer: paga-se caro por um candidato
único, sem escolha. Asset grande vai de `pixen` (1 geração, até 768 px por lado) com correção
por código.

| Tipo de asset | Ferramenta | Custo |
|---|---|---|
| Banda plana e repetitiva (céu, oceano, silhueta distante) | **procedural, por código** | 0 |
| Forma orgânica com silhueta (nuvem, montanha, árvore, casa) | `create_image_pro` | 20–40 |
| Sprite solto com fundo transparente | `create_image_pixen` | 1 |
| Terreno que repete | `create_sidescroller_tileset` | 2–3 |
| Animação de personagem | `animate_character` v3, `directions:['east']` | 1 |

### Armadilhas confirmadas

- **`pixen` ignora `no_background` em qualquer prompt com enquadramento**, não só em cenas —
  o mesmo casco veio transparente sem instrução de composição e opaco com ela. Fundo chapado
  sai de graça com `Tools/cutout.js`.
- **Pedir "margem vazia" faz o modelo PINTAR o xadrez de transparência.** "Vazio" é uma
  palavra que ele desenha. Descrever o sujeito funciona: "desenhado pequeno o bastante para
  as duas pontas estarem completas e visíveis".
- **`pixen` ignora `view: side` em construções** — a casa veio isométrica. Prédio precisa de `pro`.
- **`style_image` vaza conteúdo, não só paleta.** Passar o Conceito A (uma vila rural) fez a
  "ilha distante" e o "oceano" voltarem como cenas de vila.
- **Exclusões no prompt não seguram.** "no trees", "no ground", "no buildings" são ignorados
  com frequência. Enquadramento **positivo** funciona melhor: "isolated game asset cut out on
  a fully transparent background, only the X" tirou o chão dos props.
- **Descrever objeto longe da mão desanexa o objeto.** "lâmina erguida atrás do ombro" fez a
  espada aparecer flutuando. A formulação que funciona: "preso à mão em todos os quadros,
  nunca se desprende".
- **O v3 ignora instrução de cor de item.** Pedi arco de madeira duas vezes e veio dourado —
  resolvido por remapeamento de rampa depois, não por geração.
- **Limite de 8 jobs simultâneos.** Erros de validação e jobs falhados não são cobrados.
- **`create_image_pro` acima de 170 px devolve 1 candidato só**, ao custo cheio de 20–40. O
  conjunto de candidatos é o que justifica o preço; num canvas grande esse valor some.
- **Canvas abaixo de 32px precisa ser quadrado** no `pixen`.
- **`outline` do tileset** aceita `single color outline`, sem o "black" que `create_character` usa.

### Duas receitas de animação de personagem

- **Movimento sutil** (idle, pose parada): `keep_first_frame: true` + trava pesada de
  orientação. A âncora impede o rosto de girar para frontal.
- **Movimento amplo** (salto, queda, ataque): `keep_first_frame: false` + trava leve. Com
  âncora o corpo fica preso na pose parada — o Jump subia 2px do chão.

### Tileset Wang

Cantos NW/NE/SW/SE como `upper` (ar) ou `lower` (sólido). **A posição na folha é o índice do
array no JSON**, em ordem row-major — o campo `original_position` aponta para outra grade e
leva ao tile errado. Nos tiles de topo os cantos de cima são `upper`, então **a superfície
pisável fica na metade do tile, não no topo** — o colisor precisa seguir isso.

### Densidade de pixels — o erro invisível

Escala em unidades **não** prova que o asset combina com o personagem. Arte devolvida
ampliada 2× mede o tamanho certo e tem metade da densidade — fica chapada ao lado do
Odisseu. Rodar `Tools/scale-probe.js` em todo asset novo. O sinal é o histograma de
linhas idênticas consecutivas; MDC não serve, uma sequência ímpar zera o resultado.

### Verificação de emenda em arte com dithering

Comparar a emenda ao ladrilhar contra uma **fronteira interna equivalente**, nunca colunas
vizinhas direto — em dithering ordenado vizinhos devem diferir, e o teste ingênuo dá falso
positivo. Errei isso duas vezes.

### Quantizar na paleta

Antes de quantizar, **conferir se a paleta cobre os materiais do asset**. Quantizar sem a
rampa certa não aproxima: joga a cor na rampa errada e destrói o objeto.

---

## 4. Arquitetura — resolvida sem gastar geração

A casa pequena tinha travado depois de **60 gerações**: v1 e v2 com o telhado plano certo mas
o Odisseu não passava pela porta; v3 com a escala certa mas telha inclinada, a leitura romana
que a seção 9 do briefing proíbe.

**Nenhum dos três caminhos propostos foi necessário.** Medir em vez de gerar mostrou que a v3
não é uma casa errada — é uma casa certa com o chapéu errado. O telhado ocupa as linhas 2–55;
parede, porta e janela, tudo que passou no teste, vivem abaixo da linha 58.

A casa final é **a v3 recortada na linha 58 com uma laje plana redesenhada por código**, na
estratigrafia medida da v2. `Tools/build-house.js` reconstrói o arquivo do zero.

| | Resultado | Alvo |
|---|---|---|
| Casa | 186 × 131 px = **4,34 × 3,06 un** | ~3,0 de altura |
| Vão da porta | 39 × 82 px = **1,91 un** | > 1,4 |
| Folga do Odisseu | 6 px lateral, 22 px de altura | passa |
| Densidade | nativa 1× | igual à do personagem |
| Cores | 13, todas da paleta | só da paleta |
| Custo | **zero gerações** | — |

Prova em `Environment_Ithaca/Architecture/_prova_escala.png`. Fonte preservada em
`_fonte_v3_telha_inclinada.png`; a receita da laje veio de `_v2_telhado_plano_escala_curta.png`.

### O Grupo 3 completo — as outras três peças também por composição

Resolvida a casa pequena, o resto saiu do mesmo material.

| Asset | Em unidades | Origem |
|---|---|---|
| `ithaca_house_small_01` | 4,34 × 3,06 | v3 recortada + laje plana |
| `ithaca_house_odysseus_01` | 7,33 × 4,36 | parede + porta + janelas da v3 + pórtico de 2 colunas |
| `ithaca_column_01` | 0,65 × 2,50 | dórica, desenhada por código |
| `ithaca_wall_low_01` | 1,35 × 0,79 | espelho da parede da v3, ladrilhável |
| `ithaca_palace_01` | 17,78 × 7,00 | fachada do megaron, **1 geração** |
| `ithaca_warehouse_01` | 17,24 × 3,31 | fachada do armazém, **1 geração** |
| `ithaca_gate_01` | 2,40 × 4,99 | folha do portão, desenhada por código |
| `ithaca_gatepost_01` | 0,65 × 5,16 | batente, desenhado por código |

As quatro passam em paleta, densidade e escala; as duas casas usam **exatamente as mesmas 13
cores**. Custo total do grupo: **zero gerações**. Prancha do conjunto em
`Environment_Ithaca/Architecture/_conjunto.png`.

```
node Tools/build-architecture.js
```

reconstrói o grupo inteiro e roda todas as verificações. Os PNGs em `Architecture/` são
derivados — podem ser apagados e refeitos.

### As duas fachadas largas — os únicos prédios que precisaram de geração

Palácio e armazém têm 17,8 un de largura contra 4,1 do maior material do banco: compor daria
uma casa esticada, e faltava silhueta própria. **1 geração cada** no `pixen` (o `pro` acima de
170 px dá um candidato só por 20–40). A altura do palácio bate exatamente com o bloco que
substitui; vão da porta 1,52 × 3,13 un, folga de 32 px lateral. A porta do armazém é
decorativa — ele é pano de fundo e nenhum portão entra nele.

- **O corte madeira/pedra é propriedade do asset.** Com o padrão de 0,36 metade da parede do
  palácio virou madeira e o calcário saiu mosqueado de malva. Medido: porta em 0,60–0,69 e
  calcário em 0,356–0,43, corte **0,50**; no armazém, porta em 0,50–0,53 e parede em
  0,16–0,26, corte **0,38**. Virou argumento do `build-facade.js`.
- **O recorte precisa de TODOS os tons de borda.** O xadrez que o `pixen` desenha tem dois
  tons, e no armazém eles ficavam fora da tolerância um do outro — o segundo sobrava como
  quadradinhos opacos que a quantização virava manchas claras.
- **Porta dupla é duas componentes.** O `scale-proof` media uma folha — 32 px contra 33 px de
  Odisseu — e reprovava por 1 px um vão em que ele cabe folgado. Agora junta folhas gêmeas,
  exigindo largura comparável para as **ombreiras** da casa não entrarem como se fossem vão.
- O `build-palace.js` tinha uma cópia dessa busca, e foi ela que reportou o falso 32 px. Os
  dois agora usam a mesma função.

### Três armadilhas do ladrilhamento por espelho

- **Período `2W-2`, não `2W`.** O espelho ingênuo repete a coluna da borda e deixa uma linha
  dupla visível. Descontando as duas colunas de dobra, a emenda vira reflexão exata: medida
  em 0,0 contra mediana interna de 8,8.
- **A faixa de origem tem de estar limpa de contorno**, não só de porta e janela. Peguei uma
  faixa cujas duas primeiras colunas tinham 20 px de sobra do contorno da verga; o espelho
  virou isso numa listra preta repetida por toda a parede. E a varredura de coluna limpa
  precisa rodar **na faixa útil** — o contorno de topo e base atravessa todas as colunas.
- **Escolher a fiada, não só a faixa.** Recortar 29 px quaisquer dá parapeito liso. Das cinco
  candidatas comparadas ampliadas lado a lado, uma só tinha a irregularidade de pedra seca.

---

### Duas lições que valem além desta casa

- **Verificar densidade de pixels, não só tamanho em unidades.** Arte ampliada 2× passa no
  teste de escala e mesmo assim destoa do personagem. `Tools/scale-probe.js` detecta, por
  histograma de linhas repetidas — MDC não serve, uma sequência ímpar zera o resultado.
- **Quando o modelo acerta o material e erra a composição, recorte o material e componha por
  código.** Só volte a gerar quando faltar material de verdade.

**A Fase 1 não tem mais placeholder pendente**, e agora isso é verificável com
`PlaceholderProbe.Run`, que varre a cena inteira incluindo objetos inativos.

Eu tinha declarado isso antes de ser verdade: faltavam 15 visíveis e 10 que só apareciam
durante os atos — inclusive os bonecos e alvos do treino, que **já tinham arte no pacote** e
eu nunca liguei. Capturas não bastam: uma mostra 18 das 349 unidades da fase, e objeto
inativo não aparece em nenhuma. Ver a seção 13 do ITHACA_ENVIRONMENT.

**Decisão em aberto:** os seis NPCs nomeados (Penélope, Telêmaco, Eumeu, Bardo, Alcínoo,
Nausícaa) são arte **pintada** — 6.000 a 8.500 cores, importados a 129 px por unidade contra
os 42,857 de todo o resto. O tamanho em unidades está certo, mas a densidade é 3× maior: no
tamanho de jogo eles borram, sem contorno e com o detalhe virando papa ao lado do Odisseu.
Refazê-los em pixel art custaria 6 gerações; é arte aprovada de personagens nomeados, então
não toquei.

Duas regras que valeram para a mobília de jogo:
- **O desenho do portão tem a mesma altura do colisor.** Portão baixo com colisor alto é
  parede invisível, que o README chama de indistinguível de bug.
- **O batente é asset separado da folha**, porque permanece quando o portão abre.
- **Coordenada fracionária vira lixo, não erro.** `W/2` numa largura ímpar não grava o pixel e
  deixa cor pela metade — saiu verde-limão no meio da madeira. Só o `palette-check` acusou.
  O `fillRect` agora arredonda na entrada.

---

## 5. Grupo 9 — o navio, 3 gerações e só o casco

O único asset do cenário que não tinha como sair de material existente. Ainda assim custou
**3 gerações**, não as 20–40 de uma chamada `pro`.

**Por que não `pro`:** ele devolve um conjunto de candidatos por chamada — 4 até 170 px e
**1 só acima disso**, com custo por chamada. Um navio de 343–514 px seria 20–40 gerações para
um único candidato, sem escolha: a mesma armadilha da casa. `pixen` custa 1 e vai até 768 px
por lado, então errar é barato.

| Asset | | |
|---|---|---|
| `ithaca_ship_01` | vela içada — a partida | 391 × 257 px = 9,12 × 6,00 un |
| `ithaca_ship_01_furled` | vela enrolada — atracado no cais | mesmo canvas, mesma linha de convés |

Só o casco foi gerado. Mastro, verga, vela, cordame e remos são código. A **linha do convés**
está publicada em `ithaca_ship_01.json`, porque o `ShipDeparture` apoia a silhueta do jogador
nela e o recorte final desloca as coordenadas.

### Duas armadilhas novas do `pixen`

- **Pedir "margem vazia" faz o modelo pintar o xadrez de transparência** — 100% do canvas
  opaco, em dois cinzas alternados. "Vazio" é uma palavra que ele desenha. A formulação que
  funciona descreve o sujeito, não o espaço: *"desenhado pequeno o bastante para as duas
  pontas estarem completas e visíveis"*.
- **`no_background` cai junto com qualquer linguagem de enquadramento.** A armadilha conhecida
  era "`pixen` ignora `no_background` em cenas"; é mais larga que isso — vale para **qualquer
  prompt que faça o modelo pensar em quadro**.

Nos dois casos o fundo sai chapado, e `Tools/cutout.js` resolve de graça. Flood-fill de borda,
não "apagar toda cor igual" — mas o vazado das espirais de proa e o interior das argolas de
remo precisam da segunda passada (`--enclosed`): são cercados pelo objeto, o flood-fill de
borda não os alcança, e deixá-los opacos põe 385 px da cor do fundo dentro da silhueta.

### Ordem de desenho

O mastro vem **antes** da vela (o pano corre à frente dele; por cima, o mastro parte a vela ao
meio); a verga vem **depois** (é nela que o pano é amarrado); os estais vêm antes de tudo.

A vela usa o **dithering Bayer 4×4** do céu. O expoente da curva importa: com `d²` quase todo
o pano cai na zona de transição e o dithering lê como chiado; com `d³` só a beirada degrada.

---

## 6. Import no Unity — feito

`node Tools/unity-import.js` copiou os 46 PNGs e 6 JSON para
`Assets/Art/Environments/Ithaca/`, **escrevendo os `.meta` junto**. Os 4 tilesets entraram
fatiados em 64 sprites de 32 px.

Escrever o `.meta` em vez de importar e corrigir depois é o que faz isso funcionar **com o
Editor aberto** — batchmode exige o Editor fechado, e ele estava com o projeto travado.

| | | |
|---|---|---|
| `spritePixelsToUnits` | 42,857143 | a 100 a casa mediria 1,3 un e o Odisseu não passaria pela porta |
| `filterMode` | Point | bilinear borra pixel art, e é o default do Editor |
| compressão | desligada | compressão inventa cor e quebra a paleta fechada |
| `alphaIsTransparency` | ligado | senão sai halo escuro na borda |
| pivô | BottomCenter (Center no terreno) | assenta na linha do chão como o personagem |
| wrap | Repeat só no parallax e no muro | são os que repetem lateralmente |

**GUIDs derivados do caminho**, então re-rodar o import não quebra referência de cena — a
mesma regra da folha do personagem.

### Verificar

```
node Tools/verify-meta.js
```

confere os 46 `.meta` fora do Unity (escala, filtro, compressão, pivô, wrap, GUID único e o
rect de cada um dos 64 tiles). **Passa com 0 falhas.** Serve com o Editor aberto.

```
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod IthacaEnvironmentProbe.Run
```

confere o que de fato importou, que é a única fonte de verdade — exige o Editor fechado.
Com o Editor aberto: `Odisseia > Conferir arte de Itaca`.

**Rodada em 2026-09-03, saiu limpa:** 46 texturas conferidas, 4 tilesets fatiados com os
índices row-major corretos, 0 erro de compilação, e o convés do navio a 1,56 un acima da
quilha. Log em `Logs/itaca.log`.

A probe existe porque erro de import não vira erro: a textura carrega, renderiza e roda, só
fica com o tamanho errado e borrada — indistinguível de "a arte é assim".

---

## 7. Quantização dos Grupos 4 a 8 — feita

`node Tools/quantize-scenery.js`: **32 assets** trazidos para a paleta, originais em
`<grupo>/_originais_sem_paleta/`. **42 dos 46 assets** agora usam só cores oficiais.

- **Classificador separado** (`materialDeCenario`), não o de arquitetura: alargar aquele para
  cobrir verde regrediria o vão da janela da casa, um teal escuro a 0,205 de saturação.
- **Ocre contra marrom** por `(g-b)/(r-b) >= 0,65`. Sem essa divisão o escudo e a ânfora saíram
  marrons chapados na primeira passada.
- **A tocha é pulada por medida**: a chama `#fb7507` está a **116** da paleta, o segundo pior a
  77, o resto em 63 ou menos. Não há rampa de fogo. O limiar de 90 é o vão entre os grupos.
- **As camadas de fundo ficaram de fora — depois de tentar e reverter.** Branco de nuvem cai
  em Pedra (marrom) e verde dessaturado de ilha cai em Oliva seca (amarela); nuvem marrom e
  ilha amarela na tela. Céu e oceano já estão na paleta por construção.

Fora da paleta ficam `ithaca_torch_01` (fogo) e as três camadas de fundo geradas.

---

## 8. A arte na cena — parallax, terreno e props

O `PrologueSceneBuilder` já usa a arte de Ítaca. Sem ela, volta sozinho às silhuetas chapadas.
Capturas em `Environment_Ithaca/_fase_*.png`; detalhes na seção 13 do `ITHACA_ENVIRONMENT.md`.

```
Unity.exe -batchmode -quit -projectPath . -executeMethod PrologueScreenshot.Capture           -shotXs 2,126,195,338 -shotOut Docs/Environment_Ithaca/_fase.png
```

**Sem `-nographics`** — batchmode sem contexto gráfico devolve imagem preta, e é o erro
natural porque todas as outras chamadas do projeto usam essa flag.

### O que aprendi montando

- **O parallax tem de ser simulado na captura.** O `ParallaxLayer` só roda em runtime. Sem
  simular, a foto no começo da fase sai certa e mais adiante o fundo some — e a conclusão
  natural, errada, é que a camada não cobre a fase. Perdi uma rodada nisso.
- **Esqueci de adicionar o próprio `ParallaxLayer`** nas camadas novas. As fotos distantes
  mostravam céu vazio e estavam certas: as camadas estavam paradas no mundo. Só apareceu
  quando instrumentei a captura para logar quantas camadas encontrava — deu **0**.
- **Ladrilhar destravou o parallax forte.** As telas antigas não eram contínuas nas bordas, o
  que obrigava a fatores de 0,93–0,97 — parallax quase nulo. As camadas de Ítaca ladrilham,
  então vão em `SpriteDrawMode.Tiled` e o fator desce até onde a profundidade aparece. A de
  montanha precisou de espelho `2W-2` para ladrilhar (emenda 120,7 -> 6,6).
- **`drawMode Tiled` exige malha FullRect** no import; com Tight o recorte é descartado e o
  sprite sai esticado em vez de repetido.
- **O terreno veste o colisor, não o substitui.** A geometria de colisão é conferida pelo
  `PrologueProbe`; trocá-la por Tilemap arriscaria a garantia de que a fase é terminável. Os
  blocos continuam como colisores com o desenho desligado.
- **O tile de topo é posicionado pelo CENTRO na linha do chão**, porque o desenho sólido dele
  começa na metade do tile (cantos de cima `upper` no Wang).
- **Placeholder que funcionava contra fundo chapado grita contra paisagem pintada.** A cerca
  do treino virou um traço solto no ar, e as paredes-limite viraram lajes marrons na frente
  do mar. As duas foram resolvidas sem tocar em colisor.

`PrologueProbe.Run` continua passando: chão contínuo de -8,0 a 341,0, **39 figuras apoiadas
com desvio máximo de 0,00**, portões e alcance íntegros.

---

## 9. Orçamento e ambiente

Plano PixelLab **Tier 1: Pixel Apprentice** — 2000 gerações por ciclo, renova em **2026-10-02**.
Gasto até aqui: **352** de 2000.

O MCP `pixellab` está registrado em `~/.claude.json` sob o escopo de projeto `C:/Users/luucas`,
não sob `C:/Users/luucas/Documents/Claude` — por isso as ferramentas **não carregam** numa
sessão aberta na pasta do projeto. O contorno está pronto e testado: `Tools/pixellab.js` fala
JSON-RPC com o endpoint HTTP, e `Tools/pixellab-image.js` dispara um job de imagem e espera.
Solução permanente: registrar o servidor também no escopo desta pasta.

Ao extrair `job_id` da resposta, casar o UUID inteiro. Um fallback do tipo `/job_idD+(w+)/`
come os dígitos iniciais (`D` é não-dígito, e um UUID pode começar com letras) e devolve um
id truncado — com o job já disparado e cobrado.

Não há Python nesta máquina; scripts auxiliares vão de Node.

Artifacts publicados:
- Personagem: https://claude.ai/code/artifact/a54d574a-2017-4bd9-a0eb-8477a0b6d22d
- Cenário: https://claude.ai/code/artifact/7a762a9f-8474-45d8-aa13-625701ecb700

## Fases 09 e 10 — Mundo dos Mortos e Sereias (2026-09-06)

Integradas e verificadas. `PlaceholderProbe` sai 0 nas duas; a bateria completa
(`OdysseusSheetProbe`, `LocomotionProbe.Medir`, `JumpReachProbe.Medir`,
`InputBindingsProbe.Check`, `CampaignValidation`) sai 0; zero erro de compilação.

- Masters: `Docs/Environment_MundoDosMortos/MORTOS_MASTER_REFERENCE.md` e
  `Docs/Environment_Sereias/SEREIAS_MASTER_REFERENCE.md`.
- Vestidores: `MundoDosMortosSceneDresser` (55 objetos) e `SereiasSceneDresser` (47).
- Gerações PixelLab: 16 em Mortos, 18 em Sereias.

**Pendência anterior a estas fases:** `PlaceholderProbe` falha em Troia (fase 02) com nove
placeholders acesos — seis são plataformas de gameplay nunca vestidas (`Platform_Bridge`,
`Gauntlet_1..3`, `Wall_Troia`, `Obstacle_Low`) e `TroyScenery/Sky_Fill` só precisa de
justificativa escrita. As fases 11 a 16 ainda não foram vestidas.

## Fases 11 e 12 — Cila e Caribdis e Gado do Sol (2026-09-09)

Integradas e verificadas. `PlaceholderProbe` sai 0 nas duas, `CampaignValidation` sai 0, zero
erro de compilação.

- Masters: `Docs/Environment_CilaCaribdis/CILA_MASTER_REFERENCE.md` e
  `Docs/Environment_GadoDoSol/GADO_MASTER_REFERENCE.md`.
- Vestidores: `CilaCaribdisSceneDresser` (29 objetos) e `GadoDoSolSceneDresser` (49).
- Gerações PixelLab: **40 nas duas somadas** (528 → 568 de 2000).

### O que estas duas fases acrescentam ao que já se sabia

**O xadrez de transparência PINTADO reapareceu quatro vezes**, e o motivo de ele passar
despercebido virou ferramenta: *o visualizador de PNG mostra alpha real como xadrez cinza,
exatamente igual a um xadrez pintado.* Olhar não distingue; só contar pixels opacos distingue.
`Tools/checker-cut.js` faz as duas coisas — `temXadrezPintado` cobra e `removerXadrez` corta.

O `cutout.js` genérico **não** serve nesse caso: os cinzas do xadrez medem o mesmo valor das
faces iluminadas do basalto, e o flood-fill por tolerância removeu 50,7% da parede, devolvendo
um esqueleto. Quando cor não separa fundo de objeto, o que separa é ESTRUTURA (o xadrez ocupa a
primeira linha inteira) ou GEOMETRIA (o redemoinho é uma elipse).

**Rampa estreita demais não escurece um asset: ela o APAGA.** A parede colunar quantizada na
rampa `Basalto` (4 passos, faixa de 0,18) virou uma mancha preta sem colunas. A medida que
resolveu foi ver que ela já era mais escura que o chão (L 0,278 contra 0,432) — a quantização
não corrigia nada. Escurecer por FATOR preserva todo o contraste interno.

**Exclusão no prompt não segura substantivo de arquitetura.** "not built by anyone, no bricks,
no masonry" corrigiu Sereias e falhou três vezes aqui, porque *"boca de caverna", "arco" e
"prateleira de rocha" já são substantivos de arquitetura*. A saída foi a da casa de Ítaca:
parar de gerar e construir — o pináculo é uma fatia da parede e a falésia da caverna é a mesma
parede escavada, as duas a custo zero.

**Quatro de cinco faixas de parallax não ladrilhavam**, e nenhuma tinha dado sinal. Medir com
`seam-test.js` antes de montar é barato; `mirrorDouble` resolve as quatro.

**Pivô na base desenha PARA CIMA.** Três objetos da fase 12 foram postos fora do quadro sem um
único erro no console: o véu da tempestade em y=4 ocupava de 4 a 44; as ondas de Cila em
baseY −3,2 flutuavam no ar entre as colunas. Toda faixa posicionada por topo precisa descer a
altura inteira.

**Um agrupamento vale dez componentes.** A fase 11 saiu com 13 `ParallaxLayer` contra 5 a 8 das
outras onze fases. Penduradas em dois contêineres com um componente cada, caíram para 5 — o
mesmo resultado visual, metade das escritas de transform por quadro.

### Pendências

- **Notion não foi atualizado.** O servidor MCP dele usa OAuth (`https://mcp.notion.com/mcp`,
  sem token estático em `~/.claude.json`) e devolve `401 invalid_token` ao ser chamado por
  `Tools/notion.js`. O contorno que funciona para o PixelLab não funciona aqui. Toda a
  documentação desta rodada está nos dois masters acima, prontos para publicação quando o
  servidor for autenticado no escopo desta pasta.
- **Nada foi testado em play mode, mobile ou navegador** — vale para estas duas fases e para as
  nove anteriores.
- `LocalizationProbe.Check` falha com `código pede chave que não existe: speaker.` — é falso
  positivo pré-existente: o probe lê o literal da concatenação `"speaker." + falante` em
  `PrologueProbe.cs:245`. Sem relação com as fases 11 e 12, que não usam chaves de falante.

## Fases 13 e 14 — Calipso e Ítaca Return (2026-09-12)

Integradas e verificadas. `PlaceholderProbe` sai 0 nas duas, `CampaignValidation` sai 0, zero
erro de compilação.

- Masters: `Docs/Environment_Calipso/CALIPSO_MASTER_REFERENCE.md` e
  `Docs/Environment_ItacaReturn/ITACARETURN_MASTER_REFERENCE.md`.
- Vestidores: `CalipsoSceneDresser` (42 objetos) e `ItacaReturnSceneDresser` (62, dos quais
  **40 são arte reusada da fase 01**).
- Gerações PixelLab: **28 nas duas somadas** (568 → 596 de 2000).

### O que estas duas fases acrescentam

**Ítaca Return não tem paleta própria — usa a da fase 01, sem trocar um hex.** É a primeira fase
da campanha assim, e a decisão é o requisito: cor é o que o olho reconhece antes da forma, e uma
paleta nova diria "outro lugar" antes de qualquer prédio entrar em quadro. A mudança vem da LUZ
(fim de tarde em vez de meio-dia), do ESTADO (casa com hera, cerca quebrada, barco apodrecido) e
do que PASSOU A EXISTIR (feira, braseiros). Os prédios reusados ficam intactos de propósito: o
contraste entre o que resistiu e o que não resistiu é o que conta o tempo.

**Negar um conceito no prompt deixa o modelo escolher o substituto; nomear a alternativa não.**
"not built by anyone, no bricks, no masonry" falhou de novo na falésia de Calipso. O que
funcionou foi descrever positivamente a forma natural e insistir na propriedade que a alvenaria
não tem — CURVA: *"worn into soft rounded scallops, every edge CURVED and bulging, no flat
surface anywhere"*. É a mesma razão do sucesso de "basalto colunar hexagonal" na fase 11.

**Camada de parallax não tem borda fixa em x.** Ela se desloca em relação ao mundo a cada
quadro, então encurtá-la só muda onde a borda passeia. Faixa que precisa parar num ponto do
mundo tem de ser `Faixa` presa ao mundo, não `Camada`.

**Espelhar cria um eixo de simetria que o olho pega.** Na faixa de vila de Ítaca Return o eixo
produziu um prédio simétrico em forma de borboleta ao lado da porta de casa. A posição do eixo é
calculável a partir do início da faixa e do período; mascarar com um prop é mais barato que
regerar.

**Folga do chão tem de cobrir o alcance da CÂMERA, não o fim do chão.** Um buraco magenta no
canto do último quadro veio de folga de 8 com câmera que enxerga 11 unidades além do chão.

**Terceira fase seguida em que um ramo do classificador resolveu o material errado**: cachoeira
cinza-pedra, tronco de mármore branco, areia estourada. A regra que sobrou: **o destino dos
ramos ambíguos é declarado por asset, nunca afinado no limiar** — casca, terra, pano e calcário
ocupam a mesma região de matiz e luminância, e nenhum limiar os separa.

### Pendências

- **Notion continua inacessível**, verificado nesta rodada: `401 invalid_token`. O servidor MCP
  dele é OAuth e não tem token estático em `~/.claude.json`. `Tools/notion.js` está pronto e
  volta a funcionar quando o servidor for autenticado neste escopo.
- **Nada testado em play mode, mobile ou navegador** — vale para as catorze fases vestidas.
- As fases 15 e 16 seguem sem vestir.

## Fase 15 — Pretendentes, e revisão da 14 (2026-09-12, segunda rodada)

Fase 15 integrada e verificada; fase 14 revisada contra o segundo briefing. `PlaceholderProbe` sai 0
nas duas, `CampaignValidation` sai 0, zero erro de compilação, **zero pixel magenta** nas capturas.

- Master: `Docs/Environment_Pretendentes/PRETENDENTES_MASTER_REFERENCE.md`; a revisão da 14 está na
  seção 9 de `ITACARETURN_MASTER_REFERENCE.md`.
- Vestidor novo: `PretendentesSceneDresser` (50 objetos — 15 da fase 01, 6 da fase 14, 23 novos).
- Gerações PixelLab: **15** (601 → 616 de 2000).

### O que esta rodada acrescenta

**Paleta que estende em vez de substituir.** A 15 tem as 9 rampas de Ítaca lidas do arquivo da fase
01, intactas, mais Bronze, Fogo, Vinho e Noite — literalmente o que os pretendentes trouxeram. É o
jeito de ter identidade própria sem deixar de ser a mesma casa.

**A paleta da fase 01 tem uma cor repetida** (`#6b5d4e` em `Terra / caminho` e `Pedra`). É anterior à
regra de não repetir e ficou registrada, não corrigida: mudar a paleta da 01 quebraria a
continuidade das três fases de Ítaca.

**Os tilesets de Ítaca vazam o fundo da câmera na linha do chão.** A fileira de superfície deles tem
falhas, e com o bloco de terra terminando em −3 nada cobria a faixa atrás. Medido: até 118 px
magenta por quadro, todos em y=−2,0. **Passou despercebido na fase 14 da rodada anterior** —
encontrado agora contando pixels, não olhando. Calipso não tinha o defeito (tiles com superfície
fechada), e a comparação entre as duas apontou a causa. Qualquer fase futura que use os tiles de
Ítaca precisa do bloco de terra subindo até −1,9.

**Contar pixels de uma cor numa captura acha defeito que o olho não acha.** Uma linha pontilhada
de 1 px na borda do chão some numa miniatura de 960×540. Um script de 10 linhas contando
`#ff00ff` pegou nas duas fases de uma vez.

**Xadrez pintado dentro de objeto vazado** (navio com cordame) escapa das duas passadas do
`checker-cut.js`. Terceira passada opt-in (`cercado`) anda só por pixel cinza-neutro na cor exata
do xadrez.

**Editor do Unity aberto trava o batchmode**, e aconteceu no início desta rodada. Ler a cena direto
do YAML adianta trabalho mas **não enxerga instâncias de prefab** — inimigos e `LevelGoal` só
aparecem no probe real.

### Pendências

- **Notion continua inacessível** (OAuth; `401`). Documentação nos masters e aqui.
- **Nada testado em play mode, WebGL, mobile ou gamepad** — vale para as quinze fases vestidas.
- Fogo e tochas são sprites **estáticos**: não há componente de cintilação no projeto, e o briefing
  proíbe criar sistema novo para isso.
- Fase 16 segue sem vestir.

## Personagens da Fase 01 — Character Art Master (2026-09-29)

Referência nova de personagens: **`Docs/Characters/CHARACTER_ART_MASTER.md`** (escala, paleta por
facção, NPC design system, animação de NPC, processo PixelLab). Notion: página "Character Art Master —
Fase 01" em 🎨 Art, "PixelLab — Personagens" em 🤖 AI e decisões D-011 a D-015.

- **Odisseu mantido** (master de 16 estados). Os seis NPCs pintados saíram da fase 01: 30 figuras
  trocadas por 11 personagens v3 + 3 variações de cor, todos com Idle (`SpriteAnimator`, pingue-pongue).
- Folhas em `Assets/Resources/Odisseia/Characters/NPCs/CHR_<Nome>.png`; fontes e ids em `Docs/Characters/`.
- Penélope estava a 1,7 un (mais alta que o Odisseu); agora 1,35. Telêmaco virou criança (0,98 un).
  O instrutor usava a arte do Alcínoo. Agamenon não aparece na fase — o arauto de Micenas o representa.
- PixelLab: **38 gerações** (616 → 654 de 2000).
- Verificado: `PrologueSceneBuilder.Build`, `PrologueProbe` (39 figuras apoiadas, desvio 0,00),
  `PlaceholderProbe`, `OdysseusSheetProbe`, `CampaignValidation`, capturas em
  `Docs/Characters/Fase01/_capturas/`.

### Pendências
- A arte pintada `CHR_NPC_*` continua nas fases 14 e 15 e nos Feácios. O Telêmaco de lá é adulto:
  precisa de personagem próprio, não do da fase 01.
- Figurantes antigos `Environments/Ithaca/Figures/ithaca_villager_*` ficaram sem uso (não apagados).
- Nada testado em play mode, navegador, mobile ou gamepad — o Idle dos NPCs só roda em runtime.

## Personagens da Fase 02 — Troia (2026-09-29)

Extensão do Character Art Master: **`Docs/Characters/Fase02/TROY_CAST.md`**. Notion: "Character Art —
Fase 02 — Troia" em 🎨 Art, seção nova em "PixelLab — Personagens", decisões D-016 a D-019.

- Auditoria por `CastProbe.Run -probeScene <cena>` (novo, serve a qualquer fase): Troia tem só o
  Odisseu e 3 `EnemyBasic`. Nenhum NPC, nenhum narrativo em cena.
- **Trojan Soldier Master** (Idle, Run, Hit, Death) substitui o inimigo **só em Troia**, por override
  nas instâncias (`TroyCastDresser.Dress`). O prefab `EnemyBasic` ficou intacto.
- **Greek Soldier Master** (Micenas): 3 figuras de fundo no acampamento.
- PixelLab: **9 gerações** (654 → 663 de 2000).
- `CampaignValidation`, `OdysseusSheetProbe` e `PrologueProbe` passam. `PlaceholderProbe` em Troia
  segue falhando pelos 9 placeholders de CENÁRIO já conhecidos.

### Pendências
- As outras 6 fases com `EnemyBasic` (01, 03, 08, 09, 14, 15) ainda usam o inimigo pintado carmesim.
- Nada testado em play mode, navegador, mobile ou gamepad.

## Personagens da Fase 03 — Cicones (2026-09-29)

Extensão: **`Docs/Characters/Fase03/CICONES_CAST.md`**. Notion: "Character Art — Fase 03 — Cicones",
seção nova em "PixelLab — Personagens", decisões D-020 e D-021.

- CastProbe (agora conta Animators, overrides por inimigo e scripts da cena): Odisseu + 3 `EnemyBasic`
  idênticos ao prefab; 0 Animator; nenhum NPC, grego ou narrativo.
- **Cicones Warrior Master** (Idle, Run 3–7, Hit, Death) por override na cena (`CiconesCastDresser.Dress`).
- Override virou helper comum: `EnemyFactionOverride.Aplicar` (Troia refatorada e reverificada).
- PixelLab: **6 gerações** (663 → 669 de 2000).
- `PlaceholderProbe` Cicones limpo; `CampaignValidation` passa.
- Inimigo carmesim ainda nas fases 01, 08, 09, 14 e 15.

## Personagens da Fase 04 — Citera (2026-09-29)

Extensão: **`Docs/Characters/Fase04/CYTHERA_CAST.md`**. Notion: "Character Art — Fase 04 — Citera",
seção em "PixelLab — Personagens", decisões D-022 e D-023. **0 gerações** (669 de 2000).

- CastProbe: só o Odisseu. A fase fala com a tripulação e um "Companheiro" responde, mas o navio estava vazio.
- `CyteraCastDresser.Dress`: marinheiro, remador ocre e Elpenor oliva da Fase 01 nos conveses
  (pés no topo do colisor, guarda de 1,5 un do nascimento). `Remove` tira o elenco.
- `PlaceholderProbe` Citera limpo; `CampaignValidation` passa.
- **Pendência de cenário:** linha magenta de 1 px na altura do mar em todas as capturas; não é dos
  personagens (mesma contagem sem o elenco); as capturas de 06/09 tinham 0 → regressão posterior.

## Personagens da Fase 05 — Ciclopes (2026-09-29)

Extensão: **`Docs/Characters/Fase05/CYCLOPES_CAST.md`** — contém o POLYPHEMUS CHARACTER MASTER v1.
Notion: "Character Art — Fase 05 — Ciclopes", seção em "PixelLab — Personagens", decisões D-024 a D-026.

- CastProbe (agora com bloco de chefe): Odisseu + Polifemo placeholder (3 quadrados flutuando 3 un acima
  do chão). Chefe estacionário, sem colisor, 999 de vida; eventos `AttackTelegraphed`/`AttackExecuted` sem ouvinte.
- Polifemo 4,6 un, de pé, olhando para a esquerda; Idle + Telegraph + Attack (os dois últimos de UMA
  geração "Slam" dividida). `BossAnimator` (novo, runtime) liga os eventos; `BossController` intacto.
- **O v3 não faz um olho só** (2 tentativas): olho único pintado por `Tools/cyclops-eye.js` em cada quadro.
- `CyclopsCastDresser.Dress` / `.Poses`. Justificativas do Polifemo removidas do `PlaceholderProbe`.
- PixelLab: **28 gerações** (669 → 697 de 2000), 1 master descartado.

## Personagens da Fase 06 — Éolo (2026-09-29)

Extensão: **`Docs/Characters/Fase06/AEOLUS_CAST.md`** — AEOLUS CHARACTER MASTER v1 e AEOLUS WIND BAG MASTER.
Notion: "Character Art — Fase 06 — Éolo", seção em "PixelLab — Personagens", decisões D-027 e D-028.

- CastProbe: só o Odisseu; o Éolo não estava na cena, mas o palácio foi montado com o vão central
  reservado para ele. Entra como figura (Idle), no alto dos degraus, diante da porta (`AeolusCastDresser`).
- Odre dos ventos refeito em densidade nativa (0,61 un); `EoloSceneDresser` agora usa escala 1.
- PixelLab: **8 gerações** (697 → 705 de 2000), 3 descartes.
- `PlaceholderProbe` Éolo limpo; `CampaignValidation` passa.
- Pendência de cenário: peças do palácio encolhidas a 0,52 (mesma classe de defeito de densidade do odre antigo).

## Personagens da Fase 07 — Lestrigões (2026-09-29)

Extensão: **`Docs/Characters/Fase07/LESTRIGONS_CAST.md`** — LESTRIGON WARRIOR MASTER v1.
Notion: "Character Art — Fase 07 — Lestrigões", seção em "PixelLab — Personagens", decisões D-029 a D-031.

- CastProbe (agora com bloco de perseguidor): 2 arremessadores `Giant` placeholder + `PursuerHazard`
  **invisível** (o `LestrigoesSceneDresser` desliga o desenho dele). Sem inimigos carmesim.
- Um master para os três: arremessadores Idle/Telegraph/Attack (`BossAnimator`), perseguidor Run (filho
  `Art_Lestrigon`, que anula a escala 3×4 herdada do Transform do colisor).
- `BossArtDresser` (novo) é o helper comum de chefe; `CyclopsCastDresser` refatorado e reverificado.
- PixelLab: **36 gerações** (705 → 741 de 2000), 1 descarte.
- **Pendência de cenário registrada, não tocada:** peças do palácio de Éolo (fase 06) encolhidas a 0,52.

## Personagens da Fase 08 — Circe (2026-09-29)

Extensão: **`Docs/Characters/Fase08/CIRCE_CAST.md`** — CIRCE CHARACTER MASTER v1 e lobo encantado.
Notion: "Character Art — Fase 08 — Circe", seção em "PixelLab — Personagens", decisões D-032 a D-034.

- CastProbe: Circe ausente (templo com vão reservado); 2 `EnemyBasic` carmesim; transformação é só tinta.
- Circe (v3, perfil `south-east`) diante do templo, Idle com vapor na taça; lobos encantados (`pro` com
  estilo do Odisseu — o v3 recusa quadrúpede) por override (`CirceCastDresser`).
- PixelLab: **30 gerações** (741 → 771 de 2000), 1 descarte (lobo `standard`).
- `EnemyBasic` carmesim ainda nas fases 01, 09, 14 e 15.

## Personagens da Fase 09 — Mundo dos Mortos (2026-09-29)

Extensão: **`Docs/Characters/Fase09/MUNDO_DOS_MORTOS_CAST.md`** — Shade Warrior, Spirit NPC (`Shade`), Anticleia.
Notion: "Character Art — Fase 09 — Mundo dos Mortos", seção em "PixelLab — Personagens", decisões D-035 a D-037.

- Inimigos carmesim -> sombras de guerreiros (override); 3 sombras + Anticleia nos altares reservados
  (`UnderworldCastDresser`). Tirésias não está em cena. Transparência pela cor do renderer.
- O alpha 0,55 antigo deixava a sombra cinza invisível: agora ciano pálido 0,85 nesta cena.
- PixelLab: **12 gerações** (771 → 783 de 2000), sem descarte.
- `EnemyBasic` carmesim ainda nas fases 01, 14 e 15.

## Personagens da Fase 10 — Sereias (2026-09-29)

Extensão: **`Docs/Characters/Fase10/SIRENS_CAST.md`** — SIREN CHARACTER MASTER v1.
Notion: "Character Art — Fase 10 — Sereias", seção em "PixelLab — Personagens", decisão D-038.

- CastProbe: nenhuma sereia; só a mecânica do canto (SirenZone, SirenResistance, MastAnchor). Sem inimigos.
- Sereia mulher-ave (asas turquesa), bonita; Idle que canta; 3 figuras (santuário + ao pé das agulhas)
  com 2 variações por matiz (`SirenCastDresser`).
- PixelLab: **3 gerações** (783 → 786 de 2000), sem descarte.

## Personagens da Fase 11 — Cila e Caríbdis (2026-09-30)

Extensão: **`Docs/Characters/Fase11/CILA_CARIBDIS_CAST.md`** — CILA CHARACTER MASTER v1.
Notion: "Character Art — Fase 11 — Cila e Caríbdis", seção em "PixelLab — Personagens", decisões D-039 e D-040.

- Cila = BOSS (`BossController` "Giant"): torso de mulher e seis serpentes; Idle/Telegraph/Attack; base y = 0,55
  medida contra a câmera de jogo (tamanho 6, +1) — `ScyllaCastDresser`.
- Caríbdis = ENVIRONMENT (`FX_Charybdis`), intocada; escala 0,42 fica como pendência de cenário.
- PixelLab: **20 gerações** (786 → 806 de 2000), sem descarte.
- Pendências: Cila com pés visíveis ("pairando"); palácio de Éolo 0,52; porco da Circe; `EnemyBasic` carmesim em 01, 14, 15.

## Sprint PixelLab antes da expiração (2026-10-01)

Plano em **`Docs/PixelLabSprint/PIXELLAB_SPRINT_PLAN.md`**, 14 lotes prontos em `Docs/PixelLabSprint/lotes/` (93 gerações
de imagem + ~8 de animação), executor `Tools/sprint-run.js` (`--dry`, `--ping`, `--only P0`, `--max N`; pula o que já
foi baixado). Prioridades: Fase 16 (única sem cenário), VFX de gameplay (hoje quadrados coloridos), Troia N-08, HUD,
mapa, chamas, primeiro plano em 04/06/14/15.

**Nenhuma geração feita**: a sessão na nuvem não alcança `api.pixellab.ai` (política de rede) e não tem a credencial.
`Tools/pixellab.js` agora aceita `PIXELLAB_API_KEY` (e `PIXELLAB_MCP_URL`) além do `~/.claude.json` local.
