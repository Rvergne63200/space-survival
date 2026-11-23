#ifndef TERRAIN_WORLD_UV_SPLATMAP_COMMON_CGINC_INCLUDED
#define TERRAIN_WORLD_UV_SPLATMAP_COMMON_CGINC_INCLUDED

// Copy of Unity's TerrainSplatmapCommon.cginc (6000.4) where the texture layers are positioned from the WORLD position
// instead of the terrain corner: the textures do not move with the terrain, so neighbor chunks match whatever the tile size.
// Changes are marked with "World UV".

// Since 2018.3 we changed from _TERRAIN_NORMAL_MAP to _NORMALMAP to save 1 keyword.
// Since 2019.2 terrain keywords are changed to  local keywords so it doesn't really matter. You can use both.
#if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_MAP)
    #define _TERRAIN_NORMAL_MAP
#elif !defined(_NORMALMAP) && defined(_TERRAIN_NORMAL_MAP)
    #define _NORMALMAP
#endif

#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLES)
    // GL doesn't support sperating the samplers from the texture object
    #undef TERRAIN_USE_SEPARATE_VERTEX_SAMPLER
#else
    #define TERRAIN_USE_SEPARATE_VERTEX_SAMPLER
#endif

struct Input
{
    float4 tc;
    float2 worldXZ; // World UV: world position of the pixel, on the horizontal plane
    float worldY;   // Height of the pixel (terrain masks)
    #ifndef TERRAIN_BASE_PASS
        UNITY_FOG_COORDS(0) // needed because finalcolor oppresses fog code generation.
    #endif
};

sampler2D _Control;
float4 _Control_ST;
float4 _Control_TexelSize;
sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

// World UV: terrain size (x, z) in xy, set globally by TerrainGenerator. Unity gives _SplatN_ST.xy = terrainSize / tileSize,
// so _SplatN_ST.xy / terrainSize is the number of texture tiles per meter.
float4 _WorldUVTerrainSize;

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)
    // Some drivers have undefined behaviors when samplers are used from the vertex shader
    // with anisotropic filtering enabled. This causes some artifacts on some devices. To be
    // sure to avoid this we use the vertex_linear_clamp_sampler sampler to sample terrain
    // maps from the VS when we can.
    #if defined(TERRAIN_USE_SEPARATE_VERTEX_SAMPLER)
        UNITY_DECLARE_TEX2D(_TerrainHeightmapTexture);
        UNITY_DECLARE_TEX2D(_TerrainNormalmapTexture);
        SamplerState sampler__TerrainNormalmapTexture;
        SamplerState vertex_linear_clamp_sampler;
    #else
        sampler2D _TerrainHeightmapTexture;
        sampler2D _TerrainNormalmapTexture;
    #endif

    float4    _TerrainHeightmapRecipSize;   // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
    float4    _TerrainHeightmapScale;       // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)
#endif

UNITY_INSTANCING_BUFFER_START(Terrain)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData) // float4(xBase, yBase, skipScale, ~)
UNITY_INSTANCING_BUFFER_END(Terrain)

#ifdef _NORMALMAP
    sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
    float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
#endif

#ifdef _ALPHATEST_ON
    sampler2D _TerrainHolesTexture;

    void ClipHoles(float2 uv)
    {
        float hole = tex2D(_TerrainHolesTexture, uv).r;
        // Fixes bug where compression is enabled and 0 isn't actually 0 but low like 1/2047. (UUM-61913)
        float epsilon = 0.0005f;
        clip(hole < epsilon ? -1 : 1);
    }
#endif

// Terrain masks (TerrainMask): volumes of the constructions where the terrain is not drawn (above their floor, inside their
// walls). World to unit volume matrices; shape 0: box (|p| < 0.5), 1: cylinder along Y (radius 0.5, |p.y| < 0.5)
int _TerrainMaskCount;
float4x4 _TerrainMasks[64];
float _TerrainMaskShapes[64];

