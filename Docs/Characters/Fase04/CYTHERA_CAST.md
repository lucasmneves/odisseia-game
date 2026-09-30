# Fase 04 — Citera: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Aprovado em 2026-09-29.
Nome do arquivo em inglês (`CYTHERA`); o projeto chama a cena de `Citera` e o cenário de `Cytera` —
mantidas as três grafias existentes, nada renomeado.

**Nenhuma geração no PixelLab nesta fase.** Tudo foi reutilizado.

## 1. CastProbe

```
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CastProbe.Run
          -probeScene Assets/Scenes/Levels/Level_04_Citera.unity
```

| Item | Achado |
|---|---|
| Personagens com figura | **só o Odisseu** |
| Inimigos | **nenhum** (`EnemyController` ausente) — portanto nenhum inimigo carmesim a trocar |
| NPCs | nenhum (`NPCDialogue` ausente) |
| Animator / AnimatorController | 0 |
| Gatilhos de diálogo sem figura | `StormDialogue_1` (x = 4) e `StormDialogue_2` (x = 19) |
| Perigos da fase | `StormHazard` ×3, `TidalHazard` ×3, `WindZone`, `MovingPlatform` ×4, `WhirlpoolVisual` — nenhum com figura de personagem |

Falas da fase (`LocalizationTable`, falantes na cena):

| Fala | Falante |
|---|---|
| Intro 0–1 | narrador |
| Intro 2: "Segurem-se! Não deixem o navio virar!" | **Odisseu, falando com a tripulação** |
| Storm 1: "As ondas estão ficando maiores, capitão!" | **"Companheiro"** |
| Storm 2: "O vento arrancou o leme!…" | Odisseu |
| Outro 0–1 | narrador |

**O furo que o probe revelou:** a fase fala com a tripulação e um tripulante responde, mas o navio
estava vazio — o "Companheiro" era só um nome na caixa de diálogo (o sistema não mostra retrato).

## 2. Inventário e decisão

| Personagem | Papel | Facção | Asset | Estados | Decisão |
|---|---|---|---|---|---|
| Odisseu | Player | Casa de Odisseu | `CHR_Odysseus` (master v1) | os 16 do master | **reutilizado**; nenhum estado de tempestade é acionado pelo código, então nenhuma variação |
| Marinheiro (azul) | Figura, convés de popa | tripulação de Ítaca | `CHR_Villager_Sailor` (Fase 01) | Idle | **reutilizado** |
| Remador (ocre) | Figura, convés de popa | tripulação de Ítaca | `CHR_Villager_Sailor_Ochre` (Fase 01, variação por cor) | Idle | **reutilizado** |
| Elpenor (oliva) | Figura, convés de proa | tripulação de Ítaca | `CHR_Villager_Sailor_Olive` (Fase 01, variação por cor) | Idle | **reutilizado** |

São **os mesmos homens** que o Odisseu recrutou na Fase 01 (`Recruit_Sailor`, `Recruit_Rower`,
`Recruit_Elpenor` usam essas folhas). Tripulação nova seria outra gente no navio.

### Facções
Só a **casa de Odisseu** (o Odisseu e a sua tripulação). Nenhuma facção nova: Citera é travessia, não
povo. **Carmesim não foi posto na tripulação** — ela já tem identidade estabelecida na Fase 01 (azul,
ocre, oliva), e trocar as cores agora quebraria a continuidade com os mesmos homens. O carmesim segue
sendo do Odisseu.

### O que NÃO foi gerado, e por quê
- **Animação de "segurando-se na tempestade"** para a tripulação: seria a opção de maior impacto
  narrativo (1 geração por tripulante), mas nenhum código a aciona — as figuras só tocam o estado
  padrão (Idle). Registrado como opção se o playtest pedir.
- **Variações do Odisseu para a tempestade**: nenhum estado novo é acionado.
- **Inimigos, gregos, NPCs de Citera**: não existem na fase.

## 3. Integração

`Assets/Scripts/Editor/CyteraCastDresser.cs`:
- `CyteraCastDresser.Dress` — põe os três tripulantes, idempotente.
- `CyteraCastDresser.Remove` — tira o elenco (usado para provar que a linha magenta não é dos personagens).

Regras aplicadas:
- **Altura pelo colisor do convés**, não por número digitado: os pés ficam no topo do `BoxCollider2D`
  do `Deck_Popa` / `Deck_Proa` (y = −2,00 medido). O convés é plataforma de gameplay.
- **Guarda do ponto de nascimento:** nenhum tripulante a menos de 1,5 un de onde o Odisseu nasce.
  A primeira versão os punha em cima dele (x = −16,4 e −14,6 contra o nascimento em −15); a guarda
  recusou, e eles foram para a ponta da popa (−17,2) e para a frente do convés (−8,5, olhando para o
  jogador que chega). Elpenor na proa (39,0) olhando para quem chega.
- Sem colisor, sem script de gameplay, ordem 0 (atrás do jogador), FPS do Idle variando pela posição.
- **Nenhum prefab tocado**; nenhum override de inimigo (não há inimigo).

## 4. Validação

| Teste | Resultado |
|---|---|
| CastProbe | 3 tripulantes a 42,857 px/un, escala 1, 1,45 un de altura |
| `PlaceholderProbe` (Citera) | nada visível, nada inativo |
| `CampaignValidation` | passa |
| Compilação | 0 erros |
| Build WebGL | ver relatório |
| Capturas (`Fase04/_capturas/`) | pés exatamente no convés; legíveis contra o cinza da tempestade; a chuva passa por cima deles |

**Não testado:** play mode, navegador, mobile, gamepad.

## 5. Problemas encontrados

| Problema | Situação |
|---|---|
| Navio vazio com tripulação falando | **corrigido** — três tripulantes da Fase 01 |
| Primeira posição dos tripulantes caía no ponto de nascimento | **corrigido** — guarda no dresser |
| **Linha magenta de 1 px na altura do mar** (linha 432 da captura, 127 a 666 px por quadro) | **não é dos personagens**: capturada de novo SEM o elenco, contagem idêntica. As capturas de cenário de 06/09 tinham 0 → regressão do cenário posterior a essa data. Fora do escopo; tarefa separada aberta |
| Odisseu aparece acima do convés nas capturas | a captura é feita no ponto de nascimento, antes da gravidade; já era assim nas capturas antigas |

## 6. Custo

**0 gerações** (669 de 2000, inalterado). 0 descartes.
