using UnityEngine;
using Mirror;
using System; // Necessário para usar Actions

public class EnemyHealth : NetworkBehaviour
{
    [Header("Configuração")]
    public int maxHealth = 3;

    [SyncVar]
    private int currentHealth;

    // EVENTO: Avisa quem estiver interessado que este objeto morreu
    public event Action<GameObject> OnDeath;

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"[EnemyHealth] Tomou {damage} de dano. Vida: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"[EnemyHealth] Morreu! Disparando evento e destruindo. ID: {gameObject.GetInstanceID()}");
            
            // 1. AVISA A SALA (Antes de se destruir)
            OnDeath?.Invoke(gameObject);

            // 2. Destrói na rede
            NetworkServer.Destroy(gameObject);
        }
    }
}