void ClipTerrainMasks(float3 worldPosition)
{
    for (int i = 0; i < _TerrainMaskCount; i++)
    {
        float3 p = mul(_TerrainMasks[i], float4(worldPosition, 1)).xyz;
        bool inside = _TerrainMaskShapes[i] > 0.5 ? (dot(p.xz, p.xz) < 0.25 && abs(p.y) < 0.5) : all(abs(p) < 0.5);
        if (inside) discard;
    }
}

#if defined(TERRAIN_BASE_PASS) && defined(UNITY_PASS_META)
    // When we render albedo for GI baking, we actually need to take the ST
    float4 _MainTex_ST;
#endif

void SplatmapVert(inout appdata_full v, out Input data)
{
    UNITY_INITIALIZE_OUTPUT(Input, data);

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)

    float2 patchVertex = v.vertex.xy;
    float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

    float4 uvscale = instanceData.z * _TerrainHeightmapRecipSize;
    float4 uvoffset = instanceData.xyxy * uvscale;
    uvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;
    float2 sampleCoords = (patchVertex.xy * uvscale.xy + uvoffset.xy);

    #if defined(TERRAIN_USE_SEPARATE_VERTEX_SAMPLER)
        float hm = UnpackHeightmap(_TerrainHeightmapTexture.SampleLevel(vertex_linear_clamp_sampler, sampleCoords, 0));
    #else
        float hm = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(sampleCoords, 0, 0)));
    #endif

    v.vertex.xz = (patchVertex.xy + instanceData.xy) * _TerrainHeightmapScale.xz * instanceData.z;  //(x + xBase) * hmScale.x * skipScale;
    v.vertex.y = hm * _TerrainHeightmapScale.y;
    v.vertex.w = 1.0f;

    v.texcoord.xy = (patchVertex.xy * uvscale.zw + uvoffset.zw);
    v.texcoord3 = v.texcoord2 = v.texcoord1 = v.texcoord;

    #ifdef TERRAIN_INSTANCED_PERPIXEL_NORMAL
        v.normal = float3(0, 1, 0); // TODO: reconstruct the tangent space in the pixel shader. Seems to be hard with surface shader especially when other attributes are packed together with tSpace.
        data.tc.zw = sampleCoords;
    #else
        #if defined(TERRAIN_USE_SEPARATE_VERTEX_SAMPLER)
            float3 nor = _TerrainNormalmapTexture.SampleLevel(vertex_linear_clamp_sampler, sampleCoords, 0).xyz;
        #else
            float3 nor = tex2Dlod(_TerrainNormalmapTexture, float4(sampleCoords, 0, 0)).xyz;
        #endif
        v.normal = 2.0f * nor - 1.0f;
    #endif
#endif

    v.tangent.xyz = cross(v.normal, float3(0,0,1));
    v.tangent.w = -1;

    data.tc.xy = v.texcoord.xy;

    // World UV
    float3 worldPosition = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f)).xyz;
    data.worldXZ = worldPosition.xz;
    data.worldY = worldPosition.y;

#ifdef TERRAIN_BASE_PASS
    #ifdef UNITY_PASS_META
        data.tc.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
    #endif
#else
    float4 pos = UnityObjectToClipPos(v.vertex);
    UNITY_TRANSFER_FOG(data, pos);
#endif
}

#ifndef TERRAIN_BASE_PASS

// World UV: texture coordinates of a layer from the world position, with the tile size and offset of the layer
float2 WorldSplatUV(Input IN, float4 splatST)
{
    // Fallback on the terrain UV while the terrain size is not set
    if (_WorldUVTerrainSize.x <= 0.0f || _WorldUVTerrainSize.y <= 0.0f)
    {
        return IN.tc.xy * splatST.xy + splatST.zw;
    }

    return IN.worldXZ * (splatST.xy / _WorldUVTerrainSize.xy) + splatST.zw;
}

