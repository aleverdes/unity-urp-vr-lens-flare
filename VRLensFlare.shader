Shader "Hidden/AleVerDes/VR Lens Flare"
{
    Properties
    {
        [MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("BaseMap", 2D) = "white" {}
        _IsLeftEye("IsLeftEye", Float) = 0
        _Intensity("Intensity", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalRenderPipeline"
        }
        
        Blend SrcAlpha One

        Pass
        {
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float _IsLeftEye;
            float _Intensity;
            float4 _FlareColorValue;
            float4 _FlareData0; // x: localCos0, y: localSin0, zw: PositionOffsetXY
            float4 _FlareData1; // xy: ScreenPos, zw: FlareSize
            float4 _EyePositions; // xy: LeftEyeScreenPosition, zw: RightEyeScreenPosition 
            CBUFFER_END

            #define _FlareColor             _BaseColor

            #define _LocalCos0              _FlareData0.x
            #define _LocalSin0              _FlareData0.y
            #define _PositionTranslate      _FlareData0.zw

            #define _ScreenPos              _FlareData1.xy
            #define _FlareSize              _FlareData1.zw
            #define _LeftScreenPos          _EyePositions.xy
            #define _RightScreenPos         _EyePositions.zw

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                float2 screenParam = GetScaledScreenParams().xy;
                float screenRatio = screenParam.y / screenParam.x;
                            
                float4 posPreScale = float4(2.0f, 2.0f, 1.0f, 1.0f) * GetQuadVertexPosition(IN.vertexID) - float4(1.0f, 1.0f, 0.0f, 0.0);
                float2 uv = GetQuadTexCoord(IN.vertexID);
                uv.x = 1.0f - uv.x;
                            
                OUT.texcoord.xy = uv;

                posPreScale.xy *= _FlareSize;
                float2 local = float2(posPreScale.x * _LocalCos0 - posPreScale.y * _LocalSin0, posPreScale.x * _LocalSin0 + posPreScale.y * _LocalCos0);

                local.x *= screenRatio;
    
                #if defined(USING_STEREO_MATRICES)
                float2 screenPosition = unity_StereoEyeIndex == 1? _RightScreenPos : _LeftScreenPos;
                # else
                float2 screenPosition = _ScreenPos;
                # endif

                OUT.positionCS.xy = local + screenPosition + _PositionTranslate;
                OUT.positionCS.z = 1.0f;
                OUT.positionCS.w = 1.0f;
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.texcoord);
                c *= _BaseColor;
                return c;
            }
            ENDHLSL
        }
    }
}