using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerStats : NetworkBehaviour
{
    [Header("Identificação")]
    // SyncVar faz com que todos os jogadores saibam o ID e Kills desse jogador
    [SyncVar] public string steamId;
    
    [Header("Estatísticas")]
    [SyncVar] public int killCount = 0;

    public override void OnStartLocalPlayer()
    {
        // Assim que eu entro no jogo, pego meu ID da Steam e aviso ao Servidor
        if (SteamManager.Initialized)
        {
            string myId = SteamUser.GetSteamID().ToString();
            CmdSetupPlayer(myId);
        }
        else
        {
            // Fallback para testes no Editor sem Steam
            Debug.LogWarning("Steam não iniciada! Usando ID de teste.");
            CmdSetupPlayer("ID_TESTE_" + UnityEngine.Random.Range(0, 1000));
        }
    }

    // O Cliente pede para o Servidor: "Ei, esse é meu ID!"
    [Command]
    void CmdSetupPlayer(string id)
    {
        steamId = id;
    }

    // Função que o INIMIGO vai chamar quando morrer
    [Server]
    public void AddKill()
    {
        killCount++;
        Debug.Log($"[Server] {steamId} matou um inimigo! Total: {killCount}");

        // Salvar na API
        // Verifica se o APIService existe e se temos um ID válido
        if (APIService.instance != null && !string.IsNullOrEmpty(steamId))
        {
            // Inicia a corrotina do seu APIService para mandar os dados
            StartCoroutine(APIService.instance.UpdatePlayerKills(
                steamId, 
                killCount,
                () => Debug.Log("Kill salva na nuvem!"), // Sucesso
                (err) => Debug.LogError("Erro ao salvar kill: " + err) // Erro
            ));
        }
    }
}