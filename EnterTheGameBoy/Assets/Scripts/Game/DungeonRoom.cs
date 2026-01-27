using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class DungeonRoom : NetworkBehaviour
{
    [Header("Configuração")]
    public List<GameObject> doors;
    public List<GameObject> enemies;
    
    [Header("Teleporte")]
    public Transform[] spawnPoints; 

    private bool roomActive = false;
    private bool roomCleared = false;
    private int enemiesAlive = 0;

    public override void OnStartServer()
    {
        UpdateDoorVisuals(false);
        // Limpa a lista inicial de sujeira (objetos deletados)
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
        Debug.Log("SALA ATIVADA!");
        roomActive = true;
        SetDoors(true);
        WakeUpEnemies();
    }

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
        enemiesAlive = 0; // Reseta contagem

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            
            // 1. Ativa o objeto do inimigo
            enemy.SetActive(true);
            
            // 2. Tenta acordar a IA (Verifica qual tipo de inimigo é)
            // -------------------------------------------------------
            SlimeAI slime = enemy.GetComponentInChildren<SlimeAI>();
            if (slime != null) slime.WakeUp();

            BatAI bat = enemy.GetComponentInChildren<BatAI>();
            if (bat != null) bat.WakeUp();

            SkeletonAI skeleton = enemy.GetComponentInChildren<SkeletonAI>();
            if (skeleton != null) skeleton.WakeUp();
            // -------------------------------------------------------

            // 3. Configura a vida para saber quando a sala abre
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health == null) health = enemy.GetComponentInParent<EnemyHealth>();

            if (health != null)
            {
                enemiesAlive++; 
                health.OnDeath += HandleEnemyDeath;
                Debug.Log($"[Dungeon] Monitorando inimigo: {enemy.name}");
            }
            else
            {
                Debug.LogError($"[Dungeon] O inimigo {enemy.name} NÃO tem script EnemyHealth! A sala pode travar.");
            }
        }
        
        // Se a lista estava vazia ou ninguém tinha vida, abre a sala
        if (enemiesAlive == 0) RoomCleared();
    }

    [Server]
    void HandleEnemyDeath(GameObject deadEnemy)
    {
        enemiesAlive--;
        // Retira a inscrição do evento para evitar erros futuros
        EnemyHealth health = deadEnemy.GetComponent<EnemyHealth>();
        if (health != null) health.OnDeath -= HandleEnemyDeath;

        Debug.Log($"[Dungeon] Inimigo abatido! Restam: {enemiesAlive}");

        if (enemiesAlive <= 0)
        {
            RoomCleared();
        }
    }

    [Server]
    void RoomCleared()
    {
        if (roomCleared) return;
        
        Debug.Log("Sala Limpa! Abrindo portas...");
        roomCleared = true;
        SetDoors(false); 
    }

    [ServerCallback]
    void Update()
    {
        // Update vazio (Lógica controlada por eventos)
    }

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