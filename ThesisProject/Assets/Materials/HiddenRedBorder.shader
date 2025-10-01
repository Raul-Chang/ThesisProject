Shader "Hidden/RedBorder"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Intensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Distance from screen center (0..1)
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                // Smooth vignette mask
                float vignette = smoothstep(0.3, 0.7, dist);

                // Red overlay
                fixed4 red = fixed4(1, 0, 0, 1);

                // Blend based on intensity * vignette
                col.rgb = lerp(col.rgb, red.rgb, vignette * _Intensity);

                return col;
            }
            ENDCG
        }
    }
}
