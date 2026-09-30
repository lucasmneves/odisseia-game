using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Levels;

/// <summary>
/// Elenco da Fase 12 — Gado do Sol (Docs/Characters/Fase12/SUN_CATTLE_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod SunCattleCastDresser.Dress
/// Unity.exe -batchmode -quit -projectPath . -executeMethod SunCattleCastDresser.Poses   (fotos, sem salvar)
///
/// O gado não é personagem animado: é sprite estático. Dois bois ficam sobre os gatilhos
/// SacredCattleZone (comer = fome cheia, dano e flag); o resto é rebanho de ambiente. Este dresser
/// troca a arte pelo SUN CATTLE MASTER v1, em escala 1 (o gado antigo era prop encolhido a 0,38–0,55).
///
/// Correção de integração, sem tocar código: o SacredCattleZone esmaece o SpriteRenderer do PRÓPRIO
/// gatilho quando o boi é comido — e o GadoDoSolSceneDresser tinha desligado esse renderer e posto a arte
/// num objeto separado (Cattle_i). O boi comido continuava inteiro na tela. Aqui a arte passa para o
/// renderer do gatilho e o Cattle_i sai; o retângulo do colisor no mundo fica idêntico (gameplay igual).
///
/// Roda DEPOIS do GadoDoSolSceneDresser (que recria os Cattle_i e desliga o renderer do gatilho).
/// Idempotente. Nenhum prefab e nenhum script de jogo são tocados.
/// </summary>
public static class SunCattleCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_12_GadoDoSol.unity";
    private const string Pasta = "Assets/Art/Environments/GadoDoSol/SacredCattle/";
    private const float TopoDoChao = -2f;

    // Rebanho de ambiente: o mesmo master, variando pelagem (creme/ocre) e lado. Sem pose pastando e sem
    // bezerro (cortados por economia): os dois bezerros antigos viram bois adultos. A cor de distância
    // (tinta azulada) e a ordem −2 vêm do dresser de cenário; o Herd_17 vai a −3 porque encosta no Herd_16.
    private static readonly (string nome, string arte, bool virado, int ordem)[] Rebanho =
    {
        ("Herd_-3", "sun_cattle_idle_ochre", false, -2),
        ("Herd_7", "sun_cattle_idle", true, -2),
        ("Herd_9", "sun_cattle_idle_ochre", true, -2),
        ("Herd_16", "sun_cattle_idle", false, -2),
        ("Herd_17", "sun_cattle_idle_ochre", true, -3),
    };

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var zonas = Object.FindObjectsByType<SacredCattleZone>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OrderBy(z => z.transform.position.x).ToArray();
        if (zonas.Length == 0) { Debug.LogError("[SunCattle] nenhum SacredCattleZone"); EditorApplication.Exit(1); return; }

        for (int i = 0; i < zonas.Length; i++)
        {
            var z = zonas[i];
            var box = z.GetComponent<BoxCollider2D>();
            if (box == null) { Debug.LogError("[SunCattle] gatilho sem BoxCollider2D: " + z.name); EditorApplication.Exit(1); return; }
            Bounds antes = box.bounds;

            // A arte separada que o dresser de cenário pôs sobre este gatilho.
            GameObject separada = GameObject.Find("GadoDoSolScenery/Cattle_" + i);
            if (separada != null) { Object.DestroyImmediate(separada); }

            // Pivô do sprite na base (os cascos): o gatilho desce ao chão, em escala 1, e o colisor
            // é recalculado em coordenadas locais para cobrir o MESMO retângulo do mundo.
            z.transform.position = new Vector3(antes.center.x, TopoDoChao, z.transform.position.z);
            z.transform.localScale = Vector3.one;
            box.size = new Vector2(antes.size.x, antes.size.y);
            box.offset = new Vector2(0f, antes.center.y - TopoDoChao);

            var sr = z.GetComponent<SpriteRenderer>();
            if (sr == null) { sr = z.gameObject.AddComponent<SpriteRenderer>(); }
            // Creme nos dois que se podem comer: é a pelagem mais clara, a que mais lê como sagrada.
            sr.sprite = Arte("sun_cattle_idle");
            if (sr.sprite == null) { EditorApplication.Exit(1); return; }
            sr.enabled = true;
            sr.color = Color.white;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.sortingOrder = 1;
            // O primeiro olha para quem chega (o Odisseu entra pela esquerda); o segundo olha para a direita.
            sr.flipX = i % 2 == 0;

            Bounds depois = box.bounds;
            Debug.Log($"[SunCattle] {z.name} {i}: colisor antes {Fmt(antes)} depois {Fmt(depois)}; " +
                      $"desenho x {sr.bounds.min.x:0.00}..{sr.bounds.max.x:0.00} y {sr.bounds.min.y:0.00}..{sr.bounds.max.y:0.00}");
            if ((antes.min - depois.min).magnitude > 0.001f || (antes.max - depois.max).magnitude > 0.001f)
            {
                Debug.LogError("[SunCattle] o colisor mudou de lugar"); EditorApplication.Exit(1); return;
            }
        }

        foreach (var (nome, arte, virado, ordem) in Rebanho)
        {
            GameObject go = GameObject.Find("GadoDoSolScenery/" + nome);
            if (go == null) { Debug.LogError("[SunCattle] sem " + nome); EditorApplication.Exit(1); return; }
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = Arte(arte);
            if (sr.sprite == null) { EditorApplication.Exit(1); return; }
            sr.flipX = virado;
            sr.sortingOrder = ordem;
            go.transform.localScale = Vector3.one;
            go.transform.position = new Vector3(go.transform.position.x, TopoDoChao - 0.05f, go.transform.position.z);
            Debug.Log($"[SunCattle] {nome}: {arte}, {sr.bounds.size.x:0.00} x {sr.bounds.size.y:0.00} un");
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        EditorApplication.Exit(0);
    }

    /// <summary>Gado em repouso, o Odisseu ao lado de um boi, o boi comido e a ilha — sem salvar.</summary>
    public static void Poses()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var zonas = Object.FindObjectsByType<SacredCattleZone>(FindObjectsSortMode.None).OrderBy(z => z.transform.position.x).ToArray();
        GameObject jogador = GameObject.Find("Player");
        var corpo = jogador.transform.Find("Visual/Body");
        const string pasta = "Docs/Characters/Fase12/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);

        float xBoi = zonas[0].transform.position.x;
        // Odisseu à esquerda do primeiro boi, na altura de spawn da cena (a caixa do sprite tem margem
        // transparente abaixo dos pés: assentar pela caixa o deixava flutuando na foto).
        jogador.transform.position += new Vector3(xBoi - 1.4f - corpo.position.x, 0f, 0f);
        PrologueScreenshot.Render(xBoi, -0.5f, pasta + "odisseu_e_boi.png");
        PrologueScreenshot.Render(zonas[1].transform.position.x, -0.5f, pasta + "boi_2.png");
        PrologueScreenshot.Render(8f, -0.5f, pasta + "rebanho.png");
        foreach (float x in new[] { -8f, 4f, 16f, 28f })
        {
            PrologueScreenshot.Render(x, 0f, pasta + $"ilha_{x:00}.png");
        }

        // O boi comido: o SacredCattleZone põe alpha 0,3 no renderer do gatilho — que agora é o boi.
        var sr = zonas[0].GetComponent<SpriteRenderer>();
        Color c = sr.color; c.a = 0.3f; sr.color = c;
        PrologueScreenshot.Render(xBoi, -0.5f, pasta + "boi_comido.png");
        EditorApplication.Exit(0);
    }

    private static Sprite Arte(string nome)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(Pasta + nome + ".png");
        if (s == null) { Debug.LogError("[SunCattle] sprite não encontrado: " + nome); }
        return s;
    }

    private static string Fmt(Bounds b) => $"x {b.min.x:0.00}..{b.max.x:0.00} y {b.min.y:0.00}..{b.max.y:0.00}";
}
