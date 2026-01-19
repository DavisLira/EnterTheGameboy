using UnityEngine;

public class Weapon : MonoBehaviour, IInteractable
{
    public bool IsOnGround { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public bool CanInteract()
    {
        Debug.Log("Pode interagir com a arma!");
        return true;
    }

    public void Interact()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
