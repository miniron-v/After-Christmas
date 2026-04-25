using System;
using UnityEngine;

public class DialogueModel
{
    public event Action _onLoadedNextSentence;

    public void LoadNextSentence()
    {
        Debug.Log("Model : 다음 문장 로드");
        _onLoadedNextSentence?.Invoke();
    }
}