# CYTERA_ENVIRONMENT_MASTER — Fase 04_Cytera

**Conceito A — Tempestade moderada — aprovado em 2026-09-05.**
Ferramenta: PixelLab (`create_image_pixen`) + código · Paleta: `Palette/CYTERA_PALETTE.gpl`

> Sobrevivência no mar. É a fase mais distante das outras em cor — dessaturada, fria, sem um
> único verde de vegetação — e a que mais depende de camadas em vez de sprites.

---

## 1. Identidade aprovada, e por que a mais dramática perdeu

| | Resultado |
|---|---|
| **A — Tempestade moderada** | **Aprovado.** Vagalhões cinza-esverdeados com crista de espuma, céu de nuvem em camadas, ilhota no horizonte. Escuro o bastante para ser tempestade e claro o bastante para o jogador ver onde pisa. |
| B — Épica | Recusado. Excelente drama, **água quase preta**. |
| C — Extremamente perigosa | Recusado. O mais bonito dos três e o mais injogável: quase todo o quadro fica abaixo de 15% de luminância. |

O §17 exige plataformas identificáveis, obstáculos visíveis e personagem sempre legível. B e C
falham nos três. **A intensidade da tempestade vem de CAMADAS empilhadas — chuva, relâmpago,
nuvem, espuma — e não de escurecer a base.** É a mesma exigência do §11 (“não criar um único
sprite contendo toda a tempestade”) chegando pelo outro lado: o que torna a fase modular é
também o que a torna jogável.

---

## 2. Paleta oficial

**10 rampas, 40 cores**, geradas por `Tools/extract-palette-cytera.js`, que recusa gravar se
alguma rampa não descer em luminância ou se uma cor aparecer em duas rampas.

| Rampa | Highlight | Base | Sombra | Profunda | |
|---|---|---|---|---|---|
| Contorno | `#2b2e27` | `#1d221d` | `#141415` | `#050507` | *compartilhada — personagem* |
| Bronze | `#f5cd5c` | `#bb7e2c` | `#9d611f` | `#733e16` | *compartilhada — personagem* |
| Espuma | `#fcfdfc` | `#ebf0e9` | `#cedad6` | `#afb7af` | |
| Céu tempestade | `#9ca39a` | `#7e857f` | `#5a615d` | `#383d35` | |
| Nuvem escura | `#666e69` | `#495147` | `#353a32` | `#1e211c` | |
| Mar | `#95ada2` | `#7b9288` | `#5a6d69` | `#3c4239` | |
| Mar profundo | `#72847b` | `#56756d` | `#40463d` | `#1c1f1a` | |
| Rocha molhada | `#8a948d` | `#6f7873` | `#555d53` | `#2e322b` | |
| Madeira | `#594732` | `#483121` | `#39261b` | `#241f18` | |
| Lona | `#dfe0d3` | `#8a7855` | `#74654c` | `#463526` | |

**O relâmpago não tem rampa própria de propósito** — é a mesma família de branco da Espuma, e
uma rampa quase idêntica para o mesmo material é exatamente o erro que a paleta de Cícones
evitou ao compartilhar Fogo e Fumaça com Troia.

### O que separa Cytera das outras fases

Cícones tem **duas rampas de vegetação** e um mar turquesa raso; Cytera não tem **nenhum verde
de planta** e o mar é cinza-esverdeado frio. A ponte entre as três fases são Contorno e Bronze,
vindas do personagem: é o mínimo que mantém tudo no mesmo jogo enquanto a paleta muda de ponta
a ponta.

---

## 3. Escala e enquadramento

| | |
|---|---|
| Densidade | **42,857143 px por unidade** — herdada, não negociável |
| Câmera | ortográfica 5 → 10 un de altura, ~17,8 de largura |
| Fase | **90 un × 11 un** (medido por `LevelExtentProbe`) |
| Linha d'água | **y = −3,0**, logo abaixo das plataformas em y = −2 |

A linha d'água não é decisão estética: com o mar terminando em −4,5, sobrava uma faixa de céu
entre a água e as plataformas que lia como vazio cinza. As três alturas do mar saem dela.

---

## 4. O mar em três faixas (§11)

| Camada | Fator | Cobre | Papel |
|---|---|---|---|
| `Sea_Deep` | 0,35 | −8,3 a −6,0 | massa escura, sem detalhe |
| `Sea_Surface` | 0,18 | −6,0 a −3,0 | cristas e brilho; ladrilha nos DOIS eixos |
| `Sea_Foam` | 0,10 | linha d'água | espuma da arrebentação |

