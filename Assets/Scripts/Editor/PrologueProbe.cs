using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Odisseia.Levels;
using Odisseia.Systems;
using Odisseia.UI;

/// <summary>
/// Confere que o prólogo de Ítaca é jogável do início ao fim, sem abrir o Editor:
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod PrologueProbe.Run
///
/// A pergunta que importa não é "a cena carrega?", e sim "existe algum ato que o
/// jogador não consegue fechar?". Por isso a checagem central conta quantos
/// reportadores (NPCs, pontos de interação, o curso de treino) estão de fato ligados a
/// cada objetivo e compara com o número que ele exige: um objetivo que pede 10 e tem 9
/// NPCs ligados trava a fase para sempre, e é um erro invisível no Inspector.
///
/// Também confere que toda fala tem texto nos dois idiomas — uma chave sem entrada na
/// tabela apareceria como "dlg.Level_01..." na tela.
/// </summary>
public static class PrologueProbe
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_01_Itaca_Prologue.unity";

    /// <summary>Layer "Ground", a mesma que o PlayerController usa para achar o chão.</summary>
    private const int LayerChao = 8;

    private static readonly List<string> Falhas = new List<string>();

    [MenuItem("Odisseia/Conferir prologo de Itaca")]
    public static void Run()
    {
        Falhas.Clear();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        List<LevelObjective> cadeia = ConferirCadeiaDeAtos();
        ConferirReportadores(cadeia);
        ConferirDialogos();
        ConferirTreinamento();
        ConferirChao();
        ConferirPortoes();
        ConferirApoio();
        ConferirJogador();
        ConferirLimites();
        ConferirAlcance(cadeia);
        ConferirPartida();

        if (Falhas.Count == 0)
        {
            Debug.Log("[PrologueProbe] OK — cadeia de atos completa, falas traduzidas, treino e partida ligados.");
            Encerrar(0);
            return;
        }

        foreach (string falha in Falhas)
        {
            Debug.LogError("[PrologueProbe] " + falha);
        }

        Debug.LogError("[PrologueProbe] " + Falhas.Count + " problema(s).");
        Encerrar(1);
    }

    /// <summary>
    /// Encerra o processo só quando ele é o batchmode. Chamado pelo menu, dentro do
    /// Editor aberto, EditorApplication.Exit fecharia o Unity do desenvolvedor na cara
    /// dele — o script é o mesmo, o contexto é que muda.
    /// </summary>
    private static void Encerrar(int codigo)
    {
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(codigo);
        }
    }


    private static void Falhar(string mensagem) => Falhas.Add(mensagem);

    // ------------------------------------------------------------------ atos

    /// <summary>Percorre os objetivos do primeiro ao último, na ordem em que se abrem.</summary>
    private static List<LevelObjective> ConferirCadeiaDeAtos()
    {
        LevelObjective[] todos = Object.FindObjectsByType<LevelObjective>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var cadeia = new List<LevelObjective>();

        LevelObjective atual = null;
        foreach (LevelObjective obj in todos)
        {
            if (Campo<bool>(obj, "startsActive"))
            {
                if (atual != null)
                {
                    Falhar("mais de um objetivo marcado como inicial");
                }

                atual = obj;
            }
        }

        if (atual == null)
        {
            Falhar("nenhum objetivo começa ativo — a fase abriria sem primeiro ato");
            return cadeia;
        }

        var vistos = new HashSet<LevelObjective>();
        while (atual != null)
        {
            if (!vistos.Add(atual))
            {
                Falhar("cadeia de atos com ciclo em " + atual.name);
                break;
            }

            cadeia.Add(atual);

            string chave = Campo<string>(atual, "titleKey");
            if (!Localization.Has(chave))
            {
                Falhar(atual.name + ": chave de objetivo sem tradução (" + chave + ")");
            }

            if (Campo<ObjectiveBanner>(atual, "banner") == null)
            {
                Falhar(atual.name + ": sem faixa de objetivo");
            }

            atual = Campo<LevelObjective>(atual, "next");
        }

        if (cadeia.Count != todos.Length)
        {
            Falhar("há " + todos.Length + " objetivos na cena, mas só " + cadeia.Count + " estão na cadeia");
        }

        Debug.Log("[PrologueProbe] atos na ordem: " + string.Join(" -> ", cadeia.ConvertAll(o => o.name)));
        return cadeia;
    }

    /// <summary>
    /// Cada objetivo precisa de pelo menos tantos reportadores quanto exige. É a
    /// checagem que garante que nenhum ato fica impossível de fechar.
    /// </summary>
    private static void ConferirReportadores(List<LevelObjective> cadeia)
    {
        var contagem = new Dictionary<LevelObjective, int>();

        foreach (var npc in Object.FindObjectsByType<NPCDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Somar(contagem, Campo<LevelObjective>(npc, "objective"));

            if (Campo<DialogueSequence>(npc, "dialogue") == null)
            {
                Falhar(npc.name + ": NPC sem diálogo");
            }
        }

        foreach (var ponto in Object.FindObjectsByType<InteractPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Somar(contagem, Campo<LevelObjective>(ponto, "objective"));

            string chave = Campo<string>(ponto, "resultKey");
            if (!string.IsNullOrEmpty(chave) && !Localization.Has(chave))
            {
                Falhar(ponto.name + ": mensagem sem tradução (" + chave + ")");
            }
        }

        foreach (var curso in Object.FindObjectsByType<TrainingCourse>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Somar(contagem, Campo<LevelObjective>(curso, "objective"));
        }

        foreach (LevelObjective obj in cadeia)
        {
            int exigido = Campo<int>(obj, "required");
            contagem.TryGetValue(obj, out int disponivel);

            // O último ato fecha pela cena de partida, não por relato.
            bool ultimo = Campo<LevelObjective>(obj, "next") == null;

            if (disponivel < exigido && !ultimo)
            {
                Falhar(obj.name + ": exige " + exigido + " mas só " + disponivel +
                       " objeto(s) reportam para ele — o ato ficaria impossível");
            }
        }
    }

    private static void Somar(Dictionary<LevelObjective, int> contagem, LevelObjective obj)
    {
        if (obj == null)
        {
            return;
        }

        contagem.TryGetValue(obj, out int atual);
        contagem[obj] = atual + 1;
    }

    // ------------------------------------------------------------------ falas

    private static void ConferirDialogos()
    {
        int linhas = 0;

        foreach (var seq in Object.FindObjectsByType<DialogueSequence>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var so = new SerializedObject(seq);

            if (so.FindProperty("panel").objectReferenceValue == null
                || so.FindProperty("bodyText").objectReferenceValue == null)
            {
                Falhar(seq.name + ": sequência sem painel de texto");
            }

            if (so.FindProperty("inputActions").objectReferenceValue == null)
            {
                Falhar(seq.name + ": sem asset de controles — não daria para avançar a fala");
            }

            SerializedProperty lista = so.FindProperty("lines");
            if (lista.arraySize == 0)
            {
                Falhar(seq.name + ": sequência sem falas");
            }

            for (int i = 0; i < lista.arraySize; i++)
            {
                string chave = lista.GetArrayElementAtIndex(i).FindPropertyRelative("key").stringValue;

                if (!Localization.Has(chave))
                {
                    Falhar(seq.name + ": fala sem tradução (" + chave + ")");
                    continue;
                }

                string falante = lista.GetArrayElementAtIndex(i).FindPropertyRelative("speaker").stringValue;
                if (!string.IsNullOrEmpty(falante) && !Localization.Has("speaker." + falante))
                {
                    Falhar(seq.name + ": personagem sem nome traduzido (speaker." + falante + ")");
                }

                linhas++;
            }
        }

        Debug.Log("[PrologueProbe] falas conferidas: " + linhas + " linhas com texto nos dois idiomas.");
    }

    // ------------------------------------------------------------------ treino

    private static void ConferirTreinamento()
    {
        var curso = Object.FindAnyObjectByType<TrainingCourse>(FindObjectsInactive.Include);
        if (curso == null)
        {
            Falhar("sem TrainingCourse na cena — a fase não teria tutorial de combate");
            return;
        }

        var so = new SerializedObject(curso);

        foreach (string campo in new[] { "combat", "shield", "bow", "prompt", "inputActions", "objective" })
        {
            if (so.FindProperty(campo).objectReferenceValue == null)
            {
                Falhar("TrainingCourse: campo '" + campo + "' não ligado");
            }
        }

        SerializedProperty etapas = so.FindProperty("steps");
        var cobertas = new HashSet<int>();

        for (int i = 0; i < etapas.arraySize; i++)
        {
            SerializedProperty etapa = etapas.GetArrayElementAtIndex(i);
            string chave = etapa.FindPropertyRelative("instructionKey").stringValue;
            int acao = etapa.FindPropertyRelative("action").enumValueIndex;
            cobertas.Add(acao);

            if (!Localization.Has(chave))
            {
                Falhar("treino, etapa " + i + ": instrução sem tradução (" + chave + ")");
            }

            if (etapa.FindPropertyRelative("required").floatValue <= 0f)
            {
                Falhar("treino, etapa " + i + ": exige zero repetições");
            }

            // Etapas que dependem de acertar algo precisam ter o que acertar.
            var acaoTipo = (TrainingAction)acao;

            if (acaoTipo == TrainingAction.Block)
            {
                ConferirParceiroDeTreino(etapa, i);
            }

            if (acaoTipo == TrainingAction.SwordHit || acaoTipo == TrainingAction.ArrowHit
                || acaoTipo == TrainingAction.Block)
            {
                if (!TemAlvoDisponivel(etapas, i, acaoTipo))
                {
                    Falhar("treino, etapa " + i + " (" + acaoTipo + "): nenhum alvo/parceiro é ligado " +
                           "até aqui — a etapa não teria como ser cumprida");
                }
            }
        }

        foreach (TrainingAction obrigatoria in new[]
                 {
                     TrainingAction.Move, TrainingAction.Jump, TrainingAction.Attack,
                     TrainingAction.SwordHit, TrainingAction.Block, TrainingAction.Bow,
                     TrainingAction.ArrowHit,
                 })
        {
            if (!cobertas.Contains((int)obrigatoria))
            {
                Falhar("treino: nenhuma etapa ensina " + obrigatoria);
            }
        }

        Debug.Log("[PrologueProbe] treino: " + etapas.arraySize + " etapas cobrindo movimento, pulo, espada, escudo e arco.");
    }

    /// <summary>
    /// O parceiro da etapa do escudo precisa IR até o jogador.
    ///
    /// Esta é a falha que a etapa do escudo teve: o soldado ficava a trinta unidades
    /// dali, fora da tela, com o raio de detecção padrão de 4. A instrução dizia
    /// "segure o botão para levantar o escudo", o jogador segurava, e nada nunca o
    /// atingia — porque a etapa não fecha ao levantar o escudo, e sim ao BLOQUEAR um
    /// golpe. Um parceiro que espera ser encontrado transforma a lição numa caça ao
    /// tesouro sem pista.
    ///
    /// O campo de treino tem ~80 unidades. Exigir que o parceiro cubra pelo menos 20
    /// garante que ele apareça venha o jogador de onde vier na etapa anterior.
    /// </summary>
    private static void ConferirParceiroDeTreino(SerializedProperty etapa, int indice)
    {
        const float alcanceMinimo = 20f;
        SerializedProperty ligar = etapa.FindPropertyRelative("enableOnStart");
        bool achou = false;

        for (int j = 0; j < ligar.arraySize; j++)
        {
            var go = ligar.GetArrayElementAtIndex(j).objectReferenceValue as GameObject;
            var inimigo = go != null ? go.GetComponent<Odisseia.Enemies.EnemyController>() : null;

            if (inimigo == null)
            {
                continue;
            }

            achou = true;
            float deteccao = Campo<float>(inimigo, "detectionRadius");
            float desistencia = Campo<float>(inimigo, "loseTargetRadius");

            if (deteccao < alcanceMinimo)
            {
                Falhar("treino, etapa " + indice + ": o parceiro de treino enxerga só " +
                       deteccao.ToString("0.0") + " unidades — ele espera ser encontrado " +
                       "em vez de vir, e a etapa fica sem ninguém para bloquear");
            }

            if (desistencia <= deteccao)
            {
                Falhar("treino, etapa " + indice + ": o parceiro desiste (" +
                       desistencia.ToString("0.0") + ") antes do alcance em que enxerga (" +
                       deteccao.ToString("0.0") + ") — ele largaria a perseguição no mesmo frame");
            }
        }

        if (!achou)
        {
            Falhar("treino, etapa " + indice + ": etapa de defesa sem parceiro de treino ligado");
        }
    }

    /// <summary>
    /// Houve algum alvo (ou parceiro de treino) ligado até esta etapa, inclusive? Basta
    /// um objeto ligado em qualquer etapa anterior ou nesta.
    /// </summary>
    private static bool TemAlvoDisponivel(SerializedProperty etapas, int ate, TrainingAction acao)
    {
        for (int i = 0; i <= ate; i++)
        {
            SerializedProperty ligar = etapas.GetArrayElementAtIndex(i).FindPropertyRelative("enableOnStart");

            for (int j = 0; j < ligar.arraySize; j++)
            {
                var go = ligar.GetArrayElementAtIndex(j).objectReferenceValue as GameObject;
                if (go == null)
                {
                    continue;
                }

                if (acao == TrainingAction.Block)
                {
                    if (go.GetComponent<Odisseia.Enemies.EnemyController>() != null)
                    {
                        return true;
                    }

                    continue;
                }

                var alvo = go.GetComponent<TrainingTarget>();
                if (alvo != null && Campo<int>(alvo, "reportAction") == (int)acao)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // ------------------------------------------------------------------ chão

    /// <summary>
    /// Procura buraco no chão onde o jogador ainda não sabe pular.
    ///
    /// Existe porque essa foi exatamente a falha que passou batido: o caminho do
    /// recrutamento tinha vãos de 3,5 unidades, e o pulo só é ensinado no ato
    /// seguinte. Quem caía perdia uma vida; três quedas encerravam a jornada e
    /// mandavam o jogador para o menu principal. A cena "carregava", o roteiro estava
    /// certo, e mesmo assim a fase era impossível de terminar.
    ///
    /// O alcance de um pulo em velocidade cheia é ~4,9 unidades, mas o limite aqui é
    /// bem mais baixo de propósito: antes da lição de pulo o número aceitável é zero.
    /// </summary>
    private static void ConferirChao()
    {
        const float inicioDaFase = 0f;
        const float comecoDoTreino = 154f;
        const float alturaDoChao = -2f;

        var faixas = new List<(float esquerda, float direita)>();

        foreach (var col in Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            // Objeto desligado devolve bounds zerado, que viraria uma faixa falsa em x = 0.
            if (col.isTrigger || col.gameObject.layer != LayerChao || !col.gameObject.activeInHierarchy)
            {
                continue;
            }

            Bounds b = col.bounds;

            // Só o piso: plataformas suspensas ficam de fora, senão um vão real seria
            // "tapado" por uma plataforma dois metros acima dele.
            if (b.max.y < alturaDoChao - 0.6f || b.max.y > alturaDoChao + 0.6f)
            {
                continue;
            }

            faixas.Add((b.min.x, b.max.x));
        }

        if (faixas.Count == 0)
        {
            Falhar("nenhum piso encontrado na fase");
            return;
        }

        faixas.Sort((a, b) => a.esquerda.CompareTo(b.esquerda));

        float alcance = faixas[0].direita;
        int buracos = 0;

        if (faixas[0].esquerda > inicioDaFase)
        {
            Falhar("a fase começa sem chão sob o jogador");
        }

        for (int i = 1; i < faixas.Count; i++)
        {
            (float esquerda, float direita) = faixas[i];

            if (esquerda > alcance + 0.05f)
            {
                buracos++;
                float vao = esquerda - alcance;
                string onde = "x " + alcance.ToString("0.0") + " a " + esquerda.ToString("0.0");

                if (alcance < comecoDoTreino)
                {
                    Falhar("buraco de " + vao.ToString("0.0") + " unidades em " + onde +
                           ", ANTES de o pulo ser ensinado — cair custa uma vida numa " +
                           "habilidade que a fase ainda não apresentou");
                }
                else if (vao > 3f)
                {
                    Falhar("buraco de " + vao.ToString("0.0") + " unidades em " + onde +
                           " (o pulo alcança 4,9 em velocidade cheia; acima de 3 não " +
                           "perdoa erro de tempo)");
                }
            }

            alcance = Mathf.Max(alcance, direita);
        }

        string trecho = faixas[0].esquerda.ToString("0.0") + " a " + alcance.ToString("0.0");
        Debug.Log(buracos == 0
            ? "[PrologueProbe] chão contínuo de " + trecho + "."
            : "[PrologueProbe] chão de " + trecho + " com " + buracos + " buraco(s).");
    }

    // ------------------------------------------------------------------ portões

    /// <summary>
    /// Toda parede que segura o jogador precisa dizer por que está ali.
    ///
    /// Sem isso a fase parece travada mesmo estando certa: o jogador que passou direto
    /// por Penélope encontra um portão fechado, lê "fale com Penélope" no alto da tela,
    /// não faz ideia de que ela ficou para trás, e conclui que o jogo bugou. Uma parede
    /// muda é indistinguível de um bug.
    /// </summary>
    private static void ConferirPortoes()
    {
        const float alturaDeParede = 3f;
        int portoes = 0;

        foreach (var col in Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (col.isTrigger || col.gameObject.layer != LayerChao || !col.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (col.bounds.size.y < alturaDeParede)
            {
                continue;
            }

            var portao = col.GetComponentInParent<ProgressionGate>();

            // As paredes das pontas do mundo não são portões: elas nunca abrem, e é
            // essa a função delas.
            if (col.name.StartsWith("Boundary_"))
            {
                continue;
            }

            portoes++;

            if (portao == null)
            {
                Falhar(col.name + ": parede sem ProgressionGate — nada define quando ela abre");
                continue;
            }

            if (Campo<LevelObjective>(portao, "requiredObjective") == null)
            {
                Falhar(portao.name + ": portão sem objetivo exigido — abriria de cara");
            }

            if (Campo<Collider2D>(portao, "blockCollider") == null)
            {
                Falhar(portao.name + ": portão sem colisor de barreira");
            }

            if (Campo<Collider2D>(portao, "noticeArea") == null)
            {
                Falhar(portao.name + ": portão sem área de aviso — não teria como reavaliar ao ser tocado");
            }

            string chave = Campo<string>(portao, "lockedMessageKey");
            if (!Localization.Has(chave))
            {
                Falhar(portao.name + ": explicação sem tradução (" + chave + ")");
            }

            if (Campo<TutorialPrompt>(portao, "prompt") == null)
            {
                Falhar(portao.name + ": explicação sem onde aparecer");
            }
        }

        if (portoes == 0)
        {
            Falhar("nenhum portão encontrado — os atos não estariam em ordem");
        }

        Debug.Log("[PrologueProbe] portões: " + portoes + ", todos explicando o que falta.");
    }

    // ------------------------------------------------------------------ apoio no chão

    /// <summary>
    /// Nenhuma figura pode estar flutuando nem enterrada.
    ///
    /// Essa foi a falha: a arte de personagem é importada com pivô BottomCenter e o
    /// quadrado placeholder com pivô central. O código posicionava todo mundo "meia
    /// altura acima do chão", conta que só vale para pivô central — e cada NPC pintado
    /// nascia meia altura no ar. É o tipo de erro que nenhuma checagem de roteiro pega,
    /// porque a fase continua perfeitamente jogável; ela só fica errada de olhar.
    ///
    /// A conta certa vem do sprite: a base do desenho tem que encostar na linha do chão.
    /// </summary>
    private static void ConferirApoio()
    {
        const float alturaDoChao = -2f;
        const float tolerancia = 0.12f;

        int figuras = 0;
        float piorDesvio = 0f;

        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            // "Body" é o nome que o builder dá ao desenho de toda figura em pé.
            if (sr.gameObject.name != "Body" || !sr.gameObject.activeInHierarchy)
            {
                continue;
            }

            // Só o que o builder posiciona. O Player fica de fora por dois motivos: a
            // origem do prefab dele está nos PÉS (o colisor é que sobe, com offset), e
            // o desenho é trocado em runtime pelo SpriteAnimator — o sprite que está
            // no Editor não é o que aparece no jogo. Medir isso daria alarme falso.
            if (!EstaDentroDoMundo(sr.transform))
            {
                continue;
            }

            figuras++;
            float baseDoDesenho = sr.bounds.min.y;
            float desvio = baseDoDesenho - alturaDoChao;

            if (Mathf.Abs(desvio) > Mathf.Abs(piorDesvio))
            {
                piorDesvio = desvio;
            }

            if (Mathf.Abs(desvio) > tolerancia)
            {
                Falhar(sr.transform.parent.name + ": figura " +
                       (desvio > 0f ? "flutuando " : "enterrada ") +
                       Mathf.Abs(desvio).ToString("0.00") + " unidade(s) — a base do desenho " +
                       "está em " + baseDoDesenho.ToString("0.00") + " e o chão em " + alturaDoChao);
            }
        }

        if (figuras == 0)
        {
            Falhar("nenhuma figura encontrada na fase");
            return;
        }

        Debug.Log("[PrologueProbe] " + figuras + " figuras apoiadas no chão (pior desvio: " +
                  piorDesvio.ToString("0.00") + ").");
    }

    /// <summary>O objeto está sob a raiz "World", montada pelo PrologueSceneBuilder?</summary>
    private static bool EstaDentroDoMundo(Transform alvo)
    {
        for (Transform t = alvo; t != null; t = t.parent)
        {
            if (t.name == "World")
            {
                return true;
            }
        }

        return false;
    }

    // ------------------------------------------------------------------ jogador

    /// <summary>
    /// Odisseu precisa estar visível para os inimigos.
    ///
    /// Esta checagem existe por causa de uma falha que não deixou nenhum rastro: a
    /// instância do Player do prólogo vinha com o disfarce da Fase 14 ligado, o que
    /// move o jogador para uma layer que os <c>EnemyController</c> não procuram. O
    /// disfarce funciona exatamente como anunciado — e por isso o combate inteiro da
    /// fase deixava de existir em silêncio. A etapa do escudo, que só fecha ao
    /// bloquear um golpe, era impossível por construção.
    ///
    /// Regra: numa fase com inimigos, o jogador tem que estar numa layer que a máscara
    /// deles inclua.
    /// </summary>
    private static void ConferirJogador()
    {
        int problemas = Falhas.Count;
        var jogador = GameObject.FindGameObjectWithTag("Player");
        if (jogador == null)
        {
            Falhar("sem Player na cena");
            return;
        }

        var disfarce = jogador.GetComponent<Odisseia.Player.DisguiseEffect>();
        if (disfarce != null && Campo<bool>(disfarce, "startDisguised"))
        {
            Falhar("Player começa DISFARÇADO — nenhum inimigo consegue detectá-lo, " +
                   "e a etapa do escudo fica impossível");
        }

        var inimigos = Object.FindObjectsByType<Odisseia.Enemies.EnemyController>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (inimigos.Length == 0)
        {
            return;
        }

        foreach (var inimigo in inimigos)
        {
            LayerMask mascara = Campo<LayerMask>(inimigo, "playerLayer");

            if ((mascara.value & (1 << jogador.layer)) == 0)
            {
                Falhar(inimigo.name + ": procura o jogador em outra layer — o Player está na " +
                       jogador.layer + " e a máscara não a inclui, então ele nunca é detectado");
            }
        }

        if (Falhas.Count == problemas)
        {
            Debug.Log("[PrologueProbe] jogador na layer " + jogador.layer + ", visível para os " +
                      inimigos.Length + " inimigo(s) da fase.");
        }
    }

    // ------------------------------------------------------------------ limites

    private static void ConferirLimites()
    {
        var limites = Object.FindAnyObjectByType<LevelBounds>(FindObjectsInactive.Include);
        if (limites == null)
        {
            Falhar("sem LevelBounds — nada impede o jogador de sair da fase");
            return;
        }

        foreach (string campo in new[] { "leftWall", "rightWall", "cameraFollow", "player" })
        {
            if (Campo<Object>(limites, campo) == null)
            {
                Falhar("LevelBounds: campo '" + campo + "' não ligado");
            }
        }

        float camMin = Campo<float>(limites, "cameraMinX");
        float camMax = Campo<float>(limites, "cameraMaxX");
        float camMinY = Campo<float>(limites, "cameraMinY");
        float camMaxY = Campo<float>(limites, "cameraMaxY");

        // Limite mais estreito que a própria tela faria o Clamp inverter e a câmera
        // travar num ponto só.
        const float alturaVisivel = 10f;
        const float larguraVisivel = 18f;

        if (camMax - camMin < larguraVisivel)
        {
            Falhar("LevelBounds: janela de câmera mais estreita que a tela");
        }

        if (camMaxY - camMinY < alturaVisivel)
        {
            Falhar("LevelBounds: janela de câmera mais baixa que a tela");
        }

        Debug.Log("[PrologueProbe] limites: área jogável " +
            Campo<float>(limites, "playableMinX").ToString("0.0") + " a " +
            Campo<float>(limites, "playableMaxX").ToString("0.0") +
            ", câmera " + camMin.ToString("0.0") + " a " + camMax.ToString("0.0") + ".");
    }

    // ------------------------------------------------------------------ alcance

    /// <summary>
    /// O que cada ato pede tem que estar do lado de cá do portão que aquele ato abre.
    ///
    /// É a garantia estrutural contra softlock. Um objetivo cujo alvo ficou ATRÁS de
    /// uma barreira que só abre quando esse mesmo objetivo fechar é um beco sem saída
    /// perfeito: o jogador anda até a parede, lê que precisa fazer algo, e o que ele
    /// precisa fazer está do outro lado. A fase compila, carrega, roda — e não tem fim.
    /// </summary>
    private static void ConferirAlcance(List<LevelObjective> cadeia)
    {
        int problemas = Falhas.Count;
        var portoes = new List<(float x, LevelObjective exige)>();

        foreach (var portao in Object.FindObjectsByType<ProgressionGate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            portoes.Add((portao.transform.position.x, Campo<LevelObjective>(portao, "requiredObjective")));
        }

        foreach (LevelObjective objetivo in cadeia)
        {
            int ordemDoAto = cadeia.IndexOf(objetivo);

            // Até onde dá para andar enquanto este ato está aberto: até o primeiro
            // portão que ainda não abriu, isto é, o que espera este ato ou um posterior.
            float limite = float.MaxValue;
            string culpado = "fim da fase";

            foreach ((float x, LevelObjective exige) in portoes)
            {
                int ordemDoPortao = exige != null ? cadeia.IndexOf(exige) : -1;

                if (ordemDoPortao >= ordemDoAto && x < limite)
                {
                    limite = x;
                    culpado = exige != null ? exige.name : "?";
                }
            }

            foreach ((string nome, float x) in Reportadores(objetivo))
            {
                if (x > limite)
                {
                    Falhar(objetivo.name + ": " + nome + " está em x=" + x.ToString("0.0") +
                           ", atrás do portão de " + culpado + " (x=" + limite.ToString("0.0") +
                           ") — o ato exigiria algo inalcançável");
                }
            }
        }

        if (Falhas.Count == problemas)
        {
            Debug.Log("[PrologueProbe] alcance: todo objetivo tem seus alvos deste lado do portão.");
        }
    }

    /// <summary>Quem consegue fechar um objetivo, e onde cada um está.</summary>
    private static List<(string nome, float x)> Reportadores(LevelObjective objetivo)
    {
        var lista = new List<(string, float)>();

        foreach (var npc in Object.FindObjectsByType<NPCDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (Campo<LevelObjective>(npc, "objective") == objetivo)
            {
                lista.Add((npc.name, npc.transform.position.x));
            }
        }

        foreach (var ponto in Object.FindObjectsByType<InteractPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (Campo<LevelObjective>(ponto, "objective") == objetivo)
            {
                lista.Add((ponto.name, ponto.transform.position.x));
            }
        }

        foreach (var curso in Object.FindObjectsByType<TrainingCourse>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (Campo<LevelObjective>(curso, "objective") == objetivo)
            {
                // O curso é um objeto sem lugar; o que importa é onde estão os alvos.
                foreach (var alvo in Object.FindObjectsByType<TrainingTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    lista.Add((alvo.name, alvo.transform.position.x));
                }
            }
        }

        return lista;
    }

    // ------------------------------------------------------------------ partida

    private static void ConferirPartida()
    {
        var partida = Object.FindAnyObjectByType<ShipDeparture>(FindObjectsInactive.Include);
        if (partida == null)
        {
            Falhar("sem ShipDeparture — a fase não teria como terminar");
            return;
        }

        var so = new SerializedObject(partida);
        foreach (string campo in new[] { "player", "playerLock", "ship", "odysseusOnDeck", "goal", "boardingDialogue" })
        {
            if (so.FindProperty(campo).objectReferenceValue == null)
            {
                Falhar("ShipDeparture: campo '" + campo + "' não ligado");
            }
        }

        var objetivo = Object.FindAnyObjectByType<LevelGoal>(FindObjectsInactive.Include);
        if (objetivo == null)
        {
            Falhar("sem LevelGoal na cena");
            return;
        }

        var goalSo = new SerializedObject(objetivo);
        string proxima = goalSo.FindProperty("nextSceneName").stringValue;

        // O prólogo volta para o mapa da jornada, não direto para Troia.
        if (proxima != SceneLoader.WorldMap)
        {
            Falhar("LevelGoal aponta para '" + proxima + "' e deveria voltar ao WorldMap");
        }

        if (goalSo.FindProperty("levelManager").objectReferenceValue == null)
        {
            Falhar("LevelGoal sem LevelManager — a fase não seria marcada como concluída");
        }

        var manager = Object.FindAnyObjectByType<LevelManager>(FindObjectsInactive.Include);
        if (manager == null || manager.LevelId != "Level_01_Itaca_Prologue")
        {
            Falhar("LevelManager ausente ou com id errado");
        }
    }

    // ------------------------------------------------------------------ leitura de campos

    /// <summary>
    /// Lê um campo serializado privado. A alternativa seria abrir tudo em público só
    /// para o teste enxergar, e aí a checagem passaria a mudar o desenho do código.
    /// </summary>
    private static T Campo<T>(Object alvo, string nome)
    {
        var so = new SerializedObject(alvo);
        SerializedProperty prop = so.FindProperty(nome);

        if (prop == null)
        {
            Falhar("campo '" + nome + "' não existe em " + alvo.GetType().Name);
            return default;
        }

        switch (prop.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return (T)(object)prop.boolValue;
            case SerializedPropertyType.Integer:
                return (T)(object)prop.intValue;
            case SerializedPropertyType.Float:
                return (T)(object)prop.floatValue;
            case SerializedPropertyType.LayerMask:
                return (T)(object)(LayerMask)prop.intValue;
            case SerializedPropertyType.Enum:
                return (T)(object)prop.enumValueIndex;
            case SerializedPropertyType.String:
                return (T)(object)prop.stringValue;
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue is T valor ? valor : default;
            default:
                return default;
        }
    }
}
