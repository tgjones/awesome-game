//-----------------------------------------------------------------------------
// Description: 
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// includes
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// constants
//-----------------------------------------------------------------------------

const matrix WorldViewProjection;

const Texture GrassTexture;

const Texture NormalMapTexture;

const float TerrainSize;

const float4 LightDiffuse = float4(0.5, 0.5, 0.5, 1);
const float3 LightDirection = normalize(float3(2, -1, 1));
const float4 LightAmbient = float4(0.25, 0.25, 0.25, 1);

const texture ShadowMap;
const float ShadowMapSize;
const float ShadowMapSizeInverse;
const matrix ShadowMapProjector;


//-----------------------------------------------------------------------------
// samplers
//-----------------------------------------------------------------------------

sampler GrassSampler = sampler_state
{
	Texture = <GrassTexture>;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;
	MipFilter = ANISOTROPIC;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler NormalMapSampler = sampler_state
{
	Texture = <NormalMapTexture>;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;
	MipFilter = ANISOTROPIC;
	AddressU = WRAP;
	AddressV = WRAP;
};

// no filtering in floating point texture
sampler2D ShadowMapSampler = sampler_state
{
	Texture = <ShadowMap>;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = NONE;
  AddressU = BORDER;
  AddressV = BORDER;
  BorderColor = 0xFFFFFFFF;
};


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VertexShaderInput
{
	float3 Position  : POSITION;
	float3 Normal    : NORMAL;
	float2 TexCoords : TEXCOORD;
};

struct VertexShaderOutput20
{
	float4 Position  : POSITION;
	float2 TexCoords : TEXCOORD1;
};

struct VertexShaderOutput11
{
	float4 Position  : POSITION;
	float2 TexCoords : TEXCOORD1;
	float4 Diffuse   : TEXCOORD2;
	float4 ShadowTexCoords : TEXCOORD3;
};

struct PixelShaderOutput
{
	float4 Colour : COLOR;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

VertexShaderOutput11 VertexShader11(VertexShaderInput input)
{
	float4 inputPos = float4(input.Position, 1);
	
	VertexShaderOutput11 output;
	
	// pass vertex position through as usual
  output.Position = mul(inputPos, WorldViewProjection);
  
  // coordinates for texture
  output.TexCoords = input.TexCoords;
  
	output.Diffuse = LightDiffuse * saturate(dot(LightDirection, input.Normal));
	
	// coordinates for shadowmap
  output.ShadowTexCoords = mul(inputPos, ShadowMapProjector);
  
  return output;
}

PixelShaderOutput PixelShader11(VertexShaderOutput11 input)
{
	float fTexelSize = 1.0f / ShadowMapSize;

	// project texture coordinates
	float4 shadowTexCoords = input.ShadowTexCoords;
	shadowTexCoords.xy /= shadowTexCoords.w;

	// 2x2 PCF Filtering
	float shadowValues[4];
	shadowValues[0] = (shadowTexCoords.z < tex2D(ShadowMapSampler, shadowTexCoords).r);
	shadowValues[1] = (shadowTexCoords.z < tex2D(ShadowMapSampler, shadowTexCoords + float2(ShadowMapSizeInverse, 0)));
	shadowValues[2] = (shadowTexCoords.z < tex2D(ShadowMapSampler, shadowTexCoords + float2(0, ShadowMapSizeInverse)));
	shadowValues[3] = (shadowTexCoords.z < tex2D(ShadowMapSampler, shadowTexCoords + float2(ShadowMapSizeInverse, ShadowMapSizeInverse)));

	float2 lerpFactor = frac(ShadowMapSize * shadowTexCoords);
	float lightingFactor = lerp(lerp(shadowValues[0], shadowValues[1], lerpFactor.x),
															lerp(shadowValues[2], shadowValues[3], lerpFactor.x),
															lerpFactor.y);

	// multiply diffuse with shadowmap lookup value
	input.Diffuse *= lightingFactor;
	//input.Diffuse.a = 0.1;
	
	PixelShaderOutput output;
	output.Colour = saturate(input.Diffuse + LightAmbient) * tex2D(GrassSampler, input.TexCoords);
	//output.Colour = input.Diffuse;
	output.Colour.a = 1;
	//output.Colour = float4(tex2D(ShadowMapSampler, shadowTexCoords).r, 0, 0, 1);
	return output;
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique PerVertexLighting
{
	pass Pass0
	{
		ZEnable = true;
		FillMode = SOLID;
		CullMode = CW;
		VertexShader = compile vs_1_1 VertexShader11();
		PixelShader = compile ps_2_0 PixelShader11();
	}
}
