using UnityEngine;
using Mirror;

public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
    [Header("Dados do Jogador")]
    [SyncVar(hook = nameof(OnCharacterIndexChanged))]
    public int characterIndex = 0; // O ID do personagem escolhido (índice no database)

    // Evento para avisar a UI que o personagem mudou (para atualizar a imagem do boneco na tela de todos)
    public static event System.Action<NetworkRoomPlayerExt> OnCharacterChanged;

    public override void OnStartClient()
    {
        // Quando entra na sala, avisa a UI para se atualizar
        base.OnStartClient();
        OnCharacterChanged?.Invoke(this);
    }

    public override void OnClientEnterRoom()
    {
        base.OnClientEnterRoom();
        OnCharacterChanged?.Invoke(this);
    }

    // --- COMANDOS (Cliente pede ao Servidor) ---

    [Command]
    public void CmdSelectCharacter(int index)
    {
        // Validação básica
        characterIndex = index;
    }

    // --- HOOKS (Servidor avisa Clientes que o valor mudou) ---

    void OnCharacterIndexChanged(int oldIndex, int newIndex)
    {
        // Atualiza a UI visualmente
        OnCharacterChanged?.Invoke(this);
    }
}