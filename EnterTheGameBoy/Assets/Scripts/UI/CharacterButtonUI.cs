using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    private CharacterData data;
    private CharacterSelectController controller;
    private int characterIndex; // Novo: Guarda o ID desse botão

    // Adicionei o parâmetro 'index' aqui
    public void Setup(CharacterData character, CharacterSelectController ctrl, int index)
    {
        data = character;
        controller = ctrl;
        characterIndex = index;

        if (icon != null)
        {
            icon.sprite = data.icon;
        }
    }

    public void OnClick()
    {
        // Agora chamamos o método que aceita INT no controller
        controller.OnSelectCharacter(characterIndex);
    }
}