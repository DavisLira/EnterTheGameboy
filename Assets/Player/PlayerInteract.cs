using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 1f;
    public Vector2 lookDirection = Vector2.down;

    private Vector2 lastMovement;

    // Atualiza a direção do player
    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        if (input != Vector2.zero)
            lookDirection = input.normalized;

        lastMovement = input;
    }

    // Evento do botão de interação (E)
    public void OnInteract()
    {
        TryInteract();
    }

    void TryInteract()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookDirection, interactDistance);

        if (hit.collider != null)
        {
            Chest chest = hit.collider.GetComponent<Chest>();

            if (chest != null)
                chest.Interact();
        }

        Debug.DrawLine(transform.position,
        transform.position + (Vector3)lookDirection * interactDistance,
        Color.yellow, 0.2f);
    }
}
