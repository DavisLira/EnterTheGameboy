using UnityEngine;
using Mirror;

public class Projectile : NetworkBehaviour
{
    // Variáveis agora são públicas mas escondidas, pois serão configuradas pelo Player
    [HideInInspector] public float speed;
    [HideInInspector] public int damage;
    [HideInInspector] public float lifeTime;
    [HideInInspector] public GameObject owner;

    // --- FUNÇÃO VITAL: O Player chama isso logo após criar a bala ---
    public void Setup(float newSpeed, int newDamage, float newRange, GameObject shooter)
    {
        speed = newSpeed;
        damage = newDamage;
        lifeTime = newRange; 
        owner = shooter;
    }

    public override void OnStartServer()
    {
        // Se ninguém configurar (ex: teste), usa valores padrão
        if (lifeTime <= 0) lifeTime = 3f;
        if (speed <= 0) speed = 10f;
        
        Invoke(nameof(DestroySelf), lifeTime);
    }

    [ServerCallback]
    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    [ServerCallback]
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Ignora quem atirou
        if (other.gameObject == owner) return;
        
        // 2. Ignora triggers que não sejam inimigos (senão a bala explode ao passar num baú ou porta aberta)
        if (other.isTrigger && !other.CompareTag("Enemy")) return;

        // 3. Tenta pegar EnemyHealth (Prioridade)
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null) enemy = other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage,owner);
            DestroySelf();
            return;
        }

        // 4. Tenta pegar PlayerHealth (BLOQUEIO DE FRIENDLY FIRE)
        PlayerHealth targetPlayer = other.GetComponent<PlayerHealth>();
        
        if (targetPlayer != null)
        {
            // Se quem atirou também tem a tag Player, não dá dano!
            // (Assim, se um inimigo atirar, ainda te machuca, mas seu amigo não)
            if (owner != null && owner.CompareTag("Player"))
            {
                // É fogo amigo! Ignora e deixa a bala passar (return)
                // OU destrói a bala sem dar dano (DestroySelf) se quiser que a bala pare no amigo.
                // Vamos deixar passar (return) para não atrapalhar o tiroteio.
                return; 
            }

            // Se quem atirou foi um Inimigo (ou null), dá dano normal
            targetPlayer.TakeDamage(damage);
            DestroySelf();
            return;
        }

        // Se bateu em parede (Tag Untagged ou Default), destrói
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy")) 
        {
             DestroySelf();
        }
    }
    
    [Server]
    void DestroySelf() { NetworkServer.Destroy(gameObject); }
}