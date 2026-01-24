using UnityEngine;
using Mirror;

public class NetworkRoomPlayerExt : NetworkRoomPlayer
{
    [Header("Dados do Jogador")]
    // Começa com -1 para sabermos que ele NÃO escolheu ninguém
    [SyncVar(hook = nameof(OnCharacterIndexChanged))]
    public int characterIndex = -1;

    public static event System.Action<NetworkRoomPlayerExt> OnCharacterChanged;

    public override void OnStartClient()
    {
        base.OnStartClient();
        OnCharacterChanged?.Invoke(this);
    }

    public override void OnClientEnterRoom()
    {
        base.OnClientEnterRoom();
        OnCharacterChanged?.Invoke(this);
    }

    [Command]
    public void CmdSelectCharacter(int index)
    {
        characterIndex = index;
    }

    void OnCharacterIndexChanged(int oldIndex, int newIndex)
    {
        OnCharacterChanged?.Invoke(this);
    }
}