using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Enemies;
using Odisseia.Levels;
using Odisseia.Systems;

/// <summary>
/// Inventário de personagens de uma fase — a Etapa 1 de todo polimento de personagem:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CastProbe.Run
///           -probeScene Assets/Scenes/Levels/Level_02_Troia.unity
///
/// Lista quem tem cara (SpriteRenderer) e papel: jogador, inimigo, chefe, NPC que conversa,
/// gatilho de diálogo, qualquer coisa com SpriteAnimator e qualquer sprite vindo de uma pasta
/// de personagem. Varre inativos, porque metade do elenco de uma fase nasce desligada e um ato
/// liga depois. Ler o YAML da cena não serve: ele não enxerga as instâncias de prefab.
///
/// Para cada um: caminho, estado, papel, textura, px/un, altura no mundo e escala do Transform —
/// os três números que denunciam arte pintada esticada (px/un fora de 42,857 ou escala != 1).
///
/// -probeArt Cattle,Herd  inclui também figuras que NÃO estão em pasta de personagem nem têm papel
/// (animal de cenário, por exemplo), casando o nome do objeto ou o caminho da textura. Sem ele, um
/// elenco feito de props de ambiente sai invisível — foi o que aconteceu com o gado da Fase 12.
/// </summary>
public static class CastProbe
{
    private const float PpuDoProjeto = 42.857143f;

