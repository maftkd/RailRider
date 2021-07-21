Shader "Unlit/Gradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ColorA ("Color A", Color) = (0,0,0,0)
		_ColorB ("Color B", Color) = (1,1,1,1)
		_Step ("Step", Float) = 0.5
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		ZWrite Off
		ZTest Off
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _ColorA;
			fixed4 _ColorB;
			fixed _Step;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				col*=lerp(_ColorA,_ColorB,smoothstep(_Step,0.92,i.uv.y));
                return col;
            }
            ENDCG
        }
    }
}
