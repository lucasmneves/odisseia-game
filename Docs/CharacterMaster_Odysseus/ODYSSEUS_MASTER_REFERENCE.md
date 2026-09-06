# ODYSSEUS_MASTER_REFERENCE

**Conceito B — Aventureiro — APROVADO em 2026-08-31. Pipeline completo e integrado no Unity.**
Ferramenta: PixelLab MCP (`https://api.pixellab.ai/mcp`) · Review visual: `odysseus-master.html`

> Este personagem é a referência oficial. **Nenhuma animação futura redesenha o Odisseu do zero** —
> todas saem deste rig via `animate_character` / `create_character_state`.

---

## 1. Identidade oficial

| | |
|---|---|
| `character_id` | **`908b7f60-0624-43bc-a0cd-89ad16416400`** |
| Nome no PixelLab | `ODYSSEUS_MASTER_B_Adventurer` |
| Tags | `ODYSSEUS_MASTER_REFERENCE`, `odisseia`, `protagonista`, `fase1-aprovado` |
| Grupo de estados | `4a914ad2-a6aa-4575-aae3-d1703dc6fe5f` |

Descrição visual: túnica de linho sem mangas, arreio de couro cruzado, uma ombreira de bronze,
capa carmesim curta, espada embainhada no quadril, cabelo médio ondulado, barba curta,
botas de couro.

### Conceitos descartados (mantidos na conta, não apagados)

| | character_id | Reaproveitamento sugerido |
|---|---|---|
| A — Guerreiro | `857c28dd-c618-435c-9e80-616b60b17bb7` | base visual para soldado da guarda |
| C — Rei | `85aed3ad-1256-429e-9bb3-89e1acb6f5f7` | base visual para nobre de Ítaca |

Personagem antigo, **não usar** (gerado em `low top-down`, errado para platformer):
`77ff0a5a-35a8-4a66-b899-0b762b5b789b`.

---

## 2. Especificação travada

Régua de escala para NPCs, inimigos, chefes, animais e objetos.

- **Canvas da referência:** 64 × 64 px
- **Canvas das animações:** 84 × 84 px — o PixelLab expande sozinho, com 12 px de folga em cima e embaixo
- **Altura do personagem:** 60 px — **constante entre todos os estados**
- **Proporção:** ≈ 5,5 cabeças (cabeça ~11 px)
- **Câmera:** `side`, nível dos olhos
- **Direção de gameplay:** `east`, espelhada por código para a esquerda
- **Contorno:** preto de cor única
- **Sombreamento:** de forma, **sem luz direcional dominante** (ver seção 4)
- **Paleta:** 28 cores, 7 rampas de 4 — `Palette/ODYSSEUS_PALETTE.{hex,gpl,png}`
- **Fundo:** transparente, alpha duro, sem anti-aliasing
- **Modo PixelLab:** `v3` — 2 gerações no personagem, **1 por animação de direção única**
- **Import no Unity:** Filter Mode `Point (no filter)`, Compression `None`

---

## 3. Paleta oficial

Extraída dos pixels reais do sprite aprovado, não escolhida à mão.
Arquivos prontos para Aseprite em `Palette/`.

| Rampa | Highlight | Base | Sombra | Profunda |
|---|---|---|---|---|
| Contorno | `#000106` | `#050507` | `#000009` | `#0e0813` |
| Pele | `#e2925d` | `#ca7545` | `#a5542f` | `#8a3e24` |
| Cabelo e barba *(castanho-avermelhado, oficial)* | `#924827` | `#71381e` | `#4f2718` | `#3d1d19` |
| Túnica de linho | `#c6a263` | `#a27c41` | `#7e582a` | `#654122` |
| Bronze e dourado | `#f5cd5c` | `#bb7e2c` | `#9d611f` | `#733e16` |
| Couro | `#613115` | `#582b18` | `#45221a` | `#341719` |
| Capa carmesim | `#981d1c` | `#720e18` | `#460427` | `#240e15` |

---

## 4. Correção: a luz não é do canto superior esquerdo

O briefing pedia luz vindo do canto superior esquerdo. **Os sprites gerados não têm isso.**

Medição do gradiente de luminância (excluindo contorno e capa, para não enviesar):

| Vista | correl. luminância × X | correl. luminância × Y |
|---|---|---|
| east | +0,089 | +0,056 |
| south | −0,117 | −0,048 |
| west | −0,263 | +0,162 |

Todas as correlações são fracas (\|r\| < 0,27) e **o componente vertical troca de sinal entre
as vistas**. Não existe luz-chave direcional: o que o PixelLab aplica é sombreamento de forma
(mais escuro nas bordas e vincos).

**Decisão:** adotar sombreamento de forma como a regra oficial. Manter "luz superior esquerda"
na spec faria todo asset futuro divergir do master, e forçá-la exigiria repintar cada sprite —
o que contradiz a regra de não redesenhar.

---

## 5. Contrato de integração no Unity

`Assets/Scripts/Systems/SpriteAnimator.cs` carrega por `Resources.LoadAll` e lê o estado do
**próprio nome do sprite fatiado**: `<Folha>_<Estado>_<NN>`.

O `Player.prefab` aponta para `Odisseia/Characters/CHR_Odysseus` (arquivo atual: 1848×975).
**Não sobrescrever esse arquivo sem substituir a folha inteira** — é o sprite vivo do player.

### Estados que a folha atual já define

