using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PixelaterAndGreyScaleFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Header("Pixelate Setting")]
        [Range(1, 200)] public int pixelate = 1;

        [Header("Greyscale Setting")]
        [Range(0, 1)] public float greyscale = 0f; // 흑백 수치 (0: 컬러, 1: 흑백)
        public Material material;

        [Header("Render Timing")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public Settings settings = new Settings();
    private PixelaterAndGreyScalePass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new PixelaterAndGreyScalePass(settings);
        m_ScriptablePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    protected override void Dispose(bool disposing)
    {
        m_ScriptablePass?.Dispose();
    }
}