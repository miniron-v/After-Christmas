using System;
using UnityEngine;

[Obsolete("이제 안 씀")]
[CreateAssetMenu(fileName = "NewMapData", menuName = "Map/MapDataSO")]
public class MapDataSO : ScriptableObject
{
    public string mapID;           // 고유 ID
    public Sprite mapIcon;         // UI용 아이콘
    [TextArea] public string description; // 맵 설명
}
