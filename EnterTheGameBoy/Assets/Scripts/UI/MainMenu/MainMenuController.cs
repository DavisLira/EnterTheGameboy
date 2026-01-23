using UnityEngine;
using Mirror;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject connectionPanel;

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
        // CASO 1: Estamos rodando via STEAM (Bootstrap Steam carregou o SteamLobby)
        if (SteamLobby.Instance != null)
        {
            Debug.Log("Criando sala via STEAM...");
            SteamLobby.Instance.HostLobby();
        }
        // CASO 2: Estamos rodando LOCAL (SteamLobby é null)
        else
        {
            Debug.Log("Criando sala via LOCAL (LAN/ParrelSync)...");
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