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
                "Press E to talk",
                "Pressione E para conversar",
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

            ["dlg.Level_01_Itaca_Prologue.TelemacoDialogueController.0"] = new[]
            {
                "Father, is it true you will cross the sea? Take me with you!",
                "Pai, é verdade que você vai atravessar o mar? Me leve junto!",
            },

            ["dlg.Level_01_Itaca_Prologue.TelemacoDialogueController.1"] = new[]
            {
                "Look after your mother and the island for me, my son. It is the harder of the two tasks.",
                "Cuide de sua mãe e da ilha por mim, meu filho. É a tarefa mais difícil das duas.",
            },

            ["dlg.Level_01_Itaca_Prologue.PenelopeDialogueController.0"] = new[]
            {
                "They say Agamemnon's heralds came for kings to fight a war in Troy. Do not go, Odysseus.",
                "Dizem que os arautos de Agamenon vieram buscar reis para uma guerra em Troia. Não vá, Odisseu.",
            },

            ["dlg.Level_01_Itaca_Prologue.PenelopeDialogueController.1"] = new[]
            {
                "If you go, swear you will return. I will wait as long as it takes — but swear it.",
                "Se for, jure que volta. Eu espero o tempo que for preciso — mas jure.",
            },

            ["dlg.Level_01_Itaca_Prologue.OutroDialogueController.0"] = new[]
            {
                "The oath binds me: I swore to defend the house of Menelaus, and Agamemnon has come to collect.",
                "O juramento me obriga: prometi defender a casa de Menelau, e Agamenon cobra a promessa.",
            },

            ["dlg.Level_01_Itaca_Prologue.OutroDialogueController.1"] = new[]
            {
                "Odysseus sets sail with his men. Ithaca falls away behind them — and the war at Troy waits on the far side of the sea.",
                "Odisseu embarca com seus homens. Ítaca fica para trás — e a guerra de Troia espera do outro lado do mar.",
            },

            ["dlg.Level_01_Itaca_Prologue.IntroDialogueController.0"] = new[]
            {
                "Ithaca. My island, my home. Penelope weaves in the hall, and Telemachus still fits in the crook of my arm.",
                "Ítaca. Minha ilha, minha casa. Penélope tece no salão e Telêmaco ainda cabe no meu braço.",
            },

            ["dlg.Level_01_Itaca_Prologue.IntroDialogueController.1"] = new[]
            {
                "A ship from Mycenae docked in the harbour this morning. Agamemnon is summoning every king in Greece.",
                "Um navio de Micenas ancorou no porto esta manhã. Agamenon manda chamar todos os reis da Grécia.",
            },

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

            ["tut.Level_01_Itaca_Prologue.Tutorial_Move"] = new[]
            {
                "Use A/D or ←/→ to move.",
                "Use A/D ou ←/→ para se mover.",
            },

            ["tut.Level_01_Itaca_Prologue.Tutorial_Jump"] = new[]
            {
                "Press SPACE to jump the gap.",
                "Pressione SPACE para pular o vao.",
            },

            ["tut.Level_01_Itaca_Prologue.Tutorial_Attack"] = new[]
            {
                "Press Z to attack.",
                "Pressione Z para atacar.",
            },

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

        };
    }
}
