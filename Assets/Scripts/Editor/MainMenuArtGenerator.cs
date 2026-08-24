using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Assa o background pixel art do menu principal como PNG.
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod MainMenuArtGenerator.Generate
/// ou pelo menu Odisseia > Gerar arte do menu principal.
///
/// Mesma abordagem do resto da arte placeholder do projeto: nada de asset de terceiros,
/// nada de licença. 640x360 é o tamanho nativo — escala inteira para 1280x720 (x2) e
/// 1920x1080 (x3), então o pixel nunca sai borrado nas resoluções que o jogo usa.
///
/// Escreve em BG_MainMenu_Placeholder.png de proposito: a arte definitiva mora em
/// BG_MainMenu.png e nao pode ser sobrescrita por uma regeneracao acidental.
///
/// A composição segue a referência: Odisseu à esquerda, ruínas à direita, mar e
/// pôr do sol ao centro, borda de meandro grego embaixo — e o miolo deliberadamente
/// limpo e escurecido, porque é onde os botões vão ficar.
/// </summary>
public static class MainMenuArtGenerator
{
    private const int Width = 640;
    private const int Height = 360;
    private const string OutputPath = "Assets/Art/Odisseia/UI/MainMenu/BG_MainMenu_Placeholder.png";

    // Paleta: azul profundo, azul petróleo, dourado envelhecido, bronze, pedra.
    private static readonly Color32 SkyTop = new Color32(28, 42, 74, 255);
    private static readonly Color32 SkyMid = new Color32(74, 96, 130, 255);
    private static readonly Color32 SkyWarm = new Color32(198, 150, 106, 255);
    private static readonly Color32 SkyHorizon = new Color32(238, 200, 142, 255);
    private static readonly Color32 SeaFar = new Color32(96, 122, 148, 255);
    private static readonly Color32 SeaNear = new Color32(26, 44, 70, 255);
    private static readonly Color32 SunCore = new Color32(252, 236, 190, 255);
    private static readonly Color32 Stone = new Color32(206, 196, 174, 255);
    private static readonly Color32 StoneShade = new Color32(150, 140, 124, 255);
    private static readonly Color32 StoneDark = new Color32(96, 90, 82, 255);
    private static readonly Color32 Foliage = new Color32(38, 58, 50, 255);
    private static readonly Color32 FoliageDark = new Color32(24, 38, 36, 255);
    private static readonly Color32 Gold = new Color32(198, 158, 84, 255);
    private static readonly Color32 Bronze = new Color32(132, 96, 52, 255);
    private static readonly Color32 CloakBlue = new Color32(46, 72, 118, 255);
    private static readonly Color32 Tunic = new Color32(214, 206, 186, 255);
    private static readonly Color32 Skin = new Color32(176, 132, 100, 255);

    private const int Horizon = 196;

    private static Color32[] pixels;

    [MenuItem("Odisseia/Gerar arte do menu principal")]
    public static void Generate()
    {
        pixels = new Color32[Width * Height];

        PaintSky();
        PaintClouds();
        PaintSun();
        PaintSea();
        PaintDistantIslands();
        PaintShips();
        PaintRuins();
        PaintCliffAndTree();
        PaintOdysseus();
        PaintMeanderBorder();
        DimCenter();

        Write();
    }

    // ------------------------------------------------------------------ utilidades

