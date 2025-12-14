Shader "Custom/RingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _InnerRadius ("Inner Radius", Range(0, 10)) = 0.5
        _OuterRadius ("Outer Radius", Range(0, 10)) = 1.0
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.0
        _FadeStart ("Fade Start", Range(0, 1)) = 0.8
        _FadeEnd ("Fade End", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 localPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _InnerRadius;
            float _OuterRadius;
            float _GlowIntensity;
            float _FadeStart;
            float _FadeEnd;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate distance from center in XZ plane (assuming cylinder is oriented along Y axis)
                float2 centerPos = float2(0, 0);
                float2 localPos2D = float2(i.localPos.x, i.localPos.z);
                float distanceFromCenter = length(localPos2D - centerPos);
                
                // Check if pixel is within the ring
                if (distanceFromCenter < _InnerRadius || distanceFromCenter > _OuterRadius)
                {
                    discard; // Don't render pixels outside the ring
                }
                
                // Calculate ring progress (0 = inner edge, 1 = outer edge)
                float ringProgress = (distanceFromCenter - _InnerRadius) / (_OuterRadius - _InnerRadius);
                
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Add glow effect
                float glow = _GlowIntensity * (1.0 - ringProgress);
                col.rgb += glow * _Color.rgb;
                
                // Fade out towards edges
                float fade = 1.0;
                if (ringProgress > _FadeStart)
                {
                    fade = 1.0 - (ringProgress - _FadeStart) / (_FadeEnd - _FadeStart);
                    fade = saturate(fade);
                }
                
                col.a *= fade;
                
                return col;
            }
            ENDCG
        }
    }
} 