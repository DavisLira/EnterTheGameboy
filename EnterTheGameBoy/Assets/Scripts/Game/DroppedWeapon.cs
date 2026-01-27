using UnityEngine;
using Mirror;

public class DroppedWeapon : NetworkBehaviour, IInteractable
{
    [Header("Configuração")]
    public int weaponID = 0; // 0 = Pistola
    
    // FALTAVA ISSO AQUI:
    public GameObject weaponPrefab; // <--- Agora o campo vai aparecer no Inspector!

    private BouncyEffect bounce;

    void Awake() { bounce = GetComponent<BouncyEffect>(); }

    public override void OnStartClient() { if(bounce) bounce.Startbounce(); }

    public bool CanInteract()
    {
        return true; 
    }

    public void Interact()
    {
        var localPlayer = NetworkClient.localPlayer;
        if (localPlayer == null) return;

        var controller = localPlayer.GetComponent<PlayerWeaponController>();
        
        if (controller != null)
        {
            controller.CmdPickupWeapon(gameObject);
        }
    }
}