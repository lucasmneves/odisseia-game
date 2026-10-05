using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;
using Odisseia.WorldMap;

/// <summary>
/// Etapa 13B.1 — mapa-múndi geográfico: o arquipélago do <c>map_aegean</c> atrás do caminho, e o caminho redesenhado
/// sobre ele, pelo mar, com cada fase num lugar que faz sentido na viagem:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod WorldMapGeographyDresser.Run
///
/// Antes o mapa era uma faixa de 60 unidades sobre mar chapado. Agora Ítaca é a ilha grande da esquerda (prólogo,
/// retorno, pretendentes e final, quatro paradas na mesma ilha); a viagem vai a leste até Troia (costa do alto à
/// direita), Cícones (a península), Citera, Ciclopes e Éolo (as ilhas da direita); volta a oeste pelos Lestrigões e
/// Circe (as ilhas de baixo) até o Mundo dos Mortos na beira do mundo (o penhasco da esquerda); sobe às Sereias (ilhota),
/// passa Cila e Caríbdis no estreito entre a ilha do centro e a península, para no Gado do Sol (a ilha do centro) e em
/// Calipso (outra ilhota) antes de voltar a Ítaca.
///
/// Só dados de cena: posições do caminho, a arte de fundo, o tamanho da câmera e as velocidades do navio no mapa (o
/// caminho ficou ~3× mais longo). Ordem das fases, desbloqueio, nós, trilha e navegação continuam do
/// <see cref="WorldMapManager"/> e do <see cref="WorldMapPath"/>. Idempotente: refaz tudo a cada rodada.
/// </summary>
public static class WorldMapGeographyDresser
{
    private const string ScenePath = "Assets/Scenes/WorldMap/WorldMap.unity";
    private const string Arte = "Assets/Art/Map/map_aegean.png";
    private const string Raiz = "MapGeography";

    /// <summary>Escala inteira da arte (pixel art): 604×340 px → 42,3 × 23,8 unidades, 0,07 un./px.</summary>
    private const float Escala = 3f;

    /// <summary>Câmera do mapa: mostra ~metade da largura do arquipélago e segue o navio dentro da arte.</summary>
    private const float TamanhoDaCamera = 6f;

    /// <summary>Velocidades do navio no mapa (eram 4,5 e 3,5 para um caminho de 60 un.; agora são ~190).</summary>
    private const float VelocidadeManual = 10f;
    private const float VelocidadeAutomatica = 9f;

    /// <summary>
    /// O traçado, em PIXELS da arte (origem no canto de cima à esquerda). Número = parada de fase, na ordem da campanha;
    /// sem número = ponto de passagem pelo mar. Conferido sobre a arte (rota_preview) antes de gravar.
    /// </summary>
    private static readonly (int fase, int x, int y)[] Rota =
    {
        (1, 138, 212),                                                       // Ítaca, antes da guerra
        (0, 205, 248), (0, 262, 224), (0, 335, 214), (0, 405, 236), (0, 505, 214), (0, 512, 130), (0, 555, 95),
        (2, 568, 66),                                                        // Troia
        (0, 530, 98), (0, 492, 110),
        (3, 448, 126),                                                       // Cícones
        (0, 488, 150),
        (4, 462, 184),                                                       // Citera
        (0, 505, 214),
        (5, 548, 250),                                                       // Ciclopes
        (0, 505, 214), (0, 516, 168),
        (6, 566, 148),                                                       // Éolo
        (0, 516, 168), (0, 505, 214), (0, 488, 252),
        (7, 438, 302),                                                       // Lestrigões
        (0, 400, 254), (0, 345, 226),
        (8, 300, 264),                                                       // Circe
        (0, 252, 226), (0, 205, 248), (0, 118, 262), (0, 70, 282),
        (9, 28, 296),                                                        // Mundo dos Mortos
        (0, 76, 250), (0, 78, 150), (0, 88, 104),
        (10, 118, 86),                                                       // Sereias
        (0, 156, 106), (0, 214, 102), (0, 244, 114), (0, 292, 86), (0, 334, 118),
        (11, 336, 146),                                                      // Cila e Caríbdis
        (0, 318, 152),
        (12, 290, 150),                                                      // Gado do Sol
        (0, 244, 114), (0, 214, 102),
        (13, 185, 77),                                                       // Calipso
        (0, 224, 112), (0, 226, 198), (0, 212, 222),
        (14, 190, 222),                                                      // Ítaca, o retorno
        (15, 158, 178),                                                      // Pretendentes
        (16, 178, 142),                                                      // Final
    };

