using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Odisseia.Systems;
using Odisseia.Player;

/// <summary>
/// Confere que a folha do Odisseu fatia e carrega como o jogo espera, sem abrir o Editor:
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod OdysseusSheetProbe.Run
///
/// A pergunta que importa não é "a textura importou?", e sim "o <see cref="SpriteAnimator"/>
/// consegue montar todos os clipes a partir dos nomes dos sprites?". O animador não lê uma
/// lista configurada: ele deduz o estado do nome de cada sprite fatiado
/// (<c>&lt;Folha&gt;_&lt;Estado&gt;_&lt;NN&gt;</c>). Um sprite com nome fora do padrão não vira
/// erro — ele só some do clipe, e a animação fica curta sem ninguém perceber.
///
/// Por isso a checagem central compara, estado a estado, quantos frames o animador
/// realmente montou contra quantos deveriam existir. Também confere que todo estado com
/// clipe tem entrada de FPS no prefab do jogador: sem ela o estado roda no default e
/// um ataque de 18 fps sairia em 10.
/// </summary>
public static class OdysseusSheetProbe
{
    private const string ResourcePath = "Odisseia/Characters/CHR_Odysseus";
    private const string PrefabPath = "Assets/Prefabs/Player.prefab";

    /// <summary>Frames esperados por estado, na ordem das fases do pipeline de arte.</summary>
    private static readonly (string Estado, int Frames)[] Esperado =
    {
        ("Idle", 7), ("Run", 9), ("Jump", 8), ("Fall", 2),
        ("AttackLight", 6), ("AttackHeavy", 8), ("Shield", 6), ("Bow", 8),
        ("Damage", 6), ("Death", 8), ("Interaction", 6), ("Victory", 8),
        ("Crouch", 4), ("CrouchWalk", 3), ("Climb", 6), ("ShieldHold", 4),
    };

    /// <summary>
    /// Estados que precisam repetir em loop. Um deles com <c>loop: 0</c> congela no último
    /// quadro — foi assim que a guarda do escudo ficou estática, com a animação inteira
    /// tocando uma vez e parando.
    /// </summary>
    private static readonly string[] PrecisamDeLoop = { "Idle", "Run", "ShieldHold", "CrouchWalk", "Fall" };

    /// <summary>Ações que o mapa "Player" precisa expor para a movimentação funcionar.</summary>
    private static readonly string[] AcoesNecessarias = { "Move", "Jump", "Crouch" };

