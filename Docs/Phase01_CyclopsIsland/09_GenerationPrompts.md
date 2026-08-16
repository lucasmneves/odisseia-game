# Generation Prompts — Ilha dos Ciclopes

> Prompts prontos para artista ou IA geradora. **Consistência vem de sempre colar o
> BLOCO DE ESTILO** (§1) antes de qualquer prompt específico.
>
> Os prompts estão em inglês porque a maioria dos geradores responde melhor nesse idioma.
> As instruções sobre eles estão em português.

---

## 1. BLOCO DE ESTILO — cole antes de TODO prompt

```
STYLE: Stylized 2D digital painting for a side-scrolling platformer game.
Hand-painted look with soft brushwork, bold readable shapes, and full forms.
NOT pixel art. NOT cel-shaded. NOT photorealistic. NOT 3D render.

OUTLINE: soft dark colored outline on the outer silhouette only (2-3px),
using a dark desaturated version of the object's own color. Never pure black.
No internal outlines — separate forms with value and hue instead.

LIGHTING: single warm key light from the UPPER LEFT (low Mediterranean sun).
Cool sky fill light from above. Consistent across every asset.

DETAIL: concentrated on silhouette and face. Interior surfaces simplified.
Must stay readable when scaled to 25%.

PERSPECTIVE: strict orthographic side view. Flat. No vanishing point.

PALETTE: warm Mediterranean — sun-bleached limestone, terracotta, bronze,
olive green, deep sea blue. Cool shadows, never gray-black shadows.

SHADOW: soft one-tone form shadow + a semi-transparent elliptical
contact shadow on the ground.

BACKGROUND: fully transparent (alpha 0). No matte, no halo, no backdrop.

ORIGINALITY: original character and world design for an indie game called
"Odisseia". Do NOT imitate, reference, or reproduce characters, costumes,
poses, logos, UI, or any proprietary element from God of War, Sons of Sparta,
Hades, Assassin's Creed Odyssey, or any existing franchise. Greek mythology
is public domain — specific game franchise designs are not.
```

**Nunca gere um asset sem esse bloco.** É ele que faz peças produzidas em sessões
diferentes parecerem do mesmo jogo.

---

## 2. Fluxo recomendado

1. Gere **`CHR_Odysseus_Idle` primeiro** — é o asset canônico (Art Bible §6)
2. Aprove-o nos três portões (silhueta, 25%, paleta)
3. **Anexe a imagem aprovada como referência visual** em todos os prompts seguintes,
   junto do bloco de estilo. Referência de imagem funciona muito melhor que descrição.
4. Só depois gere o resto

---

## 3. PERSONAGENS

### 3.1 Odisseu — concept sheet (PRIMEIRO ASSET)

```
[BLOCO DE ESTILO]

SUBJECT: Character concept sheet for "Odysseus", the hero of a 2D Greek
mythology adventure platformer.

Three full-body views side by side on a neutral flat background:
FRONT view, SIDE view (facing right), BACK view. Same height, same lighting.

CHARACTER: Greek warrior, adult male, 38-45 years old. Athletic and
functional build — a seasoned soldier, not a bodybuilder. Weathered,
determined, intelligent expression. Approachable heroic, not grim.

PROPORTIONS: 6.5 heads tall. Slightly enlarged head for facial readability.

APPEARANCE:
- Dark brown-black wavy medium-length hair, pushed back, travel-worn
- Short trimmed beard with a few gray strands at the temples
- Sun-tanned warm skin

CLERK:
- Short terracota exomis tunic (#C4643C), one shoulder bare, mid-thigh
- Light boiled-leather chest piece (#7A4B2E) over the tunic
- ONE bronze pauldron on the LEFT shoulder only (#D9A441)
- Bronze forearm bracers
- Wide leather belt with bronze buckle
- Bare legs, laced sandals up the calf
- Short WINE-RED cloak (#8C2F39) pinned at the right shoulder
- Bronze leaf-shaped xiphos short sword in right hand

DO NOT include a shield.

KEY SILHOUETTE ELEMENTS (must be unmistakable in solid black):
the flowing cloak, the single bronze shoulder, the extended sword.
```

