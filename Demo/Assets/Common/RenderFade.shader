Shader "Common/RenderFade"
{
    Properties
    {
        _Tex0 ("Texture", 2D) = "white" {}
        _Tex1("Texture", 2D) = "white" {}
        _Fade("Fade", Range(0.0,1.0)) = 0.0
        _FadeOut("FadeOut", Range(0.0,1.0)) = 0.0
        _FadeTex("FadeTex", 2D) = "white" {}
        _Deform("Deform", Range(0.0,1.0)) = 0.0
        _DeformInvert("DeformInvert", Range(0.0,1.0)) = 0.0
        _DeformTex("DeformTex", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Tex0;
            sampler2D _Tex1;
            sampler2D _FadeTex;
            float _Fade;
            float _FadeOut;
            float _Deform;
            float _DeformInvert;
            sampler2D _DeformTex;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 d = tex2D(_DeformTex, lerp(i.uv, float2(1.0, 1.0)-i.uv, _DeformInvert))*_Deform;
                d.a = 0.0;
                float2 uv = i.uv + d.xy*0.015;

                float4 t0 = tex2D(_Tex0, uv)+ d * 0.05;
                float4 t1 = tex2D(_Tex1, uv)+ d * 0.05;
                
                return lerp(lerp(t0, t1, tex2D(_FadeTex, i.uv).x*0.9>=1.0-_Fade), 0.0, _FadeOut);
            }
            ENDCG
        }
    }
}
