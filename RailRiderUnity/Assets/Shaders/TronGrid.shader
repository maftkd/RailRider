Shader "Custom/TronGrid"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_GridScale ("Grid Scale", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderQueue"="Geometry"}
        LOD 200
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
	half4 _GridScale;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			fixed3 dist = _WorldSpaceCameraPos-IN.worldPos;
			float dstSqr=dot(dist,dist);
			//#temp
			clip(dstSqr-_GridScale.y);
            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			float green = step(_GridScale.w,frac(IN.worldPos.x*_GridScale.x));
			green += step(_GridScale.w,frac(IN.worldPos.z*_GridScale.z))*(1-green);
			//green = lerp(0,green,sin(_Time.y));
			o.Emission = (1-green)*lerp(fixed4(0,0,0,0),_Color,abs(sin(_Time.y)));
			//o.Albedo = 1-_Color.rgb;
            o.Albedo = _Color*0.5;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;//c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
