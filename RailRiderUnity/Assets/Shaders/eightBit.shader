Shader "PostFX/eightBit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Width("Width", Float) = 0
		_Height("Height", Float) = 0
	_Equator("Equator Vector", Vector) = (0,0,0,0)
	_FogStrength("Fog strength", Float) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

			float _Width;
			float _Height;

		half4 _Equator;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
		sampler2D _CameraDepthTexture;
		half _FogStrength;

			fixed4 frag(v2f i) : SV_Target
			{
				float2 fraction = float2(_Width,_Height);
				float2 samplePoint = float2(round(i.uv.x * _Width) / _Width, round(i.uv.y * _Height) / _Height);
                fixed4 col = tex2D(_MainTex, samplePoint);
		//float rawDepth = tex2D(_CameraDepthTexture,i.uv).r;
		//float depth = Linear01Depth(rawDepth); 
		//depth = depth*step(0,rawDepth);
		//depth = depth*depth;
		//half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0,_Equator);
		//col = lerp(col,skyData,depth*_FogStrength);
                return col;
            }
            ENDCG
        }
    }
}
