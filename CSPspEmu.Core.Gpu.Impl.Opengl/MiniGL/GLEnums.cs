﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniGL
{
	public enum AlphaFunction
	{
		Never = 512,
		Less = 513,
		Equal = 514,
		Lequal = 515,
		Greater = 516,
		Notequal = 517,
		Gequal = 518,
		Always = 519,
	}

	public enum LightName
	{
		Light0 = 16384,
		Light1 = 16385,
		Light2 = 16386,
		Light3 = 16387,
		Light4 = 16388,
		Light5 = 16389,
		Light6 = 16390,
		Light7 = 16391,
		FragmentLight0Sgix = 33804,
		FragmentLight1Sgix = 33805,
		FragmentLight2Sgix = 33806,
		FragmentLight3Sgix = 33807,
		FragmentLight4Sgix = 33808,
		FragmentLight5Sgix = 33809,
		FragmentLight6Sgix = 33810,
		FragmentLight7Sgix = 33811,
	}

	public enum ShadingModel
	{
		Flat = 7424,
		Smooth = 7425,
	}

	public enum TextureWrapMode
	{
		Clamp = 10496,
		Repeat = 10497,
		ClampToBorder = 33069,
		ClampToEdge = 33071,
		MirroredRepeat = 33648,
	}

	public enum PixelFormat
	{
		ColorIndex = 6400,
		StencilIndex = 6401,
		DepthComponent = 6402,
		Red = 6403,
		Green = 6404,
		Blue = 6405,
		Alpha = 6406,
		Rgb = 6407,
		Rgba = 6408,
		Luminance = 6409,
		LuminanceAlpha = 6410,
		AbgrExt = 32768,
		CmykExt = 32780,
		CmykaExt = 32781,
		Bgr = 32992,
		Bgra = 32993,
		Ycrcb422Sgix = 33211,
		Ycrcb444Sgix = 33212,
		Rg = 33319,
		RgInteger = 33320,
		DepthStencil = 34041,
		RedInteger = 36244,
		GreenInteger = 36245,
		BlueInteger = 36246,
		AlphaInteger = 36247,
		RgbInteger = 36248,
		RgbaInteger = 36249,
		BgrInteger = 36250,
		BgraInteger = 36251,
	}

	public enum PixelInternalFormat
	{
		One = 1,
		Two = 2,
		Three = 3,
		Four = 4,
		DepthComponent = 6402,
		Alpha = 6406,
		Rgb = 6407,
		Rgba = 6408,
		Luminance = 6409,
		LuminanceAlpha = 6410,
		R3G3B2 = 10768,
		Alpha4 = 32827,
		Alpha8 = 32828,
		Alpha12 = 32829,
		Alpha16 = 32830,
		Luminance4 = 32831,
		Luminance8 = 32832,
		Luminance12 = 32833,
		Luminance16 = 32834,
		Luminance4Alpha4 = 32835,
		Luminance6Alpha2 = 32836,
		Luminance8Alpha8 = 32837,
		Luminance12Alpha4 = 32838,
		Luminance12Alpha12 = 32839,
		Luminance16Alpha16 = 32840,
		Intensity = 32841,
		Intensity4 = 32842,
		Intensity8 = 32843,
		Intensity12 = 32844,
		Intensity16 = 32845,
		Rgb2Ext = 32846,
		Rgb4 = 32847,
		Rgb5 = 32848,
		Rgb8 = 32849,
		Rgb10 = 32850,
		Rgb12 = 32851,
		Rgb16 = 32852,
		Rgba2 = 32853,
		Rgba4 = 32854,
		Rgb5A1 = 32855,
		Rgba8 = 32856,
		Rgb10A2 = 32857,
		Rgba12 = 32858,
		Rgba16 = 32859,
		DualAlpha4Sgis = 33040,
		DualAlpha8Sgis = 33041,
		DualAlpha12Sgis = 33042,
		DualAlpha16Sgis = 33043,
		DualLuminance4Sgis = 33044,
		DualLuminance8Sgis = 33045,
		DualLuminance12Sgis = 33046,
		DualLuminance16Sgis = 33047,
		DualIntensity4Sgis = 33048,
		DualIntensity8Sgis = 33049,
		DualIntensity12Sgis = 33050,
		DualIntensity16Sgis = 33051,
		DualLuminanceAlpha4Sgis = 33052,
		DualLuminanceAlpha8Sgis = 33053,
		QuadAlpha4Sgis = 33054,
		QuadAlpha8Sgis = 33055,
		QuadLuminance4Sgis = 33056,
		QuadLuminance8Sgis = 33057,
		QuadIntensity4Sgis = 33058,
		QuadIntensity8Sgis = 33059,
		DepthComponent16 = 33189,
		DepthComponent16Sgix = 33189,
		DepthComponent24Sgix = 33190,
		DepthComponent24 = 33190,
		DepthComponent32Sgix = 33191,
		DepthComponent32 = 33191,
		CompressedRed = 33317,
		CompressedRg = 33318,
		R8 = 33321,
		R16 = 33322,
		Rg8 = 33323,
		Rg16 = 33324,
		R16f = 33325,
		R32f = 33326,
		Rg16f = 33327,
		Rg32f = 33328,
		R8i = 33329,
		R8ui = 33330,
		R16i = 33331,
		R16ui = 33332,
		R32i = 33333,
		R32ui = 33334,
		Rg8i = 33335,
		Rg8ui = 33336,
		Rg16i = 33337,
		Rg16ui = 33338,
		Rg32i = 33339,
		Rg32ui = 33340,
		CompressedRgbS3tcDxt1Ext = 33776,
		CompressedRgbaS3tcDxt1Ext = 33777,
		CompressedRgbaS3tcDxt3Ext = 33778,
		CompressedRgbaS3tcDxt5Ext = 33779,
		CompressedAlpha = 34025,
		CompressedLuminance = 34026,
		CompressedLuminanceAlpha = 34027,
		CompressedIntensity = 34028,
		CompressedRgb = 34029,
		CompressedRgba = 34030,
		DepthStencil = 34041,
		Rgba32f = 34836,
		Rgb32f = 34837,
		Rgba16f = 34842,
		Rgb16f = 34843,
		Depth24Stencil8 = 35056,
		R11fG11fB10f = 35898,
		Rgb9E5 = 35901,
		Srgb = 35904,
		Srgb8 = 35905,
		SrgbAlpha = 35906,
		Srgb8Alpha8 = 35907,
		SluminanceAlpha = 35908,
		Sluminance8Alpha8 = 35909,
		Sluminance = 35910,
		Sluminance8 = 35911,
		CompressedSrgb = 35912,
		CompressedSrgbAlpha = 35913,
		CompressedSluminance = 35914,
		CompressedSluminanceAlpha = 35915,
		CompressedSrgbS3tcDxt1Ext = 35916,
		CompressedSrgbAlphaS3tcDxt1Ext = 35917,
		CompressedSrgbAlphaS3tcDxt3Ext = 35918,
		CompressedSrgbAlphaS3tcDxt5Ext = 35919,
		DepthComponent32f = 36012,
		Depth32fStencil8 = 36013,
		Rgba32ui = 36208,
		Rgb32ui = 36209,
		Rgba16ui = 36214,
		Rgb16ui = 36215,
		Rgba8ui = 36220,
		Rgb8ui = 36221,
		Rgba32i = 36226,
		Rgb32i = 36227,
		Rgba16i = 36232,
		Rgb16i = 36233,
		Rgba8i = 36238,
		Rgb8i = 36239,
		Float32UnsignedInt248Rev = 36269,
		CompressedRedRgtc1 = 36283,
		CompressedSignedRedRgtc1 = 36284,
		CompressedRgRgtc2 = 36285,
		CompressedSignedRgRgtc2 = 36286,
	}

	public enum TextureEnvParameter
	{
		AlphaScale = 3356,
		TextureEnvMode = 8704,
		TextureEnvColor = 8705,
		TextureLodBias = 34049,
		CombineRgb = 34161,
		CombineAlpha = 34162,
		RgbScale = 34163,
		Source0Rgb = 34176,
		Src1Rgb = 34177,
		Src2Rgb = 34178,
		Src0Alpha = 34184,
		Src1Alpha = 34185,
		Src2Alpha = 34186,
		Operand0Rgb = 34192,
		Operand1Rgb = 34193,
		Operand2Rgb = 34194,
		Operand0Alpha = 34200,
		Operand1Alpha = 34201,
		Operand2Alpha = 34202,
		CoordReplace = 34914,
	}

	public enum TextureMinFilter
	{
		Nearest = 9728,
		Linear = 9729,
		NearestMipmapNearest = 9984,
		LinearMipmapNearest = 9985,
		NearestMipmapLinear = 9986,
		LinearMipmapLinear = 9987,
		Filter4Sgis = 33094,
		LinearClipmapLinearSgix = 33136,
		PixelTexGenQCeilingSgix = 33156,
		PixelTexGenQRoundSgix = 33157,
		PixelTexGenQFloorSgix = 33158,
		NearestClipmapNearestSgix = 33869,
		NearestClipmapLinearSgix = 33870,
		LinearClipmapNearestSgix = 33871,
	}

	public enum TextureMagFilter
	{
		Nearest = 9728,
		Linear = 9729,
		LinearDetailSgis = 32919,
		LinearDetailAlphaSgis = 32920,
		LinearDetailColorSgis = 32921,
		LinearSharpenSgis = 32941,
		LinearSharpenAlphaSgis = 32942,
		LinearSharpenColorSgis = 32943,
		Filter4Sgis = 33094,
		PixelTexGenQCeilingSgix = 33156,
		PixelTexGenQRoundSgix = 33157,
		PixelTexGenQFloorSgix = 33158,
	}

	public enum LightModelColorControl
	{
		SingleColor = 33273,
		SeparateSpecularColor = 33274,
	}

	public enum LightModelParameter
	{
		LightModelLocalViewer = 2897,
		LightModelTwoSide = 2898,
		LightModelAmbient = 2899,
		LightModelColorControl = 33272,
	}

	public enum ErrorCode
	{
		NoError = 0,
		InvalidEnum = 1280,
		InvalidValue = 1281,
		InvalidOperation = 1282,
		StackOverflow = 1283,
		StackUnderflow = 1284,
		OutOfMemory = 1285,
		InvalidFramebufferOperationExt = 1286,
		InvalidFramebufferOperation = 1286,
		TableTooLargeExt = 32817,
		TextureTooLargeExt = 32869,
	}

	public enum MaterialFace
	{
		Front = 1028,
		Back = 1029,
		FrontAndBack = 1032,
	}

	public enum CullFaceMode
	{
		Front = 1028,
		Back = 1029,
		FrontAndBack = 1032,
	}

	public enum ColorMaterialParameter
	{
		Ambient = 4608,
		Diffuse = 4609,
		Specular = 4610,
		Emission = 5632,
		AmbientAndDiffuse = 5634,
	}

	public enum MatrixMode
	{
		Modelview = 5888,
		Projection = 5889,
		Texture = 5890,
		Color = 6144,
	}

	public enum TextureEnvTarget
	{
		TextureEnv = 8960,
		TextureFilterControl = 34048,
		PointSprite = 34913,
	}

	public enum MaterialParameter
	{
		Ambient = 4608,
		Diffuse = 4609,
		Specular = 4610,
		Emission = 5632,
		Shininess = 5633,
		AmbientAndDiffuse = 5634,
		ColorIndexes = 5635,
	}

	public enum TextureParameterName
	{
		TextureBorderColor = 4100,
		Red = 6403,
		TextureMagFilter = 10240,
		TextureMinFilter = 10241,
		TextureWrapS = 10242,
		TextureWrapT = 10243,
		TexturePriority = 32870,
		TextureDepth = 32881,
		TextureWrapRExt = 32882,
		TextureWrapR = 32882,
		DetailTextureLevelSgis = 32922,
		DetailTextureModeSgis = 32923,
		ShadowAmbientSgix = 32959,
		TextureCompareFailValue = 32959,
		DualTextureSelectSgis = 33060,
		QuadTextureSelectSgis = 33061,
		ClampToBorder = 33069,
		ClampToEdge = 33071,
		TextureWrapQSgis = 33079,
		TextureMinLod = 33082,
		TextureMaxLod = 33083,
		TextureBaseLevel = 33084,
		TextureMaxLevel = 33085,
		TextureClipmapCenterSgix = 33137,
		TextureClipmapFrameSgix = 33138,
		TextureClipmapOffsetSgix = 33139,
		TextureClipmapVirtualDepthSgix = 33140,
		TextureClipmapLodOffsetSgix = 33141,
		TextureClipmapDepthSgix = 33142,
		PostTextureFilterBiasSgix = 33145,
		PostTextureFilterScaleSgix = 33146,
		TextureLodBiasSSgix = 33166,
		TextureLodBiasTSgix = 33167,
		TextureLodBiasRSgix = 33168,
		GenerateMipmapSgis = 33169,
		GenerateMipmap = 33169,
		TextureCompareSgix = 33178,
		TextureCompareOperatorSgix = 33179,
		TextureMaxClampSSgix = 33641,
		TextureMaxClampTSgix = 33642,
		TextureMaxClampRSgix = 33643,
		TextureLodBias = 34049,
		DepthTextureMode = 34891,
		TextureCompareMode = 34892,
		TextureCompareFunc = 34893,
	}

	public enum LightParameter
	{
		Ambient = 4608,
		Diffuse = 4609,
		Specular = 4610,
		Position = 4611,
		SpotDirection = 4612,
		SpotExponent = 4613,
		SpotCutoff = 4614,
		ConstantAttenuation = 4615,
		LinearAttenuation = 4616,
		QuadraticAttenuation = 4617,
	}

	public enum TextureTarget
	{
		Texture1D = 3552,
		Texture2D = 3553,
		ProxyTexture1D = 32867,
		ProxyTexture2D = 32868,
		Texture3D = 32879,
		ProxyTexture3D = 32880,
		DetailTexture2DSgis = 32917,
		Texture4DSgis = 33076,
		ProxyTexture4DSgis = 33077,
		TextureMinLod = 33082,
		TextureMaxLod = 33083,
		TextureBaseLevel = 33084,
		TextureMaxLevel = 33085,
		TextureRectangleArb = 34037,
		TextureRectangle = 34037,
		TextureRectangleNv = 34037,
		ProxyTextureRectangle = 34039,
		TextureCubeMap = 34067,
		TextureBindingCubeMap = 34068,
		TextureCubeMapPositiveX = 34069,
		TextureCubeMapNegativeX = 34070,
		TextureCubeMapPositiveY = 34071,
		TextureCubeMapNegativeY = 34072,
		TextureCubeMapPositiveZ = 34073,
		TextureCubeMapNegativeZ = 34074,
		ProxyTextureCubeMap = 34075,
		Texture1DArray = 35864,
		ProxyTexture1DArray = 35865,
		Texture2DArray = 35866,
		ProxyTexture2DArray = 35867,
		TextureBuffer = 35882,
		Texture2DMultisample = 37120,
		ProxyTexture2DMultisample = 37121,
		Texture2DMultisampleArray = 37122,
		ProxyTexture2DMultisampleArray = 37123,
	}

	public enum TextureUnit
	{
		Texture0 = 33984,
		Texture1 = 33985,
		Texture2 = 33986,
		Texture3 = 33987,
		Texture4 = 33988,
		Texture5 = 33989,
		Texture6 = 33990,
		Texture7 = 33991,
		Texture8 = 33992,
		Texture9 = 33993,
		Texture10 = 33994,
		Texture11 = 33995,
		Texture12 = 33996,
		Texture13 = 33997,
		Texture14 = 33998,
		Texture15 = 33999,
		Texture16 = 34000,
		Texture17 = 34001,
		Texture18 = 34002,
		Texture19 = 34003,
		Texture20 = 34004,
		Texture21 = 34005,
		Texture22 = 34006,
		Texture23 = 34007,
		Texture24 = 34008,
		Texture25 = 34009,
		Texture26 = 34010,
		Texture27 = 34011,
		Texture28 = 34012,
		Texture29 = 34013,
		Texture30 = 34014,
		Texture31 = 34015,
	}
	public enum StencilOp
	{
		Zero = 0,
		Invert = 5386,
		Keep = 7680,
		Replace = 7681,
		Incr = 7682,
		Decr = 7683,
		IncrWrap = 34055,
		DecrWrap = 34056,
	}

	public enum BlendingFactorSrc
	{
		Zero = 0,
		One = 1,
		SrcAlpha = 770,
		OneMinusSrcAlpha = 771,
		DstAlpha = 772,
		OneMinusDstAlpha = 773,
		DstColor = 774,
		OneMinusDstColor = 775,
		SrcAlphaSaturate = 776,
		ConstantColorExt = 32769,
		ConstantColor = 32769,
		OneMinusConstantColor = 32770,
		OneMinusConstantColorExt = 32770,
		ConstantAlphaExt = 32771,
		ConstantAlpha = 32771,
		OneMinusConstantAlphaExt = 32772,
		OneMinusConstantAlpha = 32772,
	}

	public enum PixelType
	{
		Byte = 5120,
		UnsignedByte = 5121,
		Short = 5122,
		UnsignedShort = 5123,
		Int = 5124,
		UnsignedInt = 5125,
		Float = 5126,
		HalfFloat = 5131,
		Bitmap = 6656,
		UnsignedByte332Ext = 32818,
		UnsignedByte332 = 32818,
		UnsignedShort4444Ext = 32819,
		UnsignedShort4444 = 32819,
		UnsignedShort5551 = 32820,
		UnsignedShort5551Ext = 32820,
		UnsignedInt8888 = 32821,
		UnsignedInt8888Ext = 32821,
		UnsignedInt1010102 = 32822,
		UnsignedInt1010102Ext = 32822,
		UnsignedByte233Reversed = 33634,
		UnsignedShort565 = 33635,
		UnsignedShort565Reversed = 33636,
		UnsignedShort4444Reversed = 33637,
		UnsignedShort1555Reversed = 33638,
		UnsignedInt8888Reversed = 33639,
		UnsignedInt2101010Reversed = 33640,
		UnsignedInt248 = 34042,
		UnsignedInt10F11F11FRev = 35899,
		UnsignedInt5999Rev = 35902,
		Float32UnsignedInt248Rev = 36269,
	}

	public enum TextureEnvMode
	{
		Add = 260,
		Blend = 3042,
		Replace = 7681,
		Modulate = 8448,
		Decal = 8449,
		ReplaceExt = 32866,
		TextureEnvBiasSgix = 32958,
		Combine = 34160,
	}

	public enum BlendingFactorDest
	{
		Zero = 0,
		One = 1,
		SrcColor = 768,
		OneMinusSrcColor = 769,
		SrcAlpha = 770,
		OneMinusSrcAlpha = 771,
		DstAlpha = 772,
		OneMinusDstAlpha = 773,
		DstColor = 774,
		OneMinusDstColor = 775,
		ConstantColorExt = 32769,
		ConstantColor = 32769,
		OneMinusConstantColor = 32770,
		OneMinusConstantColorExt = 32770,
		ConstantAlphaExt = 32771,
		ConstantAlpha = 32771,
		OneMinusConstantAlpha = 32772,
		OneMinusConstantAlphaExt = 32772,
	}

	public enum StencilFunction
	{
		Never = 512,
		Less = 513,
		Equal = 514,
		Lequal = 515,
		Greater = 516,
		Notequal = 517,
		Gequal = 518,
		Always = 519,
	}

	public enum DepthFunction
	{
		Never = 512,
		Less = 513,
		Equal = 514,
		Lequal = 515,
		Greater = 516,
		Notequal = 517,
		Gequal = 518,
		Always = 519,
	}

	public enum BlendEquationMode
	{
		FuncAdd = 32774,
		Min = 32775,
		Max = 32776,
		FuncSubtract = 32778,
		FuncReverseSubtract = 32779,
	}

	public enum PixelStoreParameter
	{
		UnpackSwapBytes = 3312,
		UnpackLsbFirst = 3313,
		UnpackRowLength = 3314,
		UnpackSkipRows = 3315,
		UnpackSkipPixels = 3316,
		UnpackAlignment = 3317,
		PackSwapBytes = 3328,
		PackLsbFirst = 3329,
		PackRowLength = 3330,
		PackSkipRows = 3331,
		PackSkipPixels = 3332,
		PackAlignment = 3333,
		PackSkipImagesExt = 32875,
		PackSkipImages = 32875,
		PackImageHeight = 32876,
		PackImageHeightExt = 32876,
		UnpackSkipImagesExt = 32877,
		UnpackSkipImages = 32877,
		UnpackImageHeight = 32878,
		UnpackImageHeightExt = 32878,
		PackSkipVolumesSgis = 33072,
		PackImageDepthSgis = 33073,
		UnpackSkipVolumesSgis = 33074,
		UnpackImageDepthSgis = 33075,
		PixelTileWidthSgix = 33088,
		PixelTileHeightSgix = 33089,
		PixelTileGridWidthSgix = 33090,
		PixelTileGridHeightSgix = 33091,
		PixelTileGridDepthSgix = 33092,
		PixelTileCacheSizeSgix = 33093,
		PackResampleSgix = 33836,
		UnpackResampleSgix = 33837,
		PackSubsampleRateSgix = 34208,
		UnpackSubsampleRateSgix = 34209,
	}

	public enum EnableCap
	{
		PointSmooth = 2832,
		LineSmooth = 2848,
		LineStipple = 2852,
		PolygonSmooth = 2881,
		PolygonStipple = 2882,
		CullFace = 2884,
		Lighting = 2896,
		ColorMaterial = 2903,
		Fog = 2912,
		DepthTest = 2929,
		StencilTest = 2960,
		Normalize = 2977,
		AlphaTest = 3008,
		Dither = 3024,
		Blend = 3042,
		IndexLogicOp = 3057,
		ColorLogicOp = 3058,
		ScissorTest = 3089,
		TextureGenS = 3168,
		TextureGenT = 3169,
		TextureGenR = 3170,
		TextureGenQ = 3171,
		AutoNormal = 3456,
		Map1Color4 = 3472,
		Map1Index = 3473,
		Map1Normal = 3474,
		Map1TextureCoord1 = 3475,
		Map1TextureCoord2 = 3476,
		Map1TextureCoord3 = 3477,
		Map1TextureCoord4 = 3478,
		Map1Vertex3 = 3479,
		Map1Vertex4 = 3480,
		Map2Color4 = 3504,
		Map2Index = 3505,
		Map2Normal = 3506,
		Map2TextureCoord1 = 3507,
		Map2TextureCoord2 = 3508,
		Map2TextureCoord3 = 3509,
		Map2TextureCoord4 = 3510,
		Map2Vertex3 = 3511,
		Map2Vertex4 = 3512,
		Texture1D = 3552,
		Texture2D = 3553,
		PolygonOffsetPoint = 10753,
		PolygonOffsetLine = 10754,
		ClipPlane0 = 12288,
		ClipPlane1 = 12289,
		ClipPlane2 = 12290,
		ClipPlane3 = 12291,
		ClipPlane4 = 12292,
		ClipPlane5 = 12293,
		Light0 = 16384,
		Light1 = 16385,
		Light2 = 16386,
		Light3 = 16387,
		Light4 = 16388,
		Light5 = 16389,
		Light6 = 16390,
		Light7 = 16391,
		Convolution1DExt = 32784,
		Convolution1D = 32784,
		Convolution2D = 32785,
		Convolution2DExt = 32785,
		Separable2D = 32786,
		Separable2DExt = 32786,
		HistogramExt = 32804,
		Histogram = 32804,
		MinmaxExt = 32814,
		PolygonOffsetFill = 32823,
		RescaleNormal = 32826,
		RescaleNormalExt = 32826,
		Texture3DExt = 32879,
		VertexArray = 32884,
		NormalArray = 32885,
		ColorArray = 32886,
		IndexArray = 32887,
		TextureCoordArray = 32888,
		EdgeFlagArray = 32889,
		InterlaceSgix = 32916,
		Multisample = 32925,
		SampleAlphaToMaskSgis = 32926,
		SampleAlphaToCoverage = 32926,
		SampleAlphaToOne = 32927,
		SampleAlphaToOneSgis = 32927,
		SampleCoverage = 32928,
		SampleMaskSgis = 32928,
		TextureColorTableSgi = 32956,
		ColorTableSgi = 32976,
		ColorTable = 32976,
		PostConvolutionColorTable = 32977,
		PostConvolutionColorTableSgi = 32977,
		PostColorMatrixColorTableSgi = 32978,
		PostColorMatrixColorTable = 32978,
		Texture4DSgis = 33076,
		PixelTexGenSgix = 33081,
		SpriteSgix = 33096,
		ReferencePlaneSgix = 33149,
		IrInstrument1Sgix = 33151,
		CalligraphicFragmentSgix = 33155,
		FramezoomSgix = 33163,
		FogOffsetSgix = 33176,
		SharedTexturePaletteExt = 33275,
		AsyncHistogramSgix = 33580,
		PixelTextureSgis = 33619,
		AsyncTexImageSgix = 33628,
		AsyncDrawPixelsSgix = 33629,
		AsyncReadPixelsSgix = 33630,
		FragmentLightingSgix = 33792,
		FragmentColorMaterialSgix = 33793,
		FragmentLight0Sgix = 33804,
		FragmentLight1Sgix = 33805,
		FragmentLight2Sgix = 33806,
		FragmentLight3Sgix = 33807,
		FragmentLight4Sgix = 33808,
		FragmentLight5Sgix = 33809,
		FragmentLight6Sgix = 33810,
		FragmentLight7Sgix = 33811,
		FogCoordArray = 33879,
		ColorSum = 33880,
		SecondaryColorArray = 33886,
		TextureCubeMap = 34067,
		ProgramPointSize = 34370,
		VertexProgramPointSize = 34370,
		VertexProgramTwoSide = 34371,
		DepthClamp = 34383,
		TextureCubeMapSeamless = 34895,
		PointSprite = 34913,
		RasterizerDiscard = 35977,
		FramebufferSrgb = 36281,
		SampleMask = 36433,
		PrimitiveRestart = 36765,
	}

	public enum HintTarget
	{
		PerspectiveCorrectionHint = 3152,
		PointSmoothHint = 3153,
		LineSmoothHint = 3154,
		PolygonSmoothHint = 3155,
		FogHint = 3156,
		PackCmykHintExt = 32782,
		UnpackCmykHintExt = 32783,
		TextureMultiBufferHintSgix = 33070,
		GenerateMipmapHintSgis = 33170,
		GenerateMipmapHint = 33170,
		ConvolutionHintSgix = 33558,
		VertexPreclipHintSgix = 33775,
		TextureCompressionHint = 34031,
		FragmentShaderDerivativeHint = 35723,
	}

	public enum HintMode
	{
		DontCare = 4352,
		Fastest = 4353,
		Nicest = 4354,
	}

	public enum BeginMode
	{
		Points = 0,
		Lines = 1,
		LineLoop = 2,
		LineStrip = 3,
		Triangles = 4,
		TriangleStrip = 5,
		TriangleFan = 6,
		Quads = 7,
		QuadStrip = 8,
		Polygon = 9,
		LinesAdjacency = 10,
		LineStripAdjacency = 11,
		TrianglesAdjacency = 12,
		TriangleStripAdjacency = 13,
	}

}