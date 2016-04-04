Shader "Doloro Ent/Sand - Linear" { 
	Properties { 
		_Tex0("Normal", 2D) = "white" {}
		_Color0("Main tint", Color) = (0.73,0.8125,0.359,1)
		_Color1("Emission tint", Color) = (0.324,1,0,1)
		[HideInInspector]_Variable0("Scale", float) = 100
		_BRDFTex ("BRDF Texture", 2D) ="white" {}
		_SpecularColor ("Specular Color", Color ) = (0.043,0.043,0.043,1)
		_SpecPower ("Specular Power", Range (0.01, 10)) = 8
		_Brightnes ("Albedo brightnes", Range (0, 1)) = 0.58
		_LightBrightnes ("Light brightnes", Range (0, 4)) = 2.5
		_Variable1("Emission", Range (0, 0.5)) = 0.24
	}
	SubShader {
		Tags {
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}
		SeparateSpecular   On

		CGPROGRAM 
		#pragma surface surf CarPaint// vertex:vert
		#pragma target 3.0
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"

		sampler2D _Tex0;
		float4 _Color0;
		float4 _Color1;
		float _Variable0;
		float _Variable1;
		float _TessMultiplier;
		float _Displacement;
		sampler2D _DispMap;
		uniform float4 _DispMap_ST;
		fixed _SpecPower;
		fixed _DiffusePower;
		sampler2D _BRDFTex;
		fixed4 _SpecularColor;
		float _Brightnes;
		float _LightBrightnes;

		struct appdata{
			float4 vertex    : POSITION;  // The vertex position in model space.
			float3 normal    : NORMAL;    // The vertex normal in model space.
			float4 texcoord  : TEXCOORD0; // The first UV coordinate.
			float4 texcoord1 : TEXCOORD1; // The second UV coordinate.
			float4 texcoord2 : TEXCOORD2; // The third UV coordinate.
			float4 tangent   : TANGENT;   // The tangent vector in Model Space (used for normal mapping).
			float4 color     : COLOR;     // Per-vertex color.
		};

		struct Input{
			float2 uv_Tex0;
			float2 uv_Tex1;
			float3 viewDir;
			float3 worldPos;
			float3 worldRefl;
			float3 worldNormal;
			float4 screenPos;
			float4 color : COLOR;

			INTERNAL_DATA
		};
		
		inline float4 LightingCarPaint (SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			half3 h = normalize( lightDir + viewDir );
			fixed diff = max( 0.5, dot(s.Normal, lightDir) );

			float ahdn = pow( clamp(1.0 - dot ( h, normalize(s.Normal) ), 0.0, 1.0), _DiffusePower );
			half4 brdf = tex2D(_BRDFTex, float2(diff, 1 - ahdn));
			
			float nh = max( 0, dot(s.Normal, h) );
			float spec = pow(nh, s.Specular * _SpecPower) * s.Gloss;

			float4 col;
			col.rgb = ( s.Albedo * _LightColor0.rgb * brdf.rgb + _LightColor0.rgb * _SpecularColor.rgb * spec ) *  atten * _LightBrightnes * 5;
			col.a = s.Alpha + _LightColor0.a * _SpecularColor.a * spec * atten;
							
			float checkVar3 = frac(sin( dot(s.Normal ,lightDir )) * 438.5453);
			if(checkVar3 > 0.99)
				col += float4(lerp(float4(s.Albedo, s.Alpha), _Color1, float4(s.Normal,1.0f))) / 5;
			else if(checkVar3 > 0.7)
				col += float4(lerp(float4(s.Albedo, s.Alpha), _Color1, float4(s.Normal,1.0f))) / 10;
			else if(checkVar3 > 0.5)
				col += float4(lerp(float4(s.Albedo, s.Alpha), _Color1, float4(s.Normal,1.0f))) / 12;
			else if(checkVar3 > 0.3)
				col += float4(lerp(float4(s.Albedo, s.Alpha), _Color1, float4(s.Normal,1.0f))) / 15;
			else if(checkVar3 > 0.1)
				col += float4(lerp(float4(s.Albedo, s.Alpha), _Color1, float4(s.Normal,1.0f))) / 20;

							
			return col;
		}
		
		//void vert (inout appdata v){
		//	float disp = tex2Dlod(_DispMap, float4(v.texcoord.xy,0,0) * _DispMap_ST * _Variable0 + _Time/_WavesSpeed).r * _Displacement;
		//	v.vertex.xyz += v.normal * disp;
		//}

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = _Color0.rgb + lerp(0, 0.6f, normalize(IN.screenPos) * _Brightnes);
			//o.Albedo *= 3;
			o.Normal = UnpackNormal(tex2D(_Tex0, IN.uv_Tex0 * _Variable0));
			
			o.Emission = _Variable1;
			o.Specular = 1;
			o.Alpha = _Color0.a;
			o.Gloss = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
