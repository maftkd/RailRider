Shader "Custom/Rail"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_EdgeColor("Edge Color", Color) = (1,1,1,1)
		_EdgeThickness("Edge thickness", Range(0,0.5)) = 0.4
		[HDR] _ColorA("Color A", Color) = (1,1,1,1)
		[HDR] _ColorB("Color B", Color) = (1,1,1,1)

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		half3 _ColorA;
		half3 _ColorB;
		half3 _EdgeColor;
		half _EdgeThickness;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed c = tex2D (_MainTex, IN.uv_MainTex).r;
			c = 1-c*.5;
			float edge = 1-step(_EdgeThickness,abs(.5-IN.uv_MainTex.y));
            //o.Albedo = lerp(_ColorA,_ColorB,IN.uv_MainTex.y)*edge*c+(1-edge)*_EdgeColor;
            o.Albedo = c*lerp(-0.25,0.25,IN.uv_MainTex.y);
			//o.Albedo=lerp(-1*edge;
            o.Metallic = _Metallic;
			//o.Emission
			o.Emission = (1-edge)*lerp(fixed4(0,0,0,0),_EdgeColor,abs(sin(_Time.y)));
			//o.Emission = (1-edge)*lerp(_EdgeColor,fixed3(0,0,0),abs(.5-frac(_Time.y*.5)));
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
