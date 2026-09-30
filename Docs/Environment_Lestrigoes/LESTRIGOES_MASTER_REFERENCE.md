# LESTRIGOES_ENVIRONMENT_MASTER — Fase 07_Lestrigoes

**Conceito A — Baía entre falésias — aprovado em 2026-09-06.**
Paleta: `Palette/LESTRIGOES_PALETTE.gpl` · 11 rampas, 44 cores

> Costa de pedra fria, grande demais para quem chega. É a fase mais fria do jogo e a mais
> difícil de acertar — ver §6.

---

## 1. Identidade aprovada

| | Resultado |
|---|---|
| **A — Baía** | **Aprovado.** Falésias colunares enormes, água escura, barcos minúsculos entre elas. O único dos três livre de arquitetura medieval, e o que estabelece a escala sem nenhum objeto. |
| B — Porto | Recusado como direção: as construções saíram em **enxaimel europeu com telhado de duas águas**. O vocabulário portuário — cais, píer, cabos, barris — foi aproveitado. |
| C — Vale fortificado | Recusado: **castelo medieval** com ameias, seteiras e telhado de telha. |

A arquitetura da cidade não veio do conceito: veio do vocabulário que Cícones já tinha
estabelecido — alvenaria de blocos irregulares, telhado plano sobre vigas aparentes, verga reta
— aplicado em escala monumental. Foi a única forma de manter a fase grega em vez de europeia.

**A torre de vigia veio com ameias** e foi corrigida por recorte, não por nova geração: o
corpo estava certo e o defeito era local, que é a regra do projeto. **A escada veio com uma
pessoa desenhada nela** e teve de ser regerada com "nobody on the steps".

---

## 2. Escala sem mexer no personagem

O briefing pede que o lugar pareça grande demais para o Odisseu. Isso é feito por três coisas,
nenhuma delas alterando o personagem:

1. a falésia de fundo tem **5,97 un**, contra 4,11 em Cícones, e sobe além do topo do quadro;
2. a muralha atravessa o trecho inteiro da cidade acima da linha do olhar;
3. o navio dos Lestrigões é o **barco de Cícones a 2,4×**, com o mesmo barco a 0,75× ao lado —
   é a comparação lado a lado que transmite a escala, não o tamanho absoluto de nenhum dos dois.

---

## 3. Paleta

Onze rampas. **A rampa `Sombra` foi cortada na validação**: os dois passos escuros dela já
existiam em `Pedra clara` e em `Contorno`, e cor repetida entre rampas confunde o classificador
— ele não saberia a que material o pixel pertence. O escuro da fase sai de `Falésia` e do
contorno.

Compartilhadas: **Contorno** e **Bronze** (personagem), **Fogo** e **Fumaça** (Troia).

O classificador **não corta por matiz** entre as pedras, de propósito: aqui o quente e o frio
são o mesmo material sob luz diferente, e separá-los por matiz partiria uma falésia ao meio. A
decisão é só por luminância.

---

## 4. Perspectiva aérea — a correção que a fase cobrou

A paleta é quase monocromática, e o efeito colateral foi grave: falésia de fundo, muralha de
meio e chão jogável mediam luminância parecida e **a fase virou uma massa cinza única**, sem
separação entre planos. O briefing põe a leitura do gameplay como prioridade máxima.

A correção é um **tint por renderer**: cada camada é puxada para a cor do céu na proporção do
seu fator de parallax. Nenhum asset novo, nenhum shader, nenhum sistema.

```
Distancia(fator) = lerp(branco, #c8dadf, clamp01((fator − 0,35) × 1,25))
```

Ela virou a regra para as fases seguintes — Circe já nasceu com ela.

---

## 5. Reuso

Doze assets vêm de outras fases em vez de serem gerados: barco (Cícones), barricada, entulho,
fumaça e fogueira (Troia), destroços e mastro (Cytera). O mastro marca a saída, porque a fuga
de Lestrigões é o reembarque.

**Os gigantes já existem na cena** como placeholders de personagem (`Giant`, 2,0 × 3,0 un em
x=8 e x=22). O briefing proíbe gerar personagens; ficam registrados no `PlaceholderProbe` com
essa justificativa, para o probe não mascarar a pendência.

---

## 6. Honestidade sobre o resultado

**Esta é a fase mais fraca das sete vestidas.** Mesmo depois da perspectiva aérea, a leitura
dos planos é a mais difícil do jogo, e a sensação de "grande demais" não chega com a força que
o briefing pede. A causa é a mesma da qualidade da atmosfera: uma paleta de onze rampas em que
oito são cinza.

O caminho de correção, se for retomado, é **injetar matiz** — musgo verde nas pedras molhadas,
madeira mais quente no porto, um céu menos lavado — e não mais objetos.
