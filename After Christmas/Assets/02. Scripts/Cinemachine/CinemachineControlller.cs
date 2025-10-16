using System;
using UnityEngine;
using UnityEngine.Playables;

public class CinemachineControlller : MonoBehaviour
{
    public PlayableDirector cinemachineDirector;

    private bool isPlaying = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("C");
            if(!isPlaying)
            {
                StartCinemachine();
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPlaying)
            {
                StopCinemachine();
            }
        }

        if (isPlaying && cinemachineDirector.state != PlayState.Playing)
        {
            cinemachineDirector.Play();
        }
    }

    private void StartCinemachine()
    {
        isPlaying = true;
        cinemachineDirector.Play();
        Debug.Log("컷신 재생");
    }

    private void StopCinemachine()
    {
        isPlaying = false;
        cinemachineDirector.Pause();
        Debug.Log("컷신 중지");
    }
}
