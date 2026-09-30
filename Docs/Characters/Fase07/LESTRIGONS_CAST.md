# Fase 07 — Lestrigões: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Aprovado em 2026-09-29.
Contém o **LESTRIGON WARRIOR MASTER v1**.

- Master × Polifemo: `Fase07/_master_vs_polifemo.png` · humano × ciclope × lestrigão: `Docs/Characters/CAST_GIANTS.png`
- Tiras: `_tiras_idle_throw_run_descartado.png` (a linha de baixo é o Run descartado) · `_tira_charge.png`
- Capturas: `Fase07/_capturas/` (`lestrigon_repouso|aviso|arremesso`, `perseguidor`, `cena_008|022`)
- Id e prompts: `Docs/Characters/cast.json` (`Lestrigon_Warrior`)

## 1. CastProbe

O probe ganhou nesta fase o bloco de **perseguidor** (`PursuerHazard`): posição, velocidade, colisor e
**se o desenho está ligado** — porque um dresser de cenário pode desligá-lo junto com os placeholders.

| Item | Achado |
|---|---|
| Arremessadores | **2 `Giant`** (`BossController`), x = 8 e 22 — placeholders: corpo 3,0 + cabeça 1,3 = **4,25 un**, flutuando de y = 1,0 a 5,25 (3 un acima do chão) |
| Gameplay deles | a cada **2,8 s** avisam (**0,7 s**) e golpeiam 3 pontos fixos do chão (5/9/13 e 19/23/27, raio 1,4); 999 de vida, **nenhum colisor** |
| Perseguidor | **`PursuerHazard`** em x = −24: colisor **3 × 4 un** que avança a **5,5 un/s** e mata ao toque — com o desenho **DESLIGADO** pelo `LestrigoesSceneDresser` (que desliga os placeholders da fase). **Uma ameaça mortal invisível** |
| Inimigos `EnemyBasic` | nenhum → **nenhum inimigo carmesim** na fase |
| NPCs, tripulação, narrativos | nenhum; falas do Odisseu e do narrador |
| Animator | 0 |
| Arma | **pedra** — "Eles despedaçam meus navios com **pedras do tamanho de casas**", e os golpes caem em pontos marcados no chão |

## 2. Inventário e estados

| Personagem | Papel | Facção | Antes | Estados usados | Decisão |
|---|---|---|---|---|---|
| Odisseu | Player | Casa de Odisseu | master v1 | 16 | **reutilizado** |
| Lestrigão arremessador ×2 | Chefe (`BossController`) | Lestrigões | quadrados | **Idle, Telegraph, Attack** (eventos do `BossController` via `BossAnimator`) | **novo master** |
| Lestrigão perseguidor | `PursuerHazard` | Lestrigões | **invisível** | **Run** (ele só se move; loop do quadro 3) | **mesmo master** |

**Não gerados:** Walk (ninguém anda devagar), Hit e Death (sem colisor, 999 de vida; o perseguidor não
recebe dano), Throw de projétil (o código não lança objeto — o golpe cai nos pontos marcados).

**Variações:** nenhuma. Os três são o mesmo guerreiro em papéis diferentes; o gameplay não os distingue.

