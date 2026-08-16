# Audio Bible — Ilha dos Ciclopes

> **Estado atual:** o jogo tem 12 clipes **placeholder gerados proceduralmente** (senoides
> e envelopes escritos como WAV por script). São suficientes para validar a integração —
> `AudioManager`, `AudioLibrary` e `SceneAudio` já funcionam — mas não têm qualidade de
> produção. Este documento especifica o que os substitui.

---

## 1. Identidade sonora

> Aventura mediterrânea: instrumentos acústicos antigos, percussão de pele, cordas
> dedilhadas, modos gregos. Épico pela **amplitude e pelo espaço**, não por muralha de som.

### Instrumentação de referência

| Família | Instrumentos |
|---|---|
| Cordas | Lira, cítara, alaúde (dedilhado), seção de cordas em sustentação |
| Sopros | Aulos (flauta dupla), flauta de osso, siringe |
| Percussão | Tympanon (tambor de moldura), krotala, tambor grave de guerra |
| Voz | Coro sem palavras (vogais abertas), usado com parcimônia |

### Modos musicais

- **Dórico** (mi–mi na escala branca) para exploração — soa antigo sem soar melancólico
- **Frígio** (mi bemol) para tensão — o segundo grau menor cria desconforto natural

Isso não é decoração: modos gregos antigos dão autenticidade cultural sem recorrer a
clichê de "música épica de trailer".

### Regra de originalidade

Nenhuma composição pode citar, adaptar ou reinterpretar trilha existente de qualquer
franquia. A instrumentação e os modos são de domínio histórico; **as melodias precisam ser
originais**.

---

## 2. Música

### MUS_CyclopsIsland_Main — exploração

| Parâmetro | Valor |
|---|---|
| Duração | **2:30–3:00** |
| Loop | **Perfeito** (sem clique na emenda) |
| BPM | 92–100 |
| Modo | Dórico |
| Dinâmica | Média, com espaço para respirar |

**Estrutura sugerida:**

| Seção | Duração | Conteúdo |
|---|---|---|
| A — Chegada | 0:00–0:40 | Lira solo, esparsa. Ondas ao fundo. Sensação de desembarque |
| B — Exploração | 0:40–1:30 | Entra percussão leve, aulos assume a melodia. Movimento |
| C — Inquietação | 1:30–2:10 | Modo escurece, cordas em sustentação. Algo observa |
| A' — Retorno | 2:10–2:45 | Volta ao tema, com mais corpo. **Emenda com 0:00** |

### MUS_CyclopsIsland_Boss — Ciclope

| Parâmetro | Valor |
|---|---|
| Duração | **2:00–2:30** |
| Loop | Perfeito |
| BPM | 120–132 |
| Modo | Frígio |
| Dinâmica | Alta, percussão dominante |

**Estrutura:**

| Seção | Duração | Conteúdo |
|---|---|---|
| Intro | 0:00–0:15 | Tambor grave solitário. Batidas espaçadas = passos |
| A — Confronto | 0:15–1:00 | Percussão completa, cordas em ostinato baixo |
| B — Escalada | 1:00–1:45 | Camadas se acumulam, coro entra, tensão progressiva |
| Sustentação | 1:45–2:15 | Pico mantido. **Emenda com 0:15**, não com 0:00 |

**Detalhe deliberado:** o loop do boss volta para 0:15, não para o início. A intro de
tambor solitário toca uma vez só, na entrada da arena — repeti-la a cada 2 minutos
quebraria a tensão.

**Elemento sonoro associado ao Ciclope:** um **tambor grave em intervalo irregular**
(a cada 3 ou 5 tempos, nunca a cada 4). A irregularidade cria desconforto subliminar e
"soa" como um gigante mancando.

---

## 3. Sound Effects

Todos: **mono**, 44,1 kHz, WAV 16-bit na origem (o Unity comprime na importação).

### Player

| Nome | Duração | Descrição |
|---|---|---|
| `SFX_Player_Jump` | 0,15 s | Esforço curto + roçar de sandália |
| `SFX_Player_Land` | 0,20 s | Impacto de sola em pedra + cascalho |
| `SFX_Player_Attack` | 0,25 s | Lâmina cortando o ar, tom médio-agudo |
| `SFX_Player_AttackHit` | 0,20 s | Bronze contra carne/couro — impacto abafado, **não metálico agudo** |
| `SFX_Player_Damage` | 0,30 s | Grunhido masculino contido + impacto |
| `SFX_Player_Death` | 0,80 s | Queda + expiração. **Sem grito dramático** (tom amigável do jogo) |
| `SFX_Player_Step` | 0,10 s | Passo em pedra. **3–4 variações** para evitar repetição óbvia |

### Inimigos

| Nome | Duração | Descrição |
|---|---|---|
| `SFX_Enemy_Attack` | 0,25 s | Investida — bufo animal + cascos |
| `SFX_Enemy_Hit` | 0,15 s | Impacto em pelo/carne |
| `SFX_Enemy_Damage` | 0,30 s | Balido/rosnado de dor, curto |
| `SFX_Enemy_Death` | 0,50 s | Queda, som se apagando |