// Anti-tiling (hex tiling, after Mikkelsen, "Practical Real-Time Hex-Tiling", JCGT 2022): the UV plane is cut into
// hexagonal cells, each cell samples the layer with its own random offset and rotation, and the three cells around
// a pixel are blended with weights favouring the most contrasted sample, so the blend keeps the contrast. No visible
// period any more. Costs 3 samples per texture instead of 1.
#define HEX_ROTATION 1.0        // 0 = offsets only, 1 = full random rotation
#define HEX_EXPONENT 4.0        // sharpness of the blend between cells (higher = sharper, hexagon edges show)
#define HEX_CELL 0.5            // cells per texture tile and per axis (< 1 : cells larger than the features of the texture)
#define HEX_CONTRAST 0.6        // how much the luminance of each sample drives the blend

void HexTriangleGrid(float2 st, out float3 w, out int2 v1, out int2 v2, out int2 v3)
{
    st *= 2.0 * sqrt(3.0);
    float2 skewed = mul(float2x2(1.0, -0.57735027, 0.0, 1.15470054), st);
    int2 baseId = int2(floor(skewed));
    float3 t = float3(frac(skewed), 0.0);
    t.z = 1.0 - t.x - t.y;
    float s = step(0.0, -t.z);
    float s2 = 2.0 * s - 1.0;
    w = float3(-t.z * s2, s - t.y * s2, s - t.x * s2);
    v1 = baseId + int2(s, s);
    v2 = baseId + int2(s, 1 - s);
    v3 = baseId + int2(1 - s, s);
}

float2 HexHash(float2 p)
{
    float2 r = mul(float2x2(127.1, 311.7, 269.5, 183.3), p);
    return frac(sin(r) * 43758.5453);
}

float2x2 HexRotation(int2 id)
{
    float angle = abs(id.x * id.y) + abs(id.x + id.y) + 3.14159265;
    angle = fmod(angle, 6.2831853);
    if (angle < 0.0) angle += 6.2831853;
    if (angle > 3.14159265) angle -= 6.2831853;
    angle *= HEX_ROTATION;
    float cs = cos(angle), sn = sin(angle);
    return float2x2(cs, -sn, sn, cs);
}

float2 HexCenter(int2 id)
{
    return mul(float2x2(1.0, 0.5, 0.0, 1.0 / 1.15470054), float2(id)) / (2.0 * sqrt(3.0));
}

// The three cells around a pixel : their sample coordinates, rotations and blend weights (w, before the contrast term)
struct HexCells
{
    float2 st1, st2, st3;
    float2x2 r1, r2, r3;
    float2 dx, dy;
    float3 w;
};

HexCells HexSetup(float2 st)
{
    HexCells h;
    h.dx = ddx(st); h.dy = ddy(st);
    int2 v1, v2, v3;
    HexTriangleGrid(st * HEX_CELL, h.w, v1, v2, v3);
    h.r1 = HexRotation(v1); h.r2 = HexRotation(v2); h.r3 = HexRotation(v3);
    float2 c1 = HexCenter(v1) / HEX_CELL, c2 = HexCenter(v2) / HEX_CELL, c3 = HexCenter(v3) / HEX_CELL;
    h.st1 = mul(st - c1, h.r1) + c1 + HexHash(float2(v1));
    h.st2 = mul(st - c2, h.r2) + c2 + HexHash(float2(v2));
    h.st3 = mul(st - c3, h.r3) + c3 + HexHash(float2(v3));
    return h;
}

// Diffuse of one layer, hex-tiled ; W returns the final blend weights, reused for the normal map
half4 HexDiffuse(sampler2D tex, HexCells h, out float3 W)
{
    half4 s1 = tex2Dgrad(tex, h.st1, mul(h.dx, h.r1), mul(h.dy, h.r1));
    half4 s2 = tex2Dgrad(tex, h.st2, mul(h.dx, h.r2), mul(h.dy, h.r2));
    half4 s3 = tex2Dgrad(tex, h.st3, mul(h.dx, h.r3), mul(h.dy, h.r3));
    const float3 L = float3(0.299, 0.587, 0.114);
    float3 lum = float3(dot(s1.rgb, L), dot(s2.rgb, L), dot(s3.rgb, L));
    W = lerp(1.0, lum, HEX_CONTRAST) * pow(h.w, HEX_EXPONENT);
    W /= (W.x + W.y + W.z);
    return W.x * s1 + W.y * s2 + W.z * s3;
}

