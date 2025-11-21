using UnityEngine;

public class Chest : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openingSprite;
    public Sprite openSprite;

    private SpriteRenderer sr;
    private bool opened = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = closedSprite;
    }

    public void Interact()
    {
        if (opened) return;

        opened = true;
        StartCoroutine(OpenChest());
    }

    private System.Collections.IEnumerator OpenChest()
    {
        sr.sprite = openingSprite;
        yield return new WaitForSeconds(0.3f);
        sr.sprite = openSprite;
    }
}
