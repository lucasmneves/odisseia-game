# Fase 06 — Éolo: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Aprovado em 2026-09-29.
Contém o **AEOLUS CHARACTER MASTER v1** e o **AEOLUS WIND BAG MASTER**.

- Master: `Fase06/_master_e_odres.png` · reprovado: `_r1_ilha_desenhada.png` · tira do Idle: `_tiras.png`
- Odre: `WindBag/_odre_antigo_na_cena.png` (antes) · `WindBag/_odre_final_vs_v2.png` (final × versão grande)
- Capturas: `Fase06/_capturas/` (`eolo_com_odisseu`, `odre_com_odisseu`, `eolo_-008`, `eolo_042`)
- Id e prompts: `Docs/Characters/cast.json` (`Aeolus`, reprovado `Aeolus_r1`); args do odre em `WindBag/_args_v*.json`

## 1. CastProbe

| Item | Achado |
|---|---|
| Personagens com figura | **só o Odisseu** |
| Éolo | **não está na cena** |
| Inimigos, NPCs, tripulação, Animator | nenhum |
| Objeto da fase | **`WindBagPickup`** (o saco dos ventos: coletável que dá 3 cargas ao `WindBagAbility` do jogador) |
| Falas | Intro (Odisseu, no passado): *"Éolo me deu os ventos contrários presos num saco… e o vento favorável para nos levar para casa."* · Outro: narrador |

### Por que o Éolo entra mesmo sem estar na cena

O próprio `EoloSceneDresser` (fase de cenário) montou o palácio com **o vão central reservado**:
*"a do meio fica VAZIA: é onde a porta entra, e é onde Éolo, o Odisseu e o diálogo cabem depois"*.
Não é personagem inventado — é uma lacuna prevista, e o personagem-título da fase. Entra do jeito mais
contido possível: **figura de fundo, sem fala nova, sem script, sem colisor** — não altera narrativa nem
gameplay. Estados: **só Idle**, o único que uma figura toca (nenhum código aciona Talk, Cast, GiveItem…).

## 2. Inventário

| Personagem / objeto | Papel | Facção | Asset antes | Estados | Decisão |
|---|---|---|---|---|---|
| Odisseu | Player | Casa de Odisseu | master v1 | 16 | **reutilizado** |
| Éolo | Figura no palácio | Éolo (céu, vento) | nenhum | Idle | **novo — AEOLUS CHARACTER MASTER v1** |
| Saco dos ventos | Coletável | — | `eolo_windbag.png` 144 px encolhido a 0,34 → **3× a densidade do projeto**, corda partida na tela, lia como saco de estopa | — | **recriado — AEOLUS WIND BAG MASTER** |

