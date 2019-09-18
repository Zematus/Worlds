Shader "Sprites/TestAnimated"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_ScrollSpeeds("Scroll Speeds", vector) = (-5, -20, 0, 0)
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			// Declare our new parameter here so it's visible to the CG shader
			float4 _ScrollSpeeds;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				// Shift the uvs over time.
				o.uv += _ScrollSpeeds * _Time.x;

				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                col.rgb = 1 - col.rgb;
                return col;
            }
            ENDCG
        }
    }
}
