using UnityEngine;

public class SkeletonAI : MonoBehaviour
{
    public float chaseSpeed = 2f;
    public float wanderSpeed = 0.5f;
    public float chaseDistance = 10f;

    public float wanderTime = 1.5f;
    public float wanderWaitTime = 2f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Transform player;

    Vector2 wanderDirection;
    float wanderTimer;
    float waitTimer;
    bool isWandering;

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

        if (distance <= chaseDistance)
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

    void UpdateVisuals(Vector2 direction)
    {
        if (direction.x > 0)
            sr.flipX = false;
        else if (direction.x < 0)
            sr.flipX = true;
    }
}
