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
        string currentScene = SceneManager.GetActiveScene().path;
        
        if (currentScene == RoomScene)
        {
            OnRoomServerConnect(conn); 
            return; 
        }

        if (currentScene != RoomScene)
        {
            if (conn.connectionId == 0)
            {
                OnRoomServerConnect(conn);
                return; 
            }
        }
        base.OnServerConnect(conn);
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