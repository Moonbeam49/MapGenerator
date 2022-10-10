Shader "Custom/TerrainShader"
{
	Properties
	{

	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		int baseColorsLength;
	float3 baseColors[8];
	float baseHeights[8];
	float baseBlends[8];

		float minHeight;
		float maxHeight;

        struct Input
        {
			float3 worldPos;
        };

		float inverseLerp(float a, float b, float value) {
			return saturate((value - a) / (b - a));
		}

        void surf (Input IN, inout SurfaceOutput o)
        {
			float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
			for (int i = 0; i < baseColorsLength; i++) {
				float drawStrength = inverseLerp(-baseBlends[i] / 2, baseBlends[i] / 2, heightPercent - baseHeights[i]);
				o.Albedo = o.Albedo * (1 - drawStrength) + baseColors[i] * drawStrength;
			}
        }
        ENDCG
    }
    FallBack "Diffuse"
}
