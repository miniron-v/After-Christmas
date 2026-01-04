using UnityEngine;

public class UITest : MonoBehaviour
{
    [SerializeField] private ManualUI manualUI;

    [Header("Test Actions")]
    [SerializeField] private ManualActionSO interactAction;
    [SerializeField] private ManualActionSO openMapAction;

    void Update()
    {
        // 1 키 → Interact 조작법
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            manualUI.ShowManual(interactAction);
        }

        // 2 키 → OpenMap 조작법
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            manualUI.ShowManual(openMapAction);
        }
    }
}