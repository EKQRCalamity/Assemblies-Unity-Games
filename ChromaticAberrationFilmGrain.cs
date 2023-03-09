using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Other/Chromatic Aberration Film Grain")]
public class ChromaticAberrationFilmGrain : PostEffectsBase
{
	public Shader shader;

	private Material material;

	private const float FRAME_TIME = 0.025f;

	private Vector4 UV_Transform = new Vector4(1f, 0f, 0f, 1f);

	public float intensity = 1f;

	public bool animated = true;

	public int earlyLoopPoint = 102;

	private int currentTexture;

	public Vector2 r;

	public Vector2 g;

	public Vector2 b;

	private Texture2D[] textures;

	private Vector2 rStart;

	private Vector2 gStart;

	private Vector2 bStart;

	public void Initialize(Texture2D[] filmGrain)
	{
		base.enabled = true;
		rStart = r;
		gStart = g;
		bStart = b;
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		textures = filmGrain;
		StartCoroutine(animate_cr());
	}

	private IEnumerator animate_cr()
	{
		float t = 0f;
		int loopsUntilFullLoop = Random.Range(7, 15);
		while (true)
		{
			t += Time.deltaTime;
			while (t > 0.025f)
			{
				t -= 0.025f;
				if (!animated)
				{
					continue;
				}
				currentTexture++;
				if (loopsUntilFullLoop > 0)
				{
					if (currentTexture >= earlyLoopPoint)
					{
						currentTexture = 0;
						loopsUntilFullLoop--;
						UV_Transform = new Vector4(MathUtils.PlusOrMinus(), 0f, 0f, MathUtils.PlusOrMinus());
					}
				}
				else if (currentTexture >= textures.Length)
				{
					currentTexture = 0;
					loopsUntilFullLoop = Random.Range(7, 15);
					UV_Transform = new Vector4(MathUtils.PlusOrMinus(), 0f, 0f, MathUtils.PlusOrMinus());
				}
			}
			yield return null;
		}
	}

	public override bool CheckResources()
	{
		CheckSupport(needDepth: false);
		material = CheckShaderAndCreateMaterial(shader, material);
		if (!isSupported)
		{
			ReportAutoDisable();
		}
		return isSupported;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckResources())
		{
			Graphics.Blit(source, destination);
			return;
		}
		material.SetVector("_UV_Transform", UV_Transform);
		material.SetFloat("_Intensity", intensity);
		if (textures != null && textures.Length > currentTexture && textures[currentTexture] != null)
		{
			material.SetTexture("_Overlay", textures[currentTexture]);
		}
		float num = (float)source.width / (float)source.height;
		float num2 = ((!(num < 1.7777778f)) ? 1f : (num / 1.7777778f));
		num2 *= 1f - 0.1f * SettingsData.Data.overscan;
		float num3 = SettingsData.Data.chromaticAberration * num2 * (float)source.height / 1080f;
		Vector2 vector = r * num3;
		Vector2 vector2 = g * num3;
		Vector2 vector3 = b * num3;
		if (SettingsData.Data.filter == BlurGamma.Filter.TwoStrip)
		{
			Vector2 vector4 = vector3 * 0.4f + vector2 * 0.6f;
			vector2 = vector4;
		}
		material.SetVector("_Screen", new Vector2(source.width, source.height));
		material.SetVector("_Red", vector);
		material.SetVector("_Green", vector2);
		material.SetVector("_Blue", vector3);
		int num4 = 0;
		switch (SettingsData.Data.filter)
		{
		case BlurGamma.Filter.TwoStrip:
			num4++;
			break;
		case BlurGamma.Filter.BW:
			num4 += 2;
			break;
		}
		Graphics.Blit(source, destination, material, num4);
	}

	protected virtual void OnDisable()
	{
		if ((bool)material)
		{
			Object.DestroyImmediate(material);
		}
	}

	public void PsychedelicEffect(float amount, float speed, float time)
	{
		StartCoroutine(psychedelic_effect(amount, speed, time));
	}

	private IEnumerator psychedelic_effect(float amount, float speed, float time)
	{
		float t = 0f;
		float slowdownTime = 0.5f;
		while (amount > 0f)
		{
			t += Time.deltaTime;
			float angle = speed * t;
			float phase = Mathf.Sin(angle) * amount;
			r = Vector2.up * phase;
			g = Vector2.up * phase / 2f;
			b = Vector2.down * phase;
			if (t >= time)
			{
				amount -= slowdownTime;
			}
			yield return null;
		}
		r = rStart;
		g = gStart;
		b = bStart;
		yield return null;
	}
}
