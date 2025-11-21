using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 1f;
    public KeyCode interactKey = KeyCode.E;
    public Vector2 lookDirection = Vector2.down; // padrão: olhando para baixo

    void Update()
    {
        // Atualiza a direção que o player está 
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input != Vector2.zero)
            lookDirection = input.normalized;

        // Quando apertar E, tenta interagir
        if (Input.GetKeyDown(interactKey))
            TryInteract();
    }

    void TryInteract()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookDirection, interactDistance);

        if (hit.collider != null)
        {
            Chest chest = hit.collider.GetComponent<Chest>();

            if (chest != null)
            {
                chest.Interact();
            }
        }

        // Desenhar a linha no editor (só aparece no Scene View)
        Debug.DrawLine(transform.position, transform.position + (Vector3)lookDirection * interactDistance, Color.yellow, 0.2f);
    }
}
