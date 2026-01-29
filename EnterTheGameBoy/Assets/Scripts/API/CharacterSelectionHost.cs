using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq; // Importante para mexer com listas
using Steamworks;

public class CharacterSelectionHost : NetworkBehaviour
{
    public void OnClickStartGame()
    {
        // Só o Host pode clicar
        if (!isServer) return;

        // 1. Coletar os SteamIDs de todos conectados AGORA
        List<string> currentPlayers = new List<string>();

        // Adiciona o Host
        currentPlayers.Add(SteamUser.GetSteamID().ToString());

        // Adiciona os Clientes
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            // O Host (connId 0) às vezes aparece aqui, às vezes não, depende da versão do Mirror.
            // O address no FizzySteamworks é o SteamID
            if (conn.connectionId != 0 && !string.IsNullOrEmpty(conn.address)) 
            {
                currentPlayers.Add(conn.address);
            }
        }

        // Remove duplicatas por segurança
        currentPlayers = currentPlayers.Distinct().ToList();

        Debug.Log($"[Host] Salvando Whitelist: {string.Join(", ", currentPlayers)}");

        // 2. Trava a UI (opcional, mostra loading)
        
        // 3. Manda para a API
        string mySteamId = SteamUser.GetSteamID().ToString();
        string saveId = GameSession.CurrentSave._id;

        StartCoroutine(APIService.instance.UpdateSaveWhitelist(saveId, mySteamId, currentPlayers,
            () => {
                Debug.Log("Whitelist Atualizada! Iniciando Jogo...");
                
                // 4. ATUALIZA A SESSÃO LOCAL TAMBÉM (Para a validação funcionar sem recarregar)
                GameSession.CurrentSave.allowedSteamIds = currentPlayers.ToArray();

                // 5. Muda de Cena
                NetworkManager.singleton.ServerChangeScene("DungeonScene"); // Nome da sua cena de jogo
            },
            (err) => {
                Debug.LogError("Erro ao salvar whitelist: " + err);
                // Decide se deixa iniciar mesmo com erro ou mostra aviso
            }
        ));
    }
}