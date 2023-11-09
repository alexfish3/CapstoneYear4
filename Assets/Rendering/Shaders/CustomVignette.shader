Shader "CustomPost/CustomVignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog INVERT_ON INVERT_OFF

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _ImageTex;
            float4 _MainTex_ST;
            float4 _MainColor;
            float _Radius;
            float _Feather;
            bool _Invert;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 image = tex2D(_ImageTex, i.uv);
                fixed4 col = tex2D(_MainTex, i.uv);

                float2 newUV = i.uv * 2 - 1;
                float circle = length(newUV);

                float mask = 1 - smoothstep(_Radius, _Radius + _Feather, circle);
                float invertMask = 1 - mask;

                float3 displayColor = col.rgb * mask;

                float3 vignetteColor = col.rgb * invertMask * _MainColor * image.rgb;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return fixed4(displayColor + vignetteColor,1);
            }
            ENDCG
        }
    }
}