| Fase | Estado | Frames | `frame_count` + `keep_first_frame` |
|---|---|---|---|
| 2 | Idle ✅ | 7 | 6 + true |
| 3 | Run | 8 | 8 + false |
| 4 | Jump | 7 | 6 + true |
| 5 | Fall *(novo)* | 4 | 4 + false |
| 6 | AttackLight | 7 | 6 + true |
| 6 | AttackHeavy | 7 | 6 + true |
| 7 | Shield *(novo)* | 6 | 6 + false |
| 8 | Bow *(novo)* | 8 | 8 + false |
| 9 | Damage | 7 | 6 + true |
| 10 | Death | 8 | 8 + false |
| 11 | Interaction *(novo)* | 6 | 6 + false |
| 12 | Victory *(novo)* | 8 | 8 + false |

`frame_count` precisa ser **par** (4–16). Com `keep_first_frame: true` o frame de referência
vira o frame 0, resultando em contagem ímpar.

---

## 6. As duas receitas de animação

A receita que consertou o Idle **quebra** as animações de movimento amplo. Use a certa para cada caso.

### Receita A — movimento sutil (Idle, respiração, poses paradas)

```
mode: "v3", directions: ["east"]
keep_first_frame: true            // frame 0 = pose exata da referência
action_description: "<ação> in strict side profile facing right, head stays turned
  right in profile, shoulders and hips stay square to the right, no turning toward
  the viewer, <detalhe>"
```

A âncora impede o rosto de girar para frontal ao longo do ciclo.

### Receita B — movimento amplo (Jump, Fall, ataques, dano, morte)

```
mode: "v3", directions: ["east"]
keep_first_frame: false           // SEM âncora
action_description: "<ação forte e específica>, side view facing right, <detalhe
  anatômico explícito do que o corpo faz>"
```

**Por quê:** com a âncora e o trava-orientação pesado, o Jump saiu com a base em
71,71,71,71,70,69,71 — o corpo subia 2px, virou passada no chão. Sem âncora, os pés
passaram a sair 9px do chão e o corpo comprimiu para 50px no agachamento aéreo.

**Cuidado com o excesso:** afrouxar demais também erra. O Fall v2 saiu com o corpo
horizontal, lendo como voo. Descrições de movimento amplo precisam dizer explicitamente
o que o corpo **não** deve fazer.

---

## 7. Os dezesseis estados

Todos em `v3`, direção `east`, canvas 84×84, **1 geração cada**.
Zero pixels semi-transparentes; nenhum frame encosta em qualquer borda do canvas.

| Fase | Estado | Frames | Receita | Grupo |
|---|---|---|---|---|
| 2 | Idle | 7 | A | `18f59529-7ba5-4b15-aa1c-fb45b16f9cdb` |
| 3 | Run | 8 | **B** *(refeito 2×, ver seção 17)* | `7f6a7775-979c-4fe1-8272-5fbc0e548515` |
| 4 | Jump | 8 | B | `5569f693-a3f7-473c-a848-cb0da1c9a064` |
| 5 | Fall | 2 | B + seleção manual | `f58c81f3-b6f8-436e-af89-d8e061b83577` |
| 6 | AttackLight | 6 | B | `101e9f71-0b6d-4bb6-acd2-105eb1e88191` |
| 6 | AttackHeavy | 8 | B | `e22cdea1-d128-4854-8d85-8bbd9cb42696` |
| 7 | Shield | 6 | B | `24087b3c-3fda-43da-bf6e-f9dd70c213cb` |
| 8 | Bow | 8 | B + remapeamento de cor | `4f387e2d-1301-4286-a5e5-881c2606ca96` |
| 9 | Damage | 6 | B | `96233883-e3f0-4df4-bde1-a8d5056f41c8` |
| 10 | Death | 8 | B | `069c98f9-28e5-47b6-9a67-2421fda3c16d` |
| 11 | Interaction | 6 | B | `e064b9f7-b9fd-4336-9c28-c975db65bb89` |
| 12 | Victory | 8 | B | `ae51eeac-71b3-42d4-aee9-a0b6b6ba2f5b` |
| — | Crouch | 4 | B | `853e8b7d-07d8-471b-99e8-d09c58d11c3f` |
| — | CrouchWalk | 3 | B + seleção manual | `7f8d7bcb-4246-498a-9b6f-8084253075ff` |
| — | Climb | 6 | B | `72088c2a-115d-487d-9c30-b4d600df6c88` |
| — | ShieldHold | 4 | B + seleção de emenda | `fb40d80b-3ef4-494d-b77f-bc41ea89d590` |

**98 frames por 20 gerações.** Arquivos em `Master_B/<Estado>_east/CHR_Odysseus_<Estado>_<NN>.png`.

### O bloqueio de escudo e arco não existia

Eu tinha registrado as Fases 7 e 8 como bloqueadas: o sprite master não tem escudo nem arco,
e `create_character_state` custa **20–40 gerações**.

Mas `animate_character` **inventa objetos que não estão no rig** — já tinha desenhado a lâmina
da espada embainhada. Testado com escudo e arco, funcionou: aspis de bronze com umbão, arco de
madeira com corda e flecha, ambos na paleta certa, **1 geração cada**.

Consequência de projeto: os itens só existem durante essas animações. É exatamente o modelo
"estado de combate" — o Odisseu não carrega escudo no idle, na corrida ou no salto.

### Qualidade — o que ficou abaixo do pedido

- ~~**Run** lê como caminhada firme, não corrida disparada.~~ Corrigido — seção 17.
- **Victory** virou saudação com a espada à frente do peito, não triunfo com o braço erguido
  (o topo do sprite nunca passa de y=12, confirmando que nada sobe acima da cabeça).
- **Fall** tem só 2 frames selecionados à mão.

Nenhuma está errada. Cada uma custa 1 geração para refazer.

### Teste da folga de canvas

O `AttackHeavy` ergue a lâmina acima da cabeça e o topo do sprite fica em y=12 no canvas 84×84.
O risco de corte que levantei na Fase 1 **não se materializou em nenhum dos 82 frames**.

