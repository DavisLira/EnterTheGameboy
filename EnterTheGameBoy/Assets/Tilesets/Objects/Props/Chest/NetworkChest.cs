using UnityEngine;
using Mirror;
using System.Collections.Generic; // Necessário para Listas

public class NetworkChest : NetworkBehaviour, IInteractable
{
    [Header("Visual")]
    public Sprite openSprite;
    public Sprite closedSprite;
    private SpriteRenderer sr;

    [Header("Loot Table")]
    // Em vez de 1 objeto, agora é uma LISTA
    public List<GameObject> possibleWeapons; 

    [SyncVar(hook = nameof(OnStateChanged))]
    public bool isOpen = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    public bool CanInteract() { return !isOpen; }

    public void Interact()
    {
        // Mesma lógica de interação via Player
        if (isOpen) return;
        var localPlayer = NetworkClient.localPlayer;
        if (localPlayer)
        {
            var controller = localPlayer.GetComponent<PlayerWeaponController>();
            if (controller) controller.CmdOpenChest(gameObject);
        }
    }

    [Server]
    public void ServerInteract()
    {
        if (isOpen) return;
        isOpen = true; // Abre o baú

        // Sorteia uma arma da lista
        if (possibleWeapons != null && possibleWeapons.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleWeapons.Count);
            GameObject weaponToDrop = possibleWeapons[randomIndex];

            if (weaponToDrop != null)
            {
                GameObject loot = Instantiate(weaponToDrop, transform.position + Vector3.down, Quaternion.identity);
                NetworkServer.Spawn(loot);
            }
        }
    }

    void OnStateChanged(bool oldVal, bool newVal) { UpdateSprite(); }
    void UpdateSprite() { if(sr) sr.sprite = isOpen ? openSprite : closedSprite; }
}