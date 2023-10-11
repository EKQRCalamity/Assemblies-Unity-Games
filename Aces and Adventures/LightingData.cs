using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Video;

[ProtoContract]
[UIField]
public class LightingData : IDataContent
{
	[ProtoContract]
	[UIField]
	public class SpotlightData
	{
		private const int SPOT_INTENSITY = 15000;

		private const int CONE_ANGLE = 30;

		private const float DEFAULT_RANGE = 5f;

		private const float MAX_RANGE = 15f;

		private const int PITCH = 60;

		private const float POSITION_OFFSET_MAX = 0.2f;

		private const float ROTATOIN_OFFSET_MAX = 45f;

		[ProtoMember(1)]
		[UIField(validateOnChange = true)]
		private bool _enabled;

		[ProtoMember(2)]
		[UIField(min = -180, max = 180)]
		[DefaultValue(60)]
		[UIHideIf("_hideCommon")]
		private int _xRotation = 60;

		[ProtoMember(3)]
		[UIField(min = -180, max = 180)]
		[UIHideIf("_hideCommon")]
		private int _yRotation;

		[ProtoMember(4)]
		[UIField(min = -180, max = 180)]
		[UIHideIf("_hideCommon")]
		private int _zRotation;

		[ProtoMember(5, DataFormat = DataFormat.FixedSize)]
		[UIField(min = 0, max = 130000, view = "UI/Input Field Standard")]
		[UIHideIf("_hideCommon")]
		[DefaultValue(15000)]
		private int _intensity = 15000;

		[ProtoMember(6)]
		[UIField]
		[UIHideIf("_hideCommon")]
		private Color32 _filter = FILTER;

		[ProtoMember(7)]
		[UIField(min = 1, max = 45)]
		[UIHideIf("_hideCommon")]
		[DefaultValue(30)]
		private int _coneAngle = 30;

		[ProtoMember(8)]
		[UIField(min = 0, max = 100)]
		[UIHideIf("_hideCommon")]
		private int _innerConeAnglePercentage;

		[ProtoMember(9)]
		[UIField(min = 0.75f, max = 10f)]
		[UIHideIf("_hideCommon")]
		[DefaultValue(1f)]
		private float _distanceToTable = 1f;

		[ProtoMember(10)]
		[UIField(min = 1f, max = 15f)]
		[UIHideIf("_hideCommon")]
		[DefaultValue(5f)]
		private float _range = 5f;

		[ProtoMember(15)]
		[UIField]
		[DefaultValue(true)]
		[UIHideIf("_hideCommon")]
		private bool _shadows = true;

		[ProtoMember(11)]
		[UIField(validateOnChange = true)]
		[UIHideIf("_hideCommon")]
		private bool _enableAnimation;

		[ProtoMember(12)]
		[UIField]
		[UIHideIf("_hideAnimation")]
		[UIHeader("Animation")]
		[UIDeepValueChange]
		private NoiseWaveVector3Data _positionAnimation = new NoiseWaveVector3Data(new NoiseWaveFloatData(0f, new RangeF(0f, 0f, -0.2f, 0.2f), 0f, new RangeF(0f, 0f, -0.2f, 0.2f)));

		[ProtoMember(13)]
		[UIField]
		[UIHideIf("_hideAnimation")]
		[UIDeepValueChange]
		private NoiseWaveVector3Data _rotationAnimation = new NoiseWaveVector3Data(new NoiseWaveFloatData(0f, new RangeF(0f, 0f, -45f, 45f), 0f, new RangeF(0f, 0f, -45f, 45f)));

		[ProtoMember(14)]
		[UIField]
		[UIHideIf("_hideAnimation")]
		[UIDeepValueChange]
		private NoiseWaveFloatData _intensityAnimation = new NoiseWaveFloatData(0f, new RangeF(1f, 1f, 0f, 1.5f), 0f, new RangeF(1f, 1f, 0f, 1.5f));

		public Quaternion rotation => Quaternion.Euler(_xRotation, _yRotation, _zRotation);

		public int intensity
		{
			get
			{
				if (!_enabled)
				{
					return 0;
				}
				return _intensity;
			}
		}

		public Color32 filter => _filter;

		public int coneAngle => _coneAngle;

		public int innerConeAnglePercent => _innerConeAnglePercentage;

		public float distance => _distanceToTable;

		public float range => _range;

		public bool shadows => _shadows;

		public bool animationEnabled => _enableAnimation;

		public NoiseWaveVector3Data positionAnimation => _positionAnimation;

		public NoiseWaveVector3Data rotationAnimation => _rotationAnimation;

		public NoiseWaveFloatData intensityAnimation => _intensityAnimation;

		private bool _filterSpecified => !_filter.EqualTo(FILTER);

		private bool _positionAnimationSpecified => _enableAnimation;

