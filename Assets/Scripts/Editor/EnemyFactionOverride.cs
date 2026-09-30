using UnityEditor;
using UnityEngine;
using Odisseia.Enemies;
using Odisseia.Systems;

/// <summary>
/// Veste os inimigos da cena aberta com a arte da facção da fase — por OVERRIDE nas instâncias,
/// nunca no prefab. O EnemyBasic é compartilhado por sete fases e cada uma tem a sua facção
/// (Docs/Characters/Fase02/TROY_CAST.md, D-018).
///
/// Só o visual muda: folha do SpriteAnimator, sprite inicial, escala do corpo (a arte antiga
/// importava a 100 px/un e o prefab a encolhia para 0,82; a nova está na densidade do projeto)
/// e ordem de desenho (na 0 o inimigo sumia atrás dos props de ordem 1). Colisor, vida, patrulha,
/// dano, FPS e loop por estado continuam os do prefab.
/// </summary>
public static class EnemyFactionOverride
{
    /// <param name="resources">Caminho dentro de Resources, sem extensão (ex.: Odisseia/Enemies/CHR_Trojan_Soldier).</param>
    /// <returns>Quantos inimigos foram vestidos, ou -1 se a folha não existir.</returns>
    public static int Aplicar(string resources, string tag)
    {
        string nome = resources.Substring(resources.LastIndexOf('/') + 1);
        Sprite parado = null;
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + resources + ".png"))
        {
            if (asset is Sprite s && s.name == nome + "_Idle_00") { parado = s; }
        }
        if (parado == null)
        {
            Debug.LogError($"[{tag}] folha {resources} não encontrada — rodar node Tools/build-cast-sheets.js antes");
            return -1;
        }

        int n = 0;
        foreach (EnemyController inimigo in Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var animador = inimigo.GetComponentInChildren<SpriteAnimator>(true);
            if (animador == null) { continue; }

            animador.transform.localScale = Vector3.one;
            var sr = animador.GetComponent<SpriteRenderer>();
            sr.sprite = parado;
            sr.sortingOrder = 2;   // plano do jogador: inimigo é leitura de gameplay

            var so = new SerializedObject(animador);
            so.FindProperty("resourcePath").stringValue = resources;
            so.ApplyModifiedPropertiesWithoutUndo();
            n++;
            Debug.Log($"[{tag}] {nome} em x={inimigo.transform.position.x:0.0}");
        }
        return n;
    }
}
