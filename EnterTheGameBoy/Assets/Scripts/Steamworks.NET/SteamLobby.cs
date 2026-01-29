using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    public static CSteamID CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";

    // Adicione referência ao manager para facilitar
    private NetworkManager manager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        manager = NetworkManager.singleton; // Pega referência
        if (!SteamManager.Initialized) return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        // Proteção Extra: Se já estiver rodando, para tudo antes de criar novo
        if (NetworkServer.active || NetworkClient.active)
        {
             manager.StopHost();
        }

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) return;

        // Proteção Crítica: Se o Mirror não desligou a tempo, força agora
        if (NetworkServer.active || NetworkClient.active)
        {
            manager.StopHost();
        }

        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        
        if (GameSession.CurrentSave != null)
        {
            SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "SaveID", GameSession.CurrentSave._id);
        }
        
        CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
    }

    // ... (Mantenha OnGameLobbyJoinRequested e OnLobbyEntered como estavam) ...
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return;

        CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        
        manager.networkAddress = hostAddress;
        manager.StartClient();
    }
}