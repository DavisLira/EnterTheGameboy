using UnityEngine;

public class SlimeAI : MonoBehaviour
{
    [Header("Chase")]
    public float chaseDistance = 8f;
    public float jumpDistance = 1.2f;
    public float jumpDuration = 0.2f;
    public float jumpCooldown = 0.8f;

    [Header("Wander")]
    public float wanderTime = 1.5f;
    public float wanderWaitTime = 2f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Transform player;

    Vector2 jumpDir;
    Vector2 jumpStart;
    float jumpTimer;
    float cooldownTimer;

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

    void FixedUpdate()
    {
        TryFindPlayer();
        if (!player) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (isJumping)
        {
            ContinueJump();
            return;
        }

        cooldownTimer -= Time.fixedDeltaTime;
        if (cooldownTimer > 0) return;

        if (distance <= chaseDistance)
            StartJump((player.position - transform.position).normalized);
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
            cooldownTimer = jumpCooldown;
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
            if (waitTimer <= 0)
                PickRandomDirection();
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
        if (direction.x > 0)
            sr.flipX = false;
        else if (direction.x < 0)
            sr.flipX = true;
    }
}
