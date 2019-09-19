Shader "Sprites/TestAnimated"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TintTex("Tint Texture", 2D) = "white" {}
		_ScrollSpeeds("Scroll Speeds", vector) = (-5.0, -20.0, 0, 0)
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

			// Declare our second texture sampler and its Scale/Translate values
			sampler2D _TintTex;
			float4 _TintTex_ST;

			float4 _ScrollSpeeds;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				// Shift the UVs so (0, 0) is in the middle of the quad.
				o.uv = v.uv - 0.5f;

				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 polar = float2(
					   atan2(i.uv.y, i.uv.x) / (2.0f * 3.141592653589f), // angle
					   length(i.uv)                                    // radius
					);

				// Copy the polar coordinates before we scale & shift them,
				// so we can scale & shift the tint texture independently.
				float2 tintUVs = polar * _TintTex_ST.xy;
				tintUVs += _ScrollSpeeds.zw * _Time.x;

				polar *= _MainTex_ST.xy;
				polar += _ScrollSpeeds.xy * _Time.x;

				fixed4 col = tex2D(_MainTex, polar);
				// Tint the colour by our second texture.
				col *= tex2D(_TintTex, tintUVs);

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
            ENDCG
        }
    }
}
