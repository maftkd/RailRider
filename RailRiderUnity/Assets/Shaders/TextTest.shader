Shader "Unlit/TextTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ColA ("Col top", Color) = (1,1,1,1)
		_ColB ("Col bot", Color) = (1,1,1,1)
		_StepMin ("Step Min", Float) = 0
		_StepMax ("Step Max", Float) = 1

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				fixed height : HEIGHT;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _ColA;
			fixed4 _ColB;
			fixed _StepMin;
			fixed _StepMax;

            v2f vert (appdata v)
            {
                v2f o;
				o.height = v.vertex.y;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
				fixed3 c = lerp(_ColB.rgb,_ColA.rgb,smoothstep(_StepMin,_StepMax,i.height));
				col = fixed4(c,col.a);
                return col;
            }
            ENDCG
        }
    }
}
