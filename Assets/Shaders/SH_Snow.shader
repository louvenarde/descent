Shader "Descent/Snow Terrain"
{
	Properties
	{
		_SnowTex ("Snow Texture", 2D) = "white" {}
		_RockTex ("Rock Texture", 2D) = "white" {}
		
		_FresnelColor ("Fresnel Color", Color) = (1,0,0,1)
		_FresnelBias ("Fresnel Bias", Float) = 0.0
		_FresnelScale ("Fresnel Scale", Float) = 1.0
		_FresnelPower ("Fresnel Power", Float) = 2.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags {"LightMode" = "ForwardBase"} // todo, impl ForwardAdd

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

			struct VertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float4 wPos : TEXCOORD1; // xyz: pos	w: snowDepth
				half4 wNorm : TEXCOORD2; // xyz: norm  w: snowAmount

				UNITY_FOG_COORDS(3)
				SHADOW_COORDS(4)
                fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
			};

			float4x4 _SnowCameraMatrix;
			sampler2D _SnowTex;
			float4 _SnowTex_ST;
			sampler2D _RockTex;
			float4 _RockTex_ST;

			sampler2D _SnowRT;

			float4 _FresnelColor;
			float _FresnelBias;
			float _FresnelScale;
			float _FresnelPower;
			
			VertexOutput vert (VertexInput v)
			{
				VertexOutput o;
				o.wPos = mul(unity_ObjectToWorld, v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv, _SnowTex);
				o.wNorm.xyz = UnityObjectToWorldNormal(v.normal);
				o.wNorm.w = saturate(abs(o.wNorm.y) * 10 - 7);

				o.uv.zw = mul(_SnowCameraMatrix, o.wPos).xy / 10.0f + 0.5f;

                half depthSnow = tex2Dlod(_SnowRT, float4(o.uv.zw, 0.0, 0.0)).r;
				
				half distToCenter = distance(o.uv.zw, float2(0.5, 0.5)) * 2;
				distToCenter = smoothstep(0.8, 0.95, distToCenter);
				
				//half band = saturate(smoothstep(0, 0.2, depthSnow)-smoothstep(0.2, 0.4, depthSnow));
				//colSnow = lerp(colSnow, float4(1,1,1,1), band);
				depthSnow *= saturate(1.0 - distToCenter) * o.wNorm.w;
				//depthSnow -= band * 0.7f;

				o.wPos.y -= depthSnow * 0.5f;
				o.pos = mul(UNITY_MATRIX_VP, o.wPos);
				o.wPos.w = depthSnow;

				// vertex lighting
                half NdotL = saturate(dot(o.wNorm, _WorldSpaceLightPos0.xyz));
                o.diff = NdotL * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(o.wNorm.xyz, 1.0f));

                TRANSFER_SHADOW(o);
				UNITY_TRANSFER_FOG(o, o.pos);

				return o;
			}

			fixed4 frag (VertexOutput i) : SV_Target
			{
                fixed4 colSnow = tex2D(_SnowTex, i.uv);
                fixed4 colRock = colSnow * fixed4(0.1, 0.1, 0.1, 1.0); // we will probably want some real rock texture but I'm scared of too many texture lookup on the 360
				
				fixed depthSnowVertex = i.wPos.w;
                fixed depthSnowPixel = tex2Dlod(_SnowRT, float4(i.uv.zw, 0.0, 0.0)).r; // pixel accurate
				
				half noise = saturate(colSnow.r * 20 - 16); // cutout snow texture
				half snowAmount = saturate(i.wNorm.w + noise);

				half distToCenter = distance(i.uv.zw, float2(0.5, 0.5)) * 2;
				distToCenter = smoothstep(0.8, 0.95, distToCenter);

				depthSnowPixel *= 1.0 - distToCenter;
				depthSnowPixel *= snowAmount;
				colSnow = lerp(colSnow, unity_AmbientSky, depthSnowPixel * 0.4f);

				fixed4 col = lerp(colRock, colSnow, snowAmount);

				// add a flat fresnel to highlight the edge of the snow
				half3 I = normalize(i.wPos.xyz - _WorldSpaceCameraPos.xyz);
				half fresnel = _FresnelBias + _FresnelScale * pow(1.0 + dot(I, normalize(i.wNorm)), _FresnelPower);
				col.rgb = lerp(col.rgb, _FresnelColor.rgb, saturate(fresnel) * _FresnelColor.a);

                fixed shadow = SHADOW_ATTENUATION(i) * (1.0 - depthSnowVertex);
				// compute the edge of the vertex displacement
				fixed band = saturate(smoothstep(0, 0.2, depthSnowVertex) - smoothstep(0.2, 0.4, depthSnowVertex));
                // darken light's illumination with shadow, lighten the band around the displacement, keep ambient intact
                fixed3 lighting = i.diff * (shadow + band * 0.4f) + i.ambient;
                col.rgb *= lighting;

				UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
			}
			ENDCG
		}
	}
	Fallback "Mobile/Diffuse"
}
