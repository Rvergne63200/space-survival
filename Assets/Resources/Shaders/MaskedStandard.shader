// Standard-like surface shader of the buildings (domes) that hides what is inside the masks of the structures
// (StructureMask: the dome connectors open the dome wall where they are). Masks: unit boxes, given as world to
// local matrices by StructureMask (_StructureMaskCount, _StructureMasks). Shadows are masked too (addshadow)
Shader "SpaceSurvival/Masked Standard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [HDR] _EmissionColor ("Emission", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow 
        #pragma target 3.5

        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        half4 _EmissionColor;

        int _StructureMaskCount;
        float4x4 _StructureMasks[32];

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            for (int i = 0; i < _StructureMaskCount; i++)
            {
                float3 p = mul(_StructureMasks[i], float4(IN.worldPos, 1)).xyz;
                if (all(abs(p) < 0.5)) discard;
            }

            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = _EmissionColor.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Standard"
}
