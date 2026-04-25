using System;
using UnityEngine;

public class DialogueView : MonoBehaviour
{
    public event Action _onClicked;

    public void OnClick()
    {
        Debug.Log($"View({gameObject.name}): 클릭됨");
        _onClicked?.Invoke();
    }

    public void ShowNextSentence()
    {
        Debug.Log($"View({gameObject.name}): 다음 문장 보여주기");
    }
}