#ifdef _NORMALMAP
// Normal map of one layer, hex-tiled : each sample is rotated back by the rotation of its cell
fixed3 HexNormal(sampler2D tex, float scale, HexCells h, float3 W)
{
    fixed3 n1 = UnpackNormalWithScale(tex2Dgrad(tex, h.st1, mul(h.dx, h.r1), mul(h.dy, h.r1)), scale);
    fixed3 n2 = UnpackNormalWithScale(tex2Dgrad(tex, h.st2, mul(h.dx, h.r2), mul(h.dy, h.r2)), scale);
    fixed3 n3 = UnpackNormalWithScale(tex2Dgrad(tex, h.st3, mul(h.dx, h.r3), mul(h.dy, h.r3)), scale);
    n1.xy = mul(h.r1, n1.xy); n2.xy = mul(h.r2, n2.xy); n3.xy = mul(h.r3, n3.xy);
    return W.x * n1 + W.y * n2 + W.z * n3;
}
#endif

// Triplanar projection : on the slopes the layers are also projected from the sides (planes ZY and XY), so they are not
// stretched on the cliffs. The three projections (each hex-tiled) are blended by the orientation of the surface. A
// projection is only sampled where its weight is not null (flat ground : the top one only).
#define TRIPLANAR_SHARPNESS 4.0     // higher = narrower transitions between the projections

struct Triplanar
{
    HexCells top, sideX, sideZ;     // planes XZ (uv = x, z), ZY (uv = z, y) and XY (uv = x, y)
    float3 w;                       // weights of sideX, top, sideZ
};

// Surface normal of the terrain in world space (the terrain is never rotated)
float3 TerrainGeomNormal(Input IN)
{
    #if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
        #if defined(TERRAIN_USE_SEPARATE_VERTEX_SAMPLER)
            return normalize(_TerrainNormalmapTexture.Sample(sampler__TerrainNormalmapTexture, IN.tc.zw).xyz * 2 - 1);
        #else
            return normalize(tex2D(_TerrainNormalmapTexture, IN.tc.zw).xyz * 2 - 1);
        #endif
    #else
        return float3(0, 1, 0);     // per-vertex normals : top projection only, as before
    #endif
}

Triplanar TriplanarSetup(Input IN, float4 splatST, float3 normal)
{
    Triplanar t;
    t.top = HexSetup(WorldSplatUV(IN, splatST));
    // Tiles per meter of the layer (see WorldSplatUV) ; the derivatives are taken here, outside the branches
    float2 scale = _WorldUVTerrainSize.x > 0.0f && _WorldUVTerrainSize.y > 0.0f ? splatST.xy / _WorldUVTerrainSize.xy : 0.0f;
    t.sideX = HexSetup(float2(IN.worldXZ.y, IN.worldY) * scale + splatST.zw);
    t.sideZ = HexSetup(float2(IN.worldXZ.x, IN.worldY) * scale + splatST.zw);
    float3 w = pow(abs(normal), TRIPLANAR_SHARPNESS);
    t.w = scale.x > 0.0f ? w / (w.x + w.y + w.z) : float3(0, 1, 0);
    return t;
}

// Diffuse of one layer ; Wx, Wy, Wz return the hex blend weights of each projection, reused for the normal map
half4 TriplanarDiffuse(sampler2D tex, Triplanar t, out float3 Wx, out float3 Wy, out float3 Wz)
{
    half4 c = 0;
    Wx = 0; Wy = 0; Wz = 0;
    [branch] if (t.w.y > 0.001) c += t.w.y * HexDiffuse(tex, t.top, Wy);
    [branch] if (t.w.x > 0.001) c += t.w.x * HexDiffuse(tex, t.sideX, Wx);
    [branch] if (t.w.z > 0.001) c += t.w.z * HexDiffuse(tex, t.sideZ, Wz);
    return c;
}