		private bool _rotationAnimationSpecified => _enableAnimation;

		private bool _intensityAnimationSpecified => _enableAnimation;

		private bool _hideCommon => !_enabled;

		private bool _hideAnimation
		{
			get
			{
				if (!_hideCommon)
				{
					return !_enableAnimation;
				}
				return true;
			}
		}

		public static implicit operator bool(SpotlightData s)
		{
			return s?._enabled ?? false;
		}
	}

	[ProtoContract]
	[UIField]
	public class FogData
	{
		[ProtoMember(1)]
		[UIField(validateOnChange = true)]
		private bool _enabled;

		[ProtoMember(2)]
		[UIField(min = 0.01f, max = 1f)]
		[DefaultValue(0.25f)]
		[UIHideIf("_hideCommon")]
		private float _density = 0.25f;

		[ProtoMember(3)]
		[UIField]
		[UIHideIf("_hideCommon")]
		private Color32 _filter = FILTER;

		public float density => _density;

		public Color32 filter => _filter;

		private bool _filterSpecified => !_filter.EqualTo(FILTER);

		private bool _hideCommon => !_enabled;

		public static implicit operator bool(FogData d)
		{
			return d?._enabled ?? false;
		}
	}

	public const float X_ROTATION = 89f;

	public const float Y_ROTATION = 0f;

	public const float Z_ROTATION = 0f;

	public static readonly Color32 FILTER = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public const int INTENSITY = 75000;

	[ProtoMember(1)]
	[UIField]
	private string _name;

	[ProtoMember(2)]
	[UIField(min = -180, max = 180)]
	[DefaultValue(89f)]
	[UIHeader("Rotation")]
	private float _xRotation = 89f;

	[ProtoMember(3)]
	[UIField(min = -180, max = 180)]
	[DefaultValue(0f)]
	private float _yRotation;

	[ProtoMember(4)]
	[UIField(min = -180, max = 180)]
	[DefaultValue(0f)]
	private float _zRotation;

	[ProtoMember(5)]
	[UIField]
	[UIHeader("Color")]
	private Color32 _filter = FILTER;

	[ProtoMember(6, DataFormat = DataFormat.FixedSize)]
	[UIField(min = 0, max = 200000, view = "UI/Input Field Standard")]
	[DefaultValue(75000)]
	private int _intensity = 75000;

	[ProtoMember(11)]
	[UIField(min = 0.25f, max = 2f)]
	[DefaultValue(1f)]
	private float _indirectLightingMultiplier = 1f;

	[ProtoMember(7)]
	[UIField(validateOnChange = true)]
	private CookieClipType? _cookie;

	[ProtoMember(8)]
	[UIField(min = 0.1f, max = 10f)]
	[DefaultValue(1f)]
	[UIHideIf("_hideCookie")]
	private float _cookieIntensity = 1f;

	[ProtoMember(9)]
	[UIField(min = 1f, max = 10f)]
	[DefaultValue(1f)]
	[UIHideIf("_hideCookie")]
	private float _cookiePlaybackSpeed = 1f;

	[ProtoMember(12)]
	[UIField]
	[UIHideIf("_hideCookie")]
	private bool _playOnce;

	[ProtoMember(13)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHeader("Fog")]
	private FogData _fog;

	[ProtoMember(10)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIMargin(24f, false)]
	[UIHeader("Spotlight")]
	private SpotlightData _spotLight;

	public Quaternion rotation => Quaternion.Euler(_xRotation, _yRotation, _zRotation);

	public Color32 filter => _filter;

	public int intensity => _intensity;

	public float indirectLightingMultiplier => _indirectLightingMultiplier;

	public VideoClip cookieClip => _cookie.HasValue ? EnumUtil<CookieClipType>.GetResource<VideoClipRef>(_cookie.Value) : null;

	public float cookieIntensity => _cookieIntensity;

	public float cookiePlaybackSpeed => _cookiePlaybackSpeed;

	public bool loopCookie => !_playOnce;

	public SpotlightData spotlight => _spotLight;

	public FogData fog => _fog;

	[ProtoMember(15)]
	public string tags { get; set; }

	private bool _filterSpecified => !_filter.EqualTo(FILTER);

	private bool _spotLightSpecified => _spotLight;

	private bool _fogSpecified => _fog;

	private bool _hideCookie => !_cookie.HasValue;

	public string GetTitle()
	{
		if (!_name.HasVisibleCharacter())
		{
			return "Unnamed Lighting Data";
		}
		return _name;
	}

	public string GetAutomatedDescription()
	{
		return null;
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
		ProjectileMediaViewTester.Instance?.UpdateLighting();
	}

	public string GetSaveErrorMessage()
	{
		if (!_name.HasVisibleCharacter())
		{
			return "Please give lighting data a name before saving.";
		}
		return null;
	}

	public void OnLoadValidation()
	{
	}
}