### Defeitos que aparecem quando a arma é inventada

A primeira versão do `AttackLight` saiu com uma **espada solta flutuando no ar**, desconectada
da mão (preservada em `_descartados/AttackLight_v1_espada_solta/`). O conserto foi repetir na
descrição que o item fica **preso à mão em todos os frames** e **nunca se desprende**. Use essa
formulação sempre que a animação envolver um objeto que não existe no sprite master.

---

## 8. Integração no Unity (concluída e verificada)

### A folha

`Assets/Resources/Odisseia/Characters/CHR_Odysseus.png``Assets/Resources/Odisseia/Characters/CHR_Odysseus.png` — **756 × 1344 px, 107 KB**.
Grade uniforme de 84×84: **uma linha por estado**, na ordem das fases do pipeline.
Células sobrando ficam transparentes.

| | |
|---|---|
| Sprites fatiados | 99 |
| Pivô | `alignment: 9` (custom), **{0,5; 0,142857}** — na linha dos pés, uniforme nos 82 |
| Nomes | `CHR_Odysseus_<Estado>_<NN>` |
| Import | Filter Mode `Point`, Compression `None`, maxTextureSize 2048 |
| Pixels por unidade | **42,857143** — ver abaixo |

O pivô em 0,142857 sai de medição, não de chute: o chão do personagem está na linha 71 do
canvas de 84, ou seja 12 px acima da base da célula — 12/84. Como é o mesmo em todos os
estados, o Odisseu não "pula" de altura ao trocar de animação.

### O que foi preservado para não quebrar referências

- **GUID mantido:** `71ffcd5a5101b09622667079891272f6`. O asset continua sendo o mesmo para o Unity.
- **`internalID` do `Idle_00` fixado em `607464041`** — é o `fileID` que o `SpriteRenderer` do
  `Player.prefab` referencia. Sem isso o sprite inicial apareceria como *missing* no Inspector.
- **`resourcePath` do prefab não mudou** (`Odisseia/Characters/CHR_Odysseus`).
- **Folha antiga preservada** em `Docs/CharacterMaster_Odysseus/_backup_folha_antiga/`
  (1848×975, com o `.meta` e uma cópia do `Player.prefab` de antes da edição).

### FPS por estado no `Player.prefab`

Sete estados já tinham configuração; os cinco novos foram adicionados.

| Estado | FPS | Loop | | Estado | FPS | Loop |
|---|---|---|---|---|---|---|
| Idle | 8 | sim | | Shield | 12 | não |
| Run | 16 | sim | | Bow | 14 | não |
| Jump | 10 | não | | Damage | 18 | não |
| Fall | 8 | sim | | Death | 10 | não |
| AttackLight | 18 | não | | Interaction | 10 | não |
| AttackHeavy | 16 | não | | Victory | 10 | não |


### Escala no mundo — `pixelsPerUnit` é obrigatório ajustar

**Erro cometido e corrigido em 2026-08-31:** a folha nova entrou com o `spritePixelsToUnits: 100`
herdado da folha antiga, e o Odisseu apareceu no jogo com **42,6% do tamanho certo**.

A causa é aritmética simples e fácil de repetir:

| | Corpo na arte | A 100 PPU |
|---|---|---|
| Folha antiga | 141 px | 1,41 unidades |
| Folha nova | 60 px | 0,60 unidades |

O `BoxCollider2D` do Player tem `m_Size: {x: 0.6, y: 1.4}` — **o mundo inteiro, o level design e a
física foram construídos em volta de um personagem de 1,4 unidades.**

Valor correto: `spritePixelsToUnits: 42.857143` (= 60 ÷ 1,4). Com ele o corpo de 60px volta a
valer exatamente 1,400 unidades e a célula de 84px vale 1,960.

**Regra:** ao trocar a resolução dos sprites, recalcular sempre `PPU = altura_do_corpo_em_px ÷ 1,4`.

### Densidade de pixel: o Odisseu não é o desalinhado

A 42,857 PPU cada pixel de arte do Odisseu ocupa 2,33× mais tela que um pixel dos NPCs atuais.
Mas a comparação não se sustenta, porque **os NPCs não são pixel art**:

| Sprite | Tamanho | Cores únicas |
|---|---|---|
| CHR_NPC_Penelope | 74×168 | 6.927 |
| CHR_NPC_Telemachus | 70×182 | 6.697 |
| CHR_NPC_Eumaeus | 74×160 | 5.307 |
| **Odisseu (novo)** | 84×84 | **51** |

São ilustrações suavizadas, não arte de pixel com paleta limitada — o oposto do que o briefing
pede ("hand-crafted pixel art", "limited color palette", "no photorealism"). São legado de outro
pipeline e serão substituídos. **A densidade oficial do jogo passa a ser a do master**, não a deles.

Subir a resolução do Odisseu para casar com o legado sairia por ~36 gerações só nas animações
(canvas maior encarece: `ceil(l·a·frames/65536)` por direção) — inviável com 15 no saldo.

### Verificação

`Assets/Scripts/Editor/OdysseusSheetProbe.cs` — probe em batchmode, no padrão dos outros
probes do projeto:

```
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod OdysseusSheetProbe.Run -logFile Logs/odysseus_sheet.log
```

Ele não pergunta "a textura importou?" e sim "o `SpriteAnimator` consegue montar todos os
clipes a partir dos nomes?" — porque um sprite com nome fora do padrão não vira erro, só some
do clipe e encurta a animação em silêncio.

Resultado em 2026-08-31, **exit 0, zero erros de compilação**:

