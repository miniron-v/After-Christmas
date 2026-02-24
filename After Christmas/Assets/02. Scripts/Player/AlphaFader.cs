using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlphaFader : MonoBehaviour
{
    
    private string transparencyProp = "_Tweak_transparency";
    private float transparentAlphaValue = -1.0f;
    private float normalAlphaValue = 0.0f;
    [SerializeField] private float fadeDuration;

    private List<Renderer> renderers = new List<Renderer>();
    private MaterialPropertyBlock propBlock;
    private Coroutine activeFadeRoutine;
    private int transparencyId;
    private CapsuleCollider cd;
    private Rigidbody rb;

    void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        CinematicController.OnStart += SetTransparentInstant;
        CinematicController.OnEnd += SetOpaqueGradual;
    }

    private void OnDisable()
    {
        CinematicController.OnStart -= SetTransparentInstant;
        CinematicController.OnEnd -= SetOpaqueGradual;
    }

    public void CacheRenderers()
    {
        renderers.Clear();
        Renderer[] children = GetComponentsInChildren<Renderer>(true);
        foreach (var r in children)
        {
            renderers.Add(r);
        }
    }

    public void SetTransparentInstant()
    {
        StopActiveRoutine();
        ApplyTransparency(transparentAlphaValue);
        TogglePhysics(false);
    }

    public void SetOpaqueGradual()
    {
        StopActiveRoutine();
        TogglePhysics(true);
        activeFadeRoutine = StartCoroutine(FadeRoutine(normalAlphaValue));
    }

    private void ApplyTransparency(float alpha)
    {
        float clampedAlpha = Mathf.Clamp(alpha, -1.0f, 0.0f);

        foreach (var r in renderers)
        {
            r.GetPropertyBlock(propBlock);
            propBlock.SetFloat(transparencyId, clampedAlpha);
            r.SetPropertyBlock(propBlock);
        }
    }

    private IEnumerator FadeRoutine(float targetValue)
    {
        float startValue = GetCurrentTransparency();
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerpValue = Mathf.Lerp(startValue, targetValue, elapsedTime / fadeDuration);

            ApplyTransparency(lerpValue);
            yield return null;
        }

        ApplyTransparency(targetValue);
        activeFadeRoutine = null;
    }

    private float GetCurrentTransparency()
    {
        if (renderers.Count > 0)
        {
            renderers[0].GetPropertyBlock(propBlock);
            return propBlock.GetFloat(transparencyId);
        }
        return transparentAlphaValue;
    }

    private void StopActiveRoutine()
    {
        if (activeFadeRoutine != null)
        {
            StopCoroutine(activeFadeRoutine);
            activeFadeRoutine = null;
        }
    }

    // 셰이더만 제어할 게 아니라, 시네마틱 감안한다면 물리연산, 콜라이더도 꺼야 함
    private void TogglePhysics(bool isEnabled)
    {
        if (cd != null) 
        {
            cd.enabled = isEnabled;
        }
        if (rb != null)
        {
            // 활성화 상태 - Kinematic 끔
            // 비활성화(시네마틱) 상태 - Kinematic켜기
            rb.isKinematic = !isEnabled;
            if (isEnabled)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }



    private void Init()
    {
        transparencyId = Shader.PropertyToID(transparencyProp);
        propBlock = new MaterialPropertyBlock();

        CacheRenderers();

        SetTransparentInstant();
        cd = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }
}