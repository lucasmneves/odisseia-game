using UnityEditor;
using UnityEngine;
using Odisseia.Player;
using Odisseia.Systems;

/// <summary>
/// Confere que o ciclo de corrida acompanha a velocidade — ou seja, que o pé não patina:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod LocomotionProbe.Medir
///
/// A conta é o **comprimento da passada**: velocidade × duração do ciclo ÷ 2 passadas. Se ele
/// muda entre andar e correr, o pé escorrega no chão — que é exatamente a leitura de "andando
/// rápido" que a corrida acabou de deixar de ter. Com o ritmo preso à velocidade a passada
/// tem de sair CONSTANTE, e é isso que este probe verifica.
///
/// Existe porque nada aqui é erro de compilação: mexer em <c>sprintMultiplier</c>, no FPS do
/// Run ou nos limites do ritmo pode voltar a soltar o pé do chão sem nenhum aviso.
/// </summary>
public static class LocomotionProbe
{
    /// <summary>Quantas passadas o ciclo cobre. Um ciclo de corrida tem duas, uma por perna.</summary>
    private const int PassadasPorCiclo = 2;

    public static void Medir()
    {
        bool falhou = false;
        GameObject player = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (player == null)
        {
            Debug.LogError("[Passada] Player.prefab não encontrado");
            EditorApplication.Exit(1);
            return;
        }

        var pc = new SerializedObject(player.GetComponent<PlayerController>());
        float maxSpeed = pc.FindProperty("maxSpeed").floatValue;
        float sprint = pc.FindProperty("sprintMultiplier").floatValue;

        var pa = new SerializedObject(player.GetComponentInChildren<PlayerAnimator>(true));
        float ritmoMin = pa.FindProperty("ritmoMinimo").floatValue;
        float ritmoMax = pa.FindProperty("ritmoMaximo").floatValue;

        SpriteAnimator sa = player.GetComponentInChildren<SpriteAnimator>(true);
        var sao = new SerializedObject(sa);
        SerializedProperty estados = sao.FindProperty("states");
        float fpsRun = 0f;
        for (int i = 0; i < estados.arraySize; i++)
        {
            SerializedProperty e = estados.GetArrayElementAtIndex(i);
            if (e.FindPropertyRelative("state").stringValue == "Run")
            {
                fpsRun = e.FindPropertyRelative("framesPerSecond").floatValue;
            }
        }

        // Contar os quadros pelo nome na folha, e não por um número escrito aqui: se alguém
        // regerar o Run com outra contagem, a conta tem de acompanhar sozinha.
        int quadros = 0;
        foreach (Sprite s in Resources.LoadAll<Sprite>("Odisseia/Characters/CHR_Odysseus"))
        {
            if (s.name.Contains("_Run_")) { quadros++; }
        }

        if (fpsRun <= 0f || quadros == 0)
        {
            Debug.LogError($"[Passada] Run sem FPS ({fpsRun}) ou sem quadros ({quadros})");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"[Passada] Run: {quadros} quadros a {fpsRun} FPS na velocidade base {maxSpeed} un/s; " +
                  $"sprint {sprint}x; ritmo limitado a [{ritmoMin}, {ritmoMax}]");

        float referencia = Passada(maxSpeed, maxSpeed, fpsRun, quadros, ritmoMin, ritmoMax);
        foreach ((string nome, float v) in new[]
                 {
                     ("andando (limiar)", 0.35f * maxSpeed),
                     ("corrida normal", maxSpeed),
                     ("sprint", maxSpeed * sprint),
                 })
        {
            float p = Passada(v, maxSpeed, fpsRun, quadros, ritmoMin, ritmoMax);
            float desvio = Mathf.Abs(p - referencia) / referencia;
            bool ruim = desvio > 0.02f;
            string linha = $"[Passada] {nome,-18} {v,5:0.0} un/s -> passada {p:0.00} un " +
                           $"({desvio * 100:0}% da referência {referencia:0.00})";

            // O limiar baixo cai no piso do ritmo de propósito — sem piso o personagem
            // congelaria num quadro. Ali a passada encurtar é o comportamento escolhido.
            if (ruim && v >= maxSpeed)
            {
                Debug.LogError(linha + "  <- o pé PATINA nesta velocidade");
                falhou = true;
            }
            else
            {
                Debug.Log(linha + (ruim ? "  (dentro do piso do ritmo, esperado)" : ""));
            }
        }

        Debug.Log(falhou ? "[Passada] RESULTADO: FALHOU" : "[Passada] RESULTADO: OK");
        EditorApplication.Exit(falhou ? 1 : 0);
    }

    private static float Passada(float v, float referencia, float fps, int quadros, float min, float max)
    {
        float ritmo = Mathf.Clamp(v / referencia, min, max);
        float ciclo = quadros / (fps * ritmo);
        return v * ciclo / PassadasPorCiclo;
    }
}
