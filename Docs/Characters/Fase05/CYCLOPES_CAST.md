# Fase 05 — Ciclopes: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Aprovado em 2026-09-29.
Contém o **POLYPHEMUS CHARACTER MASTER v1** — o primeiro chefe do projeto com arte.

- Master aprovado: `Fase05/_master_aprovado.png` · reprovados: `_r1_reprovado.png`, `_r2_dois_olhos.png`
- Olho único em cada quadro: `Fase05/_olho_por_quadro.png` · tiras: `Fase05/_tiras.png`
- Capturas: `Fase05/_capturas/` (`antes_030` = placeholder; `polifemo_repouso|aviso|golpe`; `camara_022|030`)
- Id e prompts: `Docs/Characters/cast.json` (`Polyphemus`, e o reprovado `Polyphemus_r1`)

## 1. CastProbe

O probe ganhou nesta fase um bloco de **chefe**: posição, tempos do ataque, pontos de golpe, vida,
colisores e caixas de desenho.

| Item | Achado |
|---|---|
| Personagens | Odisseu (Player) e **Polifemo** (`BossController`) — nada mais |
| Outros ciclopes | **nenhum** → nenhum Cyclops Master, nenhuma variação |
| Tripulação, NPCs, narrativos | nenhum (as falas são do Odisseu e do narrador) |
| Animator | 0 (o projeto usa `SpriteAnimator`) |
| Polifemo — arte | **placeholder**: três quadrados (`Body` 2,2 × 3,2, `Head` 1,4, `Eye` 0,3), PPU 8, escalados; flutuando de y = 1,0 a 5,6 — **3 un acima do chão da câmara**, sobre a fogueira, cabeça cortada pelo topo da câmera |
| Polifemo — gameplay | estacionário; a cada **3,5 s** telegrafa (**0,9 s**) e golpeia **três pontos fixos no chão** (x = 26, 32, 36; raio 1,3; dano 35); recuperação 0,5 s |
| Polifemo — vida | `HealthSystem` com **999**; **nenhum colisor** — espada e flecha nunca o acertam |
| Estados acionados pelo código | **nenhum**: o `BossController` expõe `AttackTelegraphed` e `AttackExecuted`, e ninguém os escutava |

### O que isso decide

| Estado | Gera? | Por quê |
|---|---|---|
| Idle | sim | é o que ele faz entre golpes |
| Telegraph (erguer a clava) | sim | `AttackTelegraphed` — o aviso precisa ser LIDO no corpo dele, não só nos marcadores |
| Attack (golpe no chão) | sim | `AttackExecuted` |
| Walk / Run | **não** | ele não se move |
| Hit / Stun | **não** | sem colisor, o `Damaged` nunca dispara |
| Death / Roar | **não** | 999 de vida, nenhum evento de rugido |
| Arremesso de pedra | **não** | o código só golpeia pontos fixos; a arma é a **clava** — a câmara já tem uma clava gigante como prop |

