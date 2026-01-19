using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkRoomManager
{
    [Header("Referencias do Jogo")]
    public CharacterDatabase characterDatabase;

    // --- 1. Força a mudança de cena ao iniciar o Host ---
    public override void OnStartHost()
    {
        base.OnStartHost();
        if (!string.IsNullOrEmpty(RoomScene))
        {
            ServerChangeScene(RoomScene);
        }
    }

    // --- 2. Permite conectar (Anti-Kick) ---
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        string currentScene = SceneManager.GetActiveScene().path;
        
        // Debug para garantir que estamos vendo o que o Mirror vê
        Debug.Log($"[DEBUG CONEXÃO] Cliente {conn.connectionId} conectando. Cena Atual: '{currentScene}' | RoomScene Configurada: '{RoomScene}'");

        // CASO 1: O Servidor já está na Sala (Situação normal de alguém entrando)
        if (currentScene == RoomScene)
        {
            Debug.Log("[DEBUG] Servidor já está na sala. Aceitando cliente manualmente.");
            
            // AQUI ESTÁ A MÁGICA: Chamamos o evento da sala direto e pulamos a verificação chata do base
            OnRoomServerConnect(conn); 
            return; 
        }

        // CASO 2: O Servidor ainda está no Menu/Carregando (Situação do Host criando)
        if (currentScene != RoomScene)
        {
            if (conn.connectionId == 0) // É o Host
            {
                Debug.Log("[DEBUG] Host conectando durante carregamento. Permitindo...");
                OnRoomServerConnect(conn);
                return; 
            }
        }

        // Se não for nenhum dos casos acima, deixa o Mirror decidir (provavelmente vai desconectar se estiver no jogo)
        base.OnServerConnect(conn);
    }

    // --- 3. Filtra tentativas de Spawn na hora errada ---
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        string currentScene = SceneManager.GetActiveScene().path;

        // Só permite criar o boneco se estivermos na cena da Sala ou do Jogo
        if (currentScene == RoomScene || currentScene == GameplayScene)
        {
            base.OnServerAddPlayer(conn);
        }
        else
        {
            Debug.Log($"[DEBUG] Segurando spawn do player até a cena carregar...");
        }
    }

    // --- 4. O GATILHO: Cria o Player quando a cena carrega ---
    public override void OnRoomClientSceneChanged()
    {
        base.OnRoomClientSceneChanged();

        // Verificamos se estamos conectados e sem player
        if (NetworkClient.active && NetworkClient.localPlayer == null)
        {
            Debug.Log("[DEBUG] Cena carregada! Solicitando criação MANUAL do Player.");
            
            // REMOVEMOS O NetworkClient.Ready();
            // O Mirror já cuidou disso. Só pedimos para criar o boneco:
            
            NetworkClient.AddPlayer(); 
        }
    }

    // --- 5. Transforma RoomPlayer em GamePlayer ---
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