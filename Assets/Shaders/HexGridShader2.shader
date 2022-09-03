Shader "Hidden/HexGridShader2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            float4 calcHexInfo(float2 uv)
            {
                const float2 s = float2(1, 1.7320508);
                float4 hexCenter = round(float4(uv, uv - float2(0.5, 1.0)) / s.xyxy);
                float4 shift = float4(uv - hexCenter.xy * s, uv - (hexCenter.zw + 0.5) * s);
                return dot(shift.xy, shift.xy) < dot(shift.zw, shift.zw) ?
                    float4(shift.xy, hexCenter.xy) : float4(shift.zw, hexCenter.zw+10);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 hex = calcHexInfo(i.uv*5+_SinTime.xy);
                float distSqr = dot(hex.xy, hex.xy);
                float mod = distSqr;
                float3 col = float3( (hex.z*32+_Time.x)%3*0.333333, (hex.w*23+_Time.y)%5*0.2, (hex.z*hex.w+_Time.z)%7*0.14 );
                return fixed4(col*mod, 1);
            }

            ENDCG
        }
    }
}