```
[Folha] 82 sprites em Resources/Odisseia/Characters/CHR_Odysseus
[Folha] Idle: 7 frames OK ... Victory: 8 frames OK   (12 estados)
[Folha] pivô uniforme em 82 sprites: (0.50, 0.14)
[Folha] prefab: resourcePath OK, 12 estados configurados
[Folha] RESULTADO: OK
```

`CampaignValidation.Run` também passa (exit 0, 16 etapas na ordem oficial) — o prefab editado
não quebrou a campanha.

---

## 9. Movimentação: agachar, andar agachado e escalar

Adicionado em 2026-09-01. Três animações novas (3 gerações) mais código.

### Agachar — `PlayerController`

Segurar `S` / `↓` / D-pad ↓ encolhe o `BoxCollider2D` **para baixo** (os pés ficam na mesma
linha) e limita a velocidade a 45%.

`crouchHeightFactor = 0.73` não é um número escolhido: é a medida do sprite agachado
(44 px de altura contra 60 em pé). O `OdysseusSheetProbe` falha se os dois divergirem mais
que 0,05 — collider maior que a arte some dentro do teto, menor sobra para fora dela.

**Levantar não é o oposto de agachar:** soltar o botão debaixo de um teto mantém o
personagem agachado, porque devolver o collider inteiro o faria atravessar a geometria.
Pular ali também não sai.

### Escalar — `PlayerClimb`

Encostar numa beirada e pular sobe por cima. A detecção usa **dois testes que precisam
discordar**: na altura do peito tem que *haver* geometria, na altura da cabeça tem que estar
*livre*. Uma parede inteira acerta os dois e é ignorada — é isso que separa beirada de parede.

Um terceiro raycast aponta para baixo além da quina, e é a altura **dele** que vira o destino,
então degraus irregulares funcionam sem ajuste por objeto.

Durante a subida o movimento para via `PlayerController.MovementSuspended`, **não** desligando
o componente: desligá-lo derrubaria o action map, compartilhado com ataque, escudo e arco.

### Fall entrou em uso

O estado existia desde a Fase 5 mas nunca era tocado — o animador mandava `Jump` para
qualquer momento fora do chão. Agora velocidade vertical abaixo de `fallThreshold` troca
para `Fall`.

### Novos controles

| Ação | Teclado | Gamepad |
|---|---|---|
| Agachar / andar agachado | `S` · `↓` | D-pad ↓ · analógico ↓ |
| Escalar beirada | `Espaço` encostado nela | botão sul |
| Golpe forte | `Z` duas vezes em até 0,8 s | botão oeste 2× |

A ação `Crouch` foi adicionada ao `PlayerControls.inputactions`. O `InputBindingsProbe.Check`
passa — nenhuma tecla colide.

### A curadoria que os frames exigiram

- **CrouchWalk:** dos 6 frames gerados só os 3 baixos entraram. Os primeiros estavam em pé e
  o loop faria o personagem levantar e abaixar a cada volta.
- **Climb:** duas tentativas. A primeira saiu com os punhos fechados na frente do rosto,
  lendo como pose de guarda. A segunda pediu braços esticados acima da cabeça e joelho ao peito.
- **Crouch:** duas tentativas. A primeira descia só 17% da altura; a segunda desce 27%.

Descartes em `Master_B/_descartados/`.

### Ainda não verificado

A fiação passa nos probes, mas `chestHeight`, `headHeight`, `reach` e `climbDuration` são
números de sensação — precisam de teste em jogo. Os gizmos do `PlayerClimb` desenham os dois
testes com o objeto selecionado: amarelo precisa encostar, ciano precisa passar livre.

**Nenhuma fase foi desenhada pensando em escalada**, então pode não haver geometria na altura
certa em lugar nenhum ainda.

---

## 10. Arco e escudo: do gatilho ausente ao sincronismo

Sintoma relatado em 2026-09-01: *"a flecha simplesmente sai do corpo do personagem"*.

Não era problema de arte nem do ponto de disparo. O `firePoint` está em (0,65; 0,75) e a
medição do sprite confirma que é o lugar certo — o arco fica a ~0,27 un à frente com a borda
dianteira em ~0,57, então 0,65 cai na ponta da flecha.

**O estado `Bow` nunca era acionado.** O `PlayerAnimator` assinava `Attacked`, `Damaged` e
`Died`, e mais nada. O `PlayerBow` já expunha um evento `Fired` e o `PlayerShield` já expunha
`IsBlocking` / `BlockingChanged` — ambos sem nenhum ouvinte.

Ou seja: arte na folha, FPS configurado no prefab, evento disparando, e o estado nunca tocado.
**Nada nesse arranjo gera erro.** É um bug silencioso por construção.

### O conserto

- `bow.Fired` → `Trigger("Bow")`, mesma convenção do ataque corpo a corpo: o evento sai depois
  da flecha ser instanciada, e o clipe trava o estado pela duração.
  8 quadros a 14 fps = ~0,57 s, um pouco acima do cooldown de 0,5 s do arco, então um segundo
  disparo nunca corta a animação pela metade.
- `shield.IsBlocking` → `Play("Shield")` testado a cada quadro. Defender é **pose mantida**,
  não disparo, então não usa `lockTimer`.

Prioridade final no `Update`: escalada → trava de clipe → no ar → escudo → agachado → correr/parado.

### A checagem que passou a existir

`OdysseusSheetProbe` agora instancia o prefab, chama `Awake` e `OnEnable` **à mão** e lê a
lista de inscritos de `PlayerBow.Fired` por reflexão.

A chamada manual é obrigatória: fora do play mode o Unity não roda esses callbacks em
MonoBehaviour comum, e sem ela a lista viria sempre vazia — o teste acusaria todo mundo.

**Testado ao contrário:** comentando a linha `bow.Fired += OnFired`, o probe sai com exit 1 e a
mensagem *"ninguém escuta PlayerBow.Fired — o arco dispara sem animação"*. Com a linha de volta,
exit 0 e *"1 inscrito(s)"*.

