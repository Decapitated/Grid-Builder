Shader "Custom/LitSnow"
{
    Properties
    {
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        _MainTex ("Main Texture", 2D) = "white" {}
        _BlendTex ("Blend texture", int) = 0
        _Color ("Main Color", Color) = (1,1,1,1)
        _Range ("Edge Range", float) = 10
        _Normal ("Snow Normal", Vector) = (0, 1, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        
        sampler2D _MainTex;
        bool _BlendTex;
        fixed4 _Color;
        float _Range;
        float3 _Normal;


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 col = tex2D(_MainTex, IN.uv_MainTex);
            float angle = dot(normalize(IN.worldNormal), normalize(_Normal));
            float angleDegrees = acos(angle) * (180.0 / 3.1415926535897932384626433832795);
            o.Albedo = (col * _Color).rgb;
            if (angleDegrees <= _Range) {
                if(_BlendTex) o.Alpha = col.r;
                else o.Alpha = 1;
            } else {
                o.Alpha = 0;
            }
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
