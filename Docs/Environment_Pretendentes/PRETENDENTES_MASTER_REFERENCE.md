# PRETENDENTES_ENVIRONMENT_MASTER — Fase 15_Pretendentes

**Conceito C (luz e contraste) + A (exterior) + B (salão) — aprovado em 2026-09-12.**
Paleta: `Palette/PRETENDENTES_PALETTE.gpl` · **13 rampas: as 9 de Ítaca intactas + Bronze, Fogo, Vinho, Noite**

> A mesma casa das fases 01 e 14, tomada. Nada aqui é uma Ítaca nova — é a Ítaca de antes com
> o que os pretendentes trouxeram por cima.

---

## 1. Três conceitos, três trechos da fase

| | Cores | Faixas de valor | Famílias de matiz | Papel |
|---|---|---|---|---|
| A — Palácio ocupado | 69 | 7 de 10 | 4 (laranja 54%) | vocabulário do **exterior** |
| B — Banquete | 62 | 8 de 10 | **3 (laranja 63% + vermelho 25%)** | vocabulário do **salão** |
| **C — Barricadas** | 38 | 7 de 10 | **4, equilibradas (azul 25%)** | **regra de luz e paleta** |

A fase tem três ambientes em sequência — entrada, pátio, salão — e cada conceito corresponde a um
deles. Escolher um só seria errado. O que precisava ser escolhido era a **regra comum**, e ela saiu
da medição:

**O Conceito B mediu 88% dos pixels em famílias quentes.** Um salão iluminado só por fogo, com
pedra quente e vinho vermelho, colapsa numa massa única — é a falha de Lestrigões, em laranja, e
numa fase de combate isso esconde os inimigos. O Conceito C é o único com contraste frio/quente
(azul 25% contra laranja 25%). A regra que vale para a fase inteira: **sombra azul-noite, e só
fogo, vinho e comida quentes.**

---

## 2. Paleta: estender, não substituir

A fase 14 usa a paleta da fase 01 sem trocar um hex. A 15 precisa de identidade própria — luz
artificial, fogo, excesso — e as duas exigências só cabem juntas de um jeito: **todos os 36 hex de
Ítaca continuam aqui, lidos do arquivo da fase 01**, e a paleta ganha rampas que Ítaca nunca teve.

As rampas novas não são escolha de gosto — são literalmente o que os pretendentes trouxeram:

| Rampa | O que é |
|---|---|
| **Fogo** | a luz da fase. Ítaca não tinha rampa de fogo; a tocha da 01 e o braseiro da 14 ficaram fora da paleta por isso |
| **Vinho** | estandartes e vinho derramado. Ítaca não tem vermelho nenhum |
| **Noite** | a sombra azul que mantém o salão legível — a correção medida do Conceito B |
| **Bronze** | escudos, elmos, taças. Rampa compartilhada do personagem, que Ítaca não carregava |

**Achado durante a validação:** a paleta da fase 01 tem uma cor repetida — `#6b5d4e` é o passo
Profunda de `Terra / caminho` e o passo Base de `Pedra`. É anterior à regra de não repetir (que
nasceu em Lestrigões). **Não foi corrigida**: mudar a paleta da fase 01 quebraria a continuidade
das três fases de Ítaca. A checagem desta paleta cobra só as rampas novas.

---

## 3. A arte vem de três lugares, e a origem é a tese

O vestidor conta cada objeto pela origem:

| Origem | Qtd. | O quê |
|---|---|---|
| **Fase 01** | 15 | portão, colunas, **o campo de treino inteiro** (boneco, poste, alvo, suporte de armas), **o arsenal de Odisseu** (lança, escudo, feixe de flechas), mesa, barril, caixote, ânfora, montanhas |
| **Fase 14** | 6 | **o céu de fim de tarde, escurecido para noite**, os braseiros, o piso de laje |
| **Novos** | 23 | só o que os pretendentes trouxeram |

**Os pretendentes estão usando as coisas dele.** O suporte de armas onde Odisseu treinava no
prólogo agora guarda as lanças deles. Nenhum asset novo diz isso tão bem quanto o asset velho no
lugar errado.

**O céu é literalmente o da fase 14**, com a cor multiplicada por um azul de noite. A 14 termina ao
anoitecer; esta começa horas depois, e o céu que o jogador acabou de ver continua lá em cima. A
cobertura de céu é a cor medida da primeira linha do asset multiplicada pelo **mesmo** tom,
calculada no código para não haver emenda.

---

## 4. Luz natural vira luz artificial

| | Fase 14 | Fase 15 |
|---|---|---|
| Fonte | sol se pondo | fogueira, tochas, braseiros |
| Véu do último trecho | violeta-azulado, **escurece** | laranja, **esquenta** em direção ao trono |
| Leitura | "a noite está chegando" | "isto já não é a casa dele" |

Mesma ferramenta (degradê de alpha por coluna), sentido contrário. É a quarta fase seguida a usá-la
com uma cor diferente: azul frio em Gado do Sol, âmbar em Calipso, violeta na 14, fogo aqui.

