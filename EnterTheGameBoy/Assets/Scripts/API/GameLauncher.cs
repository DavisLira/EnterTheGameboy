using UnityEngine;
using Steamworks; // Requer Steamworks.NET instalado
using System.Collections;

public class GameLauncher : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingScreen;
    public GameObject mainMenu;
    public GameObject errorText; // Opcional

    void Start()
    {
        // Garante que a Steam está rodando
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam não iniciada! O jogo vai fechar ou rodar em modo offline.");
            return;
        }

        StartCoroutine(AuthenticateUser());
    }

    IEnumerator AuthenticateUser()
    {
        // 1. Pega dados da Steam
        CSteamID steamID = SteamUser.GetSteamID();
        string sIDString = steamID.ToString();
        string sName = SteamFriends.GetPersonaName();

        Debug.Log($"[Steam] Usuário: {sName} (ID: {sIDString})");

        // 2. Chama a API
        // Ele vai esperar a resposta do servidor antes de continuar
        yield return APIService.instance.LoginWithSteam(
            sIDString, 
            sName, 
            OnLoginSuccess, 
            OnLoginFail
        );
    }

    void OnLoginSuccess(PlayerData player)  
    {
        // Mude de player.id para player._id
        Debug.Log($"[Login] Bem-vindo, {player.username}! Seu ID no banco é: {player._id}");
        
        // ... resto do código igual ...
        if(loadingScreen) loadingScreen.SetActive(false);
        if(mainMenu) mainMenu.SetActive(true);
    }

    void OnLoginFail(string error)
    {
        Debug.LogError($"[Login Falhou] Motivo: {error}");
        // Mostrar popup de erro para o jogador...
    }
}