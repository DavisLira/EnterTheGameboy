using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkRoomManager
{
    [Header("Referencias do Jogo")]
    public CharacterDatabase characterDatabase;

    // --- IMPEDE O INÍCIO AUTOMÁTICO ---
    public override void OnRoomServerPlayersReady()
    {
        // Deixamos vazio intencionalmente.
        // Assim, quando todos derem "Ready", o jogo NÃO muda de cena sozinho.
        // Ficamos esperando o Host clicar no botão.
        Debug.Log("Todos prontos! Aguardando Host iniciar...");
    }

    // --- FUNÇÃO MANUAL DO HOST ---
    public void HostStartGame()
    {
        // Verifica se realmente todos estão prontos (segurança extra)
        if (allPlayersReady)
        {
            ServerChangeScene(GameplayScene);
        }
    }

    // --- SEUS CÓDIGOS ANTERIORES MANTIDOS ---
    public override void OnStartHost()
    {
        base.OnStartHost();
        if (!string.IsNullOrEmpty(RoomScene))
        {
            ServerChangeScene(RoomScene);
        }
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        // 1. Host sempre entra (ID 0)
        if (conn.connectionId == 0)
        {
            base.OnServerConnect(conn);
            return;
        }

        // 2. Verifica se temos um save carregado
        if (GameSession.CurrentSave == null)
        {
            // Se não tem save (ex: teste no editor), deixa entrar ou chuta?
            // Vamos deixar entrar para debug.
            base.OnServerConnect(conn);
            return;
        }

        // 3. Pega o SteamID de quem está entrando
        // FizzySteamworks coloca o SteamID no campo address
        string incomingSteamID = conn.address; 
        Debug.Log($"[Auth] Jogador tentando entrar: {incomingSteamID}");

        // 4. Verifica Whitelist
        bool isAllowed = false;
        
        // A. Está na lista?
        foreach (string allowed in GameSession.CurrentSave.allowedSteamIds)
        {
            if (allowed == incomingSteamID) isAllowed = true;
        }

        // B. Regra do "Mundo Novo / Aberto"
        // Se só tem o Host na lista, significa que é um jogo novo ou aberto a amigos
        // (Você pode criar uma flag no banco "isPrivate" se quiser mudar isso depois)
        if (GameSession.CurrentSave.allowedSteamIds.Length <= 1)
        {
            isAllowed = true;
            // IMPORTANTE: Adiciona ele na API para que ele tenha permissão futura
            // Precisamos rodar numa Coroutine, mas NetworkManager não é bom pra isso se a cena mudar.
            // O ideal é o APIService fazer isso.
            APIService.instance.StartCoroutine(
                APIService.instance.AddPlayerToSave(GameSession.CurrentSave._id, incomingSteamID, null, null)
            );
        }

        if (isAllowed)
        {
            Debug.Log("[Auth] Acesso PERMITIDO.");
            base.OnServerConnect(conn);
        }
        else
        {
            Debug.LogWarning("[Auth] Acesso NEGADO. Jogador não está na Whitelist.");
            conn.Disconnect();
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        string currentScene = SceneManager.GetActiveScene().path;
        if (currentScene == RoomScene || currentScene == GameplayScene)
        {
            base.OnServerAddPlayer(conn);
        }
    }

    public override void OnRoomClientSceneChanged()
    {
        base.OnRoomClientSceneChanged();
        if (NetworkClient.active && NetworkClient.localPlayer == null)
        {
            NetworkClient.AddPlayer(); 
        }
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        NetworkRoomPlayerExt lobbyPlayer = roomPlayer.GetComponent<NetworkRoomPlayerExt>();
        Transform startPos = GetStartPosition();

        GameObject gamePlayer = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        PlayerModelLoader loader = gamePlayer.GetComponent<PlayerModelLoader>();
        if (loader != null)
        {
            loader.SetCharacter(lobbyPlayer.characterIndex);
        }

        return gamePlayer;
    }
}