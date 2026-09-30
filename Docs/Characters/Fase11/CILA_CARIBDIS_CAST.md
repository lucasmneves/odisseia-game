# Fase 11 — Cila e Caríbdis: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Aprovado em 2026-09-30.
Contém o **CILA (SCYLLA) CHARACTER MASTER v1**. Caríbdis é classificada como **ENVIRONMENT / ENTITY ART** e não
ganha master.

- Master e rotações: `Fase11/_master_rotacoes.png` · tiras: `_tiras.png`
- Capturas: `Fase11/_capturas/` (`antes_006|016`, `cila_repouso|aviso|golpe`, `estreito_006|012|016`)
- Id e prompts: `Docs/Characters/cast.json` (`Scylla`)

## 1. CastProbe e classificação

| Entidade | Implementação encontrada | Classificação |
|---|---|---|
| **Cila** | `BossController` "Giant" em x = 6 — o marcador que o `CilaCaribdisSceneDresser` deixou, com a falésia e a caverna escavadas em volta dele. Placeholder: quadrados (corpo 3,0 + cabeça 1,3), y = 1,0 a 5,25. A cada **2,8 s** avisa (**0,7 s**) e golpeia **3 pontos do convés** (x = 3, 6, 9; raio 1,4). 999 de vida, **sem colisor** | **BOSS** |
| **Caríbdis** | `CilaCaribdisScenery/Charybdis_Art`: folha `FX_Charybdis` de 8 quadros (estado Spin) no `SpriteAnimator`, feita pelo `Tools/build-cila-fx.js` — **um** asset gerado, girado em coordenadas polares por código (emenda perfeita, custo zero). Escala 0,42 no Transform; colisão/perigo pelos `TidalHazard`/`KillZone`, não pela arte | **ENVIRONMENT / ENTITY** |
| Odisseu | Player; nenhum estado especial acionado | reutilizado |
| Tripulação, NPCs, inimigos | nenhum | — |
| Animator | 0 | — |

Fala de abertura (Odisseu): "Um estreito, dois monstros. Caríbdis engole o mar inteiro três vezes ao dia;
**Cila arranca homens do convés com seis bocas**. Não há como vencer — só atravessar."

**Estados da Cila** — só o que o `BossController` produz (via `BossAnimator`): **Idle, Telegraph, Attack**.
Não gerados: Dive, Rise, Roar, Hit, Death, TentacleAttack (nenhum evento os aciona; sem colisor, 999 de vida).

**Caríbdis não foi tocada.** É entidade de ambiente já bem resolvida; forçar um Character Master seria
contra o briefing. A escala 0,42 (≈ 2,4× a densidade do projeto) fica registrada como pendência de
ENVIRONMENT ART, junto com o palácio de Éolo.

