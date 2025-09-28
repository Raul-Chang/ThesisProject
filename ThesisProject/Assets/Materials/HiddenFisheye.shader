Shader "Hidden/Fisheye"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
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
                // remap uv to -1..1 center
                float2 uv = i.uv * 2.0 - 1.0;
                float r2 = dot(uv, uv); // r^2
                // barrel distortion factor (tweak scale inside)
                float k = _Intensity * 0.9;
                float factor = 1.0 + k * r2;
                float2 distorted = uv / factor;

                float2 finalUV = (distorted + 1.0) * 0.5;

                // outside the circle -> black
                if (finalUV.x < 0.0 || finalUV.x > 1.0 || finalUV.y < 0.0 || finalUV.y > 1.0)
                    return fixed4(0,0,0,1);

                return tex2D(_MainTex, finalUV);
            }
            ENDCG
        }
    }
}