    private static string ScenePath
    {
        get
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-probeScene") { return args[i + 1]; }
            }
            return "Assets/Scenes/Levels/Level_01_Itaca_Prologue.unity";
        }
    }

    private static string[] ArteExtra
    {
        get
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-probeArt") { return args[i + 1].Split(','); }
            }
            return new string[0];
        }
    }

    public static void Run()
    {
        string[] arteExtra = ArteExtra;
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var linhas = new List<string>();
        var porTextura = new Dictionary<string, int>();

        foreach (GameObject raiz in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (SpriteRenderer sr in raiz.GetComponentsInChildren<SpriteRenderer>(true))
            {
                string papel = Papel(sr.transform);
                string textura = sr.sprite != null ? AssetDatabase.GetAssetPath(sr.sprite.texture) : "(sem sprite)";
                bool pastaDePersonagem = textura.Contains("/Characters/") || textura.Contains("/Enemies/")
                    || textura.Contains("/NPCs/") || textura.Contains("/Figures/");
                bool pedida = arteExtra.Any(a => sr.name.Contains(a) || textura.Contains(a));
                if (papel == null && !pastaDePersonagem && !pedida) { continue; }

                float ppu = sr.sprite != null ? sr.sprite.pixelsPerUnit : 0f;
                float altura = sr.sprite != null ? sr.sprite.bounds.size.y * Mathf.Abs(sr.transform.lossyScale.y) : 0f;
                float escala = Mathf.Abs(sr.transform.lossyScale.y);
                string alerta = (Mathf.Abs(ppu - PpuDoProjeto) > 0.01f ? " PPU!" : "")
                    + (Mathf.Abs(escala - 1f) > 0.01f ? " ESCALA!" : "");

                linhas.Add(string.Format("[Cast] {0} | {1} | {2} | {3} | {4} | ppu {5:0.###} | altura {6:0.00} un | escala {7:0.00}{8}",
                    Caminho(sr.transform), sr.gameObject.activeInHierarchy ? (sr.enabled ? "ativo" : "renderer DESLIGADO") : "INATIVO",
                    papel ?? "figura", textura, sr.sprite != null ? sr.sprite.name : "-", ppu, altura, escala, alerta)
                    + (pedida ? string.Format(" | x {0:0.00}..{1:0.00} y {2:0.00}..{3:0.00} ordem {4} cor {5}",
                        sr.bounds.min.x, sr.bounds.max.x, sr.bounds.min.y, sr.bounds.max.y, sr.sortingOrder, sr.color) : ""));
                porTextura[textura] = porTextura.TryGetValue(textura, out int n) ? n + 1 : 1;
            }

            // Papel sem desenho próprio (gatilho de diálogo em cena vazia, por exemplo).
            foreach (DialogueTrigger d in raiz.GetComponentsInChildren<DialogueTrigger>(true))
            {
                linhas.Add("[Cast] " + Caminho(d.transform) + " | gatilho de diálogo (sem figura)");
            }
        }

        Debug.Log("[Cast] cena: " + ScenePath);
        foreach (string l in linhas) { Debug.Log(l); }

        var raizes = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        // Animator/AnimatorController: o projeto não usa (o SpriteAnimator troca sprites pelo
        // nome). Contar em vez de presumir — se algum aparecer, os estados vêm dele, não da folha.
        int animators = raizes.Sum(r => r.GetComponentsInChildren<Animator>(true).Length);
        Debug.Log("[Cast] Animator na cena: " + animators);

        // Variante de inimigo = instância do prefab com campo de comportamento sobrescrito.
        // É o único jeito de dois EnemyBasic diferirem em gameplay.
        foreach (EnemyController e in raizes.SelectMany(r => r.GetComponentsInChildren<EnemyController>(true)))
        {
            var mods = PrefabUtility.GetPropertyModifications(e.gameObject);
            string campos = mods == null ? "" : string.Join(", ", mods
                .Where(m => !(m.target is Transform) && !m.propertyPath.StartsWith("m_Local") && !m.propertyPath.StartsWith("m_Name"))
                .Select(m => m.target.GetType().Name + "." + m.propertyPath + "=" + m.value)
                .Distinct());
            Debug.Log(string.Format("[Cast] inimigo em x={0:0.0}: overrides {1}", e.transform.position.x,
                campos.Length > 0 ? campos : "(nenhum — igual ao prefab)"));
        }

        // Chefe: o que a arte precisa respeitar — onde está, que caixa de acerto tem, onde golpeia.
        foreach (BossController b in raizes.SelectMany(r => r.GetComponentsInChildren<BossController>(true)))
        {
            var so = new SerializedObject(b);
            var pontos = so.FindProperty("attackPoints");
            var lista = new List<string>();
            for (int i = 0; pontos != null && i < pontos.arraySize; i++)
            {
                var t = pontos.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                if (t != null) { lista.Add($"{t.name}@({t.position.x:0.0},{t.position.y:0.0})"); }
            }
            var vida = b.GetComponent<Odisseia.Core.HealthSystem>();
            Debug.Log($"[Cast] CHEFE {b.name} em ({b.transform.position.x:0.0},{b.transform.position.y:0.0}); " +
                $"intervalo {so.FindProperty("attackInterval").floatValue}s, aviso {so.FindProperty("telegraphDuration").floatValue}s, " +
                $"recuperação {so.FindProperty("recoveryDuration").floatValue}s, raio {so.FindProperty("attackRadius").floatValue}; " +
                $"pontos: {string.Join(" ", lista)}; vida {(vida != null ? new SerializedObject(vida).FindProperty("maxHealth").intValue.ToString() : "nenhuma")}");
            foreach (Collider2D c in b.GetComponentsInChildren<Collider2D>(true))
            {
                Debug.Log($"[Cast]   colisor {c.name} {c.GetType().Name} trigger={c.isTrigger} " +
                    $"x {c.bounds.min.x:0.00}..{c.bounds.max.x:0.00} y {c.bounds.min.y:0.00}..{c.bounds.max.y:0.00} layer {LayerMask.LayerToName(c.gameObject.layer)}");
            }
            foreach (SpriteRenderer s in b.GetComponentsInChildren<SpriteRenderer>(true))
            {
                Debug.Log($"[Cast]   desenho {s.name} x {s.bounds.min.x:0.00}..{s.bounds.max.x:0.00} y {s.bounds.min.y:0.00}..{s.bounds.max.y:0.00} ordem {s.sortingOrder}");
            }
        }

        // Perseguidor: ameaça que mata ao toque e anda sozinha (gigante, avalanche, maré). Não é
        // EnemyController nem BossController, e o dresser de cenário pode ter DESLIGADO o desenho dele
        // junto com os placeholders — ameaça mortal invisível. Por isso o renderer é listado mesmo desligado.
        foreach (PursuerHazard ph in raizes.SelectMany(r => r.GetComponentsInChildren<PursuerHazard>(true)))
        {
            var so = new SerializedObject(ph);
            var c = ph.GetComponent<Collider2D>();
            var sr = ph.GetComponent<SpriteRenderer>();
            Debug.Log($"[Cast] PERSEGUIDOR {ph.name} em ({ph.transform.position.x:0.0},{ph.transform.position.y:0.0}); " +
                $"velocidade {so.FindProperty("speed").floatValue}, reposiciona {so.FindProperty("resetOffsetX").floatValue} atrás do respawn; " +
                (c != null ? $"colisor x {c.bounds.min.x:0.00}..{c.bounds.max.x:0.00} y {c.bounds.min.y:0.00}..{c.bounds.max.y:0.00}; " : "sem colisor; ") +
                (sr != null ? $"desenho {(sr.enabled ? "LIGADO" : "DESLIGADO")} ({(sr.sprite != null ? sr.sprite.name : "-")})" : "sem SpriteRenderer") +
                $"; filhos {ph.transform.childCount}");
        }

        // Zona de interação da fase (gatilho com script de Odisseia.Levels): onde o jogador interage
        // e se a arte que o jogador VÊ cai sobre ela. O script pode mexer só no renderer do próprio
        // gatilho, e a arte pode ser outro objeto — então os dois são listados.
        foreach (MonoBehaviour m in raizes.SelectMany(r => r.GetComponentsInChildren<MonoBehaviour>(true))
            .Where(m => m != null && m.GetType().Namespace == "Odisseia.Levels"))
        {
            var c = m.GetComponent<Collider2D>();
            if (c == null || !c.isTrigger && m.GetType().Name != "SacredCattleZone") { continue; }
            var proprio = m.GetComponent<SpriteRenderer>();
            var sobre = raizes.SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true))
                .Where(s => s != proprio && s.sprite != null && arteExtra.Any(a => s.name.Contains(a) || AssetDatabase.GetAssetPath(s.sprite.texture).Contains(a))
                    && s.bounds.min.x <= c.bounds.center.x && s.bounds.max.x >= c.bounds.center.x)
                .Select(s => s.name);
            Debug.Log($"[Cast] ZONA {m.GetType().Name} {Caminho(m.transform)} colisor x {c.bounds.min.x:0.00}..{c.bounds.max.x:0.00} " +
                $"y {c.bounds.min.y:0.00}..{c.bounds.max.y:0.00}; renderer próprio " +
                (proprio == null ? "nenhum" : (proprio.enabled ? "LIGADO" : "desligado") + $" ({(proprio.sprite != null ? proprio.sprite.name : "-")})") +
                $"; arte pedida sobre ela: {string.Join(", ", sobre)}");
        }

        // Scripts de fase na cena, para achar papel de personagem que não usa EnemyController
        // (perseguidor, perigo com figura, gatilho de ato).
        var tipos = raizes.SelectMany(r => r.GetComponentsInChildren<MonoBehaviour>(true))
            .Where(m => m != null && m.GetType().Namespace != null && m.GetType().Namespace.StartsWith("Odisseia"))
            .GroupBy(m => m.GetType().Name).OrderBy(g => g.Key);
        Debug.Log("[Cast] scripts do jogo na cena: " + string.Join(", ", tipos.Select(g => g.Key + " x" + g.Count())));
        foreach (var par in porTextura.OrderByDescending(p => p.Value))
        {
            Debug.Log(string.Format("[Cast] textura {0} x{1}", par.Key, par.Value));
        }
        Debug.Log("[Cast] total: " + linhas.Count);
        EditorApplication.Exit(0);
    }

    private static string Papel(Transform t)
    {
        for (Transform p = t; p != null; p = p.parent)
        {
            if (p.GetComponent<BossController>() != null) { return "CHEFE"; }
            if (p.GetComponent<EnemyController>() != null) { return "inimigo"; }
            if (p.GetComponent<NPCDialogue>() != null) { return "NPC"; }
            if (p.GetComponent("PlayerController") != null) { return "JOGADOR"; }
        }
        return t.GetComponent<SpriteAnimator>() != null ? "animado" : null;
    }

    private static string Caminho(Transform t)
    {
        var sb = new StringBuilder(t.name);
        for (Transform p = t.parent; p != null; p = p.parent) { sb.Insert(0, p.name + "/"); }
        return sb.ToString();
    }
}