### Ciclope

Todos com **peso grave** — corpo de 4,2 unidades precisa soar enorme.

| Nome | Duração | Descrição |
|---|---|---|
| `SFX_Cyclops_Roar` | 2,0 s | Rugido gutural profundo. Toca na entrada da arena |
| `SFX_Cyclops_Attack` | 0,40 s | Cajado cortando o ar + impacto em pedra |
| `SFX_Cyclops_AttackHeavy` | 0,70 s | Pancada de dois braços. Sub-graves fortes |
| `SFX_Cyclops_Hit` | 0,25 s | Impacto em pele grossa — abafado, denso |
| `SFX_Cyclops_Damage` | 0,60 s | Rugido de dor, mais agudo que o `Roar` |
| `SFX_Cyclops_Death` | 2,5 s | Queda em estágios + tremor final |
| `SFX_Cyclops_Footstep` | 0,35 s | Passo pesado + cascalho. **Acompanha screen shake** |

**Aviso sonoro do ataque:** durante os 0,9 s de `AttackPrepare`, um **sub-grave crescente**
(rumble subindo) dá ao jogador um segundo canal de aviso além do visual. É acessibilidade
real — jogadores que perdem a dica visual ainda ouvem.

### Ambiente (loops)

| Nome | Duração | Onde |
|---|---|---|
| `SFX_Ambience_Wind` | 20 s loop | Área externa |
| `SFX_Ambience_Birds` | 25 s loop | Área externa, esparso |
| `SFX_Ambience_Waves` | 18 s loop | Praia |
| `SFX_Ambience_Water` | 15 s loop | Poças, gotejamento |
| `SFX_Ambience_Cave` | 22 s loop | Interior — reverb longo, gotas |
| `SFX_Ambience_Fire` | 8 s loop | Junto de `PROP_Torch` |
| `SFX_RockFall` | 1,2 s | Evento — queda de pedras |

---

## 4. Especificação técnica

### Níveis (evita o erro clássico de mixagem de jogo)

| Categoria | Pico alvo | Notas |
|---|---|---|
| Música | **-18 dBFS RMS** | Fica atrás de tudo |
| SFX de player | -12 dBFS | Presente, não dominante |
| SFX de impacto | -8 dBFS | Os mais altos do jogo |
| Ciclope | -10 dBFS | Peso vem dos graves, não do volume |
| Ambiente | **-24 dBFS** | Quase subliminar |

**Sem normalização para 0 dB.** Headroom é o que impede o mix de virar lama quando cinco
sons disparam juntos.

### Import settings — Unity

| Tipo | Load Type | Compressão | Qualidade | Mono |
|---|---|---|---|---|
| Música | `Streaming` | Vorbis | 0,5 | Não (estéreo) |
| SFX curto (< 1 s) | `Decompress On Load` | Vorbis | 0,7 | **Sim** |
| SFX longo / ambiente | `Compressed In Memory` | Vorbis | 0,5 | Sim |

**Música em `Streaming`:** duas faixas de ~3 min descomprimidas na memória custariam
~30 MB. Streaming mantém isso fora da RAM — decisão importante para WebGL.

### Orçamento

| Item | Estimativa |
|---|---|
| 2 músicas (Vorbis 0,5, ~3 min) | ~2,6 MB |
| ~25 SFX | ~0,5 MB |
| 7 loops de ambiente | ~0,9 MB |
| **Total** | **~4,0 MB** |

Substitui os ~1,2 MB de placeholder atuais. Somado ao orçamento de arte (`03` §5), o build
final projetado fica em **~13 MB** — aceitável para WebGL, mas é o teto. Se apertar, corte
a duração das músicas antes de cortar SFX (impacto perceptual muito menor).

---

## 5. Integração com o sistema existente

O `AudioLibrary.asset` já tem os campos. A substituição é **trocar as referências** —
nenhum código muda:

| Campo do `AudioLibrary` | Placeholder atual | Substituto |
|---|---|---|
| `levelMusicTense` | `music_level_tense` | `MUS_CyclopsIsland_Main` |
| *(novo campo necessário)* | — | `MUS_CyclopsIsland_Boss` |
| `sfxAttack` | `sfx_attack` | `SFX_Player_Attack` |
| `sfxHit` | `sfx_hit` | `SFX_Player_AttackHit` |
| `sfxPlayerDamage` | `sfx_player_damage` | `SFX_Player_Damage` |
| `sfxDeath` | `sfx_death` | `SFX_Player_Death` |
| `sfxJump` | `sfx_jump` | `SFX_Player_Jump` |

**Lacuna identificada:** o `AudioLibrary` atual não tem campo para música de boss nem para
os SFX do Ciclope/ambiente. Precisa de extensão — está registrado como tarefa de código no
`10_ProductionChecklist.md`, fora do escopo desta etapa de arte.
