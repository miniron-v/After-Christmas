using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PixelaterAndGreyScalePass : ScriptableRenderPass
{
    private PixelaterAndGreyScaleFeature.Settings settings;
    private RTHandle tempTexture;

    // 패스 간에 데이터를 전달할 구조체
    private class PassData
    {
        public TextureHandle source;
        public Material material;
        public float greyscale;
    }

    public PixelaterAndGreyScalePass(PixelaterAndGreyScaleFeature.Settings settings)
    {
        this.settings = settings;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        TextureHandle cameraColor = resourceData.cameraColor;
        if (!cameraColor.IsValid()) return;

        // 1. 해상도 계산 (Pixelate 수치에 따라 결정)
        var desc = cameraData.cameraTargetDescriptor;
        int width = Mathf.Max(1, desc.width / settings.pixelate);
        int height = Mathf.Max(1, desc.height / settings.pixelate);
        desc.width = width;
        desc.height = height;
        desc.depthBufferBits = 0;

        // 2. 임시 텍스처 할당 (Point 필터링으로 도트 질감 유지)
        RenderingUtils.ReAllocateHandleIfNeeded(ref tempTexture, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_TempPixelTexture");
        TextureHandle tempTextureHandle = renderGraph.ImportTexture(tempTexture);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Downsample Pass", out var passData))
        {
            passData.source = cameraColor;
            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.SetRenderAttachment(tempTextureHandle, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                // 단순히 이미지를 작게 구겨 넣습니다. (Point 필터링이라 픽셀이 뭉쳐짐)
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0.0f, false);
            });
        }

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Upsample and Greyscale Pass", out var passData))
        {
            passData.source = tempTextureHandle;
            passData.material = settings.material;
            passData.greyscale = settings.greyscale;

            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                if (data.material != null)
                {
                    // 셰이더의 _Greyscale 변수 독립적으로 제어
                    data.material.SetFloat("_Greyscale", data.greyscale);
                    
                    // 셰이더를 사용하여 확대 블릿
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                }
                else
                {
                    // 머티리얼이 없을 경우를 대비한 기본 블릿
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0.0f, false);
                }
            });
        }
    }

    public void Dispose()
    {
        tempTexture?.Release();
    }
}