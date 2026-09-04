using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Confere que a arte do cenário de Ítaca importou com as configurações que o jogo exige:
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod IthacaEnvironmentProbe.Run
///
/// O que torna esta checagem necessária: um erro de import não vira erro em lugar nenhum.
/// Uma textura que entrou com os defaults do Editor — 100 px por unidade, filtro bilinear,
/// compressão ligada — carrega, renderiza e roda. Ela só fica com o tamanho errado em
/// relação ao personagem e borrada de perto, e isso é indistinguível de "a arte é assim".
///
/// Os 42,857 px por unidade não são preferência: vêm do corpo de 60 px do Odisseu valendo
/// 1,4 unidades. Uma casa importada a 100 px/un mediria 1,3 un em vez de 3,06 — o jogador
/// não passaria pela porta, e o sintoma apareceria como problema de level design.
///
/// A checagem dos tilesets é sobre o indice: o <c>ithaca_tiles_&lt;kit&gt;_NN</c> tem que
/// corresponder à posição row-major a partir do TOPO da folha, que é a ordem do array no
/// JSON do PixelLab. O campo <c>original_position</c> do JSON aponta para outra grade; quem
/// seguir ele monta o terreno com os tiles trocados.
/// </summary>
public static class IthacaEnvironmentProbe
{
    private const string Raiz = "Assets/Art/Environments/Ithaca";

    /// <summary>Herdado do personagem. Ver ODYSSEUS_MASTER_REFERENCE.</summary>
    private const float PixelsPorUnidade = 42.857143f;

    /// <summary>Tolerância de comparação do PPU — o valor é dízima e não fecha exato.</summary>
    private const float ToleranciaPpu = 0.001f;

    private const int TileTerreno = 32;
    private const int TilesPorKit = 16;

    /// <summary>Pastas que repetem lateralmente e por isso precisam de wrap Repeat.</summary>
    private static readonly HashSet<string> Repetem = new HashSet<string>
    {
        "Background/ithaca_bg_clouds", "Background/ithaca_bg_island_far",
        "Background/ithaca_bg_mountains_far", "Background/ithaca_bg_ocean",
        "Background/ithaca_bg_sky", "Architecture/ithaca_wall_low_01",
    };

    [MenuItem("Odisseia/Conferir arte de Itaca")]
    public static void Conferir()
    {
        Executar();
    }

