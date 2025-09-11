using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene("itemHandlerScene2");
    }

    void Update()
    {
        // 임시 씬이동 테스트(O,P == 1,2번 씬)
        if (Input.GetKeyDown(KeyCode.O))
        {
            SceneManager.LoadScene("itemHandlerScene");
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene("itemHandlerScene2");
        }
    }
}
