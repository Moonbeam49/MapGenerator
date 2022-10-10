Shader "Unlit/WaterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MainPattern("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0,0,1,0.5)
			_DepthShallowColor("DepthShallow", Color) = (0.325,0.807,0.971,0.725)
			_DepthDeepColor("DepthDeep", Color) = (0.086,0.407,1,0.749)
		_DepthMaxDistance("Depth Maximum Distance", Float) = 1
			_FoamDistance("Foam Distance", Float) = 0.4
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent"
			"Queue" = "Transparent"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert

            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"

            struct MeshData
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
				float3 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD1;
				float4 screenPosition : TEXCOORD2;
				float3 worldPosition : TEXCOORD3;
            };

            sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _Color;
			float4 _DepthShallowColor;
			float4 _DepthDeepColor;
			float _DepthMaxDistance;
			float _FoamDistance;

			sampler2D _CameraDepthTexture;

			Texture2D _MainPattern;

            Interpolators vert (MeshData v)
            {
				float3 curWorldPos = mul (unity_ObjectToWorld, v.vertex)/25;
				curWorldPos.z = curWorldPos.z + _Time.x*0.15;
				float4 texCol = tex2Dlod(_MainTex,float4(curWorldPos.x,curWorldPos.z,0,0));
				v.vertex.y = v.vertex.y + (1-texCol.b*2);
				float3 posPlusTangent = v.vertex + v.tangent * 0.01;
				posPlusTangent.y = posPlusTangent.y - texCol.b;

				float3 bitangent = cross(v.normal, v.tangent);

				float3 posPlusBitangent = v.vertex + bitangent * 0.01;
				posPlusBitangent.y = posPlusBitangent.y - texCol.b;

				float3 modifiedTangent = posPlusTangent - v.vertex;
				float3 modifiedBitangent = posPlusBitangent - v.vertex;

				float3 modifiedNormal = cross(modifiedTangent, modifiedBitangent);
				v.normal = normalize(modifiedNormal);
                Interpolators o;

				o.worldPosition = curWorldPos;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.screenPosition = ComputeScreenPos(o.vertex);

                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
				float3 Normal = i.normal;
				float3 LightDir = _WorldSpaceLightPos0.xyz;
				float3 diffuseLight = dot(Normal, LightDir) * _LightColor0.xyz;

				float existingDepth01 = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPosition)).r;
				float existingDepthLinear = LinearEyeDepth(existingDepth01);
				float depthDifference = existingDepthLinear - i.screenPosition.w;
				float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
				float4 waterColor = lerp(_DepthShallowColor, _DepthDeepColor, waterDepthDifference01);
				float foamDepthDifference01 = saturate(depthDifference / _FoamDistance);
				float colorCutoff = foamDepthDifference01 < _FoamDistance ? 1 : 0;

				float4 texColor = tex2D(_MainTex, i.worldPosition.xz);

				return waterColor*texColor + colorCutoff;
                //return float4(diffuseLight*_Color.xyz,_Color.a);
            }
            ENDCG
        }
    }
}
