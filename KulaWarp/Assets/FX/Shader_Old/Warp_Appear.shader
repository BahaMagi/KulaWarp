// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/Warp_Appear" 
{
	/**
	* _MainTex:			Main texture that sets the albedo. Defaults to white if left undefined
	* _Normal:			Normal Map
	* _MetallicSmooth:	Used togeter with _Metallic and _Glossiness 
	* _AO:				Ambient Occlusion map. 
	* _Glossiness:		Used with _MetallicSmooth to define the Smoothness property
	* _Metallic:		Used with _MetallicSmooth to define the Metallic property
	* _Emission:		Base emission of the object. Set this to black if only the dissolve edges are supposed to glow
	* _EdgeColor:		Color of the glowing dissolve edges
	* _EdgeSize:		Size of the dissolve edges relative to object size. 1 means the whole object will glow right away. 0 leads to undefined behaviour
	* _Nois:			Noise map that will determine the dissolve edge pattern. A gradient map (e.g. a cloud map) is recommended to yield a continuos dissolve effect
	* _cutoff:			Used to animate the glow effect. When multiple objects use the same material use the [PerRendererData] version instead to decouple the effect. Keep in mind that this removes the property from the inspector
	*/
	Properties 
	{
		_MainTex		("Albedo", 2D) = "white" {}
		_Normal			("Normal", 2D) = "bump"  {}
		_MetallicSmooth ("Metallic (RGB) Smooth (A)", 2D) = "white" {}
		_AO				("AO", 2D)	   = "white" {}
		_Glossiness		("Smoothness", Range(0,1))	= 0.5
		_Metallic		("Metallic", Range(0,1))	= 0.0
		[HDR]_Emission  ("Emission", Color)			= (0,0,0,0)
		[HDR]_EdgeColor ("Edge Color", Color)		= (1,1,1,1)
		_EdgeSize	    ("EdgeSize", Range(0,1))	= 0.2
		_Noise			("Noise", 2D)  = "white" {}
		//[PerRendererData]_cutoff ("cutoff", Range(0,1)) = 0.0
		_cutoff ("cutoff", Range(0,1)) = 0.0
	}
	SubShader 
	{
		// Set render tags for a cutout shader
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout"}
		Cull Off // Turn off culling to draw the inside of the object you can see during the dissolve process

		LOD 200
		
		////////////////////////
		// Start shader program
		CGPROGRAM

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert addshadow 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#pragma multi_compile __ _USE_GRADIENT_ON

		sampler2D _MainTex;
		sampler2D _Noise;
		sampler2D _Gradient;
		sampler2D _Normal;
		sampler2D _MetallicSmooth;
		sampler2D _AO;

		struct Input 
		{
			float2 uv_Noise;
			float2 uv_MainTex;
			fixed4 color : COLOR0;
			float3 worldPos;
		};


		half _Glossiness, _Metallic, _Cutoff, _EdgeSize;
		half _cutoff;
		half4 _EdgeColor, _Emission;


		void vert (inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			half3 Noise = tex2D (_Noise, IN.uv_Noise);
			_cutoff     = lerp(0, _cutoff + _EdgeSize, _cutoff);
			half Edge   = smoothstep(1-_cutoff + _EdgeSize, 1-_cutoff, clamp(Noise.r, _EdgeSize, 1));

			fixed4 c             = tex2D (_MainTex, IN.uv_MainTex);
			fixed3 EmissiveCol   = c.a * _Emission;
			half4 MetallicSmooth = tex2D(_MetallicSmooth, IN.uv_MainTex);

			o.Albedo     = c.rgb;
			o.Occlusion  = tex2D (_AO, IN.uv_MainTex);
			o.Emission   = EmissiveCol + _EdgeColor * Edge;
			o.Normal     = UnpackNormal (tex2D (_Normal, IN.uv_MainTex));
			o.Metallic   = MetallicSmooth.r * _Metallic;
			o.Smoothness = MetallicSmooth.a * _Glossiness;
			
			clip(_cutoff - Noise); // Clip pixel if _cutoff - Noise is negative
		}
		ENDCG
		// End shader program
		////////////////////////
	}
	FallBack "Diffuse"
}
