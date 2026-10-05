using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a tela final (Ending) com a arte do pack — PXL-009 (o casal do reencontro) e PP-14 (o quarto da cama na
/// oliveira, o sinal do reconhecimento no canto XXIII):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod EndingSceneDresser.Run
///
/// Antes: Odisseu, Penélope e Telêmaco eram caixas coloridas sobre céu e chão chapados. Só apresentação: os botões, os
/// textos e o EndingController não são tocados; as caixas e os fundos só têm o SpriteRenderer desligado. Raiz própria
/// (<see cref="Raiz"/>), destruída e recriada a cada rodada — idempotente.
///
/// Posições lidas em PIXELS da arte do quarto (604×340, a mesma proporção 16:9 da tela) e convertidas pela própria
/// bounds do sprite, então não dependem do pivô.
/// </summary>
public static class EndingSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Menu/Ending.unity";
    private const string Raiz = "EndingScenery";
    private const string Quarto = "Assets/Art/Ending/ending_bedroom_olive.png";
    private const string Casal = "Assets/Art/Ending/ending_reunion.png";

    public static void Run()
    {
        bool ok = Executar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    private static Sprite fundo;

    private static bool Executar()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        fundo = AssetDatabase.LoadAssetAtPath<Sprite>(Quarto);
        Sprite casal = AssetDatabase.LoadAssetAtPath<Sprite>(Casal);
        if (fundo == null || casal == null) { Debug.LogError("[Ending] arte do quarto ou do casal ausente"); return false; }

        GameObject antigo = GameObject.Find(Raiz);
        while (antigo != null) { Object.DestroyImmediate(antigo); antigo = GameObject.Find(Raiz); }
        Transform raiz = new GameObject(Raiz).transform;

        // A câmera fecha na altura do quarto: a arte tem 16:9, então ele preenche a tela sem borda nem corte.
        Camera cam = Camera.main;
        cam.orthographicSize = fundo.bounds.extents.y;
        cam.transform.position = new Vector3(0f, 0f, cam.transform.position.z);

        Peca(raiz, "Bedroom", fundo, Vector2.zero, -50);

        // O casal no chão livre à esquerda (entre o tear e a mesinha), fora da faixa dos botões.
        Peca(raiz, "Reunion", casal, NoChao(casal, 150, 305), 0);

        // Telêmaco adulto, à direita, olhando para os pais: a folha do jogo com o Idle animado.
        Sprite telemaco = PrimeiroQuadro("Odisseia/Characters/NPCs/CHR_Telemachus_Adult");
        if (telemaco != null)
        {
            GameObject t = Peca(raiz, "Telemachus", telemaco, NoChao(telemaco, 492, 308), -1);
            t.GetComponent<SpriteRenderer>().flipX = true;   // os masters olham para a direita
            var anim = t.AddComponent<SpriteAnimator>();
            var so = new SerializedObject(anim);
            so.FindProperty("resourcePath").stringValue = "Odisseia/Characters/NPCs/CHR_Telemachus_Adult";
            so.FindProperty("defaultState").stringValue = "Idle";
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // A chama da lychnos sobre a lamparina pintada (centro x 223, base y 246 na arte).
        Sprite lamparina = PrimeiroQuadro("Odisseia/Environments/Fire/shared_oil_lamp_anim");
        if (lamparina != null)
        {
            GameObject l = Peca(raiz, "OilLamp", lamparina, NoChao(lamparina, 223, 246), -40);
            var anim = l.AddComponent<SpriteAnimator>();
            var so = new SerializedObject(anim);
            so.FindProperty("resourcePath").stringValue = "Odisseia/Environments/Fire/shared_oil_lamp_anim";
            so.FindProperty("defaultState").stringValue = "anim";
            so.FindProperty("defaultFramesPerSecond").floatValue = 8f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        foreach (string nome in new[] { "Sky_Background", "Ground_Background", "Odisseu", "Penelope", "Telemaco" })
        {
            GameObject go = GameObject.Find(nome);
            if (go == null) { continue; }
            foreach (SpriteRenderer sr in go.GetComponentsInChildren<SpriteRenderer>(true)) { sr.enabled = false; }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[Ending] quarto, casal, Telêmaco e lamparina; câmera {cam.orthographicSize:0.00}");
        return true;
    }

    /// <summary>Posição no mundo para a BASE do sprite cair no pixel (px, py) da arte do quarto (origem no topo).</summary>
    private static Vector2 NoChao(Sprite sprite, float px, float py)
    {
        float ppu = fundo.pixelsPerUnit;
        float x = fundo.bounds.min.x + px / ppu;
        float yBase = fundo.bounds.max.y - py / ppu;
        // Ajusta pelo pivô do próprio sprite: a base dele (bounds.min.y) tem de cair em yBase.
        return new Vector2(x - sprite.bounds.center.x, yBase - sprite.bounds.min.y);
    }

    private static GameObject Peca(Transform raiz, string nome, Sprite sprite, Vector2 posicao, int ordem)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(raiz, false);
        go.transform.position = new Vector3(posicao.x, posicao.y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = ordem;
        return go;
    }

    private static Sprite PrimeiroQuadro(string recurso)
    {
        Sprite[] quadros = Resources.LoadAll<Sprite>(recurso);
        if (quadros.Length == 0) { Debug.LogWarning("[Ending] sem " + recurso); return null; }
        System.Array.Sort(quadros, (a, b) => string.CompareOrdinal(a.name, b.name));
        return quadros[0];
    }
}
