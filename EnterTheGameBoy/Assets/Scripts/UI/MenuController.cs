using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    
    private bool isOpen = false;

    void Awake()
    {
        menuCanvas.SetActive(false);
    }

    public void ToggleMenu()
    {
        if (isOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        isOpen = true;
        menuCanvas.SetActive(true);
    }

    public void CloseMenu()
    {
        isOpen = false;
        menuCanvas.SetActive(false);
    }
}