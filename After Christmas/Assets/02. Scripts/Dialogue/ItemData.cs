using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Dialogue System/ItemInfo Data")]
public class ItemData : ScriptableObject
{
    // 대화에 나오는 전체 인원
    [Serializable]
    public class ItemInfo
    {
        [TextArea(3, 10)]
        public string sentence;
    }

    public ItemInfo[] itemInfo;
}
