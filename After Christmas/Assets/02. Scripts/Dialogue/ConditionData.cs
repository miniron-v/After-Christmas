using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Condition", menuName = "Dialogue System/Condition Data")]
public class ConditionData : ScriptableObject
{
    [Header("1. 아이템 조건")]
    public string requiredItemID = "";
    public bool mustHaveItem = true;

    [Header("2. 선행 대화 조건")]
    [Tooltip("선행 대화의 고유 ID. 0이면 조건 없음.")]
    public int prerequisiteDialogueID = 0;
    public bool mustHaveCompletedDialogue = true;

    public bool CheckCondition(ItemInfoManager manager)
    {
        // 1. 아이템 조건 검사
        if (!string.IsNullOrEmpty(requiredItemID))
        {
            if (manager.IsHavingItem(requiredItemID) != mustHaveItem)
                return false;
        }

        return true;
    }
}