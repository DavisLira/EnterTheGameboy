using UnityEngine;

public class DungeonDoor : MonoBehaviour
{
    private Animator anim;
    private BoxCollider2D col;

    void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        
        // Garante estado inicial: Porta aberta (chão)
        // Colisor desligado para o player andar por cima
        SetDoorState(false); 
    }

    // Função chamada pelo DungeonRoom
    public void SetDoorState(bool isClosed)
    {
        // 1. Controla a Animação
        if(anim != null)
        {
            anim.SetBool("IsClosed", isClosed);
        }

        // 2. Controla a Física
        // Se isClosed = true (Fechada), o colisor liga e vira parede.
        // Se isClosed = false (Aberta), o colisor desliga e vira chão.
        if(col != null)
        {
            col.enabled = isClosed;
        }
    }
}