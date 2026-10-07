Shader "Custom/HeartSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1, 0.6, 0.8, 1)
        _BottomColor ("Bottom Color", Color) = (1, 0.85, 0.95, 1)
        _HeartColor ("Heart Color", Color) = (1, 0.4, 0.6, 1)
        _Tiling ("Tiling (rows)", Float) = 12
        _HeartSize ("Heart Size", Float) = 0.25
        _DiagonalOffset ("Diagonal Offset", Float) = 0.5
        _ScrollSpeed ("Scroll Speed", Float) = 0.15
        _AnimTime ("Animation Time (auto)", Float) = 0
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 texcoord : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _HeartColor;
            float _Tiling;
            float _HeartSize;
            float _DiagonalOffset;
            float _ScrollSpeed;
            float _AnimTime;
            float _EdgeSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.texcoord;
                return o;
            }

            float heartShape(float2 p, float softness)
            {
                p.y += 0.25;
                p *= 1.8;
                float a = atan2(p.x, p.y) / 3.14159;
                float r = length(p);
                float h = abs(a);
                float d = (13.0 * h - 22.0 * h * h + 10.0 * h * h * h) / (6.0 - 5.0 * h);
                // Узкая зона сглаживания — почти не смешивается с фоном
                return smoothstep(r, r - softness, d);
            }

fixed4 frag (v2f i) : SV_Target
{
    float3 dir = normalize(i.dir);

    // Фон (градиент)
    float t = saturate(dir.y * 0.5 + 0.5);
    fixed4 bg = lerp(_BottomColor, _TopColor, t);

    // --- Кубическая проекция (6 сторон куба) ---
    float3 aDir = abs(dir);
    float2 uv;

    if (aDir.x >= aDir.y && aDir.x >= aDir.z)
        uv = dir.yz / aDir.x;       // +X / -X
    else if (aDir.y >= aDir.x && aDir.y >= aDir.z)
        uv = dir.xz / aDir.y;       // +Y / -Y
    else
        uv = dir.xy / aDir.z;       // +Z / -Z

    uv *= _Tiling * 0.5;
    uv += float2(_AnimTime * _ScrollSpeed, _AnimTime * _ScrollSpeed);

    // Диагональные ряды
    float rowIndex = floor(uv.y);
    uv.x += rowIndex * _DiagonalOffset;

    float2 local = frac(uv) - 0.5;

    float heart = heartShape(local / _HeartSize, _EdgeSoftness);

    fixed3 finalColor = lerp(bg.rgb, _HeartColor.rgb, heart);
    return fixed4(finalColor, 1);
}
            ENDCG
        }
    }
    Fallback Off
}