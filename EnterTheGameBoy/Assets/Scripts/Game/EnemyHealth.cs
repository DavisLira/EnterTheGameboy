using UnityEngine;
using Mirror;
using System;
using System.Collections; // Necessário para usar Actions

public class EnemyHealth : NetworkBehaviour
{
    [Header("Configuração")]
    public int maxHealth = 3;

    // MUDANÇA 1: Adicionamos o Hook aqui
    [SyncVar(hook = nameof(OnHealthChanged))]
    private int currentHealth;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer; // ARRASTE O SPRITE DO INIMIGO AQUI
    public Color damageColor = Color.red;
    private Color originalColor;
    private Coroutine flashCoroutine;

    // EVENTO: Avisa quem estiver interessado que este objeto morreu
    public event Action<GameObject> OnDeath;

    void Awake()
    {
        // Tenta pegar automático se esqueceu de arrastar
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Salva a cor original (ex: Branco)
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    [Server]
    public void TakeDamage(int damage, GameObject attacker = null)
    {
        currentHealth -= damage;
        Debug.Log($"[EnemyHealth] Tomou {damage} de dano. Vida: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"[EnemyHealth] Morreu! Disparando evento e destruindo. ID: {gameObject.GetInstanceID()}");
            
            if (attacker != null)
            {
                PlayerStats stats = attacker.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.AddKill(); // Adiciona +1 kill
                }
            }

            // 1. AVISA A SALA (Antes de se destruir)
            OnDeath?.Invoke(gameObject);

            // 2. Destrói na rede
            NetworkServer.Destroy(gameObject);
        }
    }

    void OnHealthChanged(int oldVal, int newVal)
    {
        // Se a vida diminuiu, pisca
        if (newVal < oldVal)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }

    IEnumerator FlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
    }
}