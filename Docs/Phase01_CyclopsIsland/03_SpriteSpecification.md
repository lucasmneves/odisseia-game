# Sprite Specification — Ilha dos Ciclopes

> **PPU = 100** para todos os assets. Justificativa completa em `00_ArtBible.md` §5.

---

## 1. Tamanhos de canvas

O canvas é **maior que o personagem** para acomodar movimento de animação (espada erguida,
manto voando, salto) sem cortar. O personagem não preenche o canvas.

| Asset | Personagem (px) | **Canvas (px)** | Folga | Por quê |
|---|---|---|---|---|
| Odisseu | 60 × 140 | **192 × 192** | Generosa | Espada erguida + manto + pulo |
| Fera | 110 × 90 | **128 × 128** | Modesta | Movimento pequeno |
| **Ciclope** | 220 × 420 | **512 × 512** | Grande | Cajado erguido (3,5 un) + braços longos |
| Tile | 100 × 100 | **100 × 100** | Nenhuma | Grade exata — folga quebraria o tiling |
| Props | variável | Múltiplo de 32, ajustado | Mínima | Recorte justo |
| VFX | variável | **128 × 128** | — | Padrão único simplifica o pipeline |
| Background | — | **2048 × 1152** | — | Ver `02_EnvironmentBible.md` §3 |

**Canvas quadrado e potência de dois** (192 é 64×3, aceitável) simplifica empacotamento em
atlas e evita bugs de pivot.

---

## 2. Pivots

| Asset | Pivot | Coordenada normalizada | Motivo |
|---|---|---|---|
| Odisseu | **Base, centro** | (0,5 ; 0,0) | Alinha com o pé do `CapsuleCollider2D`; o personagem "pisa" no y do transform |
| Fera | Base, centro | (0,5 ; 0,0) | Idem |
| Ciclope | Base, centro | (0,5 ; 0,0) | Idem |
| Tiles | Canto inferior-esquerdo | (0,0 ; 0,0) | Padrão de Tilemap do Unity |
| Props | Base, centro | (0,5 ; 0,0) | Assentam no chão sem ajuste manual |
| VFX | Centro | (0,5 ; 0,5) | Explodem a partir do ponto de origem |
| Backgrounds | Centro | (0,5 ; 0,5) | Facilita centralizar na câmera |

**Regra crítica:** o pivot precisa ser **idêntico em todos os frames** de uma animação. Um
pivot deslocado num frame faz o personagem "tremer" — bug clássico e difícil de rastrear
depois.

**Consistência com o collider existente:** o `CapsuleCollider2D` do `Player.prefab` tem
`offset = (0 ; 0,7)` e `size = (0,6 ; 1,4)` — ou seja, o collider vai de y=0 a y=1,4 com o
transform na base. Um pivot na base faz o sprite alinhar exatamente, sem offset extra.

---

## 3. Orientação

- **Todos os personagens são desenhados voltados para a DIREITA.**
- A virada para a esquerda é feita em runtime invertendo `localScale.x` do transform
  `Visual` — o `PlayerController` **já faz isso** (método `UpdateFacing`).
- **Não produza frames espelhados.** Dobraria o custo de textura sem ganho.
- Consequência de design: elementos assimétricos (o ombro de bronze de Odisseu fica no lado
  esquerdo) vão trocar de lado ao virar. É aceitável e imperceptível em movimento.

---

## 4. Sprite sheets

### Formato

- **Uma folha por animação**, frames em **linha horizontal única**
- Sem espaçamento entre frames (o atlas cuida do padding)
- PNG-24 com canal alfa, sem entrelaçamento
- Fundo totalmente transparente (alfa 0), **sem matte branco ou preto** nas bordas

### Exemplo de layout

```
CHR_Odysseus_Run.png  →  1536 × 192 px  (8 frames × 192)
[f01][f02][f03][f04][f05][f06][f07][f08]
```

### Fatiamento no Unity

`Sprite Mode: Multiple` → `Sprite Editor` → `Slice: Grid By Cell Size` com a célula igual
ao canvas (ex.: 192 × 192). Pivot definido no fatiamento, não por frame.

---

## 5. Orçamento de textura — WebGL

> Esta é a restrição que mais influencia o número de frames. O build atual do jogo tem
> **5,8 MB no total**; o art pass não pode multiplicar isso por dez.

### Contagem de pixels por personagem

| Personagem | Canvas | Frames (doc `04`) | Pixels brutos |
|---|---|---|---|
| Odisseu | 192² | 38 | 1,40 M |
| Fera | 128² | 22 | 0,36 M |
| Ciclope | 512² | 46 | **12,06 M** |