    public static void Run()
    {
        bool falhou = false;

        Sprite[] todos = Resources.LoadAll<Sprite>(ResourcePath);
        Debug.Log($"[Folha] {todos.Length} sprites em Resources/{ResourcePath}");

        if (todos.Length == 0)
        {
            Debug.LogError("[Folha] nenhum sprite carregou — a folha não foi fatiada");
            EditorApplication.Exit(1);
            return;
        }

        // Reproduz exatamente a leitura de nome que o SpriteAnimator faz.
        var porEstado = new Dictionary<string, int>();
        foreach (Sprite sprite in todos)
        {
            string nome = sprite.name;
            int ultimo = nome.LastIndexOf('_');
            if (ultimo <= 0 || !int.TryParse(nome.Substring(ultimo + 1), out _))
            {
                Debug.LogError($"[Folha] sprite fora do padrão <Folha>_<Estado>_<NN>: {nome}");
                falhou = true;
                continue;
            }

            string cabeca = nome.Substring(0, ultimo);
            int sep = cabeca.LastIndexOf('_');
            string estado = sep >= 0 ? cabeca.Substring(sep + 1) : cabeca;
            porEstado[estado] = porEstado.TryGetValue(estado, out int n) ? n + 1 : 1;
        }

        foreach ((string estado, int frames) in Esperado)
        {
            if (!porEstado.TryGetValue(estado, out int achados))
            {
                Debug.LogError($"[Folha] estado ausente: {estado}");
                falhou = true;
                continue;
            }

            if (achados != frames)
            {
                Debug.LogError($"[Folha] {estado}: {achados} frames, esperado {frames}");
                falhou = true;
                continue;
            }

            Debug.Log($"[Folha] {estado}: {achados} frames OK");
        }

        foreach (string extra in porEstado.Keys.Except(Esperado.Select(e => e.Estado)))
        {
            Debug.LogError($"[Folha] estado inesperado na folha: {extra}");
            falhou = true;
        }

        // O pivô precisa ser o mesmo em todos os frames, senão o personagem "pula"
        // de altura ao trocar de estado, mesmo com a arte correta.
        Vector2[] pivos = todos.Select(s => new Vector2(
            s.pivot.x / s.rect.width, s.pivot.y / s.rect.height)).ToArray();
        Vector2 primeiro = pivos[0];
        int divergentes = pivos.Count(p => (p - primeiro).sqrMagnitude > 0.0001f);
        if (divergentes > 0)
        {
            Debug.LogError($"[Folha] {divergentes} sprites com pivô diferente do primeiro");
            falhou = true;
        }
        else
        {
            Debug.Log($"[Folha] pivô uniforme em {pivos.Length} sprites: {primeiro}");
        }

        // Todo estado com clipe precisa de FPS configurado no prefab.
        GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        SpriteAnimator animador = player != null ? player.GetComponentInChildren<SpriteAnimator>(true) : null;
        if (animador == null)
        {
            Debug.LogError($"[Folha] SpriteAnimator não encontrado em {PrefabPath}");
            falhou = true;
        }
        else
        {
            SerializedObject so = new SerializedObject(animador);
            SerializedProperty caminho = so.FindProperty("resourcePath");
            if (caminho.stringValue != ResourcePath)
            {
                Debug.LogError($"[Folha] resourcePath do prefab é '{caminho.stringValue}', esperado '{ResourcePath}'");
                falhou = true;
            }

            SerializedProperty estados = so.FindProperty("states");
            var configurados = new HashSet<string>();
            var emLoop = new HashSet<string>();
            for (int i = 0; i < estados.arraySize; i++)
            {
                SerializedProperty item = estados.GetArrayElementAtIndex(i);
                string nome = item.FindPropertyRelative("state").stringValue;
                configurados.Add(nome);
                if (item.FindPropertyRelative("loop").boolValue)
                {
                    emLoop.Add(nome);
                }
            }

            foreach (string estado in PrecisamDeLoop)
            {
                if (configurados.Contains(estado) && !emLoop.Contains(estado))
                {
                    Debug.LogError($"[Folha] {estado} está sem loop — congela no último quadro");
                    falhou = true;
                }
            }

            foreach ((string estado, int _) in Esperado)
            {
                if (!configurados.Contains(estado))
                {
                    Debug.LogError($"[Folha] estado sem FPS no prefab: {estado}");
                    falhou = true;
                }
            }

            Debug.Log($"[Folha] prefab: resourcePath OK, {configurados.Count} estados configurados");

            // Agachar e escalar não valem nada sem os componentes ligados no prefab.
            // A busca sai da RAIZ do prefab: o SpriteAnimator vive num objeto de visual
            // que não é ancestral do objeto onde ficam controller e escalada.
            if (player.GetComponentInChildren<PlayerClimb>(true) == null)
            {
                Debug.LogError("[Folha] PlayerClimb ausente no prefab do jogador");
                falhou = true;
            }

            PlayerController controle = player.GetComponentInChildren<PlayerController>(true);
            if (controle == null)
            {
                Debug.LogError("[Folha] PlayerController ausente no prefab do jogador");
                falhou = true;
            }
            else
            {
                // O collider agachado precisa acompanhar a arte: se a fração não bater com
                // a altura real do sprite agachado, o personagem some dentro do teto ou
                // sobra para fora dele.
                var soControle = new SerializedObject(controle);
                float fracao = soControle.FindProperty("crouchHeightFactor").floatValue;
                const float FracaoArte = 44f / 60f;
                if (Mathf.Abs(fracao - FracaoArte) > 0.05f)
                {
                    Debug.LogError($"[Folha] crouchHeightFactor {fracao:0.00} destoa da arte ({FracaoArte:0.00})");
                    falhou = true;
                }
                else
                {
                    Debug.Log($"[Folha] crouchHeightFactor {fracao:0.00} bate com o sprite agachado");
                }
            }
        }

        // A ação de agachar é nova; sem ela o botão simplesmente não responde.
        var controles = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
            "Assets/ScriptableObjects/PlayerControls.inputactions");
        if (controles == null)
        {
            Debug.LogError("[Folha] PlayerControls.inputactions não encontrado");
            falhou = true;
        }
        else
        {
            var mapa = controles.FindActionMap("Player", throwIfNotFound: false);
            foreach (string acao in AcoesNecessarias)
            {
                if (mapa == null || mapa.FindAction(acao) == null)
                {
                    Debug.LogError($"[Folha] ação ausente no mapa Player: {acao}");
                    falhou = true;
                }
            }

            Debug.Log($"[Folha] input: {string.Join(", ", AcoesNecessarias)} presentes");
        }

