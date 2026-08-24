using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Confere a tradução: se a tabela está completa, se as chaves usadas no código e nas
/// cenas existem, e se nenhuma cena ficou com texto que a tabela não conhece.
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod LocalizationProbe.Check
/// </summary>
public static class LocalizationProbe
{
    private static bool falhou;

    [MenuItem("Odisseia/Conferir traducao")]
    public static void Check()
    {
        falhou = false;

        ConferirTabela();
        ConferirChavesDoCodigo();
        ConferirChavesDasCenas();
        ConferirNomesDeFase();
        ConferirTrocaDeIdioma();

        Debug.Log(falhou ? "[Traducao] RESULTADO: FALHOU" : "[Traducao] RESULTADO: OK");
        EditorApplication.Exit(falhou ? 1 : 0);
    }

    /// <summary>Toda chave precisa dos dois idiomas, e sem texto repetido por engano.</summary>
    private static void ConferirTabela()
    {
        int idiomas = System.Enum.GetValues(typeof(Language)).Length;
        var incompletas = new List<string>();

        foreach (KeyValuePair<string, string[]> par in LocalizationTable.Entries)
        {
            if (par.Value.Length < idiomas || par.Value.Any(string.IsNullOrWhiteSpace))
            {
                incompletas.Add(par.Key);
            }
        }

        Debug.Log($"[Traducao] tabela: {LocalizationTable.Entries.Count} chaves, {idiomas} idiomas");

        foreach (string chave in incompletas)
        {
            Erro("chave sem os dois idiomas: " + chave);
        }
    }

    /// <summary>Chave citada no código que não existe na tabela apareceria crua na tela.</summary>
    private static void ConferirChavesDoCodigo()
    {
        var usadas = new HashSet<string>();

        foreach (string arquivo in Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories))
        {
            string fonte = File.ReadAllText(arquivo);
            foreach (Match m in Regex.Matches(fonte, @"Localization\.(?:Get|Has)\(""([^""]+)"""))
            {
                usadas.Add(m.Groups[1].Value);
            }
        }

        Debug.Log($"[Traducao] chaves citadas no código: {usadas.Count}");

        foreach (string chave in usadas.Where(k => !LocalizationTable.Entries.ContainsKey(k)).OrderBy(k => k))
        {
            Erro("código pede chave que não existe: " + chave);
        }
    }

    /// <summary>O mesmo para as chaves gravadas dentro das cenas.</summary>
    private static void ConferirChavesDasCenas()
    {
        var usadas = new HashSet<string>();

        foreach (string arquivo in Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories))
        {
            string cena = File.ReadAllText(arquivo);
            // O "- " e do item de lista do YAML: as falas sao gravadas como
            // "  - key: dlg.…". Sem aceitar esse prefixo a conferencia ignora
            // silenciosamente TODAS as chaves de dialogo, que sao a maioria.
            foreach (Match m in Regex.Matches(cena, @"^\s*-?\s*(?:key|messageKey): (dlg\.|tut\.|ui\.|level\.)(\S+)\s*$", RegexOptions.Multiline))
            {
                usadas.Add(m.Groups[1].Value + m.Groups[2].Value);
            }
        }

        int falas = usadas.Count(k => k.StartsWith("dlg."));
        int dicas = usadas.Count(k => k.StartsWith("tut."));
        Debug.Log($"[Traducao] chaves gravadas nas cenas: {usadas.Count} " +
                  $"({falas} falas, {dicas} dicas, {usadas.Count - falas - dicas} rótulos)");

        // As falas sao a maior parte do texto do jogo. Zero aqui significa que a
        // conferencia parou de enxergar as cenas, nao que esta tudo certo.
        if (falas == 0)
        {
            Erro("nenhuma chave de fala encontrada nas cenas — a conferência não está lendo o formato certo");
        }

        foreach (string chave in usadas.Where(k => !LocalizationTable.Entries.ContainsKey(k)).OrderBy(k => k))
        {
            Erro("cena pede chave que não existe: " + chave);
        }
    }

    /// <summary>Cada fase da campanha precisa de nome nos dois idiomas.</summary>
    private static void ConferirNomesDeFase()
    {
        string[] guids = AssetDatabase.FindAssets("t:LevelDefinition");
        int conferidas = 0;

        foreach (string guid in guids)
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            if (caminho.Contains("_ForaDaCampanha"))
            {
                continue;
            }

            var def = AssetDatabase.LoadAssetAtPath<Odisseia.Core.LevelDefinition>(caminho);
            string chave = "level." + def.LevelId;
            conferidas++;

            if (!LocalizationTable.Entries.ContainsKey(chave))
            {
                Erro("fase sem nome traduzido: " + chave);
            }
        }

        Debug.Log($"[Traducao] nomes de fase conferidos: {conferidas}");
    }

    /// <summary>Trocar o idioma tem que trocar o texto de verdade.</summary>
    private static void ConferirTrocaDeIdioma()
    {
        Language original = Localization.Current;

        Localization.Current = Language.English;
        string en = Localization.Get("ui.menu.newGame");
        string faseEn = Localization.Get("level.Level_04_Citera");

        Localization.Current = Language.Portuguese;
        string pt = Localization.Get("ui.menu.newGame");
        string fasePt = Localization.Get("level.Level_04_Citera");

        Localization.Current = original;

        Debug.Log($"[Traducao] 'ui.menu.newGame'  en='{en}'  pt='{pt}'");
        Debug.Log($"[Traducao] 'level.Level_04_Citera'  en='{faseEn}'  pt='{fasePt}'");

        if (en == pt)
        {
            Erro("trocar o idioma não mudou o texto");
        }

        // Chave inexistente devolve a propria chave, e nao vazio: o problema fica visivel.
        const string inventada = "ui.chave.que.nao.existe";
        if (Localization.Get(inventada) != inventada)
        {
            Erro("chave inexistente deveria voltar a própria chave");
        }
    }

    private static void Erro(string m)
    {
        falhou = true;
        Debug.LogError("[Traducao] " + m);
    }
}
