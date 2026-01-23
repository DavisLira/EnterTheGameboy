using Steamworks;
using UnityEngine;

public class SteamTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!SteamManager.Initialized) {
            Debug.LogError("Steam is not initialized.");
            return;
        }

        string name = SteamFriends.GetPersonaName();
        Debug.Log("Steam Name: " + name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
