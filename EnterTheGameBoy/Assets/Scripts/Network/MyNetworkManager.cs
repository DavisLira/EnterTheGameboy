using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MyNetworkManager : NetworkRoomManager
{
    [Header("Base de Dados")]
    public CharacterDatabase characterDatabase;

    [Header("Configuração de Fases")]
    // IMPORTANTE: Escreva os nomes EXATOS das cenas aqui no Inspector: "Game", "Game 1", "Game 2"
    public List<string> levelScenes; 

    // Memória das escolhas (Personagem)
    public static Dictionary<int, int> playerSelections = new Dictionary<int, int>();
    
    // Memória das Armas (ConnectionID -> WeaponID)
    public static Dictionary<int, int> playerWeaponStates = new Dictionary<int, int>();

    // =========================================================
    //              1. INÍCIO DO JOGO
    // =========================================================

    public override void OnRoomServerPlayersReady()
    {
        Debug.Log("Todos prontos! Aguardando Host iniciar...");
    }

    public void HostStartGame()
    {
        if (!allPlayersReady)
        {
            Debug.LogWarning("[Manager] Não foi possível iniciar: Jogadores não estão prontos.");
            return;
        }

        string firstSceneName = GameplayScene; 

        if (levelScenes != null && levelScenes.Count > 0)
        {
            firstSceneName = levelScenes[0];
        }

        Debug.Log($"[Manager] Host iniciou o jogo! Indo para: {firstSceneName}");
        ServerChangeScene(firstSceneName);
    }

    // =========================================================
    //              2. TRANSIÇÃO DE FASES (Portal)
    // =========================================================

    public void GoToNextLevel()
    {
        SaveAllPlayersWeapons();

        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentIndex = levelScenes.IndexOf(currentSceneName);
        int nextIndex = currentIndex + 1;

        if (nextIndex < levelScenes.Count)
        {
            string nextScene = levelScenes[nextIndex];
            Debug.Log($"[Manager] Portal ativado! Indo para: {nextScene}");
            ServerChangeScene(nextScene);
        }
        else
        {
            Debug.Log("[Manager] FIM DE JOGO!");
            ServerChangeScene("MainMenu"); 
            StopHost();
        }
    }

    void SaveAllPlayersWeapons()
    {
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                var weaponController = conn.identity.GetComponent<PlayerWeaponController>();
                if (weaponController != null)
                {
                    if (playerWeaponStates.ContainsKey(conn.connectionId))
                        playerWeaponStates[conn.connectionId] = weaponController.currentWeaponID; 
                    else
                        playerWeaponStates.Add(conn.connectionId, weaponController.currentWeaponID);
                }
            }
        }
    }

    // =========================================================
    //              3. RESPAWN (SERVER SIDE)
    // =========================================================

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        string sceneName = SceneManager.GetActiveScene().name;

        // Se for a PRIMEIRA FASE: NÃO fazemos nada manual. O RoomManager cria sozinho.
        if (levelScenes.Count > 0 && sceneName == levelScenes[0]) return;
        if (sceneName == GameplayScene) return;

        // SE FOR FASE 2, 3... (Avançadas)
        if (levelScenes.Contains(sceneName))
        {
            Debug.Log($"[Manager] Cliente {conn.connectionId} pronto na fase {sceneName}. Spawnando...");
            RespawnPlayerInNewScene(conn);
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        // Vazio intencionalmente para não conflitar com OnServerReady
    }

    void RespawnPlayerInNewScene(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        Vector3 pos = startPos != null ? startPos.position : Vector3.zero;
        Quaternion rot = startPos != null ? startPos.rotation : Quaternion.identity;

        // 1. Personagem
        int charIndex = 0;
        if (playerSelections.ContainsKey(conn.connectionId))
            charIndex = playerSelections[conn.connectionId];

        // 2. Cria Corpo
        GameObject newPlayer = Instantiate(playerPrefab, pos, rot);
        
        // 3. Skin
        PlayerModelLoader loader = newPlayer.GetComponent<PlayerModelLoader>();
        if (loader != null) loader.SetCharacter(charIndex);

        // 4. Arma
        PlayerWeaponController weaponCtrl = newPlayer.GetComponent<PlayerWeaponController>();
        if (weaponCtrl != null && playerWeaponStates.ContainsKey(conn.connectionId))
        {
            int savedWeaponID = playerWeaponStates[conn.connectionId];
            weaponCtrl.ForceWeaponInit(savedWeaponID); 
        }

        // Verifica se o objeto antigo existe antes de substituir
        if (conn.identity != null)
        {
            NetworkServer.ReplacePlayerForConnection(conn, newPlayer, ReplacePlayerOptions.Unspawn);
        }
        else
        {
            NetworkServer.AddPlayerForConnection(conn, newPlayer);
        }
    }

    // =========================================================
    //              4. CLIENT SIDE (A CORREÇÃO DO LOBBY)
    // =========================================================

    public override void OnRoomClientSceneChanged()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Verifica se é uma fase avançada (Game 1, Game 2...)
        // Lógica: Está na lista de fases MAS NÃO é a primeira.
        bool isAdvancedStage = levelScenes.Contains(sceneName) && 
                               (levelScenes.Count > 0 && sceneName != levelScenes[0]);

        // Se for fase avançada, NÃO pede spawn (o servidor vai mandar manual)
        if (isAdvancedStage)
        {
            return;
        }

        // Se for LOBBY, MENU ou PRIMEIRA FASE, age normalmente (pede spawn)
        // Isso conserta o bug de não conseguir selecionar personagem
        base.OnRoomClientSceneChanged();
        if (NetworkClient.active && NetworkClient.localPlayer == null)
        {
            NetworkClient.AddPlayer();
        }
    }

    // =========================================================
    //              5. OUTROS OVERRIDES NECESSÁRIOS
    // =========================================================

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        NetworkRoomPlayerExt lobbyPlayer = roomPlayer.GetComponent<NetworkRoomPlayerExt>();
        int index = lobbyPlayer.characterIndex;

        if (playerSelections.ContainsKey(conn.connectionId))
            playerSelections[conn.connectionId] = index;
        else
            playerSelections.Add(conn.connectionId, index);

        Transform startPos = GetStartPosition();
        GameObject gamePlayer = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        PlayerModelLoader loader = gamePlayer.GetComponent<PlayerModelLoader>();
        if (loader != null) loader.SetCharacter(index);

        if (playerWeaponStates.ContainsKey(conn.connectionId)) 
             playerWeaponStates.Remove(conn.connectionId);

        return gamePlayer;
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (conn.connectionId == 0) return;
        string currentScene = SceneManager.GetActiveScene().name;
        
        if (!levelScenes.Contains(currentScene)) return; 
        if (GameSession.CurrentSave == null) return;

        bool isAllowed = false;
        if (GameSession.CurrentSave.allowedSteamIds != null)
        {
            foreach (string allowed in GameSession.CurrentSave.allowedSteamIds)
                if (allowed == conn.address) isAllowed = true;
        }

        if (!isAllowed) conn.Disconnect();
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        if (!string.IsNullOrEmpty(RoomScene))
        {
            ServerChangeScene(RoomScene);
        }
    }
    
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
    }
}