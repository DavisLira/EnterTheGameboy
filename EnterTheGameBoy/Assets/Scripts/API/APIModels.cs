using System;

[Serializable]
public class SteamLoginRequest
{
    public string steamId;
    public string username;
}

// --- CLASSE NOVA ---
// Usada para enviar os dados no APIService.UpdatePlayerKills
[Serializable]
public class UpdateKillsRequest
{
    public string steamId;
    public int kills;
}
// -------------------

[Serializable]
public class PlayerData
{
    public string _id; 
    public string steamId;
    public string username;
    
    // --- ATUALIZAÇÃO ---
    // Adicionado para receber o total de kills ao fazer login
    public int kills; 
    // -------------------
    
    public string createdAt;
    public string updatedAt;
}

[Serializable]
public class LoginResponseWrapper
{
    public PlayerData player;
}

[Serializable]
public class GameSaveData
{
    public string _id;
    public string hostSteamId;
    public int slotIndex;
    public string name;
    public int level;
    public string updatedAt; 
    public string[] allowedSteamIds; 
}

[Serializable]
public class SaveListWrapper
{
    public GameSaveData[] saves;
}

public static class GameSession
{
    public static GameSaveData CurrentSave;
}