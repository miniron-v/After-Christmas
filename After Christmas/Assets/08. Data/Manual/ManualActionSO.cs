using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "ManualAction",
    menuName = "Manual/Action",
    order = 0)]
public class ManualActionSO : ScriptableObject
{
    public string actionName;

    [Header("Keys")]
    public List<ManualKeyVisual> keyVisuals;
}
