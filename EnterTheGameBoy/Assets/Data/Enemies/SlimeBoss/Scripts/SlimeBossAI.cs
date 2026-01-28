using UnityEngine;
using Mirror;
using System.Collections;

public class BossSlimeAI : NetworkBehaviour
{
    [Header("Estado")]
    [SyncVar] public bool isAsleep = true;

    [Header("Chase (Lento)")]
    public float chaseDistance = 10f;
    public float jumpDistance = 0.8f;
    public float jumpDuration = 0.35f;
    public float jumpCooldown = 1.2f;

    [Header("Combat")]
    public int damage = 2;
    public float timeBetweenAttacks = 1.2f;

    [Header("Machine Gun Attack")]
    public float machineGunDuration = 3f;
    public float machineGunFireRate = 0.12f;
    public float machineGunCooldown = 10f;

    [Header("Radial Pattern Attack")]
    public int radialBulletCount = 16;
    public int radialWaves = 3;
    public float radialWaveDelay = 0.4f;
    public float radialCooldown = 12f;

    float nextMachineGunTime;
    float nextRadialTime;

    [Header("Projectiles")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Minion Spawn")]
    [SerializeField] GameObject slimePrefab;
    [SerializeField] int slimeCount = 3;
    [SerializeField] float spawnInterval = 15f;
    [SerializeField] float spawnRadius = 2.5f;

    float nextSpawnTime;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Transform player;

    bool isJumping;
    bool isSpecialAttacking;

    Vector2 jumpDir;
    Vector2 jumpStart;
    float jumpTimer;
    float moveCooldownTimer;

    float nextAttackTime;
    float searchTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    public override void OnStartServer()
    {
        nextSpawnTime = Time.time + spawnInterval;
        nextMachineGunTime = Time.time + machineGunCooldown;
        nextRadialTime = Time.time + radialCooldown;
    }

    [Server]
    public void WakeUp()
    {
        isAsleep = false;
    }

    void FixedUpdate()
    {
        if (!isServer) return;
        if (isAsleep) return;

        TryFindPlayer();

        // -------- MOVIMENTO --------
        if (isJumping)
        {
            ContinueJump();
        }
        else
        {
            moveCooldownTimer -= Time.fixedDeltaTime;
            if (moveCooldownTimer <= 0f)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                StartJump(dir, jumpDistance, jumpDuration);
            }
        }

        // -------- SPAWN DE SLIMES --------
        if (Time.time >= nextSpawnTime)
        {
            SpawnSlimes();
            nextSpawnTime = Time.time + spawnInterval;
        }

        // -------- ATAQUES --------
        if (!player) return;

        if (Time.time >= nextRadialTime)
        {
            StartCoroutine(RadialAttack());
            return;
        }

        if (Time.time >= nextMachineGunTime)
        {
            StartCoroutine(MachineGunAttack());
            return;
        }
    }

    // ---------------- SPAWN ----------------

    [Server]
    void SpawnSlimes()
    {
        for (int i = 0; i < slimeCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset;

            GameObject slime = Instantiate(slimePrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(slime);
        }

    }

    // ---------------- PROJÉTEIS ----------------

    [Server]
    void SpawnProjectile(Vector2 dir)
    {
        GameObject proj = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        SlimeBossProjectile p = proj.GetComponent<SlimeBossProjectile>();
        p.Init(dir);

        NetworkServer.Spawn(proj);
    }

    Vector2 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    // ---------------- ATAQUES ----------------

    IEnumerator RadialAttack()
    {
        if (isSpecialAttacking) yield break;
        isSpecialAttacking = true;

        nextRadialTime = Time.time + radialCooldown;

        float step = 360f / radialBulletCount;

        for (int w = 0; w < radialWaves; w++)
        {
            for (int i = 0; i < radialBulletCount; i++)
            {
                float angle = step * i;
                SpawnProjectile(AngleToDirection(angle));
            }

            yield return new WaitForSeconds(radialWaveDelay);
        }

        isSpecialAttacking = false;
    }

    IEnumerator MachineGunAttack()
    {
        if (isSpecialAttacking) yield break;
        isSpecialAttacking = true;

        nextMachineGunTime = Time.time + machineGunCooldown;

        float elapsed = 0f;
        while (elapsed < machineGunDuration)
        {
            if (player)
            {
                Vector2 dir = (player.position - firePoint.position).normalized;
                SpawnProjectile(dir);
                UpdateVisuals(dir);
            }

            elapsed += machineGunFireRate;
            yield return new WaitForSeconds(machineGunFireRate);
        }

        isSpecialAttacking = false;
    }

    // ---------------- MOVIMENTO ----------------

    void StartJump(Vector2 direction, float distance, float duration)
    {
        isJumping = true;
        jumpDir = direction;
        jumpStart = rb.position;
        jumpTimer = 0f;

        jumpDistance = distance;
        jumpDuration = duration;

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

    // ---------------- DANO ----------------

    [ServerCallback]
    void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < nextAttackTime) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health == null || health.isDowned) return;

            health.TakeDamage(damage);
            nextAttackTime = Time.time + timeBetweenAttacks;

            Rigidbody2D prb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (prb)
            {
                Vector2 push = (collision.transform.position - transform.position).normalized;
                prb.AddForce(push * 6f, ForceMode2D.Impulse);
            }
        }
    }

    // ---------------- UTIL ----------------

    void TryFindPlayer()
    {
        searchTimer -= Time.fixedDeltaTime;
        if (searchTimer > 0 && player != null) return;
        searchTimer = 0.5f;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closest = Mathf.Infinity;
        Transform best = null;

        foreach (var p in players)
        {
            PlayerHealth h = p.GetComponent<PlayerHealth>();
            if (h != null && h.isDowned) continue;

            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d < closest)
            {
                closest = d;
                best = p.transform;
            }
        }

        player = best;
    }

    void UpdateVisuals(Vector2 dir)
    {
        if (dir.x > 0) sr.flipX = false;
        else if (dir.x < 0) sr.flipX = true;
    }
}
