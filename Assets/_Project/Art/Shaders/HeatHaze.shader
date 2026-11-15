Shader "Custom/UI_HeatHaze"
{
    Properties
    {
        [Header(Heat Haze Settings)]
        _DistortionStrength("Wave Strength", Range(0, 0.03)) = 0.004
        _WaveFrequency("Wave Frequency", Float) = 25.0
        _WaveSpeed("Rising Speed", Float) = 1.5
        _WarmTint("Heat Color Tint", Color) = (1.0, 0.92, 0.8, 1.0)
        _Intensity("Global Effect Intensity", Range(0, 1)) = 0.0
    }

        SubShader
        {
            // Отрисовываем поверх всей сцены после непрозрачных и прозрачных объектов
            Tags { "Queue" = "Transparent+100" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

            // Захватываем картинку с камеры, которая находится за этим объектом
            GrabPass { "_BackgroundTexture" }

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

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float4 grabPos : TEXCOORD0;
                    float2 uv : TEXCOORD1;
                };

                sampler2D _BackgroundTexture;
                float _DistortionStrength;
                float _WaveFrequency;
                float _WaveSpeed;
                fixed4 _WarmTint;
                float _Intensity;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.grabPos = ComputeGrabScreenPos(o.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Если интенсивность 0 — просто отдаем чистую картинку без искажений
                    if (_Intensity <= 0.001)
                    {
                        return tex2Dproj(_BackgroundTexture, i.grabPos);
                    }

                // Считаем медленные волны, поднимающиеся снизу вверх (_Time.y * _WaveSpeed)
                float waveX = sin(i.uv.y * _WaveFrequency - _Time.y * _WaveSpeed) * cos(i.uv.x * 10.0 + _Time.y * 0.5);
                float waveY = cos(i.uv.y * (_WaveFrequency * 0.8) - _Time.y * (_WaveSpeed * 1.2));

                // Смягчаем эффект по краям экрана (виньетка искажения), чтобы не рвать границы кадра
                float edgeMask = smoothstep(0.0, 0.15, i.uv.x) * smoothstep(1.0, 0.85, i.uv.x) *
                                 smoothstep(0.0, 0.15, i.uv.y) * smoothstep(1.0, 0.85, i.uv.y);

                float2 offset = float2(waveX, waveY * 0.5) * _DistortionStrength * _Intensity * edgeMask;

                // Применяем смещение к координатам захваченного экрана
                float4 distortedGrabPos = i.grabPos;
                distortedGrabPos.xy += offset * distortedGrabPos.w;

                fixed4 col = tex2Dproj(_BackgroundTexture, distortedGrabPos);

                // Плавно подмешиваем легкий теплый оттенок пустыни
                fixed4 finalColor = lerp(col, col * _WarmTint, _Intensity * 0.6);
                finalColor.a = 1.0;

                return finalColor;
            }
            ENDCG
        }
        }
}
