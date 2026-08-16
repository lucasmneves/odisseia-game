# Asset Naming Convention — Odisseia

Padrão único para todo o projeto, não só para esta fase.

---

## 1. Fórmula

```
PREFIXO_Assunto_Variante[_Índice].extensão
```

- **PascalCase** em cada segmento
- **Underscore** separa segmentos
- Sem espaços, sem acentos, sem hífen
- Índice com 2 dígitos quando houver sequência (`_01`, `_02`)

---

## 2. Prefixos

| Prefixo | Categoria | Exemplo |
|---|---|---|
| `CHR_` | Personagem (sprite/animação) | `CHR_Odysseus_Idle` |
| `ENV_` | Tile de ambiente | `ENV_CyclopsIsland_Ground_01` |
| `BG_` | Camada de background | `BG_CyclopsIsland_01_Sky` |
| `PROP_` | Objeto de cenário | `PROP_GreekColumn_Broken` |
| `VFX_` | Efeito visual | `VFX_SwordImpact_01` |
| `UI_` | Interface | `UI_HealthIcon` |
| `SFX_` | Efeito sonoro | `SFX_Player_Jump` |
| `MUS_` | Música | `MUS_CyclopsIsland_Main` |
| `ATL_` | Sprite Atlas | `ATL_Odysseus` |
| `MAT_` | Material | `MAT_SpriteDefault` |
| `ANIM_` | AnimationClip (Unity) | `ANIM_Odysseus_Run` |
| `AC_` | Animator Controller | `AC_Odysseus` |
| `PF_` | Prefab | `PF_Odysseus` |

---

## 3. Nomes canônicos desta fase

### Personagens

```
CHR_Odysseus_Idle          CHR_Cyclops_Idle
CHR_Odysseus_Run           CHR_Cyclops_Walk
CHR_Odysseus_Jump          CHR_Cyclops_AttackPrepare
CHR_Odysseus_Attack        CHR_Cyclops_Attack
CHR_Odysseus_Damage        CHR_Cyclops_HeavyAttack
CHR_Odysseus_Death         CHR_Cyclops_SpecialAttack
                           CHR_Cyclops_Damage
CHR_Beast_Idle             CHR_Cyclops_Death
CHR_Beast_Run
CHR_Beast_Attack
CHR_Beast_Damage
CHR_Beast_Death
```

### Ambiente — tiles

Formato: `ENV_CyclopsIsland_<Categoria>_<Índice>`

```
ENV_CyclopsIsland_Ground_01 .. _09
ENV_CyclopsIsland_Platform_01 .. _03
ENV_CyclopsIsland_Cliff_01 .. _04
ENV_CyclopsIsland_Rock_01 .. _05
ENV_CyclopsIsland_Cave_01 .. _06
ENV_CyclopsIsland_Grass_01 .. _04
ENV_CyclopsIsland_Sand_01 .. _04
ENV_CyclopsIsland_Stone_01 .. _04
ENV_CyclopsIsland_AncientRuins_01 .. _06
ENV_CyclopsIsland_Decoration_01 .. _08
```

### Backgrounds

Formato: `BG_CyclopsIsland_<NN>_<Camada>`

```
BG_CyclopsIsland_01_Background
BG_CyclopsIsland_01_Midground
BG_CyclopsIsland_01_Foreground
BG_CyclopsIsland_02_Background
BG_CyclopsIsland_02_Midground
BG_CyclopsIsland_02_Foreground
```

### Props

```
PROP_GreekColumn_Broken    PROP_GreekVase
PROP_Rock_Large            PROP_WoodenCrate
PROP_OliveTree             PROP_StoneStructure
PROP_Bush                  PROP_Altar
PROP_Torch                 PROP_CyclopsBasket
```

### VFX

```
VFX_DustRun_01 .. _03      VFX_Fire_01 .. _04
VFX_DustLand_01 .. _03     VFX_Smoke_01 .. _03
VFX_SwordImpact_01 .. _04  VFX_CyclopsSlam_01 .. _05
VFX_HitReceived_01 .. _04  VFX_Collect_01 .. _04
VFX_StoneParticles_01 .. _05
```

### Áudio

```
MUS_CyclopsIsland_Main       SFX_Cyclops_Roar
MUS_CyclopsIsland_Boss       SFX_Cyclops_Attack
                             SFX_Cyclops_AttackHeavy
SFX_Player_Jump              SFX_Cyclops_Hit
SFX_Player_Land              SFX_Cyclops_Damage
SFX_Player_Attack            SFX_Cyclops_Death
SFX_Player_AttackHit         SFX_Cyclops_Footstep
SFX_Player_Damage
SFX_Player_Death             SFX_Ambience_Wind
SFX_Player_Step_01 .. _04    SFX_Ambience_Birds
                             SFX_Ambience_Waves
SFX_Enemy_Attack             SFX_Ambience_Water
SFX_Enemy_Hit                SFX_Ambience_Cave
SFX_Enemy_Damage             SFX_Ambience_Fire
SFX_Enemy_Death              SFX_RockFall
```

---

## 4. Relação com a nomenclatura pedida no briefing

O briefing lista nomes como `CHR_Odysseus_Idle`, `PROP_GreekColumn`, `BG_CyclopsIsland_01`.
Este documento os adota integralmente, com duas extensões:

1. **Índice em conjuntos** (`_01`, `_02`) — necessário porque tiles e VFX têm variantes.
2. **Sufixo de camada nos BGs** (`_Background`, `_Midground`, `_Foreground`) — o briefing
   pede parallax de 3 camadas, então `BG_CyclopsIsland_01` sozinho seria ambíguo.

`PROP_GreekColumn` virou `PROP_GreekColumn_Broken` porque a especificação (doc `02` §4)
define a coluna como quebrada — o nome deve descrever o asset real.

---

## 5. Regras de manutenção

1. **O nome do arquivo é a fonte de verdade.** Renomear um asset no Unity quebra
   referências — se precisar renomear, faça pelo Editor (que atualiza os `.meta`), nunca
   pelo explorador de arquivos.
2. **Nada de `_final`, `_new`, `_v2`, `_copy`.** Versionamento é do Git.
3. **Nada de acento ou `ç`.** Alguns pipelines de build (e o WebGL) tratam mal caracteres
   não-ASCII em caminhos.
4. **O sprite sheet e seus clipes compartilham o assunto:** `CHR_Odysseus_Run.png` gera
   `ANIM_Odysseus_Run.anim`.
