using UnityEngine;

public class BatAI : MonoBehaviour
{
    public float chaseDistance = 10f;
    public float attackDistance = 3f;

    public float wanderSpeed = 0.6f;
    public float chaseSpeed = 2.5f;
    public float dashSpeed = 8f;

    public float wanderTime = 1.2f;
    public float wanderWaitTime = 2f;

    public float chargeTime = 0.5f;
    public float dashDuration = 0.25f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Transform player;

    // Wander
    Vector2 wanderDirection;
    float wanderTimer;
    float waitTimer;
    bool isWandering;

    // Attack
    bool isCharging;
    bool isDashing;
    float chargeTimer;
    float dashTimer;
    Vector2 dashDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        TryFindPlayer();
        if (!player) return;

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

    void TryFindPlayer()
    {
        if (player) return;

        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go)
            player = go.transform;
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
            if (waitTimer <= 0)
                PickRandomDirection();
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

        anim.SetBool("isDashing", true);
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
        if (dir.x > 0)
            sr.flipX = false;
        else if (dir.x < 0)
            sr.flipX = true;
    }
}
