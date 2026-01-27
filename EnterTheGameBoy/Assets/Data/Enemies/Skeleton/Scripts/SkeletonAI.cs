using UnityEngine;
using Mirror;

public class SkeletonAI : NetworkBehaviour
{
    [Header("Estado Inicial")]
    [SyncVar]
    public bool isAsleep = true;

    [Header("Combate")]
    public int damage = 1;
    public float timeBetweenAttacks = 1.0f;
    public float chaseDistance = 10f;
    public float chaseSpeed = 2f;

    [Header("Wander")]
    public float wanderSpeed = 0.5f;
    public float wanderTime = 1.5f;
    public float wanderWaitTime = 2f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Transform player;

    // Controle de Dano
    float nextAttackTime = 0f;

    // Wander
    Vector2 wanderDirection;
    float wanderTimer;
    float waitTimer;
    bool isWandering;

    // Otimização
    float searchTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    [Server]
    public void WakeUp()
    {
        isAsleep = false;
        Debug.Log($"{gameObject.name} (Skeleton) acordou!");
    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (isAsleep)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isMoving", false);
            return;
        }

        TryFindPlayer();

        if (!player)
        {
            Wander();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseDistance)
            ChasePlayer();
        else
            Wander();
    }

    // --- SISTEMA DE DANO PADRONIZADO ---
    [ServerCallback]
    void OnCollisionStay2D(Collision2D collision)
    {
        if (isAsleep) return;
        if (Time.time < nextAttackTime) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                nextAttackTime = Time.time + timeBetweenAttacks;

                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb)
                {
                    Vector2 pushDir = (collision.transform.position - transform.position).normalized;
                    playerRb.AddForce(pushDir * 5f, ForceMode2D.Impulse);
                }
            }
        }
    }

    void TryFindPlayer()
    {
        searchTimer -= Time.fixedDeltaTime;
        if (searchTimer > 0 && player != null) return;
        searchTimer = 0.5f;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDist = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (GameObject p in players)
        {
            PlayerHealth health = p.GetComponent<PlayerHealth>();
            if (health != null && health.isDowned) continue;

            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d < chaseDistance && d < closestDist)
            {
                closestDist = d;
                bestTarget = p.transform;
            }
        }
        player = bestTarget;
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;

        anim.SetBool("isMoving", true);
        UpdateVisuals(direction);
    }

    void Wander()
    {
        if (isWandering)
        {
            wanderTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = wanderDirection * wanderSpeed;

            anim.SetBool("isMoving", true);
            UpdateVisuals(wanderDirection);

            if (wanderTimer <= 0)
            {
                isWandering = false;
                waitTimer = wanderWaitTime;
                rb.linearVelocity = Vector2.zero;
                anim.SetBool("isMoving", false);
            }
        }
        else
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isMoving", false);

            if (waitTimer <= 0) PickRandomDirection();
        }
    }

    void PickRandomDirection()
    {
        wanderDirection = Random.insideUnitCircle.normalized;
        wanderTimer = wanderTime;
        isWandering = true;
    }

    void UpdateVisuals(Vector2 direction)
    {
        if (direction.x > 0) sr.flipX = false;
        else if (direction.x < 0) sr.flipX = true;
    }
}