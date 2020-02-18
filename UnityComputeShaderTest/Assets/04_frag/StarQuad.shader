Shader "StarQuad"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,1,1)
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
                matrix_._11_22_33_44 = float4(0.01,0.01,0.01, 1.0);//scale
                matrix_._14_24_34 += _CubeDataBuffer[instanceID].position;//translate
                v.vertex = mul(matrix_, v.vertex);

				//billboad
                   
                    float3 viewPos = UnityObjectToViewPos(float3(0, 0, 0));
                    
                    // スケールと回転（平行移動なし）だけModel変換して、View変換はスキップ
                    float3 scaleRotatePos = mul((float3x3)unity_ObjectToWorld, v.vertex);
                    
                    // scaleRotatePosを右手系に変換して、viewPosに加算
                    // 本来はView変換で暗黙的にZが反転されているので、
                    // View変換をスキップする場合は明示的にZを反転する必要がある
                    viewPos += float3(scaleRotatePos.xy, -scaleRotatePos.z);
                    
                    o.vertex = mul(UNITY_MATRIX_P, float4(viewPos, 1));


                o.vertex = UnityObjectToClipPos(v.vertex);
               // o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;//tex2D(_MainTex, i.texcoord);
                return col;
            }
            ENDCG
        }
    }
}