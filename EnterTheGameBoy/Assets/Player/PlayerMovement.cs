using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Vector2 moveInput = Vector2.zero;
    private Vector2 LastNonZeroDirection = Vector2.down;

    [SyncVar] public float syncMoveX;
    [SyncVar] public float syncMoveY;
    [SyncVar] public bool syncIsMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!NetworkClient.ready || !isLocalPlayer) return;
        
        if (!isLocalPlayer) return;

        moveInput = context.ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
            LastNonZeroDirection = moveInput.normalized;

        // Atualiza variáveis locais e manda para o servidor
        UpdateAnimationParamsAndSync(moveInput);
    }

    // Atualiza apenas dados (local) e notifica servidor
    private void UpdateAnimationParamsAndSync(Vector2 input)
    {
        bool isMoving = input.sqrMagnitude > 0.01f;

        // Atualiza somente as propriedades locais (para uso instantâneo)
        // (PlayerAnimator vai ler moveInput / LastNonZeroDirection quando isLocalPlayer)

        // Envia para o servidor atualizar as SyncVars (servidor é a fonte da verdade para remotos)
        CmdSyncAnimation(isMoving,
                         isMoving ? input.normalized.x : LastNonZeroDirection.x,
                         isMoving ? input.normalized.y : LastNonZeroDirection.y);
    }

    [Command]
    void CmdSyncAnimation(bool isMoving, float mx, float my)
    {
        syncIsMoving = isMoving;
        syncMoveX = mx;
        syncMoveY = my;
    }

    void FixedUpdate()
    {
        if (!NetworkClient.ready) return;

        if (!isLocalPlayer) return;

        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    // Função chamada pelo Servidor (DungeonRoom)
    [Server]
    public void ForceTeleport(Vector3 position)
    {
        // 1. Move no servidor (para validar)
        transform.position = position;
        
        // 2. Manda o dono do boneco mover também (para evitar glitch visual)
        TargetTeleport(position);
    }

    [TargetRpc] // Roda APENAS no cliente dono deste boneco
    void TargetTeleport(Vector3 position)
    {
        transform.position = position;
        
        // Se tiver Rigidbody, zera a velocidade para não sair voando
        if (TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero; // Unity 6 (use .velocity em versões antigas)
            rb.angularVelocity = 0f;
        }
    }
}