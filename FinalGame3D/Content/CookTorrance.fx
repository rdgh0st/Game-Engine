float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;
float3 LightPosition;
float Shininess;
float Roughness; // clamp from 0-1
float4 AmbientColor;
float AmbientIntensity;
float4 SpecularColor;
float4 DiffuseColor;
float DiffuseIntensity;
texture decalMap;
float SpecularIntensity = 1;
float4 LightColor;
float F0;


sampler tsampler1 = sampler_state {
	texture = <decalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput {
    float4 Position: POSITION0;
    float4 Normal: NORMAL0;
    float2 TexCoord: TEXCOORD0;
};

struct VertexShaderOutput {
    float2 TexCoord: TEXCOORD0;
    float4 Position: POSITION0;
	float4 Color: COLOR;
	float4 Normal: NORMAL0;
	float4 WorldPosition: POSITION1;
};

VertexShaderOutput CookTorranceVS(VertexShaderInput input) {
    VertexShaderOutput output;

	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.Position = mul(viewPos, Projection);
	output.WorldPosition = worldPos;
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	output.Color = 0;
    output.TexCoord = input.TexCoord;

	return output;
}

float4 CookTorrancePSTexture(VertexShaderOutput input) : COLOR {

    float3 N = normalize(input.Normal.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 L = normalize(LightPosition);
	float3 H = normalize(L + V);
    float VdotH = dot(V, H);
    float NdotH = dot(N, H);
    float LdotH = dot(L, H);
    float NdotL = dot(N, L);
    float NdotV = dot(N, V);

    float4 texColor = tex2D(tsampler1, input.TexCoord);
	float4 ambient = ambient = AmbientColor * texColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * min(max(0, NdotL), 1);

    if (NdotH <= 0 || NdotL <= 0) {
        return texColor * (ambient + diffuse);
    }
	
    // float3 specular intensity is the cook torrance formula, multiply it with specular color
    float denom = 4 * NdotL * NdotV;

    // GGXDistribution
    float alphaSquare = pow(Roughness, 2);
    float pi = 3.14159265358979323846;
    float denomGGX = pi * pow((pow(NdotH, 2) * (alphaSquare - 1)) + 1, 2);
    float GGXDistribution = alphaSquare / denomGGX;

    // FresnelSchlick
    float M = pow(min(0, max(1, 1 - VdotH)), 5);
    float FresnelSchlick = F0 + (1 - F0) * M;

    // Geometry
    float K = pow((Roughness + 1), 2) / 8;
    float denomG1L = (NdotL * (1 - K)) + K;
    float G1L = NdotL / denomG1L;

    float denomG1V = (NdotV * (1 - K)) + K;
    float G1V = NdotV / denomG1V;

    float Geometry = G1L * G1V;

    // Apply Cook-Torrance Formula and find final Specular value
    float numer = GGXDistribution * FresnelSchlick * Geometry;
    float cookTorranceValue = numer / denom;
    float4 finalSpecular = cookTorranceValue * SpecularColor * SpecularIntensity * NdotL;

    // integrate all color values into final return color
    float4 color = LightColor * saturate(NdotL) * ((texColor * diffuse) + finalSpecular) + ambient;
    return color;
}

float4 CookTorrancePSNoTexture(VertexShaderOutput input) : COLOR {

    float3 N = normalize(input.Normal.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 L = normalize(LightPosition);
	float3 H = normalize(L + V);
    float VdotH = dot(V, H);
    float NdotH = dot(N, H);
    float LdotH = dot(L, H);
    float NdotL = dot(N, L);
    float NdotV = dot(N, V);

	float4 ambient = ambient = AmbientColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * min(max(0, NdotL), 1);

    if (NdotH <= 0 || NdotL <= 0) {
        return (ambient + diffuse);
    }
	
    // float3 specular intensity is the cook torrance formula, multiply it with specular color
    float denom = 4 * NdotL * NdotV;

    // GGXDistribution
    float alphaSquare = pow(Roughness, 2);
    float pi = 3.14159265358979323846;
    float denomGGX = pi * pow((pow(NdotH, 2) * (alphaSquare - 1)) + 1, 2);
    float GGXDistribution = alphaSquare / denomGGX;

    // FresnelSchlick
    float M = pow(min(0, max(1, 1 - VdotH)), 5);
    float FresnelSchlick = F0 + (1 - F0) * M;

    // Geometry
    float K = pow((Roughness + 1), 2) / 8;
    float denomG1L = (NdotL * (1 - K)) + K;
    float G1L = NdotL / denomG1L;

    float denomG1V = (NdotV * (1 - K)) + K;
    float G1V = NdotV / denomG1V;

    float Geometry = G1L * G1V;

    // Apply Cook-Torrance Formula and find final Specular value
    float numer = GGXDistribution * FresnelSchlick * Geometry;
    float cookTorranceValue = numer / denom;
    float4 finalSpecular = cookTorranceValue * SpecularColor * SpecularIntensity * NdotL;

    // integrate all color values into final return color
    float4 color = LightColor * saturate(NdotL) * ((diffuse) + finalSpecular) + ambient;
    return color;
}

technique CookTorranceTexture {
	pass Pass1 {
		VertexShader = compile vs_3_0 CookTorranceVS();
		PixelShader = compile ps_3_0 CookTorrancePSTexture();
	}
}

technique CookTorranceNoTexture {
	pass Pass1 {
		VertexShader = compile vs_3_0 CookTorranceVS();
		PixelShader = compile ps_3_0 CookTorrancePSNoTexture();
	}
}