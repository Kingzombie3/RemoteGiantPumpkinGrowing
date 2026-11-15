Shader "Custom/UI_VHSEffect"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        [Header(VHS Parameters)]
        _ScanlineIntensity("Scanline Intensity", Range(0, 1)) = 0.15
        _ScanlineCount("Scanline Count", Float) = 240.0
        _ChromaticAberration("Chromatic Aberration", Range(0, 0.02)) = 0.002
        _TrackingSpeed("Tracking Wave Speed", Float) = 2.0
        _TrackingAmount("Tracking Wave Amount", Range(0, 0.02)) = 0.0015
        _NoiseIntensity("Static Noise", Range(0, 0.3)) = 0.04
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                sampler2D _MainTex;
                fixed4 _Color;
                float _ScanlineIntensity;
                float _ScanlineCount;
                float _ChromaticAberration;
                float _TrackingSpeed;
                float _TrackingAmount;
                float _NoiseIntensity;

                // Простой генератор псевдослучайного шума
                float rand(float2 co)
                {
                    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
                }

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = v.texcoord;
                    o.color = v.color * _Color;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 uv = i.texcoord;

                    // 1. Легкое VHS-дрожание горизонтальных строк
                    float wave = sin(uv.y * 30.0 + _Time.y * _TrackingSpeed) * cos(uv.y * 12.0 - _Time.y * 1.5);
                    uv.x += wave * _TrackingAmount;

                    // 2. Хроматическая аберрация (сдвиг R и B каналов)
                    float r = tex2D(_MainTex, uv + float2(_ChromaticAberration, 0)).r;
                    float g = tex2D(_MainTex, uv).g;
                    float b = tex2D(_MainTex, uv - float2(_ChromaticAberration, 0)).b;
                    float a = tex2D(_MainTex, uv).a;

                    fixed4 col = fixed4(r, g, b, a) * i.color;

                    // 3. Строчная развертка (Scanlines)
                    float scanline = sin(uv.y * _ScanlineCount * 3.14159) * 0.5 + 0.5;
                    col.rgb -= scanline * _ScanlineIntensity * col.rgb;

                    // 4. Легкий зернистый шум пленки
                    float noise = (rand(uv + frac(_Time.y)) - 0.5) * _NoiseIntensity;
                    col.rgb += noise;

                    return col;
                }
                ENDCG
            }
        }
}
