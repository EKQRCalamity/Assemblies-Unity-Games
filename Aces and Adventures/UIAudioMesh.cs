using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIAudioMesh : MaskableGraphic
{
	public const int MIN_RESOLUTION = 50;

	public const int DEFAULT_RESOLUTION = 400;

	public const int MAX_RESOLUTION = 1000;

	public const int MIN_SAMPLE_RATE = 1;

	public const int DEFAULT_SAMPLE_RATE = 20;

	public const int MAX_SAMPLE_RATE = 100;

	public const float MIN_BAR_WIDTH = 0.1f;

	public const float DEFAULT_BAR_WIDTH = 0.5f;

	public const float MAX_BAR_WIDTH = 1f;

	public Texture2D texture;

	[Header("Audio")]
	[SerializeField]
	private AudioClip _clip;

	[Range(50f, 1000f)]
	[SerializeField]
	private int _resolution = 400;

	[SerializeField]
	[Range(0.1f, 1f)]
	private float _barWidth = 0.5f;

	[SerializeField]
	[Range(1f, 100f)]
	private int _sampleRate = 20;

	[SerializeField]
	[HideInInspector]
	private Vector2[] _unitizedVertexPositionCache;

	[SerializeField]
	[ShowOnly]
	private Vector2 _displayRange;

	private Vector2 _displayRangeUsedForVertexCache;

	[SerializeField]
	[HideInInspector]
	private bool _persistVertexCacheOnSetDirty;

	public AudioClip clip
	{
		get
		{
			return _clip;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _clip, value))
			{
				color = color.SetAlpha(1f);
				_SetDirty();
				_displayRange = new Vector2(0f, _clip.length);
			}
		}
	}

	public int resolution
	{
		get
		{
			return _resolution;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _resolution, value))
			{
				_SetDirty();
			}
		}
	}

	public float barWidth
	{
		get
		{
			return _barWidth;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _barWidth, value))
			{
				_SetDirty();
			}
		}
	}

	public int sampleRate
	{
		get
		{
			return _sampleRate;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _sampleRate, value))
			{
				_SetDirty();
			}
		}
	}

	public Vector2 displayRange
	{
		get
		{
			return _displayRange;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _displayRange, value))
			{
				_SetDirty();
			}
		}
	}

	public override Texture mainTexture => texture;

	public static void GetUnitizedBarGraph(ref Vector2[] positions, AudioClip clip, int resolutionPerChannel = 400, float barWidth = 0.5f, int sampleRate = 20, Vector2? normalizedDisplayRange = null)
	{
		GetUnitizedBarGraph(ref positions, clip.GetAllSamples(), clip.channels, resolutionPerChannel, barWidth, sampleRate, normalizedDisplayRange);
	}

	public static void GetUnitizedBarGraph(ref Vector2[] positions, float[] samples, int numberOfChannels, int resolutionPerChannel = 400, float barWidth = 0.5f, int sampleRate = 20, Vector2? normalizedDisplayRange = null)
	{
		if (normalizedDisplayRange.HasValue)
		{
			int num = Mathf.FloorToInt(normalizedDisplayRange.Value.x * (float)samples.Length);
			int num2 = Mathf.FloorToInt(normalizedDisplayRange.Value.y * (float)samples.Length);
			samples = samples.ToList().GetRange(num, num2 - num).ToArray();
		}
		Rect rect = new Rect(Vector2.zero, Vector2.one);
		if (resolutionPerChannel * 4 * numberOfChannels > 65000)
		{
			resolutionPerChannel = 65000 / numberOfChannels / 4 - 1;
		}
		sampleRate = Math.Max(numberOfChannels, sampleRate);
		int num3 = Math.Max(1, Mathf.CeilToInt((float)samples.Length / ((float)resolutionPerChannel * (float)sampleRate)));
		resolutionPerChannel = Mathf.RoundToInt((float)samples.Length / ((float)num3 * (float)sampleRate));
		int num4 = resolutionPerChannel * numberOfChannels * 4;
		positions = ((positions == null || num4 != positions.Length) ? new Vector2[num4] : positions);
		float num5 = 1f / (float)samples.Length;
		Vector2 size = new Vector2(rect.size.x, (0f - rect.size.y) / (float)numberOfChannels);
		Vector2 vector = new Vector2(0f, size.y);
		int num6 = 0;
		float num7 = 0.5f / (float)resolutionPerChannel * rect.size.x * barWidth;
		for (int i = 0; i < numberOfChannels; i++)
		{
			Rect rect2 = new Rect(new Vector2(rect.xMin, rect.yMax) + i * vector, size);
			float xMin = rect2.xMin;
			float xMax = rect2.xMax;
			float yMin = rect2.yMin;
			float yMax = rect2.yMax;
			float num8 = 1f;
			float num9 = -1f;
			int num10 = 0;
			for (int j = i; j < samples.Length; j += sampleRate)
			{
				float val = samples[j];
				num8 = Math.Min(num8, val);
				num9 = Math.Max(num9, val);
				num10++;
				if (num10 % num3 == 0)
				{
					float y = Mathf.Lerp(yMax, yMin, num8 * 0.5f + 0.5f);
					float y2 = Mathf.Lerp(yMax, yMin, num9 * 0.5f + 0.5f);
					float num11 = Mathf.Lerp(xMin, xMax, (float)j * num5);
					float x = num11 - num7;
					float x2 = num11 + num7;
					positions[num6++] = new Vector2(x, y);
					positions[num6++] = new Vector2(x, y2);
					positions[num6++] = new Vector2(x2, y2);
					positions[num6++] = new Vector2(x2, y);
					num8 = 1f;
					num9 = -1f;
					num10 = 0;
					if (num6 >= positions.Length)
					{
						return;
					}
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (_clip == null && _unitizedVertexPositionCache.IsNullOrEmpty())
		{
			color = color.SetAlpha(0f);
		}
	}

	private void _UpdateUnitizedVertexPositions()
	{
		if ((_unitizedVertexPositionCache == null || _unitizedVertexPositionCache.Length == 0) && !(_clip == null))
		{
			_displayRangeUsedForVertexCache = _displayRange;
			float length = _clip.length;
			float num = _displayRange.Range() / length;
			GetUnitizedBarGraph(ref _unitizedVertexPositionCache, _clip, _resolution, _barWidth, (int)Math.Max(1.0, Math.Ceiling((float)_sampleRate * num)), (num == 1f) ? null : new Vector2?(new Vector2(_displayRange.x / length, _displayRange.y / length)));
		}
	}

	private void _SetDirty()
	{
		if (!_persistVertexCacheOnSetDirty)
		{
			_unitizedVertexPositionCache = null;
		}
		SetVerticesDirty();
	}

	public void SetClip(AudioClip clip)
	{
		this.clip = clip;
	}

	public void SetVertexData(Vector2[] vertexPositions)
	{
		_unitizedVertexPositionCache = vertexPositions;
		_persistVertexCacheOnSetDirty = true;
		color = color.SetAlpha(1f);
		SetVerticesDirty();
	}

	public void SetResolution(int resolution)
	{
		this.resolution = resolution;
	}

	public void SetBarWidth(float barWidth)
	{
		this.barWidth = barWidth;
	}

	public void SetSampleRate(int sampleRate)
	{
		this.sampleRate = sampleRate;
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if (_clip == null && _unitizedVertexPositionCache.IsNullOrEmpty())
		{
			base.OnPopulateMesh(vh);
			return;
		}
		vh.Clear();
		_UpdateUnitizedVertexPositions();
		Rect pixelAdjustedRect = GetPixelAdjustedRect();
		Vector2 min = pixelAdjustedRect.min;
		Vector2 max = pixelAdjustedRect.max;
		for (int i = 0; i < _unitizedVertexPositionCache.Length; i++)
		{
			Vector2 vector = _unitizedVertexPositionCache[i];
			vh.AddVert(new Vector3(Mathf.Lerp(min.x, max.x, vector.x), Mathf.Lerp(min.y, max.y, vector.y)), color, vector);
		}
		int num = _unitizedVertexPositionCache.Length - 3;
		for (int j = 0; j < num; j += 4)
		{
			vh.AddTriangle(j, j + 1, j + 2);
			vh.AddTriangle(j, j + 2, j + 3);
		}
	}
}
