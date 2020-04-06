Shader "Unlit/Moon"
{
    Properties
    {
		[HDR] _Color ("Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Geometry+1" }
        LOD 100
		ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				return _Color;
            }
            ENDCG
        }
    }
}
