using UnityEditor;
using UnityEngine;

/// <summary>
/// QA-04 — o Odisseu grudava na lateral das plataformas no ar enquanto a direção era segurada:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod PlayerPhysicsFix.Run
///
/// Causa: o colisor do jogador não tinha PhysicsMaterial2D (nem o Physics2D um material padrão), e o atrito contra a
/// PAREDE segurava o corpo — medido em Calipso: 3 de 3 pulos presos em x = 11,70, sem subir um degrau de 1,2 un com um
/// pulo de 1,71.
///
/// Correção mínima: um material de atrito 0 SÓ no CapsuleCollider2D do prefab Player. Nada mais depende desse atrito —
/// o PlayerController escreve a velocidade horizontal a cada passo, as plataformas móveis carregam o jogador somando o
/// FrameDelta (sem atrito, por projeto) e os chãos são caixas planas. Velocidade, pulo, colisor e dano não mudam.
/// Idempotente.
/// </summary>
public static class PlayerPhysicsFix
{
    private const string Pasta = "Assets/Settings/Physics";
    private const string Material = Pasta + "/Player_NoFriction.physicsMaterial2D";
    private const string Prefab = "Assets/Prefabs/Player.prefab";

    public static void Run()
    {
        bool ok = Aplicar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    private static bool Aplicar()
    {
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Material);
        if (material == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings")) { AssetDatabase.CreateFolder("Assets", "Settings"); }
            if (!AssetDatabase.IsValidFolder(Pasta)) { AssetDatabase.CreateFolder("Assets/Settings", "Physics"); }
            material = new PhysicsMaterial2D("Player_NoFriction") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(material, Material);
        }

        GameObject raiz = PrefabUtility.LoadPrefabContents(Prefab);
        try
        {
            var capsula = raiz.GetComponent<CapsuleCollider2D>();
            if (capsula == null) { Debug.LogError("[Física] Player sem CapsuleCollider2D"); return false; }
            if (capsula.sharedMaterial == material) { Debug.Log("[Física] Player já com atrito 0"); return true; }

            capsula.sharedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(raiz, Prefab);
            Debug.Log("[Física] Player: CapsuleCollider2D com Player_NoFriction (atrito 0)");
            return true;
        }
        finally { PrefabUtility.UnloadPrefabContents(raiz); }
    }
}
