using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Referências de Texto")]
    public TMP_Text nameText;    // Título (ex: Mundo de Davis)
    public TMP_Text levelText;   // Info (ex: Nível 5)
    public TMP_Text dateText;    // Data (ex: 27/01/2026)

    [Header("Referências dos Botões (Irmãos)")]
    public Button selectButton;  // O botão grandão transparente (Filho 1)
    public Button deleteButton;  // O botão da lixeira (Filho 2)

    // Variáveis internas para saber quem sou eu
    private GameSaveData myData;
    private int myIndex;
    private SaveManager manager;

    // Função chamada pelo SaveManager para preencher este slot
    public void Setup(GameSaveData data, int index, SaveManager mgr)
    {
        myData = data;
        myIndex = index;
        manager = mgr;

        // 1. Limpa cliques antigos (Segurança)
        selectButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();

        // 2. Configura o comportamento dos botões
        // Botão Principal: Avisa o manager que clicou neste slot (para Jogar ou Criar)
        selectButton.onClick.AddListener(() => manager.OnSlotClicked(myIndex, myData));

        // Botão Lixeira: Avisa o manager para deletar
        deleteButton.onClick.AddListener(() => manager.OnDeleteRequested(myData));

        // 3. Configura o Visual
        if (data != null)
        {
            // --- SAVE EXISTENTE ---
            nameText.text = data.name;
            levelText.text = $"NIVEL {data.level}";

            // Formatação bonita da data
            if (DateTime.TryParse(data.updatedAt, out DateTime date))
            {
                dateText.text = date.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                dateText.text = "--/--/--";
            }

            // Mostra a lixeira (só pode apagar se existir save)
            deleteButton.gameObject.SetActive(true);
        }
        else
        {
            // --- SLOT VAZIO ---
            nameText.text = $"SLOT    {index + 1}";
            levelText.text = "NOVO   JOGO";
            dateText.text = ""; // Sem data

            // Esconde a lixeira (não dá para apagar o vazio)
            deleteButton.gameObject.SetActive(false);
        }
    }
}