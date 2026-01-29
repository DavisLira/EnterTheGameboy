using UnityEngine;
using Steamworks;
using Mirror;

public class SaveManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject saveSelectionPanel; // Arraste o Panel_SaveSelection
    public SaveSlotUI[] uiSlots; // Arraste os 3 botões da tela (Slot 1, 2, 3)
    public GameObject loadingIndicator;

    // Função chamada pelo botão "JOGAR" do Menu Principal
    public void OpenSaveMenu()
    {
        saveSelectionPanel.SetActive(true);
        loadingIndicator.SetActive(true);

        // Busca saves na API
        string steamID = SteamUser.GetSteamID().ToString();
        StartCoroutine(APIService.instance.GetMySaves(steamID, OnSavesReceived, OnError));
    }

    void OnSavesReceived(GameSaveData[] saves)
    {
        loadingIndicator.SetActive(false);

        // Preenche os 3 slots (0, 1, 2)
        for (int i = 0; i < 3; i++)
        {
            // Procura se existe save para este slot
            GameSaveData found = null;
            if (saves != null)
            {
                foreach (var s in saves) if (s.slotIndex == i) found = s;
            }

            // Configura o visual do botão
            uiSlots[i].Setup(found, i, this);
        }
    }

    // Chamado quando clica no botão do slot
    public void OnSlotClicked(int index, GameSaveData data)
    {
        if (data != null)
        {
            // Jogo Existente
            Debug.Log($"Carregando Save: {data.name}");
            StartGameWithSave(data);
        }
        else
        {
            // Criar Novo
            string steamID = SteamUser.GetSteamID().ToString();
            string defaultName = $"MUNDO ({index+1})";
            
            loadingIndicator.SetActive(true);
            
            StartCoroutine(APIService.instance.CreateNewSave(steamID, index, defaultName, 
                (newSave) => {
                    StartGameWithSave(newSave);
                }, 
                OnError));
        }
        StartGameWithSave(data);
    }

    void StartGameWithSave(GameSaveData save)
    {
        GameSession.CurrentSave = save;

        // --- CORREÇÃO: DESCOMENTE ISSO ---
        if (NetworkServer.active || NetworkClient.active)
        {
            NetworkManager.singleton.StopHost();
        }
        // ---------------------------------

        if (SteamLobby.Instance != null)
        {
            SteamLobby.Instance.HostLobby();
        }
        else
        {
            NetworkManager.singleton.StartHost();
        }
    }

    public void OnDeleteRequested(GameSaveData data)
    {
        // Opcional: Aqui você poderia abrir um Painel de "Tem certeza?"
        // Vamos fazer direto por enquanto:
        
        string steamID = SteamUser.GetSteamID().ToString();
        
        loadingIndicator.SetActive(true); // Bloqueia a tela

        StartCoroutine(APIService.instance.DeleteSave(data._id, steamID, 
            () => {
                Debug.Log("Save deletado!");
                // Recarrega a lista para o botão voltar a ficar "Vazio"
                OpenSaveMenu(); 
            }, 
            OnError));
    }

    void OnError(string err) { Debug.LogError(err); }
}