Shader "Unlit/NormalEdge"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _Range ("Edge Range", float) = 10
        _Fade ("Should Fade", int) = 0
        [Range(0f, 1f)]
        _InvertFade ("Invert Fade", int) = 0
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
                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = _WorldSpaceCameraPos - o.worldPos;
                return o;
            }

            fixed4 _Color;
            float _Range;
            bool _Fade;
            bool _InvertFade;

            fixed4 frag (v2f i) : SV_Target
            {
                const float Rad2Deg = 180 / UNITY_PI;

                float t = unity_CameraProjection._m11;
                float fov = atan(1.0f / t) * 2.0 * Rad2Deg;
                float magnitude = sqrt(i.viewDir.x*i.viewDir.x + i.viewDir.y*i.viewDir.y + i.viewDir.z*i.viewDir.z);

                //_Range = _Range * magnitude * fov;
                _Range = _Range * (magnitude * (2.0 / magnitude));

                fixed4 col = tex2D(_MainTex, i.uv);
                float angle = dot(normalize(i.worldNormal), normalize(i.viewDir));
                float angleDegrees = acos(angle) * Rad2Deg;
                if ((angleDegrees >= (90.0 - _Range) && angleDegrees <= 90)) {
                    float dist = (90.0 - angleDegrees) / _Range;
                    col.a = col.r;
                    if(_Fade)
                    {
                        if(_InvertFade) col.a *= dist;
                        else col.a *= abs(dist - 1);
                    }
                    // The angle is between 80 and 90 degrees, so show the fragment.
                    return col * _Color;
                } else {
                    col.a = 0;
                    // The angle is not between 80 and 90 degrees, so hide the fragment.
                    return col;
                }
            }
            ENDCG
        }
    }
}