    private static void Set(int x, int y, Color32 c)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        pixels[y * Width + x] = c;
    }

    private static Color32 Get(int x, int y)
    {
        x = Mathf.Clamp(x, 0, Width - 1);
        y = Mathf.Clamp(y, 0, Height - 1);
        return pixels[y * Width + x];
    }

    private static void Rect(int x0, int y0, int w, int h, Color32 c)
    {
        for (int y = y0; y < y0 + h; y++)
        {
            for (int x = x0; x < x0 + w; x++)
            {
                Set(x, y, c);
            }
        }
    }

    private static Color32 Mix(Color32 a, Color32 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Color32(
            (byte)Mathf.RoundToInt(a.r + (b.r - a.r) * t),
            (byte)Mathf.RoundToInt(a.g + (b.g - a.g) * t),
            (byte)Mathf.RoundToInt(a.b + (b.b - a.b) * t),
            255);
    }

    private static Color32 Darken(Color32 c, float amount)
    {
        return Mix(c, new Color32(10, 14, 24, 255), amount);
    }

    /// <summary>Ruído estável — a arte tem que sair idêntica a cada geração.</summary>
    private static float Noise(int x, int y, int seed)
    {
        int h = x * 374761393 + y * 668265263 + seed * 1442695040;
        h = (h ^ (h >> 13)) * 1274126177;
        return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
    }

    // ------------------------------------------------------------------ céu e mar

    private static void PaintSky()
    {
        for (int y = Horizon; y < Height; y++)
        {
            float t = (y - Horizon) / (float)(Height - Horizon);
            Color32 c = t < 0.35f
                ? Mix(SkyHorizon, SkyWarm, t / 0.35f)
                : t < 0.7f
                    ? Mix(SkyWarm, SkyMid, (t - 0.35f) / 0.35f)
                    : Mix(SkyMid, SkyTop, (t - 0.7f) / 0.3f);

            for (int x = 0; x < Width; x++)
            {
                Set(x, y, c);
            }
        }
    }

    private static void PaintClouds()
    {
        // Faixas horizontais alongadas com dithering nas bordas: nuvem de pixel art
        // sem precisar de blur.
        for (int banda = 0; banda < 9; banda++)
        {
            int cy = Horizon + 14 + banda * 17 + (int)(Noise(banda, 3, 11) * 8);
            int cx = (int)(Noise(banda, 7, 23) * Width);
            int largura = 70 + (int)(Noise(banda, 11, 31) * 150);
            int altura = 3 + (int)(Noise(banda, 13, 41) * 5);

            float calor = Mathf.Clamp01(1f - (cy - Horizon) / 120f);
            Color32 corpo = Mix(new Color32(120, 134, 158, 255), new Color32(236, 196, 158, 255), calor);

            for (int y = cy; y < cy + altura; y++)
            {
                for (int x = cx - largura / 2; x < cx + largura / 2; x++)
                {
                    int xx = ((x % Width) + Width) % Width;
                    float borda = Mathf.Abs(x - cx) / (largura * 0.5f);
                    if (borda > 0.75f && Noise(xx, y, 53) > 1f - (borda - 0.75f) * 3.4f)
                    {
                        continue;
                    }

                    Set(xx, y, Mix(Get(xx, y), corpo, 0.55f));
                }
            }
        }
    }

    private static void PaintSun()
    {
        const int cx = 300;
        const int cy = Horizon + 12;
        const int raio = 15;

        for (int y = cy - raio; y <= cy + raio; y++)
        {
            for (int x = cx - raio; x <= cx + raio; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (d > raio)
                {
                    // Halo suave em degraus, sem gradiente contínuo.
                    if (d < raio + 10)
                    {
                        float t = 1f - (d - raio) / 10f;
                        Set(x, y, Mix(Get(x, y), SunCore, t * 0.35f));
                    }
                    continue;
                }

                Set(x, y, y < cy ? Mix(SunCore, SkyHorizon, 0.3f) : SunCore);
            }
        }
    }

    private static void PaintSea()
    {
        for (int y = 0; y < Horizon; y++)
        {
            float t = y / (float)Horizon;
            Color32 c = Mix(SeaNear, SeaFar, t * t);

            for (int x = 0; x < Width; x++)
            {
                Set(x, y, c);
            }
        }

        // Reflexo do sol: coluna de brilho quebrada, mais larga perto da câmera.
        for (int y = 0; y < Horizon; y++)
        {
            float prof = 1f - y / (float)Horizon;
            int meia = 2 + (int)(prof * 16f);
            for (int x = 300 - meia; x <= 300 + meia; x++)
            {
                if (Noise(x, y, 71) > 0.42f + prof * 0.3f)
                {
                    Set(x, y, Mix(Get(x, y), SunCore, 0.45f - prof * 0.2f));
                }
            }
        }

        // Linhas de ondulação horizontais.
        for (int y = 4; y < Horizon; y += 3)
        {
            float prof = 1f - y / (float)Horizon;
            for (int x = 0; x < Width; x++)
            {
                if (Noise(x / 3, y, 97) > 0.72f)
                {
                    Set(x, y, Mix(Get(x, y), new Color32(150, 178, 200, 255), 0.25f + prof * 0.2f));
                }
            }
        }
    }

    private static void PaintDistantIslands()
    {
        // Silhuetas baixas na linha do horizonte, dos dois lados do sol.
        PaintHeadland(70, 10, 120, Darken(SkyMid, 0.35f));
        PaintHeadland(200, 6, 70, Darken(SkyMid, 0.25f));
        PaintHeadland(430, 12, 150, Darken(SkyMid, 0.4f));
    }

    private static void PaintHeadland(int cx, int altura, int largura, Color32 cor)
    {
        for (int x = cx - largura / 2; x < cx + largura / 2; x++)
        {
            float t = (x - (cx - largura / 2f)) / largura;
            int h = (int)(altura * Mathf.Sin(t * Mathf.PI) * (0.7f + Noise(x, 0, 13) * 0.5f));
            for (int y = Horizon; y < Horizon + h; y++)
            {
                Set(x, y, cor);
            }
        }
    }

    private static void PaintShips()
    {
        PaintShip(250, Horizon - 26, 1);
        PaintShip(352, Horizon - 54, 1);
        PaintShip(196, Horizon - 82, 0);
    }

    /// <summary>Barco pequeno: casco, mastro e vela triangular.</summary>
    private static void PaintShip(int x, int y, int escala)
    {
        int w = 9 + escala * 5;
        int h = 3 + escala;

        for (int i = 0; i < w; i++)
        {
            float t = i / (float)(w - 1);
            int recuo = (int)(Mathf.Abs(t - 0.5f) * 2f * h);
            for (int j = recuo; j < h; j++)
            {
                Set(x + i, y + j, Darken(Bronze, 0.25f));
            }
        }

        int mastroX = x + w / 2;
        int mastroH = 8 + escala * 4;
        for (int j = 0; j < mastroH; j++)
        {
            Set(mastroX, y + h + j, Darken(Bronze, 0.4f));
        }

        for (int j = 0; j < mastroH - 2; j++)
        {
            int largura = (mastroH - 2 - j) / 2;
            for (int i = 1; i <= largura; i++)
            {
                Set(mastroX + i, y + h + j, Mix(Tunic, SkyHorizon, 0.25f));
            }
        }
    }

    // ------------------------------------------------------------------ ruínas

    private static void PaintRuins()
    {
        // Templo à direita: plataforma, colunas e entablamento.
        const int baseX = 470;
        const int baseY = Horizon - 30;

        Rect(baseX, baseY, Width - baseX, 12, StoneShade);
        Rect(baseX, baseY + 12, Width - baseX, 4, Stone);

        for (int c = 0; c < 4; c++)
        {
            int cx = baseX + 18 + c * 40;
            PaintColumn(cx, baseY + 16, 92);
        }

        // Entablamento e frontão.
        Rect(baseX + 6, baseY + 108, Width - baseX - 6, 8, Stone);
        Rect(baseX + 6, baseY + 116, Width - baseX - 6, 5, StoneShade);
        for (int i = 0; i < 26; i++)
        {
            Rect(baseX + 6 + i * 2, baseY + 121, 8 - i / 4, 3, Darken(Stone, 0.1f));
        }

        // Encosta rochosa sustentando o templo, para ele não flutuar sobre o mar.
        for (int x = baseX - 54; x < Width; x++)
        {
            float t = Mathf.Clamp01((x - (baseX - 54)) / 54f);
            int topo = baseY + (int)(t * 10f);
            for (int y = 0; y < topo; y++)
            {
                float prof = 1f - y / (float)topo;
                Set(x, y, Darken(StoneDark, 0.2f + prof * 0.3f));
            }
        }

        // Degraus descendo a encosta em direção ao mar.
        for (int d = 0; d < 5; d++)
        {
            Rect(baseX - 44 + d * 10, baseY - 16 + d * 5, 46, 5, d % 2 == 0 ? Stone : StoneShade);
        }

        PaintVase(baseX - 26, baseY + 2);
        PaintBanner(baseX + 96, baseY + 104);
    }

    private static void PaintColumn(int x, int y, int altura)
    {
        // Fuste com canelura (uma coluna clara, uma escura, alternando).
        for (int j = 0; j < altura; j++)
        {
            for (int i = 0; i < 14; i++)
            {
                Color32 c = i switch
                {
                    < 2 => StoneShade,
                    < 4 => Stone,
                    < 6 => StoneShade,
                    < 9 => Stone,
                    < 11 => StoneShade,
                    _ => Darken(StoneShade, 0.2f),
                };

                Set(x + i, y + j, c);
            }
        }

        // Capitel e base.
        Rect(x - 2, y + altura, 18, 5, Stone);
        Rect(x - 1, y - 4, 16, 4, Stone);
    }

    private static void PaintVase(int x, int y)
    {
        for (int j = 0; j < 22; j++)
        {
            float t = j / 21f;
            int meia = (int)(3 + Mathf.Sin(t * Mathf.PI * 0.95f) * 6f);
            for (int i = -meia; i <= meia; i++)
            {
                Set(x + i, y + j, i < -meia + 2 ? Darken(Bronze, 0.3f) : Bronze);
            }
        }

        // Faixa decorativa escura no bojo.
        for (int i = -6; i <= 6; i++)
        {
            Set(x + i, y + 12, Darken(Bronze, 0.55f));
            Set(x + i, y + 13, Darken(Bronze, 0.55f));
        }

        Rect(x - 4, y + 22, 9, 3, Darken(Bronze, 0.2f));
    }

    private static void PaintBanner(int x, int y)
    {
        Rect(x, y, 2, 46, Bronze);
        Rect(x - 12, y + 4, 26, 40, CloakBlue);
        Rect(x - 12, y + 4, 26, 2, Gold);

        // Coruja de Atena, bem esquemática.
        Rect(x - 6, y + 16, 14, 14, Gold);
        Rect(x - 4, y + 26, 4, 4, CloakBlue);
        Rect(x + 2, y + 26, 4, 4, CloakBlue);
        Rect(x - 1, y + 20, 2, 6, CloakBlue);
    }

    // ------------------------------------------------------------------ esquerda

    private static void PaintCliffAndTree()
    {
        // Promontório onde Odisseu está: blocos de pedra encaixados, relva no topo e
        // a face escurecendo conforme desce — sem isso vira uma mancha cinza chapada.
        for (int x = 0; x < 190; x++)
        {
            float t = x / 190f;
            int topo = (int)(Horizon - 34 - Mathf.Pow(t, 2.2f) * 30f);
            topo += (int)(Noise(x / 6, 0, 61) * 3f);

            for (int y = 0; y < topo; y++)
            {
                float prof = 1f - y / (float)topo;
                Color32 c = Darken(StoneDark, 0.15f + prof * 0.35f + t * 0.15f);

                // Juntas da alvenaria: fiadas horizontais e emendas verticais alternadas.
                int fiada = y / 9;
                bool junta = y % 9 == 0;
                bool emenda = (x + (fiada % 2) * 11) % 22 == 0;
                if (junta || emenda)
                {
                    c = Darken(c, 0.35f);
                }
                else if (y % 9 == 1)
                {
                    c = Mix(c, Stone, 0.14f);
                }

                Set(x, y, c);
            }

            // Relva e terra na borda de cima.
            for (int y = topo - 4; y < topo; y++)
            {
                Set(x, y, y >= topo - 2 ? Foliage : FoliageDark);
            }

            if (Noise(x, 1, 83) > 0.62f)
            {
                Set(x, topo, Foliage);
            }
        }

        // Árvore no canto superior esquerdo: tronco + massa de folhagem irregular.
        for (int j = 0; j < 150; j++)
        {
            int tx = 24 + (int)(Mathf.Sin(j * 0.05f) * 6f);
            Rect(tx, Height - 150 + j, 8, 1, j % 11 == 0 ? Darken(Bronze, 0.65f) : Darken(Bronze, 0.5f));
        }

        for (int y = Height - 130; y < Height; y++)
        {
            for (int x = 0; x < 170; x++)
            {
                float dx = (x - 60) / 90f;
                float dy = (y - (Height - 40)) / 80f;
                float d = dx * dx + dy * dy;
                if (d > 1f)
                {
                    continue;
                }

                if (d > 0.72f && Noise(x, y, 17) > 0.45f)
                {
                    continue;
                }

                Set(x, y, Noise(x / 2, y / 2, 29) > 0.6f ? Foliage : FoliageDark);
            }
        }
    }

    private static void PaintOdysseus()
    {
        // Figura de costas, olhando o mar: capa azul, escudo redondo, lança.
        const int px = 96;
        const int py = Horizon - 40;

        // Lança.
        for (int j = 0; j < 74; j++)
        {
            Set(px + 26, py + j, Darken(Bronze, 0.35f));
        }
        Rect(px + 25, py + 74, 3, 6, Stone);

        // Pernas.
        Rect(px + 6, py, 5, 20, Skin);
        Rect(px + 14, py, 5, 20, Darken(Skin, 0.15f));
        Rect(px + 5, py, 7, 3, Darken(Bronze, 0.4f));
        Rect(px + 13, py, 7, 3, Darken(Bronze, 0.4f));

        // Túnica e capa.
        Rect(px + 4, py + 18, 17, 16, Tunic);
        for (int j = 0; j < 34; j++)
        {
            int largura = 15 + j / 6;
            Rect(px + 3, py + 20 + j, largura, 1, j % 7 == 0 ? Darken(CloakBlue, 0.2f) : CloakBlue);
        }

        // Cabeça e coroa de louros.
        Rect(px + 9, py + 54, 9, 9, Skin);
        Rect(px + 8, py + 62, 11, 3, Darken(Bronze, 0.2f));
        for (int i = 0; i < 11; i++)
        {
            if (i % 2 == 0)
            {
                Set(px + 8 + i, py + 65, Gold);
            }
        }

        // Escudo redondo com sol em relevo.
        const int sx = px - 6;
        const int sy = py + 30;
        for (int y = -13; y <= 13; y++)
        {
            for (int x = -13; x <= 13; x++)
            {
                float d = Mathf.Sqrt(x * x + y * y);
                if (d > 13f)
                {
                    continue;
                }

                Color32 c = d > 11.5f ? Gold : d > 10f ? Bronze : Darken(Bronze, 0.35f);
                if (d < 4f)
                {
                    c = Gold;
                }
                else if (d < 9f && Mathf.Abs(Mathf.Sin(Mathf.Atan2(y, x) * 4f)) > 0.72f)
                {
                    c = Bronze;
                }

                Set(sx + x, sy + y, c);
            }
        }
    }

    // ------------------------------------------------------------------ acabamento

    private static void PaintMeanderBorder()
    {
        // Faixa de meandro grego na base, o "wave pattern" da referência.
        const int h = 26;
        Rect(0, 0, Width, h, new Color32(22, 36, 62, 255));
        Rect(0, h - 2, Width, 2, Darken(Gold, 0.35f));

        for (int bloco = 0; bloco * 24 < Width; bloco++)
        {
            int x = bloco * 24;
            // Espiral quadrada simplificada.
            Rect(x + 2, 6, 18, 2, Gold);
            Rect(x + 2, 6, 2, 12, Gold);
            Rect(x + 2, 16, 14, 2, Gold);
            Rect(x + 14, 10, 2, 8, Gold);
            Rect(x + 8, 10, 8, 2, Gold);
            Rect(x + 8, 10, 2, 5, Gold);
        }
    }

    /// <summary>
    /// Escurece o miolo para o texto dos botões ter contraste garantido, sem depender
    /// de o que estiver desenhado atrás. É o que mantém a UI legível sem cobrir a arte.
    /// </summary>
    private static void DimCenter()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float dx = Mathf.Abs(x - Width * 0.5f) / (Width * 0.42f);
                float dy = Mathf.Abs(y - Height * 0.48f) / (Height * 0.62f);
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                float dim = Mathf.Clamp01(1f - d) * 0.42f;
                if (dim > 0.005f)
                {
                    Set(x, y, Darken(Get(x, y), dim));
                }
            }
        }
    }

    private static void Write()
    {
        var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, mipChain: false);
        texture.SetPixels32(pixels);
        texture.Apply();

        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
        File.WriteAllBytes(OutputPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
        ConfigureImporter();

        Debug.Log($"[MainMenuArtGenerator] {OutputPath} ({Width}x{Height})");
    }

    /// <summary>Import de pixel art: Point, sem compressão, sem suavização.</summary>
    private static void ConfigureImporter()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(OutputPath);
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.spritePixelsPerUnit = 100;
        importer.maxTextureSize = 1024;
        importer.SaveAndReimport();
    }
}
