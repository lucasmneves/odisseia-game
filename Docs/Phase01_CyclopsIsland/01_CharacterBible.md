# Character Bible — Ilha dos Ciclopes

> Deriva de `00_ArtBible.md`. Proporções e escalas vêm de §4 daquele documento.

---

## 1. ODISSEU — protagonista

### Conceito

Um comandante veterano de dez anos de guerra que quer voltar para casa. **Não** é um
berserker nem um semideus: é astuto, cansado e determinado. A postura deve comunicar
"experiente" e não "furioso" — ombros firmes, guarda baixa e confiante no idle, olhar à
frente.

### Especificação física

| Atributo | Valor |
|---|---|
| Altura no jogo | **1,4 unidades** = 140 px @ PPU 100 |
| Largura (colisão) | 0,6 un = 60 px |
| Proporção | **6,5 cabeças** (cabeça ≈ 21,5 px) |
| Constituição | Atlético e funcional — musculatura de soldado, não de fisiculturista |
| Idade aparente | 38–45 anos |

### Design

| Elemento | Descrição |
|---|---|
| **Cabelo** | Escuro (castanho-preto), médio, ondulado, preso para trás — desgrenhado pela viagem |
| **Barba** | Curta e aparada, mesma cor do cabelo, com fios grisalhos nas têmporas (comunica idade sem enrugar o rosto) |
| **Pele** | Bronzeada de sol e mar, tom quente médio |
| **Túnica** | Exomíde curta (deixa um ombro livre), lã crua/terracota, até meia-coxa |
| **Armadura** | **Leve**: peitoral de couro fervido sobre a túnica + um ombro (pauldron) de bronze no lado esquerdo. Nunca armadura completa — pesaria a silhueta e contradiria a agilidade |
| **Cinto** | Couro largo com fivela de bronze |
| **Braçadeiras** | Bronze nos antebraços — pontos de brilho que ajudam a ler o movimento dos braços |
| **Pernas** | Nuas até o joelho, sandálias com amarração até a panturrilha |
| **Espada** | Xifos grego — lâmina folha de bronze, ~0,5 un (50 px), punho de couro escuro |
| **Escudo** | **Opcional, fora do slice.** Ver nota abaixo |
| **Manto** | Curto, cor vinho, preso no ombro direito — dá movimento à silhueta em corrida e pulo |

**Nota sobre o escudo:** o briefing o marca como opcional. **Decisão: fora do Vertical
Slice 01.** Motivo: o `PlayerCombat` implementado não tem mecânica de bloqueio; um escudo
visível prometeria uma ação que o jogo não entrega, e ainda dobraria o trabalho de
animação (todo frame precisaria da mão de escudo). Entra quando/se houver bloqueio.

### Silhueta — o teste

Preenchida de preto, Odisseu tem que ser reconhecível por:
1. O **manto** voando atrás (assimetria única no jogo)
2. O **ombro de bronze** quebrando a linha do ombro esquerdo
3. A **espada** estendida

Nenhum outro personagem do slice pode ter manto. É a assinatura dele.

### Paleta — Odisseu

| Função | Hex | Uso |
|---|---|---|
| Pele (base) | `#C88A5A` | Braços, pernas, rosto |
| Pele (sombra) | `#8F5A38` | Sombra própria |
| Pele (luz) | `#E3AC7C` | Toque superior-esquerdo |
| Túnica (base) | `#C4643C` | Terracota — cor de identidade |
| Túnica (sombra) | `#8A4028` | |
| Couro (base) | `#7A4B2E` | Peitoral, cinto, sandálias |
| Couro (sombra) | `#4E2E1B` | |
| Bronze (base) | `#D9A441` | Ombro, braçadeiras, lâmina |
| Bronze (brilho) | `#F5D98A` | Realce especular |
| Bronze (sombra) | `#8F6420` | |
| Manto | `#8C2F39` | Vinho — segunda cor de identidade |
| Cabelo/barba | `#3A2A22` | |
| **Contorno** | `#2A1A14` | Marrom muito escuro — **nunca preto puro** |

**Cores de identidade: terracota `#C4643C` + bronze `#D9A441` + vinho `#8C2F39`.**
Nenhum inimigo, prop ou tile pode usar essa combinação — é reservada ao protagonista.

---

## 2. CICLOPE — boss da fase

### Conceito

Um pastor gigante, não um demônio. Ameaçador pela **escala e pela força bruta**, não por
crueldade estilizada. Tem posses simples (peles, um cajado, cestos) — vive ali. Isso o
torna mais crível e menos genérico do que um monstro puro.

### Especificação física

| Atributo | Valor |
|---|---|
| Altura no jogo | **4,2 unidades** = 420 px @ PPU 100 |
| Largura (ombros) | ~2,2 un = 220 px |
| Proporção vs Odisseu | **3,0×** |
| Ocupação de tela | ~35% da altura visível |
| Proporção corporal | 4 cabeças — cabeça enorme, membros grossos e curtos (deixa "pesado" em vez de "esguio") |

### Design