### A faixa azul do escudo

Existia um GameObject `ShieldVisual` no prefab, com o sprite de placeholder e
`m_Color: {r: 0.55, g: 0.72, b: 0.95}` — azul claro. O `PlayerShield` o ligava e desligava
junto com a defesa (`shieldVisual.SetActive(blocking)`).

Era placeholder de quando não existia arte de escudo. Com o aspis de bronze na folha ele
passou a ser uma faixa azul por cima do personagem.

**Conserto:** `shieldVisual` desatribuído no prefab (`{fileID: 0}`); o código já tinha guarda
de nulo. O GameObject continua no prefab, inativo e sem referência — não removi para não mexer
na lista de filhos por YAML sem necessidade.

### Sincronismo da flecha

O `PlayerBow.TryFire()` instanciava a flecha e **só depois** disparava `Fired`. Como a animação
começa no `Fired`, a flecha saía no quadro em que o personagem ainda estava parado: o tiro
acontecia antes de o arco subir.

**Conserto:** o disparo foi separado do lançamento.

- No input: valida, desconta a flecha, marca o cooldown e dispara `Fired` — a animação começa
  e o `TrainingCourse` conta o tiro (ele também escuta esse evento).
- Depois de `releaseDelay` (0,15 s, serializado): a flecha nasce, com o VFX e o som junto.

A **posição** é lida no lançamento, porque o personagem pode ter andado durante o atraso.
A **direção** fica travada no momento do input — virar de lado no meio do saque não redireciona
um tiro já pago.

Ser desligado durante o atraso (cutscene, diálogo, morte) cancela a flecha, em vez de fazê-la
nascer depois, fora de contexto.

0,15 s cai por volta do quadro 2 de 8 do clipe, onde o arco já está erguido. É serializado —
ajustável no inspector sem tocar em código.

### Checagens adicionadas ao probe

- `shieldVisual` tem que estar nulo, senão a faixa azul volta sem ninguém notar.
- `releaseDelay` tem que ser maior que zero, senão a flecha volta a sair antes do arco subir.

### Arco refeito (1 geração + remapeamento)

Grupo oficial: `4f387e2d-1301-4286-a5e5-881c2606ca96` — 8 quadros, direção `east`, Receita B.

Dos três defeitos, **a geração resolveu dois**:

- **Rigidez:** o arco mantém a mesma forma e o mesmo tamanho nos 8 quadros. A largura do sprite
  cresce suave (36 → 51 px) e a altura fica constante em 58–59 px, sem deriva vertical.
- **Perfil:** o corpo não vira mais de frente nos quadros finais.

**A cor não saiu na geração.** Duas tentativas pedindo "madeira escura, nunca dourado ou
metálico" e o arco continuou na rampa Bronze. Insistir numa terceira seria repetir uma
instrução que já falhou duas vezes.

**Resolvido por remapeamento, custo zero.** Script em `scratchpad/madeira.js`:

1. Marca os pixels da família dourada (matiz 30–55, luminância ≥ 100).
2. **Semente:** o dourado à frente da borda dianteira das pernas só pode ser o arco — a
   ombreira e o punho da espada ficam dentro da massa do corpo. As pernas nunca têm arco por
   perto, então a borda delas é uma separação confiável.
3. **Cresce** por vizinhança 8-conectada a partir das sementes, para o punho do arco entrar
   junto mesmo cruzando na frente do torso. A ombreira e o punho da espada não encostam no
   arco (há pele e túnica no meio) e por isso não são alcançados.
4. Remapeia por luminância em 4 passos.

Destino: `#9d611f` `#733e16` `#613115` `#45221a` — **as quatro já estão na paleta oficial**,
então o remapeamento não introduziu nenhuma cor nova.

Resultado: 252 px trocados nos 8 quadros. A ombreira e o punho da espada continuam bronze,
como devem.

Original dourado preservado em `Master_B/_descartados/Bow_v1_dourado/`.

---

## 11. O golpe de espada: o forte existia e era inalcançável

Sintoma em 2026-09-01: *"o golpe da espada está com um movimento bem limitado"*.

Duas causas, e a principal não era a arte.

### O `AttackHeavy` nunca era acionado

Grep no projeto inteiro: nenhum código tocava o estado. O golpe forte de 8 quadros — a
animação com o arco mais amplo do personagem — existia só na folha. O jogo só tocava
`AttackLight`.

Medindo a mudança de pixels entre quadros consecutivos:

| | Mudança por quadro | Amplitude |
|---|---|---|
| AttackLight | 35% · 68% · 60% · **34% · 34%** | 18 px |
| AttackHeavy | 42% · 57% · 39% · **92% · 88%** · 63% · 38% | 19 px |
| Run (base de comparação) | 81% · 78% · 77% · 72% · 80% · 77% · 78% · 82% | 13 px |

No `AttackLight` os quadros 3→4→5 mudam só 34%: ele estende a espada e **segura a pose**.
De 6 quadros, 3 carregam o golpe. O `AttackHeavy` tem dois picos de 92% e 88% — o golpe
grande estava lá o tempo todo.

### O conserto: sequência, sem tecla nova

Um segundo golpe dentro de `comboWindow` (0,8 s) sai como forte: mais dano (35 contra 20),
alcance maior (0,85 contra 0,6) e cooldown 1,5× mais longo. Depois do forte a corrente zera,
senão bastaria martelar o botão para só dar golpes fortes.

Sai pela mesma tecla de propósito — uma tecla nova seria mais uma coisa a aprender por um
movimento que a sequência entrega naturalmente.