**O Ciclope sozinho consome 8× mais pixels que Odisseu.** É o gargalo real e precisa de
tratamento específico.

### Estratégias obrigatórias

1. **Tight packing com trim** no Sprite Atlas do Unity. A maioria dos frames tem 40–60% de
   área transparente (especialmente o Ciclope, cujo canvas 512² acomoda o cajado erguido
   mas fica vazio na maior parte dos frames). O trim recorta isso.
   → Redução realista: **~45%**

2. **Crunch compression** na importação (qualidade 50). É compressão com perda mas o estilo
   pintado/estilizado esconde os artefatos muito melhor que pixel art.
   → Redução adicional: **~70%** sobre o já comprimido

3. **Frame counts enxutos** — ver `04_AnimationSpecification.md`. A direção pede *leitura
   clara*, não animação fluida de 24 fps. 8–10 fps com poses fortes lê melhor em sprite
   pequeno e custa metade.

### Orçamento estimado final

| Atlas | Conteúdo | Dimensão | Estimativa em disco |
|---|---|---|---|
| `ATL_Odysseus` | 38 frames trimados | 2048² | ~0,7 MB |
| `ATL_Cyclops` | 46 frames trimados | 2048² × 2 | ~1,5 MB |
| `ATL_SecondaryEnemy` | 22 frames | 1024² | ~0,2 MB |
| `ATL_Environment` | 53 tiles + 10 props | 1024² | ~0,3 MB |
| `ATL_VFX` | ~30 frames | 1024² | ~0,2 MB |
| `BG_01` (3 camadas) | 2048 × 1152 × 3 | — | ~0,9 MB |
| `BG_02` (3 camadas) | 2048 × 1152 × 3 | — | ~0,9 MB |
| **Total estimado** | | | **~4,7 MB** |

Somado ao build atual (5,8 MB), chega a **~10,5 MB**. É aceitável para WebGL — carrega em
poucos segundos numa conexão razoável — mas é o **teto**. Se passar disso, as ações são,
nesta ordem: (1) cortar frames do Ciclope, (2) reduzir BGs para 1600 × 900, (3) baixar a
qualidade do Crunch para 40.

**Estas estimativas não foram medidas** — são projeções a partir das dimensões. Meça o
build real depois do primeiro atlas integrado e ajuste.

---

## 6. Import settings — Unity

Aplicar a **todos** os sprites de personagem, prop e VFX:

| Campo | Valor | Motivo |
|---|---|---|
| Texture Type | `Sprite (2D and UI)` | |
| Sprite Mode | `Multiple` (sheets) / `Single` (estáticos) | |
| **Pixels Per Unit** | **100** | Art Bible §5 |
| Mesh Type | `Tight` | Menos overdraw |
| Extrude Edges | 1 | Evita costura no atlas |
| Filter Mode | **`Bilinear`** | Estilo pintado, não pixel art. `Point` serrilharia |
| Compression | `Normal Quality` + **Crunch 50** | |
| Max Size | 2048 | |
| Generate Mip Maps | **Off** | Câmera ortográfica fixa — mipmaps só desperdiçariam 33% de memória |
| Wrap Mode | `Clamp` | |

**Exceção — Tiles:**

| Campo | Valor | Motivo |
|---|---|---|
| Filter Mode | `Bilinear` | Consistência |
| Extrude Edges | **2** | Tiles precisam de margem maior contra sangramento de borda |
| Mesh Type | `Full Rect` | Obrigatório para Tilemap |

**Exceção — Backgrounds:**

| Campo | Valor |
|---|---|
| Wrap Mode | **`Repeat`** (obrigatório para o ladrilhamento horizontal) |
| Max Size | 2048 |
| Compression | Normal + Crunch 50 |

---

## 7. Checklist por asset entregue

Antes de considerar um sprite pronto:

- [ ] Canvas no tamanho especificado
- [ ] Pivot correto e **idêntico em todos os frames**
- [ ] Voltado para a direita
- [ ] Fundo transparente sem matte
- [ ] Todas as cores conferidas contra a paleta (`02` §1)
- [ ] Contorno na cor definida (nunca preto puro)
- [ ] Luz superior-esquerda
- [ ] Sombra projetada presente (personagens e props)
- [ ] **Portão de silhueta**: identificável em preto sólido
- [ ] **Portão 25%**: legível reduzido e em escala de cinza
- [ ] Nomenclatura conforme `06_NamingConvention.md`
