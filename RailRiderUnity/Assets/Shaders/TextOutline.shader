Shader "Unlit/TextOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MainColor ("Main color", Color) = (1,1,1,1)
		_Threshold ("Edge threshold", Float) = 0.5
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _MainColor;
			fixed _Threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				//sample some neighboring pixels
				fixed2 texel=fixed2(_ScreenParams.z-1,_ScreenParams.w-1);
				//fixed2 texel = fixed2(0.01,0.01);

				//kernal samples
				//left
				fixed aa = tex2D(_MainTex, i.uv+fixed2(-texel.x,texel.y)).a;
				fixed ba = tex2D(_MainTex, i.uv+fixed2(-texel.x,0)).a;
				fixed ca = tex2D(_MainTex, i.uv+fixed2(-texel.x,-texel.y)).a;

				//top/bottom
				fixed ab = tex2D(_MainTex, i.uv+fixed2(0,texel.y)).a;
				fixed cb = tex2D(_MainTex, i.uv+fixed2(0,-texel.y)).a;

				//right
				fixed ac = tex2D(_MainTex, i.uv+fixed2(texel.x,texel.y)).a;
				fixed bc = tex2D(_MainTex, i.uv+fixed2(texel.x,0)).a;
				fixed cc = tex2D(_MainTex, i.uv+fixed2(texel.x,-texel.y)).a;

				fixed gx = abs(-aa-ba*4-ca+ac+bc*4+cc);
				fixed gy = abs(-ca-cb*4-cc+aa+ab*4+ac);
				fixed g = gx*gx+gy*gy;
				col.rgb = lerp(_MainColor.rgb,fixed3(0,0,0),g*_Threshold);
                return col;
            }
            ENDCG
        }
    }
}
