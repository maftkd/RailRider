Shader "Unlit/Highlight"
{
    Properties
    {
		_Power("Rim power", Float) = 1
		_Color("Rim color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
		Cull Front
		Blend SrcAlpha OneMinusSrcAlpha

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
				half3 normal : NORMAL;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				half3 normal : NORMAL;
				half3 viewDir : VIEW;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = normalize(UnityObjectToWorldNormal(v.normal));
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			fixed _Power;
			fixed4 _Color;

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				fixed4 col = _Color;
				fixed rim = saturate(-dot(i.viewDir,i.normal));
				rim = pow(rim,_Power);
				col.a=rim;
				//col.rgb=rim;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
