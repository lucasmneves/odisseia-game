using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Primeiro plano das fases 04, 06, 14 e 15 — as quatro que não tinham (PXL-021, Asset Completion):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod PackForegroundDresser.Run
///
/// Raiz própria (<see cref="RaizDoPrimeiroPlano"/>) em cada cena, destruída e recriada a cada rodada: idempotente, e
/// não depende da ordem dos vestidores de cena. Mesma regra das outras onze fases: ordem 12 (acima do jogador, 0, e dos
/// inimigos, 2), base enterrada abaixo da linha do chão, poucas peças ("primeiro plano é pontuação, não moldura"), e
/// nenhuma sobre ponto de PARADA — spawn, NPC, checkpoint, objetivo (D-048). Sem colisor, sem script.
///
/// Cordame (04), galho (14) e estandarte (15) PENDEM de um ponto no alto, e a câmera destas fases sobe até ~2,4 un com o
/// pulo duplo: como primeiro plano, nenhuma posição esconderia a amarração sem esconder a peça. Entram como DECORAÇÃO
/// presa a uma estrutura da cena (raiz <see cref="RaizDaDecoracao"/>), logo à frente dela e atrás do jogador.
/// </summary>
public static class PackForegroundDresser
{
    private const string RaizDoPrimeiroPlano = "PackForeground";
    private const string RaizDaDecoracao = "PackDecor";
    private const string Arte = "Assets/Art/Environments/";
    private const float TopoDoChao = -2f;
    private const int Ordem = 12;

    private struct Peca
    {
        public string nome, arte;
        public float x, y;
        public bool espelhar;
        /// <summary>Ordem própria = decoração presa a uma estrutura (raiz PackDecor); 0 = primeiro plano.</summary>
        public int ordem;
        public Peca(string nome, string arte, float x, float y, bool espelhar = false, int ordem = 0)
        {
            this.nome = nome; this.arte = arte; this.x = x; this.y = y; this.espelhar = espelhar; this.ordem = ordem;
        }
    }

    private static readonly (string cena, Peca[] pecas)[] Fases =
    {
        // 04 — popa: à esquerda do nascimento (x=−15), na borda do convés que a câmera mostra no início.
        ("Level_04_Citera", new[]
        {
            new Peca("FG_RopeBarrel", "Cytera/Foreground/cytera_fg_rope_barrel.png", -18.3f, TopoDoChao - 0.3f),
            // Pendurado da ponta esquerda da verga do mastro da proa (Art_Mast_Proa, x 35,55..39,45, ordem −7): a corda
            // está a ~0,15 un à esquerda do centro do sprite, o topo dela na altura da verga (~3,1).
            new Peca("Decor_Rigging_Bow", "Cytera/Foreground/cytera_fg_rigging.png", 35.75f, 3.1f - 2.05f, false, -6),
        }),
        // 06 — balaustrada partida na ponta do primeiro trecho, à esquerda do nascimento (x=−14); a nuvem passa à
        // frente do abismo entre a plataforma móvel e Floor_3a, ABAIXO da linha do chão: não cobre onde se pisa.
        ("Level_06_Eolo", new[]
        {
            new Peca("FG_Balustrade", "Eolo/Foreground/eolo_fg_balustrade.png", -17.2f, TopoDoChao - 0.3f),
            new Peca("FG_Cloud", "Eolo/Foreground/eolo_fg_cloud.png", 16.2f, TopoDoChao - 1.45f),
        }),
        // 14 — mato à esquerda do nascimento (x=−14) e, espelhado, entre Telêmaco (27..29) e o objetivo (37,4).
        ("Level_14_Itaca_Return", new[]
        {
            new Peca("FG_Weeds_Start", "ItacaReturn/Foreground/itaca_ret_fg_weeds.png", -17f, TopoDoChao - 0.45f),
            new Peca("FG_Weeds_Field", "ItacaReturn/Foreground/itaca_ret_fg_weeds.png", 33f, TopoDoChao - 0.45f, true),
            // Do beiral da casa tomada pela hera (House_Overgrown, x 3,6..8,4, ordem −18), no canto direito.
            new Peca("Decor_Vine_Overgrown", "ItacaReturn/Foreground/itaca_ret_fg_branch.png", 7.9f, 1.0f - 1.56f, false, -17),
        }),
        // 15 — ânfora derramada à esquerda do nascimento (x=−12) e na entrada do salão, entre Telêmaco (15..17) e o
        // primeiro pretendente (24).
        ("Level_15_Pretendentes", new[]
        {
            new Peca("FG_Amphora_Court", "Pretendentes/Foreground/pret_fg_spilled_amphora.png", -14.8f, TopoDoChao - 0.3f),
            new Peca("FG_Amphora_Hall", "Pretendentes/Foreground/pret_fg_spilled_amphora.png", 19.6f, TopoDoChao - 0.3f, true),
            // Sobre a porta do salão, acima da coluna da entrada (Hall_Door_Column_0,9, topo 0,82), na ordem dos
            // estandartes do salão (−29). As paredes já têm estandartes a cada 8 un; um a mais no meio quebraria o ritmo.
            new Peca("Decor_Banner_Door", "Pretendentes/Foreground/pret_fg_banner_edge.png", 20.9f, 1.3f, false, -29),
        }),
    };

    [MenuItem("Odisseia/Primeiro plano 04-06-14-15")]
    public static void Vestir() => Executar();

    public static void Run()
    {
        bool ok = Executar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    private static bool Executar()
    {
        bool ok = true;
        foreach (var (cena, pecas) in Fases)
        {
            var scene = EditorSceneManager.OpenScene($"Assets/Scenes/Levels/{cena}.unity", OpenSceneMode.Single);

            foreach (string nome in new[] { RaizDoPrimeiroPlano, RaizDaDecoracao })
            {
                GameObject antigo = GameObject.Find(nome);
                while (antigo != null) { Object.DestroyImmediate(antigo); antigo = GameObject.Find(nome); }
            }
            Transform raiz = null, decoracao = null;

            int n = 0;
            foreach (Peca p in pecas)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Arte + p.arte);
                if (sprite == null) { Debug.LogError($"[FG] {cena}: sem {p.arte}"); ok = false; continue; }

                var go = new GameObject(p.nome);
                bool decor = p.ordem != 0;
                if (decor && decoracao == null) { decoracao = new GameObject(RaizDaDecoracao).transform; }
                if (!decor && raiz == null) { raiz = new GameObject(RaizDoPrimeiroPlano).transform; }
                go.transform.SetParent(decor ? decoracao : raiz, false);
                // O pivô do pack é BottomCenter: a base cai em y, abaixo da linha do chão.
                go.transform.position = new Vector3(p.x, p.y, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = decor ? p.ordem : Ordem;
                sr.flipX = p.espelhar;
                n++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[FG] {cena}: {n} peças (primeiro plano e decoração)");
        }

        return ok;
    }
}
