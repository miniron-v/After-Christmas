using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "ManualAction",
    menuName = "Manual/Action",
    order = 0)]
public class ManualActionSO : ScriptableObject
{
    public string actionName;

    [Header("Input Keys")]
    public int rowKeyCount;
    public List<KeyCode> keys;

    [Header("UI")]
    public Sprite sprite;
}