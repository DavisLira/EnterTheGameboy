using UnityEngine;
using UnityEngine.InputSystem;
using Mirror; // Adicione Mirror

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;
    private NetworkIdentity netIdentity; // Para checar se sou eu

    void Start()
    {
        interactionIcon.SetActive(false);
        netIdentity = GetComponent<NetworkIdentity>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        // SEGURANÇA MULTIPLAYER:
        // Só executa se esse script estiver no Player que eu estou controlando
        if (netIdentity != null && !netIdentity.isLocalPlayer) return;

        if (context.performed)
        {
            // Chama o Interact normal. A mágica do multiplayer vai acontecer DENTRO do objeto.
            interactableInRange?.Interact();
            
            // Opcional: Esconder ícone após interagir
            // interactionIcon.SetActive(false); 
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Só mostra ícone e detecta se for o MEU player colidindo
        if (netIdentity != null && !netIdentity.isLocalPlayer) return;

        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (netIdentity != null && !netIdentity.isLocalPlayer) return;

        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}