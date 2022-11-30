// Parameters that should be set from the program
float4x4 World; // World Matrix
float4x4 View; // View Matrix
float4x4 Projection; // Projection Matrix
float3 LightPosition; // in world space
float3 CameraPosition; // in world space
float Shininess; // scalar value
float3 AmbientColor;
float3 DiffuseColor;
float3 SpecularColor;
texture DiffuseTexture;

sampler DiffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
};
// We create structs to help us manage the inputs/outputs
// to vertex and pixel shaders
struct VertexInput
{
    float4 Position : POSITION0; // Here, POSITION0 and NORMAL0
	float3 Normal : NORMAL0; // are called mnemonics
	float2 UV: TEXCOORD0;
};
// --- Per Vertex technique - Gouraud
// We will create an output struct for this technique
struct GouraudVertexOutput
{
    float4 Position : POSITION0;
	float3 Color : COLOR0;
};
// The Vertex shader
// We will perform all the lighting calculations here
GouraudVertexOutput GouraudVertex(VertexInput input)
{
	GouraudVertexOutput output; // create the output struct
	// First, do the transformations
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	// Now do the lighting computation. First calculate the directions
	float3 lightDirection = normalize(LightPosition - worldPosition.xyz);
	float3 viewDirection = normalize(CameraPosition - worldPosition.xyz);
	float3 worldNormal = mul(input.Normal, World);
	float3 reflectDirection = -reflect(lightDirection, worldNormal);
	// Now compute the light components
	float diffuse = max(dot(lightDirection, worldNormal), 0);
	float specular = pow(max(dot(reflectDirection, viewDirection), 0), Shininess);
	// Finally, save the color to the output struct
	output.Color = AmbientColor + diffuse * DiffuseColor + specular * SpecularColor;
    return output;
}
// The Pixel Shader
// All this function does is to pass through the color through 
float4 GouraudPixel(GouraudVertexOutput input) : COLOR0
{
    return float4(input.Color, 1);
}

// --- Per Pixel techniques - Phong, Blinn, Schlick
// We will need a different output struct for this
struct PhongVertexOutput
{
	float4 Position : POSITION0;
	float2 UV: TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
};
// A common vertex shader for all the different techniques
PhongVertexOutput PhongVertex(VertexInput input)
{
	PhongVertexOutput output;
	// Do the transformations as before
	// Save the world position for use in the pixel shader
	output.WorldPosition = mul(input.Position, World);
	float4 viewPosition = mul(output.WorldPosition, View);
	output.Position = mul(viewPosition, Projection);
	// as well as the normal in world space
	output.WorldNormal = mul(input.Normal, World);
	output.UV = input.UV;
	return output;
}
// The pixel shader performs the lighting
float4 PhongPixel(PhongVertexOutput input) : COLOR0
{
	// The lighting operation, same as in the Gouraud vertex method
	float3 lightDirection = normalize(LightPosition - input.WorldPosition.xyz);
	float3 viewDirection = normalize(CameraPosition - input.WorldPosition.xyz);
	// Need to normalize my incoming normal, length could be less than 1
	input.WorldNormal = normalize(input.WorldNormal);
	float3 reflectDirection = -reflect(lightDirection, input.WorldNormal);
	// Now, compute the lighint components
	float diffuse = max(dot(lightDirection, input.WorldNormal), 0);
	float specular = pow(max(dot(reflectDirection, viewDirection), 0), Shininess);
	float4 final = float4((AmbientColor + diffuse * DiffuseColor + specular * SpecularColor) * tex2D(DiffuseSampler, input.UV), 1);
	return final;
}

struct PhongBlinnVertexOutput
{
	float4 Position : POSITION0;
	float2 UV: TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
};
PhongBlinnVertexOutput PhongBlinnVertex(VertexInput input)
{
	PhongBlinnVertexOutput output;
	// Do the transformations as before
	// Save the world position for use in the pixel shader
	output.WorldPosition = mul(input.Position, World);
	float4 viewPosition = mul(output.WorldPosition, View);
	output.Position = mul(viewPosition, Projection);
	// as well as the normal in world space
	output.WorldNormal = mul(input.Normal, World);
	output.UV = input.UV;
	return output;
}
float4 PhongBlinnPixel(PhongBlinnVertexOutput input) : COLOR0
{
	float3 lightDirection = normalize(LightPosition - input.WorldPosition.xyz);
	float3 viewDirection = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 normal = normalize(input.WorldNormal);
    // ls = ((L + V) · N)^s
	float diffuse = max(dot(lightDirection, input.WorldNormal), 0) * tex2D(DiffuseSampler, input.UV);
	float3 H = (lightDirection + viewDirection) / length(lightDirection + viewDirection);
	float specular = max(0, pow(dot(H, normal), (Shininess * 4)));
	return float4(AmbientColor + diffuse * DiffuseColor + specular * SpecularColor, 1);
}

struct SchlickVertexOutput
{
	float4 Position : POSITION0;
	float2 UV: TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
	float3 WorldNormal : TEXCOORD2;
};

SchlickVertexOutput SchlickVertex(VertexInput input)
{
	SchlickVertexOutput output;
	// Do the transformations as before
	// Save the world position for use in the pixel shader
	output.WorldPosition = mul(input.Position, World);
	float4 viewPosition = mul(output.WorldPosition, View);
	output.Position = mul(viewPosition, Projection);
	// as well as the normal in world space
	output.WorldNormal = mul(input.Normal, World);
	output.UV = input.UV;
	return output;
}
// The pixel shader performs the lighting
float4 SchlickPixel(SchlickVertexOutput input) : COLOR0
{
	float3 lightDirection = normalize(LightPosition - input.WorldPosition.xyz);
	float3 viewDirection = normalize(CameraPosition - input.WorldPosition.xyz);
	input.WorldNormal = normalize(input.WorldNormal);
	float3 reflectDirection = -reflect(lightDirection, input.WorldNormal);
	// Is = t / (s + t – t*s),  t = R · V
	float t = max(dot(reflectDirection, viewDirection), 0);
	float diffuse = max(dot(lightDirection, input.WorldNormal), 0) * tex2D(DiffuseSampler, input.UV);
	float specular = t / (Shininess + t - t * Shininess);
	return float4(AmbientColor + diffuse * DiffuseColor + specular * SpecularColor, 1);
}

// Now the different techniques
technique Gouraud
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 GouraudVertex();
		PixelShader = compile ps_3_0 GouraudPixel();
    }
}
technique Phong
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 PhongVertex();
		PixelShader = compile ps_3_0 PhongPixel();
	}
}
technique PhongBlinn
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 PhongBlinnVertex();
		PixelShader = compile ps_3_0 PhongBlinnPixel();
	}
}
technique Schlick
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 SchlickVertex();
		PixelShader = compile ps_3_0 SchlickPixel();
	}
}