    public static void Run()
    {
        bool falhou = !Executar();
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(falhou ? 1 : 0);
        }
    }

    private static bool Executar()
    {
        if (!Directory.Exists(Raiz))
        {
            Debug.LogError($"[Itaca] pasta nao encontrada: {Raiz} — rode 'node Tools/unity-import.js'");
            return false;
        }

        string[] texturas = AssetDatabase
            .FindAssets("t:Texture2D", new[] { Raiz })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct()
            .OrderBy(caminho => caminho)
            .ToArray();

        if (texturas.Length == 0)
        {
            Debug.LogError($"[Itaca] nenhuma textura em {Raiz}");
            return false;
        }

        bool ok = true;
        int fatiados = 0;

        foreach (string caminho in texturas)
        {
            var importador = AssetImporter.GetAtPath(caminho) as TextureImporter;
            if (importador == null)
            {
                Debug.LogError($"[Itaca] {caminho} nao tem TextureImporter");
                ok = false;
                continue;
            }

            string relativo = caminho.Substring(Raiz.Length + 1).Replace(".png", string.Empty);
            ok &= ConferirImport(relativo, importador);

            if (importador.spriteImportMode == SpriteImportMode.Multiple)
            {
                ok &= ConferirTileset(caminho, relativo);
                fatiados++;
            }
            else
            {
                RelatarTamanho(caminho, relativo);
            }
        }

        ok &= ConferirConvesDoNavio();

        Debug.Log($"[Itaca] {texturas.Length} texturas conferidas, {fatiados} tilesets fatiados");
        Debug.Log(ok
            ? "[Itaca] import OK — escala, filtro e pivo conferem com o personagem"
            : "[Itaca] import REPROVADO — ver os erros acima");
        return ok;
    }

    private static bool ConferirImport(string relativo, TextureImporter importador)
    {
        bool ok = true;

        if (importador.textureType != TextureImporterType.Sprite)
        {
            Debug.LogError($"[Itaca] {relativo}: textureType {importador.textureType}, esperado Sprite");
            ok = false;
        }

        if (Mathf.Abs(importador.spritePixelsPerUnit - PixelsPorUnidade) > ToleranciaPpu)
        {
            Debug.LogError(
                $"[Itaca] {relativo}: {importador.spritePixelsPerUnit.ToString("0.######", CultureInfo.InvariantCulture)} px por unidade, " +
                $"esperado {PixelsPorUnidade.ToString("0.######", CultureInfo.InvariantCulture)} — o asset sai com o tamanho errado ao lado do Odisseu");
            ok = false;
        }

        // Filtro bilinear em pixel art borra a arte inteira. É o default do Editor.
        if (importador.filterMode != FilterMode.Point)
        {
            Debug.LogError($"[Itaca] {relativo}: filtro {importador.filterMode}, esperado Point");
            ok = false;
        }

        // Compressão inventa cor e quebra a paleta fechada de 35 cores.
        if (importador.textureCompression != TextureImporterCompression.Uncompressed)
        {
            Debug.LogError($"[Itaca] {relativo}: compressao {importador.textureCompression}, esperado Uncompressed");
            ok = false;
        }

        if (!importador.alphaIsTransparency)
        {
            Debug.LogError($"[Itaca] {relativo}: alphaIsTransparency desligado — halo escuro nas bordas");
            ok = false;
        }

        if (importador.mipmapEnabled)
        {
            Debug.LogError($"[Itaca] {relativo}: mipmaps ligados — some com o detalhe de longe");
            ok = false;
        }

        TextureWrapMode esperadoWrap = Repetem.Contains(relativo) ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        if (importador.wrapMode != esperadoWrap)
        {
            Debug.LogError($"[Itaca] {relativo}: wrap {importador.wrapMode}, esperado {esperadoWrap}");
            ok = false;
        }

        var settings = new TextureImporterSettings();
        importador.ReadTextureSettings(settings);

        // Terreno vai para Tilemap, que espera pivô central. Todo o resto apoia na base, para
        // assentar na linha do chão do mesmo jeito que o personagem — a mesma regra que o
        // PrologueProbe.ConferirApoio() cobra das figuras em pé.
        bool ehTerreno = relativo.StartsWith("Terrain/");
        var esperadoAlinhamento = ehTerreno ? SpriteAlignment.Center : SpriteAlignment.BottomCenter;
        if ((SpriteAlignment)settings.spriteAlignment != esperadoAlinhamento)
        {
            Debug.LogError(
                $"[Itaca] {relativo}: pivo {(SpriteAlignment)settings.spriteAlignment}, esperado {esperadoAlinhamento}");
            ok = false;
        }

        return ok;
    }

    private static bool ConferirTileset(string caminho, string relativo)
    {
        string kit = Path.GetFileNameWithoutExtension(caminho);

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<Sprite>().ToArray();
        if (sprites.Length != TilesPorKit)
        {
            Debug.LogError($"[Itaca] {relativo}: {sprites.Length} tiles fatiados, esperado {TilesPorKit}");
            return false;
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
        int colunas = texture.width / TileTerreno;
        bool ok = true;

        foreach (Sprite sprite in sprites)
        {
            string sufixo = sprite.name.Substring(sprite.name.LastIndexOf('_') + 1);
            if (!sprite.name.StartsWith(kit + "_") || !int.TryParse(sufixo, out int indice))
            {
                Debug.LogError($"[Itaca] {relativo}: sprite '{sprite.name}' fora do padrao {kit}_NN");
                ok = false;
                continue;
            }

            if (sprite.rect.width != TileTerreno || sprite.rect.height != TileTerreno)
            {
                Debug.LogError($"[Itaca] {sprite.name}: {sprite.rect.width}x{sprite.rect.height}, esperado {TileTerreno}x{TileTerreno}");
                ok = false;
                continue;
            }

            // O rect do Unity tem origem embaixo à esquerda; o indice documentado é row-major
            // a partir do topo. Conferir a conversão é o que impede o terreno trocado.
            int esperadoX = (indice % colunas) * TileTerreno;
            int esperadoY = texture.height - (indice / colunas + 1) * TileTerreno;
            if ((int)sprite.rect.x != esperadoX || (int)sprite.rect.y != esperadoY)
            {
                Debug.LogError(
                    $"[Itaca] {sprite.name}: rect ({sprite.rect.x},{sprite.rect.y}), " +
                    $"esperado ({esperadoX},{esperadoY}) para o indice {indice} row-major a partir do topo");
                ok = false;
            }
        }

        if (ok)
        {
            Debug.Log($"[Itaca] {relativo}: {TilesPorKit} tiles de {TileTerreno}px, indices row-major conferem");
        }

        return ok;
    }

    private static void RelatarTamanho(string caminho, string relativo)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (sprite == null)
        {
            return;
        }

        Vector2 unidades = sprite.bounds.size;
        Debug.Log($"[Itaca] {relativo}: {unidades.x:0.00} x {unidades.y:0.00} un");
    }

    /// <summary>
    /// A linha do convés é publicada pelo pipeline de arte em <c>ithaca_ship_01.json</c> —
    /// é onde o <c>ShipDeparture</c> apoia a silhueta do jogador. Se ela cair fora do sprite,
    /// a silhueta embarca flutuando ou enterrada, e nada acusa.
    /// </summary>
    private static bool ConferirConvesDoNavio()
    {
        const string json = Raiz + "/Ships/ithaca_ship_01.json";
        const string png = Raiz + "/Ships/ithaca_ship_01.png";

        var texto = AssetDatabase.LoadAssetAtPath<TextAsset>(json);
        var textura = AssetDatabase.LoadAssetAtPath<Texture2D>(png);
        if (texto == null || textura == null)
        {
            Debug.LogError("[Itaca] navio: falta ithaca_ship_01.png ou .json");
            return false;
        }

        var dados = JsonUtility.FromJson<NavioMeta>(texto.text);
        if (dados == null || dados.deck == null)
        {
            Debug.LogError("[Itaca] navio: JSON sem a linha do conves");
            return false;
        }

        bool dentro = dados.deck.x >= 0 && dados.deck.x < textura.width
                      && dados.deck.y >= 0 && dados.deck.y < textura.height;
        if (!dentro)
        {
            Debug.LogError(
                $"[Itaca] navio: conves ({dados.deck.x},{dados.deck.y}) fora do sprite {textura.width}x{textura.height}");
            return false;
        }

        // O JSON conta o y de cima para baixo, como a imagem; o mundo conta de baixo para cima.
        float alturaDoConves = (textura.height - dados.deck.y) / PixelsPorUnidade;
        Debug.Log($"[Itaca] navio: conves a {alturaDoConves:0.00} un acima da quilha, em ({dados.deck.x},{dados.deck.y})");
        return true;
    }

    [System.Serializable]
    private class NavioMeta
    {
        public Ponto deck;
    }

    [System.Serializable]
    private class Ponto
    {
        public int x;
        public int y;
    }
}
