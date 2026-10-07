using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Prova a chama do altar (Etapa 13B.2) em play mode, pelo caminho do jogo:
///
/// Unity.exe -batchmode -projectPath . -executeMethod AltarFlameProbe.Run [-altarOut Docs/QA/_hud/altar_flame.png]
/// (sem -nographics e sem -quit; sai sozinho, código 0 = OK)
///
/// Em cada cena: o altar apagado fica parado (quadro 00, sem animador); o jogador é posto no altar e o gatilho o acende;
/// a chama troca de quadro (só 01–06) com o jogador correndo e a câmera indo atrás; congela na pausa e volta no Resume;
/// posição, escala, colisor, ordem e camada não mudam; a chama fica atrás do jogador; nenhum objeto sobra depois da
/// faísca do acendimento. Troia → Circe → Final → Troia de novo (troca de cena, saída e reentrada). Na 1ª cena grava 4
/// recortes do altar aceso lado a lado. Roda no mudo. Não salva cena nenhuma; restaura as opções de play mode.
/// </summary>
public static class AltarFlameProbe
{
    private static readonly string[] Cenas = { "Level_02_Troia", "Level_08_Circe", "Level_16_Final", "Level_02_Troia" };

    private static int cena, passo, falhas;
    private static double acordar, fim;
    private static bool opcoesAtivas;
    private static EnterPlayModeOptions opcoes;
    private static string saida;

    private static Checkpoint altar;
    private static SpriteRenderer sr;
    private static GameObject jogador;
    private static Vector3 pos0, escala0, cam0;
    private static Bounds colisor0;
    private static int ordem0, camada0, objetos0;
    private static readonly HashSet<string> vistos = new HashSet<string>();
    private static readonly HashSet<string> chama = new HashSet<string>();
    private static readonly List<Texture2D> fotos = new List<Texture2D>();
    private static bool sumiu, mudoOriginal;

    public static void Run()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-altarOut");
        saida = i >= 0 && i < a.Length - 1 ? a[i + 1] : "Docs/QA/_hud/altar_flame.png";

