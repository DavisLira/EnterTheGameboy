using UnityEngine;
using Mirror;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Vida")]
    public int maxHealth = 6;
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int currentHealth;

    [Header("Estado de Morte")]
    [SyncVar(hook = nameof(OnStateChanged))]
    public bool isDowned = false; 

    [Header("Reviver & Cores")]
    public float timeToRevive = 5f; 
    public Color normalColor = Color.white;
    public Color deadColor = Color.red;      // Cor quando morre
    public Color revivingColor = Color.green; // Cor alvo enquanto revive

    [Header("Feedback de Dano")]
    public Color damageColor = Color.red; // Cor da piscada
    private Coroutine flashCoroutine;     // Para controlar a piscada e não bugar se tomar muito tiro rapido

    // Sincroniza o progresso (0.0 a 1.0) para todos verem a cor mudando
    [SyncVar(hook = nameof(OnProgressChanged))]
    private float reviveProgress = 0f;

    private float reviveTimer = 0f;

    // Referências (Arraste no Inspector!)
    public SpriteRenderer visualRenderer; 
    public Animator visualAnimator;       
    
    private PlayerMovement moveScript; 
    private PlayerWeaponController weaponScript; 
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveScript = GetComponent<PlayerMovement>(); 
        weaponScript = GetComponent<PlayerWeaponController>();
    }

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (isDowned) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (currentHealth <= 0)
        {
            isDowned = true; 
        }
    }

    // --- LÓGICA DE REVIVER ---
    [ServerCallback]
    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDowned && collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth friend = collision.gameObject.GetComponent<PlayerHealth>();
            if (friend != null && !friend.isDowned)
            {
                reviveTimer += Time.fixedDeltaTime;
                
                // Atualiza a SyncVar (Isso avisa os clientes para mudarem a cor)
                reviveProgress = Mathf.Clamp01(reviveTimer / timeToRevive);

                if (reviveTimer >= timeToRevive) Revive();
            }
        }
    }

    [ServerCallback]
    void OnCollisionExit2D(Collision2D collision)
    {
        if (isDowned)
        {
            reviveTimer = 0f;
            reviveProgress = 0f; // Reseta a cor para vermelho
        }
    }

    [Server]
    void Revive()
    {
        isDowned = false;
        currentHealth = maxHealth / 2;
        reviveTimer = 0f;
        reviveProgress = 0f;
    }

    // --- VISUAL (Hooks) ---

    // 1. Hook do Progresso (Mistura Vermelho com Verde)
    void OnProgressChanged(float oldVal, float newVal)
    {
        if (isDowned)
        {
            SpriteRenderer[] sprites = GetModelSprites();
            foreach(var sprite in sprites)
            {
                sprite.color = Color.Lerp(deadColor, revivingColor, newVal);
            }
        }
    }

    // 2. Hook de Estado (Morte/Vida)
    void OnStateChanged(bool oldVal, bool isDown)
    {
        // 1. Busca os Sprites no lugar CERTO (Dentro do ModelHolder)
        // Usamos uma função auxiliar para não poluir este método
        SpriteRenderer[] characterSprites = GetModelSprites();
        
        // Busca o Animator também dentro do ModelHolder
        Animator charAnim = GetModelAnimator();

        // A. Trava Física (igual antes)
        if (rb)
        {
            if (isDown)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        // B. Lógica (igual antes)
        if (moveScript) moveScript.enabled = !isDown;
        if (weaponScript) weaponScript.SetWeaponActive(!isDown);

        // C. VISUAL (A Correção)
        if (isDown)
        {
            // MORREU
            if (charAnim) charAnim.enabled = false; 
            
            // Pinta TODOS os sprites do personagem (Corpo, Cabeça, etc)
            foreach(var sprite in characterSprites)
            {
                sprite.color = deadColor; 
            }
        }
        else
        {
            // REVIVEU
            if (charAnim) charAnim.enabled = true;
            
            foreach(var sprite in characterSprites)
            {
                sprite.color = normalColor;
            }
        }
        
        // D. EXTRA: Esconder o ícone de interação se morrer
        // Se você quiser garantir que o balão suma quando morre:
        var detector = GetComponentInChildren<InteractionDetector>();
        if (detector != null && detector.gameObject.activeSelf)
        {
            detector.gameObject.SetActive(!isDown);
        }
    }

    // --- FUNÇÕES AUXILIARES PARA ACHAR O MODELO ---
    
    private SpriteRenderer[] GetModelSprites()
    {
        // Procura especificamente o objeto chamado "ModelHolder"
        Transform modelHolder = transform.Find("ModelHolder");
        
        if (modelHolder != null)
        {
            // Retorna apenas os sprites que estão DENTRO do ModelHolder
            return modelHolder.GetComponentsInChildren<SpriteRenderer>();
        }
        
        // Fallback: Se não achar ModelHolder, procura em tudo mas tenta ignorar o balão
        Debug.LogWarning("PlayerHealth: ModelHolder não encontrado! Procurando genericamente.");
        return GetComponentsInChildren<SpriteRenderer>();
    }

    private Animator GetModelAnimator()
    {
        Transform modelHolder = transform.Find("ModelHolder");
        if (modelHolder != null)
        {
            return modelHolder.GetComponentInChildren<Animator>();
        }
        return GetComponentInChildren<Animator>();
    }

    void OnHealthChanged(int oldVal, int newVal) 
    {
        // Só pisca se:
        // 1. A vida diminuiu (Dano)
        // 2. Não estou caído (se estiver caído, a cor já é vermelha fixa)
        if (newVal < oldVal && !isDowned)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashDamageEffect());
        }
    }

    IEnumerator FlashDamageEffect()
    {
        SpriteRenderer[] sprites = GetModelSprites();

        // 1. Pinta de vermelho
        foreach(var sprite in sprites) sprite.color = damageColor;

        // 2. Espera 0.2s
        yield return new WaitForSeconds(0.2f);

        // 3. Volta para a cor normal (SE não tiver morrido nesse meio tempo)
        if (!isDowned)
        {
            foreach(var sprite in sprites) sprite.color = normalColor;
        }
    }
}