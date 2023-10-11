using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class GPUImage
{
	private const int BlurSampleCount = 19;

	private const int SelectiveBlurSampleCount = 10;

	private static Dictionary<string, Material> Materials = new Dictionary<string, Material>();

	private static List<RenderTexture> TempRenderTargets = new List<RenderTexture>();

	private static RenderTexture LastActive = null;

	private static GameObject _DebugDisplayParent;

	private static bool _DoDebugDisplay = false;

	private static float _DebugDisplayOffset = 0.75f;

	public static Material GetMaterial(string shader)
	{
		if (Materials.ContainsKey(shader))
		{
			return Materials[shader];
		}
		Shader shader2 = Shader.Find(shader);
		if (shader2 != null)
		{
			Materials.Add(shader, new Material(shader2));
			return Materials[shader];
		}
		UnityEngine.Debug.LogError("GPUImage.GetMaterial: Shader [" + shader + "] not found.");
		return null;
	}

	public static RenderTexture IterateEffect(Texture input, Material effect, int iterations, int pass, params RenderTexture[] rts)
	{
		int num = 0;
		if (input == rts[0])
		{
			num++;
		}
		for (int i = 0; i < iterations; i++)
		{
			Graphics.Blit(input, rts[num], effect, pass);
			input = rts[num];
			num++;
			num %= rts.Length;
		}
		return input as RenderTexture;
	}

	public static RenderTexture GetTempRenderTexture(int width, int height, int depthBuffer = 0, RenderTextureFormat format = RenderTextureFormat.ARGB32, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear, int antiAliasing = 1, bool generateMipmaps = false)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, depthBuffer, format, readWrite, antiAliasing);
		temporary.Clear();
		if (temporary.useMipMap != generateMipmaps)
		{
			temporary.useMipMap = generateMipmaps;
		}
		TempRenderTargets.Add(temporary);
		return temporary;
	}

	public static void Clear(this RenderTexture rt)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = rt;
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		RenderTexture.active = active;
	}

	public static RenderTexture GetTempRenderTexture(RenderTexture rt)
	{
		return GetTempRenderTexture(rt.width, rt.height, rt.depth, rt.format, (!rt.sRGB) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB, rt.antiAliasing, rt.useMipMap);
	}

	public static void ReleaseTempRenderTextures()
	{
		for (int i = 0; i < TempRenderTargets.Count; i++)
		{
			RenderTexture.ReleaseTemporary(TempRenderTargets[i]);
		}
		TempRenderTargets.Clear();
	}

	public static void BeginProcess()
	{
		LastActive = RenderTexture.active;
	}

	public static void EndProcess()
	{
		RenderTexture.active = LastActive;
		ReleaseTempRenderTextures();
	}

	public static void CopyToTexture2D(this RenderTexture rt, Texture2D copyTo)
	{
		if (rt.width != copyTo.width || rt.height != copyTo.height)
		{
			copyTo.Reinitialize(rt.width, rt.height);
		}
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = rt;
		copyTo.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
		copyTo.Apply();
		RenderTexture.active = active;
	}

	public static Texture2D ToTexture2D(this RenderTexture rt, bool mipmap = true, int anisoLevel = 9, FilterMode filter = FilterMode.Trilinear, TextureWrapMode wrap = TextureWrapMode.Clamp, TextureFormat format = TextureFormat.ARGB32, bool? linearOverride = null, bool makeNoLongerReadable = false)
	{
		Texture2D obj = new Texture2D(rt.width, rt.height, format, mipmap, linearOverride ?? (!rt.sRGB))
		{
			anisoLevel = anisoLevel,
			filterMode = filter,
			wrapMode = wrap
		};
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = rt;
		obj.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
		obj.Apply(updateMipmaps: true, makeNoLongerReadable);
		RenderTexture.active = active;
		return obj;
	}

	public static RenderTexture InsureSRGB(this RenderTexture rt)
	{
		if (rt.sRGB)
		{
			return rt;
		}
		RenderTexture tempRenderTexture = GetTempRenderTexture(rt.width, rt.height, rt.depth, rt.format, RenderTextureReadWrite.sRGB, rt.antiAliasing, rt.useMipMap);
		Graphics.Blit(rt, tempRenderTexture);
		return tempRenderTexture;
	}

	public static Texture2D IntoTexture2D(this RenderTexture rt, Texture2D texture)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = rt;
		texture.Reinitialize(rt.width, rt.height);
		texture.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
		texture.Apply();
		RenderTexture.active = active;
		return texture;
	}

	public static RenderTexture ToRenderTexture(this Texture2D texture, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear)
	{
		RenderTexture tempRenderTexture = GetTempRenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, readWrite);
		Graphics.Blit(texture, tempRenderTexture);
		return tempRenderTexture;
	}

	public static bool IsPow2(this Texture2D t)
	{
		if (t.width.IsPowerOf(2))
		{
			return t.height.IsPowerOf(2);
		}
		return false;
	}

	public static int MaxDimension(this Texture t)
	{
		return Mathf.Max(t.width, t.height);
	}

	public static float AspectRatio(this Texture t)
	{
		return (float)t.width / (float)t.height;
	}

	public static bool IsCompressed(this Texture2D t)
	{
		if (t.format != TextureFormat.DXT1)
		{
			return t.format == TextureFormat.DXT5;
		}
		return true;
	}

	public static Vector4 GetTexelSize(this Texture texture)
	{
		return new Vector4(1f / (float)texture.width, 1f / (float)texture.height, 0f, 0f);
	}

	public static Texture2D ToTexture2D(this float[] array)
	{
		Texture2D texture2D = new Texture2D(array.Length, 1, TextureFormat.ARGB32, mipChain: false, linear: true);
		texture2D.filterMode = FilterMode.Point;
		texture2D.anisoLevel = 0;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < array.Length; i++)
		{
			texture2D.SetPixel(i, 0, array[i].ToColor());
		}
		texture2D.Apply();
		return texture2D;
	}

	public static bool HasTransparency(this Texture2D t, float threshold = 0.1f)
	{
		for (int i = 0; i < t.width; i++)
		{
			for (int j = 0; j < t.height; j++)
			{
				if (t.GetPixel(i, j).a < threshold)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static List<Vector2> BinaryToPoints(this Texture2D t, int cellSize = 3, Rect? bounds = null)
	{
		float num = (float)t.height / (float)t.width;
		int num2 = t.width - 1;
		int num3 = t.height - 1;
		float num4 = 1f / (float)num2;
		float num5 = 1f / (float)num3;
		List<Vector2> list = new List<Vector2>();
		Rect rect = bounds ?? new Rect(-1f, -1f * num, 2f, 2f * num);
		float xMin = rect.xMin;
		float xMax = rect.xMax;
		float yMin = rect.yMin;
		float yMax = rect.yMax;
		int num6 = 0;
		cellSize = Math.Max(cellSize, 1);
		for (int i = 0; i < t.width; i += cellSize)
		{
			for (int j = 0; j < t.height; j += cellSize)
			{
				float num7 = 0f;
				float num8 = 0f;
				float num9 = 0f;
				int num10 = Math.Min(i + cellSize, t.width);
				int num11 = Math.Min(j + cellSize, t.height);
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				for (int k = i; k < num10; k++)
				{
					for (int l = j; l < num11; l++)
					{
						if (t.GetPixel(k, l).r > 0.5f)
						{
							num7 += 1f;
							num8 += (float)k;
							num9 += (float)l;
							if (k == 0)
							{
								flag = true;
							}
							else if (k == num2)
							{
								flag2 = true;
							}
							if (l == 0)
							{
								flag3 = true;
							}
							else if (l == num3)
							{
								flag4 = true;
							}
						}
					}
				}
				if (num7 > 0f)
				{
					Vector2 item = new Vector2(Mathf.Lerp(xMin, xMax, num8 / num7 * num4), Mathf.Lerp(yMin, yMax, num9 / num7 * num5));
					if (flag)
					{
						item.x = xMin;
					}
					else if (flag2)
					{
						item.x = xMax;
					}
					if (flag3)
					{
						item.y = yMin;
					}
					else if (flag4)
					{
						item.y = yMax;
					}
					list.Add(item);
				}
			}
			num6++;
		}
		return list;
	}

	[Conditional("UNITY_EDITOR")]
	public static void DebugDisplay(this Texture rt, string name, bool clearPrevious = false)
	{
		if (_DoDebugDisplay)
		{
			if (clearPrevious)
			{
				ClearDebugDisplayTextures();
			}
			if (_DebugDisplayParent == null)
			{
				_DebugDisplayParent = new GameObject("TextureDebugDisplay");
			}
			float num = rt.MaxDimension();
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
			gameObject.name = _DebugDisplayParent.transform.childCount + "_" + name;
			gameObject.transform.localScale = new Vector3((float)rt.width / num, (float)rt.height / num, 1f);
			_DebugDisplayOffset += gameObject.transform.localScale.x * 0.5f;
			gameObject.transform.position = new Vector3(_DebugDisplayOffset, gameObject.transform.localScale.y * 0.5f, 0f);
			gameObject.GetComponent<Renderer>().material = GetMaterial("Process/Identity");
			gameObject.GetComponent<Renderer>().material.mainTexture = rt;
			gameObject.transform.parent = _DebugDisplayParent.transform;
			_DebugDisplayOffset += gameObject.transform.localScale.x * 0.5f;
		}
	}

	public static void ClearDebugDisplayTextures()
	{
		if ((bool)_DebugDisplayParent)
		{
			_DebugDisplayParent.DestroyChildren();
		}
		_DebugDisplayOffset = 0f;
	}

	[Conditional("UNITY_EDITOR")]
	public static void DebugInfo(this Texture t, string name)
	{
		UnityEngine.Debug.Log(string.Format("Debug Texture [{0}]: Resolution = [{1}, {2}], Filter = [{3}], Wrap = [{4}], Aniso = [{5}], Mipmap = [{6}], MSAA = {7}, Depth = {8}", name, t.width, t.height, t.filterMode, t.wrapMode, t.anisoLevel, t.IsMipmapped(), (t is RenderTexture) ? ((RenderTexture)t).antiAliasing.ToString() : "N/A", (t is RenderTexture) ? ((RenderTexture)t).depth.ToString() : "N/A"));
	}

	public static bool IsMipmapped(this Texture t)
	{
		if (t is Texture2D)
		{
			return (t as Texture2D).mipmapCount > 0;
		}
		if (t is RenderTexture)
		{
			return (t as RenderTexture).autoGenerateMips;
		}
		return false;
	}

	public static Texture2D GaussianKernel(int numSamples, float blurAmount, float targetCurveArea)
	{
		float[] array = new float[numSamples];
		int num = (numSamples - 1) / 2;
		for (int i = -num; i < numSamples - num; i++)
		{
			array[i + num] = (float)(1.0 / (double)Mathf.Sqrt(MathF.PI * 2f * blurAmount) * (double)Mathf.Exp((float)(-(i * i)) / (2f * blurAmount * blurAmount)));
		}
		float num2 = 0f;
		for (int j = 0; j < numSamples; j++)
		{
			num2 += array[j];
		}
		float num3 = targetCurveArea / num2;
		for (int k = 0; k < numSamples; k++)
		{
			array[k] *= num3;
		}
		return array.ToTexture2D();
	}

	public static Texture2D[] GaussianOffsets(float xRes, float yRes, int numSamples)
	{
		float[] array = new float[numSamples];
		float[] array2 = new float[numSamples];
		float num = 1f / xRes;
		float num2 = 1f / yRes;
		int num3 = (numSamples - 1) / 2;
		for (int i = 0; i < numSamples; i++)
		{
			array[i] = (float)(-num3 + i) * num;
			array2[i] = (float)(-num3 + i) * num2;
		}
		return new Texture2D[2]
		{
			array.ToTexture2D(),
			array2.ToTexture2D()
		};
	}

	public static Texture2D GaussianKernelOneSided(int numSamples, float blurAmount, float targetCurveArea)
	{
		float[] array = new float[numSamples];
		for (int i = 0; i < numSamples; i++)
		{
			array[i] = (float)(1.0 / (double)Mathf.Sqrt(MathF.PI * 2f * blurAmount) * (double)Mathf.Exp((float)(-(i * i)) / (2f * blurAmount * blurAmount)));
		}
		float num = 0f;
		for (int j = 0; j < numSamples; j++)
		{
			num += array[j];
		}
		float num2 = num - array[0];
		float num3 = targetCurveArea / (num + num2);
		for (int k = 0; k < numSamples; k++)
		{
			array[k] *= num3;
		}
		return array.ToTexture2D();
	}

	public static Texture2D[] GaussianOffsetsOneSided(float xRes, float yRes, int numSamples)
	{
		float[] array = new float[numSamples];
		float[] array2 = new float[numSamples];
		float num = 1f / xRes;
		float num2 = 1f / yRes;
		for (int i = 0; i < numSamples; i++)
		{
			array[i] = (float)i * num;
			array2[i] = (float)i * num2;
		}
		return new Texture2D[2]
		{
			array.ToTexture2D(),
			array2.ToTexture2D()
		};
	}

	public static RenderTexture Blur(Texture source, float blurAmount, bool blurAlpha = true, int downSampleCount = 0)
	{
		Material material = GetMaterial("Process/GaussianBlur");
		blurAmount = Math.Max(MathUtil.BigEpsilon, blurAmount);
		Texture2D value = GaussianKernel(19, blurAmount, 1f);
		Texture2D[] array = GaussianOffsets(source.width, source.height, 19);
		material.SetTexture("_Kernel", value);
		material.SetTexture("_XOffsets", array[0]);
		material.SetTexture("_YOffsets", array[1]);
		int num = (int)(Mathf.Pow(2f, downSampleCount) + 0.5f);
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width / num, source.height / num);
		int num2 = ((!blurAlpha) ? 2 : 0);
		Graphics.Blit(source, tempRenderTexture, material, num2);
		Graphics.Blit(tempRenderTexture.ToTexture2D(), tempRenderTexture, material, num2 + 1);
		return tempRenderTexture;
	}

	public static RenderTexture Median(Texture source, byte iterations = 1)
	{
		Material material = GetMaterial("Process/Median");
		material.SetVector("_TexelSize", source.texelSize);
		iterations = Math.Max(iterations, (byte)1);
		return IterateEffect(source, material, iterations, 0, GetTempRenderTexture(source.width, source.height), GetTempRenderTexture(source.width, source.height));
	}

	public static RenderTexture Posterize(Texture source, byte levels = 128)
	{
		levels = Math.Max((byte)2, levels);
		Material material = GetMaterial("Process/Posterize");
		material.SetFloat("_Levels", 1f / (float)(int)levels);
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture Threshold(Texture source, Color lowThreshold, Color highThreshold)
	{
		Material material = GetMaterial("Process/Threshold");
		material.SetColor("_LowThreshold", lowThreshold);
		material.SetColor("_HighThreshold", highThreshold);
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture Invert(Texture source)
	{
		Material material = GetMaterial("Process/Invert");
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture Multiply(Texture source, Texture multipier, EffectChannelType type = EffectChannelType.All)
	{
		Material material = GetMaterial("Process/Multiply");
		material.SetTexture("_SecondTex", multipier);
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material, (int)type);
		return tempRenderTexture;
	}

	public static RenderTexture Pow(Texture source, float pow, EffectChannelType type = EffectChannelType.All)
	{
		Material material = GetMaterial("Process/Pow");
		material.SetFloat("_Pow", pow);
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material, (int)type);
		return tempRenderTexture;
	}

	public static RenderTexture LumToAlpha(Texture source)
	{
		Material material = GetMaterial("Process/LumToAlpha");
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture AlphaToLum(Texture source)
	{
		Material material = GetMaterial("Process/AlphaToLum");
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture Lerp(Texture start, Texture end, Texture lerp)
	{
		Material material = GetMaterial("Process/Lerp");
		material.SetTexture("_SecondTex", end);
		material.SetTexture("_LerpTex", lerp);
		RenderTexture tempRenderTexture = GetTempRenderTexture(start.width, start.height);
		Graphics.Blit(start, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture Luminance(Texture source)
	{
		Material material = GetMaterial("Process/Luminance");
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture CannyEdgeDetect(Texture source, float blurAmount = 1f, float edgeThreshold = 0.075f, byte noiseReduction = 3, int downSampleCount = 0)
	{
		if (noiseReduction > 0)
		{
			source = Median(source, noiseReduction);
		}
		if (blurAmount > 0f)
		{
			source = Blur(source, blurAmount, blurAlpha: true, downSampleCount);
		}
		Material material = GetMaterial("Process/CannyGradient");
		material.SetVector("_TexelSize", source.GetTexelSize());
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		Material material2 = GetMaterial("Process/CannyEdgeDetect");
		material2.SetVector("_TexelSize", source.GetTexelSize());
		material2.SetFloat("_Threshold", edgeThreshold);
		RenderTexture tempRenderTexture2 = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(tempRenderTexture, tempRenderTexture2, material2);
		return tempRenderTexture2;
	}

	public static RenderTexture AlphaDilateColor(Texture source, Color color, int dilateAmount = 1, float threshold = 0.25f)
	{
		Material material = GetMaterial("Process/AlphaDilateColor");
		RenderTexture[] rts = new RenderTexture[2]
		{
			GetTempRenderTexture(source.width, source.height),
			GetTempRenderTexture(source.width, source.height)
		};
		material.SetFloat("_AlphaThreshold", threshold);
		material.SetVector("_TexelSize", new Vector4(1f / (float)source.width, 1f / (float)source.height, 0f, 0f));
		material.SetColor("_Color", color);
		return IterateEffect(source, material, dilateAmount, 0, rts);
	}

	public static RenderTexture AlphaCorrode(Texture source, int corrodeAmount = 1)
	{
		Material material = GetMaterial("Process/AlphaDilateColor");
		RenderTexture[] rts = new RenderTexture[2]
		{
			GetTempRenderTexture(source.width, source.height),
			GetTempRenderTexture(source.width, source.height)
		};
		material.SetVector("_TexelSize", new Vector4(1f / (float)source.width, 1f / (float)source.height, 0f, 0f));
		return IterateEffect(source, material, corrodeAmount, 1, rts);
	}

	public static RenderTexture SetAlphaColor(Texture source, Color color, bool generateMipmaps = false)
	{
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1, generateMipmaps);
		Material material = GetMaterial("Process/SetAlphaColor");
		material.SetColor("_AlphaColor", color);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture SetAlpha(Texture source, Texture alpha)
	{
		Material material = GetMaterial("Process/SetAlpha");
		material.SetTexture("_AlphaTex", alpha);
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture OverlayAlpha(Texture source, Texture alpha, Color? color = null)
	{
		Material material = GetMaterial("Process/OverlayAlpha");
		material.SetTexture("_AlphaTex", alpha);
		material.SetColor("_Color", color ?? Color.white);
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture HardLight(Texture source, Texture hardLightTex, Color? color = null, bool overlay = false)
	{
		Material material = GetMaterial("Process/HardLight");
		material.SetTexture("_HardLightTex", hardLightTex);
		material.SetColor("_Color", color ?? Color.white);
		material.SetFloat("_Overlay", overlay.ToFloat());
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static RenderTexture ProcessTextureDimensions(Texture source, int maxResolution = 1024, bool forcePow2 = true)
	{
		int num = source.width;
		int num2 = source.height;
		int num3 = source.MaxDimension();
		if (num3 > maxResolution)
		{
			float num4 = (float)maxResolution / (float)num3;
			num = (int)((float)num * num4 + 0.5f);
			num2 = (int)((float)num2 * num4 + 0.5f);
		}
		if (forcePow2)
		{
			num = MathUtil.RoundToNearestPowerOfInt(num, 2f);
			num2 = MathUtil.RoundToNearestPowerOfInt(num2, 2f);
		}
		source.anisoLevel = 9;
		source.wrapMode = TextureWrapMode.Clamp;
		source.filterMode = FilterMode.Trilinear;
		int num5 = source.width;
		int num6 = source.height;
		int num7 = 0;
		Texture source2 = source;
		RenderTexture[] array = new RenderTexture[2];
		do
		{
			num7++;
			num7 %= array.Length;
			if ((float)num5 / (float)num > 2f)
			{
				num5 /= 2;
				num6 /= 2;
			}
			else
			{
				num5 = num;
				num6 = num2;
			}
			array[num7] = GetTempRenderTexture(num5, num6);
			Graphics.Blit(source2, array[num7]);
			source2 = array[num7];
		}
		while (num5 > num);
		return array[num7];
	}

	public static RenderTexture ScaleTexture(Texture source, int newMaxDimensionResolution)
	{
		Material material = GetMaterial("Process/Identity");
		material.mainTexture = source;
		float num = (float)newMaxDimensionResolution / (float)source.MaxDimension();
		RenderTexture tempRenderTexture = GetTempRenderTexture(Mathf.RoundToInt((float)source.width * num), Mathf.RoundToInt((float)source.height * num));
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}

	public static Color32[] GetPixels32Unreadable(this Texture2D t, bool mipmap = false)
	{
		Texture2D texture2D = t.ToRenderTexture().ToTexture2D(mipmap);
		Color32[] pixels = texture2D.GetPixels32();
		UnityEngine.Object.Destroy(texture2D);
		return pixels;
	}

	public static Rect GetAlphaRect(this Texture2D t, bool textureIsReadable = false, int padding = 1, byte alphaThreshold = 25)
	{
		int num = int.MaxValue;
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		int num4 = int.MinValue;
		Color32[] array = (textureIsReadable ? t.GetPixels32() : t.GetPixels32Unreadable());
		for (int i = 0; i < t.width; i++)
		{
			for (int j = 0; j < t.height; j++)
			{
				if (array[i + t.width * j].a >= alphaThreshold)
				{
					num = ((i < num) ? i : num);
					num2 = ((i > num2) ? i : num2);
					num3 = ((j < num3) ? j : num3);
					num4 = ((j > num4) ? j : num4);
				}
			}
		}
		return Rect.MinMaxRect(Mathf.Max(0, num - padding), Mathf.Max(0, num3 - padding), Mathf.Min(t.width, num2 + padding), Mathf.Min(t.height, num4 + padding));
	}

	public static Texture2D AutoCropAlphaCPU(this Texture2D t, int padding = 0, byte alphaThreshold = 25, bool makeNonReadable = false)
	{
		Rect alphaRect = t.GetAlphaRect(textureIsReadable: true, padding, alphaThreshold);
		SRect sRect = new SRect(alphaRect.min.RoundToShort2(), alphaRect.max.RoundToShort2());
		Texture2D texture2D = new Texture2D(sRect.width, sRect.height, t.format, t.IsMipmapped(), linear: false);
		texture2D.SetPixels(t.GetPixels(sRect.min.x, sRect.min.y, sRect.width, sRect.height));
		texture2D.Apply(t.IsMipmapped(), makeNonReadable);
		return texture2D;
	}

	public static Texture2D AutoCropAlpha(Texture2D source, int padding = 2, int maxResolution = 512, float alphaThreshold = 0.25f)
	{
		int num = int.MaxValue;
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		int num4 = int.MinValue;
		Color[] pixels = source.GetPixels();
		for (int i = 0; i < source.width; i++)
		{
			for (int j = 0; j < source.height; j++)
			{
				if (pixels[i + source.width * j].a >= alphaThreshold)
				{
					num = ((i < num) ? i : num);
					num2 = ((i > num2) ? i : num2);
					num3 = ((j < num3) ? j : num3);
					num4 = ((j > num4) ? j : num4);
				}
			}
		}
		int num5 = num2 - num;
		int num6 = num4 - num3;
		num3 = source.height - num4;
		int num7 = Math.Max(num5, num6);
		maxResolution = Math.Min(MathUtil.CeilingToNearestPowerOfInt(num7, 2f), maxResolution);
		maxResolution -= padding;
		int num8 = num5;
		int num9 = num6;
		if (num7 > maxResolution)
		{
			float num10 = (float)maxResolution / (float)num7;
			num8 = Mathf.FloorToInt((float)num8 * num10);
			num9 = Mathf.RoundToInt((float)num9 * num10);
		}
		int canvasX = MathUtil.CeilingToNearestPowerOfInt(num8, 2f);
		int canvasY = MathUtil.CeilingToNearestPowerOfInt(num9, 2f);
		string path = IOUtil.WriteBytesTemp("Process", ".png", source.EncodeToPNG());
		NConvert.CropResizeAndCanvas(path, num, num3, num5, num6, maxResolution, canvasX, canvasY, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: true, linear: true);
		texture2D.LoadImage(IOUtil.LoadBytes(path));
		return texture2D;
	}

	public static Texture2D PadTextureToPow2(Texture2D texture, float xPivot = 0.5f, float yPivot = 0.5f, float blurAmount = 0f)
	{
		int num = MathUtil.CeilingToNearestPowerOfInt(texture.width, 2f);
		int num2 = MathUtil.CeilingToNearestPowerOfInt(texture.height, 2f);
		if (num == texture.width && num2 == texture.height)
		{
			return texture;
		}
		TextureWrapMode wrapMode = texture.wrapMode;
		texture.wrapMode = TextureWrapMode.Clamp;
		FilterMode filterMode = texture.filterMode;
		texture.filterMode = ((num != texture.width && num2 != texture.height) ? FilterMode.Bilinear : FilterMode.Point);
		RenderTexture tempRenderTexture = GetTempRenderTexture(num, num2);
		float num3 = texture.AspectRatio();
		float num4 = tempRenderTexture.AspectRatio();
		Rect rect = new Rect(0f, 0f, 1f, 1f);
		Rect r = rect;
		Rect optimalInscirbedAspectRatioRect = r.GetOptimalInscirbedAspectRatioRect(num3 / num4, new Vector2(xPivot, yPivot));
		r.size = r.size.Multiply(optimalInscirbedAspectRatioRect.size.Inverse());
		r.position -= new Vector2((r.width - rect.width) * xPivot, (r.height - rect.height) * yPivot);
		Vector2 min = r.min;
		Vector2 max = r.max;
		Vector4 value = new Vector4(min.x, min.y, max.x, max.y);
		Material material = ((blurAmount <= 0f) ? GetMaterial("Process/Pad") : GetMaterial("Process/Pad Mirror Blur"));
		material.mainTexture = texture;
		material.SetVector("_UVRect", value);
		material.SetVector("_TexelSize", tempRenderTexture.texelSize);
		if (blurAmount > 0f)
		{
			material.SetFloat("_BlurAmount", blurAmount);
		}
		Graphics.Blit(texture, tempRenderTexture, material, 0);
		if (blurAmount > 0f)
		{
			Graphics.Blit(tempRenderTexture.ToTexture2D(), tempRenderTexture, material, 1);
		}
		texture.wrapMode = wrapMode;
		texture.filterMode = filterMode;
		tempRenderTexture.IntoTexture2D(texture);
		ReleaseTempRenderTextures();
		return texture;
	}

	public static Texture2D SetAlphaBackground(Texture2D texture, Texture2D backgroundTexture)
	{
		float num = (float)(texture.width - backgroundTexture.width) * 0.5f / (float)texture.width;
		float num2 = (float)(texture.height - backgroundTexture.height) * 0.5f / (float)texture.height;
		Material material = GetMaterial("Process/AlphaBackground");
		material.mainTexture = texture;
		material.SetTexture("_BackgroundTex", backgroundTexture);
		material.SetVector("_UVRect", new Vector4(0f - num, 0f - num2, 1f + num, 1f + num2));
		RenderTexture tempRenderTexture = GetTempRenderTexture(texture.width, texture.height);
		Graphics.Blit(texture, tempRenderTexture, material);
		tempRenderTexture.IntoTexture2D(texture);
		ReleaseTempRenderTextures();
		return texture;
	}

	public static Texture2D Crop(Texture2D texture, Vector4 uvCropRect, int? cropXResolution = null, int? cropYResolution = null, bool mipmap = true, bool linear = false)
	{
		cropXResolution = cropXResolution ?? Mathf.RoundToInt((uvCropRect.z - uvCropRect.x) * (float)texture.width);
		cropYResolution = cropYResolution ?? Mathf.RoundToInt((uvCropRect.w - uvCropRect.y) * (float)texture.height);
		RenderTexture tempRenderTexture = GetTempRenderTexture(cropXResolution.Value, cropYResolution.Value);
		Material material = GetMaterial("Process/Pad");
		material.mainTexture = texture;
		material.SetVector("_UVRect", uvCropRect);
		material.SetVector("_TexelSize", texture.texelSize);
		Graphics.Blit(texture, tempRenderTexture, material, 0);
		Texture2D result = tempRenderTexture.IntoTexture2D(new Texture2D(1, 1, TextureFormat.RGBA32, mipmap, linear));
		ReleaseTempRenderTextures();
		return result;
	}

	public static RenderTexture Flip(this Texture source, bool flipHorizontal, bool flipVertical)
	{
		Material material = GetMaterial("Process/Flip");
		material.SetFloat("_Horizontal", flipHorizontal.ToFloat());
		material.SetFloat("_Vertical", flipVertical.ToFloat());
		RenderTexture tempRenderTexture = GetTempRenderTexture(source.width, source.height);
		Graphics.Blit(source, tempRenderTexture, material);
		return tempRenderTexture;
	}
}
