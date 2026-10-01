# PixelLab Production Sprint — plano (2026-10-01)

Créditos do ciclo expiram em **2026-10-02**. Este plano diz **o que gerar, em que ordem, por quê e como integrar**,
e vem com os lotes prontos em `lotes/` e um executor que não cobra duas vezes (`Tools/sprint-run.js`).

> **Nenhuma geração foi feita nesta sessão.** A sessão na nuvem não alcança o PixelLab: `api.pixellab.ai` é negado
> pela política de rede do ambiente, e a credencial vive no `~/.claude.json` da máquina local. Ver §7.

---

## 1. Diagnóstico — o que o projeto já tem e o que falta de verdade

Fonte: `ESTADO_ATUAL.md`, `QA/FULL_CAMPAIGN_PLAYTEST.md`, `Characters/FINAL_CHARACTER_ART_POLISH.md`, dressers de cena.

**Já resolvido (não gerar de novo):** 15 das 16 fases têm cenário completo (paleta, parallax, tiles, props), elenco em
pixel art v3 a 42,857 px/un e `PlaceholderProbe` limpo, exceto Troia. Odisseu tem 16 estados / 99 quadros.

| Lacuna | Onde | Evidência | Precisa de PixelLab? |
|---|---|---|---|
| **Fase 16 sem cenário** — só `Floor_1/2`, `Sky_Background` e 4 `AxeRing` | 16 | cena; não existe `Environment_Final` | **sim** — props-assinatura (machados, arco), lareira, tear, camadas |
| **VFX de gameplay são quadradinhos coloridos** | todas | `VfxBurst` espalha quadrados de cor sólida | **sim** — `Spawn` já recebe `Sprite`, troca direta |
| HUD só texto (QA-15) | todas | QA | sim — ícones 32 px |
| Nós do mapa são quadrados escuros (QA-16) | mapa | QA | sim — fundo + 16 emblemas |
| Fogo e tochas estáticos | 01, 02, 14, 15, 16 | `ESTADO_ATUAL` (Fase 15) | chama-chave sim; o loop sai por código |
| Inimigos sem animação de ataque | 02, 03, 09, 15, 01 | `EnemyAnimator`: só Idle/Run/Hit/Death | sim — 1 geração por facção (+ ~20 linhas de código) |
| Defesa no ar sem visual (QA-14) | todas | QA | sim — JumpShield / FallShield |
| Primeiro plano ausente | 04, 06, 14, 15 | dressers sem FG | sim — 2 peças por fase |
| Entablamento de Éolo esticado 2× (N-06) | 06 | FINAL POLISH | sim — 1 peça nativa |
| Troia com 10 placeholders (N-08) | 02 | `PlaceholderProbe` | **quase não** — tiles, muralha e barricada já existem; faltam ponte e bloco caído |
| Porco da Circe (P-04) | 08 | FINAL POLISH | adiado: exige código no efeito + `pro` (o v3 recusa quadrúpede) |

**Conclusão:** as lacunas reais custam **~100 gerações** com candidatos para escolha. O saldo (~800) é muito maior que
isso. A regra do briefing — não gerar para consumir crédito — vale: o excedente só entra no §5 (P3), comprando
**qualidade** (mais candidatos nos assets de maior visibilidade), nunca volume.

---

## 2. Regras de estilo que valem para todo lote (herdadas, não inventadas)

- **Densidade nativa 1×**: 42,857 px = 1 un; Odisseu 60 px. Rodar `Tools/scale-probe.js` em todo asset novo.
- **Paleta da fase**: 16 é a casa de Ítaca → paleta `PRETENDENTES_PALETTE` (as 9 rampas de Ítaca + Bronze, Fogo,
  Vinho, Noite). Regra da 15 mantida: *sombra azul-noite; só fogo, vinho e comida quentes*. A 16 muda a LUZ (aurora),
  não a paleta.
- Prompts: enquadramento **positivo** ("Isolated game asset cut out on a fully transparent background, only the X");
  nunca "margem vazia" (o modelo pinta o xadrez); nunca exclusões; objetos descritos "pequenos o bastante para estar
  completos".
- `pixen` para tudo (1 geração, até 768 px). `pro` só abaixo de 170 px e só no P3.
- Depois de baixar: `checker-cut.js` / `cutout.js` → quantizar na paleta (conferir antes se a paleta cobre o material)
  → `scale-probe.js` → `seam-test.js` nas faixas → `unity-import.js` → `verify-meta.js`.

---

## 3. Orçamento

