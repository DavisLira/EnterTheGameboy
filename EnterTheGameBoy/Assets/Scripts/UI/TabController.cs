using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    [Header("Tabs Configuration")]
    public Image[] tabImages;
    public Sprite[] tabImagesInactive;
    public Sprite[] tabImagesActive;

    [Header("Pages configuration")]
    public GameObject[] pages;

    void Start()
    {
        ActivateTab(0);
    }

    public void ActivateTab(int index)
    {
        // Ativa/desativa páginas
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
        }

        // Atualiza sprites
        for (int i = 0; i < tabImages.Length; i++)
        {
            if (i == index)
                tabImages[i].sprite = tabImagesActive[i];
            else
                tabImages[i].sprite = tabImagesInactive[i];
        }
    }
}