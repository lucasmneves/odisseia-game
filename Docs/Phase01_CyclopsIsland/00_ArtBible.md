# Art Bible — Ilha dos Ciclopes (Vertical Slice 01)

> **Documento-mãe.** Toda decisão visual dos outros documentos deriva daqui. Se houver
> conflito entre este arquivo e qualquer outro, este vence.

---

## 0. Nota de escopo — leia antes de tudo

**Discrepância de numeração, resolvida explicitamente:**

O briefing chama isto de "FASE 1". O projeto já tem uma campanha de 16 fases onde
**Fase 1 = Troia** e **Fase 4 = Ciclopes** (`Level_04_Ciclopes.unity`, com Polifemo,
tema caverna e luta de boss já implementados em gameplay).

**Decisão adotada:** este pacote é o **Vertical Slice 01** — a *primeira fase a receber
arte de produção*, não a primeira fase da narrativa. Ela se materializa sobre a cena
existente `Level_04_Ciclopes`.

Isso preserva os dois trabalhos: a ordem narrativa da campanha continua íntegra, e a arte
entra numa fase que já tem gameplay validado (boss, caverna, plataformas) para testar.

Se a intenção for realmente **reordenar a campanha** para o Ciclope virar a primeira fase,
isso é uma decisão de design de campanha e afeta `LevelDefinition`/`CampaignManager` — me
avise e eu faço a reordenação como tarefa separada.

**O que este documento é:** especificação e prompts para produção de arte.
**O que não é:** os arquivos de arte em si. Não gero imagens nem áudio — os prompts do
documento `09` existem para que um artista ou uma IA geradora produza os assets mantendo
consistência.

---

## 1. Declaração de estilo (uma frase)

> Ilustração 2D **estilizada e pintada**, com contornos suaves, formas grandes e legíveis,
> paleta quente mediterrânea contrastando com sombras frias — heroica e convidativa, nunca
> sombria ou fotorrealista.

### Referência conceitual — e limite legal

A referência é **exclusivamente de gênero e sensação**: *action platformer 2D + fantasia
mitológica grega + aventura épica*. Grandeza de escala, arquitetura grega antiga, monstros
enormes, iluminação dramática.

**Proibido em qualquer asset ou prompt:** personagens, sprites, poses específicas, UI,
logos, tipografia, esquemas de cor assinatura ou qualquer elemento proprietário de God of
War, Sons of Sparta ou qualquer outra franquia. Todo prompt no documento `09` inclui uma
cláusula explícita de design original. Odisseia precisa ter identidade própria — a
referência serve para calibrar o *tom*, não para ser copiada.

---

## 2. Os 11 pilares de consistência

Estes são os parâmetros que fazem tudo "parecer o mesmo jogo". São obrigatórios em
**todos** os assets.

| # | Parâmetro | Regra definida |
|---|---|---|
| 1 | **Estilo** | Pintura digital estilizada, formas cheias. Sem pixel art, sem cel-shading duro, sem fotorrealismo |
| 2 | **Proporções** | Personagens heroicos: **6,5 cabeças** de altura para Odisseu. Cabeça levemente maior que o realista para leitura facial |
| 3 | **Outline** | Contorno externo **escuro colorido** (nunca preto puro), 2–3 px @ PPU 100. Interno: sem outline — separação por valor e cor |
| 4 | **Nível de detalhe** | Detalhe concentrado na **silhueta e no rosto**. Superfícies internas simplificadas. Regra: legível a 25% do tamanho |
| 5 | **Iluminação** | Fonte principal **superior-esquerda**, quente (sol mediterrâneo). Luz de preenchimento fria vinda do céu/mar. Consistente em todos os assets |
| 6 | **Perspectiva** | **Ortográfica lateral pura** para gameplay. Zero perspectiva em elementos colidíveis. Apenas backgrounds podem sugerir profundidade |
| 7 | **Saturação** | Personagens **alta** (60–80%). Midground **média** (40–55%). Background **baixa** (20–35%). É o principal mecanismo de leitura |
| 8 | **Paleta** | Ver documento `02` §Paleta. Nenhuma cor fora da paleta definida |
| 9 | **Escala** | Odisseu = **1,4 unidades Unity**. Todo o resto é medido a partir dele. Ver §4 |
| 10 | **Espessura visual** | Membros e objetos com massa aparente — nada de formas finas/frágeis. Mínimo 8 px de espessura @ PPU 100 |
| 11 | **Sombras** | Sombra própria: 1 tom, borda suave. Sombra projetada: elipse escura semitransparente sob personagens/props, **obrigatória** (ancora no chão) |