Saldo informado ≈ 40% (~800). Último registro no repositório: 845 usadas / 1155 restantes (2026-09-30).

| Prioridade | Conteúdo | Gerações | Como |
|---|---|---|---|
| **P0** | Fase 16 (22), VFX de gameplay (16), Troia (4) | **42** | `sprint-run.js --only P0` |
| **P1** | HUD (14), mapa (21), chamas-chave (6) | **41** | `--only P1` |
| **P1-chr** | ataque de 5 facções, JumpShield/FallShield, reconhecimento da Penélope | **~8** | `build-cast.js` (§4.3) |
| **P2** | primeiro plano 04/06/14/15 (8), entablamento de Éolo (2) | **10** | `--only P2` |
| **P3** | `pro` (4 candidatos) só onde os candidatos `pixen` reprovarem | **0–240** | manual, após revisão |
| | **Total planejado** | **~100 (+ até 240)** | |

O executor ordena P0 → P2 e `--max N` corta pelo fim da fila.

---

## 4. Pipeline por fase

### 4.1 Fase 16 — Final (o salão do arco)

| Etapa | Decisão | Custo |
|---|---|---|
| Environment Master | **Reuso da 15**: mesmo salão, mesma paleta; luz de aurora em vez de noite. Escrever `FINAL_MASTER_REFERENCE.md` com a captura aprovada | 0 |
| Tileset | piso de laje da 14/15 (reuso; bloco de terra até −1,9 por causa da falha dos tiles de Ítaca) | 0 |
| Platforms | `Floor_1/2` vestidos com o piso; nenhuma plataforma nova na cena | 0 |
| Props | **`p0_a`**: machado no poste (3 cand.) — veste os 4 `AxeRing`; fileira de 12 (2); arco de Odisseu no suporte (3). **`p0_b`**: lareira, tear, escudos no chão, facho de aurora. Reuso: colunas, trono, mesas, braseiros (15) e arsenal (01) | 16 |
| Background | **`p0_c`**: parede do mégaron ao amanhecer (faixa 512, `mirrorDouble` se não ladrilhar); céu da 14 clareado | 2 |
| Foreground | **`p0_c`**: vigas com lâmpadas no topo da tela; coluna em silhueta | 4 |
| Characters | Penélope, Telêmaco adulto, Eumeu: reuso (`cast.json`). Penélope: 1 animação de reconhecimento (§4.3) | 1 |
| Enemies | nenhum na cena | 0 |
| VFX | `vfx_collect_sparkle` no acerto do anel; flecha já tem arte própria | (P0 VFX) |
| Unity | `FinalSceneDresser` novo, no molde do `PretendentesSceneDresser` (contar origem: 15 / 01 / nova) | — |
| QA | `PlaceholderProbe` (Sky_Fill com justificativa medida), `CampaignValidation`, captura sem `-nographics`, contagem de `#ff00ff` | — |

### 4.2 Fase 02 — Troia (N-08)

| Placeholder | Solução | Custo |
|---|---|---|
| `Platform_Bridge` | `troy_plank_bridge` (`p0_f`) | 2 |
| `Gauntlet_1..3` | `troy_fallen_block_platform` (`p0_f`), um por degrau, com variação por espelho | 2 |
| `Wall_Troia` | `troy_wall_section_01` existente | 0 |
| `Obstacle_Low` | `troy_barricade_01` existente (desenho na altura do colisor — regra do portão de Ítaca) | 0 |
| mastro/vela do objetivo | casco de `ithaca_ship_01` + mastro/vela por código (`build-ship.js`) | 0 |
| `Sky_Fill` | justificativa com a cor medida da 1ª linha de `troy_bg_sky` no `PlaceholderProbe` | 0 |

### 4.3 Personagens e inimigos (via `build-cast.js`, não pelo executor de imagem)

`animate_character` v3, `directions:['east']`, 1 geração cada, receitas do master (movimento amplo:
`keep_first_frame:false` + trava leve).

| Personagem (`cast.json`) | Estado | Prompt-base | Fase |
|---|---|---|---|
| Trojan Soldier | Attack | *thrusting the spear forward hard at chest height, front foot stepping in, the spear stays attached to his hand in every frame* | 02 |
| Cicones Warrior | Attack | idem, com a arma da facção | 03 |
| Shade Warrior | Attack | idem | 09 |
| Suitor (açafrão) | Attack | *swinging a short sword in a wide arc …, the sword stays attached to his hand in every frame* | 15 |
| Ithaca Sparring | Attack | idem à lança | 01 |
| Odisseu (master) | JumpShield, FallShield | *jumping/falling with the round shield raised in front, the shield stays attached to his arm in every frame* | todas |
| Penélope | Recognition | *raising both hands to her mouth in surprise, then stepping forward* (Receita A, âncora) | 16 |