        // Todo teste no mudo.
        mudoOriginal = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = true;
        AudioListener.volume = 0f;

        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene($"Assets/Scenes/Levels/{Cenas[0]}.unity", OpenSceneMode.Single);
        cena = 0; passo = 0; falhas = 0;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>
    /// Sem play mode: em cada fase da campanha, os sprites da camada do altar que encostam nele com ordem de -3 a 1 —
    /// quem desenha por cima dele (ordem maior ou igual) e quem fica atrás.
    ///
    /// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod AltarFlameProbe.Vizinhanca
    /// </summary>
    public static void Vizinhanca()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Scene Level_", new[] { "Assets/Scenes/Levels" }))
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
            foreach (Checkpoint c in Object.FindObjectsByType<Checkpoint>())
            {
                var a = c.GetComponent<SpriteRenderer>();
                var linhas = Object.FindObjectsByType<SpriteRenderer>()
                    .Where(r => r != a && r.enabled && r.sortingLayerID == a.sortingLayerID && r.bounds.Intersects(a.bounds)
                                && r.sortingOrder >= -3 && r.sortingOrder <= 1 && r.GetComponentInParent<Checkpoint>() == null)
                    .Select(r => $"{r.name}={r.sortingOrder}{(r.sortingOrder >= a.sortingOrder ? " (por cima)" : "")}");
                Debug.Log($"[Altar] {System.IO.Path.GetFileNameWithoutExtension(caminho)} altar x={c.transform.position.x:0.0} ordem {a.sortingOrder}: {string.Join(", ", linhas)}");
            }
        }
    }

    /// <summary>
    /// Sem play mode e sem salvar: recorte de cada altar das fases dadas, com a ordem do prefab e com a ordem 0, lado a
    /// lado (esquerda = ordem do prefab), para ver o que a ordem muda no cenário.
    ///
    /// Unity.exe -batchmode -quit -projectPath . -executeMethod AltarFlameProbe.Comparar -altarScenes Level_01_Itaca_Prologue,Level_14_Itaca_Return -altarOut x.png
    /// </summary>
    public static void Comparar()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        string Arg(string n, string d) { int i = System.Array.IndexOf(a, n); return i >= 0 && i < a.Length - 1 ? a[i + 1] : d; }
        string[] cenas = Arg("-altarScenes", "Level_01_Itaca_Prologue").Split(',');
        string destino = Arg("-altarOut", "Docs/QA/_hud/altar_ordem.png");
        const int w = 160, h = 200;
        var linhas = new List<Texture2D[]>();
        foreach (string nome in cenas)
        {
            EditorSceneManager.OpenScene($"Assets/Scenes/Levels/{nome}.unity", OpenSceneMode.Single);
            Camera principal = Camera.main;
            var go = new GameObject("_AltarCam");
            var cam = go.AddComponent<Camera>();
            cam.CopyFrom(principal);
            cam.orthographicSize = 1.9f;
            foreach (Checkpoint c in Object.FindObjectsByType<Checkpoint>().OrderBy(c => c.transform.position.x))
            {
                var r = c.GetComponent<SpriteRenderer>();
                int ordem = r.sortingOrder;
                r.sprite = Quadros(c).FirstOrDefault() ?? r.sprite;
                go.transform.position = new Vector3(c.transform.position.x, c.transform.position.y + 1f, principal.transform.position.z);
                var par = new Texture2D[2];
                for (int k = 0; k < 2; k++)
                {
                    r.sortingOrder = k == 0 ? ordem : 0;
                    var rt = new RenderTexture(w, h, 24) { filterMode = FilterMode.Point };
                    cam.targetTexture = rt;
                    cam.Render();
                    RenderTexture.active = rt;
                    par[k] = new Texture2D(w, h, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave }; // sobrevive à troca de cena
                    par[k].ReadPixels(new Rect(0, 0, w, h), 0, 0);
                    par[k].Apply();
                    RenderTexture.active = null;
                    cam.targetTexture = null;
                    rt.Release();
                }
                r.sortingOrder = ordem;
                linhas.Add(par);
                Debug.Log($"[Altar] recorte {linhas.Count}: {nome} x={c.transform.position.x:0.0} (ordem {ordem} | 0)");
            }
            Object.DestroyImmediate(go);
        }

        // Pares em grade de 4 por linha: [ordem do prefab | ordem 0], 12 px entre pares.
        int porLinha = 4, pw = 2 * w + 2 + 12;
        int nl = (linhas.Count + porLinha - 1) / porLinha;
        var tela = new Texture2D(porLinha * pw, nl * (h + 8), TextureFormat.RGB24, false);
        var fundo = Enumerable.Repeat(new Color(0.1f, 0.1f, 0.1f), tela.width * tela.height).ToArray();
        tela.SetPixels(fundo);
        for (int i = 0; i < linhas.Count; i++)
        {
            int x = (i % porLinha) * pw, y = (nl - 1 - i / porLinha) * (h + 8);
            tela.SetPixels(x, y, w, h, linhas[i][0].GetPixels());
            tela.SetPixels(x + w + 2, y, w, h, linhas[i][1].GetPixels());
        }
        tela.Apply();
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destino));
        System.IO.File.WriteAllBytes(destino, tela.EncodeToPNG());
        Debug.Log($"[Altar] {destino}");
    }

    private static double Agora => EditorApplication.timeSinceStartup;

    private static void Falha(string msg) { falhas++; Debug.LogError($"[Altar] {Cenas[cena]}: FALHA — {msg}"); }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || Agora < acordar) { return; }
        try { Passo(); }
        catch (System.Exception e) { Falha(e.ToString()); Encerrar(); }
    }

    private static void Passo()
    {
        switch (passo)
        {
            case 0: // cena carregada: espera montar
                AudioListener.volume = 0f;
                acordar = Agora + 1.5;
                passo = 1;
                return;

            case 1: // altar apagado e parado
                jogador = GameObject.FindGameObjectWithTag("Player");
                altar = Object.FindObjectsByType<Checkpoint>().OrderBy(c => c.transform.position.x).FirstOrDefault();
                if (jogador == null || altar == null) { Falha("sem jogador ou sem altar"); Proxima(); return; }
                sr = altar.GetComponent<SpriteRenderer>();
                chama.Clear();
                foreach (Sprite s in Quadros(altar)) { chama.Add(s.name); }
                if (chama.Count != 6) { Falha($"activeFrames com {chama.Count} quadros (esperado 6)"); }
                pos0 = altar.transform.position; escala0 = altar.transform.lossyScale;
                colisor0 = altar.GetComponent<Collider2D>().bounds;
                ordem0 = sr.sortingOrder; camada0 = sr.sortingLayerID;
                objetos0 = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length;
                vistos.Clear(); sumiu = false;
                Vizinhos();
                fim = Agora + 1.0; passo = 2;
                Debug.Log($"[Altar] {Cenas[cena]}: {Object.FindObjectsByType<Checkpoint>().Length} altares; testando o de x={pos0.x:0.0} (timeScale {Time.timeScale})");
                return;

            case 2:
                Amostrar();
                if (Agora < fim) { acordar = Agora + 0.1; return; }
                if (vistos.Count != 1 || !vistos.Contains("item_checkpoint_altar_00")) { Falha($"apagado mostrou {string.Join(",", vistos)}"); }
                if (altar.GetComponent<SpriteAnimator>() != null) { Falha("animador antes de acender"); }
                Debug.Log($"[Altar] 1. parado: {string.Join(",", vistos)}, sem animador — OK");
                // Põe o jogador no altar: o gatilho acende como no jogo.
                var rb = jogador.GetComponent<Rigidbody2D>();
                Vector3 alvo = colisor0.center;
                if (rb != null) { rb.position = alvo; rb.linearVelocity = Vector2.zero; }
                jogador.transform.position = alvo;
                fim = Agora + 1.5; passo = 3;
                return;

            case 3: // acendeu?
                if (sr.sprite != null && sr.sprite.name != "item_checkpoint_altar_00") { vistos.Clear(); acordar = Agora + 1.2; passo = 4; return; }
                if (Agora > fim) { Falha("o gatilho não acendeu o altar"); Proxima(); return; }
                acordar = Agora + 0.05;
                return;

            case 4: // fotos com o jogador no altar (só na 1ª cena)
                if (cena == 0 && fotos.Count < 4) { fotos.Add(Recorte()); acordar = Agora + 0.13; return; }
                Profundidade();
                cam0 = Camera.main.transform.position;
                vistos.Clear();
                fim = Agora + 2.0; passo = 5;
                return;

            case 5: // jogo correndo: jogador anda, câmera acompanha
                // Leva o jogador 4 un./s para a direita (a câmera vai atrás); empurrar por velocidade travava em parede.
                var corpo = jogador.GetComponent<Rigidbody2D>();
                Vector3 passoX = Vector3.right * 4f * 0.05f;
                jogador.transform.position += passoX;
                if (corpo != null) { corpo.position = jogador.transform.position; corpo.linearVelocity = Vector2.zero; }
                Amostrar();
                if (Agora < fim) { acordar = Agora + 0.05; return; }
                float andou = Vector3.Distance(Camera.main.transform.position, cam0);
                Conferir("2-3. jogando/câmera", minimo: 4);
                Debug.Log($"[Altar] câmera andou {andou:0.00} un.");
                if (andou < 0.5f) { Falha("a câmera não acompanhou (teste da câmera inválido)"); }
                var pausa = Object.FindObjectsByType<Odisseia.UI.PauseMenu>().FirstOrDefault();
                if (pausa == null) { Falha("sem PauseMenu"); Proxima(); return; }
                pausa.Pause();
                vistos.Clear(); fim = Agora + 1.0; passo = 6;
                return;

            case 6: // pausa: congela como os outros fogos (timeScale 0)
                Amostrar();
                if (Agora < fim) { acordar = Agora + 0.1; return; }
                if (vistos.Count != 1) { Falha($"andou na pausa: {string.Join(",", vistos)}"); }
                else { Debug.Log($"[Altar] 4. pausa: parado em {vistos.First()} — OK"); }
                Object.FindObjectsByType<Odisseia.UI.PauseMenu>().First().Resume();
                vistos.Clear(); fim = Agora + 1.5; passo = 7;
                return;

            case 7: // volta da pausa
                Amostrar();
                if (Agora < fim) { acordar = Agora + 0.05; return; }
                Conferir("5. volta da pausa", minimo: 3);
                int objetos = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Length;
                Debug.Log($"[Altar] rastro: {objetos0} objetos antes de acender, {objetos} depois");
                if (objetos > objetos0) { Falha($"sobraram {objetos - objetos0} objetos"); }
                Proxima();
                return;
        }
    }

    private static Sprite[] Quadros(Checkpoint c)
    {
        SerializedProperty p = new SerializedObject(c).FindProperty("activeFrames");
        var r = new Sprite[p.arraySize];
        for (int i = 0; i < r.Length; i++) { r[i] = p.GetArrayElementAtIndex(i).objectReferenceValue as Sprite; }
        return r.Where(s => s != null).ToArray();
    }

    private static void Amostrar()
    {
        if (sr == null || sr.sprite == null || !sr.enabled || !sr.gameObject.activeInHierarchy) { sumiu = true; return; }
        vistos.Add(sr.sprite.name);
    }

    private static void Conferir(string nome, int minimo)
    {
        var fora = vistos.Where(v => !chama.Contains(v)).ToList();
        if (sumiu) { Falha($"{nome}: a chama sumiu"); }
        if (vistos.Count < minimo) { Falha($"{nome}: só {vistos.Count} quadros ({string.Join(",", vistos)})"); }
        if (fora.Count > 0) { Falha($"{nome}: quadros fora da chama: {string.Join(",", fora)}"); }
        if (altar.transform.position != pos0 || altar.transform.lossyScale != escala0) { Falha($"{nome}: o altar mexeu"); }
        Bounds b = altar.GetComponent<Collider2D>().bounds;
        if (b.center != colisor0.center || b.size != colisor0.size || !altar.GetComponent<Collider2D>().isTrigger) { Falha($"{nome}: o colisor mudou"); }
        if (sr.sortingOrder != ordem0 || sr.sortingLayerID != camada0) { Falha($"{nome}: a ordem mudou"); }
        var srJogador = jogador != null ? jogador.GetComponentsInChildren<SpriteRenderer>().Max(r => r.sortingOrder) : int.MinValue;
        // Empate antigo (altar e Body na ordem 0 desde o 1º commit): pendência registrada na 13B.2, não falha da chama.
        // A ordem -2 foi testada e descartada: muros e o cais do Prólogo passavam a cobrir o altar (AltarFlameProbe.Comparar).
        if (sr.sortingLayerID == 0 && sr.sortingOrder >= srJogador) { Debug.LogWarning($"[Altar] {nome}: PENDÊNCIA — altar ({sr.sortingOrder}) empata com o jogador ({srJogador})"); }
        Debug.Log($"[Altar] {nome}: {vistos.Count} quadros ({string.Join(",", vistos.OrderBy(v => v))}); ordem altar {sr.sortingOrder} / jogador {srJogador}; posição, escala e colisor iguais — {(falhas == 0 ? "OK" : "ver falhas")}");
    }

    /// <summary>Sprites da camada do altar que encostam nele, com a ordem: o que mudaria se o altar descesse de ordem.</summary>
    private static void Vizinhos()
    {
        Bounds b = sr.bounds;
        foreach (SpriteRenderer r in Object.FindObjectsByType<SpriteRenderer>())
        {
            if (r == sr || !r.enabled || r.sortingLayerID != sr.sortingLayerID || !r.bounds.Intersects(b)) { continue; }
            if (r.transform.IsChildOf(jogador.transform)) { continue; }
            if (r.sortingOrder < -3 || r.sortingOrder > 3) { continue; }
            Debug.Log($"[Altar] vizinho {r.name} ({r.sprite?.name}): ordem {r.sortingOrder}");
        }
    }

    /// <summary>Registra quem desenha por cima: camada, ordem e z do altar e de cada sprite do jogador.</summary>
    private static void Profundidade()
    {
        foreach (SpriteRenderer r in jogador.GetComponentsInChildren<SpriteRenderer>())
        {
            Debug.Log($"[Altar] jogador {r.name}: camada {r.sortingLayerName} ordem {r.sortingOrder} z {r.transform.position.z:0.00} em {r.transform.position}");
        }
        Debug.Log($"[Altar] altar: camada {sr.sortingLayerName} ordem {sr.sortingOrder} z {sr.transform.position.z:0.00} em {sr.transform.position}; câmera z {Camera.main.transform.position.z:0.0}, transparencySortMode {Camera.main.transparencySortMode}");
    }

    private static Texture2D Recorte()
    {
        Camera cam = Camera.main;
        var rt = new RenderTexture(1280, 720, 24) { filterMode = FilterMode.Point };
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        Vector3 tela = cam.WorldToScreenPoint(pos0 + Vector3.up * 0.9f);
        // Escala da câmera do 1280×720 para a tela do batch.
        float sx = 1280f / cam.pixelWidth, sy = 720f / cam.pixelHeight;
        int w = 220, h = 260;
        int x = Mathf.Clamp(Mathf.RoundToInt(tela.x * sx) - w / 2, 0, 1280 - w);
        int y = Mathf.Clamp(Mathf.RoundToInt(tela.y * sy) - h / 2, 0, 720 - h);
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(x, y, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;
        rt.Release();
        Debug.Log($"[Altar] foto {fotos.Count + 1}: {sr.sprite.name}");
        return tex;
    }

    private static void Proxima()
    {
        cena++;
        if (cena >= Cenas.Length) { Encerrar(); return; }
        passo = 0;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(Cenas[cena]);
        acordar = Agora + 1.0;
        Debug.Log($"[Altar] 6-7. trocando de cena → {Cenas[cena]}");
    }

    private static void Encerrar()
    {
        EditorApplication.update -= Tick;
        if (fotos.Count > 0)
        {
            int w = fotos[0].width, h = fotos[0].height;
            var tira = new Texture2D(w * fotos.Count + 4 * (fotos.Count - 1), h, TextureFormat.RGB24, false);
            for (int i = 0; i < fotos.Count; i++) { tira.SetPixels(i * (w + 4), 0, w, h, fotos[i].GetPixels()); }
            tira.Apply();
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(saida));
            System.IO.File.WriteAllBytes(saida, tira.EncodeToPNG());
            Debug.Log($"[Altar] {saida}");
        }
        Debug.Log(falhas == 0 ? "[Altar] RESULTADO: OK" : $"[Altar] RESULTADO: {falhas} FALHA(S)");
        EditorApplication.ExitPlaymode();
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;
        EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
        EditorSettings.enterPlayModeOptions = opcoes;
        EditorApplication.Exit(falhas == 0 ? 0 : 1);
    }
}