## 2. POLYPHEMUS CHARACTER MASTER v1

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Enemies/CHR_Polyphemus.png` (1554 × 612) |
| PixelLab | `41ff35f6-2355-4bbc-aa7d-f301bcbcbf33` · v3 · size 208 |
| Altura | **197 px = 4,60 un = 3,28× o Odisseu** — a mesma altura do placeholder que substitui (não é escala nova de gameplay) |
| PPU | 42,857 (o do projeto); escala do Transform 1 |
| Pivô | base da célula (pés do Idle), **centrado no corpo** — espelhar não o desloca |
| Estados | Idle 6 `[0,1,2,3,2,1]` · Telegraph 6 (Slam 0–5) · Attack 2 (Slam 6–7) |
| Cores | 58 no master — pele bronzeada, velo cru, corda, madeira de oliveira, cabelo castanho escuro. Nenhum carmesim, nenhum neon |
| Silhueta | ombros largos e curvados, braços enormes, mãos grandes, pernas pesadas, velo sobre um ombro, clava de oliveira baixa ao lado — gigante em qualquer tamanho |
| Rosto | **um olho só, grande e redondo, no meio da testa, sob uma monocelha pesada** (pintado por código, ver §3) |
| Direção | olha para a **esquerda** (`flipX`): é de lá que o Odisseu entra na câmara |

Prompt aprovado (base `side view, 16-bit pixel art game character, Greek mythology, wild rocky island of the Cyclopes`):

> Polyphemus the cyclops of Greek myth, a colossal hulking shepherd giant, his face has only one eye:
> one huge round cyclops eye in the middle of his face where two eyes would be, directly above the nose,
> under one heavy unibrow, wild dark curly hair and a thick unkempt beard, broad hunched shoulders, huge
> muscular arms and oversized hands, barrel chest, thick heavy legs, weathered tanned skin, a rough
> off-white sheep fleece wrapped around his waist and slung over one shoulder, rope belt, bare feet,
> holding a huge knotted olive-wood club low at his side with its head resting on the ground, the club
> kept away from his face, the club stays attached to his hands in every frame, menacing brutish but
> slightly comical expression

Animação `Slam` (Receita B, 8 quadros — o máximo do v3 com canvas de 256 px):

> slowly raising the huge club high over his head with both hands, body leaning back to wind up,
> holding it there for a moment, then slamming it straight down onto the ground in front of him with
> all his weight, body bending forward as the club head strikes the ground, side view facing right, …

**Uma animação, dois estados:** o aviso e o golpe saem da mesma geração dividida (0–5 e 6–7) — o golpe
começa exatamente onde o aviso termina, e custou uma geração em vez de duas.

## 3. O olho — onde o modelo falhou, e a correção

| Tentativa | Resultado | Custo |
|---|---|---|
| r1: "a single huge round eye set in the centre of his forehead" | **dois olhos normais + um terceiro pequeno na testa** — um humano de três olhos; e na vista de jogo a clava cobria o rosto | 7 |
| r2: "his face has only one eye … where two eyes would be" + clava baixa | corpo ótimo, rosto livre — **mas dois olhos de novo**, sob uma monocelha | 7 |

Duas falhas no mesmo eixo → parar de gerar (a regra das fases 11–14). De perfil, qualquer rosto mostra
um olho só; o que faz um ciclope é esse olho ser **grande** e estar **no meio da testa**. É defeito
local (~12 × 12 px), então foi corrigido por código: **`Tools/cyclops-eye.js`**.

- Apaga o olho humano (vira pele de bochecha), pinta um olho de ~9 × 7 px (contorno, esclera, íris
  puxada para a frente com 1 px de esclera antes do contorno, pupila, brilho) e uma monocelha pesada.
  **Só cores que já estão no sprite.**
- **Animar a partir do quadro editado não basta:** o v3 usa o quadro inicial
  (`custom_start_frame_base64`) como referência de POSE, não de anatomia, e redesenhou o olho humano
  nos outros 14 quadros. Então o olho é pintado **em cada quadro**:
  - a posição da cabeça vem de casamento de padrão com a cabeça da rotação original (sem a região do olho);
  - quando o olho humano está aberto, a posição **real** dele manda (a cabeça inclina no golpe e a
    translação sozinha erra) — com um teto de 12 px de esclera, para não ancorar no olho já pintado.
- Os quadros originais do v3 ficam em `Polyphemus/<Estado>_east_v3_original/`; a rotação original em
  `Polyphemus_east_v3_original.png`. A ferramenta é idempotente.

## 4. Integração no Unity

- **`BossAnimator`** (`Assets/Scripts/Enemies/BossAnimator.cs`, o único código de runtime novo): o
  equivalente do `EnemyAnimator` para o chefe. Escuta `AttackTelegraphed` → Telegraph e
  `AttackExecuted` → Attack; segura o impacto 0,35 s e volta ao Idle. **Não toca no `BossController`**
  — tempo, dano, alvo e pontos de ataque são os mesmos. Sem ele, a arte estaria na folha e nunca
  rodaria (a lição de "estados sem gatilho").
- **`CyclopsCastDresser.Dress`**: troca o desenho do `Body` (escala 1, cor branca, `flipX`, ordem 1 —
  na frente da fogueira e dos jarros, atrás do jogador), **apoia os pés no chão da câmara** (y = −2, o
  mesmo dos pontos de ataque), remove `Head` e `Eye` (quadrados sem referência externa), configura FPS
  e loop por estado (Idle 5 loop, Telegraph 7 sem loop ≈ 0,85 s dentro do aviso de 0,9 s, Attack 10 sem
  loop) e liga o `BossAnimator`.
- **`CyclopsCastDresser.Poses`**: fotografa repouso, aviso e golpe com o Odisseu no chão diante dele,
  **sem salvar a cena**. Para isso o `Render` do `PrologueScreenshot` passou de `private` a `internal`.
- O Polifemo é objeto da cena, não de prefab — nenhum prefab compartilhado envolvido.
- **`PlaceholderProbe`**: as três justificativas do placeholder do Polifemo foram **retiradas** — se um
  quadrado voltar, o probe acusa. A fase sai limpa sem nenhuma justificativa de personagem.

## 5. Validação

| Teste | Resultado |
|---|---|
| CastProbe | Polifemo a 42,857 px/un, escala 1, desenho x 27,0..33,0 · y −2,00..2,71; `BossAnimator` presente |
| `PlaceholderProbe` (Ciclopes) | nada visível, nada inativo |
| `CampaignValidation` | passa |
| Compilação | 0 erros |
| Build WebGL | ver relatório |
| Capturas | 3 poses legíveis; **olho único visível no tamanho de jogo** nas três; escala 3,3× imediata ao lado do Odisseu; pés no mesmo chão; a fogueira atrás dá contraluz |

**Não testado:** play mode (o ciclo aviso → golpe → Idle só roda em runtime), navegador, mobile, gamepad.

## 6. Problemas encontrados

| Problema | Situação |
|---|---|
| Chefe era placeholder flutuando 3 un acima do chão | **corrigido** — arte, pés no chão |
| Nenhum estado visual era acionado (eventos sem ouvinte) | **corrigido** — `BossAnimator` |
| v3 desenha dois olhos (2 tentativas) | **corrigido por código** — `cyclops-eye.js` |
| v3 redesenha o olho humano ao animar a partir do quadro editado | **corrigido por código**, quadro a quadro |
| Slam de 12 quadros recusado (máx. 8 em canvas de 256) | refeito com 8 — erro de validação, não cobrado |
| Janela de recorte deslocada pela clava (~0,6 un do corpo) | **corrigido** — janela centrada no corpo (`centroNoCorpo`) |
| O chefe não pode ser atingido (sem colisor) | **não mexido** — é regra de gameplay do `BossController` ("esquiva, não derrota") |

## 7. Custo

**28 gerações** (669 → 697 de 2000): r1 7 (**descartado**) + r2 7 + Idle 6 + Slam 8.
Um asset descartado (r1). O pedido de Slam com 12 quadros foi recusado sem cobrança.