Código necessário (0 gerações): `EnemyAnimator` ganha o estado `Attack`, tocado quando `EnemyController` aplica dano
(hoje linha 170); estado ausente é ignorado pelo `SpriteAnimator`, então facções sem a folha nova não quebram.
`PlayerAnimator` usa `JumpShield`/`FallShield` quando `IsBlocking` no ar.

### 4.4 Fases 01–15 — o que entra

| Fase | Entra | Lote |
|---|---|---|
| 01, 14, 15 | chamas animadas nas tochas/braseiros | `p1_f` + código |
| 02 | N-08 (§4.2), chama da fogueira | `p0_f`, `p1_f` |
| 04 Citera | primeiro plano: cordame, crista de onda | `p2_a` |
| 06 Éolo | primeiro plano (urna, nuvem) + entablamento nativo (N-06) | `p2_a`, `p2_b` |
| 14 Ítaca Return | primeiro plano: galho de oliveira, capim | `p2_a` |
| 15 Pretendentes | primeiro plano: estandarte rasgado, cratera tombada | `p2_a` |
| todas | VFX de golpe/defesa/morte/poeira/respingo/coleta | `p0_d`, `p0_e` |
| 03, 05, 07–13 | nada novo além de VFX/HUD — já estão completas | — |

### 4.5 Interface

- **HUD** (`p1_a`, `p1_b`): coração, flecha, dracma, elmo (vidas), louro (XP), escudo, moldura com meandro.
  Resolve QA-15 junto com rótulos; a fonte com travessão (QA-10) é trabalho de código.
- **Mapa** (`p1_c`–`p1_e`): carta do Egeu em pergaminho, navio-marcador, nó bloqueado/concluído, 16 emblemas.

---

## 5. P3 — onde o excedente compra qualidade

Só depois de revisar os candidatos `pixen`. Para os assets abaixo de 170 px em que **nenhum** candidato passar
(silhueta, paleta, densidade), uma chamada `create_image_pro` (4 candidatos, 20–40) com `style_image` de um asset
aprovado **da mesma fase** (nunca uma cena: o estilo vaza conteúdo). Candidatos naturais: machado no poste, arco de
Odisseu, conjunto de ícones do HUD, emblemas do mapa que falharem. Teto: 6 chamadas ≈ 240.

O que sobrar depois disso **fica sem gastar**. Gerar sem lacuna para preencher produz assets que ninguém integra.

---

## 6. Integração e QA (por lote, depois de gerar)

1. Escolher o candidato (`_a/_b/_c`) e renomear sem sufixo; os outros ficam em `_fontes/` como histórico.
2. `node Tools/checker-cut.js` se o executor acusar xadrez pintado; senão `Tools/cutout.js`.
3. Quantizar na paleta da fase (script `quantize-*` / `build-*` da fase; destino de ramo ambíguo declarado por asset).
4. `Tools/scale-probe.js` (densidade) e, nas faixas, `Tools/seam-test.js` (+ `mirrorDouble` se precisar).
5. `node Tools/unity-import.js` → `node Tools/verify-meta.js` (0 falhas).
6. Dresser da fase (Editor fechado para batchmode) → `PlaceholderProbe`, `CampaignValidation`, captura, contagem de
   magenta, `GlobalCastAudit` para personagens.
7. Registrar no master da fase e no `ESTADO_ATUAL.md` (gerações antes → depois).

---

## 7. Como rodar

```bash
node Tools/sprint-run.js Docs/PixelLabSprint/lotes --dry          # o que falta e quanto custa (grátis)
node Tools/sprint-run.js Docs/PixelLabSprint/lotes --ping         # testa a conexão (grátis)
node Tools/sprint-run.js Docs/PixelLabSprint/lotes --only P0      # gera o P0
node Tools/sprint-run.js Docs/PixelLabSprint/lotes --max 100      # tudo até 100 gerações
```

- **Na máquina local** (Windows, MCP registrado): funciona como está.
- **Numa sessão na nuvem**: liberar `api.pixellab.ai` na política de rede do ambiente e definir `PIXELLAB_API_KEY`
  nas variáveis do ambiente (`PIXELLAB_MCP_URL` opcional). `Tools/pixellab.js` lê as duas.
- Repetir o comando retoma: item já baixado é pulado; job falhado não é cobrado.
