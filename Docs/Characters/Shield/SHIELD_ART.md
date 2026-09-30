# Escudo do Odisseu — N-07 do Final Polish

2026-09-30. **Nenhum asset novo. Nenhuma geração no PixelLab (0 créditos; saldo segue 845 / 2000).**

- Capturas: `Shield/_capturas/` — `parado`, `movimento`, `erguendo`, `defesa`, `defesa_no_ar` (`_antes` e `_depois`) e
  `_comparacao.png` (recorte ampliado: defesa antes | depois | erguendo | no ar antes | no ar depois)
- Script: `Assets/Scripts/Editor/ShieldArtFix.cs` (`Apply`, `Shots -shieldShots antes|depois`)

## 1. Auditoria

| Item | Achado |
|---|---|
| `ShieldVisual` | filho de `Player/Visual` (a raiz que o `PlayerController` espelha) · local **(0,42; 0,70)** · escala **(0,16; 0,85)** → retângulo de 0,16 × 0,85 un |
| Sprite | `PlaceholderSquare` (8 px/un), cor (0,55; 0,72; 0,95) — **quadrado azul** |
| Sorting | layer `Default`, **ordem 2** (o `Body` do jogador está na 0) |
| Quem liga | `PlayerShield.ApplyVisual` → `shieldVisual.SetActive(blocking)` enquanto o botão de defesa está seguro (em qualquer estado, inclusive no ar). Nenhuma cena sobrescreve o objeto |
| Animação | `PlayerAnimator`: no chão, com `IsBlocking`, toca **`Shield`** (6 quadros, erguer) e depois **`ShieldHold`** (4, guarda em loop). No ar, `Jump`/`Fall` |
| Asset reutilizável | **o próprio master do Odisseu**: os 10 quadros de `Shield`/`ShieldHold` da folha `CHR_Odysseus` já desenham um **escudo hoplita redondo, bronze e madeira, com umbo** — no estilo do master, porque É o master. Nos arquivos de cenário: `ithaca_shield_round_01` e `troy_broken_shield_01` (props, não usados) |

**Conclusão:** o escudo definitivo já existia e já aparecia na defesa. O quadrado azul era desenhado **por cima** dele,
em ordem 2, nas 16 fases (captura `defesa_antes`). Gerar um escudo novo criaria um segundo escudo sobre o primeiro.

## 2. Correção

**Desligado o `SpriteRenderer` do `ShieldVisual` no prefab `Player`** — uma linha (`m_Enabled: 1 → 0`), conferida no
diff. Preservados: o objeto, a posição, a escala, o pivô, a ordem 2, a referência no `PlayerShield` e o
`SetActive(blocking)`. Nenhuma lógica de defesa, combate, controle ou animação mudou; o master não foi tocado.

(Ao salvar pelo Unity, o prefab foi reserializado com campos de valor padrão e blocos reordenados — sem efeito, mas
ruído no diff. O arquivo foi restaurado do commit e só a linha do renderer foi trocada.)

| | Escudo em jogo (defesa no chão) |
|---|---|
| Asset | `Resources/Odisseia/Characters/CHR_Odysseus.png`, estados `Shield` / `ShieldHold` |
| Tamanho | célula 84 × 84 px (1,96 un); o escudo cobre do peito ao quadril do Odisseu (não medido em pixels) |
| PPU / escala | 42,857 · 1 |
| Pivô | o do master (BottomCenter, pés na última linha) |
| Sorting | `Default`, ordem 0 (é o próprio `Body`) |
| Direção | acompanha sozinho: é parte do sprite do corpo, e o `Visual` é espelhado pelo `PlayerController` |

## 3. Verificação

| Estado | Resultado |
|---|---|
| Idle, movimento | sem escudo, como antes (o escudo só aparece na defesa) |
| Defesa — erguer (`Shield`) | escudo sobe à frente do peito, sem placeholder |
| Defesa — guarda (`ShieldHold`) | escudo de bronze à frente do corpo, **sem cobrir o rosto** |
| Defesa no ar | **sem indicação visual** (antes: o quadrado azul). Ver §4 |
| Ataque, arco, dano | travam o estado pela duração do clipe (código inalterado) |
| Mudança de direção | o escudo faz parte do corpo espelhado |

Verificado por captura (poses montadas na cena da Fase 03, sem salvar). **Não testado em play mode**, navegador, mobile ou
gamepad — em especial a transição erguer → guarda e defender andando.

## 4. Limitação conhecida

A defesa **no ar** continua mitigando dano (o `PlayerShield` não olha se está no chão), mas o `PlayerAnimator` mostra
`Jump`/`Fall`, que não têm escudo. Mostrar o escudo ali exigiria mudar o `PlayerAnimator` ou dar ao `ShieldVisual` um
sprite que só aparecesse no ar — mudança de sistema, fora do escopo. Se for feito: o renderer está pronto, só desligado.

## 5. Validação

| Teste | Resultado |
|---|---|
| `GlobalCastAudit` | 16 cenas; o `ShieldVisual` segue listado (PlaceholderSquare, inativo), agora com o renderer desligado |
| `CampaignValidation` | passa (exit 0, 0 erros de compilação) |
| Build WebGL | **Build Finished, Result: Success** (exit 0) |