        falhou |= !EventosLigados(player);
        falhou |= !VisualDoArcoEEscudo(player);
        falhou |= !TodoEstadoTemGatilho();

        Debug.Log(falhou ? "[Folha] RESULTADO: FALHOU" : "[Folha] RESULTADO: OK");
        EditorApplication.Exit(falhou ? 1 : 0);
    }

    /// <summary>
    /// Confere que todo estado da folha tem, no <see cref="PlayerAnimator"/>, uma constante
    /// que é <b>usada</b> em algum lugar — e não só declarada.
    ///
    /// É a generalização do bug que apareceu quatro vezes: arco, escudo, golpe forte,
    /// interagir e vitória tinham arte na folha, quadros corretos e FPS no prefab, e
    /// simplesmente nunca eram tocados. Um estado sem gatilho não gera erro, não aparece no
    /// console e não quebra nada — ele só não existe para o jogador.
    ///
    /// A checagem lê o código-fonte porque é onde o gatilho vive. Uma constante que aparece
    /// uma única vez no arquivo é a declaração dela e mais nada: ninguém a usa.
    /// </summary>
    private static bool TodoEstadoTemGatilho()
    {
        const string Caminho = "Assets/Scripts/Player/PlayerAnimator.cs";
        if (!System.IO.File.Exists(Caminho))
        {
            Debug.LogError($"[Folha] {Caminho} não encontrado");
            return false;
        }

        string fonte = System.IO.File.ReadAllText(Caminho);

        // Comentar a chamada é a forma mais provável de um gatilho sumir, e o nome do
        // estado continua escrito no comentário. Sem tirar comentários, a checagem
        // aprovaria um gatilho desligado — foi o que aconteceu no primeiro teste dela.
        fonte = System.Text.RegularExpressions.Regex.Replace(fonte, @"/\*.*?\*/", " ",
            System.Text.RegularExpressions.RegexOptions.Singleline);
        fonte = System.Text.RegularExpressions.Regex.Replace(fonte, @"//[^\n]*", " ");

        // private const string StateBow = "Bow";  ->  StateBow / Bow
        var declaracoes = System.Text.RegularExpressions.Regex.Matches(
            fonte, @"const\s+string\s+(\w+)\s*=\s*""(\w+)""\s*;");

        var porEstado = new Dictionary<string, string>();
        foreach (System.Text.RegularExpressions.Match m in declaracoes)
        {
            porEstado[m.Groups[2].Value] = m.Groups[1].Value;
        }

        bool ok = true;
        var mortos = new List<string>();

        foreach ((string estado, int _) in Esperado)
        {
            if (!porEstado.TryGetValue(estado, out string identificador))
            {
                mortos.Add($"{estado} (sem constante)");
                ok = false;
                continue;
            }

            int usos = System.Text.RegularExpressions.Regex.Matches(
                fonte, $@"\b{identificador}\b").Count;

            if (usos < 2)
            {
                mortos.Add($"{estado} (constante declarada, nunca usada)");
                ok = false;
            }
        }

        if (ok)
        {
            Debug.Log($"[Folha] os {Esperado.Length} estados têm gatilho no PlayerAnimator");
        }
        else
        {
            Debug.LogError("[Folha] estados sem gatilho — a arte existe mas o jogador nunca vê: "
                + string.Join(", ", mortos));
        }

        return ok;
    }

    /// <summary>
    /// Confere as duas coisas que faziam arco e escudo aparecerem errado na tela, mesmo com
    /// a animação já tocando.
    ///
    /// O <c>shieldVisual</c> era um retângulo azul de placeholder, de quando não existia arte
    /// de escudo. Com o aspis de bronze na folha ele virou uma faixa azul por cima do
    /// personagem — e continuaria ligado sem ninguém notar, porque é só um campo preenchido.
    ///
    /// O <c>releaseDelay</c> zerado faz a flecha nascer no mesmo quadro em que a animação
    /// começa, ou seja com o personagem ainda parado: o tiro sai antes do arco subir.
    /// </summary>
    private static bool VisualDoArcoEEscudo(GameObject prefab)
    {
        if (prefab == null)
        {
            return false;
        }

        bool ok = true;

        PlayerShield escudo = prefab.GetComponentInChildren<PlayerShield>(true);
        if (escudo == null)
        {
            Debug.LogError("[Folha] PlayerShield ausente no prefab");
            ok = false;
        }
        else if (new SerializedObject(escudo).FindProperty("shieldVisual").objectReferenceValue != null)
        {
            Debug.LogError("[Folha] shieldVisual ainda ligado — a faixa azul de placeholder volta a aparecer");
            ok = false;
        }
        else
        {
            Debug.Log("[Folha] escudo sem placeholder azul");
        }

        // O AttackHeavy ficou meses só como arte na folha, sem nenhum código capaz de
        // acioná-lo. Com comboWindow zerado ele volta a ficar inalcançável.
        PlayerCombat combate = prefab.GetComponentInChildren<PlayerCombat>(true);
        if (combate != null)
        {
            float janela = new SerializedObject(combate).FindProperty("comboWindow").floatValue;
            if (janela <= 0f)
            {
                Debug.LogError("[Folha] comboWindow zerado — AttackHeavy fica inalcançável");
                ok = false;
            }
            else
            {
                Debug.Log($"[Folha] comboWindow {janela:0.00}s dá acesso ao AttackHeavy");
            }
        }

        PlayerBow arco = prefab.GetComponentInChildren<PlayerBow>(true);
        if (arco != null)
        {
            float atraso = new SerializedObject(arco).FindProperty("releaseDelay").floatValue;
            if (atraso <= 0f)
            {
                Debug.LogError("[Folha] releaseDelay zerado — a flecha sai antes do arco subir");
                ok = false;
            }
            else
            {
                Debug.Log($"[Folha] releaseDelay {atraso:0.00}s sincroniza a flecha com a animação");
            }
        }

        return ok;
    }

    /// <summary>
    /// Confere que alguém escuta <see cref="PlayerBow.Fired"/>.
    ///
    /// Esta é a checagem que existe por causa de um bug real: o arco tinha arte na folha,
    /// FPS no prefab e evento no componente — e mesmo assim a flecha saía do personagem sem
    /// nenhuma animação, porque o <see cref="PlayerAnimator"/> nunca assinava o evento.
    /// Nada nesse arranjo dá erro; o estado só nunca é tocado. Por isso a verificação lê a
    /// lista real de inscritos por reflexão, em vez de conferir apenas que as peças existem.
    ///
    /// O prefab é instanciado e <c>Awake</c>/<c>OnEnable</c> são chamados <b>à mão</b>: fora
    /// do play mode o Unity não roda esses callbacks em MonoBehaviour comum, então sem essa
    /// chamada a lista viria sempre vazia e o teste passaria a acusar todo mundo.
    /// </summary>
    private static bool EventosLigados(GameObject prefab)
    {
        if (prefab == null)
        {
            return false;
        }

        const System.Reflection.BindingFlags Privados =
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

        GameObject instancia = null;
        try
        {
            instancia = Object.Instantiate(prefab);

            PlayerBow arco = instancia.GetComponentInChildren<PlayerBow>(true);
            PlayerAnimator anim = instancia.GetComponentInChildren<PlayerAnimator>(true);
            if (arco == null || anim == null)
            {
                Debug.LogError("[Folha] PlayerBow ou PlayerAnimator ausente no prefab");
                return false;
            }

            foreach (string metodo in new[] { "Awake", "OnEnable" })
            {
                typeof(PlayerAnimator).GetMethod(metodo, Privados)?.Invoke(anim, null);
            }

            var handler = typeof(PlayerBow).GetField("Fired", Privados)?.GetValue(arco) as System.Delegate;
            int inscritos = handler?.GetInvocationList().Length ?? 0;

            if (inscritos == 0)
            {
                Debug.LogError("[Folha] ninguém escuta PlayerBow.Fired — o arco dispara sem animação");
                return false;
            }

            Debug.Log($"[Folha] PlayerBow.Fired tem {inscritos} inscrito(s)");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Folha] não foi possível instanciar o prefab para checar eventos: " + e.Message);
            return false;
        }
        finally
        {
            if (instancia != null)
            {
                Object.DestroyImmediate(instancia);
            }
        }
    }
}
