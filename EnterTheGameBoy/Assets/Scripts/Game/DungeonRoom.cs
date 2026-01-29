using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class DungeonRoom : NetworkBehaviour
{
    [Header("Configuração")]
    public List<GameObject> doors;
    public List<GameObject> enemies;
    
    [Header("Configuração de Boss")]
    public bool isBossRoom = false;          // Marque true no Inspector para a sala do Boss
    public GameObject portalPrefab;          // Arraste o prefab do Portal aqui
    public Transform roomCenter;             // Um objeto vazio no centro da sala para spawnar o portal

    [Header("Teleporte")]
    public Transform[] spawnPoints; 

    private bool roomActive = false;
    private bool roomCleared = false;
    private int enemiesAlive = 0;

    public override void OnStartServer()
    {
        UpdateDoorVisuals(false);
        enemies.RemoveAll(item => item == null);
    }

    [ServerCallback] 
    void OnTriggerEnter2D(Collider2D other)
    {
        if (roomActive || roomCleared) return;

        if (other.CompareTag("Player"))
        {
            StartRoom();
            TeleportParty(other.gameObject); 
        }
    }

    [Server]
    void StartRoom()
    {
        roomActive = true;
        SetDoors(true);
        WakeUpEnemies();
    }

    // ... (Métodos SetDoors, RpcSetDoors, UpdateDoorVisuals IGUAIS AO SEU CÓDIGO) ...
    [Server]
    void SetDoors(bool closed)
    {
        UpdateDoorVisuals(closed);
        RpcSetDoors(closed);
    }

    [ClientRpc]
    void RpcSetDoors(bool closed)
    {
        UpdateDoorVisuals(closed);
    }

    void UpdateDoorVisuals(bool closed)
    {
        foreach (var doorObj in doors)
        {
            if(doorObj != null) 
            {
                doorObj.SetActive(true);
                DungeonDoor doorScript = doorObj.GetComponent<DungeonDoor>();
                if (doorScript != null) doorScript.SetDoorState(closed);
            }
        }
    }

    [Server]
    void WakeUpEnemies()
    {
        enemiesAlive = 0; 

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            enemy.SetActive(true);
            
            // Acorda as IAs (Seu código original)
            SlimeAI slime = enemy.GetComponentInChildren<SlimeAI>();
            if (slime != null) slime.WakeUp();
            BossSlimeAI slimeBoss = enemy.GetComponentInChildren<BossSlimeAI>();
            if (slimeBoss != null) slimeBoss.WakeUp();
            BatAI bat = enemy.GetComponentInChildren<BatAI>();
            if (bat != null) bat.WakeUp();
            SkeletonAI skeleton = enemy.GetComponentInChildren<SkeletonAI>();
            if (skeleton != null) skeleton.WakeUp();

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health == null) health = enemy.GetComponentInParent<EnemyHealth>();

            if (health != null)
            {
                enemiesAlive++; 
                health.OnDeath += HandleEnemyDeath;
            }
        }
        
        if (enemiesAlive == 0) RoomCleared();
    }

    [Server]
    void HandleEnemyDeath(GameObject deadEnemy)
    {
        enemiesAlive--;
        EnemyHealth health = deadEnemy.GetComponent<EnemyHealth>();
        if (health != null) health.OnDeath -= HandleEnemyDeath;

        if (enemiesAlive <= 0)
        {
            RoomCleared();
        }
    }

    [Server]
    void RoomCleared()
    {
        if (roomCleared) return;
        
        roomCleared = true;
        SetDoors(false); 

        // --- NOVO: LÓGICA DO DROP DO PORTAL ---
        if (isBossRoom)
        {
            SpawnBossReward();
        }
    }

    [Server]
    void SpawnBossReward()
    {
        if (portalPrefab != null && roomCenter != null)
        {
            Debug.Log("Boss Morto! Spawnando portal...");
            GameObject portal = Instantiate(portalPrefab, roomCenter.position, Quaternion.identity);
            NetworkServer.Spawn(portal);
        }
        else
        {
            Debug.LogWarning("DungeonRoom: É sala de Boss, mas falta o PortalPrefab ou RoomCenter!");
        }
    }

    // ... (TeleportParty e Update continuam iguais) ...
    [Server]
    void TeleportParty(GameObject activatorPlayer)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        int index = 0;
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                GameObject currentPlayer = conn.identity.gameObject;
                
                if (currentPlayer == activatorPlayer) 
                {
                    index++;
                    continue;
                }

                Transform targetPoint = spawnPoints[index % spawnPoints.Length];

                var moveScript = currentPlayer.GetComponent<PlayerMovement>();
                if (moveScript != null)
                {
                    moveScript.ForceTeleport(targetPoint.position);
                }
                else
                {
                    currentPlayer.transform.position = targetPoint.position;
                }

                index++;
            }
        }
    }
}