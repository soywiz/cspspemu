﻿//#define DEBUG_TEXTURE_CACHE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Utils;
using OpenTK.Graphics.OpenGL;
using CSharpUtils.Extensions;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public class Texture : IDisposable
	{
		public int TextureId { get; private set; }
		public uint TextureHash
		{
			get
			{
				return TextureCacheKey.TextureHash;
			}
		}

		public DateTime RecheckTimestamp;
		public TextureCacheKey TextureCacheKey;
		public int Width;
		public int Height;

		public Texture()
		{
			OpenglGpuImpl.GraphicsContext.MakeCurrent(OpenglGpuImpl.NativeWindow.WindowInfo);

			//lock (OpenglGpuImpl.GpuLock)
			{
				TextureId = GL.GenTexture();
				var GlError = GL.GetError();
				//Console.Error.WriteLine("GenTexture: {0} : Thread : {1} <- {2}", GlError, Thread.CurrentThread.ManagedThreadId, TextureId);
				if (GlError != ErrorCode.NoError)
				{
					//TextureId = 0;
				}
			}
		}

		public bool SetData(PixelFormatDecoder.OutputPixel *Pixels, int TextureWidth, int TextureHeight)
		{
			//lock (OpenglGpuImpl.GpuLock)
			{
				//if (TextureId != 0)
				{
					this.Width = TextureWidth;
					this.Height = TextureHeight;
					Bind();
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureWidth, TextureHeight, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888Reversed, new IntPtr(Pixels));
					GL.Flush();
					var GlError = GL.GetError();

					if (GlError != ErrorCode.NoError)
					{
						//Console.Error.WriteLine("TexImage2D: {0} : TexId:{1} : {2} : {3}x{4}", GlError, TextureId, new IntPtr(Pixels), TextureWidth, TextureHeight);
						TextureId = 0;
						Bind();
						return false;
					}
				}
			}
			return true;

			//glTexEnvf(GL_TEXTURE_ENV, GL_RGB_SCALE, 1.0); // 2.0 in scale_2x
			//GL.TexEnv(TextureEnvTarget.TextureEnv, GL_TEXTURE_ENV_MODE, TextureEnvModeTranslate[state.texture.effect]);

		}

		public void Bind()
		{
			//lock (OpenglGpuImpl.GpuLock)
			{
				if (TextureId != 0)
				{
					//GL.Enable(EnableCap.Texture2D);
					GL.BindTexture(TextureTarget.Texture2D, TextureId);

					var GlError = GL.GetError();
					if (GlError != ErrorCode.NoError)
					{
						//Console.Error.WriteLine("Bind: {0} : {1}", GlError, TextureId);
					}
				}
				else
				{
					//GL.Disable(EnableCap.Texture2D);
				}
			}
		}

		public void Dispose()
		{
			if (TextureId != 0)
			{
				GL.DeleteTexture(TextureId);
				TextureId = 0;
			}
		}

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}

	public struct TextureCacheKey
	{
		public uint TextureAddress;
		public uint TextureHash;
		public GuPixelFormats TextureFormat;

		public uint ClutAddress;
		public uint ClutHash;
		public GuPixelFormats ClutFormat;
		public int ClutStart;
		public int ClutShift;
		public int ClutMask;
		public bool Swizzled;

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}

	sealed unsafe public class TextureCache : PspEmulatorComponent
	{
		PspMemory PspMemory;
		//Dictionary<TextureCacheKey, Texture> Cache = new Dictionary<TextureCacheKey, Texture>();
		Dictionary<ulong, Texture> Cache = new Dictionary<ulong, Texture>();

		byte[] SwizzlingBuffer = new byte[1024 * 1024 * 4];
		PixelFormatDecoder.OutputPixel[] DecodedTextureBuffer = new PixelFormatDecoder.OutputPixel[1024 * 1024];

		public override void InitializeComponent()
		{
			PspMemory = PspEmulatorContext.GetInstance<PspMemory>();
		}

		public Texture Get(TextureStateStruct* TextureState, ClutStateStruct* ClutState)
		{
			Texture Texture;
			//GC.Collect();
			bool Swizzled = TextureState->Swizzled;
			uint TextureAddress = TextureState->Mipmap0.Address;
			uint ClutAddress = ClutState->Address;
			var ClutFormat = ClutState->PixelFormat;
			var ClutStart = ClutState->Start;
			var ClutDataStart = PixelFormatDecoder.GetPixelsSize(ClutFormat, ClutStart);

			ulong Hash1 = TextureAddress | (ulong)((ClutAddress + ClutDataStart) << 32);
			bool Recheck = false;
			if (Cache.TryGetValue(Hash1, out Texture))
			{
				if (Texture.RecheckTimestamp != RecheckTimestamp)
				{
					Recheck = true;
				}
			}
			else
			{
				Recheck = true;
			}

			if (Recheck)
			{
				//Console.Write(".");

				//Console.WriteLine("{0:X}", ClutAddress);
				var TexturePointer = (byte*)PspMemory.PspAddressToPointerSafe(TextureAddress);
				var ClutPointer = (byte *)PspMemory.PspAddressToPointerSafe(ClutAddress);
				var TextureFormat = TextureState->PixelFormat;
				//var Width = TextureState->Mipmap0.TextureWidth;
				int BufferWidth = TextureState->Mipmap0.BufferWidth;
				var Height = TextureState->Mipmap0.TextureHeight;
				var TextureDataSize = PixelFormatDecoder.GetPixelsSize(TextureFormat, BufferWidth * Height);
				if (ClutState->NumberOfColors > 256)
				{
					ClutState->NumberOfColors = 256;
				}
				var ClutDataSize = PixelFormatDecoder.GetPixelsSize(ClutFormat, ClutState->NumberOfColors);
				var ClutCount = ClutState->NumberOfColors;
				var ClutShift = ClutState->Shift;
				var ClutMask = ClutState->Mask;

				//Console.WriteLine(TextureFormat);


				if (!PspMemory.IsAddressValid((uint)(TextureAddress + TextureDataSize - 1)))
				{
					Console.Error.WriteLine("Invalid TEXTURE!");
					return new Texture();
				}

				if (TextureDataSize > 2048 * 2048 * 4)
				{
					Console.Error.WriteLine("UPDATE_TEXTURE(TEX={0},CLUT={1}:{2}:{3}:{4}:0x{5:X},SIZE={6}x{7},{8},Swizzled={9})", TextureFormat, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, BufferWidth, Height, BufferWidth, Swizzled);
					Console.Error.WriteLine("Invalid TEXTURE!");
					return new Texture();
				}

				//Console.WriteLine("TextureAddress=0x{0:X}, TextureDataSize=0x{1:X}", TextureAddress, TextureDataSize);

				TextureCacheKey TextureCacheKey = new TextureCacheKey()
				{
					TextureAddress = TextureAddress,
					TextureFormat = TextureFormat,
					TextureHash = FastHash((uint*)TexturePointer, TextureDataSize),

					ClutHash = FastHash((uint*)&(ClutPointer[ClutDataStart]), ClutDataSize),
					ClutAddress = ClutAddress,
					ClutFormat = ClutFormat,
					ClutStart = ClutStart,
					ClutShift = ClutShift,
					ClutMask = ClutMask,
					Swizzled = Swizzled,
				};

				if (Texture == null || (!Texture.TextureCacheKey.Equals(TextureCacheKey)))
				{
#if DEBUG_TEXTURE_CACHE
					string TextureName = "texture_" + TextureCacheKey.TextureHash + "_" + TextureCacheKey.ClutHash + "_" + TextureFormat + "_" + ClutFormat + "_" + BufferWidth + "x" + Height;

					Console.Error.WriteLine("UPDATE_TEXTURE(TEX={0},CLUT={1}:{2}:{3}:{4}:0x{5:X},SIZE={6}x{7},{8},Swizzled={9})", TextureFormat, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, BufferWidth, Height, BufferWidth, Swizzled);
#endif
					Texture = new Texture();
					Texture.TextureCacheKey = TextureCacheKey;
					{
						//int TextureWidth = Math.Max(BufferWidth, Height);
						//int TextureHeight = Math.Max(BufferWidth, Height);
						int TextureWidth = BufferWidth;
						int TextureHeight = Height;

						fixed (PixelFormatDecoder.OutputPixel* TexturePixelsPointer = DecodedTextureBuffer)
						{
							if (Swizzled)
							{
								fixed (byte* SwizzlingBufferPointer = SwizzlingBuffer)
								{
									Marshal.Copy(new IntPtr(TexturePointer), SwizzlingBuffer, 0, TextureDataSize);
									PixelFormatDecoder.UnswizzleInline(TextureFormat, (void*)SwizzlingBufferPointer, BufferWidth, Height);
									PixelFormatDecoder.Decode(
										TextureFormat, (void*)SwizzlingBufferPointer, TexturePixelsPointer, BufferWidth, Height,
										ClutPointer, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, StrideWidth: PixelFormatDecoder.GetPixelsSize(TextureFormat, TextureWidth)
									);
								}
							}
							else
							{
								PixelFormatDecoder.Decode(
									TextureFormat, (void*)TexturePointer, TexturePixelsPointer, BufferWidth, Height,
									ClutPointer, ClutFormat, ClutCount, ClutStart, ClutShift, ClutMask, StrideWidth: PixelFormatDecoder.GetPixelsSize(TextureFormat, TextureWidth)
								);
							}
							
#if DEBUG_TEXTURE_CACHE
							var Bitmap = new Bitmap(BufferWidth, Height);
							BitmapUtils.TransferChannelsDataInterleaved(
								Bitmap.GetFullRectangle(),
								Bitmap,
								(byte*)TexturePixelsPointer,
								BitmapUtils.Direction.FromDataToBitmap,
								BitmapChannel.Red,
								BitmapChannel.Green,
								BitmapChannel.Blue,
								BitmapChannel.Alpha
							);
							Bitmap.Save(TextureName + ".png");
#endif

							var Result = Texture.SetData(TexturePixelsPointer, TextureWidth, TextureHeight);
#if DEBUG_TEXTURE_CACHE
							if (!Result || Texture.TextureId == 0)
							{
								var Bitmap2 = new Bitmap(BufferWidth, Height);
								BitmapUtils.TransferChannelsDataInterleaved(
									Bitmap2.GetFullRectangle(),
									Bitmap2,
									(byte*)TexturePixelsPointer,
									BitmapUtils.Direction.FromDataToBitmap,
									BitmapChannel.Red,
									BitmapChannel.Green,
									BitmapChannel.Blue,
									BitmapChannel.Alpha
								);
								string TextureName2 = @"C:\projects\csharp\cspspemu\__invalid_texture_" + TextureCacheKey.TextureHash + "_" + TextureCacheKey.ClutHash + "_" + TextureFormat + "_" + ClutFormat + "_" + BufferWidth + "x" + Height;
								Bitmap.Save(TextureName2 + ".png");
							}
#endif
						}
					}
					if (Cache.ContainsKey(Hash1))
					{
						Cache[Hash1].Dispose();
					}
					Cache[Hash1] = Texture;
				}
			}

			Texture.RecheckTimestamp = RecheckTimestamp;

			return Texture;
		}

		private DateTime RecheckTimestamp = DateTime.MinValue;

		public void RecheckAll()
		{
			RecheckTimestamp = DateTime.Now;
		}

		static public uint FastHash(uint* Pointer, int Count, uint StartHash = 0)
		{
			return Hashing.FastHash(Pointer, Count, StartHash);
		}
	}
}