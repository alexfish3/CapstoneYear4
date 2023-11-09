Shader "Hidden/CompositeShader" 
{
    Properties{
            _MainTex1("Texture 1", 2D) = "white" {}
            _MainTex2("Texture 2", 2D) = "white" {}
    }

        SubShader{
            Tags {"RenderPipeline" = "UniversalPipeline"}

            Pass 
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    float2 uv1 : TEXCOORD0;
                };

                struct v2f {
                    float2 uv1 : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex1;
                sampler2D _MainTex2;

                v2f vert(appdata_t v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv1 = v.uv1;
                    return o;
                }

                half4 frag(v2f i) : SV_Target {
                    half4 color1 = tex2D(_MainTex1, i.uv1);
                    half4 color2 = tex2D(_MainTex2, i.uv1);

                    half4 finalColor = lerp(color1, color2, color2.a);

                    return finalColor;
                }
                ENDCG
            }

    }
}
