// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Transparent/Drainage" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_LengthTex("River Length Texture", 2D) = "white" {}
	}

		SubShader{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			LOD 200

		CGPROGRAM
		#pragma surface surf Lambert alpha:fade

		sampler2D _MainTex;
		sampler2D _LengthTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed4 c2 = tex2D(_LengthTex, IN.uv_MainTex);
			c.rgb = lerp(c.rgb, c.rgb / 2.0, frac((_Time.x * 7) - c2.r));
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

		Fallback "Legacy Shaders/Transparent/Diffuse"
}