O evento `Attacked` **não mudou de assinatura**: quem só conta golpes (o `TrainingCourse`)
continua funcionando. Quem precisa distinguir lê `PlayerCombat.LastAttackWasHeavy`, que é o
que o `PlayerAnimator` faz para escolher entre os dois estados.

### A tentativa de refazer o AttackLight falhou

Uma geração pedindo arco amplo com "a lâmina começa erguida atrás do ombro". A instrução
funcionou como movimento — a mudança por quadro subiu para 29% · 67% · 69% · 58% · 66%,
sem quadros mortos — mas **a espada apareceu solta atrás do corpo** nos dois primeiros
quadros e sumiu no terceiro. Pedir a lâmina longe da mão desconecta o objeto da mão.

Descartada; o `AttackLight` original continua em uso. Em
`Master_B/_descartados/AttackLight_v3_espada_solta/`.

**Padrão que já apareceu duas vezes:** descrever um objeto **longe da mão** faz o v3
desanexá-lo. Foi o mesmo defeito do primeiro `AttackLight` gerado. A formulação que funciona
é sempre "preso à mão em todos os quadros, nunca se desprende".

### Checagem adicionada

`comboWindow` tem que ser maior que zero, senão o `AttackHeavy` volta a ficar inalcançável —
exatamente o estado em que estava.

---

## 12. Interagir e vitória: os dois últimos estados mortos

A suspeita se confirmou. Contando referências ao nome de cada estado no código:

```
Interaction  0 referências        Shield   10
Victory      0 referências        Bow      12
                                  Jump     14
```

`Interaction` e `Victory` fechavam a lista de estados com arte pronta e nenhum gatilho —
os mesmos que arco, escudo e golpe forte.

### Por que estes dois foram ligados ao contrário

Ataque, arco e escudo são **componentes do jogador**: expõem eventos e o `PlayerAnimator`
assina. Interagir e vencer são pedidos por **objetos de cena** — um `InteractPoint`, o
`LevelGoal` — e não há evento no jogador para assinar.

Por isso a direção se inverte: o `PlayerAnimator` expõe `PlayInteraction()` e `PlayVictory()`,
e o gameplay chama.

- **`InteractPoint`** guarda o `PlayerAnimator` na entrada do gatilho e chama `PlayInteraction()`
  quando a interação acontece.
- **`LevelGoal.Trigger()`** chama `PlayVictory()`. A fase pode terminar por colisor **ou por
  roteiro** (o prólogo chama `Trigger()` direto), então não dá para contar com o `other` do
  gatilho: usa o `playerLock`, que já aponta para o jogador, e cai na busca por tag quando ele
  não está preenchido.

O `PlayerInputLock` desliga controle, ataque, escudo e arco, mas **não** o animador — a pose de
vitória sai normalmente mesmo com o outro travando o jogador.

### A checagem que generaliza os cinco casos

O probe agora lê o código do `PlayerAnimator` e exige que a constante de **cada** estado da
folha seja usada em algum lugar, não só declarada. Uma constante que aparece uma vez no arquivo
é a declaração dela e mais nada.

**O primeiro teste negativo reprovou a própria checagem.** Comentei `Trigger(StateVictory)` e o
probe passou mesmo assim: o regex contava o identificador dentro do comentário. Comentar a
chamada é justamente a forma mais provável de um gatilho sumir. Depois de remover comentários
antes de contar, o teste negativo falha com a mensagem certa:

```
[Folha] estados sem gatilho — a arte existe mas o jogador nunca vê: Victory (constante declarada, nunca usada)
```

Com o gatilho de volta: `os 15 estados têm gatilho no PlayerAnimator`, exit 0.
`CampaignValidation` e `PrologueProbe` também passam — este último exercita os pontos de
interação que foram modificados.

---

## 13. A guarda do escudo: um clipe não bastava

Sintoma: *"o Odisseu segurando o escudo está estático"*.

**A arte não era o problema.** O clipe `Shield` tem bastante movimento — 58% · 89% · 87% ·
85% · 67% de mudança entre quadros, com a largura indo de 35 a 49 px. Ele é a transição de
**erguer** o escudo.

O problema era `loop: 0`: ele ergue e **congela no último quadro** enquanto o botão segue
apertado.

E loopar esse mesmo clipe não resolveria — o personagem ficaria erguendo o escudo em ciclo,
repetidamente.

### Dois clipes, não um

- **`Shield`** (6 quadros, `loop: 0`) — ergue o escudo. Toca uma vez.
- **`ShieldHold`** (4 quadros, `loop: 1`, 8 fps) — a guarda que respira. Entra quando a
  subida termina.

No `PlayerAnimator`, a troca usa o `IsFinished` que o `SpriteAnimator` já expunha:

```
bool subiu = animator.CurrentState == StateShield && animator.IsFinished;
Play(subiu || animator.CurrentState == StateShieldHold ? StateShieldHold : StateShield);
```

### Escolha dos quadros pela qualidade da emenda

A geração veio com 6 quadros, mas os dois primeiros ainda estavam subindo o escudo. Medindo
a diferença do último quadro para o primeiro do loop:

| Faixa | Emenda do loop |
|---|---|
| 0–5 | 73% |
| 1–5 | 64% |
| **2–5** | **46%** |

Entre quadros o movimento próprio varia de 46% a 52%. Só a faixa **2–5** tem uma emenda
dentro dessa faixa — nas outras o salto seria visível como um pop a cada volta.

Ficaram 4 quadros. Os dois de subida estão em
`Master_B/_descartados/ShieldHold_frames_de_subida/`.

### Checagem adicionada

Uma lista de estados que **precisam** de `loop: 1` — `Idle`, `Run`, `ShieldHold`,
`CrouchWalk`, `Fall`. Qualquer um deles com `loop: 0` congela no último quadro, que é
exatamente o defeito que a guarda tinha.

