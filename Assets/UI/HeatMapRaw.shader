Shader "UI/HeatMapRaw"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color0("Color 0", Color) = (0,0,0,1)
        _Color1("Color 1", Color) = (0,.9,.2,1)
        _Color2("Color 2", Color) = (.9,1,.3,1)
        _Color3("Color 3", Color) = (.9,.7,.1,1)
        _Color4("Color 4", Color) = (1,0,0,1)

        _Range0("Range 0", Range(0,1)) = 0.0
        _Range1("Range 1", Range(0,1)) = 0.25
        _Range2("Range 2", Range(0,1)) = 0.5
        _Range3("Range 3", Range(0,1)) = 0.75
        _Range4("Range 4", Range(0,1)) = 1.0

        _Diameter("Diameter", Range(0,1)) = 0.5 // Adjusted
        _Strength("Strength", Range(0.1,4)) = -2.0 // Adjusted
        _PulseSpeed("Pulse Speed", Range(0,5)) = 0.0
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" } // For UI elements
        LOD 100

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

            float4 _Color0, _Color1, _Color2, _Color3, _Color4;
            float _Range0, _Range1, _Range2, _Range3, _Range4;
            float _Diameter, _Strength, _PulseSpeed;

            // Declare global arrays for colors and point ranges
            float3 colors[5];
            float pointranges[5];

            // Passed in array for hit points: 3 floats per point (x, y, intensity)
            float _Hits[3 * 32];
            int _HitCount = 0;

            // Initialize heatmap colors and point ranges (only once)
            void initialize()
            {
                colors[0] = _Color0.rgb;
                colors[1] = _Color1.rgb;
                colors[2] = _Color2.rgb;
                colors[3] = _Color3.rgb;
                colors[4] = _Color4.rgb;

                pointranges[0] = _Range0;
                pointranges[1] = _Range1;
                pointranges[2] = _Range2;
                pointranges[3] = _Range3;
                pointranges[4] = _Range4;
            }

            // Get heatmap color for a given weight
            float3 getHeatForPixel(float weight)
            {
                if (weight <= pointranges[0]) return colors[0];
                if (weight >= pointranges[4]) return colors[4];

                for (int i = 1; i < 5; i++)
                {
                    if (weight < pointranges[i])
                    {
                        float dist_from_lower_point = weight - pointranges[i - 1];
                        float size_of_point_range = pointranges[i] - pointranges[i - 1];
                        float ratio_over_lower_point = dist_from_lower_point / size_of_point_range;

                        float3 color_range = colors[i] - colors[i - 1];
                        return colors[i - 1] + color_range * ratio_over_lower_point;
                    }
                }
                return colors[0];  // Fallback
            }

            // Calculate distance-based contribution for heatmap
            float distsq(float2 a, float2 b)
            {
                float effect_size = _Diameter;
                return pow(max(0.0, 1.0 - distance(a, b) / effect_size), 2.0);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // UI needs clip space transformation
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // UV transformation for the texture
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Initialize heatmap color ranges and point ranges (only once)
                initialize();

                float2 uv = i.uv * 4.0 - float2(2.0, 2.0);  // Transform UV coordinates to -2 to 2 range
                float totalWeight = 0.0;

                // Loop through hit points and calculate total weight
                for (int j = 0; j < _HitCount; j++)
                {
                    float2 work_pt = float2(_Hits[j * 3], _Hits[j * 3 + 1]);
                    float pt_intensity = _Hits[j * 3 + 2];

                    totalWeight += distsq(uv, work_pt) * pt_intensity * _Strength * (1 + sin(_Time.y * _PulseSpeed));
                }

                // Get heat color based on the computed weight
                float3 heatColor = getHeatForPixel(totalWeight);

                // Directly apply the heatmap color without mixing with the base texture
                return float4(heatColor, 1.0);  // Full heatmap color with no blending with the white texture
            }

            ENDCG
        }
    }
}