---

## 3. Hierarquia de leitura (a regra mais importante)

O jogador **nunca** pode confundir personagem com cenário. A hierarquia é imposta por
**três eixos simultâneos** — não confie em apenas um:

| Camada | Saturação | Valor (luminosidade) | Contorno | Detalhe |
|---|---|---|---|---|
| **Odisseu** | Altíssima | Médio-alto, destaca do fundo | Mais forte do jogo | Máximo |
| **Inimigos / Boss** | Alta | Escuro, pesado | Forte | Alto na silhueta |
| **Interativos** (coletável, checkpoint, objetivo) | Alta + **brilho** | Mais claro que o entorno | Médio | Médio |
| **Plataformas colidíveis** | Média | Contrasta com o fundo | Médio | Médio |
| **Props decorativos** | Média-baixa | Próximo do fundo | Fraco | Baixo |
| **Midground** | Baixa | Escurecido/esmaecido | Nenhum | Baixo |
| **Background** | Muito baixa | Bem claro ou bem escuro | Nenhum | Mínimo |

**Teste obrigatório de validação:** reduza qualquer tela a **25%** e converta para escala
de cinza. Odisseu e as plataformas colidíveis ainda devem ser distinguíveis. Se falhar, o
asset volta para revisão — sem exceção.

**Regra do colidível:** tudo que o jogador pode pisar tem **borda superior mais clara**
(1 tom acima do corpo do bloco). Isso torna a superfície pisável identificável de relance.
Props sem colisão nunca recebem essa borda.

---

## 4. Escala — derivada do jogo real, não arbitrária

Todos os números abaixo vêm de valores **já implementados** no projeto (`Player.prefab`,
`CameraFollow`, `PlayerController`) — não foram escolhidos por estética.

### Medidas de origem (verificadas no projeto)

| Valor | Medida | Origem |
|---|---|---|
| Altura de Odisseu | **1,4 unidades** | `CapsuleCollider2D.size.y` do `Player.prefab` |
| Largura de Odisseu | **0,6 unidades** | `CapsuleCollider2D.size.x` |
| Altura visível da câmera | **12 unidades** | `CameraFollow.orthographicSize = 6` × 2 |
| Largura visível (16:9) | **21,3 unidades** | 12 × 16/9 |
| Velocidade máxima | 6 un/s | `PlayerController.maxSpeed` |
| Força de pulo | 12 | `PlayerController.jumpForce` |
| Gravidade | 3× padrão = 29,43 un/s² | `PlayerController.gravityScale` |

### Consequências para o level design (calcule, não chute)

| Métrica | Valor | Como foi obtido |
|---|---|---|
| **Altura máxima de pulo** | **2,45 un** | v²/2g = 12²/(2×29,43) |
| Tempo até o ápice | 0,41 s | v/g |
| Duração total do pulo | ~0,82 s | 2 × tempo até ápice |
| **Alcance horizontal máximo** | **~4,9 un** | maxSpeed × duração do pulo |

**Regras de design derivadas** (com margem de segurança, porque tocar o limite exato é
frustrante):

- Vãos saltáveis: **≤ 4,0 un** (margem de 18%)
- Degraus/plataformas alcançáveis: **≤ 2,0 un** de altura (margem de 18%)
- Corredor mínimo para o jogador passar: **1,8 un** de altura
- Vão intransponível de propósito (exige rota alternativa): **≥ 6,0 un**

### Tabela de escala dos personagens

| Personagem | Altura (un) | Razão vs Odisseu | Ocupa da tela |
|---|---|---|---|
| Odisseu | 1,4 | 1,0× | 11,7% |
| Inimigo secundário (fera) | 0,9 | 0,64× | 7,5% |
| **Ciclope (boss)** | **4,2** | **3,0×** | **35%** |

O Ciclope a 3,0× fica no topo da faixa pedida (2–3×) — a escolha é deliberada: um boss que
ocupa mais de um terço da altura da tela comunica ameaça sem precisar de nenhum texto.
Também bate com o gameplay já implementado (`Level_04_Ciclopes` posiciona Polifemo numa
saliência acima da passagem, sem collider — ver documento `07`).

---

## 5. Resolução base — decisão documentada

> O briefing pede explicitamente para não escolher resolução arbitrariamente. Aqui está o
> raciocínio completo.

