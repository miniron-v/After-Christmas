using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Item/ItemData")]
public class ItemDataSO : ScriptableObject
{
    public string itemID;
    public Sprite icon;
    [TextArea] public string description;
}
