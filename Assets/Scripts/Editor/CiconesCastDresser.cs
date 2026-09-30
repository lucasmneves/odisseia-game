using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Elenco da Fase 03 — Cicones (Docs/Characters/Fase03/CICONES_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CiconesCastDresser.Dress
///
/// O CastProbe da fase achou só o Odisseu e três EnemyBasic sem nenhum override — nenhum NPC,
/// nenhum grego, nenhum personagem narrativo. Então o único trabalho é vestir os três inimigos
/// de guerreiros cicones, por override na cena; o prefab compartilhado fica intacto.
/// Separado do CiconesSceneDresser, que veste o cenário.
/// </summary>
public static class CiconesCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_03_Cicones.unity";
    private const string CiconeResources = "Odisseia/Enemies/CHR_Cicones_Warrior";

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        int inimigos = EnemyFactionOverride.Aplicar(CiconeResources, "CiconesCast");
        if (inimigos <= 0)
        {
            EditorApplication.Exit(1);
            return;
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[CiconesCast] {inimigos} inimigos viraram guerreiros cicones.");
        EditorApplication.Exit(0);
    }
}