### Estado da verificação

A folha passou para **756 × 1344, 98 sprites** (16 estados). Conferido estaticamente: 98
`internalID` únicos, GUID e PPU preservados, `ShieldHold` presente no meta, no prefab
(8 fps, loop) e usado no `PlayerAnimator`.

**O `OdysseusSheetProbe` ainda não rodou nesta mudança** — o Editor do Unity estava aberto e
o batchmode recusa abrir o mesmo projeto duas vezes. Rodar com o Editor fechado:

```
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod OdysseusSheetProbe.Run -logFile Logs/guarda.log
```

---

## 14. O que sobrou como opcional

1. ~~**Run mais veloz**~~ — feito, seção 17.
2. **Victory com braço erguido** — saiu como saudação com a espada à frente do peito.
3. **Fall com mais frames** — hoje são 2, selecionados à mão.

Cada um custa 1 geração e nenhum bloqueia o jogo.

---

## 15. Orçamento PixelLab

Plano **trial**: 40 gerações totais, US$ 0,00 em créditos. **Usadas 33, restam 7.**

| Item | Custo |
|---|---|
| `create_character` v3 @ 64px | 2 gerações |
| `animate_character` v3, 1 direção @ 64px | **1 geração** (mesmo com 16 frames) |
| `animate_character` template | 1 geração **por direção** |
| `create_character` modo `pro` | 20–40 gerações — inviável |
| `create_character_state` | 20–40 gerações — inviável (ver seção 7) |

Animar as 8 direções custaria 8× mais. **O platformer usa só `east`**, espelhado por código —
é isso que faz o pipeline de 12 fases caber em 12 gerações.

---

## 16. Como esta sessão acessou o PixelLab

O MCP `pixellab` está registrado em `~/.claude.json` sob o escopo de projeto `C:/Users/luucas`,
não sob `C:/Users/luucas/Documents/Claude` — por isso as ferramentas não carregam numa sessão
aberta na pasta do projeto. Contorno: JSON-RPC direto no endpoint HTTP via Node.
Solução permanente: registrar o servidor também no escopo desta pasta (exige reiniciar a sessão).

---

## 17. O Run refeito — a receita errada estava documentada o tempo todo

**2026-09-05.** A queixa foi "o Odisseu está apenas andando rápido". Estava certa, e a causa
já estava escrita aqui: a tabela da seção 7 registrava o Run como **Receita A**.

A Receita A é a de movimento *sutil* — âncora `keep_first_frame: true` mais o trava-orientação
pesado. É exatamente a combinação que a seção 6 documenta como destruidora de movimento amplo:
"com a âncora e o trava-orientação pesado, o Jump saiu com a base em 71,71,71,71,70,69,71 — o
corpo subia 2px, virou passada no chão". O Run tinha o mesmo defeito, pelo mesmo motivo, e
ninguém tinha ligado uma coisa à outra.

Medição do ciclo antigo: pés entre y=69 e y=71 — **2 px de folga**, nenhum quadro no ar.

### Duas tentativas, e por que o template perdeu

| | Receita | Frames | Canvas | Distância da paleta | Equipamento |
|---|---|---|---|---|---|
| v2 | template `running-8-frames` | 8 | **64 px** | **11,8** | **perdido** |
| v3 | **B** (v3, `keep_first_frame: false`) | 8 | 84 px | 3,7 | intacto |

O template devolve a melhor corrida das três — inclinação do tronco, cotovelos dobrados,
joelho alto. Mas ele **redesenha o personagem**: ampliado a 8× não há espada embainhada, não há
ombreira de bronze, não há arreio de couro cruzado. Para o sprite que é a referência oficial
do projeto, isso é outro personagem, não outra animação. Preservado em
`_descartados/Run_v2_template_sem_equipamento/`.

**Regra que sai daqui:** template troca o esqueleto e leva o equipamento junto. Para um
personagem com identidade travada, template só serve se a silhueta for genérica.

### Detalhes do template que custam tempo se descobertos por acidente

- Ele devolve o **canvas nativo do personagem (64 px)**, não os 84 px dos outros estados.
  Encaixar exige `Tools/fit-character-state.js`.
- `animate_character` **deduplica por template + direção**: repetir a mesma chamada devolve
  "already complete" sem gerar nada e sem cobrar. Para tentar de novo é preciso
  `delete_animation` antes.
- Passar `action_description` junto de um `template_animation_id` **não** muda o resultado.
- O personagem tem 8 direções. Sem `directions: ["east"]` o template custaria 8 gerações.

### O que ficou

Ciclo de 8 quadros a **16 FPS** (era 9 a 13). O FPS não é gosto: a 6 un/s do
`PlayerController`, um ciclo de 0,50 s cobre 3 un, ou seja **1,5 un por passada** — coerente
com um personagem de 1,4 un. Fora dessa faixa o pé patina no chão, que é o outro jeito de uma
corrida parecer caminhada acelerada.

### Segunda rodada: os braços não tinham se mexido

A primeira Receita B parecia resolvida e não estava. A queixa foi "precisa movimentar mais as
pernas e os braços". Medindo, ela estava certa **e era específica**: as pernas tinham melhorado
muito, os braços não tinham mudado nada.

`Tools/motion-metrics.js` transforma isso em número, e o que o faz funcionar é a paleta. A
rampa **Pele** acha antebraço e punho; medir a silhueta inteira mediria a **capa**, que esvoaça
sozinha e daria movimento de braço onde não há. O número que importa é o **curso do punho** —
deslocamento do centro de massa da pele do tronco ao longo do ciclo — porque o braço da corrida
se move na diagonal (punho do quadril ao queixo) e só a extensão horizontal perderia metade.