| Elemento | Descrição |
|---|---|
| **Olho** | **Um só**, grande, âmbar, centralizado. Pupila vertical. É o ponto focal absoluto — o elemento mais claro e saturado do personagem |
| **Cabeça** | Desproporcionalmente grande, mandíbula pesada, testa baixa e saliente |
| **Pele** | Cinza-oliva rochoso, textura áspera com manchas mais escuras |
| **Torso** | Massivo, peito largo, barriga presente (força de trabalho, não definição de atleta) |
| **Braços** | Muito longos e grossos — alcançam abaixo do joelho. Deixam claro o alcance de ataque |
| **Roupa** | Tanga simples de pele de cabra, costurada tosca. Uma correia de couro no peito |
| **Adornos** | Bracelete de pedra bruta amarrado no antebraço direito; ossos/pedras trançados no cabelo |
| **Cabelo/barba** | Emaranhado, escuro, com fios acinzentados |
| **Arma** | **Cajado de pastor**: tronco de oliveira, ~3,5 un, com uma pedra amarrada na ponta |
| **Pés** | Descalços, enormes, unhas grossas |

### Elementos de identificação do boss (leitura obrigatória)

O jogador precisa saber, sem texto, que aquilo é o boss:

1. **O olho âmbar brilhante** — único elemento âmbar saturado da fase inteira
2. **Escala** — nada mais na fase chega perto de 4,2 un
3. **Brilho do olho pulsa** durante a preparação de ataque (aviso telegrafado — o
   `BossController` já implementa telegrafia; o visual precisa acompanhar)

### Telegrafia visual dos ataques

O `BossController` do projeto avisa antes de golpear (`telegraphDuration = 0,9 s`). A arte
deve tornar isso óbvio:

| Estado | Sinal visual |
|---|---|
| Preparação (0,9 s) | Olho **brilha e pulsa**, corpo se inclina para trás, cajado ergue |
| Ataque | Movimento rápido para baixo, poeira no impacto |
| Recuperação (0,5 s) | Cajado no chão, ombros caídos, olho volta ao normal — **janela de oportunidade legível** |

### Paleta — Ciclope

| Função | Hex | Uso |
|---|---|---|
| Pele (base) | `#7C8471` | Cinza-oliva |
| Pele (sombra) | `#4A5044` | |
| Pele (luz) | `#9BA38C` | |
| **Olho (íris)** | `#FFB020` | **Âmbar — exclusivo do boss** |
| Olho (brilho) | `#FFE08A` | Estado de preparação |
| Esclera | `#EDE4CE` | |
| Pele de cabra | `#B8A184` | Tanga |
| Couro escuro | `#5A4432` | Correias |
| Pedra (bracelete) | `#6E6A63` | |
| Madeira (cajado) | `#6B5138` | |
| Cabelo | `#2E2A26` | |
| **Contorno** | `#231F1C` | |

**Cor de identidade: âmbar `#FFB020`.** Não aparece em nenhum outro lugar da fase.

---

## 3. INIMIGO SECUNDÁRIO — Fera da Ilha

### Conceito

Escolhi **fera selvagem caprina** em vez de sátiro. Razão: o sátiro é humanoide e bípede —
sua silhueta competiria com a de Odisseu à distância, ferindo a hierarquia de leitura
(Art Bible §3). Uma fera **quadrúpede** tem silhueta instantaneamente distinta e comunica
"animal agressivo" sem ambiguidade.

Narrativamente encaixa: é o rebanho selvagem da ilha do ciclope, tornado agressivo.

### Especificação física

| Atributo | Valor |
|---|---|
| Altura | **0,9 unidades** = 90 px @ PPU 100 |
| Comprimento | ~1,1 un = 110 px |
| Proporção vs Odisseu | 0,64× — claramente menor |
| Postura | Quadrúpede, cabeça baixa em posição de investida |

### Design

Deliberadamente **simples** — o briefing pede pouca complexidade, e isso também mantém o
custo de animação baixo:

| Elemento | Descrição |
|---|---|
| **Corpo** | Compacto, caprino, pelo eriçado nas costas |
| **Cabeça** | Baixa, chifres curvos para frente (leitura de "investida") |
| **Olhos** | Pequenos, vermelhos — indicam agressividade sem detalhe facial |
| **Pelo** | Marrom-escuro com dorso mais claro |
| **Cascos** | Escuros, sólidos |
| **Detalhe** | Mínimo. Sem acessórios, sem equipamento |

### Silhueta

Quadrúpede baixa e larga, com dois chifres curvos. Impossível confundir com Odisseu
(bípede, alto, com manto) ou com o Ciclope (gigante, bípede).

### Paleta — Fera

| Função | Hex |
|---|---|
| Pelo (base) | `#6B4A32` |
| Pelo (sombra) | `#422C1D` |
| Pelo (dorso claro) | `#8E6A4A` |
| Chifres | `#C9B79A` |
| Cascos | `#332620` |
| Olhos | `#D64533` |
| **Contorno** | `#241811` |

---

## 4. Tabela comparativa — validação de consistência

Produza esta imagem **antes de animar qualquer coisa**: os três personagens lado a lado,
mesma base, mesma iluminação.

| | Odisseu | Fera | Ciclope |
|---|---|---|---|
| Altura (un) | 1,4 | 0,9 | 4,2 |
| Altura (px) | 140 | 90 | 420 |
| Razão | 1,0× | 0,64× | 3,0× |
| Cor de identidade | Terracota + bronze | — (marrom neutro) | Âmbar |
| Silhueta | Bípede + manto | Quadrúpede + chifres | Gigante + olho único |
| Contorno | `#2A1A14` | `#241811` | `#231F1C` |

**Critério de aprovação:** com os três em preto sólido, um observador deve identificar cada
um. Se dois se confundem, o design volta para revisão.
