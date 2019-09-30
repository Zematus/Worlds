Shader "Sprites/Drainage"
{
	Properties
	{
		[PerRendererData] _MainTex("Drainage Texture", 2D) = "white" {}
		_LengthTex("River Length Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex	: SV_POSITION;
				fixed4 color	: COLOR;
				half2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _LengthTex;

			fixed4 frag(v2f IN) : COLOR
			{
				half4 texcol = tex2D(_MainTex, IN.texcoord);
				half4 texcol2 = tex2D(_LengthTex, IN.texcoord);
				texcol.rgb = lerp(texcol.rgb, texcol.rgb / 2.0, frac((_Time.x * 7) - texcol2.x));
				texcol = texcol * IN.color;
				return texcol;
			}
		ENDCG
		}
	}
			
	Fallback "Sprites/Default"
}
