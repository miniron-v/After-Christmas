using UnityEngine;
using static DialogueData;

[CreateAssetMenu(menuName = "New Dialogue/CharacterData")]
public class NewCharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterImage;
}
