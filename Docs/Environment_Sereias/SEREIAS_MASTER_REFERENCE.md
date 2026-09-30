# SEREIAS_ENVIRONMENT_MASTER — Fase 10_Sereias

**Conceito A — Estreito ensolarado — aprovado em 2026-09-06.**
Paleta: `Palette/SEREIAS_PALETTE.gpl` · 9 rampas, 36 cores

> A única fase do jogo em que **ser bonita é requisito de gameplay**, e não acabamento.

---

## 1. Identidade aprovada

| | Resultado |
|---|---|
| **A — Estreito** | **Aprovado como direção.** Mar turquesa parado, ilhéus de calcário creme, luz dourada — e um casco naufragado como a única nota errada. É a tese do briefing desenhada. |
| **B — Praia** | **Aprovado como vocabulário:** areia, linha de espuma, pedras roladas, barcos encalhados, colunas de verga reta, ossos. |
| C — Rochedos | **Recusado como direção.** Saiu cinza e frio — o mesmo erro que já está documentado como a fraqueza de Lestrigões, e o oposto de "belo e convidativo". O vocabulário de destroços foi aproveitado. |

O percurso segue a geometria que o level design já tinha: praia de partida (−18 a −4) com o
`MastAnchor` em x=−5, a ilha das sereias (−4 a 14) e a praia de chegada (14 a 34), com o navio
da fuga no `LevelGoal`.

---

## 2. O perigo não pode avisar

Se o cenário anunciar a ameaça — céu carregado, cor de alerta, monstro à vista — a fase deixa
de funcionar, porque a armadilha das sereias É a beleza. Então:

- o céu é creme claro com raios de sol, nunca cinza;
- a paleta não tem nenhuma cor de alerta, e as nove rampas são todas quentes ou turquesa;
- o perigo aparece só em **objetos pequenos e baixos**: um naufrágio por praia, e nada mais.

O santuário no meio da ilha é a mentira central: colunas e verga reta, promessa de que há algo
bom ali.

---

## 3. Nove rampas, seis famílias de matiz

`Areia e calcário` · `Água rasa` · `Água funda` · `Espuma` · `Vegetação` · `Madeira naufrágio`,
mais `Contorno`, `Bronze` (personagem) e `Céu` (Ítaca) compartilhadas.

São **menos** rampas que Lestrigões (onze) e mais legíveis, porque a lição de lá não era "use
menos" e sim **"separe por família de matiz"**. Aqui creme, marrom, turquesa claro, teal fundo,
branco-esverdeado e verde-oliva se separam sozinhos — e é por isso que esta fase **não usa o
tint de perspectiva aérea** que Lestrigões cobrou e que Circe e Mortos herdaram.

**Não existe rampa de "pedra molhada"**, e a ausência é decidida: o inventário tem cinzas de
baixa saturação no risco d'água, mas eles mediriam L 0,37 a 0,24 — uma faixa estreita de cinza,
exatamente a forma de rampa que achatou Lestrigões. Pedra molhada sai do passo escuro de
`Areia e calcário`; é o mesmo material sob água, não um material novo.

---

## 4. O que a geração errou, e como foi corrigido

Quatro dos quinze assets saíram errados, e as correções seguiram a regra do projeto — defeito
local vira recorte, direção errada vira nova geração:

| Asset | Defeito | Correção |
|---|---|---|
| `shrine` | veio com **frontão triangular** apesar de "verga reta, sem telhado triangular" | **recorte medido**: a silhueta cresce até 188 px e estabiliza em 168; corta-se onde ela para de crescer |
| `mid_cliff` | veio como **alvenaria de blocos**, não geologia | regerado com "not built by anyone, no bricks, no masonry"; saiu como lajes sobre fundo preto → o preto virou alpha e ele passou a ser a faixa de meio de campo |
| `tiles_rock` | mesma alvenaria | mesmo prompt corrigido |
| `bg_islets` | veio como uma **parede cinza texturizada** | regerado com ênfase em distância e haze |

`water_surface` foi **descartado inteiro**: veio com riscos **verticais** e lia como cachoeira,
não como mar parado. A faixa é construída por `build-sereias-water.js` com riscos horizontais —
a mesma técnica que tirou o rio de Mundo dos Mortos da leitura de "grama".

---

## 5. As três armadilhas de composição

Todas invisíveis no asset e óbvias no quadro montado. Vale registrar porque nenhuma delas é
sobre arte:

1. **Duas camadas de fundo com céu próprio viram barras.** `bg_sea` e `bg_islets` empilhados
   produziam três faixas horizontais com emendas duras atravessando a tela. Só uma camada pode
   carregar céu: o do `bg_islets` foi recortado para alpha, e ele passou a se sobrepor.
2. **`Camada` desenha do pivô PARA CIMA.** As lajes em `baseY = −1,4` subiam até y=4,57 e
   enchiam o alto do quadro; o mar, que é o assunto da fase, virava uma fresta. Rebaixadas para
   −4,2, só o topo delas aparece acima da linha do chão.
3. **A margem escura do tile não é chapada.** A linha 0 do tile de areia mistura `#816f63` e
   `#ae957f`, e a diferença (111) passa longe da tolerância de 24 do `cortarCeu` — a margem
   ficava e virava **uma barra escura de 1,07 un sob os pés do jogador**. O que separa margem de
   superfície é a luminância da linha (0,45 contra 0,85), não a uniformidade dela.

E uma regra que ficou no vestidor: **a altura do chão vem do sprite, nunca de uma constante.**
O pipeline recorta esses tiles em três lugares diferentes, e todo recorte muda a altura sem
avisar.

---

## 6. Reuso

O mastro, a vela e o convés vêm de **Cytera**. Não é economia: é literalmente o mesmo navio da
fase 04, e desenhar um segundo criaria duas embarcações para uma viagem só. O mesmo mastro
aparece duas vezes na fase — em x=−5, onde Odisseu se amarra, e no objetivo, onde ele reembarca.

---

## 7. Assets

**18 gerações** (3 conceitos + 8 + 7 + 3 correções), 17 texturas no Unity. Mar, canto e véu são
código, a custo zero de geração.

`VFX/sereias_haze.png` existe e **não é usado**: entrava como faixa quente de alpha 0,55 e
produzia uma terceira banda cinza-esverdeada entre o céu e a rocha — a emenda que a fase estava
tentando não ter. As seis famílias de matiz já separam os planos, e o véu contradizia isso.

**Não é gerado:** as sereias. O briefing proíbe gerar personagens, e o vão do santuário e o
espaço sobre as agulhas de rocha ficam livres para elas entrarem depois.

---

## 8. Honestidade sobre o resultado

- **O canto está no limite do perceptível.** Os arcos turquesa aparecem contra o creme, mas
  fracos. É deliberado — o briefing proíbe que o efeito esconda jogador, inimigo ou plataforma —
  e ainda assim é o ponto mais frágil da fase. Se for reforçado, o caminho é aumentar o
  **contraste de matiz**, não o alpha.
- **A fase é muito clara e muito creme.** Areia e calcário dominam a área do quadro, e as outras
  cinco famílias de matiz aparecem em superfície pequena. Funciona como costa mediterrânea, mas
  a variedade que a paleta promete não se realiza inteira na tela.
- **A ponta direita em x=38** ainda mostra uma quina do barranco se a câmera chegar ao extremo.