### A escolha: **PPU = 100** (Pixels Per Unit)

### Por quê

1. **Nunca faz upscale.** A câmera mostra 12 unidades verticais. A 100 PPU, isso são
   **1200 px** de conteúdo. Numa tela 1080p, a arte é *reduzida* para 0,9× — redução é
   nítida, ampliação é borrada. Se escolhêssemos PPU 64 (768 px), qualquer tela acima de
   768p ampliaria e borraria.

2. **Odisseu fica em 140 px de altura.** É o tamanho onde rosto, espada e armadura ainda
   se leem — abaixo de ~100 px, detalhe facial vira ruído; acima de ~200 px, o custo de
   textura explode sem ganho visível no tamanho em que ele aparece na tela.

3. **É o padrão do Unity.** PPU 100 é o default de sprite do engine. Reduz atrito de
   importação e erros de escala em prefabs.

4. **Cabe no orçamento WebGL.** Ver a tabela de orçamento de textura no documento `03`.

### O que foi descartado, e por quê

| Opção | Motivo da recusa |
|---|---|
| PPU 16/32 (pixel art) | O estilo pedido é pintado/cinematográfico. Pixel art seria outra direção de arte |
| PPU 64 | Odisseu ficaria com 90 px — perde leitura facial, e amplia (borra) acima de 768p |
| PPU 128 | Múltiplos quebrados (1,4 × 128 = 179,2 px). Ganho invisível, custo de textura +64% |
| PPU 200+ | Insustentável para WebGL. Atlas do boss sozinho passaria de 16 MB |

### Grade de tiles

**Tile = 1,0 unidade = 100 × 100 px.** Um tile equivale a um passo de mundo, o que torna o
level design mensurável: "vão de 4 tiles" é literalmente 4,0 unidades = saltável.

---

## 6. Pipeline de produção e controle de consistência

### Ordem obrigatória (não pule etapas)

```
A. Direção artística   ← este documento + paleta travada
B. Concept art          ← Odisseu primeiro; ele calibra tudo
C. Personagens          ← sprites estáticos, aprovados em escala lado a lado
D. Ambiente             ← tileset + backgrounds
E. Props
F. Animações            ← só depois do sprite base aprovado
G. VFX                  ← só depois das animações (o timing depende delas)
H. Áudio
I. Integração Unity
J. Validação do slice
```

**Motivo da ordem:** cada etapa trava parâmetros que a seguinte consome. Animar antes de
aprovar o sprite base significa refazer todos os frames. Fazer VFX antes da animação
significa efeitos dessincronizados do golpe.

### Os três portões de qualidade

Nenhum asset é aceito sem passar nos três:

1. **Portão de silhueta** — preencha o asset de preto sólido. Ele ainda é identificável?
2. **Portão de escala 25%** — reduza a 25% em escala de cinza. Ainda se lê contra o fundo?
3. **Portão de paleta** — todas as cores estão na paleta do documento `02`? (Conte-gotas
   comparando com a paleta; nenhuma cor "quase igual" é aceitável.)

### Asset-chave de referência

**`CHR_Odysseus_Idle` frame 01 é o asset canônico.** Ele é produzido primeiro e aprovado
antes de qualquer outra coisa. Todo asset posterior é comparado lado a lado com ele para
verificar estilo, contorno, iluminação e saturação. Se Odisseu mudar, tudo muda — por isso
ele é travado primeiro.

---

## 7. Índice dos documentos

| Doc | Conteúdo |
|---|---|
| `00_ArtBible.md` | **Este arquivo.** Estilo, escala, resolução, pipeline |
| `01_CharacterBible.md` | Odisseu, Ciclope, inimigo secundário — design e paletas |
| `02_EnvironmentBible.md` | Tileset, backgrounds, paleta mestra da fase |
| `03_SpriteSpecification.md` | Tamanhos de canvas, pivots, atlas, orçamento de textura |
| `04_AnimationSpecification.md` | Frames, FPS, duração, sincronia com gameplay |
| `05_AudioBible.md` | Música, SFX, especificação técnica de áudio |
| `06_NamingConvention.md` | Padrão de nomenclatura completo |
| `07_UnityIntegration.md` | Import settings, prefabs, tilemap, migração do placeholder |
| `08_VFXSpecification.md` | Efeitos visuais e sincronia com animação |
| `09_GenerationPrompts.md` | **Prompts prontos para gerar cada asset** |
| `10_ProductionChecklist.md` | Tabela de status de produção |
