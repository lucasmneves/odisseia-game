# Vertical Slice 01 — Ilha dos Ciclopes

Pacote de direção de arte e áudio para a primeira fase do Odisseia a receber arte de
produção.

---

## Como usar

**Se você vai produzir arte:** leia `00_ArtBible.md` inteiro (é curto e é a fonte de
verdade), depois vá para `09_GenerationPrompts.md` e comece pelo Odisseu.

**Se você vai integrar no Unity:** `07_UnityIntegration.md`.

**Se você quer só saber o que falta:** `10_ProductionChecklist.md`.

---

## Documentos

| Ordem | Documento | Conteúdo |
|---|---|---|
| 1 | [`00_ArtBible.md`](00_ArtBible.md) | **Comece aqui.** Estilo, 11 pilares de consistência, escala, resolução (com justificativa), pipeline |
| 2 | [`01_CharacterBible.md`](01_CharacterBible.md) | Odisseu, Ciclope, Fera — design, paletas, silhuetas |
| 3 | [`02_EnvironmentBible.md`](02_EnvironmentBible.md) | Paleta mestra, tileset (53 tiles), backgrounds, 10 props |
| 4 | [`03_SpriteSpecification.md`](03_SpriteSpecification.md) | Canvas, pivots, atlas, **orçamento de textura WebGL** |
| 5 | [`04_AnimationSpecification.md`](04_AnimationSpecification.md) | 19 animações, frames, FPS, sincronia com o código |
| 6 | [`08_VFXSpecification.md`](08_VFXSpecification.md) | 9 conjuntos de efeitos, sincronia por frame |
| 7 | [`05_AudioBible.md`](05_AudioBible.md) | 2 músicas, 25 SFX, níveis de mix, import settings |
| 8 | [`06_NamingConvention.md`](06_NamingConvention.md) | Padrão de nomes de todo o projeto |
| 9 | [`07_UnityIntegration.md`](07_UnityIntegration.md) | Migração sem quebrar gameplay, Tilemap, prefabs, Animators |
| 10 | [`09_GenerationPrompts.md`](09_GenerationPrompts.md) | **Prompts prontos** para cada asset |
| 11 | [`10_ProductionChecklist.md`](10_ProductionChecklist.md) | Tabela de status, sprints, riscos |

---

## Decisões-chave (resumo)

| Decisão | Valor | Onde está justificado |
|---|---|---|
| Resolução | **PPU 100** | `00` §5 — nunca amplia em 1080p; Odisseu fica com 140 px |
| Escala do Odisseu | **1,4 unidades** | Vem do `CapsuleCollider2D` já existente |
| Escala do Ciclope | **4,2 un (3,0×)** | Topo da faixa pedida; ocupa 35% da tela |
| Tile | **1 unidade = 100 px** | Level design fica mensurável em unidades |
| Inimigo secundário | **Fera quadrúpede**, não sátiro | `01` §3 — sátiro competiria com a silhueta do Odisseu |
| Escudo do Odisseu | **Fora do slice** | Não há mecânica de bloqueio; prometeria o que o jogo não entrega |
| Partículas mágicas | **Fora do slice** | Não há magia nesta fase; brilho é reservado a interativos |
| Fase alvo | `Level_04_Ciclopes.unity` | `00` §0 — a fase de gameplay já existe e funciona |

---

## O que este pacote **não** é

Não são os arquivos de arte. **Não gero imagens nem áudio** — o que existe aqui é a
especificação e os prompts para que um artista ou uma IA geradora produza os assets
mantendo consistência.

Todos os 64 itens do checklist estão em `TODO`.

---

## Alerta de numeração

O briefing chama isto de "Fase 1". A campanha implementada tem **Fase 1 = Troia** e
**Fase 4 = Ciclopes**. Este pacote é o **Vertical Slice 01** — a primeira fase a receber
arte —, materializado sobre a cena `Level_04_Ciclopes` que já tem gameplay validado.

Se a intenção era reordenar a campanha para o Ciclope ser a primeira fase narrativa, isso
é uma mudança de design de campanha (afeta `LevelDefinition`/`CampaignManager`) e precisa
ser feita como tarefa separada. Detalhes em `00_ArtBible.md` §0.
