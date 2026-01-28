using UnityEngine;
using Mirror;

public class SlimeBossProjectile : NetworkBehaviour
{
    [Header("Config")]
    public float speed = 8f;
    public int damage = 1;
    public float lifeTime = 5f;

    Vector2 direction;

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), lifeTime);
    }

    [Server]
    public void Init(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void FixedUpdate()
    {
        if (!isServer) return;

        transform.position += (Vector3)(direction * speed * Time.fixedDeltaTime);
    }

    [ServerCallback]
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null && !health.isDowned)
        {
            health.TakeDamage(damage);
        }

        DestroySelf();
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
