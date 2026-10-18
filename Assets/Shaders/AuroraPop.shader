// Full-screen black overlay for a UI Image. Colored aurora curtains bloom where a
// button is pressed and then fade back to black. Pops are driven by AuroraPopOverlay.cs.
Shader "UI/AuroraPop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BaseColor ("Background", Color) = (0,0,0,1)
        _Brightness ("Brightness", Range(0, 4)) = 1.6
        _Spread ("Horizontal Spread", Range(0.05, 1)) = 0.35
        _Speed ("Flow Speed", Range(0, 3)) = 0.6

        // Standard UI properties so Mask / RectMask2D keep working
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #define MAX_POPS 8

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            fixed4 _BaseColor;
            float _Brightness;
            float _Spread;
            float _Speed;

            // Set from script
            float _AuroraTime;              // unscaled time, keeps flowing while paused
            float _Aspect;                  // screen width / height
            float4 _PopData[MAX_POPS];      // xy = center (0-1), z = strength (0-1), w = random seed
            float4 _PopColors[MAX_POPS];

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float amp = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    v += amp * noise(p);
                    p *= 2.03;
                    amp *= 0.5;
                }
                return v;
            }

            // One aurora curtain around a pop center
            float curtain(float2 uv, float2 center, float seed, float t)
            {
                float x = (uv.x - center.x) * _Aspect;

                // Wavy ribbon line the light hangs from
                float wave = (fbm(float2(x * 1.5 + seed * 10.0, t * 0.3)) - 0.5) * 0.35;
                float ribbonY = center.y + wave;

                // Light falls downward from the ribbon and fades upward quickly
                float dy = uv.y - ribbonY;
                float body = dy < 0.0 ? exp(dy * 4.0) : exp(-dy * 18.0);

                // Vertical rays flickering along the curtain
                float rays = fbm(float2(x * 9.0 + seed * 3.0, t * 0.8));
                rays = pow(saturate(rays * 1.4), 2.0);

                // Fade out to the sides of the pop
                float side = exp(-(x * x) / (_Spread * _Spread));

                return body * (0.35 + rays) * side;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = _AuroraTime * _Speed;
                float3 col = _BaseColor.rgb;

                for (int k = 0; k < MAX_POPS; k++)
                {
                    float strength = _PopData[k].z;
                    if (strength <= 0.001) continue;

                    float a = curtain(i.uv, _PopData[k].xy, _PopData[k].w, t);

                    // Shift hue slightly toward the top for the classic aurora gradient
                    float3 c = _PopColors[k].rgb;
                    float3 topTint = c.gbr * 0.6 + c * 0.4;
                    float topMix = saturate((i.uv.y - _PopData[k].y) * 3.0 + 0.5);
                    float3 tint = lerp(c, topTint, topMix * 0.5);

                    col += tint * a * strength * _Brightness;
                }

                return fixed4(col, _BaseColor.a * i.color.a);
            }
            ENDCG
        }
    }
}
