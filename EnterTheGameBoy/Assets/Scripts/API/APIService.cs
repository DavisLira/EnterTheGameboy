using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;

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
}