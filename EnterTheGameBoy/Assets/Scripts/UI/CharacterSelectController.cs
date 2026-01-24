using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Collections.Generic; // Necessário para Listas

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
    public TextMeshProUGUI startGameButtonText; // <--- ARRASTE O TEXTO DENTRO DO BOTÃO INICIAR AQUI

    // Lista para controlar os botões visuais
    private List<CharacterButtonUI> spawnedButtons = new List<CharacterButtonUI>();
    private NetworkRoomPlayerExt localRoomPlayer;

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
        // Limpa antigos...
        foreach (Transform child in content) Destroy(child.gameObject);
        spawnedButtons.Clear();
        
        for (int i = 0; i < database.characters.Length; i++)
        {
            GameObject buttonObj = Instantiate(characterButtonPrefab, content);
            CharacterButtonUI buttonUI = buttonObj.GetComponent<CharacterButtonUI>();
            
            if (buttonUI != null) 
            {
                // AQUI: database.characters[i] deve ser do tipo CharacterData
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
            // Percorre todos os botões e pinta apenas o que corresponde ao index escolhido
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                bool isSelected = (localRoomPlayer.characterIndex == i);
                spawnedButtons[i].SetSelectedState(isSelected);
            }

            // --- 2. LÓGICA DO BOTÃO CONFIRMAR ---
            bool temPersonagem = localRoomPlayer.characterIndex != -1;
            if(confirmButton) confirmButton.interactable = temPersonagem;

            // --- 3. LÓGICA DO HOST (Botão Iniciar + Contador) ---
            if (NetworkServer.active && localRoomPlayer.index == 0)
            {
                if(startGameButton)
                {
                    startGameButton.gameObject.SetActive(true);
                    
                    var manager = NetworkManager.singleton as MyNetworkManager;
                    if(manager != null)
                    {
                        // Habilita se todos estiverem prontos
                        startGameButton.interactable = manager.allPlayersReady;

                        // --- ATUALIZA O TEXTO DO CONTADOR (X/N) ---
                        if(startGameButtonText != null)
                        {
                            // Conta quantos jogadores existem na sala
                            int totalPlayers = manager.roomSlots.Count;
                            
                            // Conta quantos estão Ready
                            int readyPlayers = 0;
                            foreach(var slot in manager.roomSlots)
                            {
                                if(slot != null && slot.readyToBegin)
                                    readyPlayers++;
                            }

                            // Formata o texto: "INICIAR (3/4)"
                            startGameButtonText.text = $"INICIAR   ({readyPlayers}/{totalPlayers})";
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
        if (localRoomPlayer == null) return;

        bool newState = !localRoomPlayer.readyToBegin;
        localRoomPlayer.CmdChangeReadyState(newState);
        
        if(confirmButtonText) confirmButtonText.text = newState ? "AGUARDANDO..." : "CONFIRMAR";
    }
    
    public void OnClickStartGame()
    {
        var manager = NetworkManager.singleton as MyNetworkManager;
        if (manager != null) manager.HostStartGame();
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
}