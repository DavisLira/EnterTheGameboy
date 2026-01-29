using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Collections.Generic; // Necessário para Listas
using System.Linq; // <--- ADICIONADO: Necessário para usar .Distinct() na lista de IDs

public class CharacterSelectController : MonoBehaviour
{
    [Header("Database")]
    public CharacterDatabase database;

    [Header("UI")]
    public Transform content;
    public GameObject characterButtonPrefab;
    
    [Header("Botões")]
    public Button confirmButton; 
    public TextMeshProUGUI confirmButtonText;
    public TextMeshProUGUI roomCodeText;
    public Button startGameButton; 
    public TextMeshProUGUI startGameButtonText;

    // Lista para controlar os botões visuais
    private List<CharacterButtonUI> spawnedButtons = new List<CharacterButtonUI>();
    private NetworkRoomPlayerExt localRoomPlayer;

    public bool HasSelectedCharacter = false;

    void Start()
    {
        // Configuração do Texto Steam/Local
        if (SteamLobby.Instance != null && SteamLobby.CurrentLobbyID != CSteamID.Nil)
        {
             if(roomCodeText != null) roomCodeText.text = SteamLobby.CurrentLobbyID.ToString();
        }
        else
        {
             if(roomCodeText != null) roomCodeText.text = "SALA LOCAL";
        }
        
        GenerateButtons();

        if(confirmButton) confirmButton.interactable = false;
        if(startGameButton) startGameButton.gameObject.SetActive(false);
    }

    void GenerateButtons()
    {
        foreach (Transform child in content) Destroy(child.gameObject);
        spawnedButtons.Clear();
        
        for (int i = 0; i < database.characters.Length; i++)
        {
            GameObject buttonObj = Instantiate(characterButtonPrefab, content);
            CharacterButtonUI buttonUI = buttonObj.GetComponent<CharacterButtonUI>();
            
            if (buttonUI != null) 
            {
                buttonUI.Setup(database.characters[i], this, i);
                spawnedButtons.Add(buttonUI);
            }
        }
    }

