Shader "Unlit/Sky"
{
    Properties
    {
		_SkyColor("Sky Color", Color) = (0,0,1,1)
		_SunColor("Sun Color", Color) = (1,1,0,1)
		_SunSize("Sun Size", Vector) = (0.8,0.9,0,0)
    }
    SubShader
    {
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off
		ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				half3 rayDir : RAY;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				float3 eyeRay = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
				o.rayDir=half3(-eyeRay);
                return o;
            }

			fixed4 _SkyColor;
			fixed4 _SunColor;
			fixed4 _SunSize;

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				half3 ray = normalize(i.rayDir);
				float dt = -dot(_WorldSpaceLightPos0.xyz,ray);
				fixed4 col = lerp(_SkyColor,_SunColor,smoothstep(_SunSize.x,_SunSize.y,dt));
				//fixed4 col = fixed4(i.rayDir,1);
                return col;
				//return dt;
            }
            ENDCG
        }
    }
}
