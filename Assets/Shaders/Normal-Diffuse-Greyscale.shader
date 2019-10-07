// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Diffuse-Greyscale" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_EffectAmount("Effect Amount", Range(0, 1)) = 1.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		fixed4 _Color;

		uniform float _EffectAmount;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			c.rgb = lerp(c.rgb, dot(c.rgb, float3(0.3, 0.59, 0.11)), _EffectAmount);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
		}

			Fallback "Legacy Shaders/VertexLit"
}