Todas ladrilham. **Deslocar `Sea_Surface` em X no Unity já anima o mar** — não há folha de
sprites de oceano, e é isso que mantém os draw calls no orçamento do §16.

## 5. Parallax completo (§9)

`Sky_Fill` (−60) · `Sea_Fill` (−58) · `BG_Sky` 1,00 (−50) · nuvens avulsas (−46) ·
mar em três faixas (−22/−20/−18) · `Rain_Back` 0,55 (−2) · plataformas e efeitos (−7 a +9) ·
`Rain_Front` 0,15 (+14).

**A âncora de camada é `x0 = meioDaCâmera·(1−f) + inícioDaCâmera·f`.** Ancorar na borda
esquerda da fase — como o `TroySceneDresser` faz — só funciona quando a câmera começa lá. Em
Cícones isso deixou a camada de fator 1,00 a 38 unidades da câmera para sempre, e o céu
simplesmente não aparecia.

**Nuvens são objetos avulsos, não camada que ladrilha.** Ladrilhada, a mesma forma repetia na
mesma altura a cada 9 unidades e a repetição lia mais que a nuvem. Oito cópias com altura,
escala e espelhamento variados cobrem a fase sem se denunciar.

---

## 6. O que é código e o que é geração

| Por código (custo zero) | Gerado (1 geração cada) |
|---|---|
| céu de tempestade, superfície do mar, água profunda, linha de espuma, chuva, relâmpago, três anéis do redemoinho, borrifo, tábuas do deck | onda grande, onda média, nuvem, rocha alta, recife largo, mastro com vela, destroços |

A divisão não é de conveniência: **o que ladrilha ou é geométrico vai por código**, porque só
assim a emenda é exata e a cor sai da rampa. O orgânico vai por geração.

Duas armadilhas confirmadas de novo aqui:
- A onda grande e o mastro voltaram com **fundo opaco** apesar de `no_background: true` —
  resolvido por `Tools/cutout.js`, que é de graça.
- O “deck do navio” gerado **voltou como um barril**: descrição com estrutura (“seção de
  convés, com o casco curvando abaixo”) é lida como objeto curvo. Deck é faixa que repete, e
  faixa que repete é sempre código.

### Chuva: inclinação 1:1, não “bonita”

A risca de chuva anda 1 px em X para cada 1 em Y num ladrilho quadrado — assim ela entra em
`x` e sai em `x + altura`, que é exatamente um ladrilho, e a chuva emenda nos dois eixos.
Qualquer outra inclinação cria uma costura diagonal visível a cada repetição.

A densidade foi cortada de 170 riscas a alpha 190 para **62 a alpha 120/70**: são DUAS camadas
de parallax, a densidade vista soma, e a primeira versão apagou o personagem.

---

## 7. Iluminação (§12)

Não há luz direcional dentro do sprite, como em todo o resto do projeto — sombreamento de
forma. O que muda em Cytera é o **valor**: a faixa útil desce para 0,13–0,66 de luminância,
contra 0,19–0,92 em Cícones. O branco só aparece na espuma e no relâmpago, e é justamente por
ser raro que ele lê como violência da água.

Relâmpago é **núcleo branco dentro de um halo mais escuro**. Sem o halo lê como risco de giz.

---

## 8. Integração

Assets em `Assets/Art/Environments/Cytera/{Background,Ocean,Rocks,Ship,Effects,Weather}`.
Montagem por `CyteraSceneDresser.Run`, idempotente sob a raiz `CyteraScenery`.

**O vestidor não cria geometria.** A cena já vinha com `Deck_Popa`, `Destroco_1..4`,
`Onda_1..3`, `Rocha_1..4`, os três anéis do `Redemoinho`, `Raio_1..3` e `Deck_Proa`. Ele
desliga o *desenho* de placeholder de cada um e põe a arte na medida do colisor — inclusive a
escolha entre rocha alta e recife largo, que sai da largura medida do colisor e não de uma
lista escrita à mão que sairia de sincronia com o level design.

---

## 9. Custo

| | |
|---|---|
| Conceitos | 3 gerações |
| Assets orgânicos | 8 gerações |
| Camadas e efeitos por código | 0 |
| **Total da fase** | **11 gerações** |