## 3. AEOLUS CHARACTER MASTER v1

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Characters/NPCs/CHR_Aeolus.png` |
| PixelLab | `7e125e83-2159-411e-b176-5ae3c9583526` · v3 · size 68 |
| Altura | 66 px de corpo = **1,54 un = 1,10× o Odisseu** (célula 1,63 un com o cajado) — "um pouco maior", figura divina |
| PPU | 42,857; escala 1 |
| Paleta | **azul-céu claro** (quíton), **branco** (himátion), prata (cabelo e barba), **ouro discreto** (diadema), bronze (bracelete), madeira clara (cajado). Sem carmesim; **não é o índigo de Micenas** — o azul dele é claro e frio, de céu, não de tinta real |
| Silhueta | manto branco esvoaçando para trás, cabelo e barba soprados de lado, cajado alto com espiral — ninguém mais no jogo tem tecido em movimento permanente |
| Idle | 6 quadros `[3,4,5,6,5,4]` a 5 FPS — **o próprio Idle é o vento**: manto e cabelo esvoaçam o tempo todo, sem partícula nem efeito extra. Quadros 0–2 fora: neles o cajado cruza o rosto |
| Em cena | `AeolusCast/Aeolus`: no **alto dos degraus, diante da porta de bronze** (piso do pórtico, y = −0,45), x = 41,6, junto ao navio de saída (x = 46), **olhando para a esquerda** (de onde o Odisseu chega). Ordem −11: na frente da porta (−15) e das colunas (−13), atrás do mastro (−7) e do jogador (2) |

Prompt aprovado (base `side view, 16-bit pixel art game character, Greek mythology`):

> Aeolus, keeper of the winds and king of Aeolia in Greek myth, tall dignified mature man, long
> silver-white hair and a long silver beard streaming sideways as if blown by the wind, ankle-length pale
> sky-blue chiton, a white woollen himation mantle billowing out behind him, a thin pale gold circlet, a
> bronze arm band, leather sandals, holding a slender white wooden staff topped with a small spiral in his
> front hand, the staff stays attached to his hand, calm powerful expression

**Risco registrado:** barba branca + cajado flerta com o "mago" que o briefing proíbe. O que segura a
leitura grega é o quíton de braços nus, o himátion, o diadema e as sandálias. Aceito.

## 4. AEOLUS WIND BAG MASTER

| | |
|---|---|
| Arquivo | `Assets/Art/Environments/Eolo/Props/eolo_windbag.png` (mesmo caminho e GUID; antigo guardado em `WindBag/_eolo_windbag_antigo_144px.png`) |
| Tamanho | 26 × 26 px = **0,61 un**, densidade nativa, escala 1 |
| Ferramenta | `create_image_pixen`, canvas 32 × 32 |
| Leitura | odre de couro de boi **mole**, gargalo franzido atado com **cordão de prata** de pontas soltas, **espiral de vento azul-céu** pintada — o saco do mito (Odisseia X: odre de couro de boi, cordão de prata), não um saco comum |

Prompt (v3, aprovado):

> isolated game item cut out on a fully transparent background, only the bag of the winds from Greek myth:
> a soft floppy ox-hide leather sack with wrinkled folds and visible stitched seams, bulging and lumpy, its
> neck gathered and bunched and tied tight with a shining silver cord whose ends dangle, a pale sky-blue
> wind spiral painted on the dark brown hide

| Versão | Resultado |
|---|---|
| v1 (56 px, "round and swollen as if full of air") | esfera lisa — **lia como pote de barro** |
| v2 (56 px, couro mole, dobras) | lia como odre, mas **1,28 un — quase a altura do Odisseu** |
| **v3 (32 px, mesmo prompt)** | **0,61 un, tamanho de objeto que se carrega** — aprovado |

O `WindBagPickup` (colisor de 0,5 un, cargas, coleta) **não foi tocado** — só a arte que o veste.
O `EoloSceneDresser` passou a pôr a arte em **escala 1** (antes 0,34) com o motivo escrito, para que
rodar o cenário de novo não volte ao defeito. O odre paira ~0,15 un acima do chão porque o dresser o
apoia na base do colisor de coleta — anterior a este trabalho e comum em coletável; mantido.

## 5. Integração

`Assets/Scripts/Editor/AeolusCastDresser.cs`:
- `Dress` — põe o Éolo pelo piso do pórtico (a posição da `Palace_Door`, não um número digitado), liga o
  Idle e garante o odre em escala 1. Idempotente.
- `Poses` — fotografa o Éolo com o Odisseu ao pé dos degraus, e o odre com o Odisseu ao lado; não salva a cena.

Nenhum prefab envolvido; nenhum override de inimigo (não há inimigo). `BossController`, `WindBagPickup`,
`WindBagAbility`, `WindZone`: intocados.

## 6. Validação

| Teste | Resultado |
|---|---|
| CastProbe | Éolo a 42,857 px/un, escala 1, 1,63 un |
| `PlaceholderProbe` (Éolo) | nada visível, nada inativo |
| `CampaignValidation` | passa |
| Compilação | 0 erros |
| Build WebGL | ver relatório |
| Capturas | Éolo no alto do pórtico diante da porta, legível contra o céu, olhando o Odisseu ao pé dos degraus; odre ao lado do Odisseu, tamanho de objeto, espiral azul legível |

**Não testado:** play mode, navegador, mobile, gamepad.

## 7. Problemas encontrados

| Problema | Situação |
|---|---|
| Éolo ausente com o palácio reservado para ele | **corrigido** — figura no vão do palácio |
| Palavra de cenário no texto-base ("floating island of Aeolia among the clouds") foi **desenhada**: ilha de grama sob os pés e nuvens em volta | **regerado** sem lugar no texto-base (r1 descartado, 2 gerações) |
| Odre com 3× a densidade (144 px a 0,34) | **recriado** em tamanho nativo |
| Odre esférico lia como pote | **regerado** descrevendo couro mole e dobras |
| Odre de 1,28 un quase do tamanho do Odisseu | **regerado** em canvas de 32 px (0,61 un) |
| Cajado cruzando o rosto nos quadros 0–2 do Idle | **cortado** — Idle usa 3–6 |
| Palácio montado com peças encolhidas a 0,52 (mesma classe de defeito de densidade do odre) | **fora do escopo** (cenário) — registrado |

## 8. Custo

**8 gerações** (697 → 705 de 2000): Éolo r1 2 (**descartado**) + r2 2 + Idle 1; odre v1 1 + v2 1
(**descartados**) + v3 1. Três assets descartados.
