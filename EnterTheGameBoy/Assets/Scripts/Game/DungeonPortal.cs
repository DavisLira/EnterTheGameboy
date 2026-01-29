using UnityEngine;
using Mirror;

public class DungeonPortal : NetworkBehaviour
{
    [Header("Config")]
    public float interactionRadius = 1.5f;

    [SyncVar]
    private bool isActive = false;

    public override void OnStartServer()
    {
        isActive = true;
    }

    [ServerCallback]
    void Update()
    {
        if (!isActive) return;

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                float dist = Vector2.Distance(transform.position, conn.identity.transform.position);
                if (dist < interactionRadius)
                {
                    Interact();
                    break;
                }
            }
        }
    }

    [Server]
    void Interact()
    {
        if (!isActive) return;
        isActive = false;

        // Pega o Singleton do NetworkManager e chama a função nova
        var manager = NetworkManager.singleton as MyNetworkManager;

        if (manager != null)
        {
            manager.GoToNextLevel();
        }
        else
        {
            Debug.LogError("MyNetworkManager não encontrado! Verifique se o script está no objeto NetworkManager.");
        }
    }
}