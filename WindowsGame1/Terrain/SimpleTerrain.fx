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

const float TerrainSize;


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
	float4 Position          : POSITION;
	float2 TexCoords         : TEXCOORD0;
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
	output.Colour = tex2D(GrassSampler, input.TexCoords).rgba;
	return output;
}


//-----------------------------------------------------------------------------
// techniques
//-----------------------------------------------------------------------------

technique Normal
{
	pass Pass0
	{
		ZEnable = true;
		FillMode = WIREFRAME;
		CullMode = CW;
		VertexShader = compile vs_1_1 VertexShader();
		PixelShader = compile ps_1_1 PixelShader();
	}
}