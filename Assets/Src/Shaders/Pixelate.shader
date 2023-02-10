Shader "Unlit/Pixelate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScreenHeight ("Screen Height" , int) = 145
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Cull Back
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
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            int _ScreenHeight;

            fixed4 frag (v2f i) : SV_Target
            {
                float pixelScreenHeight = _ScreenHeight;
                float pixelScreenWidth = (int)(pixelScreenHeight * (_ScreenParams.x / _ScreenParams.y) * 0.5);
                float2 BlockCount = { pixelScreenWidth, pixelScreenHeight };
                float2 BlockSize = { 1.0 / pixelScreenWidth, 1.0 / pixelScreenHeight };
                float2 HalfBlockSize = BlockSize / 2.0;

                float2 blockPos = floor(i.uv * BlockCount);
                float2 blockCenter = blockPos * BlockSize + HalfBlockSize;

                // sample the texture
                fixed4 col = tex2D(_MainTex, blockCenter);

                return col;
            }
            ENDCG
        }
    }
}
