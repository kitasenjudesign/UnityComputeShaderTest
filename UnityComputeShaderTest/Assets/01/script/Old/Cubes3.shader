Shader "Cubes3"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert addshadow
		#pragma instancing_options procedural:setup
		
		struct Input
		{
			float2 uv_MainTex;
            float4 col;//position  : POSITION;
		};
		// Boidの構造体
		struct CubeData
		{
			float3 position;
			float3 velocity;
			float4 color;
			float3 basePos;
		};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			// Boidデータの構造体バッファ
			StructuredBuffer<CubeData> _CubeDataBuffer;
		#endif

		sampler2D _MainTex; // テクスチャ

		half   _Glossiness; // 光沢
		half   _Metallic;   // 金属特性
		fixed4 _Color;      // カラー

		float4x4 _modelMatrix;
		float3 _ObjectScale; // Boidオブジェクトのスケール

		// オイラー角（ラジアン）を回転行列に変換
		float4x4 eulerAnglesToRotationMatrix(float3 angles)
		{
			float ch = cos(angles.y); float sh = sin(angles.y); // heading
			float ca = cos(angles.z); float sa = sin(angles.z); // attitude
			float cb = cos(angles.x); float sb = sin(angles.x); // bank

			// Ry-Rx-Rz (Yaw Pitch Roll)
			return float4x4(
				ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
				cb * sa, cb * ca, -sb, 0,
				-sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
				0, 0, 0, 1
			);
		}

		// 頂点シェーダ
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

			// インスタンスIDからBoidのデータを取得
			CubeData cubeData = _CubeDataBuffer[unity_InstanceID]; 

			float3 pos = cubeData.position.xyz; // Boidの位置を取得
			float3 scl = float3(0.1,0.1,0.1);//_ObjectScale;          // Boidのスケールを取得

			// オブジェクト座標からワールド座標に変換する行列を定義
			float4x4 object2world = (float4x4)0; 
			// スケール値を代入
			object2world._11_22_33_44 = float4(scl.xyz, 1.0);
			// 速度からY軸についての回転を算出
			float rotY = 
				atan2(cubeData.velocity.x, cubeData.velocity.z);
			// 速度からX軸についての回転を算出
			float rotX = 
				-asin(cubeData.velocity.y / (length(cubeData.velocity.xyz) + 1e-8));
			// オイラー角（ラジアン）から回転行列を求める
			float4x4 rotMatrix = eulerAnglesToRotationMatrix(float3(rotX, rotY, 0));
			// 行列に回転を適用
			object2world = mul(rotMatrix, object2world);
			// 行列に位置（平行移動）を適用
			object2world._14_24_34 += pos.xyz;

			// 頂点を座標変換
			//
			v.vertex = mul(object2world, v.vertex);
			// 法線を座標変換
			v.normal = normalize(mul(object2world, v.normal));

			
			o.col = _CubeDataBuffer[unity_InstanceID].color;//色


			#endif
		}
		
		void setup()
		{
		}

		// サーフェスシェーダ
		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = IN.col.xyz;//c.rgb;// * IN.color.xyz;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}