## 3. LESTRIGON WARRIOR MASTER v1

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Enemies/CHR_Lestrigon_Warrior.png` (1098 × 764, 19 quadros) |
| PixelLab | `b97b3d21-4dff-45fe-8c3b-5c0906ce4daa` · v3 · size 192 |
| Altura | 186 px = **4,34 un = 3,1× o Odisseu** — a altura dos placeholders (4,25), não escala nova |
| PPU | 42,857; escala 1 |
| Estados | Idle 6 `[0,1,2,3,2,1]` · Telegraph 6 (Throw 0–5) · Attack 2 (Throw 6–7) · Run 5 (Charge 3–7) |
| Paleta | pele **ocre-avermelhada**, couro escuro, bronze, pedra cinza — **quente contra o cenário frio** (basalto, água escura, alvenaria cinza). Sem carmesim, sem índigo, sem o velo branco do Polifemo |
| Direção | arremessadores olham para a esquerda (o Odisseu vem de lá, com o perseguidor atrás); o perseguidor corre para a direita |

Prompt (base `side view, 16-bit pixel art game character, Greek mythology` — **sem lugar**, lição da Fase 06):

> Laestrygonian giant warrior from the man-eating giant people of Greek myth, a towering broad-shouldered
> giant, ruddy ochre-brown skin, two fierce eyes under a heavy brow, shaved head with a single black
> topknot, a long black braided beard, thick muscular arms and legs, a dark leather kilt studded with
> bronze, a wide leather belt, a bronze torque around his neck and bronze arm rings, bare feet, holding a
> large rough grey boulder in both hands in front of his belly, the boulder stays held in both hands,
> menacing brutish expression

### Diferença do Polifemo — por forma e por ação, não só por cor

| | Polifemo (ciclope) | Lestrigão |
|---|---|---|
| Olhos | **um**, grande, no meio da testa | **dois**, sob testa pesada |
| Cabeça | juba castanha desgrenhada | **cabeça raspada com um coque preto**, barba trançada |
| Roupa | velo branco de pastor | **couro escuro com tachas de bronze**, torque, braceletes |
| Pele | bronzeada clara | **ocre-avermelhada** |
| Arma / ação | clava, **golpe** no chão | **pedra**, **arremesso** |
| Quem é | criatura solitária | **povo guerreiro** (a fase é uma cidade de gigantes) |

Teste em `CAST_GIANTS.png`: em preto, a juba e a clava longa do ciclope contra o coque pontudo e a pedra
redonda do lestrigão — distinguíveis sem nenhum detalhe interno.

**Risco registrado:** barba trançada e perneiras de pelo puxam para o "bárbaro". O torso nu, o torque e
os braceletes de bronze seguram a época. Aceito.

### Animações

| Geração | Uso | Observação |
|---|---|---|
| Idle (Receita A) | Idle | estável |
| Throw (Receita B, 8 q.) | **Telegraph 0–5** (ergue a pedra, segura acima do ombro) + **Attack 6–7** (traz para a frente e arremessa) | uma geração, dois estados — como o Slam do Polifemo |
| Run (Receita B) | **descartado** | a pedra **sumia nos quadros 1–4 e voltava em 5–7**: com o loop a partir do 3, ela piscaria |
| Charge (Receita B) | **Run 3–7** | pedido "de mãos vazias" e veio **com a pedra em todos os quadros** — o contrário do pedido, mas o requisito era consistência, e ele a cumpre |

## 4. Integração no Unity

- **`BossArtDresser`** (novo, comum): a lógica de vestir chefe saiu do `CyclopsCastDresser` no segundo
  uso. O Polifemo foi re-vestido por ele e saiu idêntico (6,00 × 4,71 un).
- **`LestrigonCastDresser.Dress`**:
  - os 2 arremessadores pelo `BossArtDresser` — pés no chão (y = −2, o mesmo dos pontos de ataque),
    `flipX`, ordem 1, `Head` removido, Idle 5 / Telegraph 9 (0,67 s, dentro do aviso de 0,7 s) / Attack 10,
    `BossAnimator` ligado;
  - o perseguidor ganha um **filho** `Art_Lestrigon` com o gigante correndo (estado padrão Run, 10 FPS,
    ordem 1). **O `PursuerHazard`, o colisor e a velocidade não mudam.**
- **`LestrigonCastDresser.Poses`**: fotos das 3 poses com o Odisseu e do perseguidor alcançando-o, sem salvar.
- Nenhum prefab compartilhado envolvido. `BossController` intacto.
- **`PlaceholderProbe`**: justificativas de `Giant/Body` e `Giant/Head` retiradas.

## 5. Validação

| Teste | Resultado |
|---|---|
| CastProbe | 2 arremessadores e o perseguidor a 42,857 px/un, **escala 1**, 4,41 un; pés em y = −2,00 |
| `PlaceholderProbe` Lestrigões e Ciclopes | nada visível, nada inativo |
| `CampaignValidation` | passa |
| Compilação | 0 erros |
| Build WebGL | ver relatório |
| Capturas | arremessador em repouso, aviso e arremesso com o Odisseu diante dele (3,1×, mesmo chão); perseguidor correndo atrás do Odisseu; comparação de gigantes em cor e silhueta |

**Não testado:** play mode (ciclo aviso → arremesso e corrida do perseguidor só rodam em runtime),
navegador, mobile, gamepad.

## 6. Problemas encontrados

| Problema | Situação |
|---|---|
| Arremessadores eram quadrados flutuando 3 un acima do chão | **corrigido** |
| **Perseguidor invisível** (desenho desligado pelo dresser de cenário) | **corrigido** — gigante correndo, como filho |
| Gigante do perseguidor com **17,6 un**: herdou a escala 3 × 4 do Transform do `PursuerHazard` (que dá o tamanho do colisor e não pode mudar) | **corrigido** — o filho anula a escala herdada; o CastProbe acusou (`ESCALA!`) |
| Run com a pedra piscando no loop | **descartado**, refeito como Charge |
| Nenhum estado dos chefes era acionado | **corrigido** — `BossAnimator` (o mesmo do Polifemo) |

## 7. Pendências

- **Cenário (ENVIRONMENT ART / SCENARIO), não tocado:** as peças do palácio de Éolo (Fase 06) foram
  montadas encolhidas a **0,52** — mesma classe de defeito de densidade que o odre antigo tinha. Registrado,
  sem alteração, como pede o briefing.
- As fases 01, 08, 09, 14 e 15 ainda usam o `EnemyBasic` carmesim.

## 8. Custo

**36 gerações** (705 → 741 de 2000): master 6 + Idle/Throw/Run 22 + Charge 8. **Um descarte** (o Run, 8).

## Correção de gameplay — BUG-001 (2026-09-30)

Detalhes em `Docs/QA/FULL_CAMPAIGN_PLAYTEST.md` §9. O perseguidor andava durante as travas de controle (fala de
abertura e fala final) e matava o Odisseu sem que ele pudesse agir. A cena ganhou o `LestrigoesChaseGuard`
(`Assets/Scripts/Levels/`), posto pelo `LestrigoesChaseFix` (override da cena): perseguidor e gigantes parados até o fim
da fala de abertura, por 1,5 s depois de cada respawn e a partir da conclusão da fase; a arte do perseguidor fica em
**Idle** enquanto está parado. O checkpoint foi de x = 6,0 (dentro da área do golpe em x = 5) para **x = 1,5**.
`PursuerHazard`, `BossController`, a arte e a velocidade continuam os mesmos.
