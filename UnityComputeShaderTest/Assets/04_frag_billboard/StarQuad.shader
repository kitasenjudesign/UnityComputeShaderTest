Shader "StarQuad"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("_MainTex", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
        Cull Off
        Lighting Off
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

			// Boidの構造体
			struct CubeData
			{
				float3 position;
				float3 velocity;
				float4 color;
				float3 basePos;
				float2 uv;
				float time;
			};

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f 
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            StructuredBuffer<CubeData> _CubeDataBuffer;
            float3 _DokabenMeshScale;

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize; 
            fixed4 _Color;

            v2f vert (appdata_t v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // スケールと位置(平行移動)を適用
                float4x4 matrix_ = (float4x4)0;
                matrix_._11_22_33_44 = float4(0.05,0.05,0.05, 1.0);//scale
                matrix_._14_24_34 += _CubeDataBuffer[instanceID].position;//translate
                //v.vertex = mul(matrix_, v.vertex);//world座標

				// billboard mesh towards camera
				float3 vpos = mul((float3x3)matrix_, v.vertex.xyz);
				float4 worldCoord = float4(matrix_._m03, matrix_._m13, matrix_._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				float4 outPos = mul(UNITY_MATRIX_P, viewPos);

                o.vertex = outPos;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
                clip(col.a-0.5);
                return col;
            }
            ENDCG
        }
    }
}