#ifdef _NORMALMAP
// Normal map of one layer, as a world space perturbation : each projection bends the normal along its own uv axes
float3 TriplanarNormal(sampler2D tex, float scale, Triplanar t, float3 Wx, float3 Wy, float3 Wz)
{
    float3 p = 0;
    [branch] if (t.w.y > 0.001)
    {
        fixed3 n = HexNormal(tex, scale, t.top, Wy);
        p += t.w.y * float3(n.x, 0, n.y);
    }
    [branch] if (t.w.x > 0.001)
    {
        fixed3 n = HexNormal(tex, scale, t.sideX, Wx);
        p += t.w.x * float3(0, n.y, n.x);
    }
    [branch] if (t.w.z > 0.001)
    {
        fixed3 n = HexNormal(tex, scale, t.sideZ, Wz);
        p += t.w.z * float3(n.x, n.y, 0);
    }
    return p;
}
#endif

#ifdef TERRAIN_STANDARD_SHADER
void SplatmapMix(Input IN, half4 defaultAlpha, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal)
#else
void SplatmapMix(Input IN, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal)
#endif
{
    #ifdef _ALPHATEST_ON
        ClipHoles(IN.tc.xy);
    #endif

    ClipTerrainMasks(float3(IN.worldXZ.x, IN.worldY, IN.worldXZ.y));

    // adjust splatUVs so the edges of the terrain tile lie on pixel centers
    float2 splatUV = (IN.tc.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
    splat_control = tex2D(_Control, splatUV);
    weight = dot(splat_control, half4(1,1,1,1));

    #if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
        clip(weight == 0.0f ? -1 : 1);
    #endif

    // Normalize weights before lighting and restore weights in final modifier functions so that the overal
    // lighting result can be correctly weighted.
    splat_control /= (weight + 1e-3f);

    // World UV, triplanar on the slopes, every projection hex-tiled (see TriplanarSetup, HexSetup)
    float3 geomNormal = TerrainGeomNormal(IN);
    Triplanar t0 = TriplanarSetup(IN, _Splat0_ST, geomNormal);
    Triplanar t1 = TriplanarSetup(IN, _Splat1_ST, geomNormal);
    Triplanar t2 = TriplanarSetup(IN, _Splat2_ST, geomNormal);
    Triplanar t3 = TriplanarSetup(IN, _Splat3_ST, geomNormal);
    // The layers absent at this pixel are not sampled
    float3 Wx0 = 0, Wy0 = 0, Wz0 = 0, Wx1 = 0, Wy1 = 0, Wz1 = 0, Wx2 = 0, Wy2 = 0, Wz2 = 0, Wx3 = 0, Wy3 = 0, Wz3 = 0;
    half4 d0 = 0, d1 = 0, d2 = 0, d3 = 0;
    [branch] if (splat_control.r > 0.001) d0 = TriplanarDiffuse(_Splat0, t0, Wx0, Wy0, Wz0);
    [branch] if (splat_control.g > 0.001) d1 = TriplanarDiffuse(_Splat1, t1, Wx1, Wy1, Wz1);
    [branch] if (splat_control.b > 0.001) d2 = TriplanarDiffuse(_Splat2, t2, Wx2, Wy2, Wz2);
    [branch] if (splat_control.a > 0.001) d3 = TriplanarDiffuse(_Splat3, t3, Wx3, Wy3, Wz3);

    mixedDiffuse = 0.0f;
    #ifdef TERRAIN_STANDARD_SHADER
        mixedDiffuse += splat_control.r * d0 * half4(1.0, 1.0, 1.0, defaultAlpha.r);
        mixedDiffuse += splat_control.g * d1 * half4(1.0, 1.0, 1.0, defaultAlpha.g);
        mixedDiffuse += splat_control.b * d2 * half4(1.0, 1.0, 1.0, defaultAlpha.b);
        mixedDiffuse += splat_control.a * d3 * half4(1.0, 1.0, 1.0, defaultAlpha.a);
    #else
        mixedDiffuse += splat_control.r * d0;
        mixedDiffuse += splat_control.g * d1;
        mixedDiffuse += splat_control.b * d2;
        mixedDiffuse += splat_control.a * d3;
    #endif

    #ifdef _NORMALMAP
        // Perturbation in world space, added to the surface normal, then expressed in the tangent frame of the surface
        // (tangent along +X, bitangent along +Z on flat ground : the frame rebuilt below from geomNormal)
        float3 perturbation = 0;
        [branch] if (splat_control.r > 0.001) perturbation += TriplanarNormal(_Normal0, _NormalScale0, t0, Wx0, Wy0, Wz0) * splat_control.r;
        [branch] if (splat_control.g > 0.001) perturbation += TriplanarNormal(_Normal1, _NormalScale1, t1, Wx1, Wy1, Wz1) * splat_control.g;
        [branch] if (splat_control.b > 0.001) perturbation += TriplanarNormal(_Normal2, _NormalScale2, t2, Wx2, Wy2, Wz2) * splat_control.b;
        [branch] if (splat_control.a > 0.001) perturbation += TriplanarNormal(_Normal3, _NormalScale3, t3, Wx3, Wy3, Wz3) * splat_control.a;
        float3 worldNormal = normalize(geomNormal + perturbation);
        float3 frameTangent = normalize(cross(geomNormal, float3(0, 0, 1)));
        float3 frameBitangent = normalize(cross(frameTangent, geomNormal));
        mixedNormal = float3(dot(worldNormal, frameTangent), dot(worldNormal, frameBitangent), dot(worldNormal, geomNormal));
#if defined(SHADER_API_SWITCH) || defined(SHADER_API_SWITCH2)
        mixedNormal.z += UNITY_HALF_MIN; // to avoid nan after normalizing
#else
        mixedNormal.z += 1e-5f; // to avoid nan after normalizing
#endif
    #endif

    #if defined(INSTANCING_ON) && defined(SHADER_TARGET_SURFACE_ANALYSIS) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
        mixedNormal = float3(0, 0, 1); // make sure that surface shader compiler realizes we write to normal, as UNITY_INSTANCING_ENABLED is not defined for SHADER_TARGET_SURFACE_ANALYSIS.
    #endif

    #if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)

        #ifdef _NORMALMAP
            float3 geomTangent = normalize(cross(geomNormal, float3(0, 0, 1)));
            float3 geomBitangent = normalize(cross(geomTangent, geomNormal));
            mixedNormal = mixedNormal.x * geomTangent
                          + mixedNormal.y * geomBitangent
                          + mixedNormal.z * geomNormal;
        #else
            mixedNormal = geomNormal;
        #endif
        mixedNormal = mixedNormal.xzy;
    #endif
}

#ifndef TERRAIN_SURFACE_OUTPUT
    #define TERRAIN_SURFACE_OUTPUT SurfaceOutput
#endif

void SplatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
{
    color *= o.Alpha;
    #ifdef TERRAIN_SPLAT_ADDPASS
        UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
    #else
        UNITY_APPLY_FOG(IN.fogCoord, color);
    #endif
}

void SplatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT o, inout half4 outGBuffer0, inout half4 outGBuffer1, inout half4 outGBuffer2, inout half4 emission)
{
    UnityStandardDataApplyWeightToGbuffer(outGBuffer0, outGBuffer1, outGBuffer2, o.Alpha);
    emission *= o.Alpha;
}

#endif // TERRAIN_BASE_PASS

#endif // TERRAIN_WORLD_UV_SPLATMAP_COMMON_CGINC_INCLUDED
