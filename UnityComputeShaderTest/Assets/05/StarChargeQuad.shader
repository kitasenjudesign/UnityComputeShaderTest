Shader "ShootingStar/StarChargeQuad"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex ("_MainTex", 2D) = "white" {}
        _ColTex ("_ColTex", 2D) = "white" {}
        _GradeTex ("_GradeTex", 2D) = "white" {}
        _Size("_Size",float) = 1
        
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
        ZWrite ON
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "./Easing.cginc"

			// Boidの構造体
			struct CubeData
			{
				float3 position;
				float3 velocity;
				float4 color;
				float timeRatio;
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
                float4 col : Color;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            StructuredBuffer<CubeData> _CubeDataBuffer;
            float3 _DokabenMeshScale;

            sampler2D _MainTex;
            sampler2D _ColTex;
            sampler2D _GradeTex;
            
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize; 
            fixed4 _Color;
            float _Duration;
            float _Size;

            v2f vert (appdata_t v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                CubeData data = _CubeDataBuffer[instanceID];
                float size = sin(saturate(data.timeRatio)*3.1415);//_CubeDataBuffer[instanceID].time/_Duration;

                //だんだん小さくなっていく
                //float s = ( 1-timeRatio );
                //s += 0.002 * sin(timeRatio * 100 + 50 * data.color.x);
                float s = 0.2 * size * _Size * data.uv.x;

                // スケールと位置(平行移動)を適用
                float4x4 matrix_ = (float4x4)0;
                matrix_._11_22_33_44 = float4(s,s,s, 1.0);//scale
                matrix_._14_24_34 += data.position;//translate
                //v.vertex = mul(matrix_, v.vertex);//world座標

				// billboard mesh towards camera
				float3 vpos = mul((float3x3)matrix_, v.vertex.xyz);
				float4 worldCoord = float4(matrix_._m03, matrix_._m13, matrix_._m23, 1);
				float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				float4 outPos = mul(UNITY_MATRIX_P, viewPos);

                o.vertex = outPos;
                
                float2 gradePos = float2(
                    saturate( saturate(1-data.timeRatio) + 0.3*(data.color.x-0.5) ),
                    0.5
                );
                o.col = float4(gradePos.xy,0,1);
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 grade = tex2D(_GradeTex,  i.texcoord.xy);
                fixed4 col0=tex2D(_MainTex,  i.texcoord.xy);
                fixed4 col = tex2D(_ColTex,  i.col.xy);
                col.rgb = col.rgb +(pow(grade.rgb,1)*0.05);
                //col.a = col0.a;
                clip(col0.a-0.5);
                return col;
            }
            ENDCG
        }
    }
}