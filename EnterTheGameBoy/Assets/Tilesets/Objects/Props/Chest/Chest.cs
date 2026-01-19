using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public Sprite closedSprite;
    public Sprite openingSprite;
    public Sprite openSprite;
    private SpriteRenderer sr;

    public bool IsOpened { get; private set; }
    public string ChestID { get; private set; }
    public GameObject itemPrefab;


    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject);
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = closedSprite;
    }

    public bool CanInteract()
    {
        return !IsOpened;
    }

    public void Interact()
    {
        if(!CanInteract()) return;
        StartCoroutine(OpenChest());
    }

    private System.Collections.IEnumerator OpenChest()
    {
        setOpened(true);
        if(itemPrefab)
        {
            sr.sprite = openingSprite;
            yield return new WaitForSeconds(0.3f);
            sr.sprite = openSprite;
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            droppedItem.GetComponent<BouncyEffect>()?.Startbounce();
        }
        
    }

    public void setOpened(bool opened)
    {
        if (IsOpened = opened)
        {
            sr.sprite = openSprite;
        }
    }
}