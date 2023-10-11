using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
	private const string HUE_TEXTURE = "UI/Components/ColorPicker/hue";

	private const string ALPHA_TEXTURE = "UI/Components/ColorPicker/alpha";

	private const int H_RESOLUTION = 64;

	private const int SV_RESOLUTION = 32;

	private const int COMPONENT_MAP_WIDTH = 32;

	private const int COMPONENT_MAP_HEIGHT = 1;

	private const float MIN_SATURATION = 0.0001f;

	private const float MIN_VALUE = 0.0001f;

	public bool setStartColor;

	public Color startColor = new Color(1f, 1f, 1f, 1f);

	public Slider intensitySlider;

	public RawImage intensityImage;

	private Color _color = new Color(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);

	private Texture2D hMap;

	private Texture2D sMap;

	private Texture2D vMap;

	private Texture2D svMap;

	private Texture2D rMap;

	private Texture2D gMap;

	private Texture2D bMap;

	private Texture2D aMap;

	private Texture2D _iMap;

	private int frameOfLastSet = int.MinValue;

	[Header("Color")]
	public ColorEvent OnColorChange;

	public ColorEvent OnRGBChange;

	[Header("Color32")]
	public Color32Event OnColor32Change;

	public Color32Event OnColor32RBGChange;

	[Header("RGBA")]
	public FloatEvent OnRChange;

	public FloatEvent OnGChange;

	public FloatEvent OnBChange;

	public FloatEvent OnAChange;

	[Header("HSV")]
	public FloatEvent OnHueChange;

	public FloatEvent OnSaturationChange;

	public FloatEvent OnValueChange;

	public Vector2Event OnSVChange;

	[Header("Textures")]
	public Texture2DEvent OnRMapChange;

	public Texture2DEvent OnGMapChange;

	public Texture2DEvent OnBMapChange;

	public Texture2DEvent OnAMapChange;

	public Texture2DEvent OnHMapChange;

	public Texture2DEvent OnSMapChange;

	public Texture2DEvent OnVMapChange;

	public Texture2DEvent OnSVMapChange;

	private bool _initialized => sMap != null;

	private Texture2D iMap
	{
		get
		{
			Texture2D texture2D = _iMap;
			if ((object)texture2D == null)
			{
				RawImage rawImage = intensityImage;
				Texture2D obj = new Texture2D(32, 1, TextureFormat.RGB24, mipChain: false)
				{
					filterMode = FilterMode.Bilinear,
					anisoLevel = 16
				};
				Texture texture = obj;
				rawImage.texture = obj;
				texture2D = (_iMap = texture as Texture2D);
			}
			return texture2D;
		}
	}

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			if (!_initialized)
			{
				Awake();
			}
			int frameCount = Time.frameCount;
			if (frameOfLastSet != frameCount && !_color.Equals(value))
			{
				frameOfLastSet = frameCount;
				Color rgb = _color;
				Color32 color = _color;
				_color = value;
				Color32 color2 = _color;
				OnColorChange.Invoke(_color);
				if (!color.Equals(color2))
				{
					OnColor32Change.Invoke(color2);
				}
				bool flag = false;
				if (rgb.r != _color.r)
				{
					OnRChange.Invoke(_color.r);
					flag = true;
				}
				if (rgb.g != _color.g)
				{
					OnGChange.Invoke(_color.g);
					flag = true;
				}
				if (rgb.b != _color.b)
				{
					OnBChange.Invoke(_color.b);
					flag = true;
				}
				if (rgb.a != _color.a)
				{
					OnAChange.Invoke(_color.a);
				}
				if (flag)
				{
					Color color3 = _color;
					color3.a = 1f;
					_UpdateMap(rMap, color3, 0);
					_UpdateMap(gMap, color3, 1);
					_UpdateMap(bMap, color3, 2);
					OnRGBChange.Invoke(color3);
					OnColor32RBGChange.Invoke(color3);
					_UpdateIntensityMap();
				}
				Color obj = rgb.ToHSV();
				Color color4 = _color.ToHSV();
				bool flag3;
				bool flag2;
				bool flag4 = (flag3 = (flag2 = false));
				if (obj.r != color4.r)
				{
					_UpdateSVMap(color4.r);
					OnHueChange.Invoke(color4.r);
					flag4 = true;
				}
				if (obj.g != color4.g)
				{
					OnSaturationChange.Invoke(color4.g);
					flag3 = true;
				}
				if (obj.b != color4.b)
				{
					OnValueChange.Invoke(color4.b);
					flag2 = true;
				}
				if (flag3 || flag2)
				{
					_UpdateHMap(color4.g, color4.b);
					OnSVChange.Invoke(new Vector2(color4.g, color4.b));
				}
				if (flag4 || flag2)
				{
					_UpdateSMap(color4.r, color4.b);
				}
				if (flag4 || flag3)
				{
					_UpdateVMap(color4.r, color4.g);
				}
			}
		}
	}

	public float r
	{
		get
		{
			return _color.r;
		}
		set
		{
			color = new Color(value, _color.g, _color.b, _color.a);
		}
	}

	public float g
	{
		get
		{
			return _color.g;
		}
		set
		{
			color = new Color(_color.r, value, _color.b, _color.a);
		}
	}

	public float b
	{
		get
		{
			return _color.b;
		}
		set
		{
			color = new Color(_color.r, _color.g, value, _color.a);
		}
	}

	public float a
	{
		get
		{
			return _color.a;
		}
		set
		{
			color = new Color(_color.r, _color.g, _color.b, value);
		}
	}

	public float hue
	{
		get
		{
			return _color.ToHSV().r;
		}
		set
		{
			Color hsv = _color.ToHSV();
			hsv.r = value;
			color = hsv.ToRGB();
		}
	}

	public float saturation
	{
		get
		{
			return _color.ToHSV().g;
		}
		set
		{
			Color hsv = _color.ToHSV();
			hsv.g = Mathf.Max(0.0001f, value);
			color = hsv.ToRGB();
		}
	}

	public float value
	{
		get
		{
			return _color.ToHSV().b;
		}
		set
		{
			Color hsv = _color.ToHSV();
			hsv.b = Mathf.Max(0.0001f, value);
			color = hsv.ToRGB();
		}
	}

	public Vector2 saturationAndValue
	{
		get
		{
			Color color = _color.ToHSV();
			return new Vector2(color.g, color.b);
		}
		set
		{
			Color hsv = _color.ToHSV();
			hsv.g = Mathf.Max(0.0001f, value.x);
			hsv.b = Mathf.Max(0.0001f, value.y);
			color = hsv.ToRGB();
		}
	}

	private void Awake()
	{
		if (_initialized)
		{
			return;
		}
		_InitHMap();
		sMap = new Texture2D(32, 1, TextureFormat.RGB24, mipChain: false);
		_InitMap(sMap);
		_UpdateSMap(0f, 1f, triggerEvent: true);
		vMap = new Texture2D(1, 32, TextureFormat.RGB24, mipChain: false);
		_InitMap(vMap);
		_UpdateVMap(0f, 0f, triggerEvent: true);
		svMap = new Texture2D(32, 32, TextureFormat.RGB24, mipChain: false);
		_InitMap(svMap);
		_UpdateSVMap(0f, triggerSVMapChangeEvent: true);
		rMap = new Texture2D(32, 1, TextureFormat.RGB24, mipChain: false);
		_InitMap(rMap);
		_UpdateMap(rMap, _color, 0, OnRMapChange);
		gMap = new Texture2D(32, 1, TextureFormat.RGB24, mipChain: false);
		_InitMap(gMap);
		_UpdateMap(gMap, _color, 1, OnGMapChange);
		bMap = new Texture2D(32, 1, TextureFormat.RGB24, mipChain: false);
		_InitMap(bMap);
		_UpdateMap(bMap, _color, 2, OnBMapChange);
		aMap = new Texture2D(32, 1, TextureFormat.ARGB32, mipChain: false);
		_InitMap(aMap);
		_UpdateMap(aMap, _color, 3, OnAMapChange);
		if ((bool)intensityImage)
		{
			intensitySlider.onValueChanged.AddListener(delegate
			{
				_UpdateIntensityMap();
			});
			_UpdateIntensityMap();
		}
	}

	private void Start()
	{
		if (setStartColor)
		{
			SetColor(startColor);
		}
	}

	private void _InitMap(Texture2D map)
	{
		map.wrapMode = TextureWrapMode.Clamp;
		map.filterMode = FilterMode.Bilinear;
		map.anisoLevel = 16;
	}

	private void _InitHMap()
	{
		hMap = new Texture2D(1, 64, TextureFormat.RGB24, mipChain: false);
		_InitMap(hMap);
		_UpdateHMap(0f, 1f, triggerHMapChangeEvent: true);
	}

	public void SetColor(Color color)
	{
		frameOfLastSet = int.MinValue;
		this.color = color;
	}

	public void SetColor(object obj)
	{
		SetColor((Color)obj);
	}

	public void SetColor32(Color32 color32)
	{
		SetColor(color32);
	}

	private void _UpdateMap(Texture2D map, Color rgb, int index, Texture2DEvent e = null)
	{
		Color minMap = rgb;
		minMap[index] = 0f;
		Color maxMap = rgb;
		maxMap[index] = 1f;
		if (index == 3)
		{
			minMap = Color.black;
			minMap.a = 0f;
			maxMap = Color.white;
		}
		map.Map((Vector2 v) => minMap.Lerp(maxMap, v.x));
		e?.Invoke(map);
	}

	private void _UpdateHMap(float saturation, float value, bool triggerHMapChangeEvent = false)
	{
		hMap.Map((Vector2 v) => new Color(v.y, saturation, value).ToRGB());
		if (triggerHMapChangeEvent)
		{
			OnHMapChange.Invoke(hMap);
		}
	}

	private void _UpdateSVMap(float hue, bool triggerSVMapChangeEvent = false)
	{
		svMap.Map((Vector2 v) => new Color(hue, v.x, v.y).ToRGB());
		if (triggerSVMapChangeEvent)
		{
			OnSVMapChange.Invoke(svMap);
		}
	}

	private void _UpdateSMap(float hue, float value, bool triggerEvent = false)
	{
		sMap.Map((Vector2 v) => new Color(hue, v.x, value).ToRGB());
		if (triggerEvent)
		{
			OnSMapChange.Invoke(sMap);
		}
	}

	private void _UpdateVMap(float hue, float saturation, bool triggerEvent = false)
	{
		vMap.Map((Vector2 v) => new Color(hue, saturation, v.y).ToRGB());
		if (triggerEvent)
		{
			OnVMapChange.Invoke(vMap);
		}
	}

	private void _UpdateIntensityMap()
	{
		if ((bool)intensityImage)
		{
			iMap.Map((Vector2 p) => (!(p.x < 0.5f)) ? color.Lerp(Color.white, (p.x - 0.5f) * 2f, includeAlpha: false) : Color.black.Lerp(color, p.x + p.x, includeAlpha: false));
		}
	}
}
