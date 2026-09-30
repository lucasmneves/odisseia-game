using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Elenco da Fase 09 — Mundo dos Mortos (Docs/Characters/Fase09/MUNDO_DOS_MORTOS_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod UnderworldCastDresser.Dress
/// Unity.exe -batchmode -quit -projectPath . -executeMethod UnderworldCastDresser.Poses   (fotos, sem salvar)
///
/// - Os dois EnemyBasic carmesim viram SOMBRAS DE GUERREIROS (EnemyFactionOverride). O override de cena
///   que já existia — alpha 0,55 na cor do renderer — continua valendo: a transparência é da Unity.
/// - As sombras sem nome e a mãe (Anticleia) entram nos altares que o MundoDosMortosSceneDresser pôs nos
///   DialogueTrigger, deixando "o espaço em volta livre para Odisseu, as sombras e o diálogo".
///   Figuras sem colisor e sem script; a posição vem do altar e dos braseiros lidos da cena.
/// - A variação da multidão é por COR do renderer (tom pálido + alpha): nenhum asset a mais.
///
/// Idempotente.
/// </summary>
public static class UnderworldCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_09_MundoDosMortos.unity";
    private const string RaizDoElenco = "UnderworldCast";
    private const string Pasta = "Odisseia/Characters/NPCs/";
    private const float TopoDoChao = -2f;

    // Tons espectrais (multiplicam a arte cinza-azulada) — frios e pálidos, nunca quentes.
    private static readonly Color SombraAzul = new Color(0.80f, 0.90f, 1.00f, 0.50f);
    private static readonly Color SombraVioleta = new Color(0.88f, 0.82f, 1.00f, 0.45f);
    private static readonly Color SombraVerde = new Color(0.82f, 1.00f, 0.92f, 0.50f);
    private static readonly Color Mae = new Color(0.92f, 0.95f, 1.00f, 0.72f);   // mais presente que as sombras

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        int inimigos = EnemyFactionOverride.Aplicar("Odisseia/Enemies/CHR_Shade_Warrior", "UnderworldCast");

        // O alpha 0,55 que a cena já tinha foi pensado para o lanceiro CARMESIM, que se destacava pela cor.
        // Com a arte cinza da sombra, 55% sobre a pedra escura deixou o inimigo MAIS ESCURO que a parede
        // atrás dele (luminância 65,9 contra 73,0, medido na captura) — inimigo invisível. Tom espectral
        // pálido e alpha 0,85: continua translúcido e volta a ler.
        foreach (Odisseia.Enemies.EnemyController e in Object.FindObjectsByType<Odisseia.Enemies.EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var sr = e.GetComponentInChildren<SpriteAnimator>(true)?.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.color = new Color(0.86f, 0.96f, 1f, 0.85f); }
        }

        GameObject antigo = GameObject.Find(RaizDoElenco);
        if (antigo != null) { Object.DestroyImmediate(antigo); }
        var raiz = new GameObject(RaizDoElenco).transform;
        GameObject jogador = GameObject.Find("Player");
        float nascimento = jogador != null ? jogador.transform.position.x : float.NaN;

        // Sombras: em volta do altar do sangue, voltadas para ele.
        float altarS = X("Altar_DialogueTrigger_Shades"), dirS = X("Brazier_DialogueTrigger_Shades_R");
        int n = 0;
        n += Figura(raiz, "Shade_1", "CHR_Shade", altarS + (dirS - altarS) * 0.45f, true, SombraAzul, nascimento);
        n += Figura(raiz, "Shade_2", "CHR_Shade", dirS + 0.9f, true, SombraVioleta, nascimento);
        // A terceira ficava à esquerda do altar, mas o Odisseu nasce ali (x=-12): a guarda recusou.
        // Vai para o fim da fila, do outro lado do braseiro — todas voltadas para o sangue.
        n += Figura(raiz, "Shade_3", "CHR_Shade", dirS + 2.0f, true, SombraVerde, nascimento);

        // A mãe: do outro lado do altar, olhando para o Odisseu que chega pela esquerda.
        float altarM = X("Altar_DialogueTrigger_Mother"), dirM = X("Brazier_DialogueTrigger_Mother_R");
        n += Figura(raiz, "Anticleia", "CHR_Anticleia", altarM + (dirM - altarM) * 0.5f, true, Mae, nascimento);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[UnderworldCast] {inimigos} sombras guerreiras (inimigos); {n} figuras nos altares.");
        EditorApplication.Exit(inimigos == 2 && n == 4 ? 0 : 1);
    }

    private static float X(string nome)
    {
        GameObject go = GameObject.Find(nome);
        if (go == null) { Debug.LogError("[UnderworldCast] não encontrado: " + nome); return 0f; }
        return go.transform.position.x;
    }

    private static int Figura(Transform pai, string nome, string folha, float x, bool olharEsquerda, Color cor, float nascimento)
    {
        if (!float.IsNaN(nascimento) && Mathf.Abs(x - nascimento) < 1.5f)
        {
            Debug.LogError($"[UnderworldCast] {nome} em x={x:0.0} cairia em cima do jogador (x={nascimento:0.0})");
            return 0;
        }
        Sprite parado = null;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Pasta + folha + ".png"))
        {
            if (a is Sprite s && s.name == folha + "_Idle_00") { parado = s; }
        }
        if (parado == null) { Debug.LogError("[UnderworldCast] sem folha " + folha); return 0; }

        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        var corpo = new GameObject("Body");
        corpo.transform.SetParent(go.transform, false);
        var sr = corpo.AddComponent<SpriteRenderer>();
        sr.sprite = parado;
        sr.flipX = olharEsquerda;
        sr.color = cor;           // tom e transparência: o efeito espectral é da Unity, não do sprite
        sr.sortingOrder = 0;      // na frente do altar (-4), atrás do jogador (2)
        go.transform.position = new Vector3(x, TopoDoChao - parado.bounds.min.y, 0f);

        var animador = corpo.AddComponent<SpriteAnimator>();
        var so = new SerializedObject(animador);
        so.FindProperty("resourcePath").stringValue = Pasta + folha;
        so.FindProperty("defaultState").stringValue = "Idle";
        so.FindProperty("defaultFramesPerSecond").floatValue = 4.5f + Mathf.Repeat(Mathf.Abs(x) * 0.37f, 1.2f);
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"[UnderworldCast] {nome} em x={x:0.0}");
        return 1;
    }

    /// <summary>Odisseu diante da mãe, diante das sombras e diante de uma sombra guerreira — sem salvar.</summary>
    public static void Poses()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject jogador = GameObject.Find("Player");
        var corpoDoJogador = jogador.transform.Find("Visual/Body");
        void PorJogador(float x) =>
            jogador.transform.position += new Vector3(x - jogador.transform.position.x, TopoDoChao - corpoDoJogador.position.y, 0f);
        const string pasta = "Docs/Characters/Fase09/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);

        float mae = GameObject.Find(RaizDoElenco + "/Anticleia").transform.position.x;
        PorJogador(mae - 3f);
        PrologueScreenshot.Render(mae - 1.5f, 0f, pasta + "anticleia_com_odisseu.png");

        float sombras = X("Altar_DialogueTrigger_Shades");
        PorJogador(sombras - 3.5f);
        PrologueScreenshot.Render(sombras, 0f, pasta + "sombras_com_odisseu.png");

        var inimigo = Object.FindFirstObjectByType<Odisseia.Enemies.EnemyController>();
        PorJogador(inimigo.transform.position.x - 2.2f);
        PrologueScreenshot.Render(inimigo.transform.position.x - 1f, 0f, pasta + "sombra_guerreira_com_odisseu.png");
        EditorApplication.Exit(0);
    }
}
