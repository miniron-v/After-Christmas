using UnityEngine;
using static DialogueData;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Dialogue System/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterImage;
}
