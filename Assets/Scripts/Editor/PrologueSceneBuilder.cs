using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Levels;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

/// <summary>
/// Monta a Fase 1 (Ítaca, o prólogo) inteira: exploração, convocação, família,
/// recrutamento, treinamento, preparação e partida.
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod PrologueSceneBuilder.Build
/// ou pelo menu Odisseia &gt; Montar prologo de Itaca.
///
/// É script de Editor, e não YAML escrito à mão, pelo mesmo motivo do
/// <c>LevelSelectSceneBuilder</c>: a fase tem quase trezentos objetos e dezenas de
/// referências cruzadas (objetivo aponta para portão, portão para NPC, NPC para
/// diálogo), e um campo esquecido no YAML não dá erro — dá um roteiro que trava no
/// meio. Aqui a montagem é código revisável e repetível.
///
/// Idempotente: roda quantas vezes quiser. O que é da fase (chão, NPCs, atos) é
/// refeito do zero; o que é de infraestrutura (HUD, pause, morte, câmera, Player,
/// LevelGoal) é preservado e reaproveitado.
/// </summary>
public static class PrologueSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_01_Itaca_Prologue.unity";
    private const string SquarePath = "Assets/Art/Player/PlaceholderSquare.png";
    private const string ControlsPath = "Assets/ScriptableObjects/PlayerControls.inputactions";

    // Elenco em pixel art (Docs/Characters/CHARACTER_ART_MASTER.md). Cada nome é uma folha em
    // Resources/Odisseia/Characters/NPCs/CHR_<nome>.png com o estado Idle; o Idle_00 é também o
    // sprite parado. Substituíram a arte pintada a 129 px/un (CHR_NPC_*), que borrava no
    // tamanho de jogo e deixava a Penélope mais alta que o Odisseu. A arte pintada continua no
    // projeto porque as fases 14 e 15 ainda a usam.
    private const string ElencoPasta = "Assets/Resources/Odisseia/Characters/NPCs/";
    private const string ElencoResources = "Odisseia/Characters/NPCs/";
    private const string ChrPenelope = "Penelope";
    private const string ChrTelemaco = "Telemachus_Child";
    private const string ChrArauto = "Herald_Mycenae";
    private const string ChrSoldado = "Soldier_Ithaca";
    private const string ChrInstrutor = "Trainer_Veteran";
    private const string ChrMulher = "Villager_Woman";
    private const string ChrMercador = "Villager_Merchant";
    private const string ChrPescador = "Villager_Fisherman";
    private const string ChrLavrador = "Villager_Farmer";
    private const string ChrMarinheiro = "Villager_Sailor";
    private const string ChrMarinheiroOcre = "Villager_Sailor_Ochre";
    private const string ChrMarinheiroOliva = "Villager_Sailor_Olive";
    private const string ChrArtesao = "Villager_Elder_Brown";

    private const string BgFar = "Assets/Art/Odisseia/Backgrounds/BG_Fase01_Greece_Far.png";
    private const string BgMid = "Assets/Art/Odisseia/Backgrounds/BG_Fase01_Greece_Mid.png";
    private const string BgNear = "Assets/Art/Odisseia/Backgrounds/BG_Fase01_Greece_Near.png";

    // ---------------------------------------------------- arte de cenário de Ítaca
    //
    // Pipeline em Docs/Environment_Ithaca; `node Tools/unity-import.js` traz para cá.
    // Quando estes arquivos não existem a fase volta sozinha para as silhuetas chapadas,
    // então o builder continua rodando num clone sem a arte.

    private const string ItacaRaiz = "Assets/Art/Environments/Ithaca/";
    private const string ItacaCeu = ItacaRaiz + "Background/ithaca_bg_sky.png";
    private const string ItacaNuvens = ItacaRaiz + "Background/ithaca_bg_clouds.png";
    private const string ItacaIlha = ItacaRaiz + "Background/ithaca_bg_island_far.png";
    private const string ItacaOceano = ItacaRaiz + "Background/ithaca_bg_ocean.png";
    private const string ItacaMontanhas = ItacaRaiz + "Background/ithaca_bg_mountains_far.png";

    private const string ItacaTerrenoGrama = ItacaRaiz + "Terrain/ithaca_tiles_grass.png";
    private const string ItacaTerrenoTerra = ItacaRaiz + "Terrain/ithaca_tiles_earth.png";
    private const string ItacaTerrenoPedra = ItacaRaiz + "Terrain/ithaca_tiles_stone.png";
    private const string ItacaTerrenoMadeira = ItacaRaiz + "Terrain/ithaca_tiles_wood.png";

    /// <summary>
    /// Lado do tile de terreno em unidades: 32 px a 42,857 px por unidade.
    /// </summary>
    private const float LadoDoTile = 32f / 42.857143f;

    /// <summary>
    /// Índice do tile de topo plano na folha Wang (NW e NE de ar, SW e SE de sólido).
    ///
    /// O desenho sólido dele começa na METADE do tile, não no topo — medido nos pixels, e é
    /// por isso que o tile de topo é posicionado com o CENTRO na linha do chão: assim a
    /// superfície desenhada coincide com a superfície do colisor.
    /// </summary>
    private const int TileTopo = 3;

    /// <summary>Índice do tile totalmente sólido, usado para preencher abaixo do topo.</summary>
    private const int TileCheio = 6;

    private const string ItacaCasa = ItacaRaiz + "Architecture/ithaca_house_small_01.png";
    private const string ItacaPalacio = ItacaRaiz + "Architecture/ithaca_house_odysseus_01.png";
    private const string ItacaColuna = ItacaRaiz + "Architecture/ithaca_column_01.png";
    private const string ItacaPalacioMegaron = ItacaRaiz + "Architecture/ithaca_palace_01.png";
    private const string ItacaArmazem = ItacaRaiz + "Architecture/ithaca_warehouse_01.png";
    private const string ItacaPortao = ItacaRaiz + "Architecture/ithaca_gate_01.png";
    private const string ItacaBatente = ItacaRaiz + "Architecture/ithaca_gatepost_01.png";
    private const string ItacaTorre = ItacaRaiz + "Architecture/ithaca_watchtower_01.png";
    private const string ItacaBigorna = ItacaRaiz + "Props/ithaca_anvil_01.png";
    private const string ItacaForja = ItacaRaiz + "Props/ithaca_forge_01.png";
    private const string ItacaLavoura = ItacaRaiz + "Nature/ithaca_field_01.png";
    private const string ItacaCascoNaPraia = ItacaRaiz + "Port/ithaca_boat_hull_01.png";

    private const string ItacaBoneco = ItacaRaiz + "Training/ithaca_training_dummy_01.png";
    private const string ItacaAlvo = ItacaRaiz + "Training/ithaca_training_target_01.png";
    private const string ItacaTear = ItacaRaiz + "Props/ithaca_loom_01.png";
    private const string ItacaMuro = ItacaRaiz + "Architecture/ithaca_wall_low_01.png";
    private const string ItacaNavio = ItacaRaiz + "Ships/ithaca_ship_01.png";
    private const string ItacaNavioAtracado = ItacaRaiz + "Ships/ithaca_ship_01_furled.png";
    private const string ItacaRede = ItacaRaiz + "Port/ithaca_fishing_net_01.png";
    private const string ItacaPosteDoCais = ItacaRaiz + "Port/ithaca_dock_post_01.png";
    private const string ItacaSuporteDeArmas = ItacaRaiz + "Training/ithaca_weapon_rack_01.png";

    /// <summary>
    /// Vegetação e miudezas espalhadas pela fase. Índice fixo porque o espalhamento é
    /// sorteado com semente — a fase precisa sair igual toda vez que for remontada.
    /// </summary>
    private static readonly string[] ItacaVegetacao =
    {
        ItacaRaiz + "Nature/ithaca_tree_olive_01.png",
        ItacaRaiz + "Nature/ithaca_tree_cypress_01.png",
        ItacaRaiz + "Nature/ithaca_bush_01.png",
        ItacaRaiz + "Nature/ithaca_rock_medium_01.png",
        ItacaRaiz + "Nature/ithaca_rock_small_01.png",
        ItacaRaiz + "Nature/ithaca_tall_grass_01.png",
        ItacaRaiz + "Nature/ithaca_grass_tuft_01.png",
        ItacaRaiz + "Nature/ithaca_flowers_01.png",
    };

    /// <summary>
    /// Fundo pintado (Art/Odisseia/Backgrounds) em vez das silhuetas chapadas.
    ///
    /// Desligado de propósito. As telas pintadas são bonitas e são arte FINAL; o resto
    /// desta fase ainda é retângulo colorido, e as duas coisas juntas na mesma tela
    /// fazem o cenário parecer quebrado — a paisagem em detalhe atrás e um caixote bege
    /// liso na frente. Enquanto a fase for placeholder, o fundo também é.
    ///
    /// Ligar aqui devolve as três camadas pintadas, já posicionadas e sem emenda.
    /// </summary>
    private const bool UsarFundoPintado = false;

    private const int LayerGround = 8;
    private const int LayerEnemy = 10;

    /// <summary>Altura do topo do chão. Tudo que fica "em pé" nasce sobre esta linha.</summary>
    private const float GroundTop = -2f;

    /// <summary>Ponta direita do nível, usada para dimensionar céu, fundo e poço.</summary>
    private const float LevelEnd = 430f;

    /// <summary>Largura visível com a câmera ortográfica de tamanho 5, em 16:9.</summary>
    private const float LarguraDaTela = 18f;

    // ------------------------------------------------------------------ paleta

    private static readonly Color CorChao = new Color(0.55f, 0.45f, 0.32f);
    private static readonly Color CorPedra = new Color(0.72f, 0.68f, 0.6f);
    private static readonly Color CorCasa = new Color(0.85f, 0.78f, 0.62f);
    private static readonly Color CorTelhado = new Color(0.7f, 0.35f, 0.28f);
    private static readonly Color CorPalacio = new Color(0.93f, 0.9f, 0.8f);
    private static readonly Color CorCeu = new Color(0.55f, 0.78f, 0.93f);
    private static readonly Color CorMar = new Color(0.24f, 0.46f, 0.68f);
    private static readonly Color CorTerra = new Color(0.36f, 0.28f, 0.2f);
    private static readonly Color CorMarDistante = new Color(0.42f, 0.62f, 0.78f);
    private static readonly Color CorMontanha = new Color(0.44f, 0.52f, 0.62f);
    private static readonly Color CorColina = new Color(0.46f, 0.58f, 0.44f);
    private static readonly Color CorJanela = new Color(0.35f, 0.42f, 0.5f);
    private static readonly Color CorMadeira = new Color(0.5f, 0.34f, 0.2f);
    private static readonly Color CorPortao = new Color(0.42f, 0.3f, 0.2f);
    private static readonly Color CorAlvo = new Color(0.86f, 0.5f, 0.35f);
    private static readonly Color CorBoneco = new Color(0.76f, 0.66f, 0.45f);
    private static readonly Color CorNpc = new Color(0.62f, 0.72f, 0.85f);
    private static readonly Color CorSoldado = new Color(0.72f, 0.66f, 0.5f);
    private static readonly Color CorProp = new Color(0.6f, 0.5f, 0.35f);
    private static readonly Color CorVela = new Color(0.94f, 0.93f, 0.88f);

    /// <summary>Passo Profunda da rampa de Céu (#6481a0) — o topo do degradê da arte de Ítaca.</summary>
    private static readonly Color CorCeuProfundo = new Color(100f / 255f, 129f / 255f, 160f / 255f);

    /// <summary>Passo Profunda da rampa de Água (#406b84).</summary>
    private static readonly Color CorAguaProfunda = new Color(64f / 255f, 107f / 255f, 132f / 255f);

    private static Sprite square;
    private static InputActionAsset controls;

    // referências reaproveitadas da cena existente
    private static GameObject player;
    private static PlayerInputLock playerLock;
    private static PlayerCombat playerCombat;
    private static PlayerShield playerShield;
    private static PlayerBow playerBow;
    private static TutorialPrompt prompt;
    private static ObjectiveBanner banner;
    private static LevelGoal goal;
    private static Transform hudCanvas;
    private static GameObject dialoguePanel;
    private static Text dialogueSpeaker;
    private static Text dialogueBody;
    private static CameraFollow cameraFollow;

    private static Transform world;
    private static Transform actsRoot;
    private static Transform dialoguesRoot;

    [MenuItem("Odisseia/Montar prologo de Itaca")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        square = Arte(SquarePath);
        controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);

        if (square == null || controls == null)
        {
            Fail("sprite placeholder ou asset de controles não encontrado");
            return;
        }

        if (!LocalizarInfraestrutura())
        {
            return;
        }

        LimparMundoAntigo(scene);

        world = NovaRaiz("World");
        actsRoot = NovaRaiz("Acts");
        dialoguesRoot = NovaRaiz("Dialogues");

        MontarFundo();
        MontarChao();

        // Os diálogos primeiro: NPCs, pontos e objetivos referenciam as sequências.
        Dictionary<string, DialogueSequence> falas = MontarDialogos();

        MontarCena(falas);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[Prologue] fase montada: 6 atos, tutorial de combate, porto e partida.");
        Encerrar(0);
    }

    /// <summary>
    /// Encerra o processo só quando ele é o batchmode. Chamado pelo menu, dentro do
    /// Editor aberto, EditorApplication.Exit fecharia o Unity do desenvolvedor na cara
    /// dele — o script é o mesmo, o contexto é que muda.
    /// </summary>
    private static void Encerrar(int codigo)
    {
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(codigo);
        }
    }

    private static void Fail(string mensagem)
    {
        Debug.LogError("[Prologue] " + mensagem);
        Encerrar(1);
    }

    // ------------------------------------------------------------------ infraestrutura

    /// <summary>
    /// Reencontra o que a cena já tinha e que não faz sentido recriar: o Player (que é
    /// prefab), a HUD, o objetivo de fase e a câmera.
    /// </summary>
    private static bool LocalizarInfraestrutura()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Fail("Player não encontrado na cena");
            return false;
        }

        player.transform.position = new Vector3(2f, GroundTop + 0.8f, 0f);
        NormalizarJogador();

        playerLock = player.GetComponent<PlayerInputLock>();
        playerCombat = player.GetComponent<PlayerCombat>();
        playerShield = player.GetComponent<PlayerShield>();
        playerBow = player.GetComponent<PlayerBow>();

        prompt = Object.FindAnyObjectByType<TutorialPrompt>();
        goal = Object.FindAnyObjectByType<LevelGoal>();
        cameraFollow = Object.FindAnyObjectByType<CameraFollow>();

        var canvas = Object.FindAnyObjectByType<HUD>();
        if (prompt == null || goal == null || canvas == null || cameraFollow == null)
        {
            Fail("HUD, TutorialPrompt, LevelGoal ou CameraFollow ausentes na cena");
            return false;
        }

        hudCanvas = canvas.transform;

        // O objetivo da fase fica fora do caminho: quem encerra o prólogo é a cena de
        // partida, chamando LevelGoal.Trigger().
        goal.transform.position = new Vector3(LevelEnd - 10f, GroundTop + 1f, 0f);

        LocalizarFaixaDeContexto();
        banner = GarantirBanner();
        GarantirPainelDeDialogo();
        return true;
    }

    /// <summary>
    /// Garante que Odisseu comece o prólogo como Odisseu.
    ///
    /// A instância do Player desta cena vinha com o disfarce de mendigo ligado
    /// (<c>startDisguised</c>, layer 12) — mecânica da Fase 14, quase certamente
    /// herdada de uma cópia daquela cena, já que as duas se passam em Ítaca.
    ///
    /// O efeito é silencioso e total: o disfarce existe justamente para tirar o
    /// jogador da layer que os inimigos procuram. Com ele ligado, NENHUM inimigo do
    /// prólogo enxerga Odisseu — eles perseguem alguém que, para a física, não está
    /// lá. Nada disso aparece como erro; o combate simplesmente não acontece, e a
    /// etapa do escudo, que só fecha ao bloquear um golpe, fica impossível.
    /// </summary>
    private static void NormalizarJogador()
    {
        player.layer = 9;

        var disfarce = player.GetComponent<DisguiseEffect>();
        if (disfarce != null)
        {
            Wire(disfarce, "startDisguised", false);
        }
    }

    /// <summary>
    /// A faixa "Ítaca — antes da guerra" estava com o texto cravado na cena, em
    /// português, num jogo que abre em inglês. A chave já existia na tabela; faltava
    /// ligar o rótulo nela.
    /// </summary>
    private static void LocalizarFaixaDeContexto()
    {
        Transform faixa = hudCanvas.Find("PersistentBanner");
        if (faixa == null)
        {
            return;
        }

        var localizado = faixa.GetComponent<LocalizedText>();
        if (localizado == null)
        {
            localizado = faixa.gameObject.AddComponent<LocalizedText>();
        }

        Wire(localizado, "key", "ui.hud.itacaBefore");
    }

    /// <summary>Faixa de objetivo no alto da HUD, abaixo do nome da fase.</summary>
    private static ObjectiveBanner GarantirBanner()
    {
        var existente = Object.FindAnyObjectByType<ObjectiveBanner>();
        if (existente != null)
        {
            return existente;
        }

        var panel = new GameObject("ObjectivePanel", typeof(RectTransform));
        panel.transform.SetParent(hudCanvas, false);
        var rect = (RectTransform)panel.transform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(560f, 30f);
        rect.anchoredPosition = new Vector2(0f, -54f);

        var texto = panel.AddComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = 18;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = new Color(0.95f, 0.95f, 0.9f);
        texto.horizontalOverflow = HorizontalWrapMode.Overflow;
        texto.raycastTarget = false;

        var holder = new GameObject("ObjectiveBannerController");
        holder.transform.SetParent(hudCanvas, false);
        var comp = holder.AddComponent<ObjectiveBanner>();
        Wire(comp, "panel", panel);
        Wire(comp, "label", texto);
        panel.SetActive(false);
        return comp;
    }

    /// <summary>
    /// Um painel de diálogo só, compartilhado por todas as falas. Antes havia um painel
    /// por sequência; com 26 conversas isso seriam 26 painéis idênticos na hierarquia,
    /// e só um aparece por vez de qualquer forma.
    /// </summary>
    private static void GarantirPainelDeDialogo()
    {
        Transform painel = hudCanvas.Find("DialoguePanel");

        if (painel == null)
        {
            painel = hudCanvas.Find("IntroDialogueController_Panel");
        }

        if (painel == null)
        {
            Fail("painel de diálogo não encontrado na HUD");
            return;
        }

        painel.name = "DialoguePanel";
        dialoguePanel = painel.gameObject;
        dialogueSpeaker = painel.Find("SpeakerText").GetComponent<Text>();
        dialogueBody = painel.Find("BodyText").GetComponent<Text>();
        dialoguePanel.SetActive(false);

        // Os outros painéis viram redundantes assim que todo mundo aponta para este.
        foreach (Transform filho in hudCanvas)
        {
            if (filho.name.EndsWith("DialogueController_Panel"))
            {
                Object.DestroyImmediate(filho.gameObject);
            }
        }
    }

    /// <summary>
    /// Apaga o que é conteúdo de fase. Fica de pé só a infraestrutura, para a montagem
    /// não depender do que sobrou da execução anterior.
    /// </summary>
    private static void LimparMundoAntigo(Scene scene)
    {
        var protegidos = new HashSet<string>
        {
            "Main Camera", "HUD Canvas", "LevelManager", "LevelIntro", "SceneAudio",
            "Player", "LevelGoal",
        };

        foreach (GameObject raiz in scene.GetRootGameObjects())
        {
            if (protegidos.Contains(raiz.name))
            {
                continue;
            }

            Object.DestroyImmediate(raiz);
        }

        // As sequências de diálogo antigas moram dentro da HUD e são refeitas com o
        // roteiro novo.
        var mortas = new List<GameObject>();
        foreach (Transform filho in hudCanvas)
        {
            if (filho.GetComponent<DialogueSequence>() != null)
            {
                mortas.Add(filho.gameObject);
            }
        }

        foreach (GameObject morta in mortas)
        {
            Object.DestroyImmediate(morta);
        }
    }

    private static Transform NovaRaiz(string nome)
    {
        var go = new GameObject(nome);
        return go.transform;
    }

    // ------------------------------------------------------------------ blocos

    private static GameObject Bloco(string nome, Transform pai, float x, float y, float largura,
        float altura, Color cor, int ordem = 0, bool solido = false)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(largura, altura, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.color = cor;
        sr.sortingOrder = ordem;

        if (solido)
        {
            go.AddComponent<BoxCollider2D>();
            go.layer = LayerGround;
        }

        return go;
    }

    /// <summary>Chão com o topo em <paramref name="topo"/>, medido da esquerda para a direita.</summary>
    private static GameObject Chao(string nome, float esquerda, float direita, float topo = GroundTop)
    {
        float largura = direita - esquerda;
        return Vestido(Bloco(nome, world, esquerda + largura * 0.5f, topo - 0.5f, largura, 1f, CorChao, 0, true));
    }

    private static GameObject Plataforma(string nome, float centroX, float topo, float largura)
    {
        return Vestido(Bloco(nome, world, centroX, topo - 0.25f, largura, 0.5f, CorPedra, 0, true));
    }

    /// <summary>
    /// Apaga o desenho do bloco quando existe arte de terreno para cobri-lo, preservando o
    /// colisor. É isso que deixa a colisão intacta enquanto a aparência muda: o retângulo
    /// colorido continua sendo o chão para a física, só parou de aparecer.
    /// </summary>
    private static GameObject Vestido(GameObject bloco)
    {
        if (Tile(ItacaTerrenoGrama, TileTopo) == null)
        {
            return bloco;
        }

        var sr = bloco.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }

        return bloco;
    }

    /// <summary>
    /// Um sprite do tileset Wang pelo índice documentado (row-major a partir do topo da
    /// folha). O nome fatiado é <c>&lt;kit&gt;_NN</c>, escrito assim por
    /// <c>Tools/unity-import.js</c> justamente para este acesso ser por índice.
    /// </summary>
    private static Sprite Tile(string caminho, int indice)
    {
        string kit = caminho.Substring(caminho.LastIndexOf('/') + 1).Replace(".png", string.Empty);
        string alvo = kit + "_" + indice.ToString("00");

        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(caminho))
        {
            if (asset is Sprite sprite && sprite.name == alvo)
            {
                return sprite;
            }
        }

        return null;
    }

    /// <summary>
    /// Veste um trecho de chão com o tileset: uma fiada de topo e o preenchimento abaixo.
    ///
    /// O tile de topo é posicionado com o CENTRO na linha do chão porque o desenho sólido
    /// dele começa na metade do tile — os cantos de cima do Wang são "upper" (ar). Alinhar
    /// pelo topo do tile deixaria a superfície desenhada meio tile abaixo da superfície do
    /// colisor, e o personagem pareceria afundado no chão.
    ///
    /// Dois renderers por trecho, em drawMode Tiled, em vez de um objeto por tile: a fase
    /// tem 349 unidades de chão, o que daria mais de dois mil GameObjects.
    /// </summary>
    private static void Terreno(string nome, float esquerda, float direita, string kit,
        float topo = GroundTop, int fiadasDeBaixo = 7, string kitDeBaixo = null)
    {
        Sprite tileTopo = Tile(kit, TileTopo);
        // O preenchimento pode vir de outro kit: o tile cheio de pedra saiu em fiadas
        // regulares de tijolo (desvio já registrado do Conceito A) e, empilhado cinco vezes,
        // vira um paredão amarelo. Pedra no calçamento e terra por baixo lê como cais.
        Sprite tileCheio = Tile(kitDeBaixo ?? kit, TileCheio);
        if (tileTopo == null || tileCheio == null)
        {
            return;
        }

        float largura = direita - esquerda;
        float centro = esquerda + largura * 0.5f;

        FaixaDeTiles(nome + "_Top", centro, topo, largura, LadoDoTile, tileTopo, -1);

        // O preenchimento começa onde o tile de topo termina e desce até passar do que a
        // câmera enxerga, senão o mundo acaba numa linha e o fundo aparece por baixo.
        float altura = fiadasDeBaixo * LadoDoTile;
        FaixaDeTiles(nome + "_Fill", centro, topo - LadoDoTile * 0.5f - altura * 0.5f,
            largura, altura, tileCheio, -2);
    }

    /// <summary>
    /// Um sprite de cenário apoiado na linha do chão. Devolve null quando a arte não existe,
    /// e aí quem chamou desenha o placeholder de sempre.
    ///
    /// Nunca escala: a arte foi feita em 42,857 px por unidade, e esticar um sprite de pixel
    /// art quebra a densidade que todo o resto do cenário respeita.
    /// </summary>
    private static GameObject Prop(string nome, Transform pai, float x, string caminho,
        int ordem = -1, float topo = GroundTop, bool espelhado = false)
    {
        Sprite arte = Arte(caminho);
        if (arte == null)
        {
            return null;
        }

        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        // O pivô é BottomCenter, então a base do desenho cai exatamente na linha do chão.
        go.transform.position = new Vector3(x, topo, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.flipX = espelhado;
        return go;
    }

    /// <summary>
    /// Muro baixo de pedra seca ladrilhado num trecho. O sprite foi feito para ladrilhar
    /// (emenda medida em 0,0 contra mediana interna de 8,8), então vai num renderer só em
    /// drawMode Tiled. Devolve false quando a arte não existe.
    /// </summary>
    private static bool MuroBaixo(string nome, Transform pai, float esquerda, float direita)
    {
        Sprite arte = Arte(ItacaMuro);
        if (arte == null)
        {
            return false;
        }

        float largura = direita - esquerda;
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.position = new Vector3(esquerda + largura * 0.5f, GroundTop, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = -1;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(largura, arte.bounds.size.y);
        return true;
    }

    /// <summary>
    /// Espalha vegetação e pedras num trecho, com semente fixa para a fase sair igual a cada
    /// remontagem. Fica atrás dos personagens e à frente do terreno.
    /// </summary>
    private static void Vegetacao(Transform pai, float esquerda, float direita, int quantidade, int semente)
    {
        if (Arte(ItacaVegetacao[0]) == null)
        {
            return;
        }

        var zona = new GameObject("Vegetation").transform;
        zona.SetParent(pai, false);

        UnityEngine.Random.InitState(semente);
        for (int i = 0; i < quantidade; i++)
        {
            float x = UnityEngine.Random.Range(esquerda, direita);
            string arte = ItacaVegetacao[UnityEngine.Random.Range(0, ItacaVegetacao.Length)];
            // Espelhar metade quebra a repetição sem custar asset novo.
            Prop("Flora_" + i, zona, x, arte, -1, GroundTop, UnityEngine.Random.value < 0.5f);
        }
    }

    private static void FaixaDeTiles(string nome, float x, float y, float largura, float altura,
        Sprite tile, int ordem)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(world, false);
        go.transform.position = new Vector3(x, y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = tile;
        sr.sortingOrder = ordem;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(largura, altura);
    }

    /// <summary>
    /// Figura em pé sobre o chão (NPC, boneco, prop), apoiada pela BASE do desenho.
    ///
    /// A posição sai do <c>bounds</c> do sprite em vez de "meia altura acima do chão",
    /// porque meia altura só vale para pivô central. A arte de personagem deste projeto
    /// é importada com pivô BottomCenter (alignment 7) e o quadrado placeholder com
    /// pivô central (alignment 0) — a conta fixa acertava o quadrado e deixava todo
    /// personagem flutuando meia altura no ar.
    ///
    /// Com <c>bounds.min.y</c> a mesma linha serve para qualquer pivô, inclusive o que
    /// alguém escolher para a arte final.
    /// </summary>
    private static GameObject Figura(string nome, Transform pai, float x, float largura, float altura,
        Color cor, int ordem = 1, Sprite arte = null)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);

        var corpo = new GameObject("Body");
        corpo.transform.SetParent(go.transform, false);

        var sr = corpo.AddComponent<SpriteRenderer>();
        sr.sprite = arte != null ? arte : square;
        sr.color = arte != null ? Color.white : cor;
        sr.sortingOrder = ordem;

        // A arte dos NPCs tem proporção própria; esticá-la pela escala do bloco
        // deformaria o personagem. O quadrado, sim, é esticado de propósito.
        Vector3 escala = arte != null
            ? Vector3.one * (arte.bounds.size.y > 0f ? altura / arte.bounds.size.y : 1f)
            : new Vector3(largura, altura, 1f);

        corpo.transform.localScale = escala;

        float baseDoDesenho = sr.sprite.bounds.min.y * escala.y;
        go.transform.position = new Vector3(x, GroundTop - baseDoDesenho, 0f);

        return go;
    }

    /// <summary>
    /// Centro da figura em coordenadas locais da raiz. As áreas de gatilho usam isto:
    /// como a raiz agora fica onde o pivô do desenho manda, "y = 0" não é mais o meio
    /// do personagem.
    /// </summary>
    private static Vector2 CentroDaFigura(GameObject figura, float altura)
    {
        return new Vector2(0f, GroundTop + altura * 0.5f - figura.transform.position.y);
    }

    /// <summary>
    /// Carrega o sprite de um .png. Vai atras dos sub-assets porque
    /// LoadAssetAtPath&lt;Sprite&gt; devolve null quando o asset principal do arquivo e a
    /// Texture2D, e nao o sprite fatiado dela.
    /// </summary>
    private static Sprite Arte(string caminho)
    {
        var direto = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (direto != null)
        {
            return direto;
        }

        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(caminho))
        {
            if (asset is Sprite sprite)
            {
                return sprite;
            }
        }

        Debug.LogWarning("[Prologue] sprite nao encontrado: " + caminho);
        return null;
    }

    // ------------------------------------------------------------------ fundo e chão

    private static void MontarFundo()
    {
        Transform fundo = new GameObject("Background").transform;
        fundo.SetParent(world, false);

        // Céu inteiro num bloco só: nenhuma camada de parallax pode deixar buraco.
        // A cor sai da própria arte quando ela existe, para o topo do degradê não encontrar
        // uma faixa de cor diferente onde o sprite do céu acaba.
        Bloco("Sky", fundo, LevelEnd * 0.5f, 6f, LevelEnd + 60f, 30f, CorDoTopoDoCeu(), -50);

        if (MontarFundoDeItaca(fundo))
        {
            return;
        }

        if (UsarFundoPintado)
        {
            Camada(fundo, "BG_Far", BgFar, 0.97f, -30, 1.08f, 3.2f);
            Camada(fundo, "BG_Mid", BgMid, 0.95f, -20, 1f, 0.6f);
            Camada(fundo, "BG_Near", BgNear, 0.93f, -10, 1f, -0.5f);
            return;
        }

        // Faixa de mar no horizonte: bloco chapado, sem parallax, sem emenda possível.
        Bloco("Horizon_Sea", fundo, LevelEnd * 0.5f, -0.4f, LevelEnd + 60f, 3.2f, CorMarDistante, -40);

        Silhueta(fundo, "Hills_Far", 0.55f, -30, CorMontanha, 3f, 6.5f, 9f, 16f, 7311);
        Silhueta(fundo, "Hills_Mid", 0.35f, -20, CorColina, 1.6f, 3.4f, 6f, 11f, 9042);
    }

    /// <summary>
    /// As cinco camadas de Ítaca, da mais distante para a mais próxima. Devolve false se a
    /// arte não estiver importada, e aí a fase volta às silhuetas chapadas.
    ///
    /// A diferença para <see cref="Camada"/>: estas ladrilham. As telas pintadas antigas não
    /// eram contínuas nas bordas, o que obrigava a um fator de parallax alto (0,93–0,97) com
    /// uma imagem só cobrindo a fase inteira — ou seja, parallax quase nulo. As camadas de
    /// Ítaca foram feitas para ladrilhar (a de montanha por espelho, ver
    /// <c>Tools/make-tileable.js</c>), então o fator pode descer para onde a profundidade
    /// realmente aparece.
    ///
    /// Fator ALTO é longe: a camada acompanha a câmera e quase não desliza na tela.
    /// </summary>
    private static bool MontarFundoDeItaca(Transform pai)
    {
        if (Arte(ItacaCeu) == null)
        {
            return false;
        }

        // A ordem de empilhamento segue a composição de referência do pipeline de arte
        // (Docs/Environment_Ithaca/Layers/_composicao_demo.png): céu, nuvens, ilha no
        // horizonte, faixa de mar e, à frente de tudo, a serra.
        //
        // A ilha nasce ABAIXO da linha do mar de propósito: o mar é desenhado depois e cobre
        // a base dela, que é o que faz a ilha parecer estar no horizonte e não boiando.
        CamadaLadrilhada(pai, "BG_Sky", ItacaCeu, 1.00f, -50, -2.4f);
        CamadaLadrilhada(pai, "BG_Clouds", ItacaNuvens, 0.90f, -48, 1.6f);
        CamadaLadrilhada(pai, "BG_Island", ItacaIlha, 0.82f, -46, -0.9f);
        CamadaLadrilhada(pai, "BG_Ocean", ItacaOceano, 0.74f, -44, -3.0f);
        CamadaLadrilhada(pai, "BG_Mountains", ItacaMontanhas, 0.62f, -42, -2.6f);
        return true;
    }

    /// <summary>
    /// Uma camada de parallax que ladrilha, num renderer só.
    ///
    /// <c>SpriteDrawMode.Tiled</c> repete o sprite dentro de <c>size</c> gerando a geometria,
    /// então uma camada de 430 unidades custa um renderer em vez de centenas de objetos. Ele
    /// exige malha FullRect no import — com malha Tight o recorte é descartado e a camada sai
    /// esticada em vez de repetida.
    ///
    /// A largura vem da conta do deslize: a camada anda <c>(1 - fator)</c> vezes o
    /// comprimento da fase em relação à câmera, e ainda precisa cobrir a tela nas duas pontas.
    /// </summary>
    private static void CamadaLadrilhada(Transform pai, string nome, string caminho, float fator,
        int ordem, float baseY)
    {
        Sprite arte = Arte(caminho);
        if (arte == null)
        {
            return;
        }

        float deslize = LevelEnd * (1f - fator);
        float largura = deslize + LarguraDaTela * 2f;

        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);

        // Centraliza no meio do trajeto que a camada faz EM RELAÇÃO à câmera: esse trajeto vai
        // de 0 (no começo da fase) a -deslize (no fim), então o meio fica em deslize/2.
        go.transform.position = new Vector3(deslize * 0.5f, baseY, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        // O pivô da arte de cenário é BottomCenter, então size.y cresce para cima a partir
        // de baseY. Manter size.y na altura nativa impede a camada de repetir na vertical —
        // um céu em degradê repetido verticalmente vira listra.
        sr.size = new Vector2(largura, arte.bounds.size.y);

        var parallax = go.AddComponent<ParallaxLayer>();
        Wire(parallax, "parallaxFactor", fator);
    }

    /// <summary>
    /// Cor do bloco de céu que fica atrás de tudo e garante cobertura.
    ///
    /// Com a arte de Ítaca ele usa o topo do degradê — <c>#6481a0</c>, o passo Profunda da
    /// rampa de Céu. Se a cor não casar com o topo do sprite, aparece uma linha horizontal
    /// exatamente onde o sprite acaba e o bloco começa. Está fixo aqui, e não lido da
    /// textura, porque as texturas são importadas com <c>isReadable</c> desligado — ligar
    /// custaria memória em todas elas para uma cor só.
    /// </summary>
    private static Color CorDoTopoDoCeu()
    {
        return Arte(ItacaCeu) != null ? CorCeuProfundo : CorCeu;
    }

    /// <summary>
    /// Uma camada de parallax, num sprite só.
    ///
    /// Repetir o sprite lado a lado para cobrir 430 unidades era o caminho óbvio e
    /// estava errado: estas imagens não são contínuas nas bordas, então cada emenda
    /// virava uma linha vertical no meio da paisagem. A saída é o contrário — um
    /// fator de parallax ALTO, perto de 1, faz a camada acompanhar a câmera quase
    /// coladinha, e aí uma imagem só cobre a fase inteira.
    ///
    /// A conta: a camada desliza (1 - fator) x comprimento da fase em relação à
    /// câmera. Com fator 0,93 num nível de 430, são 30 unidades de deslize; somando a
    /// largura da tela (~18), a imagem precisa de 48 e tem 69. Sobra folga, e o
    /// parallax continua existindo — só é sutil, que é o certo para uma paisagem
    /// distante.
    /// </summary>
    private static void Camada(Transform pai, string nome, string caminho, float fator, int ordem,
        float escala, float y)
    {
        Sprite arte = Arte(caminho);
        if (arte == null)
        {
            return;
        }

        float largura = arte.bounds.size.x * escala;
        float deslize = LevelEnd * (1f - fator);

        if (deslize + LarguraDaTela > largura)
        {
            Debug.LogWarning("[Prologue] camada " + nome + " nao cobre a fase: precisa de "
                + (deslize + LarguraDaTela).ToString("0.0") + " e tem " + largura.ToString("0.0"));
        }

        // Centraliza a camada no meio do trajeto da câmera, para a folga ficar
        // repartida entre as duas pontas em vez de acabar toda de um lado só.
        float meio = LevelEnd * 0.5f;

        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.position = new Vector3(meio * (1f - fator), y, 2f);
        go.transform.localScale = new Vector3(escala, escala, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;

        var parallax = go.AddComponent<ParallaxLayer>();
        Wire(parallax, "parallaxFactor", fator);
    }


    /// <summary>
    /// Uma camada de morros: blocos de larguras e alturas variadas ao longo da fase.
    ///
    /// Blocos soltos em vez de uma imagem repetida resolvem o problema das emendas por
    /// construção — não existe borda para casar. E o parallax pode voltar a ser forte
    /// (0,55 e 0,35), que é o que dá sensação de profundidade de verdade.
    ///
    /// A conta da cobertura: a camada aparece em <c>partida + câmera x fator</c>, então
    /// para acompanhar a câmera de 0 até o fim da fase, os blocos precisam existir de
    /// -meia tela até <c>fim x (1 - fator) + meia tela</c>. Fora disso seria desenho
    /// jogado fora.
    ///
    /// A semente fixa mantém o relevo idêntico a cada remontagem: a fase não pode mudar
    /// de aparência só porque alguém rodou o builder de novo.
    /// </summary>
    private static void Silhueta(Transform pai, string nome, float fator, int ordem, Color cor,
        float alturaMin, float alturaMax, float larguraMin, float larguraMax, int semente)
    {
        var raiz = new GameObject(nome);
        raiz.transform.SetParent(pai, false);

        UnityEngine.Random.InitState(semente);

        float meiaTela = LarguraDaTela * 0.5f;
        float inicio = -meiaTela - larguraMax;
        float fim = LevelEnd * (1f - fator) + meiaTela + larguraMax;

        int i = 0;
        for (float x = inicio; x < fim; i++)
        {
            float largura = UnityEngine.Random.Range(larguraMin, larguraMax);
            float altura = UnityEngine.Random.Range(alturaMin, alturaMax);

            // Os morros se sobrepõem um pouco, senão aparece o céu entre eles.
            Bloco(nome + "_" + i, raiz.transform, x + largura * 0.5f,
                GroundTop + altura * 0.5f, largura, altura, cor, ordem);

            x += largura * 0.72f;
        }

        var parallax = raiz.gameObject.AddComponent<ParallaxLayer>();
        Wire(parallax, "parallaxFactor", fator);
    }

    private static void MontarChao()
    {
        // Chão contínuo do começo ao cais. NENHUM buraco antes do campo de
        // treinamento, de propósito: o pulo só é ensinado no Ato 4, e um vão mortal
        // antes disso cobra do jogador uma habilidade que a fase ainda não ensinou —
        // três quedas e a jornada acabava no menu principal.
        //
        // A arte de terreno é VESTIDA por cima destes colisores, não os substitui: a
        // geometria de colisão daqui é ajustada e conferida pelo PrologueProbe, e trocá-la
        // por um Tilemap arriscaria a garantia de que a fase é terminável.
        Chao("Floor_Town", -8f, 79f);
        Chao("Floor_Recruit", 79f, 154f);
        Chao("Floor_Training", 154f, 240f);
        Chao("Floor_Arsenal", 240f, 276f);
        Chao("Floor_Port", 276f, 322f);
        Chao("Pier", 322f, 341f);

        // Os quatro kits acompanham o percurso: vila e recrutamento em grama, treino e
        // arsenal em terra batida, porto em pedra, cais em madeira.
        Terreno("Terrain_Town", -8f, 79f, ItacaTerrenoGrama);
        Terreno("Terrain_Recruit", 79f, 154f, ItacaTerrenoGrama);
        Terreno("Terrain_Training", 154f, 240f, ItacaTerrenoTerra);
        Terreno("Terrain_Arsenal", 240f, 276f, ItacaTerrenoTerra);
        Terreno("Terrain_Port", 276f, 322f, ItacaTerrenoPedra, GroundTop, 5, ItacaTerrenoTerra);
        // O cais é um deck, não um paredão: uma fiada só, e o mar aparece por baixo.
        Terreno("Terrain_Pier", 322f, 341f, ItacaTerrenoMadeira, GroundTop, 1);

        // As plataformas do treino ficam ACIMA do chão, não sobre um abismo: quem erra
        // o pulo cai de volta no campo e tenta de novo, em vez de perder uma vida.
        Plataforma("Training_Platform_1", 189f, -0.4f, 4f);
        Plataforma("Training_Platform_2", 194.5f, 0.8f, 4f);
        Plataforma("Training_Platform_3", 200f, -0.4f, 4f);

        Terreno("Terrain_Platform_1", 187f, 191f, ItacaTerrenoPedra, -0.4f, 1, ItacaTerrenoTerra);
        Terreno("Terrain_Platform_2", 192.5f, 196.5f, ItacaTerrenoPedra, 0.8f, 1, ItacaTerrenoTerra);
        Terreno("Terrain_Platform_3", 198f, 202f, ItacaTerrenoPedra, -0.4f, 1, ItacaTerrenoTerra);

        // Terra sob o chão inteiro: sem isto o mundo termina numa linha e o fundo aparece por
        // baixo, como se o chão flutuasse. Ladrilhado com o tile cheio de terra — era um bloco
        // chapado de 349 unidades, e ele APARECE quando a câmera desce, o que eu tinha
        // presumido que não acontecia.
        Sprite terraCheia = Tile(ItacaTerrenoTerra, TileCheio);
        if (terraCheia != null)
        {
            FaixaDeTiles("Bedrock", 166.5f, -6.4f, 349f, 8f, terraCheia, -3);
        }
        else
        {
            Bloco("Bedrock", world, 166.5f, -6.4f, 349f, 8f, CorTerra, -3);
        }

        // Mar depois do cais.
        // Mar depois do cais. A camada de oceano do parallax serve aqui também: é a mesma
        // água, ladrilha, e um bloco azul chapado ao lado dela denunciava a diferença.
        Sprite oceano = Arte(ItacaOceano);
        if (oceano != null)
        {
            var mar = new GameObject("Sea");
            mar.transform.SetParent(world, false);
            mar.transform.position = new Vector3(400f, GroundTop - 1f, 0f);
            var sr = mar.AddComponent<SpriteRenderer>();
            sr.sprite = oceano;
            sr.sortingOrder = -4;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            // Altura NATIVA. Esticar o size.y faz a faixa repetir na vertical e aparecer uma
            // listra no meio do mar — o mesmo motivo pelo qual o céu não repete na vertical.
            sr.size = new Vector2(140f, oceano.bounds.size.y);

            // Água profunda abaixo da faixa, na cor mais funda da rampa de Água, para o mar
            // não terminar numa linha reta onde o sprite acaba.
            Bloco("Sea_Deep", world, 400f, -8f, 140f, 8f, CorAguaProfunda, -5);
        }
        else
        {
            Bloco("Sea", world, 400f, -6f, 140f, 8f, CorMar, -4);
        }

        MontarLimites();
    }

    /// <summary>
    /// Paredes nas duas pontas, enquadramento da câmera e rede de segurança.
    ///
    /// As paredes são altas de propósito (12 unidades): o pulo alcança 2,45, então não
    /// existe combinação de plataforma e pulo que passe por cima. E ficam encostadas na
    /// borda do mundo, fora do caminho — o jogador só descobre que existem quando tenta
    /// sair.
    /// </summary>
    private static void MontarLimites()
    {
        const float esquerda = -8f;
        const float direita = 341f;

        // As paredes são física, não cenário: com o fundo chapado elas passavam por barranco,
        // mas contra a paisagem pintada viram uma laje marrom de 12 unidades na frente do mar.
        // O colisor continua; só o desenho sai.
        GameObject paredeEsq = Vestido(Bloco("Boundary_Left", world, esquerda - 0.5f, GroundTop + 6f,
            1f, 12f, CorTerra, -2, true));
        GameObject paredeDir = Vestido(Bloco("Boundary_Right", world, direita + 0.5f, GroundTop + 6f,
            1f, 12f, CorTerra, -2, true));

        var limites = new GameObject("LevelBounds");
        limites.transform.SetParent(world, false);

        var comp = limites.AddComponent<LevelBounds>();
        Wire(comp, "playableMinX", esquerda);
        Wire(comp, "playableMaxX", direita);
        Wire(comp, "leftWall", paredeEsq.GetComponent<BoxCollider2D>());
        Wire(comp, "rightWall", paredeDir.GetComponent<BoxCollider2D>());
        Wire(comp, "cameraFollow", cameraFollow);
        Wire(comp, "cameraMinX", esquerda - 2f);
        Wire(comp, "cameraMaxX", 360f);
        Wire(comp, "cameraMinY", -6f);
        Wire(comp, "cameraMaxY", 8f);
        Wire(comp, "player", player.transform);
        Wire(comp, "rescueMargin", 6f);
        Wire(comp, "rescueMinY", -20f);

        var kill = new GameObject("KillZone");
        kill.transform.SetParent(world, false);
        kill.transform.position = new Vector3(LevelEnd * 0.5f, -11f, 0f);
        var killBox = kill.AddComponent<BoxCollider2D>();
        killBox.size = new Vector2(LevelEnd + 80f, 4f);
        killBox.isTrigger = true;
        kill.AddComponent<KillZone>();
    }

    // ------------------------------------------------------------------ diálogos

    private static Dictionary<string, DialogueSequence> MontarDialogos()
    {
        var falas = new Dictionary<string, DialogueSequence>();

        falas["Intro"] = Dialogo("IntroDialogueController", "odysseus", 3);
        falas["Herald"] = Dialogo("HeraldDialogueController", null, 8,
            new[] { "herald", "herald", "herald", "herald", "odysseus", "odysseus", "herald", "odysseus" });
        falas["Mentor"] = Dialogo("MentorDialogueController", null, 4,
            new[] { "mentor", "odysseus", "odysseus", "mentor" });
        falas["Penelope"] = Dialogo("PenelopeDialogueController", null, 6,
            new[] { "penelope", "odysseus", "penelope", "odysseus", "penelope", "odysseus" });
        falas["Telemaco"] = Dialogo("TelemacoDialogueController", null, 4,
            new[] { "telemachus", "odysseus", "telemachus", "odysseus" });
        falas["Promise"] = Dialogo("PromiseDialogueController", null, 4,
            new[] { "penelope", "odysseus", "odysseus", "penelope" });

        falas["Fisherman"] = Dialogo("RecruitFishermanDialogue", "fisherman", 1);
        falas["Farmer"] = Dialogo("RecruitFarmerDialogue", "farmer", 1);
        falas["Sailor"] = Dialogo("RecruitSailorDialogue", "sailor", 1);
        falas["Smith"] = Dialogo("RecruitBlacksmithDialogue", "blacksmith", 2);
        falas["Eurylochus"] = Dialogo("RecruitEurylochusDialogue", "eurylochus", 2);
        falas["Rower"] = Dialogo("RecruitRowerDialogue", "rower", 1);
        falas["Shepherd"] = Dialogo("RecruitShepherdDialogue", "shepherd", 1);
        falas["Watchman"] = Dialogo("RecruitWatchmanDialogue", "watchman", 1);
        falas["Carpenter"] = Dialogo("RecruitCarpenterDialogue", "carpenter", 1);
        falas["Elpenor"] = Dialogo("RecruitElpenorDialogue", null, 2,
            new[] { "elpenor", "odysseus" });

        falas["Muster"] = Dialogo("MusterDialogueController", "odysseus", 2);
        falas["TrainerIntro"] = Dialogo("TrainerIntroDialogue", "trainer", 3);
        falas["TrainerEnd"] = Dialogo("TrainerEndDialogue", null, 3,
            new[] { "odysseus", "odysseus", "trainer" });
        falas["Blacksmith"] = Dialogo("BlacksmithDialogueController", null, 4,
            new[] { "blacksmith", "blacksmith", "blacksmith", "odysseus" });
        falas["Port"] = Dialogo("PortDialogueController", "odysseus", 2);

        falas["FarewellPenelope"] = Dialogo("FarewellPenelopeDialogue", null, 5,
            new[] { "penelope", "odysseus", "penelope", "odysseus", "penelope" });
        falas["FarewellTelemaco"] = Dialogo("FarewellTelemacoDialogue", null, 4,
            new[] { "telemachus", "telemachus", "odysseus", "odysseus" });
        falas["Boarding"] = Dialogo("BoardingDialogueController", null, 2,
            new[] { "", "odysseus" });
        falas["Outro"] = Dialogo("OutroDialogueController", "", 3);

        return falas;
    }

    /// <summary>
    /// Cria uma sequência de falas. O texto NÃO mora aqui: cada linha recebe a chave
    /// "dlg.&lt;cena&gt;.&lt;controlador&gt;.&lt;índice&gt;" e o conteúdo vem da tabela de
    /// idiomas, que é onde inglês e português convivem.
    /// </summary>
    private static DialogueSequence Dialogo(string nome, string falante, int linhas, string[] falantes = null)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(dialoguesRoot, false);

        var seq = go.AddComponent<DialogueSequence>();
        Wire(seq, "panel", dialoguePanel);
        Wire(seq, "speakerText", dialogueSpeaker);
        Wire(seq, "bodyText", dialogueBody);
        Wire(seq, "inputActions", controls);

        var so = new SerializedObject(seq);
        SerializedProperty lista = so.FindProperty("lines");
        lista.arraySize = linhas;

        for (int i = 0; i < linhas; i++)
        {
            SerializedProperty linha = lista.GetArrayElementAtIndex(i);
            linha.FindPropertyRelative("key").stringValue =
                "dlg.Level_01_Itaca_Prologue." + nome + "." + i;
            linha.FindPropertyRelative("speaker").stringValue =
                falantes != null ? falantes[i] : (falante ?? string.Empty);
            linha.FindPropertyRelative("text").stringValue = string.Empty;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        return seq;
    }

    // ------------------------------------------------------------------ atos

    private static LevelObjective Objetivo(string nome, string chave, int necessario)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(actsRoot, false);

        var obj = go.AddComponent<LevelObjective>();
        Wire(obj, "titleKey", chave);
        Wire(obj, "required", necessario);
        Wire(obj, "banner", banner);
        Wire(obj, "playerLock", playerLock);
        return obj;
    }

    /// <summary>
    /// Portão de progressão: barra o caminho até o objetivo indicado fechar, e explica
    /// o que falta enquanto estiver barrando.
    ///
    /// A raiz NUNCA é desligada — é ela que sabe reavaliar a condição. O que sai ao
    /// abrir é só o colisor e a folha da porta.
    /// </summary>
    private static GameObject Portao(string nome, float x, string chaveDaDica,
        LevelObjective exigido)
    {
        var raiz = new GameObject(nome);
        raiz.transform.SetParent(world, false);
        raiz.transform.position = new Vector3(x, 0f, 0f);

        // A folha da porta: é ela que tem o colisor sólido e some ao abrir.
        GameObject folha = Bloco(nome + "_Door", raiz.transform, x, GroundTop + 2.5f,
            1.2f, 5f, CorPortao, 1, true);

        // Com arte, o bloco vira só colisor e a folha desenhada entra no lugar dele.
        //
        // O desenho tem a MESMA altura que o colisor (5 un), de propósito. Desenhar um portão
        // baixo e deixar o colisor alto criaria parede invisível, e o README é explícito:
        // parede muda é indistinguível de bug. O que se vê é o que bloqueia.
        GameObject desenho = Prop(nome + "_Leaf", raiz.transform, x, ItacaPortao, 1);
        if (desenho != null)
        {
            folha.GetComponent<SpriteRenderer>().enabled = false;
        }

        // Os batentes ficam de pé mesmo depois de aberto, para o lugar continuar
        // legível como uma passagem.
        if (Prop(nome + "_Post_L", world, x - 1.5f, ItacaBatente, 1) == null)
        {
            Bloco(nome + "_Post_L", world, x - 1.2f, GroundTop + 3f, 0.6f, 6f, CorMadeira, 1);
            Bloco(nome + "_Post_R", world, x + 1.2f, GroundTop + 3f, 0.6f, 6f, CorMadeira, 1);
        }
        else
        {
            Prop(nome + "_Post_R", world, x + 1.5f, ItacaBatente, 1);
        }

        var aviso = new GameObject(nome + "_Notice");
        aviso.transform.SetParent(raiz.transform, false);
        aviso.transform.position = new Vector3(x, GroundTop + 1.5f, 0f);
        var area = aviso.AddComponent<BoxCollider2D>();
        area.size = new Vector2(7f, 4f);
        area.isTrigger = true;

        var portao = raiz.AddComponent<ProgressionGate>();
        Wire(portao, "requiredObjective", exigido);
        Wire(portao, "blockCollider", folha.GetComponent<BoxCollider2D>());
        Wire(portao, "noticeArea", area);
        Wire(portao, "lockedMessageKey", chaveDaDica);
        Wire(portao, "prompt", prompt);
        // A folha desenhada some junto com o bloco quando o portão abre. Sem isto o colisor
        // sairia e o desenho ficaria, com o jogador atravessando um portão fechado.
        if (desenho != null)
        {
            WireArray(portao, "lockedVisuals", folha, desenho);
        }
        else
        {
            WireArray(portao, "lockedVisuals", folha);
        }

        return raiz;
    }

    private static GameObject Checkpoint(float x)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Checkpoint.prefab");
        var instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab, world);
        instancia.transform.position = new Vector3(x, GroundTop + 0.7f, 0f);
        return instancia;
    }

    private static GameObject Coletavel(float x, float y)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Collectible.prefab");
        var instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab, world);
        instancia.transform.position = new Vector3(x, y, 0f);
        return instancia;
    }

    /// <summary>NPC conversável: figura + gatilho de alcance + fala.</summary>
    /// <summary>
    /// Um sprite está na densidade do projeto quando importa a 42,857 px por unidade. A arte
    /// pintada dos NPCs nomeados importa a 129 e precisa ser escalada; a arte de figurante
    /// gerada agora já nasce na densidade certa e não pode ser tocada.
    /// </summary>
    private static bool EhPixelArt(Sprite arte)
    {
        return Mathf.Abs(arte.pixelsPerUnit - 42.857143f) < 0.01f;
    }

    /// <summary>
    /// Um sprite nomeado dentro de uma folha fatiada. <c>Arte</c> devolve o PRIMEIRO sprite do
    /// arquivo, que numa folha de 99 quadros é quase certamente o errado.
    /// </summary>
    private static Sprite SpriteDaFolha(string caminho, string nome)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(caminho))
        {
            if (asset is Sprite sprite && sprite.name == nome)
            {
                return sprite;
            }
        }

        return null;
    }

    /// <summary>Quadro parado (Idle_00) de um personagem do elenco; null se a folha não existir.</summary>
    private static Sprite Elenco(string chr)
    {
        return SpriteDaFolha(ElencoPasta + "CHR_" + chr + ".png", "CHR_" + chr + "_Idle_00");
    }

    /// <summary>
    /// Liga o Idle do elenco no corpo da figura e vira o rosto. O FPS varia um pouco com a
    /// posição: sem isso os seis soldados do treino respiram em uníssono, que lê como máquina.
    /// A arte olha para a direita (o master é todo `east`); quem conversa com o Odisseu olha
    /// para a esquerda, que é de onde o jogador chega.
    /// </summary>
    private static void DarVida(GameObject figura, string chr, bool olharEsquerda)
    {
        Transform corpo = figura != null ? figura.transform.Find("Body") : null;
        if (corpo == null || Elenco(chr) == null)
        {
            return;
        }

        corpo.GetComponent<SpriteRenderer>().flipX = olharEsquerda;
        var animador = corpo.gameObject.AddComponent<SpriteAnimator>();
        Wire(animador, "resourcePath", ElencoResources + "CHR_" + chr);
        Wire(animador, "defaultState", "Idle");
        Wire(animador, "defaultFramesPerSecond", 5.5f + Mathf.Repeat(figura.transform.position.x * 0.37f, 1.2f));
    }

    private static GameObject FiguraDoElenco(string nome, Transform pai, float x, string chr,
        Color cor, int ordem = 1, bool olharEsquerda = false)
    {
        Sprite arte = Elenco(chr);
        GameObject go = arte != null
            ? Figura(nome, pai, x, 0.9f, arte.bounds.size.y, cor, ordem, arte)
            : Figura(nome, pai, x, 0.8f, 1.6f, cor, ordem);
        DarVida(go, chr, olharEsquerda);
        return go;
    }

    /// <summary>NPC conversável do elenco: olha para o jogador e respira.</summary>
    private static GameObject NpcDoElenco(string nome, Transform pai, float x, DialogueSequence fala,
        LevelObjective objetivo, string chr, Color cor, bool automatico = false)
    {
        GameObject go = Npc(nome, pai, x, fala, objetivo, cor, 1.6f, Elenco(chr), automatico);
        DarVida(go, chr, true);
        return go;
    }

    private static GameObject Npc(string nome, Transform pai, float x, DialogueSequence fala,
        LevelObjective objetivo, Color cor, float altura = 1.6f, Sprite arte = null,
        bool automatico = false)
    {
        // Arte de figurante em pixel art usa a altura NATIVA, para o fator de escala dar 1 e
        // os 42,857 px por unidade se manterem. A arte pintada antiga (129 px/un) precisa da
        // escala, e por isso a regra vale só para o que já está na densidade certa.
        float alturaReal = arte != null && EhPixelArt(arte) ? arte.bounds.size.y : altura;
        GameObject go = Figura(nome, pai, x, 0.9f, alturaReal, cor, 1, arte);

        var alcance = go.AddComponent<BoxCollider2D>();
        alcance.size = new Vector2(3.2f, 3f);
        alcance.offset = CentroDaFigura(go, alturaReal);
        alcance.isTrigger = true;

        var dialogo = go.AddComponent<NPCDialogue>();
        Wire(dialogo, "inputActions", controls);
        Wire(dialogo, "dialogue", fala);
        Wire(dialogo, "playerLock", playerLock);
        Wire(dialogo, "prompt", prompt);
        Wire(dialogo, "objective", objetivo);
        Wire(dialogo, "autoStart", automatico);
        return go;
    }

    /// <summary>Ponto de interação de cenário: pilha de escudos, barris, velas.</summary>
    private static GameObject Ponto(string nome, Transform pai, float x, string chaveResultado,
        LevelObjective objetivo, Color cor, float largura = 1.4f, float altura = 1.2f,
        DialogueSequence fala = null, string caminhoDaArte = null)
    {
        // Com arte, a altura passa a ser a NATIVA do sprite. O Figura escala a arte para a
        // altura pedida — o que é certo para NPC, cuja proporção é própria, e errado para
        // cenário: esticar quebraria os 42,857 px por unidade que todo o resto respeita.
        // Passando a altura nativa, o fator de escala dá exatamente 1.
        Sprite arte = caminhoDaArte != null ? Arte(caminhoDaArte) : null;
        GameObject go = arte != null
            ? Figura(nome, pai, x, largura, arte.bounds.size.y, cor, 1, arte)
            : Figura(nome, pai, x, largura, altura, cor);

        var alcance = go.AddComponent<BoxCollider2D>();
        alcance.size = new Vector2(3f, 3f);
        alcance.offset = CentroDaFigura(go, altura);
        alcance.isTrigger = true;

        var ponto = go.AddComponent<InteractPoint>();
        Wire(ponto, "inputActions", controls);
        Wire(ponto, "resultKey", chaveResultado);
        Wire(ponto, "prompt", prompt);
        Wire(ponto, "objective", objetivo);
        Wire(ponto, "playerLock", playerLock);
        Wire(ponto, "dialogue", fala);
        Wire(ponto, "doneRenderer", go.transform.Find("Body").GetComponent<SpriteRenderer>());
        return go;
    }

    /// <summary>Boneco de treino / alvo de arco: leva dano de verdade e avisa o curso.</summary>
    private static GameObject Alvo(string nome, Transform pai, float x, TrainingCourse curso,
        TrainingAction acao, Color cor, float largura, float altura, string caminhoDaArte = null)
    {
        // Com arte, o tamanho vem do desenho — e o COLISOR vai junto. Deixar a caixa no
        // tamanho do placeholder enquanto o desenho encolhe daria um alvo que se acerta sem
        // encostar, ou que não se acerta encostando; nos dois casos o tutorial mente.
        Sprite arte = caminhoDaArte != null ? Arte(caminhoDaArte) : null;
        if (arte != null)
        {
            largura = arte.bounds.size.x;
            altura = arte.bounds.size.y;
        }

        GameObject go = Figura(nome, pai, x, largura, altura, cor, 1, arte);
        go.layer = LayerEnemy;

        // Gatilho, e nao parede: a espada usa OverlapCircle e a flecha e ela propria um
        // gatilho, entao os dois acertam do mesmo jeito — e uma fileira de bonecos
        // solidos no meio do campo viraria um bloqueio de caminho.
        var caixa = go.AddComponent<BoxCollider2D>();
        caixa.size = new Vector2(largura, altura);
        caixa.offset = CentroDaFigura(go, altura);
        caixa.isTrigger = true;

        var vida = go.AddComponent<HealthSystem>();
        Wire(vida, "maxHealth", 9999);

        var alvo = go.AddComponent<TrainingTarget>();
        Wire(alvo, "course", curso);
        Wire(alvo, "reportAction", (int)acao);
        Wire(alvo, "body", go.transform.Find("Body").GetComponent<SpriteRenderer>());
        return go;
    }

    /// <summary>
    /// Parceiro de treino: é o EnemyBasic de sempre, com dois ajustes de instância.
    ///
    /// Vida alta, porque um parceiro que morre no segundo golpe deixa a etapa da defesa
    /// sem ninguém para bloquear. E alcance de detecção grande, porque ele precisa ir
    /// ATÉ o jogador: com o raio padrão de 4, quem terminou a etapa anterior alguns
    /// metros adiante segura o escudo num campo vazio e a etapa nunca fecha. O soldado
    /// é que procura o rei, não o contrário.
    /// </summary>
    private static GameObject Sparring(string nome, Transform pai, float x)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnemyBasic.prefab");
        var instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pai);
        instancia.name = nome;
        instancia.transform.position = new Vector3(x, GroundTop + 0.6f, 0f);

        var vida = instancia.GetComponent<HealthSystem>();
        Wire(vida, "maxHealth", 9999);

        var controle = instancia.GetComponent<Odisseia.Enemies.EnemyController>();
        Wire(controle, "detectionRadius", 30f);

        // Explícito de propósito: o prefab foi salvo antes deste campo existir, então
        // o valor vinha do inicializador da classe e não aparecia no Inspector. A
        // etapa inteira depende do golpe ser bloqueável — isso não pode ficar
        // dependendo de um padrão invisível.
        Wire(controle, "attackBlockable", true);

        // Um parceiro de treino tem que ser movimentado: com o ritmo do inimigo comum
        // (chega a 2,5 e bate a cada 1 s) a etapa vira espera. Aqui ele trota até o rei
        // e insiste.
        Wire(controle, "chaseSpeed", 3.5f);
        Wire(controle, "attackCooldown", 0.6f);

        // Precisa ser maior que a detecção, senão ele desiste no mesmo frame em que vê.
        Wire(controle, "loseTargetRadius", 45f);
        return instancia;
    }

    private static GameObject Gatilho(string nome, Transform pai, float x, DialogueSequence fala,
        float largura = 2.5f)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.position = new Vector3(x, GroundTop + 1.5f, 0f);

        var caixa = go.AddComponent<BoxCollider2D>();
        caixa.size = new Vector2(largura, 4f);
        caixa.isTrigger = true;

        var gatilho = go.AddComponent<DialogueTrigger>();
        Wire(gatilho, "dialogue", fala);
        Wire(gatilho, "playerLock", playerLock);
        return go;
    }

    // ------------------------------------------------------------------ a fase

    private static void MontarCena(Dictionary<string, DialogueSequence> falas)
    {
        // A abertura da fase continua sendo a cutscene de LevelIntro.
        var intro = Object.FindAnyObjectByType<LevelIntro>();
        Wire(intro, "playerLock", playerLock);
        Wire(intro, "introDialogue", falas["Intro"]);
        Wire(goal, "outroDialogue", falas["Outro"]);

        // --- objetivos, na ordem dos atos --------------------------------------
        LevelObjective explorar = Objetivo("Obj_1_Explore", "obj.prologue.explore", 4);
        LevelObjective convocacao = Objetivo("Obj_1_Summons", "obj.prologue.summons", 1);
        LevelObjective familia = Objetivo("Obj_2_Family", "obj.prologue.family", 2);
        LevelObjective recrutar = Objetivo("Obj_3_Recruit", "obj.prologue.recruit", 10);
        LevelObjective treinar = Objetivo("Obj_4_Training", "obj.prologue.training", 1);
        LevelObjective equipar = Objetivo("Obj_5_Equipment", "obj.prologue.equipment", 5);
        LevelObjective navios = Objetivo("Obj_5_Ships", "obj.prologue.ships", 4);
        LevelObjective despedida = Objetivo("Obj_6_Farewell", "obj.prologue.farewell", 2);
        LevelObjective partir = Objetivo("Obj_6_Depart", "obj.prologue.depart", 1);

        Wire(explorar, "startsActive", true);
        Encadear(explorar, convocacao);
        Encadear(convocacao, familia);
        Encadear(familia, recrutar);
        Encadear(recrutar, treinar);
        Encadear(treinar, equipar);
        Encadear(equipar, navios);
        Encadear(navios, despedida);
        Encadear(despedida, partir);

        Wire(convocacao, "completionDialogue", falas["Mentor"]);
        Wire(familia, "completionDialogue", falas["Promise"]);
        Wire(recrutar, "completionDialogue", falas["Muster"]);
        Wire(treinar, "completionDialogue", falas["TrainerEnd"]);
        Wire(navios, "completionDialogue", falas["Port"]);

        // --- portões: um ato de cada vez ---------------------------------------
        // Cada portão guarda a própria condição. O objetivo não sabe que existe
        // portão nenhum — quem pergunta é a barreira, e por isso um aviso perdido não
        // deixa mais a fase trancada.
        Portao("Gate_Hall", 62f, "gate.prologue.hall", convocacao);
        Portao("Gate_Recruit", 80f, "gate.prologue.recruit", familia);
        Portao("Gate_Training", 154.5f, "gate.prologue.training", recrutar);
        Portao("Gate_Arsenal", 240.5f, "gate.prologue.arsenal", treinar);
        Portao("Gate_Port", 277f, "gate.prologue.port", equipar);

        MontarAto1e2(falas, explorar, convocacao, familia);
        MontarAto3(falas, recrutar);
        MontarAto4(falas, treinar);
        MontarAto5(falas, equipar, navios);
        MontarAto6(falas, navios, despedida, partir);
    }

    private static void Encadear(LevelObjective de, LevelObjective para)
    {
        Wire(de, "next", para);
    }

    // Ato 1 (Ítaca e a convocação) e Ato 2 (Penélope e Telêmaco)
    private static void MontarAto1e2(Dictionary<string, DialogueSequence> falas,
        LevelObjective explorar, LevelObjective convocacao, LevelObjective familia)
    {
        Transform cidade = new GameObject("Act1_Ithaca").transform;
        cidade.SetParent(world, false);

        Casa(cidade, 8f, 2.4f, 2.6f);
        Casa(cidade, 14f, 3f, 3.4f);
        Casa(cidade, 24f, 2.6f, 2.4f);
        Casa(cidade, 34f, 3.2f, 3f);
        Casa(cidade, 40f, 2.4f, 2.6f);

        // Palácio: o volume que o jogador reconhece de longe.
        // O palácio precisa ser reconhecível de longe: é para onde o objetivo manda o
        // jogador depois da convocação, e mais uma caixa bege no meio de outras caixas
        // beges não é destino nenhum.
        // A fachada do megaron substitui o prédio, o frontão, o friso e as cinco colunas: ela
        // já traz colunata, friso e cornija desenhados. Tem 17,78 x 7,00 un — a altura bate
        // exatamente com a do bloco que havia aqui, e a largura fica 2,2 un menor.
        //
        // Espelhada de propósito. A porta está 3,99 un à direita do centro da fachada; com o
        // espelho ela cai em 64,0, ao lado do Gate_Hall (62) e do tear (63). Sem espelhar, a
        // porta desenhada ficaria em 72,0 e o jogador entraria no salão por uma parede.
        if (Prop("Palace", cidade, 68f, ItacaPalacioMegaron, -2, GroundTop, true) == null)
        {
            Bloco("Palace", cidade, 68f, GroundTop + 3.5f, 20f, 7f, CorPalacio, -2);
            Bloco("Palace_Pediment", cidade, 68f, GroundTop + 7.3f, 22f, 1.2f, CorTelhado, -1);
            Bloco("Palace_Frieze", cidade, 68f, GroundTop + 6.4f, 20.6f, 0.5f, CorPedra, -1);

            for (int i = 0; i < 5; i++)
            {
                Bloco("Column_" + i, cidade, 60f + i * 4f, GroundTop + 3f, 0.9f, 6f, CorPedra, -1);
            }

            for (int i = 0; i < 3; i++)
            {
                Bloco("Palace_Step_" + i, cidade, 68f, GroundTop + 0.15f + i * 0.3f,
                    21f - i * 2f, 0.3f, CorPedra, -1);
            }
        }

        // Moradores e soldados: presença, não interação.
        FiguraDoElenco("Villager_1", cidade, 18f, ChrMulher, CorNpc);
        FiguraDoElenco("Villager_2", cidade, 26f, ChrMercador, CorNpc, 1, true);
        FiguraDoElenco("Soldier_1", cidade, 30f, ChrSoldado, CorSoldado);
        FiguraDoElenco("Soldier_2", cidade, 32.5f, ChrSoldado, CorSoldado, 1, true);
        if (Prop("Spear_1", cidade, 30.6f, ItacaRaiz + "Arsenal/ithaca_spear_01.png", 2) == null)
        {
            Bloco("Spear_1", cidade, 30.6f, GroundTop + 1.2f, 0.12f, 2.4f, CorMadeira, 2);
            Bloco("Spear_2", cidade, 33.1f, GroundTop + 1.2f, 0.12f, 2.4f, CorMadeira, 2);
        }
        else
        {
            Prop("Spear_2", cidade, 33.1f, ItacaRaiz + "Arsenal/ithaca_spear_01.png", 2, GroundTop, true);
        }

        // Ato 1 — explorar quatro cantos da ilha.
        Ponto("Look_Town", cidade, 10f, "int.prologue.town", explorar, CorPedra, 1.2f, 1.2f,
            null, ItacaRaiz + "Props/ithaca_cart_01.png");
        Ponto("Look_People", cidade, 22f, "int.prologue.people", explorar, CorProp, 1.2f, 1f,
            null, ItacaRaiz + "Props/ithaca_bench_01.png");
        Ponto("Look_Soldiers", cidade, 31.5f, "int.prologue.soldiers", explorar, CorProp, 1.2f, 1f,
            null, ItacaSuporteDeArmas);
        Ponto("Look_Palace", cidade, 44f, "int.prologue.palace", explorar, CorPedra, 1.4f, 1.4f,
            null, ItacaRaiz + "Props/ithaca_amphora_01.png");

        Coletavel(16f, GroundTop + 0.9f);
        Coletavel(38f, GroundTop + 0.9f);

        // O arauto de Agamenon só chega depois de o jogador conhecer o reino.
        // O arauto é a presença de Agamenon na fase: Micenas em índigo e ouro, contra o linho e o
        // bronze de Ítaca. Agamenon mesmo não entra em cena.
        GameObject arauto = NpcDoElenco("NPC_Herald", cidade, 48f, falas["Herald"], convocacao, ChrArauto, new Color(0.85f, 0.75f, 0.35f));
        arauto.SetActive(false);
        WireArray(explorar, "enableOnComplete", arauto);

        // Ato 2 — dentro do salão.
        // Piso do salão: fiada de pedra ladrilhada. Era uma barra chapada de 20 unidades.
        if (Tile(ItacaTerrenoPedra, TileTopo) != null)
        {
            FaixaDeTiles("Hall_Floor", 70f, GroundTop, 20f, LadoDoTile, Tile(ItacaTerrenoPedra, TileTopo), -1);
        }
        else
        {
            Bloco("Hall_Floor", cidade, 70f, GroundTop + 0.05f, 20f, 0.1f, CorPedra, -1);
        }
        // A conversa com a família fecha o Ato 2 e abre o portão seguinte: começa
        // sozinha, para não existir a possibilidade de passar direto e travar.
        NpcDoElenco("NPC_Penelope", cidade, 66f, falas["Penelope"], familia, ChrPenelope, CorNpc,
            automatico: true);
        NpcDoElenco("NPC_Telemaco", cidade, 72f, falas["Telemaco"], familia, ChrTelemaco, CorNpc,
            automatico: true);
        if (Prop("Loom", cidade, 63f, ItacaTear, 0) == null)
        {
            Bloco("Loom", cidade, 63f, GroundTop + 1f, 1.6f, 2f, CorMadeira, 0);
        }

        Checkpoint(77f);
    }

    /// <summary>
    /// Casa de Ítaca. Continua sendo retângulo colorido — mas porta e janela bastam
    /// para o olho ler "casa" em vez de "caixote", e é o que separa um placeholder
    /// legível de um cenário que parece quebrado.
    /// </summary>
    private static void Casa(Transform pai, float x, float largura, float altura)
    {
        // A arte da casa tem tamanho próprio (4,34 x 3,06 un) e não é esticada para caber em
        // largura/altura: a porta dela foi dimensionada para o Odisseu passar, e escalar
        // desfaria justamente essa medida. Os parâmetros seguem valendo para o placeholder.
        if (Prop("House", pai, x, ItacaCasa, -2) != null)
        {
            return;
        }

        Bloco("House", pai, x, GroundTop + altura * 0.5f, largura, altura, CorCasa, -2);
        Bloco("House_Roof", pai, x, GroundTop + altura + 0.3f, largura + 0.5f, 0.6f, CorTelhado, -1);
        Bloco("House_Door", pai, x, GroundTop + 0.55f, 0.6f, 1.1f, CorMadeira, -1);
        Bloco("House_Window", pai, x + largura * 0.28f, GroundTop + altura * 0.68f,
            0.45f, 0.45f, CorJanela, -1);
    }

    // Ato 3 — recrutar os homens de Ítaca
    private static void MontarAto3(Dictionary<string, DialogueSequence> falas, LevelObjective recrutar)
    {
        Transform zona = new GameObject("Act3_Recruit").transform;
        zona.SetParent(world, false);

        NpcDoElenco("Recruit_Fisherman", zona, 86f, falas["Fisherman"], recrutar, ChrPescador, CorNpc);
        NpcDoElenco("Recruit_Farmer", zona, 92f, falas["Farmer"], recrutar, ChrLavrador, CorNpc);
        NpcDoElenco("Recruit_Sailor", zona, 98f, falas["Sailor"], recrutar, ChrMarinheiro, CorNpc);
        NpcDoElenco("Recruit_Blacksmith", zona, 105f, falas["Smith"], recrutar, ChrMercador, CorNpc);
        NpcDoElenco("Recruit_Eurylochus", zona, 118f, falas["Eurylochus"], recrutar, ChrSoldado, CorSoldado);
        NpcDoElenco("Recruit_Rower", zona, 124f, falas["Rower"], recrutar, ChrMarinheiroOcre, CorNpc);
        NpcDoElenco("Recruit_Shepherd", zona, 129f, falas["Shepherd"], recrutar, ChrLavrador, CorNpc);
        NpcDoElenco("Recruit_Watchman", zona, 140f, falas["Watchman"], recrutar, ChrSoldado, CorNpc);
        NpcDoElenco("Recruit_Carpenter", zona, 145f, falas["Carpenter"], recrutar, ChrArtesao, CorNpc);
        NpcDoElenco("Recruit_Elpenor", zona, 150f, falas["Elpenor"], recrutar, ChrMarinheiroOliva, CorNpc);

        // Cenário de trabalho da ilha.
        if (Prop("Nets", zona, 88f, ItacaRede, 0) == null)
        {
            Bloco("Nets", zona, 88f, GroundTop + 0.4f, 2f, 0.8f, CorProp, 0);
        }

        if (Prop("Field", zona, 94f, ItacaLavoura, -1) == null)
        {
            Bloco("Field", zona, 94f, GroundTop + 0.2f, 4f, 0.4f, new Color(0.6f, 0.7f, 0.35f), -1);
        }

        if (Prop("Forge", zona, 107f, ItacaForja, 0) == null)
        {
            Bloco("Forge", zona, 107f, GroundTop + 0.8f, 2f, 1.6f, new Color(0.4f, 0.35f, 0.35f), 0);
        }

        // Casco em manutenção na praia: barco de pesca escorado, 5,06 un. Não é o navio de
        // Odisseu — aquele tem 9,12 un, pertence ao cais e, posto aqui, contaria a história
        // errada: a expedição pronta antes de ser preparada.
        if (Prop("Boat_Hull", zona, 126f, ItacaCascoNaPraia, -1) == null)
        {
            Bloco("Boat_Hull", zona, 126f, GroundTop + 0.5f, 5f, 1f, CorMadeira, -1);
        }

        if (Prop("Watchtower", zona, 142f, ItacaTorre, -2) == null)
        {
            Bloco("Watchtower", zona, 142f, GroundTop + 2.5f, 1.6f, 5f, CorPedra, -2);
        }

        Vegetacao(zona, 80f, 152f, 26, 4471);

        Coletavel(112f, GroundTop + 0.9f);
        Coletavel(134f, GroundTop + 0.9f);

        Checkpoint(152f);
    }

    // Ato 4 — o treinamento, que é o tutorial de combate
    private static void MontarAto4(Dictionary<string, DialogueSequence> falas, LevelObjective treinar)
    {
        Transform campo = new GameObject("Act4_Training").transform;
        campo.SetParent(world, false);

        // A cerca do campo: com a arte, um muro baixo de pedra seca ladrilhado na linha do
        // chão. O placeholder era uma barra a 2,6 de altura, que contra um fundo chapado
        // passava por cerca e contra a paisagem pintada vira um traço solto no céu.
        if (!MuroBaixo("Training_Wall", campo, 155f, 239f))
        {
            Bloco("Training_Fence", campo, 197f, GroundTop + 2.6f, 84f, 0.2f, CorMadeira, -3);
        }

        if (Prop("Rack", campo, 162f, ItacaSuporteDeArmas, 0) == null)
        {
            Bloco("Rack", campo, 162f, GroundTop + 0.7f, 2.4f, 1.4f, CorMadeira, 0);
        }

        Vegetacao(campo, 156f, 238f, 18, 8823);

        // Soldados treinando ao fundo: a ilha inteira se preparando, não só o rei.
        for (int i = 0; i < 6; i++)
        {
            FiguraDoElenco("Soldier_Drill_" + i, campo, 166f + i * 2.4f, ChrSoldado, CorSoldado, -1, true);
        }

        var cursoGo = new GameObject("TrainingCourse");
        cursoGo.transform.SetParent(campo, false);
        var curso = cursoGo.AddComponent<TrainingCourse>();

        // Alvos e parceiros nascem desligados: cada etapa liga o seu.
        GameObject boneco1 = Alvo("Dummy_Sword_1", campo, 172f, curso, TrainingAction.SwordHit, CorBoneco, 1f, 2f, ItacaBoneco);
        GameObject boneco2 = Alvo("Dummy_Sword_2", campo, 176f, curso, TrainingAction.SwordHit, CorBoneco, 1f, 2f, ItacaBoneco);
        GameObject sparring = Sparring("Sparring_Partner", campo, 182f);
        GameObject alvo1 = Alvo("Target_Bow_1", campo, 216f, curso, TrainingAction.ArrowHit, CorAlvo, 1.2f, 2f, ItacaAlvo);
        GameObject alvo2 = Alvo("Target_Bow_2", campo, 220f, curso, TrainingAction.ArrowHit, CorAlvo, 1.2f, 2f, ItacaAlvo);
        GameObject alvo3 = Alvo("Target_Bow_3", campo, 224f, curso, TrainingAction.ArrowHit, CorAlvo, 1.2f, 2f, ItacaAlvo);
        GameObject alvoFinal = Alvo("Target_Combo", campo, 230f, curso, TrainingAction.ArrowHit, CorAlvo, 1.2f, 2f, ItacaAlvo);
        GameObject sparringFinal = Sparring("Sparring_Combo", campo, 234f);
        GameObject bonecoFinal = Alvo("Dummy_Combo", campo, 238f, curso, TrainingAction.SwordHit, CorBoneco, 1f, 2f, ItacaBoneco);

        boneco1.SetActive(false);
        boneco2.SetActive(false);
        sparring.SetActive(false);
        alvo1.SetActive(false);
        alvo2.SetActive(false);
        alvo3.SetActive(false);
        alvoFinal.SetActive(false);
        sparringFinal.SetActive(false);
        bonecoFinal.SetActive(false);

        Wire(curso, "combat", playerCombat);
        Wire(curso, "shield", playerShield);
        Wire(curso, "playerHealth", player.GetComponent<Odisseia.Core.HealthSystem>());
        Wire(curso, "bow", playerBow);
        Wire(curso, "inputActions", controls);
        Wire(curso, "prompt", prompt);
        Wire(curso, "objective", treinar);

        DefinirEtapas(curso, new[]
        {
            Etapa("tut.prologue.move", "Move", TrainingAction.Move, 2.5f),
            Etapa("tut.prologue.sword", "Attack", TrainingAction.Attack, 2f),
            Etapa("tut.prologue.swordDummy", "Attack", TrainingAction.SwordHit, 3f, new[] { boneco1, boneco2 }),
            Etapa("tut.prologue.shield", "Shield", TrainingAction.Block, 2f,
                new[] { sparring }, new[] { sparring }),
            Etapa("tut.prologue.jump", "Jump", TrainingAction.Jump, 2f),
            Etapa("tut.prologue.jumpGaps", "Jump", TrainingAction.Jump, 3f),
            Etapa("tut.prologue.bow", "Bow", TrainingAction.Bow, 1f),
            Etapa("tut.prologue.bowTargets", "Bow", TrainingAction.ArrowHit, 3f,
                new[] { alvo1, alvo2, alvo3 }),
            Etapa("tut.prologue.combo", "Attack", TrainingAction.SwordHit, 1f,
                new[] { alvoFinal, sparringFinal, bonecoFinal }),
        });

        // O treinador abre o ato; o curso liga junto com ele.
        // O instrutor era a arte do Alcínoo, rei feácio, de manto azul e 1,8 un.
        FiguraDoElenco("NPC_Trainer", campo, 160f, ChrInstrutor, CorSoldado);
        Gatilho("Trigger_TrainerIntro", campo, 158f, falas["TrainerIntro"]);

        WireArray(treinar, "enableOnStart", cursoGo);
        cursoGo.SetActive(false);

        Coletavel(194.5f, GroundTop + 3.2f);
        Checkpoint(156f);
        Checkpoint(202f);
        Checkpoint(237f);
    }

    private static TrainingCourse.Step Etapa(string chave, string acaoDica, TrainingAction acao,
        float vezes, GameObject[] ligar = null, GameObject[] desligar = null)
    {
        return new TrainingCourse.Step
        {
            instructionKey = chave,
            hintAction = acaoDica,
            action = acao,
            required = vezes,
            enableOnStart = ligar,
            disableOnComplete = desligar,
        };
    }

    /// <summary>
    /// Grava as etapas do curso. Vai por SerializedProperty porque o campo é privado —
    /// mexer nele por reflexão daria um valor que o Editor não persiste.
    /// </summary>
    private static void DefinirEtapas(TrainingCourse curso, TrainingCourse.Step[] etapas)
    {
        var so = new SerializedObject(curso);
        SerializedProperty lista = so.FindProperty("steps");
        lista.arraySize = etapas.Length;

        for (int i = 0; i < etapas.Length; i++)
        {
            SerializedProperty item = lista.GetArrayElementAtIndex(i);
            TrainingCourse.Step etapa = etapas[i];

            item.FindPropertyRelative("instructionKey").stringValue = etapa.instructionKey;
            item.FindPropertyRelative("hintAction").stringValue = etapa.hintAction;
            item.FindPropertyRelative("action").enumValueIndex = (int)etapa.action;
            item.FindPropertyRelative("required").floatValue = etapa.required;

            SerializedProperty ligar = item.FindPropertyRelative("enableOnStart");
            int quantos = etapa.enableOnStart != null ? etapa.enableOnStart.Length : 0;
            ligar.arraySize = quantos;
            for (int j = 0; j < quantos; j++)
            {
                ligar.GetArrayElementAtIndex(j).objectReferenceValue = etapa.enableOnStart[j];
            }

            SerializedProperty desligar = item.FindPropertyRelative("disableOnComplete");
            int quantosFora = etapa.disableOnComplete != null ? etapa.disableOnComplete.Length : 0;
            desligar.arraySize = quantosFora;
            for (int j = 0; j < quantosFora; j++)
            {
                desligar.GetArrayElementAtIndex(j).objectReferenceValue = etapa.disableOnComplete[j];
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Ato 5 — armas, equipamentos e navios
    private static void MontarAto5(Dictionary<string, DialogueSequence> falas,
        LevelObjective equipar, LevelObjective navios)
    {
        Transform arsenal = new GameObject("Act5_Arsenal").transform;
        arsenal.SetParent(world, false);

        // Fachada do armazém: 17,24 x 3,31 un contra os 20 x 4 do bloco que substitui. Ela é
        // pano de fundo — nenhum portão entra no armazém, e as bancadas de preparação ficam à
        // frente dela —, então as portas desenhadas são decorativas e não precisam passar no
        // teste da seção 25 como as da casa e do palácio precisam.
        if (Prop("Armory", arsenal, 256f, ItacaArmazem, -2) == null)
        {
            Bloco("Armory", arsenal, 256f, GroundTop + 2f, 20f, 4f, CorCasa, -2);
            Bloco("Armory_Roof", arsenal, 256f, GroundTop + 4.3f, 21f, 0.6f, CorTelhado, -1);
        }
        if (Prop("Anvil", arsenal, 246f, ItacaBigorna, 0) == null)
        {
            Bloco("Anvil", arsenal, 246f, GroundTop + 0.5f, 1.4f, 1f, new Color(0.35f, 0.35f, 0.38f), 0);
        }
        Prop("Arsenal_Rack", arsenal, 271f, ItacaSuporteDeArmas, 0);
        Prop("Arsenal_Spear", arsenal, 240f, ItacaRaiz + "Arsenal/ithaca_spear_01.png", 0);
        Prop("Arsenal_Shield", arsenal, 242f, ItacaRaiz + "Arsenal/ithaca_shield_round_01.png", 0);

        NpcDoElenco("NPC_Blacksmith", arsenal, 244f, falas["Blacksmith"], equipar, ChrMercador, CorSoldado);
        Ponto("Prep_Swords", arsenal, 250f, "int.prologue.swords", equipar, CorPedra, 1.4f, 1.2f,
            null, ItacaRaiz + "Arsenal/ithaca_sword_01.png");
        Ponto("Prep_Shields", arsenal, 256f, "int.prologue.shields", equipar, CorPedra, 1.6f, 1.4f,
            null, ItacaRaiz + "Arsenal/ithaca_shield_round_01.png");
        Ponto("Prep_Arrows", arsenal, 262f, "int.prologue.arrows", equipar, CorMadeira, 1.4f, 1.4f,
            null, ItacaRaiz + "Arsenal/ithaca_arrows_bundle_01.png");
        Ponto("Prep_Armor", arsenal, 268f, "int.prologue.armor", equipar, CorProp, 1.6f, 1.2f,
            null, ItacaRaiz + "Props/ithaca_crate_01.png");

        Transform porto = new GameObject("Act5_Port").transform;
        porto.SetParent(world, false);

        // Uma pequena frota, para a partida ter escala. Atracada: vela enrolada na verga,
        // porque a expedição ainda está sendo preparada — só o navio de Odisseu iça vela, e
        // só no fim da fase.
        for (int i = 0; i < 4; i++)
        {
            float x = 288f + i * 12f;
            // Alternar o lado quebra a repetição de quatro cascos idênticos em fila.
            if (Prop("Ship_Hull_" + i, porto, x, ItacaNavioAtracado, -4, GroundTop - 1.2f, i % 2 == 1) != null)
            {
                continue;
            }

            Bloco("Ship_Hull_" + i, porto, x, GroundTop - 0.6f, 9f, 1.6f, CorMadeira, -4);
            Bloco("Ship_Mast_" + i, porto, x, GroundTop + 2.4f, 0.3f, 6f, CorMadeira, -4);
            Bloco("Ship_Sail_" + i, porto, x + 1.4f, GroundTop + 3f, 3f, 3.4f, CorVela, -5);
        }

        Ponto("Port_Supplies", porto, 282f, "int.prologue.supplies", navios, CorProp, 1.4f, 1.4f,
            null, ItacaRaiz + "Props/ithaca_basket_01.png");
        // Não há remo no pacote; a lança é o cabo de madeira longo mais próximo.
        Ponto("Port_Oars", porto, 292f, "int.prologue.oars", navios, CorMadeira, 1.8f, 0.8f,
            null, ItacaRaiz + "Arsenal/ithaca_spear_01.png");
        // Vela avulsa também não existe; o rolo de corda lê como cordame preparado para o velame.
        Ponto("Port_Sails", porto, 304f, "int.prologue.sails", navios, CorVela, 1.6f, 1.4f,
            null, ItacaRaiz + "Props/ithaca_rope_coil_01.png");
        // Tripulação são pessoas, e não há sprite disso; o banco marca o ponto onde ela se reúne.
        Ponto("Port_Crew", porto, 316f, "int.prologue.crew", navios, CorNpc, 1.2f, 1.6f,
            null, ItacaRaiz + "Props/ithaca_bench_01.png");

        FiguraDoElenco("Sailor_1", porto, 296f, ChrMarinheiro, CorNpc);
        FiguraDoElenco("Sailor_2", porto, 300f, ChrMarinheiroOcre, CorNpc, 1, true);
        FiguraDoElenco("Sailor_3", porto, 312f, ChrMarinheiroOliva, CorNpc);
        if (Prop("Barrels", porto, 286f, ItacaRaiz + "Props/ithaca_barrel_01.png", 0) == null)
        {
            Bloco("Barrels", porto, 286f, GroundTop + 0.5f, 1.2f, 1f, CorProp, 0);
        }

        if (Prop("Ropes", porto, 308f, ItacaRaiz + "Props/ithaca_rope_coil_01.png", 0) == null)
        {
            Bloco("Ropes", porto, 308f, GroundTop + 0.3f, 1.6f, 0.6f, CorProp, 0);
        }

        // Postes de amarração ao longo do cais.
        for (int i = 0; i < 5; i++)
        {
            Prop("Dock_Post_" + i, porto, 284f + i * 9f, ItacaPosteDoCais, 0);
        }

        Prop("Port_Net", porto, 298f, ItacaRede, 0);
        Prop("Port_Anchor", porto, 318f, ItacaRaiz + "Port/ithaca_anchor_01.png", 0);

        Checkpoint(279f);
        Checkpoint(320f);
    }

    // Ato 6 — a despedida e a partida
    private static void MontarAto6(Dictionary<string, DialogueSequence> falas,
        LevelObjective navios, LevelObjective despedida, LevelObjective partir)
    {
        Transform cais = new GameObject("Act6_Departure").transform;
        cais.SetParent(world, false);

        // Penélope e Telêmaco vão até o cais: é lá que a despedida acontece e é de lá
        // que ela assiste à partida — voltar ao palácio seria refazer a fase inteira a pé.
        GameObject penelope = NpcDoElenco("NPC_Penelope_Port", cais, 326f, falas["FarewellPenelope"],
            despedida, ChrPenelope, CorNpc, automatico: true);
        GameObject telemaco = NpcDoElenco("NPC_Telemaco_Port", cais, 330f, falas["FarewellTelemaco"],
            despedida, ChrTelemaco, CorNpc, automatico: true);

        penelope.SetActive(false);
        telemaco.SetActive(false);
        WireArray(navios, "enableOnComplete", penelope, telemaco);

        // O navio de Odisseu, que zarpa no fim.
        var navio = new GameObject("Ship_Odysseus");
        navio.transform.SetParent(cais, false);
        navio.transform.position = new Vector3(340f, 0f, 0f);

        // Altura do convés acima da base do sprite, publicada pelo pipeline de arte em
        // ithaca_ship_01.json: 257 px de altura menos 190 px até a linha da borda, a 42,857
        // px por unidade. Adivinhar essa linha põe a silhueta flutuando ou enterrada, e nada
        // no jogo acusa — a única pista seria olhar.
        const float ConvesAcimaDaBase = (257f - 190f) / 42.857143f;
        const float LinhaDoConves = GroundTop + 0.2f;

        float xDoConves = 340f;
        if (Prop("Hull", navio.transform, 340f, ItacaNavio, -3, LinhaDoConves - ConvesAcimaDaBase) == null)
        {
            Bloco("Hull", navio.transform, 340f, GroundTop - 0.4f, 11f, 2f, CorMadeira, -3);
            Bloco("Mast", navio.transform, 340f, GroundTop + 3f, 0.35f, 7f, CorMadeira, -3);
            Bloco("Sail", navio.transform, 341.6f, GroundTop + 3.6f, 3.6f, 4f, CorVela, -4);
            xDoConves = 337f;
        }

        // A silhueta do convés é o próprio Odisseu parado — é ele que embarca. Vinha como
        // retângulo dourado, o que só passava despercebido enquanto o resto também era bloco.
        Sprite odisseuParado = SpriteDaFolha("Assets/Resources/Odisseia/Characters/CHR_Odysseus.png",
            "CHR_Odysseus_Idle_00");
        GameObject conves = odisseuParado != null
            ? Figura("Odysseus_OnDeck", navio.transform, xDoConves, 0.8f, odisseuParado.bounds.size.y,
                Color.white, 2, odisseuParado)
            : Figura("Odysseus_OnDeck", navio.transform, xDoConves, 0.8f, 1.5f, new Color(0.9f, 0.8f, 0.4f), 2);
        conves.transform.position = new Vector3(conves.transform.position.x, LinhaDoConves, conves.transform.position.z);
        conves.SetActive(false);

        var embarque = new GameObject("Trigger_Boarding");
        embarque.transform.SetParent(cais, false);
        embarque.transform.position = new Vector3(337f, GroundTop + 1.5f, 0f);
        var caixa = embarque.AddComponent<BoxCollider2D>();
        caixa.size = new Vector2(3f, 4f);
        caixa.isTrigger = true;

        var partida = embarque.AddComponent<ShipDeparture>();
        Wire(partida, "player", player);
        Wire(partida, "playerLock", playerLock);
        Wire(partida, "ship", navio.transform);
        Wire(partida, "odysseusOnDeck", conves);
        Wire(partida, "cameraFollow", cameraFollow);
        Wire(partida, "boardingDialogue", falas["Boarding"]);
        Wire(partida, "goal", goal);

        embarque.SetActive(false);
        WireArray(despedida, "enableOnComplete", embarque);
    }

    // ------------------------------------------------------------------ serialização

    private static void Wire(Object alvo, string campo, object valor)
    {
        if (alvo == null)
        {
            return;
        }

        var so = new SerializedObject(alvo);
        SerializedProperty prop = so.FindProperty(campo);

        if (prop == null)
        {
            Debug.LogError("[Prologue] campo '" + campo + "' não existe em " + alvo.GetType().Name);
            return;
        }

        switch (prop.propertyType)
        {
            case SerializedPropertyType.ObjectReference:
                prop.objectReferenceValue = valor as Object;
                break;
            case SerializedPropertyType.String:
                prop.stringValue = (string)valor;
                break;
            case SerializedPropertyType.Integer:
                prop.intValue = System.Convert.ToInt32(valor);
                break;
            case SerializedPropertyType.Float:
                prop.floatValue = System.Convert.ToSingle(valor);
                break;
            case SerializedPropertyType.Boolean:
                prop.boolValue = (bool)valor;
                break;
            case SerializedPropertyType.Enum:
                prop.enumValueIndex = System.Convert.ToInt32(valor);
                break;
            default:
                Debug.LogError("[Prologue] tipo não tratado em " + campo);
                break;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WireArray(Object alvo, string campo, params GameObject[] valores)
    {
        var so = new SerializedObject(alvo);
        SerializedProperty prop = so.FindProperty(campo);

        if (prop == null || !prop.isArray)
        {
            Debug.LogError("[Prologue] array '" + campo + "' não existe em " + alvo.GetType().Name);
            return;
        }

        prop.arraySize = valores.Length;
        for (int i = 0; i < valores.Length; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
