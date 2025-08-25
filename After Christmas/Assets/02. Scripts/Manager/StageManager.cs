using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private int requiredInteractionCount;
    private int currentInteractionCount = 0;

    private void OnEnable()
    {
        Item.interactWithItem += IncreaseCount;
    }

    private void OnDisable()
    {
        Item.interactWithItem -= IncreaseCount;
    }

    private void IncreaseCount()
    {
        currentInteractionCount++;
        if (isClearable())
        {
            StageClear();
        }
    }

    private bool isClearable()
    {
        return requiredInteractionCount == currentInteractionCount;
    }

    private void StageClear()
    {
        // 씬 전환 같은거 넣으면 될듯
        Debug.Log("클리어");
    }
}
