// Render Shadow Map

const matrix LightWorldViewProjection : WORLDVIEWPROJECTION;

struct SMVS_INPUT
{
	float4 Pos : POSITION;
};

struct SMVS_OUTPUT
{
	float4 Pos      : POSITION;
	float3 PixelPos : TEXCOORD0;
};

struct SMPS_OUTPUT
{
	float4 Color : COLOR0;
};

SMVS_OUTPUT ShadowMapVertexShader(SMVS_INPUT input)
{
	SMVS_OUTPUT output;
	
  // pass vertex position through as usual
  output.Pos = mul(input.Pos, LightWorldViewProjection);
  
  // output pixel pos
  output.PixelPos = output.Pos.xyz;
  
  return output;
}

SMPS_OUTPUT ShadowMapPixelShader(SMVS_OUTPUT input)
{
  // write z coordinate (linearized depth) to texture
  SMPS_OUTPUT output;
  output.Color = input.PixelPos.z;
  return output;
}

technique ShadowMap
{
	pass Pass0
	{
		CullMode = CW;
		VertexShader = compile vs_2_0 ShadowMapVertexShader();
		PixelShader = compile ps_2_0 ShadowMapPixelShader();
	}
}