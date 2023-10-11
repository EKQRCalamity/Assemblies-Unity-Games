using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIGlow : MaskableGraphic
{
	private const float MIN_MAGNITUDE_MULTIPLIER = 0.001f;

	private static bool Initialized;

	private static Material UIGlowMaterialBlueprint;

	public static int DistortionMap_ID;

	public static int Frequency_ID;

	public static int Magnitude_ID;

	public static int AlphaPower_ID;

	public static int SpeedX_ID;

	public static int SpeedY_ID;

	public static int ColorLerp_ID;

	public static int FadeOutPower_ID;

	public static int UFadeDirection_ID;

	public static int UAlphaFade_ID;

	public static int UMultiplier_ID;

	public static int VAlphaFade_ID;

	public static int VMultiplier_ID;

	public static int BrightnessMax_ID;

	public static int BrightnessPower_ID;

	public static int TintAlphaPower_ID;

	public static int UnscaledTime_ID;

	public static int TimeOffset_ID;

	public static int MainTexAspect_ID;

	[Header("Image")]
	[SerializeField]
	protected Texture2D _distortionMapOverride;

	[SerializeField]
	protected bool _useStandardImageRendering;

	[SerializeField]
	protected bool _preserveAspect;

	[SerializeField]
	[FormerlySerializedAs("sprite")]
	protected Sprite _sprite;

	[Range(0.01f, 2f)]
	[SerializeField]
	protected float _scale = 1f;

	[Header("Non Standard Image Rendering Only")]
	[SerializeField]
	protected Color32 _outerColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);

	[Header("Distortion")]
	[Range(0.01f, 1f)]
	[SerializeField]
	protected float _frequency = 0.035f;

	[Range(0.0001f, 1f)]
	[SerializeField]
	protected float _magnitude = 0.25f;

	[Range(0.01f, 6f)]
	[SerializeField]
	protected float _alphaPower = 2f;

	[SerializeField]
	protected Vector2 _uvShift = new Vector2(2.21f, -6.81f);

	[Header("Tint And Alpha")]
	[Range(0f, 1f)]
	[SerializeField]
	protected float _colorLerp = 1f;

	[Range(0f, 6f)]
	[SerializeField]
	protected float _fadeOutPower = 3f;

	[Range(0f, 6f)]
	[SerializeField]
	protected float _tintAlphaPower = 1f;

	[Header("U Fade")]
	[Range(0f, 1f)]
	[SerializeField]
	protected float _uFadeDirection;

	[Range(0f, 6f)]
	[SerializeField]
	protected float _uAlphaFade;

	[Range(1f, 6f)]
	[SerializeField]
	protected float _uMultiplier = 2f;

	[Header("V Fade")]
	[Range(0f, 6f)]
	[SerializeField]
	protected float _vAlphaFade = 6f;

	[Range(1f, 6f)]
	[SerializeField]
	protected float _vMultiplier = 3f;

	[Header("Brightness")]
	[Range(0f, 1f)]
	[SerializeField]
	protected float _brightnessMax = 1f;

	[Range(0f, 12f)]
	[SerializeField]
	protected float _brightnessPower = 6f;

	[Header("Multipliers")]
	[SerializeField]
	[Range(0.001f, 2f)]
	protected float _magnitudeMultiplier = 1f;

	public Texture2D distortionMapOverride
	{
		get
		{
			return _distortionMapOverride;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _distortionMapOverride, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public bool useStandardImageRendering
	{
		get
		{
			return _useStandardImageRendering;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _useStandardImageRendering, value))
			{
				this.SetMaterialAndVerticesDirty();
			}
		}
	}

	public bool preserveAspect
	{
		get
		{
			return _preserveAspect;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _preserveAspect, value))
			{
				this.SetMaterialAndVerticesDirty();
			}
		}
	}

	public Sprite sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _sprite, value))
			{
				this.SetMaterialAndVerticesDirty();
			}
		}
	}

	public float scale
	{
		get
		{
			return _scale;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _scale, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public Color32 outerColor
	{
		get
		{
			return _outerColor;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _outerColor, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public float frequency
	{
		get
		{
			return _frequency;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _frequency, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float magnitude
	{
		get
		{
			return _magnitude;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _magnitude, value))
			{
				this.SetMaterialAndVerticesDirty();
			}
		}
	}

	public float alphaPower
	{
		get
		{
			return _alphaPower;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _alphaPower, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public Vector2 uvShift
	{
		get
		{
			return _uvShift;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _uvShift, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float colorLerp
	{
		get
		{
			return _colorLerp;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _colorLerp, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float fadeOutPower
	{
		get
		{
			return _fadeOutPower;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _fadeOutPower, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float tintAlphaPower
	{
		get
		{
			return _tintAlphaPower;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _tintAlphaPower, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float uFadeDirection
	{
		get
		{
			return _uFadeDirection;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _uFadeDirection, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float uAlphaFade
	{
		get
		{
			return _uAlphaFade;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _uAlphaFade, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float uMultiplier
	{
		get
		{
			return _uMultiplier;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _uMultiplier, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float vAlphaFade
	{
		get
		{
			return _vAlphaFade;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _vAlphaFade, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float vMultiplier
	{
		get
		{
			return _vMultiplier;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _vMultiplier, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float brightnessMax
	{
		get
		{
			return _brightnessMax;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _brightnessMax, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float brightnessPower
	{
		get
		{
			return _brightnessPower;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _brightnessPower, value))
			{
				SetMaterialDirty();
			}
		}
	}

	public float magnitudeMultiplier
	{
		get
		{
			return _magnitudeMultiplier;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _magnitudeMultiplier, Mathf.Max(0.001f, value)))
			{
				this.SetMaterialAndVerticesDirty();
			}
		}
	}

	public override bool raycastTarget
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public override Texture mainTexture
	{
		get
		{
			if (!(sprite != null))
			{
				if (!(material != null) || !(material.mainTexture != null))
				{
					return Graphic.s_WhiteTexture;
				}
				return material.mainTexture;
			}
			return sprite.texture;
		}
	}

	private static void Initialize()
	{
		if (!Initialized)
		{
			Initialized = true;
			UIGlowMaterialBlueprint = Resources.Load<Material>("UI/Materials/UIGlow");
			DistortionMap_ID = Shader.PropertyToID("_DistortionTex");
			Frequency_ID = Shader.PropertyToID("_DistortionFrequency");
			Magnitude_ID = Shader.PropertyToID("_DistortionMagnitude");
			AlphaPower_ID = Shader.PropertyToID("_DistortionAlphaPower");
			SpeedX_ID = Shader.PropertyToID("_DistortionSpeedX");
			SpeedY_ID = Shader.PropertyToID("_DistortionSpeedY");
			ColorLerp_ID = Shader.PropertyToID("_ColorLerp");
			FadeOutPower_ID = Shader.PropertyToID("_FadeOutPower");
			UFadeDirection_ID = Shader.PropertyToID("_UFadeDirection");
			UAlphaFade_ID = Shader.PropertyToID("_UAlphaFade");
			UMultiplier_ID = Shader.PropertyToID("_UMultiplier");
			VAlphaFade_ID = Shader.PropertyToID("_VAlphaFade");
			VMultiplier_ID = Shader.PropertyToID("_VMultiplier");
			BrightnessMax_ID = Shader.PropertyToID("_BrightnessMax");
			BrightnessPower_ID = Shader.PropertyToID("_BrightnessPower");
			TintAlphaPower_ID = Shader.PropertyToID("_TintAlphaPower");
			UnscaledTime_ID = Shader.PropertyToID("_UnscaledTime");
			TimeOffset_ID = Shader.PropertyToID("_TimeOffset");
			MainTexAspect_ID = Shader.PropertyToID("_MainTexAspect");
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Initialize();
		material = Object.Instantiate(UIGlowMaterialBlueprint);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		material.SetFloat(TimeOffset_ID, Random.value * 10000f);
	}

	private void Update()
	{
		material.SetFloat(UnscaledTime_ID, Time.unscaledTime * 0.05f);
	}

	private void _AddCornerVerts(VertexHelper vh, Rect rect, Vector2 offset, Vector2 corner)
	{
		Vector2 vector = rect.Lerp(corner);
		Vector2 multiplier = (corner - new Vector2(0.5f, 0.5f)) * -2f;
		Vector2 vector2 = offset.Multiply(multiplier);
		vh.AddVert(vector + vector2, color, corner + vector2.Multiply((rect.size * 0.5f + vector2.Abs()).Inverse()));
		vh.AddVert(vector - vector2, outerColor, corner);
	}

	protected override void UpdateMaterial()
	{
		base.UpdateMaterial();
		if ((bool)distortionMapOverride && (bool)material)
		{
			material.SetTexture(DistortionMap_ID, distortionMapOverride);
		}
		material.SetFloat(Frequency_ID, frequency);
		material.SetFloat(Magnitude_ID, magnitude * (useStandardImageRendering ? 1f : 0.5f) * magnitudeMultiplier);
		material.SetFloat(AlphaPower_ID, alphaPower);
		material.SetFloat(SpeedX_ID, uvShift.x);
		material.SetFloat(SpeedY_ID, uvShift.y);
		material.SetFloat(ColorLerp_ID, colorLerp);
		material.SetFloat(FadeOutPower_ID, fadeOutPower);
		material.SetFloat(UFadeDirection_ID, uFadeDirection);
		material.SetFloat(UAlphaFade_ID, uAlphaFade);
		material.SetFloat(UMultiplier_ID, uMultiplier);
		material.SetFloat(VAlphaFade_ID, vAlphaFade);
		material.SetFloat(VMultiplier_ID, vMultiplier);
		material.SetFloat(BrightnessMax_ID, brightnessMax);
		material.SetFloat(BrightnessPower_ID, brightnessPower);
		material.SetFloat(TintAlphaPower_ID, tintAlphaPower);
		material.SetFloat(MainTexAspect_ID, (sprite != null) ? sprite.rect.size.AspectRatio() : 1f);
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		Rect rect = base.rectTransform.rect;
		float num = magnitude * magnitudeMultiplier;
		if (magnitudeMultiplier <= 0.001f)
		{
			vh.Clear();
			return;
		}
		if (useStandardImageRendering && (bool)sprite)
		{
			vh.GenerateSlicedSprite(this, sprite, rect.ExtrudeFromCenter(1f + num * scale * 2f), fillCenter: true, preserveAspect);
			return;
		}
		vh.Clear();
		rect = rect.ExtrudeFromCenter(1f + num * scale * 0.5f);
		Vector2 offset = (rect.max - rect.center) * num * 0.5f;
		_AddCornerVerts(vh, rect, offset, new Vector2(0f, 0f));
		_AddCornerVerts(vh, rect, offset, new Vector2(0f, 1f));
		_AddCornerVerts(vh, rect, offset, new Vector2(1f, 1f));
		_AddCornerVerts(vh, rect, offset, new Vector2(1f, 0f));
		for (int i = 0; i < 4; i++)
		{
			int num2 = i + i;
			int num3 = num2 + 1;
			int idx = (num2 + 2) % 8;
			vh.AddTriangle(num2, num3, idx);
			vh.AddTriangle(num3, (num2 + 3) % 8, idx);
		}
	}

	public void SetColors(Color32 color)
	{
		this.color = color;
		outerColor = color;
	}

	public void SetRGBs(Color rgb)
	{
		color = rgb.SetAlpha(color.a);
		outerColor = rgb.SetAlpha(outerColor.Alpha());
	}

	public void SetAlphas(float alpha)
	{
		color = color.SetAlpha(alpha);
		outerColor = outerColor.SetAlpha(alpha);
	}
}