    public static void Run()
    {
        bool ok = Executar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    private static bool Executar()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Sprite mapa = AssetDatabase.LoadAssetAtPath<Sprite>(Arte);
        var gerente = Object.FindAnyObjectByType<WorldMapManager>();
        var caminho = Object.FindAnyObjectByType<WorldMapPath>();
        var navio = Object.FindAnyObjectByType<WorldMapPlayerController>();
        var camera = Object.FindAnyObjectByType<CameraFollow>();

        if (mapa == null || gerente == null || caminho == null || navio == null || camera == null)
        {
            Debug.LogError($"[MapaGeo] faltou algo: arte={mapa != null} gerente={gerente != null} caminho={caminho != null} " +
                           $"navio={navio != null} câmera={camera != null}");
            return false;
        }

        // Fundo: a arte inteira, centrada na origem, atrás de tudo (trilha 1, brilhos 0, nós e navio acima).
        GameObject antigo = GameObject.Find(Raiz);
        while (antigo != null) { Object.DestroyImmediate(antigo); antigo = GameObject.Find(Raiz); }
        var fundo = new GameObject(Raiz).AddComponent<SpriteRenderer>();
        fundo.sprite = mapa;
        fundo.sortingOrder = -20;
        fundo.transform.position = Vector3.zero;
        fundo.transform.localScale = new Vector3(Escala, Escala, 1f);
        Bounds arte = fundo.bounds;

        // Caminho: pixel → mundo pelos limites do próprio sprite (não depende do pivô).
        var pontos = new SerializedObject(caminho).FindProperty("points");
        pontos.ClearArray();
        int paradas = 0;
        foreach ((int fase, int x, int y) in Rota)
        {
            pontos.InsertArrayElementAtIndex(pontos.arraySize);
            SerializedProperty p = pontos.GetArrayElementAtIndex(pontos.arraySize - 1);
            p.FindPropertyRelative("position").vector2Value = new Vector2(
                arte.min.x + x / (float)mapa.rect.width * arte.size.x,
                arte.max.y - y / (float)mapa.rect.height * arte.size.y);
            p.FindPropertyRelative("isLevelStop").boolValue = fase > 0;
            if (fase > 0)
            {
                paradas++;
                if (fase != paradas) { Debug.LogError($"[MapaGeo] parada {fase} fora de ordem (esperada {paradas})"); return false; }
            }
        }
        pontos.serializedObject.ApplyModifiedPropertiesWithoutUndo();

        Definir(gerente, "geography", fundo);
        Definir(gerente, "autoTravelSpeed", VelocidadeAutomatica);
        Definir(navio, "moveSpeed", VelocidadeManual);
        Definir(camera, "orthographicSize", TamanhoDaCamera);
        // O painel da fase cobre o terço de baixo da tela: a câmera fica 1 un. abaixo do navio (era 1 acima), e o navio e
        // as fases vizinhas caem na faixa livre entre o título e o painel.
        Definir(camera, "offset", new Vector2(0f, -1f));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[MapaGeo] OK — {paradas} paradas, {Rota.Length} pontos, arte {arte.size.x:0.0}×{arte.size.y:0.0} un., " +
                  $"câmera {TamanhoDaCamera}, navio {VelocidadeManual}/{VelocidadeAutomatica}.");
        return paradas == 16;
    }

    private static void Definir(Object alvo, string campo, object valor)
    {
        var so = new SerializedObject(alvo);
        SerializedProperty p = so.FindProperty(campo);
        if (p == null) { Debug.LogError($"[MapaGeo] campo {campo} não existe em {alvo.GetType().Name}"); return; }
        switch (valor)
        {
            case float f: p.floatValue = f; break;
            case Vector2 v: p.vector2Value = v; break;
            case Object o: p.objectReferenceValue = o; break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
