using UnityEditor;
using UnityEngine;
using Odisseia.Enemies;
using Odisseia.Systems;

/// <summary>
/// Veste um chefe (BossController) com a folha da fase e liga o BossAnimator — o equivalente, para
/// chefes, do EnemyFactionOverride. Nasceu no Polifemo (Fase 05) e virou comum no segundo uso
/// (Lestrigões, Fase 07): as regras não podem divergir entre fases.
///
/// Só DESENHO e animação: tempo, dano, alvo e pontos de ataque do BossController, HealthSystem e
/// colisores ficam como estão. Os placeholders eram quadrados (corpo + cabeça + olho) flutuando acima
/// do chão; a arte é uma figura só, com os pés no chão.
/// </summary>
public static class BossArtDresser
{
    public struct Estado
    {
        public string nome; public float fps; public bool loop;
        public Estado(string nome, float fps, bool loop) { this.nome = nome; this.fps = fps; this.loop = loop; }
    }

    /// <param name="folha">Caminho em Resources, sem extensão (ex.: Odisseia/Enemies/CHR_Polyphemus).</param>
    /// <param name="chao">Topo do chão onde os pés apoiam (o mesmo dos pontos de ataque).</param>
    /// <param name="remover">Partes do placeholder que deixam de existir (Head, Eye).</param>
    /// <returns>O SpriteRenderer vestido, ou null se a folha não existir.</returns>
    public static SpriteRenderer Vestir(BossController boss, string folha, float chao, bool olharEsquerda,
        Estado[] estados, params string[] remover)
    {
        string nome = folha.Substring(folha.LastIndexOf('/') + 1);
        Sprite parado = null;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + folha + ".png"))
        {
            if (a is Sprite s && s.name == nome + "_Idle_00") { parado = s; }
        }
        if (parado == null) { Debug.LogError("[BossArt] folha não encontrada: " + folha); return null; }

        foreach (string parte in remover)
        {
            Transform t = boss.transform.Find(parte);
            if (t != null) { Object.DestroyImmediate(t.gameObject); }
        }

        Transform corpo = boss.transform.Find("Body");
        var sr = corpo.GetComponent<SpriteRenderer>();
        sr.sprite = parado;
        sr.color = Color.white;          // o placeholder era tingido
        sr.flipX = olharEsquerda;
        sr.sortingOrder = 1;             // na frente dos props de cenário, atrás do jogador (2)
        corpo.localScale = Vector3.one;  // 42,857 px/un: a arte já tem o tamanho do chefe
        corpo.localRotation = Quaternion.identity;
        // Pivô na base da célula (pés do Idle), centrado no corpo: espelhar não desloca.
        corpo.position = new Vector3(boss.transform.position.x, chao, corpo.position.z);

        // Checagem explícita, não ??: componente ausente da Unity é um "null falso" que ?? não reconhece.
        var animador = corpo.GetComponent<SpriteAnimator>();
        if (animador == null) { animador = corpo.gameObject.AddComponent<SpriteAnimator>(); }
        var so = new SerializedObject(animador);
        so.FindProperty("resourcePath").stringValue = folha;
        so.FindProperty("defaultState").stringValue = "Idle";
        so.FindProperty("defaultFramesPerSecond").floatValue = 5f;
        var lista = so.FindProperty("states");
        lista.arraySize = estados.Length;
        for (int i = 0; i < estados.Length; i++)
        {
            var e = lista.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("state").stringValue = estados[i].nome;
            e.FindPropertyRelative("framesPerSecond").floatValue = estados[i].fps;
            e.FindPropertyRelative("loop").boolValue = estados[i].loop;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        var ligacao = boss.GetComponent<BossAnimator>();
        if (ligacao == null) { ligacao = boss.gameObject.AddComponent<BossAnimator>(); }
        var sl = new SerializedObject(ligacao);
        sl.FindProperty("boss").objectReferenceValue = boss;
        sl.FindProperty("animator").objectReferenceValue = animador;
        sl.ApplyModifiedPropertiesWithoutUndo();
        return sr;
    }
}
