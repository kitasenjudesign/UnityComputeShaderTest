Shader "Cubes2" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Amount ("_Amount", Range(0,5)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows
		#pragma surface surf Standard addshadow vertex:vert
        //#pragma surface surf Standard vertex:vert
        #pragma instancing_options procedural:setup

		// Use shader model 3.0 target, to get nicer looking lighting
		//#pragma target 4.5

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
            float4 col;//position  : POSITION;
		};
		struct CubeData
		{
			float3 position;
			float3 velocity;
			float4 color;
		};
			
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        	StructuredBuffer<CubeData> _CubeDataBuffer;
		#endif

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Amount;
        
		void setup(){
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            #endif
        }

		float3 rotate(float3 p, float angle, float3 axis){
			float3 a = normalize(axis);
			float s = sin(angle);
			float c = cos(angle);
			float r = 1.0 - c;
			float3x3 m = float3x3(
				a.x * a.x * r + c,
				a.y * a.x * r + a.z * s,
				a.z * a.x * r - a.y * s,
				a.x * a.y * r - a.z * s,
				a.y * a.y * r + c,
				a.z * a.y * r + a.x * s,
				a.x * a.z * r + a.y * s,
				a.y * a.z * r - a.x * s,
				a.z * a.z * r + c
			);
			return mul(m,p);
		}

        void vert(inout appdata_full v, out Input o )
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
			
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				
				v.vertex.xyz = rotate(v.vertex.xyz,_Time.z*3,_Time.xyz);
				v.normal.xyz = rotate(v.normal.xyz,_Time.z*3,_Time.xyz);

                float4x4 matrix_ = (float4x4)0;
                //スケール
                matrix_._11_22_33_44 = float4(0.2,0.2,0.2,1.0);//float4(_DokabenMeshScale.xyz, 1.0);
                //移動
                matrix_._14_24_34 += _CubeDataBuffer[unity_InstanceID].position;
                v.vertex = mul(matrix_, v.vertex);
            
				//o.color = _CubeDataBuffer[unity_InstanceID].color;
				//o.col = fixed4(1,0,0,1);
				o.col = _CubeDataBuffer[unity_InstanceID].color;
            	//o.position = mul(UNITY_MATRIX_VP,v.vertex);//UnityObjectToClipPos(v.vertex);
			#endif

		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			
            // Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = IN.col.xyz;
			//o.Albedo = c.rgb * IN.col.xyz;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

		}
		ENDCG
	}
	FallBack "Diffuse"
}