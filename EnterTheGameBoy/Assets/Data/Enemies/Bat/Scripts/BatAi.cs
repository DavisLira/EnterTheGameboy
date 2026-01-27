using UnityEngine;
using Mirror;

public class BatAI : NetworkBehaviour
{
    [Header("Estado Inicial")]
    [SyncVar] 
    public bool isAsleep = true;

    [Header("Atributos de Combate")]
    public int damage = 1;
    public float timeBetweenAttacks = 1.0f;
    public float chaseDistance = 10f;
    public float attackDistance = 3f;

    [Header("Velocidades")]
    public float wanderSpeed = 0.6f;
    public float chaseSpeed = 2.5f;
    public float dashSpeed = 8f;

    [Header("Tempos")]
    public float wanderTime = 1.2f;
    public float wanderWaitTime = 2f;
    public float chargeTime = 0.5f;
    public float dashDuration = 0.25f;

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

    // Attack / Dash
    bool isCharging;
    bool isDashing;
    float chargeTimer;
    float dashTimer;
    Vector2 dashDirection;

    // Otimização de busca
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
        Debug.Log($"{gameObject.name} (Bat) acordou!");
    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (isAsleep) 
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        TryFindPlayer();
        
        // Se não tiver player, apenas vaga (Wander)
        if (!player) 
        {
            Wander();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (isDashing)
            Dash();
        else if (isCharging)
            Charge();
        else if (distance <= attackDistance)
            StartCharge();
        else if (distance <= chaseDistance)
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

                // Knockback
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

    void Wander()
    {
        if (isWandering)
        {
            wanderTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = wanderDirection * wanderSpeed;
            UpdateVisuals(wanderDirection);

            if (wanderTimer <= 0)
            {
                isWandering = false;
                waitTimer = wanderWaitTime;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0) PickRandomDirection();
        }
    }

    void PickRandomDirection()
    {
        wanderDirection = Random.insideUnitCircle.normalized;
        wanderTimer = wanderTime;
        isWandering = true;
    }

    void ChasePlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * chaseSpeed;
        UpdateVisuals(dir);
    }

    void StartCharge()
    {
        isCharging = true;
        chargeTimer = chargeTime;
        rb.linearVelocity = Vector2.zero;
        dashDirection = (player.position - transform.position).normalized;
    }

    void Charge()
    {
        chargeTimer -= Time.fixedDeltaTime;
        rb.linearVelocity = Vector2.zero;
        // Opcional: Adicionar feedback visual de "carregando" (ex: piscar cor)

        if (chargeTimer <= 0)
        {
            isCharging = false;
            isDashing = true;
            dashTimer = dashDuration;
        }
    }

    void Dash()
    {
        rb.linearVelocity = dashDirection * dashSpeed;
        dashTimer -= Time.fixedDeltaTime;
        anim.SetBool("isDashing", true); // Garanta que essa bool existe no Animator do Bat
        UpdateVisuals(dashDirection);

        if (dashTimer <= 0)
        {
            isDashing = false;
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isDashing", false);
        }
    }

    void UpdateVisuals(Vector2 dir)
    {
        if (dir.x > 0) sr.flipX = false;
        else if (dir.x < 0) sr.flipX = true;
    }
}