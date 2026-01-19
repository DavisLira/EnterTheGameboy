using UnityEngine;
using Mirror; // Importante
using TMPro;  // Se estiver usando TextMeshPro para o InputField

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;       // Arraste o objeto "MainPanel" aqui
    public GameObject connectionPanel; // Arraste o objeto "ConnectionPanel" aqui

    [Header("Inputs")]
    public TMP_InputField codeInput;   // Arraste seu Input Field aqui (se for TMP)
    // public InputField codeInput;    // Use esta linha se for o Input Field antigo do Unity

    void Start()
    {
        // Garante que começa no menu certo
        ShowMainPanel();
    }

    // --- Navegação de UI ---

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        connectionPanel.SetActive(false);
    }

    public void OnPlay() // O botão "Jogar" chama isso
    {
        mainPanel.SetActive(false);
        connectionPanel.SetActive(true);
    }

    public void OnBack() // O botão "Voltar" do painel de conexão chama isso
    {
        ShowMainPanel();
    }

    // --- Lógica do Mirror ---

    public void OnCreateRoom() // Botão "Criar Sala"
    {
        // Inicia o Host. Como configuramos a "Room Scene" no NetworkManager,
        // ele vai carregar a CharacterSelect automaticamente.
        NetworkManager.singleton.StartHost();
    }

    public void OnJoinRoom() // Botão "Entrar"
    {
        string address = "localhost"; 
        
        // Futuramente, aqui vamos converter o CÓDIGO para IP ou usar o Relay.
        // Por enquanto, se o input estiver vazio, entra localmente.
        if (codeInput != null && !string.IsNullOrEmpty(codeInput.text))
        {
            address = codeInput.text; 
        }

        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();
    }

    // --- Outros Botões ---

    public void OnUpgrades()
    {
        Debug.Log("Upgrades (em breve)");
    }

    public void OnSettings()
    {
        Debug.Log("Configurações (em breve)");
    }

    public void OnExit()
    {
        Application.Quit();
    }
}