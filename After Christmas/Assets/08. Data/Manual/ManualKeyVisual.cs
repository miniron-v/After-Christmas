using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManualKeyVisual", menuName = "Manual/ManualKeyVisual")]
public class ManualKeyVisual : ScriptableObject
{
    public KeyCode key;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite activeSprite;
}
