Shader "Hidden/InteractableOutline"
{
    Properties
    {
        _MainTex ("Mask", 2D) = "black" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width (px)", Float) = 3
    }

    SubShader
    {
        ZWrite Off
        ZTest Always
        Cull Off

        // Pass 0: silhouette of the object into the mask texture
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 vert(float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            fixed4 frag() : SV_Target
            {
                return 1;
            }
            ENDCG
        }

        // Pass 1: outline around the mask, blended over the camera image
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define SAMPLE_COUNT 16

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            fixed4 frag(v2f_img i) : SV_Target
            {
                // Inside the silhouette, keep the object visible
                if (tex2D(_MainTex, i.uv).r > 0.5)
                {
                    discard;
                }

                // Two rings of samples, so thin parts of the object are not missed
                float outline = 0;

                for (int s = 0; s < SAMPLE_COUNT; s++)
                {
                    float angle = s * UNITY_TWO_PI / SAMPLE_COUNT;
                    float2 direction = float2(cos(angle), sin(angle)) * _OutlineWidth * _MainTex_TexelSize.xy;

                    outline = max(outline, tex2D(_MainTex, i.uv + direction).r);
                    outline = max(outline, tex2D(_MainTex, i.uv + direction * 0.5).r);
                }

                return fixed4(_OutlineColor.rgb, _OutlineColor.a * saturate(outline));
            }
            ENDCG
        }
    }
}
