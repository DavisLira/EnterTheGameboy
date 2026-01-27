using System;

[Serializable]
public class SteamLoginRequest
{
    public string steamId;
    public string username;
}

[Serializable]
public class PlayerData
{
    // MUDANÇA 1: O Mongo retorna '_id', então o nome aqui TEM que ser '_id'
    public string _id; 
    public string steamId;
    public string username;
    public string createdAt;
    public string updatedAt;
}

// MUDANÇA 2: Uma classe auxiliar só para ler o pacote "player": { ... }
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
    public string updatedAt; // A data vem como string do Mongo (ISO 8601)
    public string[] allowedSteamIds; // Whitelist
}

[Serializable]
public class SaveListWrapper
{
    public GameSaveData[] saves;
}

// Classe estática para segurar o Save escolhido na memória
public static class GameSession
{
    public static GameSaveData CurrentSave;
}