### 3.2 Odisseu — sprite base

```
[BLOCO DE ESTILO]
[ANEXAR o concept sheet aprovado como referência]

SUBJECT: Single game sprite of Odysseus in a neutral idle pose,
side view FACING RIGHT, full body, standing on flat ground.

Relaxed confident stance, sword held low at his side, weight settled,
looking forward. Cloak hanging with slight movement.

FRAMING: character occupies about 73% of the canvas height, centered
horizontally, FEET AT THE BOTTOM EDGE of the character area.
Leave headroom above for animation.

OUTPUT: 192x192 pixels, transparent background.
```

### 3.3 Ciclope — concept sheet

```
[BLOCO DE ESTILO]
[ANEXAR o sprite de Odisseu como referência de escala e estilo]

SUBJECT: Boss character concept sheet for a giant one-eyed Cyclops,
shepherd of a wild Greek island.

Two views: FRONT and SIDE (facing right). Include a small silhouette of
a normal-sized human warrior beside him FOR SCALE — the Cyclops must be
exactly 3 times the human's height.

CHARACTER: Enormous, heavy, brutish but not demonic. He is a giant
SHEPHERD who lives here, not a monster from hell. Menacing through sheer
mass and scale.

PROPORTIONS: 4 heads tall (huge head, thick short limbs) — reads as
massive and heavy rather than tall and lean.

APPEARANCE:
- ONE large amber eye (#FFB020) centered on the forehead, vertical pupil.
  This is the visual focal point — the brightest, most saturated element.
- Oversized head, heavy jaw, low protruding brow
- Rocky gray-olive skin (#7C8471) with rough texture and darker patches
- Massive torso, broad chest, a working man's belly
- Very long thick arms reaching below the knees
- Simple stitched goat-hide loincloth (#B8A184)
- One leather strap across the chest
- Rough stone bracelet tied on the right forearm
- Tangled dark hair with bones and small stones braided in
- Barefoot, huge feet
- Carries a shepherd's staff: olive tree trunk with a stone lashed to the tip

TONE: dangerous but not gory. No blood, no visible wounds.
```

### 3.4 Fera da Ilha — inimigo secundário

```
[BLOCO DE ESTILO]
[ANEXAR o sprite de Odisseu como referência]

SUBJECT: Small aggressive quadruped beast — a feral goat-like creature
from a wild Greek island. Minor enemy in a 2D platformer.

SIDE VIEW facing right, standing in a low charging stance.

CHARACTER: Compact goat-like body, head lowered ready to charge,
two curved horns swept forward, bristled fur along the spine,
small red eyes (#D64533), dark solid hooves.

PROPORTIONS: clearly SMALLER than a human warrior — roughly two-thirds
his height, and low to the ground.

DETAIL LEVEL: deliberately SIMPLE. No accessories, no armor, no gear.
Clean silhouette that reads instantly as "aggressive animal".

COLORS: dark brown fur (#6B4A32), lighter brown back (#8E6A4A),
bone-colored horns (#C9B79A).

OUTPUT: 128x128 pixels, transparent background.
```

---

## 4. ANIMAÇÕES

Template — troque as variáveis entre `<>`:

```
[BLOCO DE ESTILO]
[ANEXAR o sprite base do personagem]

SUBJECT: Sprite sheet animation of <PERSONAGEM>, <N> frames in a single
horizontal row, side view facing right.

ANIMATION: <NOME> — <descrição do movimento>

CRITICAL REQUIREMENTS:
- Character stays at the SAME position and SAME scale in every frame
- Feet aligned to the same baseline in every frame (except jump/death)
- Identical style, colors, outline and lighting in every frame
- Each frame separated cleanly, no overlap, no gap
- Transparent background

OUTPUT: <N> frames of <W>x<H> pixels each, single row.
```

