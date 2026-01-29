using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;

public class APIService : MonoBehaviour
{
    public static APIService instance;

    // IMPORTANTE: Coloque o caminho até onde o 'playerRouter' é carregado.
    // Se no seu app.ts for: app.use("/players", playerRouter), então aqui fica ".../players"
    private string baseURL = "http://cs0w4g4k80gk0sowwgccsc4c.76.13.161.103.sslip.io"; 

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public IEnumerator LoginWithSteam(string steamId, string username, Action<PlayerData> onSuccess, Action<string> onError)
    {
        string endpoint = "/players/steam-login"; // Definido no player.routes.ts
        string url = baseURL + endpoint;

        // Cria o objeto de envio
        SteamLoginRequest payload = new SteamLoginRequest 
        { 
            steamId = steamId, 
            username = username 
        };

        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[API] Tentando login em: {url}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[API Error] {request.error} : {request.downloadHandler.text}");
                onError?.Invoke(request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"[API Success] Recebido: {jsonResponse}");

                try 
                {
                    // CORREÇÃO: Deserializa o "Pacote" primeiro
                    LoginResponseWrapper wrapper = JsonUtility.FromJson<LoginResponseWrapper>(jsonResponse);
                    
                    // Verifica se o pacote veio com o player dentro
                    if (wrapper != null && wrapper.player != null && !string.IsNullOrEmpty(wrapper.player._id))
                    {
                        // Sucesso! Passa apenas os dados do player para o jogo
                        onSuccess?.Invoke(wrapper.player);
                    }
                    else
                    {
                        Debug.LogError($"JSON lido, mas campos vazios. Wrapper: {wrapper}, Player: {wrapper?.player}");
                        onError?.Invoke("JSON estruturado incorretamente ou ID vazio.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[JSON Parse Error] {e.Message}");
                    onError?.Invoke("Erro ao ler resposta JSON.");
                }
            }
        }
    }

    public IEnumerator GetMySaves(string steamId, Action<GameSaveData[]> onSuccess, Action<string> onError)
    {
        string url = baseURL + "/saves/" + steamId;

        Debug.Log($"[API Check] Tentando acessar: {url}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                // O JSON vem como { "saves": [ ... ] }
                SaveListWrapper wrapper = JsonUtility.FromJson<SaveListWrapper>(json);
                onSuccess?.Invoke(wrapper.saves);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    public IEnumerator CreateNewSave(string steamId, int slotIndex, string worldName, Action<GameSaveData> onSuccess, Action<string> onError)
    {
        string url = baseURL + "/saves/create";
        
        // 1. Cria o objeto anônimo com os dados
        var payload = new 
        { 
            steamId = steamId, 
            slotIndex = slotIndex, 
            name = worldName 
        };
        
        // 2. Transforma em JSON (Texto)
        // OBS: O JsonUtility da Unity não serializa objetos anônimos muito bem. 
        // Se der erro aqui, crie uma classe ou struct simples para isso. 
        // Mas vamos tentar o método "hardcode" seguro se você não tiver Newtonsoft.Json:
        string json = $"{{\"steamId\":\"{steamId}\",\"slotIndex\":{slotIndex},\"name\":\"{worldName}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[API] Criando save: {json}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log($"[API] Save criado: {responseJson}");

                try 
                {
                    // Converte a resposta de volta para GameSaveData
                    GameSaveData newSave = JsonUtility.FromJson<GameSaveData>(responseJson);
                    onSuccess?.Invoke(newSave);
                }
                catch (Exception e)
                {
                    onError?.Invoke("Erro ao ler resposta do save: " + e.Message);
                }
            }
            else
            {
                Debug.LogError($"[API Error] {request.error}: {request.downloadHandler.text}");
                onError?.Invoke(request.error);
            }
        }
    }

    // No APIService.cs
    public IEnumerator AddPlayerToSave(string saveId, string newSteamId, Action<string> onSuccess, Action<string> onError)
    {
        string url = baseURL + "/saves/add-player";
        var payload = new { saveId = saveId, newSteamId = newSteamId };
        
        // Copie a lógica de POST do CreateSave ou Login...
        // ...
        yield return null; 
    }

    public IEnumerator DeleteSave(string saveId, string steamId, Action onSuccess, Action<string> onError)
    {
        string url = baseURL + "/saves/delete";
        
        // Objeto anônimo
        var payload = new { saveId = saveId, steamId = steamId };
        string json = $"{{\"saveId\":\"{saveId}\",\"steamId\":\"{steamId}\"}}"; // Hardcode JSON seguro

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    public IEnumerator UpdateSaveWhitelist(string saveId, string hostSteamId, List<string> playerIds, Action onSuccess, Action<string> onError)
    {
        string url = baseURL + "/saves/update-whitelist";
        
        // Criar um objeto simples para serializar a lista de strings
        var payload = new 
        { 
            saveId = saveId, 
            steamId = hostSteamId, 
            playerIds = playerIds 
        };

        // Dica: JsonUtility da Unity sofre com listas de strings simples dentro de object anonimo. 
        // Se der erro de JSON, teremos que fazer string manual, mas tente assim primeiro:
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(payload); 
        // ^ RECOMENDO USAR NEWTONSOFT.JSON (Pacote JSON .NET for Unity) para listas complexas.
        // Se não tiver, me avise que faço a versão manual string builder.

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    public IEnumerator UpdatePlayerKills(string steamId, int totalKills, Action onSuccess, Action<string> onError)
    {
        // Certifique-se que essa rota existe na sua API Node.js
        string url = baseURL + "/players/update-kills"; 

        // MUDANÇA: Usando a classe nova em vez de escrever string manual
        UpdateKillsRequest payload = new UpdateKillsRequest 
        { 
            steamId = steamId, 
            kills = totalKills 
        };
    
        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) onSuccess?.Invoke();
            else onError?.Invoke(request.error);
        }
    }
}