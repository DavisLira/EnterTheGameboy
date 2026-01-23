using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro; // Necessário para TextMeshPro
using Steamworks; // <--- ADICIONE ISSO (Necessário para Steamworks.CSteamID)

public class CharacterSelectController : MonoBehaviour
{
    [Header("Database")]
    public CharacterDatabase database;

    [Header("UI")]
    public Transform content;
    public GameObject characterButtonPrefab;
    public Button confirmButton; 
    public TextMeshProUGUI confirmButtonText;
    public TextMeshProUGUI roomCodeText;

    private NetworkRoomPlayerExt localRoomPlayer;

    void Start()
    {
        // Verifica se o SteamLobby existe e se tem um ID válido
        if (SteamLobby.Instance != null && SteamLobby.CurrentLobbyID != CSteamID.Nil)
        {
             // MODO STEAM
             // Verifica se roomCodeText foi arrastado no inspector para evitar erro null
             if(roomCodeText != null) 
                 roomCodeText.text = "ID STEAM: " + SteamLobby.CurrentLobbyID.ToString();
        }
        else
        {
             // MODO LOCAL
             if(roomCodeText != null) 
                 roomCodeText.text = "MODO LOCAL (LAN)";
        }
        
        GenerateButtons();
        if(confirmButton) confirmButton.interactable = false;
    }

    void GenerateButtons()
    {
        foreach (Transform child in content) Destroy(child.gameObject);
        
        for (int i = 0; i < database.characters.Length; i++)
        {
            GameObject buttonObj = Instantiate(characterButtonPrefab, content);
            CharacterButtonUI buttonUI = buttonObj.GetComponent<CharacterButtonUI>();
            if (buttonUI != null) buttonUI.Setup(database.characters[i], this, i);
        }
    }

    void Update()
    {
        if (localRoomPlayer == null)
        {
            var roomPlayers = FindObjectsByType<NetworkRoomPlayerExt>(FindObjectsSortMode.None);
            foreach (var player in roomPlayers)
            {
                // isLocalPlayer é mais seguro que hasAuthority nesse contexto, mas ambos funcionam
                if (player.isLocalPlayer) 
                {
                    localRoomPlayer = player;
                    Debug.Log("PLAYER LOCAL ENCONTRADO! Habilitando botões.");
                    
                    if(confirmButton) confirmButton.interactable = true;
                    
                    UpdateConfirmButtonText(); 
                    break;
                }
            }
        }
    }

    void UpdateConfirmButtonText()
    {
        if (confirmButtonText && localRoomPlayer)
        {
            confirmButtonText.text = localRoomPlayer.readyToBegin ? "Aguardando..." : "Confirmar";
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

        bool newReadyState = !localRoomPlayer.readyToBegin;
        localRoomPlayer.CmdChangeReadyState(newReadyState);
        
        if(confirmButtonText) confirmButtonText.text = newReadyState ? "Aguardando..." : "Confirmar";
    }

    public void Back()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }
}