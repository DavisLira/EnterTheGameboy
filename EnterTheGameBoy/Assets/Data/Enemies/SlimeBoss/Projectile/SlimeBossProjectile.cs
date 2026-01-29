using UnityEngine;
using Mirror;

public class SlimeBossProjectile : NetworkBehaviour
{
    [Header("Config")]
    public float speed = 8f;
    public int damage = 1;
    public float lifeTime = 5f;

    // 1. [SyncVar] faz essa variável ser enviada do Servidor para os Clientes automaticamente ao Spawnar
    [SyncVar] 
    Vector2 direction;

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), lifeTime);
    }

    [Server]
    public void Init(Vector2 dir)
    {
        // Como Init é chamado ANTES do NetworkServer.Spawn no seu script do Boss,
        // o valor dessa SyncVar será enviado junto com o objeto na criação.
        direction = dir.normalized;
    }

    void FixedUpdate()
    {
        // 2. REMOVEMOS O "if (!isServer) return;"
        // Agora tanto o Servidor quanto o Cliente executam o movimento.
        // Como ambos têm a mesma "direction" (graças ao SyncVar) e a mesma "speed",
        // a bala andará igual nos dois computadores.
        
        transform.position += (Vector3)(direction * speed * Time.fixedDeltaTime);
    }

    // 3. A colisão continua sendo APENAS no Servidor para evitar batidas duplas ou cheater
    [ServerCallback]
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Tenta pegar componentes no objeto ou nos pais (caso o collider esteja num filho)
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
