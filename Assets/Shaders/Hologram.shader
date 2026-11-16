// Blue hologram of the structure plans (Resources/Materials/Blueprint): transparent glow, bright edges (fresnel),
// horizontal scan lines moving upwards and a slight flicker
Shader "Custom/Hologram"
{
    Properties
    {
        _Color ("Color", Color) = (0.2, 0.6, 1, 0.15)
        _RimColor ("Edge color", Color) = (0.45, 0.85, 1, 1)
        _RimPower ("Edge sharpness", Range(0.5, 8)) = 2.5
        _LineDensity ("Scan lines per meter", Float) = 4
        _LineSpeed ("Scan lines speed", Float) = 0.5
        _LineStrength ("Scan lines strength", Range(0, 1)) = 0.4
        _Flicker ("Flicker", Range(0, 1)) = 0.08
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

        // Additive: the hologram adds light
        Blend SrcAlpha One
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _RimColor;
            float _RimPower;
            float _LineDensity;
            float _LineSpeed;
            float _LineStrength;
            float _Flicker;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float rim = pow(1 - saturate(abs(dot(normalize(i.worldNormal), viewDir))), _RimPower);

                // In world space: the lines go through the whole hologram at the same height
                float scanLine = frac(i.worldPos.y * _LineDensity - _Time.y * _LineSpeed);
                float scan = 1 - _LineStrength * step(0.5, scanLine);

                float flicker = 1 - _Flicker * (0.5 + 0.5 * sin(_Time.y * 37));

                fixed4 color = _Color;
                color.rgb += _RimColor.rgb * rim;
                color.a = saturate(_Color.a + rim * _RimColor.a) * scan * flicker;

                return color;
            }
            ENDCG
        }
    }

    Fallback Off
}
