using UnityEngine;
using Mirror;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject joinPanel; // Antigo connectionPanel (renomeie na Unity)
    public GameObject loadingPanel;
    
    // Referência ao novo sistema
    public SaveManager saveManager; 

    [Header("Inputs")]
    public TMP_InputField codeInput;

    void Start()
    {
        ShowMainPanel();
    }

    public void ShowMainPanel()
    {
        if(mainPanel) mainPanel.SetActive(true);
        if(joinPanel) joinPanel.SetActive(false);
        if(saveManager) saveManager.saveSelectionPanel.SetActive(false);
        if(loadingPanel) loadingPanel.SetActive(false);
    }

    // --- 1. HOST (JOGAR / CRIAR MUNDO) ---
    // Esse botão "Jogar" agora abre a seleção de Saves
    public void OnClickHostGame()
    {
        mainPanel.SetActive(false);
        
        // Chama o SaveManager para buscar os dados e abrir a tela dele
        saveManager.OpenSaveMenu();
    }

    // --- 2. CLIENT (ENTRAR EM SALA DE AMIGO) ---
    // Esse botão abre aquele painel de digitar código
    public void OnClickJoinGame()
    {
        mainPanel.SetActive(false);
        joinPanel.SetActive(true);
    }

    public void OnBack()
    {
        ShowMainPanel();
    }

    // --- 3. CONFIRMAR ENTRADA (BOTÃO NO JOIN PANEL) ---
    public void OnConfirmJoin()
    {
        string code = codeInput.text;
        
        if (string.IsNullOrEmpty(code)) return;

        loadingPanel.SetActive(true);

        // Lógica Steam
        if (SteamLobby.Instance != null)
        {
            if (ulong.TryParse(code, out ulong steamId))
            {
                Steamworks.SteamMatchmaking.JoinLobby(new Steamworks.CSteamID(steamId));
            }
        }
        // Lógica Local (IP)
        else
        {
            NetworkManager.singleton.networkAddress = code;
            NetworkManager.singleton.StartClient();
        }
    }

    public void OnExit()
    {
        Application.Quit();
    }
    
    // OBS: A função OnCreateRoom() antiga foi DELETADA.
    // Quem cria a sala agora é o SaveManager -> StartGameWithSave -> SteamLobby.HostLobby()
}