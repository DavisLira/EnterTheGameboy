using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMenuInput : NetworkBehaviour
{
    public void OnOpenCloseMenu(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer || !context.performed)
            return;

        var menu = FindFirstObjectByType<MenuController>();
        if (menu != null)
            menu.ToggleMenu();
    }
}