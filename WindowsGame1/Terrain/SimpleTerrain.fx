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

const float3 LightDirection = normalize(float3(1, -1, 0));
const float4 LightAmbient = float4(0.1, 0.1, 0.1, 1);


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


//-----------------------------------------------------------------------------
// structures
//-----------------------------------------------------------------------------

struct VertexShaderInput
{
	float3 Position  : POSITION;
	float2 TexCoords : TEXCOORD;
};

struct VertexShaderOutput
{
	float4 Position  : POSITION;
	float2 TexCoords : TEXCOORD1;
};

struct PixelShaderOutput
{
	float4 Colour : COLOR;
};


//-----------------------------------------------------------------------------
// functions
//-----------------------------------------------------------------------------

VertexShaderOutput VertexShader(VertexShaderInput input)
{
	float4 inputPos = float4(input.Position, 1);
	
	VertexShaderOutput output;
	
	// pass vertex position through as usual
  output.Position = mul(inputPos, WorldViewProjection);
  
  // coordinates for texture
  output.TexCoords = input.TexCoords;
  
  return output;
}

PixelShaderOutput PixelShader(VertexShaderOutput input)
{
	PixelShaderOutput output;
	
	float3 normal = normalize((tex2D(NormalMapSampler, input.TexCoords) - 0.5f) * 2);
	float diffuse = dot(LightDirection, normal);
	output.Colour = saturate((tex2D(GrassSampler, input.TexCoords).rgba * diffuse) + LightAmbient);
	//output.Colour = diffuse;
	//output.Colour = tex2D(NormalMapSampler, input.TexCoords);
	//output.Colour = float4(1, 0, 0, 1);
	return output;
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

/*technique PerVertexLighting
{
	pass Pass0
	{
		ZEnable = true;
		FillMode = SOLID;
		CullMode = CW;
		VertexShader = compile vs_1_1 VertexShader();
		PixelShader = compile ps_1_1 PixelShader();
	}
}*/

technique PerPixelLighting
{
	pass Pass0
	{
		ZEnable = true;
		FillMode = SOLID;
		CullMode = CW;
		VertexShader = compile vs_1_1 VertexShader();
		PixelShader = compile ps_2_0 PixelShader();
	}
}