using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Mede, na própria arte do menu, onde estão as placas de botão pintadas.
///
/// A arte entregue é um mockup: logo e botões vêm desenhados dentro da imagem. Para os
/// botões de verdade caírem exatamente em cima dos pintados — em qualquer resolução —
/// eles precisam ser ancorados em coordenadas normalizadas da imagem, e essas
/// coordenadas têm que ser medidas, não estimadas no olho.
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod MainMenuLayoutProbe.Measure
/// </summary>
public static class MainMenuLayoutProbe
{
    private const string ArtPath = "Assets/Art/Odisseia/UI/MainMenu/BG_MainMenu.png";

    [MenuItem("Odisseia/Medir placas do menu")]
    public static void Measure()
    {
        // Leitura de pixels exige o import marcado como legível.
        var importer = (TextureImporter)AssetImporter.GetAtPath(ArtPath);
        bool eraLegivel = importer.isReadable;
        if (!eraLegivel)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath);
        if (texture == null)
        {
            Debug.LogError("[LayoutProbe] arte não encontrada em " + ArtPath);
            EditorApplication.Exit(1);
            return;
        }

        int w = texture.width;
        int h = texture.height;
        Color32[] pixels = texture.GetPixels32();

        Debug.Log($"[LayoutProbe] {w}x{h}  proporção {(w / (float)h):0.000}");

        // Mapa em texto da imagem inteira. Heurística de cor sozinha erra numa arte
        // pintada e com textura; ver o desenho resolve em um olhar.
        //   #  placa de botão (azul saturado)     +  dourado (moldura, logo)
        //   .  claro (céu, mar)                   :  escuro
        const int colunas = 78;
        int linhas = Mathf.RoundToInt(colunas * h / (float)w * 0.5f);

        var mapa = new System.Text.StringBuilder("\n");
        for (int linha = linhas - 1; linha >= 0; linha--)
        {
            for (int coluna = 0; coluna < colunas; coluna++)
            {
                int px = Mathf.Clamp(Mathf.RoundToInt((coluna + 0.5f) * w / colunas), 0, w - 1);
                int py = Mathf.Clamp(Mathf.RoundToInt((linha + 0.5f) * h / linhas), 0, h - 1);
                Color32 c = pixels[py * w + px];

                bool azul = c.b > c.r + 20 && c.b > 55 && c.r < 120;
                bool dourado = c.r > 130 && c.g > 100 && c.r > c.b + 45;
                int brilho = (c.r + c.g + c.b) / 3;

                mapa.Append(dourado ? '+' : azul ? '#' : brilho > 120 ? '.' : ':');
            }

            mapa.Append('\n');
        }

        Debug.Log("[LayoutProbe] mapa (topo = topo da imagem)" + mapa);

        // A placa é azul escuro saturado: azul claramente acima de vermelho.
        bool EhPlaca(Color32 c) => c.b > c.r + 20 && c.b > 60 && c.b < 170 && c.r < 110;

        // Só a coluna central, onde os botões vivem.
        int x0 = Mathf.RoundToInt(w * 0.34f);
        int x1 = Mathf.RoundToInt(w * 0.68f);
        int minimo = Mathf.RoundToInt((x1 - x0) * 0.70f);

        var faixas = new List<Vector2Int>();
        int inicio = -1;

        // GetPixels32 vem de baixo para cima; percorro na mesma ordem do Unity.
        for (int y = 0; y < h; y++)
        {
            int n = 0;
            for (int x = x0; x < x1; x++)
            {
                if (EhPlaca(pixels[y * w + x]))
                {
                    n++;
                }
            }

            if (n >= minimo && inicio < 0)
            {
                inicio = y;
            }
            else if (n < minimo && inicio >= 0)
            {
                if (y - inicio > h * 0.03f)
                {
                    faixas.Add(new Vector2Int(inicio, y));
                }

                inicio = -1;
            }
        }

        Debug.Log($"[LayoutProbe] faixas encontradas: {faixas.Count}");

        foreach (Vector2Int faixa in faixas)
        {
            int meio = (faixa.x + faixa.y) / 2;
            int esq = -1;
            int dir = -1;

            for (int x = 0; x < w; x++)
            {
                if (!EhPlaca(pixels[meio * w + x]))
                {
                    continue;
                }

                // Ignora azul do mar longe do centro.
                if (x < w * 0.25f || x > w * 0.78f)
                {
                    continue;
                }

                if (esq < 0)
                {
                    esq = x;
                }

                dir = x;
            }

            Debug.Log($"[LayoutProbe] placa  yMin={faixa.x / (float)h:0.0000} yMax={faixa.y / (float)h:0.0000}" +
                      $"  xMin={esq / (float)w:0.0000} xMax={dir / (float)w:0.0000}" +
                      $"  (px y {faixa.x}..{faixa.y}, x {esq}..{dir})");
        }

        if (!eraLegivel)
        {
            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        EditorApplication.Exit(0);
    }
}
