using UnityEngine;
using Mirror;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject connectionPanel;
    public GameObject loadingPanel;

    [Header("Inputs")]
    public TMP_InputField codeInput;

    void Start()
    {
        ShowMainPanel();
    }

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        connectionPanel.SetActive(false);
    }

    public void OnPlay()
    {
        mainPanel.SetActive(false);
        connectionPanel.SetActive(true);
    }

    public void OnBack()
    {
        ShowMainPanel();
    }

    // --- LÓGICA HÍBRIDA (AQUI ESTÁ A MÁGICA) ---

    public void OnCreateRoom()
    {
        // 1. Ativa o loading imediatamente para o usuário saber que clicou
        if(loadingPanel) loadingPanel.SetActive(true); 
        mainPanel.SetActive(false); // Esconde os botões para ele não clicar 2x

        if (SteamLobby.Instance != null)
        {
            Debug.Log("Criando sala via STEAM...");
            SteamLobby.Instance.HostLobby();
        }
        else
        {
            Debug.Log("Criando sala via LOCAL...");
            NetworkManager.singleton.StartHost();
        }
    }

    public void OnJoinRoom()
    {
        // CASO 1: STEAM
        // Na Steam, geralmente entramos via convite, mas se você implementou input de ID:
        if (SteamLobby.Instance != null)
        {
            // Lógica para entrar via ID da Steam (se necessário)
            string code = codeInput.text;
            if (!string.IsNullOrEmpty(code) && ulong.TryParse(code, out ulong steamId))
            {
                Steamworks.SteamMatchmaking.JoinLobby(new Steamworks.CSteamID(steamId));
            }
        }
        // CASO 2: LOCAL (IP / Localhost)
        else
        {
            string address = "localhost";
            if (!string.IsNullOrEmpty(codeInput.text))
            {
                address = codeInput.text;
            }
            
            NetworkManager.singleton.networkAddress = address;
            NetworkManager.singleton.StartClient();
        }
    }

    public void OnExit()
    {
        Application.Quit();
    }
}