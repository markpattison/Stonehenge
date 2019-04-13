// GroundFromAtmosphere

// constants
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xLightDirection;
float4 xClipPlane;
float xAmbient;
float3 xCameraPosition;
float2 xMinMaxHeight;

float xPerlinSize3D;

float xG;
float xGSquared;
float3 xInvWavelength4;
float xKrESun;
float xKmESun;
float xKr4Pi;
float xKm4Pi;

float xInnerRadius;
float xOuterRadius;
float xOuterRadiusSquared;
float xScale;
float xScaleDepth;
float xScaleOverScaleDepth;

int xSamples = 4;

texture xGrassTexture;
sampler GrassTextureSampler = sampler_state
{
    texture = <xGrassTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

texture xRockTexture;
sampler RockTextureSampler = sampler_state
{
    texture = <xRockTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

texture xSandTexture;
sampler SandTextureSampler = sampler_state
{
    texture = <xSandTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

texture xSnowTexture;
sampler SnowTextureSampler = sampler_state
{
    texture = <xSnowTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

texture xRandomTexture3D;
sampler RandomTextureSampler3D = sampler_state { texture = <xRandomTexture3D>; AddressU = WRAP; AddressV = WRAP; AddressW = WRAP; };

struct GroundFromAtmosphere_ToVertex
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
	float2 TexCoords : TEXCOORD0;
};

struct GroundFromAtmosphere_VertexToPixel
{
	float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
	float3 ScatteringColour : COLOR0;
	float3 Attenuation : COLOR1;
	float2 TextureCoords : TEXCOORD1;
	float Depth : TEXCOORD4;
	float3 WorldPosition: TEXCOORD5;
};

struct PixelToFrame
{
	float4 Color : COLOR0;
};

float Perlin3D(float3 pIn)
{
	float3 p = (pIn + 0.5) * xPerlinSize3D;

	float3 posAAA = floor(p);
	float3 t = p - posAAA;

	float3 posBAA = posAAA + float3(1.0, 0.0, 0.0);
	float3 posABA = posAAA + float3(0.0, 1.0, 0.0);
	float3 posBBA = posAAA + float3(1.0, 1.0, 0.0);
	float3 posAAB = posAAA + float3(0.0, 0.0, 1.0);
	float3 posBAB = posAAA + float3(1.0, 0.0, 1.0);
	float3 posABB = posAAA + float3(0.0, 1.0, 1.0);
	float3 posBBB = posAAA + float3(1.0, 1.0, 1.0);

	float3 colAAA = tex3D(RandomTextureSampler3D, posAAA / xPerlinSize3D).xyz * 4.0 - 1.0;
	float3 colBAA = tex3D(RandomTextureSampler3D, posBAA / xPerlinSize3D).xyz * 4.0 - 1.0;
	float3 colABA = tex3D(RandomTextureSampler3D, posABA / xPerlinSize3D).xyz * 4.0 - 1.0;
	float3 colBBA = tex3D(RandomTextureSampler3D, posBBA / xPerlinSize3D).xyz * 4.0 - 1.0;
	float3 colAAB = tex3D(RandomTextureSampler3D, posAAB / xPerlinSize3D).xyz * 4.0 - 1.0;
	float3 colBAB = tex3D(RandomTextureSampler3D, posBAB / xPerlinSize3D).xyz * 4.0 - 1.0;
	float3 colABB = tex3D(RandomTextureSampler3D, posABB / xPerlinSize3D).xyz * 4.0 - 1.0;
	float3 colBBB = tex3D(RandomTextureSampler3D, posBBB / xPerlinSize3D).xyz * 4.0 - 1.0;

	float sAAA = dot(colAAA, p - posAAA);
	float sBAA = dot(colBAA, p - posBAA);
	float sABA = dot(colABA, p - posABA);
	float sBBA = dot(colBBA, p - posBBA);
	float sAAB = dot(colAAB, p - posAAB);
	float sBAB = dot(colBAB, p - posBAB);
	float sABB = dot(colABB, p - posABB);
	float sBBB = dot(colBBB, p - posBBB);

	//float3 s = t * t * (3 - 2 * t);
	float3 s = t * t * t * (t * (t * 6 - 15) + 10);

	float sPAA = sAAA + s.x * (sBAA - sAAA);
	float sPAB = sAAB + s.x * (sBAB - sAAB);
	float sPBA = sABA + s.x * (sBBA - sABA);
	float sPBB = sABB + s.x * (sBBB - sABB);

	float sPPA = sPAA + s.y * (sPBA - sPAA);
	float sPPB = sPAB + s.y * (sPBB - sPAB);

	float sPPP = sPPA + s.z * (sPPB - sPPA);
	return sPPP;
}

float4 Perlin3DwithDerivatives(float3 pIn)
{
    float3 p = (pIn + 0.5) * xPerlinSize3D;

    float3 posAAA = floor(p);
    float3 t = p - posAAA;

    float3 posBAA = posAAA + float3(1.0, 0.0, 0.0);
    float3 posABA = posAAA + float3(0.0, 1.0, 0.0);
    float3 posBBA = posAAA + float3(1.0, 1.0, 0.0);
    float3 posAAB = posAAA + float3(0.0, 0.0, 1.0);
    float3 posBAB = posAAA + float3(1.0, 0.0, 1.0);
    float3 posABB = posAAA + float3(0.0, 1.0, 1.0);
    float3 posBBB = posAAA + float3(1.0, 1.0, 1.0);

    float3 colAAA = tex3D(RandomTextureSampler3D, posAAA / xPerlinSize3D).xyz * 4.0 - 1.0;
    float3 colBAA = tex3D(RandomTextureSampler3D, posBAA / xPerlinSize3D).xyz * 4.0 - 1.0;
    float3 colABA = tex3D(RandomTextureSampler3D, posABA / xPerlinSize3D).xyz * 4.0 - 1.0;
    float3 colBBA = tex3D(RandomTextureSampler3D, posBBA / xPerlinSize3D).xyz * 4.0 - 1.0;
    float3 colAAB = tex3D(RandomTextureSampler3D, posAAB / xPerlinSize3D).xyz * 4.0 - 1.0;
    float3 colBAB = tex3D(RandomTextureSampler3D, posBAB / xPerlinSize3D).xyz * 4.0 - 1.0;
    float3 colABB = tex3D(RandomTextureSampler3D, posABB / xPerlinSize3D).xyz * 4.0 - 1.0;
    float3 colBBB = tex3D(RandomTextureSampler3D, posBBB / xPerlinSize3D).xyz * 4.0 - 1.0;

    float sAAA = dot(colAAA, p - posAAA);
    float sBAA = dot(colBAA, p - posBAA);
    float sABA = dot(colABA, p - posABA);
    float sBBA = dot(colBBA, p - posBBA);
    float sAAB = dot(colAAB, p - posAAB);
    float sBAB = dot(colBAB, p - posBAB);
    float sABB = dot(colABB, p - posABB);
    float sBBB = dot(colBBB, p - posBBB);

	//float3 s = t * t * (3.0 - 2.0 * t);
    float3 s = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float3 ds = t * t * (t * (t * 30.0 - 60.0) + 30.0);

    float cx = sBAA - sAAA;
    float cy = sABA - sAAA;
    float cz = sAAB - sAAA;

    float cxy = sBBA - sABA - sBAA + sAAA;
    float cxz = sBAB - sAAB - sBAA + sAAA;
    float cyz = sABB - sAAB - sABA + sAAA;

    float cxyz = sBBB - sABB - sBAB + sAAB - sBBA + sABA + sBAA - sAAA;

    float sxy = s.x * s.y;
    float sxz = s.x * s.z;
    float syz = s.y * s.z;
    float sxyz = s.x * s.y * s.z;

    float noise = sAAA
        + cx * s.x + cy * s.y + cz * s.z
        + cxy * sxy + cxz * sxz + cyz * syz
        + cxyz * sxyz;

    float4 derivAndNoise = float4(
        ds.x * (cx + cxy * s.y + cxz * s.z + cxyz * syz),
        ds.y * (cy + cxy * s.x + cyz * s.z + cxyz * sxz),
        ds.z * (cz + cxz * s.x + cyz * s.y + cxyz * sxy),
        noise);

    return derivAndNoise;
}

struct ScatteringResult
{
	float3 ScatteringColour;
	float3 Attenuation;
};

float scale(float cos)
{
	float x = max(0.0, 1.0 - cos);
	return xScaleDepth * exp(-0.00287 + x * (0.459 + x * (3.83 + x  *(-6.80 + x * 5.25))));
}

ScatteringResult Scattering(float3 worldPosition)
{
	ScatteringResult output = (ScatteringResult)0;

	float3 cameraInPlanetSpace = xCameraPosition + float3(0.0, xInnerRadius, 0.0);
	float3 vertexInPlanetSpace = worldPosition + float3(0.0, xInnerRadius, 0.0);

	float3 viewDirection = normalize(vertexInPlanetSpace - cameraInPlanetSpace);
	float distanceToVertex = length(vertexInPlanetSpace - cameraInPlanetSpace);
	float vertexHeight = length(vertexInPlanetSpace);
	float cameraHeight = length(cameraInPlanetSpace);
	float startDepth = exp((xInnerRadius - cameraHeight) * xScaleOverScaleDepth);

	float vertexHigher = (vertexHeight > cameraHeight) ? -1.0 : 1.0;

	float cameraAngle = vertexHigher * dot(-viewDirection, vertexInPlanetSpace) / vertexHeight;
	float lightAngle = -dot(xLightDirection, vertexInPlanetSpace) / vertexHeight;
	float cameraScale = vertexHigher * scale(cameraAngle);
	float lightScale = scale(lightAngle);
	float cameraOffset = startDepth * cameraScale;
	float totalScale = lightScale + cameraScale;

	float sampleLength = distanceToVertex / xSamples;
	float scaledLength = sampleLength * xScale;
	float3 sampleRay = viewDirection * sampleLength;
	float3 samplePoint = cameraInPlanetSpace + sampleRay * 0.5;
	float3 attenuate;

	float3 accumulatedColour = float3(0.0, 0.0, 0.0);
	for (int i = 0; i < xSamples; i++)
	{
		float height = length(samplePoint);
		float depth = exp(xScaleOverScaleDepth * (xInnerRadius - height));
		float scatter = depth * totalScale - cameraOffset;
		attenuate = exp(-scatter * (xInvWavelength4 * xKr4Pi + xKm4Pi));

		accumulatedColour += attenuate * (depth * scaledLength);
		samplePoint += sampleRay;
	}

	float finalHeight = length(vertexInPlanetSpace);
	float finalDepth = exp(xScaleOverScaleDepth * (xInnerRadius - finalHeight));
	float finalScatter = finalDepth * totalScale - cameraOffset;
	float3 finalAttenuate = exp(-finalScatter * (xInvWavelength4 * xKr4Pi + xKm4Pi));

	output.ScatteringColour = accumulatedColour * (xInvWavelength4 * xKrESun + xKmESun);
	output.Attenuation = finalAttenuate;

	return output;
}

GroundFromAtmosphere_VertexToPixel GroundFromAtmosphereVS(GroundFromAtmosphere_ToVertex VSInput)
{
	GroundFromAtmosphere_VertexToPixel output = (GroundFromAtmosphere_VertexToPixel)0;

	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

	float4 worldPosition = mul(VSInput.Position, xWorld);
	output.WorldPosition = worldPosition.xyz;
	output.Position = mul(VSInput.Position, preWorldViewProjection);
	output.TextureCoords = VSInput.TexCoords;

	float3 normal = normalize(mul(float4(normalize(VSInput.Normal), 0.0), xWorld)).xyz;
    output.Normal = normal;

	output.Depth = output.Position.z / output.Position.w;

	ScatteringResult scattering = Scattering(worldPosition.xyz);

	output.ScatteringColour = scattering.ScatteringColour;
	output.Attenuation = scattering.Attenuation;

	return output;
}

float Turbulence(float3 pos, float f)
{
    float t = -.5;
    for (; f <= xPerlinSize3D / 12.0; f *= 2.0)
        t += abs(Perlin3D(pos) / f);
    return t;
}

float3 BumpMapNoiseGradient(float3 worldPosition)
{
    float3 pos = worldPosition / 10.0;

    return Perlin3DwithDerivatives(pos).xyz * 0.04;
}

PixelToFrame GroundFromAtmospherePS(GroundFromAtmosphere_VertexToPixel PSInput)
{
    PixelToFrame output = (PixelToFrame) 0;

    float4 nearColour = tex2D(GrassTextureSampler, PSInput.TextureCoords * 3.0);

    float3 normal = normalize(PSInput.Normal - BumpMapNoiseGradient(PSInput.WorldPosition));

    float lightingFactor = clamp(dot(normal, -xLightDirection), 0.0, 1.0);

	output.Color = nearColour;
    output.Color.rgb *= (saturate(lightingFactor) + xAmbient);
	output.Color.rgb *= PSInput.Attenuation;
	output.Color.rgb += PSInput.ScatteringColour;

	return output;
}

technique GroundFromAtmosphere
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 GroundFromAtmosphereVS();
		PixelShader = compile ps_4_0 GroundFromAtmospherePS();
	}
}

struct ColouredVertexToPixel
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 WorldPosition : TEXCOORD0;
    float3 ScatteringColour : COLOR0;
    float3 Attenuation : COLOR1;
};

