# CICLOPES_ENVIRONMENT_MASTER — Fase 05_Ciclopes

**Conceito B — Costa rochosa com caverna — aprovado em 2026-09-06.**
Paleta: `Palette/CICLOPES_PALETTE.gpl` · 13 rampas, 52 cores

> Ilha selvagem e desabitada. A fase inteira é a passagem do sol para o interior da caverna, e
> essa passagem se conta em **valor**, não em objeto.

---

## 1. Identidade aprovada

| | Resultado |
|---|---|
| A — Ilha selvagem | Recusado como direção: saiu em **três-quartos**, com a superfície do chão à vista. Serve como fonte de vocabulário de vegetação e rocha. |
| **B — Costa com caverna** | **Aprovado.** Perfil reto, falésia de calcário estratificado com boca de caverna, praia de seixos, água turquesa. Traz a transição da fase inteira numa imagem. |
| C — Caverna do gigante | Recusado: virou uma **cripta de alvenaria com arcos romanos**. Serve como referência dos objetos de escala, nunca da arquitetura. |

### A regra que o conceito C ensinou

Pedir "câmara com colunas de pedra" ou "entrada de caverna" devolve **arquitetura construída**:
o prior do modelo para essas palavras é portal de blocos esquadrados e coluna estriada — que é
exatamente a arquitetura que o briefing proíbe. Aconteceu três vezes: no conceito C, no
`cave_mouth` e no `cliff_column`.

Caverna precisa ser descrita como **rocha bruta**:
*"raw uncut natural rock with irregular bulges and deep cracks, no bricks and no cut blocks"*.

E há um segundo limite, aprendido aqui: **pintar rocha por código sai pior ainda.** Ruído por
soma de senos vira camuflagem, não pedra. O altar de Cícones e o deck de Cytera foram por
código porque são **geométricos** — degraus, postes, tábuas. Rocha é orgânica, e orgânico é
onde a geração ganha. O que resolveu o pilar foi **compor**: silhueta por código sobre a
textura de parede gerada.

---

## 2. Três rampas de pedra, e por quê

| Rampa | Faixa de luminância | Papel |
|---|---|---|
| `Calcário` | 0,94 – 0,67 | pedra ao sol |
| `Pedra cinza` | 0,75 – 0,23 | falésia a meia luz |
| `Rocha profunda` | 0,36 – 0,10 | interior da caverna |

A escolha de qual usar é **por asset, não por classificador**: o mesmo cinza que é falésia
iluminada lá fora é parede de caverna lá dentro, e nenhum limiar de cor separa os dois — quem
separa é o lugar onde o asset vai ser usado, que só o plano de build sabe.

**Calcário sozinho não serve para pedregulho.** A faixa de 0,94 a 0,67 é curta demais para uma
pedra, que precisa escurecer na base onde toca o chão; mandados para lá, os dois pedregulhos
viraram manchas brancas chapadas. O classificador resolve porque divide o cinza entre Calcário
e Pedra cinza no corte de luminância, dando oito passos de faixa em vez de quatro.

Compartilhadas de propósito: **Contorno** e **Bronze** (personagem), **Céu** (Ítaca),
**Fogo** (Troia).

---

## 3. A transição exterior → caverna

Acontece em três coisas ao mesmo tempo, todas governadas pela posição em x. A fronteira sai da
**geometria** (`Floor_Narrow`), nunca de uma coordenada escrita à mão:

1. o terreno troca de terra selvagem para rocha;
2. o teto de estalactites entra;
3. **três véus** de opacidade crescente (0,30 · 0,52 · 0,72) escurecem progressivamente.

Véus, e não um corte: um corte seco na boca lê como parede invisível.

`Cave_Backdrop` é um bloco de rocha atrás de toda a região da caverna, subindo bem acima do
que a câmera alcança — sem ele o céu de meio-dia continuava aparecendo acima da faixa de
parede, dentro de uma caverna.

As **tochas** são a única fonte de luz do interior e o que mantém o chão legível enquanto o véu
escurece. É a resposta ao parágrafo de iluminação do briefing sem nenhuma luz dinâmica.

---

## 4. A câmara de Polifemo

Tudo ali existe para transmitir **escala**: jarros, troncos, cerca e clava de um tamanho que só
faz sentido para quem é grande. A fogueira fica no meio do trecho, que é onde o combate
acontece, e o resto do espaço central fica livre.

**O chefe já existe na cena** como placeholder de personagem (`Polyphemus`, corpo de
2,2 × 3,2 un em x=30). Este trabalho não o toca: o briefing proíbe gerar personagens. Ele está
registrado no `PlaceholderProbe` com essa justificativa, para o probe não mascarar a pendência.

---

## 5. Assets

**16 gerações.** Catorze assets gerados, mais o pilar natural composto e o fundo por código
(cinco camadas, custo zero). Nenhum asset reusado de outra fase: Ciclopes é a primeira ilha
selvagem do jogo e não repete material com Ítaca, Troia ou Cícones.

| Grupo | Assets |
|---|---|
| Gameplay | tileset de terra selvagem |
| Cave | chão, parede, estalactites, pilar natural |
| Props | pedregulho grande, agrupamento de pedras, ossos, tocha |
| Midground | oliveira torta pelo vento |
| Special | jarros, troncos, fogueira, cerca e clava — todos gigantes |
| Background | céu, montanhas, mar, colinas, falésia — **por código** |

---

## 6. Terreno sem Tilemap

`SpriteRenderer` em modo `Tiled`, como as fases 01 a 04. Decisão registrada em
`Docs/Environment_Cicones/CICONES_MASTER_REFERENCE.md` §8.

**Plataforma larga e baixa é laje, não pedregulho.** Vestida de pedregulho ela estoura:
`EscalarPara` usa o maior dos dois fatores para cobrir o colisor, e num colisor de 4,0 × 0,5
isso multiplica a altura por 0,89 — um pedregulho de 3,3 un flutuando 0,7 un acima do chão, que
foi o que a captura mostrou. Colisor com 1,5 un ou mais de largura recebe faixa de terreno.
