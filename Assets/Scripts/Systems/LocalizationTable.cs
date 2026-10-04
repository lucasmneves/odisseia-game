using System.Collections.Generic;

namespace Odisseia.Systems
{
    /// <summary>
    /// Todo o texto visível do jogo, por chave.
    ///
    /// Cada entrada é um vetor indexado por <see cref="Language"/>: posição 0 inglês,
    /// posição 1 português. O inglês é o idioma de referência — é para ele que o
    /// <see cref="Localization"/> cai quando falta tradução.
    ///
    /// Arquivo gerado a partir do texto que já existia nas cenas, para a tradução não
    /// reescrever por acidente o que o jogo dizia. Editar à mão é esperado; o que não
    /// se deve fazer é deixar uma chave só com um dos idiomas.
    /// </summary>
    public static class LocalizationTable
    {
        public static readonly Dictionary<string, string[]> Entries = new Dictionary<string, string[]>
        {
            // ---------------------------------------------------------- Interface, telas e nomes de fase

            ["ui.menu.continue"] = new[]
            {
                "CONTINUE",
                "CONTINUAR",
            },

            ["ui.menu.newGame"] = new[]
            {
                "NEW GAME",
                "NOVO JOGO",
            },

            ["ui.menu.levelSelect"] = new[]
            {
                "LEVEL SELECT",
                "SELECIONAR FASE",
            },

            ["ui.menu.settings"] = new[]
            {
                "SETTINGS",
                "CONFIGURAÇÕES",
            },

            ["ui.menu.subtitle"] = new[]
            {
                "— THE JOURNEY OF ODYSSEUS —",
                "— JORNADA DE ODISSEU —",
            },

            ["ui.menu.noSave"] = new[]
            {
                "No saved game found.",
                "Nenhum jogo salvo encontrado.",
            },

            ["ui.confirm.newGame.title"] = new[]
            {
                "START A NEW GAME?",
                "INICIAR NOVO JOGO?",
            },

            ["ui.confirm.newGame.body"] = new[]
            {
                "All current progress will be replaced.",
                "Todo o progresso atual será substituído.",
            },

            ["ui.confirm.yes"] = new[]
            {
                "CONFIRM",
                "CONFIRMAR",
            },

            ["ui.confirm.no"] = new[]
            {
                "CANCEL",
                "CANCELAR",
            },

            ["ui.settings.title"] = new[]
            {
                "SETTINGS",
                "CONFIGURAÇÕES",
            },

            ["ui.settings.section.audio"] = new[]
            {
                "AUDIO",
                "ÁUDIO",
            },

            ["ui.settings.section.controls"] = new[]
            {
                "CONTROLS",
                "CONTROLES",
            },

            ["ui.settings.section.graphics"] = new[]
            {
                "GRAPHICS",
                "GRÁFICOS",
            },

            ["ui.settings.section.language"] = new[]
            {
                "LANGUAGE",
                "IDIOMA",
            },

            ["ui.settings.masterVolume"] = new[]
            {
                "Master volume",
                "Volume geral",
            },

            ["ui.settings.music"] = new[]
            {
                "Music",
                "Música",
            },

            ["ui.settings.sfx"] = new[]
            {
                "Sound effects",
                "Efeitos sonoros",
            },

            ["ui.settings.keyboardGamepad"] = new[]
            {
                "Keyboard and gamepad",
                "Teclado e controle",
            },

            ["ui.settings.customize"] = new[]
            {
                "Customize",
                "Personalizar",
            },

            ["ui.settings.quality"] = new[]
            {
                "Quality",
                "Qualidade",
            },

            ["ui.settings.resolution"] = new[]
            {
                "Resolution",
                "Resolução",
            },

            ["ui.settings.fullscreen"] = new[]
            {
                "Fullscreen",
                "Tela cheia",
            },

            ["ui.settings.language"] = new[]
            {
                "Language",
                "Idioma",
            },

            ["ui.settings.close"] = new[]
            {
                "CLOSE",
                "FECHAR",
            },

            ["ui.common.yes"] = new[]
            {
                "YES",
                "SIM",
            },

            ["ui.common.no"] = new[]
            {
                "NO",
                "NÃO",
            },

            ["ui.levelSelect.title"] = new[]
            {
                "THE JOURNEY",
                "A JORNADA",
            },

            ["ui.levelSelect.back"] = new[]
            {
                "BACK",
                "VOLTAR",
            },

            ["ui.levelSelect.noCampaign"] = new[]
            {
                "No campaign loaded.\n\nStart the game from the Boot scene.",
                "Nenhuma campanha carregada.\n\nAbra o jogo pela cena Boot.",
            },

            ["ui.worldMap.title"] = new[]
            {
                "THE ODYSSEY",
                "A ODISSEIA",
            },

            ["ui.worldMap.subtitle"] = new[]
            {
                "Journey of Odysseus",
                "Jornada de Odisseu",
            },

            ["ui.worldMap.locked"] = new[]
            {
                "Finish the previous stage to continue your journey.",
                "Complete a fase anterior para continuar sua jornada.",
            },

            ["ui.worldMap.progress"] = new[]
            {
                "Stage {0} of {1}",
                "Fase {0} de {1}",
            },

            ["ui.worldMap.play"] = new[]
            {
                "[E] Play",
                "[E] Jogar",
            },

            ["ui.worldMap.replay"] = new[]
            {
                "Already finished — [E] to play again",
                "Já concluída — [E] para jogar de novo",
            },

            ["ui.worldMap.blocked"] = new[]
            {
                "Locked",
                "Bloqueada",
            },

            ["ui.worldMap.stageComplete"] = new[]
            {
                "STAGE COMPLETE",
                "FASE CONCLUÍDA",
            },

            ["ui.worldMap.unlocked"] = new[]
            {
                "New region unlocked:",
                "Nova região desbloqueada:",
            },

            ["ui.pause.title"] = new[]
            {
                "PAUSED",
                "PAUSADO",
            },

            ["ui.pause.resume"] = new[]
            {
                "Resume",
                "Continuar",
            },

            ["ui.pause.restart"] = new[]
            {
                "Restart stage",
                "Reiniciar fase",
            },

            ["ui.pause.menu"] = new[]
            {
                "Main menu",
                "Menu principal",
            },

            // Fim de jogo (QA-03: ganhou "tentar de novo"; os textos saíram do português fixo — QA-09).
            ["ui.gameover.title"] = new[]
            {
                "END OF THE JOURNEY",
                "FIM DA JORNADA",
            },

            ["ui.gameover.message"] = new[]
            {
                "Odysseus did not reach Ithaca this time.",
                "Odisseu não chegou a Ítaca desta vez.",
            },

            ["ui.gameover.stats"] = new[]
            {
                "Experience gathered: {0} XP\nItems collected: {1}",
                "Experiência acumulada: {0} XP\nItens coletados: {1}",
            },

            ["ui.gameover.retry"] = new[]
            {
                "Try again",
                "Tentar de novo",
            },

            ["ui.gameover.hint"] = new[]
            {
                "Stages already won stay unlocked.",
                "As fases já conquistadas continuam desbloqueadas.",
            },

            ["ui.stageComplete"] = new[]
            {
                "STAGE COMPLETE",
                "FASE CONCLUÍDA",
            },

            ["ui.ending.title"] = new[]
            {
                "THE LONG JOURNEY IS OVER",
                "A LONGA JORNADA TERMINOU",
            },

            ["ui.ending.body"] = new[]
            {
                "Odysseus has finally come home.",
                "Odisseu finalmente voltou para casa.",
            },

            ["ui.ending.playAgain"] = new[]
            {
                "Play again",
                "Jogar novamente",
            },

            ["ui.ending.backToMenu"] = new[]
            {
                "Back to menu",
                "Voltar ao menu",
            },

            ["ui.hud.hunger"] = new[]
            {
                "🍖 Hunger: {0}%",
                "🍖 Fome: {0}%",
            },

            ["ui.hud.disguised"] = new[]
            {
                "🥸 Disguised as a beggar",
                "🥸 Disfarçado de mendigo",
            },

            ["ui.hud.itacaBefore"] = new[]
            {
                "🏠 Ithaca — before the war",
                "🏠 Ítaca — antes da guerra",
            },

            ["ui.loading"] = new[]
            {
                "Loading...",
                "Carregando...",
            },

            // QA-09: textos que estavam fixos em português no código.
            ["ui.worldMap.enter"] = new[]
            {
                "PLAY",
                "JOGAR",
            },

            ["ui.mobile.rotate"] = new[]
            {
                "Rotate your device to play in landscape",
                "Gire o aparelho para jogar em modo paisagem",
            },

            ["ui.cattle.eatPrompt"] = new[]
            {
                "Press {0} to eat the sacred cattle of Helios (there will be consequences).",
                "Pressione {0} para comer o gado sagrado de Hélio (isso terá consequências).",
            },

            ["ui.rebind.waiting"] = new[]
            {
                "Press the new key for \"{0}\". Esc cancels.",
                "Pressione a nova tecla para \"{0}\". Esc cancela.",
            },

            ["ui.rebind.move"] = new[]
            {
                "Move",
                "Mover",
            },

            ["ui.rebind.moveLeft"] = new[]
            {
                "Move left",
                "Mover para a esquerda",
            },

            ["ui.rebind.moveRight"] = new[]
            {
                "Move right",
                "Mover para a direita",
            },

            ["ui.rebind.jump"] = new[]
            {
                "Jump",
                "Pular",
            },

            ["ui.rebind.attack"] = new[]
            {
                "Attack",
                "Atacar",
            },

            ["ui.rebind.shield"] = new[]
            {
                "Block",
                "Defender",
            },

            ["ui.rebind.bow"] = new[]
            {
                "Bow",
                "Arco",
            },

            ["ui.rebind.interact"] = new[]
            {
                "Interact",
                "Interagir",
            },

            ["ui.rebind.alternate"] = new[]
            {
                "{0} (alt.)",
                "{0} (alt.)",
            },

            ["ui.npc.interactPrompt"] = new[]
            {
                "Press {0} to talk",
                "Pressione {0} para conversar",
            },

            ["level.Level_01_Itaca_Prologue"] = new[]
            {
                "Ithaca — The Call",
                "Ítaca — O Chamado",
            },

            ["level.Level_02_Troia"] = new[]
            {
                "Troy",
                "Troia",
            },

            ["level.Level_03_Cicones"] = new[]
            {
                "Cicones",
                "Cícones",
            },

            ["level.Level_04_Citera"] = new[]
            {
                "Cythera",
                "Cítera",
            },

            ["level.Level_05_Ciclopes"] = new[]
            {
                "Cyclopes",
                "Ciclopes",
            },

            ["level.Level_06_Eolo"] = new[]
            {
                "Aeolus",
                "Éolo",
            },

            ["level.Level_07_Lestrigoes"] = new[]
            {
                "Laestrygonians",
                "Lestrígones",
            },

            ["level.Level_08_Circe"] = new[]
            {
                "Circe",
                "Circe",
            },

            ["level.Level_09_MundoDosMortos"] = new[]
            {
                "The Underworld",
                "Mundo dos Mortos",
            },

            ["level.Level_10_Sereias"] = new[]
            {
                "Sirens",
                "Sereias",
            },

            ["level.Level_11_CilaCaribdis"] = new[]
            {
                "Scylla and Charybdis",
                "Cila e Caríbdis",
            },

            ["level.Level_12_GadoDoSol"] = new[]
            {
                "Cattle of the Sun",
                "Gado do Sol",
            },

            ["level.Level_13_Calipso"] = new[]
            {
                "Calypso",
                "Calipso",
            },

            ["level.Level_14_Itaca_Return"] = new[]
            {
                "Ithaca — The Return",
                "Ítaca — O Retorno",
            },

            ["level.Level_15_Pretendentes"] = new[]
            {
                "The Suitors",
                "Pretendentes",
            },

            ["level.Level_16_Final"] = new[]
            {
                "Finale",
                "Final",
            },

            ["speaker.odysseus"] = new[]
            {
                "Odysseus",
                "Odisseu",
            },

            // Cenas que gravam o falante em PORTUGUÊS (o DialogueSequence busca "speaker." + nome sem acento): sem estas
            // chaves o nome saía cru, "Odisseu", com a fala em inglês. Mesmas traduções das chaves canônicas.
            ["speaker.odisseu"] = new[]
            {
                "Odysseus",
                "Odisseu",
            },

            ["speaker.telemaco"] = new[]
            {
                "Telemachus",
                "Telêmaco",
            },

            ["speaker.eumeu"] = new[]
            {
                "Eumaeus",
                "Eumeu",
            },

            ["speaker.companheiro"] = new[]
            {
                "Crewman",
                "Companheiro",
            },

            ["speaker.penelope"] = new[]
            {
                "Penelope",
                "Penélope",
            },

            ["speaker.telemachus"] = new[]
            {
                "Telemachus",
                "Telêmaco",
            },

            ["speaker.eumaeus"] = new[]
            {
                "Eumaeus",
                "Eumeu",
            },

            ["speaker.crewman"] = new[]
            {
                "Crewman",
                "Companheiro",
            },


            // ---------------------------------------------------------- Falas

            ["dlg.Level_02_Troia.OutroDialogueController.0"] = new[]
            {
                "The horse did what the spear could not: Troy has fallen. The war is over.",
                "O cavalo cumpriu o que a lança não conseguiu: Troia caiu. A guerra acabou.",
            },

            ["dlg.Level_02_Troia.OutroDialogueController.1"] = new[]
            {
                "Odysseus sails for home. Ithaca lies one sea away — and the sea has plans of its own.",
                "Odisseu embarca de volta. Ítaca fica a um mar de distância — e o mar tem seus próprios planos.",
            },

            ["dlg.Level_02_Troia.IntroDialogueController.0"] = new[]
            {
                "Ten years of siege. Agamemnon gathered the Greeks before the walls of Troy, and the walls are still standing.",
                "Dez anos de cerco. Agamenon reuniu os gregos diante das muralhas de Troia, e elas continuam de pé.",
            },

            ["dlg.Level_02_Troia.IntroDialogueController.1"] = new[]
            {
                "If force will not bring these walls down, let cunning do it: a wooden horse, with men hidden inside.",
                "Se a força não derruba estes muros, que os derrube a astúcia: um cavalo de madeira, e homens escondidos dentro dele.",
            },

            ["dlg.Level_03_Cicones.IntroDialogueController.0"] = new[]
            {
                "Our first landfall since Troy, and already we have plundered too much. The Cicones will not let us leave in peace.",
                "Primeira parada desde Troia, e já saqueamos demais. Os cícones não vão nos deixar partir em paz.",
            },

            ["dlg.Level_03_Cicones.IntroDialogueController.1"] = new[]
            {
                "I must cross the village before they surround my men.",
                "Preciso atravessar a vila antes que cerquem meus homens.",
            },

            ["dlg.Level_03_Cicones.OutroDialogueController.0"] = new[]
            {
                "We escaped the fury of the Cicones. Odysseus takes up the voyage again — the way home runs through open water.",
                "Escapamos da fúria dos cícones. Odisseu retoma a viagem — o caminho de casa é pelo mar aberto.",
            },

            ["dlg.Level_04_Citera.OutroDialogueController.0"] = new[]
            {
                "The storm passed at last. But Odysseus had been driven far off his course, to lands unknown...",
                "A tempestade finalmente passou. Mas Odisseu havia sido levado para longe de sua rota, para terras desconhecidas...",
            },

            ["dlg.Level_04_Citera.OutroDialogueController.1"] = new[]
            {
                "NEXT DESTINATION — THE ISLAND OF THE CYCLOPES",
                "PRÓXIMO DESTINO — A ILHA DOS CICLOPES",
            },

            ["dlg.Level_04_Citera.IntroDialogueController.0"] = new[]
            {
                "Having left the Cicones behind, Odysseus and his men turned once more toward home. But the sea would not grant them an easy crossing.",
                "Depois de deixar os cícones, Odisseu e seus homens retomaram o caminho para casa. Mas o mar não permitiria uma viagem tranquila.",
            },

            ["dlg.Level_04_Citera.IntroDialogueController.1"] = new[]
            {
                "A terrible storm rose on the horizon.",
                "Uma terrível tempestade surgiu no horizonte.",
            },

            ["dlg.Level_04_Citera.IntroDialogueController.2"] = new[]
            {
                "Hold fast! Do not let her capsize!",
                "Segurem-se! Não deixem o navio virar!",
            },

            ["dlg.Level_04_Citera.StormDialogue_1_Controller.0"] = new[]
            {
                "The waves are getting bigger, captain!",
                "As ondas estão ficando maiores, capitão!",
            },

            ["dlg.Level_04_Citera.StormDialogue_2_Controller.0"] = new[]
            {
                "The wind tore the steering oar away! It is no longer we who choose the course.",
                "O vento arrancou o leme! Já não somos nós que escolhemos o rumo.",
            },

            ["dlg.Level_05_Ciclopes.IntroDialogueController.0"] = new[]
            {
                "The storm threw us onto a coast no map knows. We need water and food.",
                "A tempestade nos jogou numa costa que nenhum mapa conhece. Precisamos de água e comida.",
            },

            ["dlg.Level_05_Ciclopes.IntroDialogueController.1"] = new[]
            {
                "This cave smells of sheep, and of something far larger. Better that I am not seen.",
                "Esta caverna cheira a carneiro e a algo muito maior. É melhor eu não ser visto.",
            },

            ["dlg.Level_05_Ciclopes.OutroDialogueController.0"] = new[]
            {
                "Blind with rage, Polyphemus cries out to his father, Poseidon. Odysseus is already far away — but the sea never forgets an insult.",
                "Cego de fúria, Polifemo grita para o pai, Poseidon. Odisseu já está longe, mas o mar nunca esquece uma ofensa.",
            },

            ["dlg.Level_06_Eolo.OutroDialogueController.0"] = new[]
            {
                "With Aeolus's winds still in his favour, Odysseus sees Ithaca almost within reach — until greed opens the bag and the storm hurls him back.",
                "Com os ventos de Éolo ainda ao seu favor, Odisseu vê Ítaca quase ao alcance — mas a viagem está longe de terminar.",
            },

            ["dlg.Level_06_Eolo.IntroDialogueController.0"] = new[]
            {
                "Aeolus gave me the contrary winds bound in a bag... and the fair wind to carry us home.",
                "Éolo me deu os ventos contrários presos num saco... e o vento favorável para nos levar para casa.",
            },

            ["dlg.Level_07_Lestrigoes.IntroDialogueController.0"] = new[]
            {
                "Laestrygonians! They smash my ships with stones the size of houses. Run — do not look back!",
                "Lestrígones! Eles despedaçam meus navios com pedras do tamanho de casas. Corram para o mar!",
            },

            ["dlg.Level_07_Lestrigoes.OutroDialogueController.0"] = new[]
            {
                "Only one ship escaped the Laestrygonian bay. Odysseus mourns the men he could not save.",
                "Apenas um navio escapou da baía dos lestrígones. Odisseu chora os companheiros perdidos, mas o mar não espera pelos mortos.",
            },

            ["dlg.Level_08_Circe.IntroDialogueController.0"] = new[]
            {
                "Smoke and singing come from that house in the forest... and my men have not come back.",
                "Fumaça e cantos vêm daquela casa na floresta... e meus homens não voltaram. Isso cheira a feitiçaria.",
            },

            ["dlg.Level_08_Circe.OutroDialogueController.0"] = new[]
            {
                "Circe yields before the sword and the herb of Hermes. For a year, the island holds them — until the crew begs to sail on.",
                "Circe cede diante da espada e da erva de Hermes. Por um ano, a ilha os acolhe — mas Ítaca ainda espera.",
            },

            ["dlg.Level_09_MundoDosMortos.ShadesDialogueController.0"] = new[]
            {
                "Nameless shades crowd toward the blood I spilled. They hunger to speak, and I must not let them.",
                "Sombras sem nome se aproximam do sangue que derramei. Elas anseiam por vozes.",
            },

            ["dlg.Level_09_MundoDosMortos.MotherDialogueController.0"] = new[]
            {
                "There... my mother. She died of longing, waiting for me to return.",
                "Ali... minha mãe. Ela morreu de saudade, esperando meu retorno.",
            },

            ["dlg.Level_09_MundoDosMortos.OutroDialogueController.0"] = new[]
            {
                "Tiresias has spoken: the way home is long, and Poseidon still guards it.",
                "Tirésias falou: o caminho de volta é longo, e Poseidon ainda guarda rancor. Mas há um caminho.",
            },

            ["dlg.Level_09_MundoDosMortos.IntroDialogueController.0"] = new[]
            {
                "Circe sent me here, to the threshold of the land of the dead, to hear the prophecy of Tiresias.",
                "Circe me mandou aqui, ao limiar do mundo dos mortos, para ouvir a profecia de Tirésias.",
            },

            ["dlg.Level_10_Sereias.OutroDialogueController.0"] = new[]
            {
                "The song fades in the ship's wake. Odysseus can still hear its echoes, but Ithaca calls louder.",
                "O canto se desvanece na esteira do navio. Odisseu ainda ouve seus ecos, mas Ítaca chama mais alto.",
            },

            ["dlg.Level_10_Sereias.IntroDialogueController.0"] = new[]
            {
                "Circe warned me: the sirens sing the truth we most long to hear — and no one who listens comes back alive. Bind me to the mast.",
                "Circe avisou: as sereias cantam a verdade que mais desejamos ouvir — e ninguém que as escuta volta com vida. Amarrem-me ao mastro.",
            },

            ["dlg.Level_11_CilaCaribdis.OutroDialogueController.0"] = new[]
            {
                "The strait is behind them, but the price was high. Odysseus counts the men who remain and sails on, because stopping is not a choice.",
                "O estreito fica para trás, mas o preço foi alto. Odisseu conta os que restam e segue em frente, pois parar não é opção.",
            },

            ["dlg.Level_11_CilaCaribdis.IntroDialogueController.0"] = new[]
            {
                "One strait, two monsters. Charybdis swallows the whole sea three times a day; Scylla tears men from the deck with six mouths. There is no winning — only crossing.",
                "Um estreito, dois monstros. Caríbdis engole o mar inteiro três vezes ao dia; Cila arranca homens do convés com seis bocas. Não há como vencer — só atravessar.",
            },

            ["dlg.Level_12_GadoDoSol.IntroDialogueController.0"] = new[]
            {
                "Circe warned us: do not touch the sacred cattle of Helios, or the wrath of the gods will follow.",
                "Circe avisou: não toquem no gado sagrado de Hélio, ou a ira dos deuses cairá sobre nós. Mas a fome é grande, e o vento não sopra.",
            },

            ["dlg.Level_12_GadoDoSol.OutroDialogueController.0"] = new[]
            {
                "The winds turn at last. Odysseus takes to the sea again — but not every choice can be undone.",
                "Os ventos finalmente mudam. Odisseu retoma o mar — mas nem toda escolha feita nesta ilha ficará sem resposta.",
            },

            ["dlg.Level_13_Calipso.IntroDialogueController.0"] = new[]
            {
                "Seven years. Calypso offers me immortality and a home here. But my heart never stopped going back to Ithaca.",
                "Sete anos. Calipso me oferece imortalidade e um lar aqui. Mas meu coração nunca deixou de voltar para Ítaca.",
            },

            ["dlg.Level_13_Calipso.RaftDialogueController.0"] = new[]
            {
                "Hermes brought the order of Zeus: Calypso must let me go. She weeps, but she obeys.",
                "Hermes trouxe a ordem de Zeus: Calipso deve me deixar partir. Ela chora, mas obedece.",
            },

            ["dlg.Level_13_Calipso.GroveDialogueController.0"] = new[]
            {
                "Calypso wove at her loom, singing. For a moment, I almost forgot Penelope.",
                "Calipso teceu junto ao tear, cantando. Por um instante, quase esqueci Penélope.",
            },

            ["dlg.Level_13_Calipso.OutroDialogueController.0"] = new[]
            {
                "On a simple raft, Odysseus faces the open sea once more. Now the way is clear — and it leads to Ithaca.",
                "Numa jangada simples, Odisseu enfrenta o mar aberto mais uma vez. Agora o caminho é livre — e ele leva a Ítaca.",
            },

            ["dlg.Level_14_Itaca_Return.TelemacoDialogueController.0"] = new[]
            {
                "My father left before I could really know him. All I have are stories about him.",
                "Meu pai partiu antes de eu poder conhecê-lo direito. Só ouço histórias sobre ele.",
            },

            ["dlg.Level_14_Itaca_Return.EumeuDialogueController.0"] = new[]
            {
                "A beggar, is it? Sit down, stranger, eat with me. My old master, Odysseus... I am waiting for him still.",
                "Um mendigo, hein? Sente-se, estranho, coma comigo. Meu antigo senhor, Odisseu... eu ainda espero por ele.",
            },

            ["dlg.Level_14_Itaca_Return.OutroDialogueController.0"] = new[]
            {
                "In secret, father and son know each other again. Telemachus tells of the hall overrun by the suitors.",
                "Em segredo, pai e filho se reconhecem. Telêmaco conta do salão tomado pelos pretendentes.",
            },

            ["dlg.Level_14_Itaca_Return.OutroDialogueController.1"] = new[]
            {
                "\"It is time. Let us take back what is yours.\"",
                "\"É hora. Vamos reaver o que é seu.\"",
            },

            ["dlg.Level_14_Itaca_Return.IntroDialogueController.0"] = new[]
            {
                "Ithaca. After twenty years I stand on my own soil at last — but Athena warns me: it is not yet time to be known.",
                "Ítaca. Depois de vinte anos, finalmente piso em solo natal — mas Atena me avisa: ainda não é hora de ser reconhecido.",
            },

            ["dlg.Level_14_Itaca_Return.IntroDialogueController.1"] = new[]
            {
                "I must find out what happened in my absence, before anyone finds out that I am back.",
                "Preciso descobrir o que aconteceu na minha ausência antes que alguém descubra que eu voltei.",
            },

            ["dlg.Level_15_Pretendentes.OutroDialogueController.0"] = new[]
            {
                "One by one, the suitors give way before the justice of Odysseus. The great hall falls silent at last.",
                "Um a um, os pretendentes recuam diante da justiça de Odisseu. O grande salão, finalmente, silencia.",
            },

            ["dlg.Level_15_Pretendentes.PenelopeDialogueController.0"] = new[]
            {
                "Twenty years of waiting, testing suitor after suitor with riddles and delays. I have run out of delays.",
                "Vinte anos esperando, testando pretendente após pretendente com enigmas e adiamentos. Se você é mesmo Odisseu... prove.",
            },

            ["dlg.Level_15_Pretendentes.IntroDialogueController.0"] = new[]
            {
                "The hall of my own palace, overrun by men who devour my herds and court my wife.",
                "O salão do meu próprio palácio, tomado por homens que devoram meus bens e cortejam minha esposa à força. Isso termina hoje.",
            },

            ["dlg.Level_15_Pretendentes.TelemacoDialogueController.0"] = new[]
            {
                "I am at your side, father. I always was.",
                "Estou ao seu lado, pai. Sempre estive.",
            },

            ["dlg.Level_16_Final.ClimaxDialogueController.0"] = new[]
            {
                "The string sings as it did twenty years ago. The arrow flies true — through every axe head.",
                "A corda canta como há vinte anos. A flecha voa reta — através de cada machado, sem desviar uma polegada.",
            },

            ["dlg.Level_16_Final.IntroDialogueController.0"] = new[]
            {
                "Only the hands that bent this bow in youth can string it now. Penelope brought it out as the final test — not knowing the beggar before her is the king himself.",
                "Só as mãos que curvaram este arco em juventude podem retesá-lo agora. Penélope o trouxe como prova final — sem saber que o mendigo diante dela é o próprio rei.",
            },

            ["dlg.Level_16_Final.OutroDialogueController.0"] = new[]
            {
                "The disguise falls away. \"It is I,\" says Odysseus. \"I have come home.\"",
                "O disfarce cai. \"Sou eu\", diz Odisseu. \"Voltei para casa.\"",
            },


            // ---------------------------------------------------------- Dicas de tutorial

            ["tut.Level_02_Troia.Tutorial_Attack"] = new[]
            {
                "Press Z to attack.",
                "Pressione Z para atacar.",
            },

            ["tut.Level_02_Troia.Tutorial_Jump"] = new[]
            {
                "Press SPACE to jump.",
                "Pressione SPACE para pular.",
            },

            ["tut.Level_02_Troia.Tutorial_Move"] = new[]
            {
                "Use A/D or ←/→ to move.",
                "Use A/D ou ←/→ para se mover.",
            },


            // ---------------------------------------------------------- Prólogo de Ítaca: personagens

            ["speaker.herald"] = new[]
            {
                "Herald",
                "Arauto",
            },

            ["speaker.mentor"] = new[]
            {
                "Mentor",
                "Mentor",
            },

            ["speaker.trainer"] = new[]
            {
                "Trainer",
                "Treinador",
            },

            ["speaker.blacksmith"] = new[]
            {
                "Blacksmith",
                "Ferreiro",
            },

            ["speaker.fisherman"] = new[]
            {
                "Fisherman",
                "Pescador",
            },

            ["speaker.farmer"] = new[]
            {
                "Farmer",
                "Agricultor",
            },

            ["speaker.sailor"] = new[]
            {
                "Sailor",
                "Marinheiro",
            },

            ["speaker.eurylochus"] = new[]
            {
                "Eurylochus",
                "Euríloco",
            },

            ["speaker.rower"] = new[]
            {
                "Rower",
                "Remador",
            },

            ["speaker.shepherd"] = new[]
            {
                "Shepherd",
                "Pastor",
            },

            ["speaker.watchman"] = new[]
            {
                "Watchman",
                "Vigia",
            },

            ["speaker.carpenter"] = new[]
            {
                "Carpenter",
                "Carpinteiro",
            },

            ["speaker.elpenor"] = new[]
            {
                "Elpenor",
                "Elpenor",
            },


            // ---------------------------------------------------------- Prólogo de Ítaca: objetivos

            ["obj.prologue.explore"] = new[]
            {
                "Explore Ithaca",
                "Explore Ítaca",
            },

            ["obj.prologue.summons"] = new[]
            {
                "Hear out the herald",
                "Ouça o arauto",
            },

            ["obj.prologue.family"] = new[]
            {
                "Speak with Penelope and Telemachus",
                "Fale com Penélope e Telêmaco",
            },

            ["obj.prologue.recruit"] = new[]
            {
                "Gather the men of Ithaca",
                "Reúna os homens de Ítaca",
            },

            ["obj.prologue.training"] = new[]
            {
                "Train for the war",
                "Treine para a guerra",
            },

            ["obj.prologue.equipment"] = new[]
            {
                "Prepare weapons and equipment",
                "Prepare armas e equipamentos",
            },

            ["obj.prologue.ships"] = new[]
            {
                "Prepare the ships",
                "Prepare os navios",
            },

            ["obj.prologue.farewell"] = new[]
            {
                "Say goodbye to your family",
                "Despeça-se da sua família",
            },

            ["obj.prologue.depart"] = new[]
            {
                "Board the ship and set sail",
                "Embarque no navio e parta",
            },


            // ---------------------------------------------------------- Prólogo de Ítaca: interações

            ["ui.interact.prompt"] = new[]
            {
                "Press {0} to examine",
                "Pressione {0} para examinar",
            },

            ["ui.interact.prompt.mobile"] = new[]
            {
                "Tap {0} to examine",
                "Toque em {0} para examinar",
            },

            ["ui.npc.interactPrompt.mobile"] = new[]
            {
                "Tap {0} to talk",
                "Toque em {0} para conversar",
            },

            ["int.prologue.town"] = new[]
            {
                "The town of Ithaca. Small, and mine.",
                "A cidade de Ítaca. Pequena, e minha.",
            },

            ["int.prologue.people"] = new[]
            {
                "My people sleep well, because the sea has been quiet.",
                "Meu povo dorme tranquilo, porque o mar tem estado calmo.",
            },

            ["int.prologue.soldiers"] = new[]
            {
                "A handful of soldiers. Enough for peace, not for a war.",
                "Um punhado de soldados. Suficiente para a paz, não para uma guerra.",
            },

            ["int.prologue.palace"] = new[]
            {
                "My hall. Penelope and Telemachus are inside.",
                "Meu salão. Penélope e Telêmaco estão lá dentro.",
            },

            ["int.prologue.swords"] = new[]
            {
                "Swords sharpened.",
                "Espadas afiadas.",
            },

            ["int.prologue.shields"] = new[]
            {
                "Shields checked.",
                "Escudos conferidos.",
            },

            ["int.prologue.arrows"] = new[]
            {
                "Bows strung. Arrows counted.",
                "Arcos retesados. Flechas contadas.",
            },

            ["int.prologue.armor"] = new[]
            {
                "Armour and provisions packed.",
                "Armaduras e mantimentos embalados.",
            },

            ["int.prologue.supplies"] = new[]
            {
                "Supplies loaded.",
                "Suprimentos carregados.",
            },

            ["int.prologue.oars"] = new[]
            {
                "Oars aboard.",
                "Remos a bordo.",
            },

            ["int.prologue.sails"] = new[]
            {
                "Sails mended and raised.",
                "Velas remendadas e içadas.",
            },

            ["int.prologue.crew"] = new[]
            {
                "The crews know their ships.",
                "As tripulações conhecem seus navios.",
            },


            // ---------------------------------------------------------- Prólogo de Ítaca: treinamento

            ["tut.prologue.stepDone"] = new[]
            {
                "Well done!",
                "Muito bem!",
            },

            ["tut.prologue.move"] = new[]
            {
                "Use {0} to move.",
                "Use {0} para se mover.",
            },

            ["tut.prologue.move.mobile"] = new[]
            {
                "Hold {0} to move.",
                "Segure {0} para se mover.",
            },

            ["tut.prologue.jump"] = new[]
            {
                "Press {0} to jump.",
                "Pressione {0} para pular.",
            },

            ["tut.prologue.jump.mobile"] = new[]
            {
                "Tap {0} to jump.",
                "Toque em {0} para pular.",
            },

            ["tut.prologue.sword"] = new[]
            {
                "Press {0} to swing your sword.",
                "Pressione {0} para golpear com a espada.",
            },

            ["tut.prologue.sword.mobile"] = new[]
            {
                "Tap {0} to swing your sword.",
                "Toque em {0} para golpear com a espada.",
            },

            ["tut.prologue.swordDummy"] = new[]
            {
                "Hit the training dummy with {0}.",
                "Acerte o boneco de treino com {0}.",
            },

            ["tut.prologue.swordDummy.mobile"] = new[]
            {
                "Stand next to the dummy and tap {0}.",
                "Fique perto do boneco e toque em {0}.",
            },

            ["tut.prologue.jumpGaps"] = new[]
            {
                "Cross the platforms with {0}.",
                "Atravesse as plataformas com {0}.",
            },

            ["tut.prologue.shield"] = new[]
            {
                "The soldier is coming. Hold {0} to block his blows — the shield only covers your front.",
                "O soldado vem vindo. Segure {0} para bloquear os golpes dele — o escudo só cobre a sua frente.",
            },

            ["tut.prologue.shield.mobile"] = new[]
            {
                "The soldier is coming. Hold {0} to block his blows — the shield only covers your front.",
                "O soldado vem vindo. Segure {0} para bloquear os golpes dele — o escudo só cobre a sua frente.",
            },

            ["tut.prologue.bow"] = new[]
            {
                "Press {0} to shoot. Every shot costs an arrow.",
                "Pressione {0} para disparar. Cada tiro gasta uma flecha.",
            },

            ["tut.prologue.bow.mobile"] = new[]
            {
                "Tap {0} to shoot. Every shot costs an arrow.",
                "Toque em {0} para disparar. Cada tiro gasta uma flecha.",
            },

            ["tut.prologue.bowTargets"] = new[]
            {
                "Hit the targets with the bow.",
                "Acerte os alvos com o arco.",
            },

            ["tut.prologue.combo"] = new[]
            {
                "Now all of it: bow at range, shield when they swing, sword up close.",
                "Agora tudo junto: arco à distância, escudo quando atacarem, espada de perto.",
            },


            // ---------------------------------------------------------- Prólogo de Ítaca: falas

            ["dlg.Level_01_Itaca_Prologue.IntroDialogueController.0"] = new[]
            {
                "Ithaca. My island, my home.",
                "Ítaca. Minha ilha, minha casa.",
            },

            ["dlg.Level_01_Itaca_Prologue.IntroDialogueController.1"] = new[]
            {
                "Penelope weaves in the hall, and Telemachus still fits in the crook of my arm.",
                "Penélope tece no salão, e Telêmaco ainda cabe no meu braço.",
            },

            ["dlg.Level_01_Itaca_Prologue.IntroDialogueController.2"] = new[]
            {
                "A quiet morning. Let me walk my kingdom before the day asks anything of me.",
                "Uma manhã tranquila. Vou caminhar pelo meu reino antes que o dia me peça alguma coisa.",
            },

            ["dlg.Level_01_Itaca_Prologue.HeraldDialogueController.0"] = new[]
            {
                "King Odysseus. I come from Mycenae.",
                "Rei Odisseu. Venho de Micenas.",
            },

            ["dlg.Level_01_Itaca_Prologue.HeraldDialogueController.1"] = new[]
            {
                "King Agamemnon is gathering the kings of Greece.",
                "O rei Agamenon reúne os reis da Grécia.",
            },

            ["dlg.Level_01_Itaca_Prologue.HeraldDialogueController.2"] = new[]
            {
                "Troy has challenged our honour. Every king must prepare his men.",
                "Troia desafiou nossa honra. Cada rei deve preparar seus homens.",
            },

            ["dlg.Level_01_Itaca_Prologue.HeraldDialogueController.3"] = new[]
            {
                "You have been summoned as well.",
                "Você também foi convocado.",
            },

            ["dlg.Level_01_Itaca_Prologue.HeraldDialogueController.4"] = new[]
            {
                "A war. After so many years of peace in Ithaca.",
                "Uma guerra. Depois de tantos anos de paz em Ítaca.",
            },

            ["dlg.Level_01_Itaca_Prologue.HeraldDialogueController.5"] = new[]
            {
                "How many men does Agamemnon expect me to bring?",
                "Quantos homens Agamenon espera que eu leve?",
            },

            ["dlg.Level_01_Itaca_Prologue.HeraldDialogueController.6"] = new[]
            {
                "Every man you can gather, my king.",
                "Todos os homens que puder reunir, meu rei.",
            },

            ["dlg.Level_01_Itaca_Prologue.HeraldDialogueController.7"] = new[]
            {
                "Then we have much to prepare.",
                "Então teremos muito a preparar.",
            },

            ["dlg.Level_01_Itaca_Prologue.MentorDialogueController.0"] = new[]
            {
                "The whole harbour heard it, Odysseus.",
                "O porto inteiro ouviu, Odisseu.",
            },

            ["dlg.Level_01_Itaca_Prologue.MentorDialogueController.1"] = new[]
            {
                "I swore an oath years ago. Oaths come due.",
                "Fiz um juramento anos atrás. Juramentos são cobrados.",
            },

            ["dlg.Level_01_Itaca_Prologue.MentorDialogueController.2"] = new[]
            {
                "It is not the war that weighs on me. It is who I leave behind.",
                "Não é a guerra que me pesa. É quem eu deixo para trás.",
            },

            ["dlg.Level_01_Itaca_Prologue.MentorDialogueController.3"] = new[]
            {
                "Then tell them yourself, before the island tells them for you.",
                "Então conte a eles você mesmo, antes que a ilha conte por você.",
            },

            ["dlg.Level_01_Itaca_Prologue.PenelopeDialogueController.0"] = new[]
            {
                "Is it true?",
                "É verdade?",
            },

            ["dlg.Level_01_Itaca_Prologue.PenelopeDialogueController.1"] = new[]
            {
                "Agamemnon has summoned the kings.",
                "Agamenon convocou os reis.",
            },

            ["dlg.Level_01_Itaca_Prologue.PenelopeDialogueController.2"] = new[]
            {
                "Then you are going.",
                "Então você vai.",
            },

            ["dlg.Level_01_Itaca_Prologue.PenelopeDialogueController.3"] = new[]
            {
                "I have to go.",
                "Preciso ir.",
            },

            ["dlg.Level_01_Itaca_Prologue.PenelopeDialogueController.4"] = new[]
            {
                "You always say Ithaca comes first.",
                "Você sempre diz que Ítaca vem primeiro.",
            },

            ["dlg.Level_01_Itaca_Prologue.PenelopeDialogueController.5"] = new[]
            {
                "And that is exactly why I must leave.",
                "E é exatamente por isso que preciso partir.",
            },

            ["dlg.Level_01_Itaca_Prologue.TelemacoDialogueController.0"] = new[]
            {
                "Father, are you going to fight monsters?",
                "Pai, você vai lutar contra monstros?",
            },

            ["dlg.Level_01_Itaca_Prologue.TelemacoDialogueController.1"] = new[]
            {
                "I hope to fight only men.",
                "Espero lutar apenas contra homens.",
            },

            ["dlg.Level_01_Itaca_Prologue.TelemacoDialogueController.2"] = new[]
            {
                "And when do you come back?",
                "E quando você volta?",
            },

            ["dlg.Level_01_Itaca_Prologue.TelemacoDialogueController.3"] = new[]
            {
                "As soon as the war is over.",
                "Assim que a guerra terminar.",
            },

            ["dlg.Level_01_Itaca_Prologue.PromiseDialogueController.0"] = new[]
            {
                "Swear it to me.",
                "Jure para mim.",
            },

            ["dlg.Level_01_Itaca_Prologue.PromiseDialogueController.1"] = new[]
            {
                "However long it takes.",
                "Não importa quanto tempo leve.",
            },

            ["dlg.Level_01_Itaca_Prologue.PromiseDialogueController.2"] = new[]
            {
                "I will come back to Ithaca.",
                "Eu voltarei para Ítaca.",
            },

            ["dlg.Level_01_Itaca_Prologue.PromiseDialogueController.3"] = new[]
            {
                "Then go. And keep that promise.",
                "Então vá. E cumpra essa promessa.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitFishermanDialogue.0"] = new[]
            {
                "I know every rock between here and the mainland, my king. Count me in.",
                "Conheço cada pedra daqui até o continente, meu rei. Conte comigo.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitFarmerDialogue.0"] = new[]
            {
                "The harvest can wait. Ithaca cannot.",
                "A colheita pode esperar. Ítaca não.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitSailorDialogue.0"] = new[]
            {
                "I know these waters better than any man alive.",
                "Conheço estas águas melhor que qualquer homem vivo.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitBlacksmithDialogue.0"] = new[]
            {
                "I am no soldier.",
                "Não sou soldado.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitBlacksmithDialogue.1"] = new[]
            {
                "But I can make sure your men have weapons.",
                "Mas posso garantir que seus homens tenham armas.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitEurylochusDialogue.0"] = new[]
            {
                "If you go, my king, I go.",
                "Se você vai, meu rei, eu também vou.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitEurylochusDialogue.1"] = new[]
            {
                "I have stood beside you before. I will do it again.",
                "Já estive ao seu lado antes. Estarei de novo.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitRowerDialogue.0"] = new[]
            {
                "Give me the rhythm and I will give you the sea.",
                "Me dê o ritmo e eu lhe dou o mar.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitShepherdDialogue.0"] = new[]
            {
                "My brother will watch the flock. I will watch your back.",
                "Meu irmão cuida do rebanho. Eu cuido das suas costas.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitWatchmanDialogue.0"] = new[]
            {
                "I have watched this coast for ten years. Let me watch a different one.",
                "Vigio esta costa há dez anos. Deixe-me vigiar outra.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitCarpenterDialogue.0"] = new[]
            {
                "A ship is only as good as the man who patches it.",
                "Um navio vale o quanto vale quem o remenda.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitElpenorDialogue.0"] = new[]
            {
                "I am the youngest here. That means I have the most to prove.",
                "Sou o mais novo daqui. Então sou quem mais tem a provar.",
            },

            ["dlg.Level_01_Itaca_Prologue.RecruitElpenorDialogue.1"] = new[]
            {
                "It means you stay close to me.",
                "Significa que você fica perto de mim.",
            },

            ["dlg.Level_01_Itaca_Prologue.MusterDialogueController.0"] = new[]
            {
                "The men of Ithaca are ready.",
                "Os homens de Ítaca estão prontos.",
            },

            ["dlg.Level_01_Itaca_Prologue.MusterDialogueController.1"] = new[]
            {
                "Ready is not the same as trained. To the training field.",
                "Prontos não é o mesmo que treinados. Ao campo de treino.",
            },

            ["dlg.Level_01_Itaca_Prologue.TrainerIntroDialogue.0"] = new[]
            {
                "A sword is not only for striking, my king.",
                "Uma espada não serve apenas para atacar, meu rei.",
            },

            ["dlg.Level_01_Itaca_Prologue.TrainerIntroDialogue.1"] = new[]
            {
                "Learn to control your movements first.",
                "Aprenda antes a controlar seus movimentos.",
            },

            ["dlg.Level_01_Itaca_Prologue.TrainerIntroDialogue.2"] = new[]
            {
                "The dummies do not hit back. The Trojans will.",
                "Os bonecos não revidam. Os troianos vão.",
            },

            ["dlg.Level_01_Itaca_Prologue.TrainerEndDialogue.0"] = new[]
            {
                "We will not fight with strength alone.",
                "Não lutaremos apenas com força.",
            },

            ["dlg.Level_01_Itaca_Prologue.TrainerEndDialogue.1"] = new[]
            {
                "We must fight as one.",
                "Precisamos lutar como um só.",
            },

            ["dlg.Level_01_Itaca_Prologue.TrainerEndDialogue.2"] = new[]
            {
                "They will follow you, my king. Every one of them.",
                "Eles vão seguir você, meu rei. Todos eles.",
            },

            ["dlg.Level_01_Itaca_Prologue.BlacksmithDialogueController.0"] = new[]
            {
                "Your blades are ready.",
                "Suas lâminas estão prontas.",
            },

            ["dlg.Level_01_Itaca_Prologue.BlacksmithDialogueController.1"] = new[]
            {
                "I have prepared shields for your men as well.",
                "Também preparei escudos para seus homens.",
            },

            ["dlg.Level_01_Itaca_Prologue.BlacksmithDialogueController.2"] = new[]
            {
                "And a few dozen arrows.",
                "E algumas dezenas de flechas.",
            },

            ["dlg.Level_01_Itaca_Prologue.BlacksmithDialogueController.3"] = new[]
            {
                "We will need every one of them.",
                "Vamos precisar de todas elas.",
            },

            ["dlg.Level_01_Itaca_Prologue.PortDialogueController.0"] = new[]
            {
                "Supplies aboard. Oars counted. Sails mended.",
                "Suprimentos a bordo. Remos contados. Velas remendadas.",
            },

            ["dlg.Level_01_Itaca_Prologue.PortDialogueController.1"] = new[]
            {
                "The ships of Ithaca are ready to sail.",
                "Os navios de Ítaca estão prontos para zarpar.",
            },

            ["dlg.Level_01_Itaca_Prologue.FarewellPenelopeDialogue.0"] = new[]
            {
                "Are the ships ready?",
                "Os navios estão prontos?",
            },

            ["dlg.Level_01_Itaca_Prologue.FarewellPenelopeDialogue.1"] = new[]
            {
                "They are.",
                "Estão.",
            },

            ["dlg.Level_01_Itaca_Prologue.FarewellPenelopeDialogue.2"] = new[]
            {
                "Then it is time.",
                "Então chegou a hora.",
            },

            ["dlg.Level_01_Itaca_Prologue.FarewellPenelopeDialogue.3"] = new[]
            {
                "I will come back.",
                "Eu voltarei.",
            },

            ["dlg.Level_01_Itaca_Prologue.FarewellPenelopeDialogue.4"] = new[]
            {
                "We will be waiting.",
                "Estaremos esperando.",
            },

            ["dlg.Level_01_Itaca_Prologue.FarewellTelemacoDialogue.0"] = new[]
            {
                "I made this for you.",
                "Fiz isto para você.",
            },

            ["dlg.Level_01_Itaca_Prologue.FarewellTelemacoDialogue.1"] = new[]
            {
                "It is Ithaca. So you do not forget the way home.",
                "É Ítaca. Para você não esquecer o caminho de casa.",
            },

            ["dlg.Level_01_Itaca_Prologue.FarewellTelemacoDialogue.2"] = new[]
            {
                "I will keep it close.",
                "Vou guardar bem perto.",
            },

            ["dlg.Level_01_Itaca_Prologue.FarewellTelemacoDialogue.3"] = new[]
            {
                "Look after your mother and the island for me. It is the harder of the two tasks.",
                "Cuide de sua mãe e da ilha por mim. É a tarefa mais difícil das duas.",
            },

            ["dlg.Level_01_Itaca_Prologue.BoardingDialogueController.0"] = new[]
            {
                "The men of Ithaca board. The equipment is loaded. The ships are ready.",
                "Os homens de Ítaca embarcam. Os equipamentos são carregados. Os navios estão prontos.",
            },

            ["dlg.Level_01_Itaca_Prologue.BoardingDialogueController.1"] = new[]
            {
                "Ithaca. Wait for me.",
                "Ítaca. Espere por mim.",
            },

            ["dlg.Level_01_Itaca_Prologue.OutroDialogueController.0"] = new[]
            {
                "Penelope watched from the harbour. Odysseus raised his hand, and the ships pulled away.",
                "Penélope observava do porto. Odisseu levantou a mão, e os navios se afastaram.",
            },

            ["dlg.Level_01_Itaca_Prologue.OutroDialogueController.1"] = new[]
            {
                "The ships left Ithaca. Ahead of them lay Troy.",
                "Os navios deixaram Ítaca. À frente estava Troia.",
            },

            ["dlg.Level_01_Itaca_Prologue.OutroDialogueController.2"] = new[]
            {
                "Odysseus did not yet know that this war would change his life forever.",
                "Odisseu ainda não sabia que aquela guerra mudaria sua vida para sempre.",
            },


            // ---------------------------------------------------------- Prólogo: portões fechados

            ["gate.prologue.hall"] = new[]
            {
                "The hall is closed. I should hear the herald from Mycenae first.",
                "O salão está fechado. Preciso ouvir antes o arauto de Micenas.",
            },

            ["gate.prologue.recruit"] = new[]
            {
                "Not yet. Penelope and Telemachus are back in the hall, and they deserve to hear it from me.",
                "Ainda não. Penélope e Telêmaco estão no salão, atrás de mim, e merecem ouvir isso de mim.",
            },

            ["gate.prologue.training"] = new[]
            {
                "The men of Ithaca are still scattered along the road behind me.",
                "Os homens de Ítaca ainda estão espalhados pelo caminho atrás de mim.",
            },

            ["gate.prologue.arsenal"] = new[]
            {
                "Not before the training field is done. The trainer is waiting behind me.",
                "Não antes de terminar o campo de treino. O treinador espera atrás de mim.",
            },

            ["gate.prologue.port"] = new[]
            {
                "The weapons are not ready. The blacksmith is back at the armoury.",
                "As armas não estão prontas. O ferreiro está no arsenal, atrás de mim.",
            },


            // ---------------------------------------------------------- Controle

            ["ctrl.gamepad.move"] = new[]
            {
                "Left Stick",
                "Analógico esquerdo",
            },

            ["ui.settings.vibration"] = new[]
            {
                "Controller vibration",
                "Vibração do controle",
            },


            ["tut.prologue.shieldBehind"] = new[]
            {
                "That one came from behind! Turn to face him — the shield only covers your front.",
                "Esse veio pelas costas! Vire-se para ele — o escudo só cobre a sua frente.",
            },

        };
    }
}