---

## 5. Progressão e gameplay

| Trecho | x | O que tem |
|---|---|---|
| Entrada / exterior | −16 a 0 | muro com estandartes, fogueira de assar, barricada, ânforas, tochas, portão da fase 01 |
| Pátio | 0 a 20 | Penélope (1..3), checkpoint (6), **campo de treino da fase 01 ocupado**, Telêmaco (15..17) |
| Porta do salão | 20 | duas colunas da fase 01 marcando a entrada e mascarando a junta das faixas |
| Grande salão | 20 a 44 | mesas de banquete, vinho derramado, tochas, estandartes do javali, **trono no objetivo (42)** |

Os quatro pretendentes lutam em x=24, 28, 32 e 36. **Nenhuma decoração entra em ordem de desenho à
frente deles**, e as mesas ficam ENTRE as posições dos inimigos, nunca em cima. O checkpoint em x=6
fica livre de props.

O salão é interior: acima da parede há um bloco de teto em Noite Profunda. Sem ele, com a câmera
alta, o céu noturno aparecia por cima do salão e a fase inteira parecia ao ar livre.

---

## 6. Problemas encontrados e corrigidos

**As paredes saíram de azulejo azul-claro.** A pedra gerada veio cinza, caiu na rampa `Pedra fria`
— cujo passo claro é `#a0bcc4`, azul — e o palácio virou azulejo. Destino do cinza passou a ser
`Terra / caminho`: o calcário creme da fase 01.

**As colunas iluminadas viraram cor de chama.** Luz de tocha pinta a pedra de laranja saturado, e
com limiar baixo a coluna inteira ia para `Fogo`. `fogoMinL` 200 separa CHAMA de PEDRA ILUMINADA
POR CHAMA: a chama é a coisa mais clara do quadro, a pedra que ela ilumina não.

**As portas de madeira saíram vermelho-vinho.** Com vermelho a partir de S 0,35, madeira
marrom-avermelhada entrava em `Vinho`. Medido: pano tingido e vinho ficam em S 0,62–0,79, madeira
velha abaixo de 0,5. Limiar em 0,52.

**Xadrez pintado preso dentro do navio dos pretendentes.** O recorte por borda não alcança xadrez
cercado por cordame, e a segunda passada (ilhas pequenas) não o pega porque ele encosta nas cordas
e vira parte do componente do navio. Nova terceira passada, opt-in, em `Tools/checker-cut.js`: anda
só por pixels na cor exata do xadrez **e** cinza-neutros, e remove regiões de 24 px ou mais.

**Vazamento magenta na linha do chão** — o mesmo defeito corrigido na fase 14 nesta rodada. Até 118
px por quadro na primeira montagem; zero depois.

**O Editor do Unity estava aberto no projeto** no início da rodada, travando o batchmode. Não foi
encerrado (poderia haver cena não salva); a geometria da cena foi lida direto do YAML até ele ser
fechado. O leitor de YAML **não vê instâncias de prefab** — os quatro inimigos e o `LevelGoal` só
apareceram no `SceneLayoutProbe` real. Serve para adiantar trabalho, não para decidir layout.

---

## 7. Assets

| Grupo | Arquivos |
|---|---|
| Courtyard | `pret_courtyard_wall_band` (céu cortado, espelhada, quantizada), `pret_cooking_fire` |
| GreatHall | `pret_hall_wall_band` (espelhada, quantizada), `pret_throne` |
| Banquet | `pret_banquet_table`, `pret_spilled_feast`, `pret_amphora_group`, `pret_bench_toppled` |
| Invasion | `pret_barricade`, `pret_banner`, `pret_suitor_ship` |
| VFX | `pret_torch_stand`, `pret_firelight_gradient` (construído por código) |

**As faixas de arquitetura SÃO quantizadas**, ao contrário dos fundos de céu das outras fases: não
são céu, são o palácio, e a pedra delas tem de ser a da fase 01. O brilho das tochas vira degraus
em vez de degradê — e poça de luz em degraus é como um jogo 16-bit desenha fogo numa parede.

**O chão não ganhou tileset novo.** É o mesmo piso de laje da fase 01, já extraído para a 14.

`pret_banner`, `pret_barricade`, `pret_torch_stand` e `pret_suitor_ship` também são usados no fim
da fase 14, como anúncio da ocupação.

---

## 8. Verificação

```
Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod PlaceholderProbe.Run \
          -probeScene Assets/Scenes/Levels/Level_15_Pretendentes.unity
```

| Medida | Valor |
|---|---|
| Sprites na cena | 65 |
| Camadas de parallax | 3 |
| Objetos no cenário | 50 — 15 da fase 01, 6 da fase 14, 23 novos |
| Extensão | 60 un (−16 a 44) |
| Placeholders pendentes | **0** |
| Pixels magenta por quadro | **0** |
| Gerações gastas | **15** (3 conceitos + 12 assets) |
