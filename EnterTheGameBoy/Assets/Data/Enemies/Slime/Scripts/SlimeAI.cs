using UnityEngine;
using Mirror;

public class SlimeAI : NetworkBehaviour
{
    [Header("Estado Inicial")]
    [SyncVar] // O servidor avisa os clientes se o monstro está dormindo
    public bool isAsleep = true; 

    [Header("Chase")]
    public float chaseDistance = 8f;
    public float jumpDistance = 1.2f;
    public float jumpDuration = 0.2f;
    public float jumpCooldown = 0.8f;

    [Header("Combat")]
    public int damage = 1;
    public float timeBetweenAttacks = 1.0f;

    [Header("Wander")]
    public float wanderTime = 1.5f;
    public float wanderWaitTime = 2f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Transform player;

    // Variáveis de Controle
    Vector2 jumpDir;
    Vector2 jumpStart;
    float jumpTimer;
    float moveCooldownTimer; 
    
    // Controle do Dano (MUDANÇA AQUI)
    float nextAttackTime = 0f;

    // Wander
    Vector2 wanderDir;
    float wanderTimer;
    float waitTimer;

    bool isJumping;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Função pública para a DungeonRoom chamar
    [Server]
    public void WakeUp()
    {
        isAsleep = false;
        Debug.Log($"{gameObject.name} acordou e escolheu violência.");
    }

    void FixedUpdate()
    {
        // 1. SE ESTIVER DORMINDO, NÃO SE MEXE
        // O script continua rodando, mas paramos a lógica de movimento aqui.
        if (isAsleep) 
        {
            // Opcional: Se quiser resetar animação de andar
            if(anim) anim.SetBool("isMoving", false);
            return; 
        }

        // --- DAQUI PRA BAIXO É O CÓDIGO DE MOVIMENTO NORMAL ---

        TryFindPlayer();
        if (!player) return;

        if (isJumping)
        {
            ContinueJump();
            return;
        }

        moveCooldownTimer -= Time.fixedDeltaTime;
        if (moveCooldownTimer > 0) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseDistance)
            StartJump((player.position - transform.position).normalized);
        else
            Wander();
    }

    // --- SISTEMA DE DANO CORRIGIDO ---
    [ServerCallback]
    void OnCollisionStay2D(Collision2D collision)
    {
        // Se estiver dormindo, não dá dano (opcional, remova se quiser que ele seja uma armadilha)
        if (isAsleep) return;

        // Verifica se já passou o tempo necessário desde o último ataque
        if (Time.time < nextAttackTime) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Causa Dano
                playerHealth.TakeDamage(damage);

                // Define a PRÓXIMA vez que ele pode atacar (Agora + 1 segundo)
                nextAttackTime = Time.time + timeBetweenAttacks;

                // Empurrão (Knockback)
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb)
                {
                    Vector2 pushDir = (collision.transform.position - transform.position).normalized;
                    playerRb.AddForce(pushDir * 5f, ForceMode2D.Impulse); // Aumentei um pouco a força
                }
            }
        }
    }

    // ... (Mantenha TryFindPlayer, StartJump, ContinueJump, Wander iguais ao anterior) ...
    // Variável para controlar a frequência da busca (otimização)
    float searchTimer = 0f;

    void TryFindPlayer()
    {
        // Só busca a cada 0.5 segundos para não pesar o processamento
        searchTimer -= Time.fixedDeltaTime;
        if (searchTimer > 0 && player != null) return; 
        searchTimer = 0.5f;

        // Encontra TODOS os jogadores
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        float closestDist = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (GameObject p in players)
        {
            // Verifica se tem script de vida
            PlayerHealth health = p.GetComponent<PlayerHealth>();
            
            // IGNORA se o player estiver morto
            if (health != null && health.isDowned) continue;

            float d = Vector2.Distance(transform.position, p.transform.position);
            
            // Se estiver dentro da distância de perseguição e for o mais perto até agora
            if (d < chaseDistance && d < closestDist)
            {
                closestDist = d;
                bestTarget = p.transform;
            }
        }

        // Atualiza o alvo (pode ser null se todos morrerem ou fugirem)
        player = bestTarget;
    }

    void StartJump(Vector2 direction)
    {
        isJumping = true;
        jumpDir = direction;
        jumpStart = rb.position;
        jumpTimer = 0f;
        anim.SetBool("isMoving", true);
        UpdateVisuals(direction);
    }

    void ContinueJump()
    {
        jumpTimer += Time.fixedDeltaTime;
        float t = jumpTimer / jumpDuration;
        Vector2 target = jumpStart + jumpDir * jumpDistance;
        rb.MovePosition(Vector2.Lerp(jumpStart, target, t));

        if (t >= 1f)
        {
            isJumping = false;
            moveCooldownTimer = jumpCooldown;
            anim.SetBool("isMoving", false);
        }
    }

    void Wander()
    {
        if (wanderTimer > 0)
        {
            wanderTimer -= Time.fixedDeltaTime;
            StartJump(wanderDir);
        }
        else
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0) PickRandomDirection();
        }
    }

    void PickRandomDirection()
    {
        wanderDir = Random.insideUnitCircle.normalized;
        wanderTimer = wanderTime;
        waitTimer = wanderWaitTime;
    }

    void UpdateVisuals(Vector2 direction)
    {
        if (direction.x > 0) sr.flipX = false;
        else if (direction.x < 0) sr.flipX = true;
    }
}
