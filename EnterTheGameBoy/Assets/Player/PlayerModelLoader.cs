using UnityEngine;
using Mirror;

public class PlayerModelLoader : NetworkBehaviour
{
    [Header("Setup")]
    public Transform modelHolder;
    public CharacterDatabase database; // Arraste o database aqui no inspector do prefab

    [SyncVar(hook = nameof(OnCharacterIdChanged))]
    public int characterIndexSync;

    public void SetCharacter(int index)
    {
        characterIndexSync = index;
    }

    // Quando o valor muda (ou quando o cliente conecta e recebe o valor), isso roda
    void OnCharacterIdChanged(int oldIndex, int newIndex)
    {
        SpawnVisualModel(newIndex);
    }

    void SpawnVisualModel(int index)
    {
        // Limpa modelos antigos se houver
        foreach (Transform child in modelHolder)
        {
            Destroy(child.gameObject);
        }

        // Verifica validade
        if (index < 0 || index >= database.characters.Length) return;

        CharacterData data = database.characters[index];
        
        // Instancia o visual (PunkMan, etc)
        GameObject visual = Instantiate(data.prefab, modelHolder);
        
        // Aqui você pode precisar reconectar o Animator ao seu PlayerMovement se necessário
        // Ex: GetComponent<PlayerMovement>().animator = visual.GetComponent<Animator>();
    }
}