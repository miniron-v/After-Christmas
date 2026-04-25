using UnityEngine;
using static DialogueData;

[CreateAssetMenu(menuName = "Dialogue/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterImage;
}