### Prompts prontos — Odisseu

| Animação | Descrição do movimento | Frames | Canvas |
|---|---|---|---|
| `Idle` | Subtle breathing, chest rises and falls, cloak sways gently, sword steady at side | 6 | 1152×192 |
| `Run` | Full 2-step run cycle, heroic long stride, cloak streaming behind, sword held back | 8 | 1536×192 |
| `Jump` | 6 poses: crouch, push-off, rising with legs tucked, apex spread, falling with legs reaching, landing crouch | 6 | 1152×192 |
| `Attack` | Horizontal sword slash: 2 anticipation frames pulling back, 1 IMPACT frame with sword fully extended and motion blur, 2 follow-through, 2 return to guard | 7 | 1344×192 |
| `Damage` | Recoil backward, head snapped back, off-balance, recovering | 4 | 768×192 |
| `Death` | Staggers, drops to one knee, falls forward, final pose lying still on the ground | 7 | 1344×192 |

### Prompts prontos — Ciclope (canvas 512×512 por frame)

| Animação | Descrição | Frames |
|---|---|---|
| `Idle` | Heavy breathing, chest expanding, single eye slowly scanning left and right | 6 |
| `Walk` | Slow ponderous steps, whole body rocking with weight, staff planting | 8 |
| `AttackPrepare` | Reacts and locks eye on target, leans back, raises staff overhead, **eye GLOWS brighter (#FFE08A)**, holds at peak tension | 5 |
| `Attack` | Brings staff down hard in an overhead smash, impact at frame 4 | 6 |
| `HeavyAttack` | Raises both arms and slams both fists into the ground, ground-shaking impact | 8 |
| `SpecialAttack` | Picks up a boulder, winds up, hurls it forward | 7 |
| `Damage` | Flinches slightly, head recoils, but does not stagger — he is too massive | 3 |
| `Death` | Staggers, drops the staff, falls to knees, then collapses forward, final pose still on ground | 9 |

### Prompts prontos — Fera (canvas 128×128 por frame)

| Animação | Descrição | Frames |
|---|---|---|
| `Idle` | Breathing, head bobbing slightly, tail flicking | 4 |
| `Run` | Four-legged gallop cycle | 6 |
| `Attack` | Lowers head and lunges forward horns-first, impact at frame 3 | 5 |
| `Damage` | Recoils backward, head jerks up | 3 |
| `Death` | Stumbles, tips over sideways, final pose lying still | 4 |

---

## 5. TILESET

```
[BLOCO DE ESTILO]

SUBJECT: Seamless 2D game tileset for a wild Greek island — "Cyclops Island".
Top-down-free STRICT SIDE VIEW for a side-scrolling platformer.

Produce a tile sheet on a 100x100 pixel grid, each tile fitting the grid exactly
and tiling seamlessly with its neighbors in every direction.

CATEGORY: <CATEGORIA>
<DESCRIÇÃO DA CATEGORIA>

CRITICAL RULE: every WALKABLE SURFACE has a LIGHTER TOP EDGE
(#E8DCC0, 4-6px) so the player can instantly identify what is standable.
Non-collidable decorative tiles must NEVER have this edge.

COLORS: light limestone #C9B99B, mid stone #9A8B72, shadow stone #6B6052,
dry earth #A8794E, sand #E0C9A0, olive green #6E7F4A, dark olive #48562F.

OUTPUT: transparent background, tiles aligned to the 100px grid.
```

Descrições por categoria:

| Categoria | Descrição para o prompt | Tiles |
|---|---|---|
| `Ground` | 3x3 autotile set: corners, edges, center. Earth body with rocky-grassy top | 9 |
| `Platform` | Narrow floating rock ledge, 0.5 units tall: left cap, middle, right cap | 3 |
| `Cliff` | Vertical cliff face: top, body, base, inner corner. Stratified rock layers | 4 |
| `Rock` | Loose boulder blocks in 1x1 and 2x1 sizes, varied irregular shapes | 5 |
| `Cave` | Cave interior: wall, ceiling, stalactite, entrance arch, dark floor, floor edge | 6 |
| `Grass` | Grass tuft overlays for the top of ground tiles, varied shapes, no collision | 4 |
| `Sand` | Beach sand: center, edge, transition to water, wet sand | 4 |
| `Stone` | Worked masonry: dressed floor slab, step, block wall, cracked slab | 4 |
| `AncientRuins` | Ruined Greek architecture: column base, fallen architrave, broken wall, cracked step, fluted column segment, pediment fragment | 6 |
| `Decoration` | Non-collidable overlays: cracks, moss patches, small pebbles, scratches, stains | 8 |

---

## 6. BACKGROUNDS

### BG_CyclopsIsland_01 — Ilha (3 prompts separados)

```
[BLOCO DE ESTILO]

SUBJECT: Parallax BACKGROUND layer for a 2D platformer — "Cyclops Island",
open outdoor coastal area. Wide panoramic side view.

CONTENT: Gradient sky from #7FC4DC at top to warm #F0D9A8 at the horizon.
Low sun on the LEFT. Deep blue sea (#2A6E86) filling the lower third.
Distant hazy islands on the horizon, faded into #B9C4C9 mist.

TREATMENT: LOW saturation (20-35%), low contrast, no outlines, minimal
detail. This layer must sit far BEHIND everything and never compete with
gameplay elements.

CRITICAL: must TILE SEAMLESSLY horizontally — the right edge must connect
perfectly to the left edge.

OUTPUT: 2048x1152 pixels.
```

```
[BLOCO DE ESTILO]

SUBJECT: Parallax MIDGROUND layer, same island, transparent background.

CONTENT: Rocky hillside (#9A8B72) rising from the right. Clusters of olive
trees (#6E7F4A). A single broken Greek column standing on the ridge line —
a narrative promise of ruins ahead.

TREATMENT: MEDIUM saturation (40-55%), slightly darkened, soft outlines only.
Bottom edge must fade out so it blends with the level geometry.

CRITICAL: tiles seamlessly horizontally. Transparent above the horizon.

OUTPUT: 2048x1152 pixels, transparent background.
```

```
[BLOCO DE ESTILO]

SUBJECT: Parallax FOREGROUND layer, mostly transparent.

CONTENT: Dark rocks (#6B6052) framing the lower left and lower right corners.
Olive branches and leaves hanging into frame from the TOP.
Centre must be COMPLETELY EMPTY — gameplay happens there.

TREATMENT: darkened 25%, higher contrast, silhouette-like.

OUTPUT: 2048x1152 pixels, transparent background.
```

### BG_CyclopsIsland_02 — Região do Ciclope

```
[BLOCO DE ESTILO]

SUBJECT: Parallax BACKGROUND layer — the Cyclops' territory.
Enclosed, oppressive, threatening. SAME palette family as the island,
but darker and colder treatment.

CONTENT: Overcast darkened sky (#54606B), sun hidden. Enormous mountain
walls closing in from both sides. A dark cave mouth (#241F1D) in the
distance at the center.

TREATMENT: low saturation, cold shift, heavy atmospheric haze.

CRITICAL: tiles seamlessly horizontally.

OUTPUT: 2048x1152 pixels.
```

```
[BLOCO DE ESTILO]

SUBJECT: Parallax MIDGROUND layer — Cyclops territory, transparent background.

CONTENT: GIANT boulders (each 2-4x the height of a human) establishing the
Cyclops' scale. Greek ruins in worse condition than the island area.
Horizontal bands of DENSE MIST (#B9C4C9). Scattered clean animal bones
from his flock.

TONE: ominous but NOT gory — no blood, no gore, no viscera. This is a
family-friendly game.

CRITICAL: tiles seamlessly horizontally.

OUTPUT: 2048x1152 pixels, transparent background.
```

```
[BLOCO DE ESTILO]

SUBJECT: Parallax FOREGROUND layer — cave framing, mostly transparent.

CONTENT: Stalactites descending from the top edge. Dark jagged rocks
framing both sides, creating a tunnel-like vignette. Centre COMPLETELY EMPTY.

TREATMENT: darkened 40%, near-silhouette.

OUTPUT: 2048x1152 pixels, transparent background.
```

---

## 7. PROPS

Template:

```
[BLOCO DE ESTILO]

SUBJECT: Single 2D game prop for a Greek island platformer — <NOME>.
STRICT SIDE VIEW, orthographic, transparent background.

DESCRIPTION: <DESCRIÇÃO>

SCALE REFERENCE: the game's hero is 1.4 world units tall. This prop is
<ALTURA> units tall — <COMPARAÇÃO>.

<Se colidível:> This prop is STANDABLE — give it a lighter top edge (#E8DCC0).
<Se não colidível:> This prop is DECORATIVE — do NOT give it a lighter top edge.

Include a soft elliptical contact shadow at the base.

OUTPUT: <W>x<H> pixels.
```

| Prop | Descrição | Un | Comparação | Colisão | Canvas |
|---|---|---|---|---|---|
| `PROP_GreekColumn_Broken` | Fluted Doric column snapped at two-thirds height, weathered limestone, cracks, moss at the base | 0,8×2,4 | quase 2× a altura do herói | Sim | 128×256 |
| `PROP_Rock_Large` | Rounded weathered boulder, layered stone, lichen patches | 1,6×1,4 | do tamanho do herói | Sim | 192×160 |
| `PROP_OliveTree` | Gnarled olive tree, twisted trunk, silver-green canopy | 2,2×2,8 | 2× a altura do herói | Não | 256×320 |
| `PROP_Bush` | Low dense mediterranean shrub, small stiff leaves | 0,9×0,6 | na altura do joelho | Não | 128×96 |
| `PROP_Torch` | Wall torch: iron bracket, wooden handle, wrapped cloth head (unlit — flame is separate VFX) | 0,3×0,9 | 2/3 da altura do herói | Não | 64×128 |
| `PROP_GreekVase` | Terracotta amphora with simple black geometric meander pattern, two handles | 0,5×0,7 | na altura da cintura | Não | 64×96 |
| `PROP_WoodenCrate` | Simple wooden crate with iron corner brackets, weathered planks | 0,7×0,7 | na altura da cintura | Sim | 96×96 |
| `PROP_StoneStructure` | Low ruined stone platform, dressed blocks, partially collapsed, flat top | 2,0×1,2 | mais largo que o herói | Sim | 224×160 |
| `PROP_Altar` | Small Greek offering altar, carved stone, shallow bowl on top, faint scorch marks | 1,4×1,0 | na altura do peito | Sim | 160×128 |
| `PROP_CyclopsBasket` | ENORMOUS crude woven basket, thick rough branches, oversized for a giant's hands | 1,2×1,0 | tão alto quanto o torso do herói — pertence a um gigante | Não | 160×128 |

---

## 8. VFX

```
[BLOCO DE ESTILO]

SUBJECT: 2D game VFX pieces — <NOME>.

IMPORTANT: this is NOT an animation sheet. Produce <N> SEPARATE small
individual pieces (shards / sparks / puffs) that a game engine will scatter
and fade programmatically.

DESCRIPTION: <DESCRIÇÃO>
COLORS: <CORES>

Each piece: simple, bold, readable at small size, transparent background.

OUTPUT: <N> pieces, each roughly <TAM> pixels, on a transparent sheet.
```

| VFX | Descrição | Cores | Peças |
|---|---|---|---|
| `VFX_DustRun` | Soft irregular dust puffs, no hard outline | `#C9B99B` → `#E0C9A0` | 3 |
| `VFX_DustLand` | Larger dust puffs, more energetic | idem | 3 |
| `VFX_SwordImpact` | Sharp angular impact shards + a thin curved slash arc | núcleo `#FFFFFF`, borda `#F5D98A` | 4 |
| `VFX_HitReceived` | Small elongated sparks radiating outward | `#D95A3C` + `#FFD54A` | 4 |
| `VFX_StoneParticles` | Irregular angular rock chips of varied size | `#9A8B72`, `#6B6052` | 5 |
| `VFX_Fire` | 4-frame looping flame silhouette, 3 tones | `#FFD54A` → `#E8752F` → `#B33A1E` | 4 |
| `VFX_Smoke` | Soft translucent smoke wisps, no outline | `#B9C4C9` | 3 |
| `VFX_CyclopsSlam` | 5-frame expanding flattened shockwave ring, hollow centre | `#C9B99B` + `#FFB020` | 5 |
| `VFX_Collect` | Small bright star/sparkle shapes | `#FFD54A` + `#FFFFFF` | 4 |

---

## 9. ÁUDIO

Para ferramentas de geração musical:

### MUS_CyclopsIsland_Main

```
Original instrumental game soundtrack. Ancient Greek adventure exploration
theme. Dorian mode. 96 BPM. 2 minutes 45 seconds, seamless loop.

INSTRUMENTS: lyre and kithara (plucked), aulos (double flute), bone flute,
frame drum (tympanon), light hand percussion, sustained string section.

MOOD: adventurous, spacious, sunlit, a sense of arriving somewhere ancient
and unknown. Warm and inviting with an undercurrent of unease.

STRUCTURE: sparse solo lyre opening → percussion and flute enter with the
main theme → darker middle section with sustained strings → return to the
main theme with fuller arrangement.

DYNAMICS: moderate. Leave headroom — this sits UNDER gameplay audio.

Must be an ORIGINAL composition. Do not reproduce or imitate any existing
game or film soundtrack.
```

### MUS_CyclopsIsland_Boss

```
Original instrumental game soundtrack. Ancient Greek boss battle.
Phrygian mode. 126 BPM. 2 minutes 15 seconds, seamless loop.

INSTRUMENTS: heavy war drums, deep frame drums, low string ostinato,
krotala (clappers), wordless male choir on open vowels, low brass-like drone.

MOOD: dangerous, oppressive, building dread. A giant is coming.

SIGNATURE ELEMENT: a deep bass drum hit at IRREGULAR intervals (every 3 or
5 beats, never every 4) — an unsettling limping-giant pulse.

STRUCTURE: lone bass drum intro (plays once) → full percussion confrontation
→ layered escalation with choir → sustained peak.

Must be an ORIGINAL composition.
```

### SFX — template

```
Game sound effect, mono, dry (no reverb tail), <DURAÇÃO>.
<DESCRIÇÃO>
Clean, punchy, no clipping. Leave headroom — peak around <NÍVEL> dBFS.
```

Lista completa de descrições e níveis em `05_AudioBible.md` §3.

---

## 10. Checklist antes de aceitar qualquer asset gerado

- [ ] Bloco de estilo foi incluído no prompt
- [ ] Fundo realmente transparente (sem halo branco)
- [ ] Contorno colorido escuro, **não preto puro**
- [ ] Luz vindo de cima-esquerda
- [ ] Todas as cores conferidas com conta-gotas contra a paleta
- [ ] Escala confere com a tabela do Character Bible
- [ ] **Portão de silhueta**: reconhecível em preto sólido
- [ ] **Portão 25%**: legível reduzido em escala de cinza
- [ ] Sem nenhum elemento reconhecível de franquia existente
- [ ] Nome conforme `06_NamingConvention.md`
