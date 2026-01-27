using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

[System.Serializable]
public class WeaponStats
{
    public string name;
    public Sprite handSprite;
    public GameObject dropPrefab;
    public GameObject projectilePrefab;
    
    [Header("Atributos de Combate")]
    public float fireRate = 0.5f;
    public int damage = 1;
    public float projectileSpeed = 15f;
    public float projectileLifeTime = 1f;

    [Header("Configuração Shotgun")]
    public int pellets = 1;      // Quantas balas saem? (1 para pistola, 5 para shotgun)
    public float spreadAngle = 0f; // Abertura do cone em graus (0 para pistola, 30 para shotgun)
}

public class PlayerWeaponController : NetworkBehaviour
{
    [Header("Referências Visuais")]
    public Transform weaponPivot; 
    public SpriteRenderer weaponRenderer; 
    public Transform firePoint; 

    [Header("ARSENAL (Configure aqui!)")]
    public WeaponStats[] arsenal; // <--- A NOVA LISTA PODEROSA

    [Header("Estado")]
    [SyncVar(hook = nameof(OnWeaponChanged))]
    public int currentWeaponID = -1;

    private float nextFireTime = 0f;
    private Camera mainCam;

    public override void OnStartClient()
    {
        base.OnStartClient();
        OnWeaponChanged(-1, currentWeaponID);
        mainCam = Camera.main;
    }

    void Update()
    {
        if (!isLocalPlayer || currentWeaponID == -1) return;
        if (!weaponPivot.gameObject.activeSelf) return;
        if (Mouse.current == null) return;

        HandleAiming();
        HandleShooting();
    }

    void HandleAiming()
    {
        if (Mouse.current == null) return;
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3 direction = mouseWorldPos - weaponPivot.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        weaponPivot.rotation = Quaternion.Euler(0, 0, angle);

        if (angle > 90 || angle < -90)
            weaponPivot.localScale = new Vector3(1, -1, 1);
        else
            weaponPivot.localScale = Vector3.one;
    }

    void HandleShooting()
    {
        // Proteção: ID inválido
        if(currentWeaponID < 0 || currentWeaponID >= arsenal.Length) return;

        // Pega status da arma atual
        WeaponStats stats = arsenal[currentWeaponID];

        // Usa o fireRate da arma atual
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + stats.fireRate;
            CmdShoot(firePoint.position, weaponPivot.rotation);
        }
    }

    [Command]
    void CmdShoot(Vector3 pos, Quaternion rot)
    {
        if(currentWeaponID < 0 || currentWeaponID >= arsenal.Length) return;
        
        WeaponStats stats = arsenal[currentWeaponID];
        if (stats.projectilePrefab == null) return;

        // LÓGICA DO CONE
        // Se for 1 bala (Pistola/Rifle), ângulo inicial é 0.
        // Se forem várias (Shotgun), calculamos o ângulo inicial para esquerda.
        int pelletCount = Mathf.Max(1, stats.pellets);
        
        // Ex: Se o spread é 40 graus, começa em -20 e vai até +20
        float startAngle = -stats.spreadAngle / 2f; 
        
        // Quanto de ângulo aumenta para cada bala
        float angleStep = (pelletCount > 1) ? (stats.spreadAngle / (pelletCount - 1)) : 0f;

        for (int i = 0; i < pelletCount; i++)
        {
            // Calcula a rotação desta bala específica
            float currentAngle = startAngle + (angleStep * i);
            Quaternion spreadRotation = rot * Quaternion.Euler(0, 0, currentAngle);

            // Instancia a bala com a rotação calculada
            GameObject bullet = Instantiate(stats.projectilePrefab, pos, spreadRotation);
            
            Projectile p = bullet.GetComponent<Projectile>();
            if(p != null) 
            {
                p.Setup(stats.projectileSpeed, stats.damage, stats.projectileLifeTime, gameObject);
            }

            NetworkServer.Spawn(bullet);
        }
    }
    
    [Command]
    public void CmdPickupWeapon(GameObject floorWeaponObj)
    {
        if (floorWeaponObj == null) return;
        DroppedWeapon floorStats = floorWeaponObj.GetComponent<DroppedWeapon>();
        if (floorStats == null) return;

        // Dropa a arma velha (Se tiver e for válida)
        if (currentWeaponID != -1 && currentWeaponID < arsenal.Length)
        {
            GameObject oldWeaponPrefab = arsenal[currentWeaponID].dropPrefab;
            if(oldWeaponPrefab != null)
            {
                GameObject dropped = Instantiate(oldWeaponPrefab, transform.position, Quaternion.identity);
                NetworkServer.Spawn(dropped);
            }
        }

        // Pega a nova
        currentWeaponID = floorStats.weaponID;
        NetworkServer.Destroy(floorWeaponObj);
    }
    
    void OnWeaponChanged(int oldID, int newID)
    {
        if (weaponPivot == null || weaponRenderer == null) return;

        if (newID == -1)
        {
            weaponPivot.gameObject.SetActive(false);
        }
        else
        {
            weaponPivot.gameObject.SetActive(true);
            // Atualiza Sprite baseado na lista
            if (newID < arsenal.Length)
                weaponRenderer.sprite = arsenal[newID].handSprite;
        }
    }
    
    public void SetWeaponActive(bool active)
    {
        bool shouldShow = active && (currentWeaponID != -1);
        if (weaponPivot) weaponPivot.gameObject.SetActive(shouldShow);
    }

    [Command]
    public void CmdOpenChest(GameObject chestObj)
    {
        if (chestObj != null && chestObj.TryGetComponent<NetworkChest>(out NetworkChest chest))
        {
            chest.ServerInteract();
        }
    }
}