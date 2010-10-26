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
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler NormalMapSampler = sampler_state
{
	Texture = <NormalMapTexture>;
	MagFilter = ANISOTROPIC;
	MinFilter = ANISOTROPIC;
	MipFilter = LINEAR;
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
  /*AddressU = BORDER;
  AddressV = BORDER;
  BorderColor = 0xFFFFFFFF;*/
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

struct VertexShaderOutput
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

VertexShaderOutput MyVertexShader(VertexShaderInput input)
{
	float4 inputPos = float4(input.Position, 1);
	
	VertexShaderOutput output;
	
	// pass vertex position through as usual
  output.Position = mul(inputPos, WorldViewProjection);
  
  // coordinates for texture
  output.TexCoords = input.TexCoords;
  
	output.Diffuse = LightDiffuse * saturate(dot(LightDirection, input.Normal));
	
	output.ShadowTexCoords = 0;
  
  return output;
}

VertexShaderOutput VertexShaderShadowed(VertexShaderInput input)
{
	VertexShaderOutput output = MyVertexShader(input);
	
	// coordinates for shadowmap
	float4 inputPos = float4(input.Position, 1);
  output.ShadowTexCoords = mul(inputPos, ShadowMapProjector);
  
  return output;
}

PixelShaderOutput MyPixelShader(VertexShaderOutput input)
{	
	PixelShaderOutput output;
	output.Colour = saturate(input.Diffuse + LightAmbient) * tex2D(GrassSampler, input.TexCoords);
	output.Colour.a = 1;
	return output;
}

PixelShaderOutput PixelShaderShadowed(VertexShaderOutput input)
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
	
	// final colour
	PixelShaderOutput output = MyPixelShader(input);
	return output;
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique Shadowed
{
	pass Pass0
	{
		ZEnable = true;
		FillMode = SOLID;
		CullMode = CW;
		VertexShader = compile vs_2_0 VertexShaderShadowed();
		PixelShader = compile ps_2_0 PixelShaderShadowed();
	}
}

technique Normal
{
	pass Pass0
	{
		ZEnable = true;
		FillMode = SOLID;
		CullMode = CW;
		VertexShader = compile vs_2_0 MyVertexShader();
		PixelShader = compile ps_2_0 MyPixelShader();
	}
}

