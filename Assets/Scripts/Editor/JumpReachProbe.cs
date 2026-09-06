using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Mede se o pulo ainda alcança a geometria das fases:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod JumpReachProbe.Medir
///
/// Existe porque mexer em <c>jumpForce</c> é uma mudança de número que pode tornar uma fase
/// impossível sem gerar nenhum erro. A altura de pulo sai de v²/(2g) — não é o valor do
/// impulso — e comparar isso à mão contra dezenas de plataformas não escala.
///
/// O que ele reporta é o DEGRAU: para cada superfície de chão, a subida até a superfície mais
/// próxima acima que ainda se sobrepõe a ela em X. Degrau maior que a altura de um pulo é o
/// que passa a exigir o pulo duplo; maior que dois pulos é o que fica inalcançável.
/// </summary>
public static class JumpReachProbe
{
    public static void Medir()
    {
        // Os números vêm do prefab, não de constantes daqui: um probe com a física copiada à
        // mão mente no dia em que alguém mexe no prefab e não aqui.
        float impulso = 10.04f, gravidade = 3f, maxJumps = 2f;
        GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (player != null)
        {
            var pc = player.GetComponent<Odisseia.Player.PlayerController>();
            if (pc != null)
            {
                var so = new SerializedObject(pc);
                impulso = so.FindProperty("jumpForce").floatValue;
                gravidade = so.FindProperty("gravityScale").floatValue;
                maxJumps = so.FindProperty("maxJumps").intValue;
            }
        }

        float g = Mathf.Abs(Physics2D.gravity.y) * gravidade;
        float alturaUmPulo = impulso * impulso / (2f * g);
        Debug.Log($"[Salto] impulso {impulso}, gravidade efetiva {g:0.00} -> " +
                  $"1 pulo = {alturaUmPulo:0.00} un, {maxJumps} pulos = {alturaUmPulo * maxJumps:0.00} un");

        foreach (string caminho in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" })
                     .Select(AssetDatabase.GUIDToAssetPath).OrderBy(c => c))
        {
            EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
            MedirCena(caminho, alturaUmPulo, alturaUmPulo * maxJumps);
        }

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static void MedirCena(string cena, float umPulo, float todos)
    {
        int chao = LayerMask.NameToLayer("Ground");
        List<(string nome, Bounds b)> supes = UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None)
            .Where(c => c.gameObject.layer == chao && !c.isTrigger)
            .Select(c => (nome: c.gameObject.name, b: c.bounds))
            .ToList();

        var duros = new List<string>();
        var impossiveis = new List<string>();

        foreach ((string nome, Bounds b) in supes)
        {
            // Superfície mais baixa acima desta que ainda se sobrepõe em X: é onde o jogador
            // parado em cima de `b` tentaria subir.
            float melhor = float.MaxValue;
            Bounds alvo = default;
            foreach ((string _, Bounds o) in supes)
            {
                if (o.max.y <= b.max.y + 0.05f) continue;
                if (o.min.x > b.max.x || o.max.x < b.min.x) continue;
                float degrau = o.max.y - b.max.y;
                if (degrau < melhor) { melhor = degrau; alvo = o; }
            }

            if (melhor == float.MaxValue) continue;

            string onde = $"{nome}: x {b.center.x.ToString("0.0", CultureInfo.InvariantCulture)}, " +
                          $"degrau {melhor.ToString("0.00", CultureInfo.InvariantCulture)} un " +
                          $"(y {b.max.y:0.0} -> {alvo.max.y:0.0})";
            if (melhor > todos) impossiveis.Add(onde);
            else if (melhor > umPulo) duros.Add(onde);
        }

        Debug.Log($"[Salto] {cena}: {supes.Count} superficies de chao, " +
                  $"{duros.Count} exigem pulo duplo, {impossiveis.Count} fora de alcance");
        foreach (string s in duros.Take(8)) Debug.Log("[Salto]   duplo: " + s);
        foreach (string s in impossiveis.Take(8)) Debug.LogWarning("[Salto]   INALCANCAVEL: " + s);
    }
}
