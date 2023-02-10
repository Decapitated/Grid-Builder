Shader "Unlit/NormalEdge"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _BlendTex ("Blend texture", int) = 0
        _Color ("Main Color", Color) = (1,1,1,1)
        _Range ("Edge Range", float) = 10
        _Normal ("Snow Normal", Vector) = (0, 1, 0)
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 snowDir : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _Normal;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.snowDir = _Normal;
                return o;
            }

            fixed4 _Color;
            float _Range;
            bool _BlendTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float angle = dot(normalize(i.worldNormal), normalize(i.snowDir));
                float angleDegrees = acos(angle) * (180.0 / 3.1415926535897932384626433832795);
                if (angleDegrees <= _Range) {
                    if(_BlendTex) col.a = col.r;
                    return col * _Color;
                } else {
                    col.a = 0;
                    return col;
                }
            }
            ENDCG
        }
    }
}