| Ciclo | Curso do punho | Abertura das pernas | Energia entre quadros |
|---|---|---|---|
| Caminhada (Receita A) | 2,4 | 14 | 87 |
| Receita B, 1ª tentativa | 5,9 | 26 | 104 |
| **Aceito (`Run_v5_anatomico`)** | **9,3** | **27** | 101 |
| Template recusado | 4,1 | 10 | 117 |
| Idle, como piso | 1,7 | 2 | 27 |

**O que destravou os braços foi TIRAR texto, não acrescentar.** A primeira descrição terminava
em "o corpo permanece em pé" e "a cabeça permanece de perfil voltada à direita" — cláusulas de
amortecimento que o modelo obedeceu melhor que o pedido de correr. A versão aceita mantém só a
âncora do equipamento e descreve anatomicamente o que cada membro faz: *front knee lifted up to
hip height with the shin hanging down, rear leg extended far behind with the heel kicked up
toward the buttock, front arm bent at a right angle with the fist raised to chin height, rear
arm bent and driven back past the hip*.

Isto **contradiz em parte a seção 6**: a Receita B pede "dizer explicitamente o que o corpo não
deve fazer", e foi exatamente isso que travou a corrida. A regra fina é: proibição serve para
impedir uma **deformação** (o Fall virando voo horizontal), não para fixar uma **postura** — o
segundo caso vira freio de amplitude.

### Duas correções que os números reprovaram

**Semear a interpolação com uma pose de corrida pronta.** Como o v3 parte da pose de referência,
os primeiros quadros saem mansos; passar `custom_start_frame_base64` com um quadro de corrida
parecia a correção óbvia. Saiu o oposto: o ciclo **congelou** em volta da semente — energia 88,
a mesma da caminhada, com os 8 quadros quase iguais. A amplitude nasce da viagem entre a pose
parada e a extrema; começar na extrema elimina a viagem. Preservado em
`_descartados/Run_v7_semente_congelada/`.

**Cortar os três primeiros quadros.** A emenda do quadro 3 ao 7 fecha melhor que o ciclo inteiro
(razão 0,71 contra 1,30 — pela regra da seção 9, emenda contra a mediana das transições
internas). Mas medir a **abertura entre os pés** mostra dois picos, nos quadros 3 e 7: são o
mesmo momento da passada, e é por isso que a emenda fecha tão bem. Repetir só esse trecho daria
uma perna sempre à frente — manqueira. Os oito quadros ficaram.

Fica a lição: **a métrica de emenda sozinha engana.** Ela mede continuidade, não fase. Antes de
cortar quadros de um ciclo de locomoção, contar as passadas.

### A conta da passada depende de contar as passadas

`LocomotionProbe` divide por 2 passadas por ciclo. Verificado, não assumido: a abertura entre
os pés tem dois picos na caminhada, na 1ª Receita B e no ciclo aceito. (O template tinha três —
mais um motivo para não ter entrado.) A medida usa só os pés, nunca o centro do corpo: a capa
arrasta para trás e envenena qualquer medida ancorada no tronco.

### O sprint reusa este ciclo — o FPS deixou de ser constante

**2026-09-05, no mesmo dia.** Correr é `Sprint` no mapa Player (**Shift** ou **R1** /
`<Gamepad>/rightShoulder`; os gatilhos já eram escudo e arco). Modelado como `type: Value` +
`expectedControlType: Button`, igual a `Crouch` e `Shield`, que é como o projeto faz botão
segurado. Velocidade 1,5×: 6 → 9 un/s. Agachar vence o sprint em vez de os dois se
multiplicarem.

**Não há segunda folha de arte.** O `SpriteAnimator` ganhou `PlaybackScale`, e o
`PlayerAnimator` a define como `velocidade / MaxSpeed` enquanto o estado é Run. A passada sai
constante em qualquer velocidade — é o que mantém o pé cravado no chão:

| | Velocidade | Ritmo | Passada |
|---|---|---|---|
| Corrida normal | 6,0 un/s | 16 FPS | **1,50 un** |
| Sprint | 9,0 un/s | 24 FPS | **1,50 un** |

Com FPS fixo o sprint teria passada de 2,25 un e o pé escorregaria — a mesma leitura de
"andando rápido" que a corrida acabou de deixar de ter. O efeito colateral é bem-vindo: sob a
sonolência do lótus (`SpeedMultiplier`) a corrida agora fica pesada em vez de acelerada no
lugar. `PlaybackScale` volta a 1 sozinho a cada troca de estado, para não vazar para um ataque
e desencontrar o clipe do `lockTimer`.

Verificado por `LocomotionProbe.Medir` — ele lê velocidade, multiplicador, FPS, limites e a
contagem real de quadros da folha, e falha se a passada variar mais que 2% entre a corrida
normal e o sprint. Nada disso é erro de compilação: mexer em `sprintMultiplier` ou no FPS do
Run pode voltar a soltar o pé do chão em silêncio.

O sprint entrou também no menu de remapeamento (`KeyRebindService`) e nos dois probes de
entrada. **`Crouch` continua de fora do menu de remapeamento** — falta anterior, não
introduzida aqui.

Revisão animada das três versões lado a lado: `corrida-odisseu.html`.

### Ferramentas novas

| | |
|---|---|
| `Tools/contact-strip.js` | tira de contato ampliada de um estado — um ciclo se julga pela pose de cada quadro, e não dá para ver abrindo um PNG por vez |
| `Tools/fit-character-state.js` | encaixa frames no formato da folha: paleta, pés em y=71, cabeça em x=42 |
| `Tools/splice-sheet-state.js` | troca **um** estado dentro da folha sem remontar o `.meta`, que é onde moram o GUID e os `internalID` que o prefab referencia |
