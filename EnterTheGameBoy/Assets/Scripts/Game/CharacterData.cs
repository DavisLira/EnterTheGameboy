using UnityEngine;

[CreateAssetMenu(menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public string characterId;
    public GameObject prefab;
    public Sprite icon;
}