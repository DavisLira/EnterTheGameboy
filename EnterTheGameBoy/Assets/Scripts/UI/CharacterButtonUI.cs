using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    public Image backgroundImage; // <--- ARRASTE A IMAGEM DE FUNDO DO BOTÃO AQUI NO INSPECTOR

    private CharacterData data; // Mantivemos o seu tipo 'CharacterData'
    private CharacterSelectController controller;
    private int characterIndex;

    // Ajustei o tipo do primeiro parâmetro para 'CharacterData'
    public void Setup(CharacterData character, CharacterSelectController ctrl, int index)
    {
        data = character;
        controller = ctrl;
        characterIndex = index;

        if (icon != null)
        {
            icon.sprite = data.icon;
        }
        
        // Garante que comece com a cor padrão (Branco)
        SetSelectedState(false);
    }

    public void OnClick()
    {
        controller.OnSelectCharacter(characterIndex);
    }

    // --- NOVA FUNÇÃO PARA MUDAR A COR ---
    // O Controller vai chamar isso para pintar de verde ou branco
    public void SetSelectedState(bool isSelected)
    {
        if (backgroundImage != null)
        {
            // Se selecionado = Verde, Se não = Branco
            backgroundImage.color = isSelected ? Color.green : Color.white;
        }
    }
}