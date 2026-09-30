using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Core;
using Odisseia.Enemies;
using Odisseia.Levels;
using Odisseia.Systems;

/// <summary>
/// BUG-001 (Docs/QA/FULL_CAMPAIGN_PLAYTEST.md) — Fase 07, gigante mata durante a abertura. SCENE OVERRIDE:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod LestrigoesChaseFix.Apply
///
/// 1. Põe o <see cref="LestrigoesChaseGuard"/> na cena, ligado ao PursuerHazard, aos dois gigantes (BossController),
///    à fala de abertura da LevelIntro e à vida do jogador.
/// 2. Move o Checkpoint de x = 6,0 para x = 1,5: em 6,0 ele ficava DENTRO da área do golpe do gigante em x = 5
///    (raio 1,4 → 3,6..6,4) — quem morria depois dele reaparecia sob o golpe.
/// Nenhum prefab, script de sistema ou outra fase é tocado. Idempotente.
/// </summary>
public static class LestrigoesChaseFix
{
    private const string Cena = "Assets/Scenes/Levels/Level_07_Lestrigoes.unity";
    private const float CheckpointSeguroX = 1.5f;

    public static void Apply()
    {
        EditorSceneManager.OpenScene(Cena, OpenSceneMode.Single);
        var pursuer = Object.FindAnyObjectByType<PursuerHazard>(FindObjectsInactive.Include);
        var giants = Object.FindObjectsByType<BossController>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(b => b.transform.position.x).ToArray();
        var intro = Object.FindAnyObjectByType<LevelIntro>(FindObjectsInactive.Include);
        var player = GameObject.FindGameObjectWithTag("Player");
        var checkpoint = Object.FindAnyObjectByType<Checkpoint>(FindObjectsInactive.Include);
        if (pursuer == null || intro == null || player == null || checkpoint == null)
        {
            Debug.LogError("[BUG-001] faltou perseguidor, LevelIntro, jogador ou checkpoint na cena");
            EditorApplication.Exit(1);
            return;
        }
        var fala = new SerializedObject(intro).FindProperty("introDialogue").objectReferenceValue;

        var guarda = Object.FindAnyObjectByType<LestrigoesChaseGuard>(FindObjectsInactive.Include);
        if (guarda == null)
        {
            guarda = new GameObject("LestrigoesChaseGuard").AddComponent<LestrigoesChaseGuard>();
        }
        var so = new SerializedObject(guarda);
        so.FindProperty("pursuer").objectReferenceValue = pursuer;
        var lista = so.FindProperty("giants");
        lista.arraySize = giants.Length;
        for (int i = 0; i < giants.Length; i++) { lista.GetArrayElementAtIndex(i).objectReferenceValue = giants[i]; }
        so.FindProperty("introDialogue").objectReferenceValue = fala;
        so.FindProperty("playerHealth").objectReferenceValue = player.GetComponent<HealthSystem>();
        so.FindProperty("levelManager").objectReferenceValue = Object.FindAnyObjectByType<LevelManager>(FindObjectsInactive.Include);
        so.ApplyModifiedPropertiesWithoutUndo();

        float antes = checkpoint.transform.position.x;
        checkpoint.transform.position = new Vector3(CheckpointSeguroX, checkpoint.transform.position.y, checkpoint.transform.position.z);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[BUG-001] guarda ligada: perseguidor {pursuer.name}, gigantes {giants.Length} " +
                  $"(x {string.Join(", ", giants.Select(g => g.transform.position.x.ToString("0.0")))}), fala {(fala != null ? fala.name : "NENHUMA")}; " +
                  $"checkpoint x {antes:0.0} -> {CheckpointSeguroX:0.0}");
        EditorApplication.Exit(0);
    }
}