ColouredVertexToPixel ColouredVS(float4 inPos : SV_POSITION, float3 inNormal : NORMAL)
{
    ColouredVertexToPixel Output = (ColouredVertexToPixel) 0;

    float4x4 preViewProjection = mul(xView, xProjection);
    float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);

    float3 normal = normalize(mul(float4(normalize(inNormal), 0.0), xWorld)).xyz;
    Output.Normal = normal;

    float4 worldPosition = mul(inPos, xWorld);
    Output.Position = mul(inPos, preWorldViewProjection);
    Output.WorldPosition = worldPosition.xyz;

    ScatteringResult scattering = Scattering(worldPosition.xyz);

    Output.ScatteringColour = scattering.ScatteringColour;
    Output.Attenuation = scattering.Attenuation;

    return Output;
}

PixelToFrame ColouredPS(ColouredVertexToPixel PSInput)
{
    PixelToFrame Output = (PixelToFrame) 0;

    Output.Color = float4(1.0, 1.0, 1.0, 1.0);

    float3 normal = PSInput.Normal;

    float3 reflectionVector = -reflect(xLightDirection, normal);
    float specular = dot(normalize(reflectionVector), normalize(PSInput.WorldPosition - xCameraPosition));
    specular = pow(max(specular, 0.0), 256);

    float lightingFactor = clamp(dot(normal, -xLightDirection), 0.0, 1.0);

    Output.Color.rgb *= (saturate(lightingFactor) + xAmbient);
    Output.Color.rgb += specular;
    Output.Color.rgb *= PSInput.Attenuation;
    Output.Color.rgb += PSInput.ScatteringColour;

    return Output;
}

technique Coloured
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 ColouredVS();
        PixelShader = compile ps_4_0 ColouredPS();
    }
}