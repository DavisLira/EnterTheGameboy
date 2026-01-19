using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro; // Se estiver usando TextMeshPro

public class CharacterSelectController : MonoBehaviour
{
    [Header("Database")]
    public CharacterDatabase database;

    [Header("UI")]
    public Transform content;
    public GameObject characterButtonPrefab;
    public Button confirmButton; 
    public TextMeshProUGUI confirmButtonText; 

    private NetworkRoomPlayerExt localRoomPlayer;

    void Start()
    {
        GenerateButtons();
        if(confirmButton) confirmButton.interactable = false;
    }

    void GenerateButtons()
    {
        // (Seu código de gerar botões continua igual...)
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
        // Procura o jogador local constantemente até achar
        if (localRoomPlayer == null)
        {
            var roomPlayers = FindObjectsByType<NetworkRoomPlayerExt>(FindObjectsSortMode.None);
            foreach (var player in roomPlayers)
            {
                if (player.isLocalPlayer) // É o MEU jogador?
                {
                    localRoomPlayer = player;
                    Debug.Log("PLAYER LOCAL ENCONTRADO! Habilitando botões.");
                    
                    // Libera o botão confirmar assim que achar o player
                    if(confirmButton) confirmButton.interactable = true;
                    
                    // Atualiza o texto do botão caso ele já tenha voltado de um jogo
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

        // Inverte o estado de Ready
        bool newReadyState = !localRoomPlayer.readyToBegin;
        localRoomPlayer.CmdChangeReadyState(newReadyState);
        
        // Atualiza texto visualmente (o Mirror vai confirmar depois)
        if(confirmButtonText) confirmButtonText.text = newReadyState ? "Aguardando..." : "Confirmar";
    }

    // --- A CORREÇÃO DO ERRO ESTÁ AQUI ---
    public void Back()
    {
        // NÃO use SceneManager.LoadScene("MainMenu");
        
        // Verifica se é Host ou Cliente e desliga corretamente.
        // O NetworkManager vai carregar a 'Offline Scene' (MainMenu) automaticamente.
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