    void Update()
    {
        // Acha o player local
        if (localRoomPlayer == null)
        {
            foreach (var player in FindObjectsByType<NetworkRoomPlayerExt>(FindObjectsSortMode.None))
            {
                if (player.isLocalPlayer) 
                {
                    localRoomPlayer = player;
                    UpdateConfirmButtonText(); 
                    break;
                }
            }
        }

        if (localRoomPlayer != null)
        {
            // --- 1. ATUALIZA A COR DOS BOTÕES ---
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                bool isSelected = localRoomPlayer.characterIndex == i;
                if (!HasSelectedCharacter) isSelected = false; 
                spawnedButtons[i].SetSelectedState(isSelected);
            }

            // --- 2. LÓGICA DO BOTÃO CONFIRMAR ---
            if(confirmButton) confirmButton.interactable = HasSelectedCharacter;

            // --- 3. LÓGICA DO HOST (Botão Iniciar + Contador) ---
            if (NetworkServer.active && localRoomPlayer.index == 0)
            {
                if(startGameButton)
                {
                    startGameButton.gameObject.SetActive(true);
                    
                    var manager = NetworkManager.singleton as MyNetworkManager;
                    if(manager != null)
                    {
                        // Só habilita se todos estiverem prontos (Manager cuida disso)
                        startGameButton.interactable = manager.allPlayersReady;

                        // --- ATUALIZA O TEXTO DO CONTADOR (X/N) ---
                        if(startGameButtonText != null)
                        {
                            int totalPlayers = manager.roomSlots.Count;
                            int readyPlayers = 0;
                            foreach(var slot in manager.roomSlots)
                            {
                                if(slot != null && slot.readyToBegin)
                                    readyPlayers++;
                            }

                            // Se estiver salvando, mostra outra mensagem
                            if (isSaving)
                            {
                                startGameButtonText.text = "SALVANDO...";
                            }
                            else
                            {
                                startGameButtonText.text = $"INICIAR   ({readyPlayers}/{totalPlayers})";
                            }
                        }
                    }
                }
            }
            else
            {
                if(startGameButton) startGameButton.gameObject.SetActive(false);
            }
        }
    }

    void UpdateConfirmButtonText()
    {
        if (confirmButtonText && localRoomPlayer)
        {
            confirmButtonText.text = localRoomPlayer.readyToBegin ? "AGUARDANDO..." : "CONFIRMAR";
        }
    }

    public void OnSelectCharacter(int index)
    {
        if (localRoomPlayer != null && !localRoomPlayer.readyToBegin)
        {
            localRoomPlayer.CmdSelectCharacter(index);
        }
    }

    public void Confirm()
    {
        if (localRoomPlayer == null || !HasSelectedCharacter) return;

        bool newState = !localRoomPlayer.readyToBegin;
        localRoomPlayer.CmdChangeReadyState(newState);
        
        if(confirmButtonText) confirmButtonText.text = newState ? "AGUARDANDO..." : "CONFIRMAR";
    }

    public void Back()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
            NetworkManager.singleton.StopHost();
        else
            NetworkManager.singleton.StopClient();
    }

    public void OnCopyCode()
    {
        if (roomCodeText != null) GUIUtility.systemCopyBuffer = roomCodeText.text;
    }

    // --- NOVA LÓGICA DE INICIAR COM WHITELIST ---
    
    private bool isSaving = false; // Flag para impedir duplo clique

    public void OnClickStartGame()
    {
        var manager = NetworkManager.singleton as MyNetworkManager;
        if (manager == null || !NetworkServer.active) return; // Segurança

        // Se não tiver Save carregado (ex: teste direto na cena), inicia direto
        if (GameSession.CurrentSave == null)
        {
            Debug.LogWarning("Nenhum Save carregado na GameSession. Iniciando sem Whitelist.");
            manager.HostStartGame();
            return;
        }

        // Evita clicar duas vezes enquanto salva
        if (isSaving) return;
        isSaving = true;
        startGameButton.interactable = false; // Trava o botão visualmente

        // 1. Coletar os SteamIDs de todos conectados AGORA
        List<string> currentPlayers = new List<string>();

        // Adiciona o Host
        currentPlayers.Add(SteamUser.GetSteamID().ToString());

        // Adiciona os Clientes
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            // O Host (ID 0) já foi adicionado manualmente acima.
            // conn.address no FizzySteamworks contém o SteamID do cliente
            if (conn.connectionId != 0 && !string.IsNullOrEmpty(conn.address)) 
            {
                currentPlayers.Add(conn.address);
            }
        }

        // Remove duplicatas (using System.Linq)
        currentPlayers = currentPlayers.Distinct().ToList();

        Debug.Log($"[Host] Atualizando Whitelist no Banco: {string.Join(", ", currentPlayers)}");

        // 2. Chama a API
        string mySteamId = SteamUser.GetSteamID().ToString();
        string saveId = GameSession.CurrentSave._id;

        StartCoroutine(APIService.instance.UpdateSaveWhitelist(saveId, mySteamId, currentPlayers,
            () => {
                // SUCESSO!
                Debug.Log("Whitelist salva! Iniciando partida...");
                
                // Atualiza a memória local (para reconexão funcionar imediatamente se alguém cair)
                GameSession.CurrentSave.allowedSteamIds = currentPlayers.ToArray();

                // 3. Inicia o Jogo (Muda de Cena)
                manager.HostStartGame();
                isSaving = false;
            },
            (err) => {
                // ERRO
                Debug.LogError("Erro ao salvar whitelist: " + err);
                
                // Destrava o botão para tentar de novo
                isSaving = false;
                startGameButton.interactable = true;
                if(startGameButtonText) startGameButtonText.text = "ERRO - TENTAR DNV";
            }
        ));
    }
}