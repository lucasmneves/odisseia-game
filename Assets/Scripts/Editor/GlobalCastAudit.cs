using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Enemies;
using Odisseia.Levels;
using Odisseia.Systems;

/// <summary>
/// Auditoria de elenco das 16 cenas numa rodada (Docs/Characters/FINAL_CHARACTER_ART_POLISH.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod GlobalCastAudit.Run
///
/// Por cena: cada figura de personagem (jogador, inimigo, chefe, NPC, qualquer SpriteAnimator, qualquer textura de
/// pasta de personagem) com folha, px/un, escala, ordem e alertas; cenário empatado com a ordem do jogador (o defeito
/// da jangada da Fase 13); primeiro plano cobrindo personagem parado (a grama da Fase 14); e o EnemyBasic que ainda
/// usa a folha carmesim antiga. Linhas com "!" são achados. Não salva nada.
/// </summary>
public static class GlobalCastAudit
{
    private const float Ppu = 42.857143f;

    public static void Run()
    {
        var cenas = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Levels" })
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
        foreach (string cena in cenas)
        {
            EditorSceneManager.OpenScene(cena, OpenSceneMode.Single);
            string nome = System.IO.Path.GetFileNameWithoutExtension(cena);
            var raizes = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            var todos = raizes.SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();

            var jogador = todos.FirstOrDefault(s => s.GetComponentInParent(System.Type.GetType("Odisseia.Player.PlayerController, Assembly-CSharp"), true) != null && s.name == "Body");
            int ordemJogador = jogador != null ? jogador.sortingOrder : 2;
            // O Body do jogador é 0 no prefab, mas o que vale é a ordem efetiva do plano: inimigos em 2 (EnemyFactionOverride).
            Debug.Log($"[Audit] ===== {nome} (jogador ordem {ordemJogador})");

            var figuras = todos.Where(s => s.sprite != null && EhPersonagem(s)).ToArray();
            foreach (var s in figuras)
            {
                string tex = AssetDatabase.GetAssetPath(s.sprite.texture);
                float esc = Mathf.Abs(s.transform.lossyScale.y);
                string alerta = (Mathf.Abs(s.sprite.pixelsPerUnit - Ppu) > 0.01f ? " !PPU" : "")
                    + (Mathf.Abs(esc - 1f) > 0.01f ? " !ESCALA" : "")
                    + (tex.Contains("CHR_Enemy_Basic") ? " !CARMESIM_ANTIGO" : "")
                    + (tex.Contains("/Art/Odisseia/Characters/NPCs/CHR_NPC_") ? " !PINTADO" : "");
                Debug.Log($"[Audit] {nome} | {Papel(s.transform)} | {Caminho(s.transform)} | {System.IO.Path.GetFileNameWithoutExtension(tex)} | " +
                    $"ppu {s.sprite.pixelsPerUnit:0.###} | esc {esc:0.00} | ordem {s.sortingOrder} | {s.sortingLayerName} | " +
                    $"{(s.gameObject.activeInHierarchy ? (s.enabled ? "ativo" : "desligado") : "inativo")} | y {s.bounds.min.y:0.00}{alerta}");
            }

            // Cenário entre o plano do jogador (Body, ordem 0 no prefab) e o primeiro plano (>= 10), na faixa de altura
            // em que ele anda: desenha POR CIMA do Odisseu (ou empata, na ordem 0). Foi o caso da jangada da Fase 13.
            foreach (var s in todos.Where(s => s.enabled && s.sprite != null && s.gameObject.activeInHierarchy && !EhPersonagem(s)
                && s.sortingOrder >= ordemJogador && s.sortingOrder < 10 && s.bounds.min.y < 0.5f && s.bounds.max.y > -2f
                && s.bounds.size.x < 30f))
            {
                Debug.Log($"[Audit] {nome} | !CENARIO_SOBRE_JOGADOR | {Caminho(s.transform)} ordem {s.sortingOrder} x {s.bounds.min.x:0.00}..{s.bounds.max.x:0.00} y {s.bounds.min.y:0.00}..{s.bounds.max.y:0.00}");
            }
            // Personagem parado na MESMA ordem do jogador: quem fica na frente ao passar é arbitrário.
            foreach (var f in figuras.Where(f => f.sortingOrder == ordemJogador && Papel(f.transform) is string p && (p == "NPC" || p == "figura")))
            {
                Debug.Log($"[Audit] {nome} | !EMPATE_COM_JOGADOR | {Caminho(f.transform)} ordem {f.sortingOrder}");
            }

            // Primeiro plano (ordem >= 10, não faixa de tela inteira) cobrindo personagem PARADO (NPC, figura, chefe).
            var parados = figuras.Where(f => f.enabled && f.gameObject.activeInHierarchy && Papel(f.transform) is string p
                && (p == "NPC" || p == "figura" || p == "CHEFE")).ToArray();
            foreach (var fg in todos.Where(s => s.enabled && s.sprite != null && s.sortingOrder >= 10 && s.bounds.size.x < 10f
                && s.gameObject.activeInHierarchy && !EhPersonagem(s)))
            {
                foreach (var f in parados)
                {
                    Bounds a = fg.bounds, b = f.bounds;
                    if (a.max.x > b.min.x && a.min.x < b.max.x && a.max.y > b.min.y + 0.1f && a.min.y < b.max.y)
                    {
                        float cobre = Mathf.Min(a.max.y, b.max.y) - b.min.y;
                        Debug.Log($"[Audit] {nome} | !PRIMEIRO_PLANO | {Caminho(fg.transform)} (ordem {fg.sortingOrder}) cobre {Caminho(f.transform)} até {cobre:0.00} un acima dos pés");
                    }
                }
            }
        }
        EditorApplication.Exit(0);
    }

    private static bool EhPersonagem(SpriteRenderer s)
    {
        if (s.sprite == null) { return false; }
        string tex = AssetDatabase.GetAssetPath(s.sprite.texture);
        return Papel(s.transform) != null || tex.Contains("/Characters/") || tex.Contains("/Enemies/")
            || tex.Contains("/NPCs/") || tex.Contains("SacredCattle");
    }

    private static string Papel(Transform t)
    {
        for (Transform p = t; p != null; p = p.parent)
        {
            if (p.GetComponent<BossController>() != null) { return "CHEFE"; }
            if (p.GetComponent<EnemyController>() != null) { return "inimigo"; }
            if (p.GetComponent<PursuerHazard>() != null) { return "PERSEGUIDOR"; }
            if (p.GetComponent<NPCDialogue>() != null) { return "NPC"; }
            if (p.GetComponent("PlayerController") != null) { return "JOGADOR"; }
            if (p.GetComponent("SacredCattleZone") != null) { return "figura"; }
        }
        if (t.GetComponent<SpriteAnimator>() != null)
        {
            string tex = t.GetComponent<SpriteRenderer>().sprite != null ? AssetDatabase.GetAssetPath(t.GetComponent<SpriteRenderer>().sprite.texture) : "";
            return tex.Contains("/Characters/") || tex.Contains("/Enemies/") ? "figura" : null;
        }
        return null;
    }

    private static string Caminho(Transform t)
    {
        string c = t.name;
        for (Transform p = t.parent; p != null; p = p.parent) { c = p.name + "/" + c; }
        return c;
    }
}
