Shader "Custom/PixelaterWithGreyscale"
{
    Properties { _MainTex("_MainTex", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Greyscale; // 0: 컬러, 1: 흑백

            half4 Frag(Varyings input) : SV_Target
            {
                // BlitTexture를 통해 들어온 메인 색상 샘플링
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, input.texcoord);
                
                // 루미넌스 계산 (표준 가중치)
                float luminance = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                
                // 흑백 혼합
                col.rgb = lerp(col.rgb, half3(luminance, luminance, luminance), _Greyscale);
                return col;
            }
            ENDHLSL
        }
    }
}