## 2. CILA CHARACTER MASTER v1

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Enemies/CHR_Scylla.png` (1122 × 579, 14 quadros) |
| PixelLab | `09cce54d-0f32-4b8f-b289-5cb2f76255cb` · v3 · size 192 |
| Altura | 187 px = **4,36 un** (célula 4,46) — a altura do marcador, não escala nova |
| Forma | **torso de mulher e, da cintura para baixo, seis pescoços de serpente-marinha** com cabeças que mordem — a Cila clássica. As seis cabeças leem na silhueta mesmo reduzidas |
| Paleta | **azul-profundo e violeta** (escamas, cabelo), **cinza-ardósia** (pele), **coral** (barbatanas, acento), osso (dentes). **Não é o turquesa das Sereias**; sem carmesim |
| Estados | Idle 6 `[0,1,2,3,2,1]` · **Telegraph 4** (Strike 0–3: as cabeças recuam para o alto) · **Attack 4** (Strike 4–7: mergulham para a frente e para baixo — "arrancar homens do convés") |
| Direção | olha para a esquerda (o Odisseu entra no estreito por lá) |

Prompt (base `side view, 16-bit pixel art game character, Greek mythology`):

> Scylla the sea monster of Greek myth, the upper body of a fierce woman with long wild dark violet-blue
> hair and slate-grey skin, from her waist downward a writhing mass of six long serpent necks each ending in
> a snapping sea-serpent head with pale bone teeth, deep blue and violet scales, coral-pink fins along the
> necks, clawed hands, menacing expression

Strike (uma geração, dois estados):

> rearing back with all serpent necks drawn up high, jaws open, holding for a moment, then lunging forward
> and down, every serpent head darting down to snatch prey below, side view facing right

**Por que torso humano:** o v3 só gera humanoide; o torso dá ao modelo um esqueleto que ele entende, e é a
Cila das fontes antigas (mulher com cães/serpentes da cintura para baixo).

**Risco registrado:** o top de armadura puxa para a "fantasia genérica"; as serpentes dominam a leitura.

## 3. Posição — medida, não chutada

A Cila **não** desce ao chão como os Lestrigões: ela sai da caverna, acima do estreito. A base foi medida:

- o **vão escuro** da caverna começa em **y ≈ 3,5** (fração de pixels escuros por linha na captura);
- a câmera desta fase tem **tamanho 6** e segue o jogador com **+1 un**: com o Odisseu no convés, o **topo da
  tela fica em y ≈ 5,0**.

A base do placeholder (1,0) punha o topo em 5,46 — **cabeça cortada em jogo** — e o corpo quase todo abaixo
da caverna. Em **y = 0,55** o topo fica em 5,0: cabeça e ombros dentro do vão escuro (azul-violeta sobre
preto, ótimo contraste) e as serpentes descendo pela boca da caverna em direção ao convés.

**Ponto fraco honesto:** como humanoide, o sprite tem **pés**, e eles aparecem abaixo das serpentes — ela
ainda lê um pouco como "pairando na boca da caverna". A solução definitiva seria uma Cila sem pernas saindo
da rocha, ou a caverna mais baixa (cenário). Registrado como pendência.

## 4. Integração

`Assets/Scripts/Editor/ScyllaCastDresser.cs`:
- `Dress` — `BossArtDresser.Vestir` (o mesmo do Polifemo e dos Lestrigões): folha, escala 1, `flipX`, `Head`
  removido, Idle 5 / Telegraph 6 (4 quadros = 0,67 s, dentro do aviso de 0,7 s) / Attack 10, `BossAnimator`
  ligado; ordem −20 (na frente da falésia, −24).
- `Poses` — repouso, aviso e golpe com o Odisseu no convés; sem salvar.

`build-cast-sheets.js` ganhou `pesDoPrimeiro`: a Cila apoia em caudas de serpente que oscilam 1 px entre
quadros; a base passa a ser a do quadro 0. Para quem tem pés, a exigência de linha fixa continua.

Intocados: `BossController`, Caríbdis, `TidalHazard`, `KillZone`, nenhum prefab.

**Efeito colateral corrigido:** na Fase 07 as justificativas `Giant/*` saíram do `PlaceholderProbe`; como o
marcador da Cila também se chama `Giant`, o probe desta fase passou a acusá-lo. Com a Cila vestida, ele volta
a sair limpo.

## 5. Validação

| Teste | Resultado |
|---|---|
| CastProbe | Cila a 42,857 px/un, escala 1, x 3,84..8,16 · y 0,55..5,01 |
| `PlaceholderProbe` | nada visível, nada inativo |
| `CampaignValidation` | passa |
| Compilação | 0 erros |
| Build WebGL | ver relatório |
| Capturas | Cila em repouso, aviso e golpe sobre o Odisseu; o estreito com Caríbdis (inalterada) |

**Não testado:** play mode (ciclo aviso → golpe), navegador, mobile, gamepad. As capturas usam câmera de
tamanho 5; o jogo usa 6 — o enquadramento em jogo é um pouco mais aberto.

## 6. Custo

**20 gerações** (786 → 806 de 2000): master 6 + Idle e Strike 14. Nenhum descarte.

## 7. Pendências (não tocadas)

- **Caríbdis a 0,42** (≈ 2,4× a densidade) — ENVIRONMENT ART.
- **Cila "pairando"**: pés visíveis abaixo das serpentes (sprite humanoide × caverna alta).
- Palácio de Éolo a 0,52 — ENVIRONMENT ART.
- Porco da Circe — tinta rosa.
- `EnemyBasic` carmesim nas fases 01, 14 e 15.
