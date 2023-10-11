using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Events;

[UIField("Find Alpha Shape", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
public class FindAlphaShapeControl : MonoBehaviour
{
	private const string ADVANCED = "Advanced Settings";

	private const string IMAGE = "Advanced Settings|Image Processing";

	private const string MESH = "Advanced Settings|Mesh Generation";

	private const string BOUNDARIES = "Advanced Settings|Boundaries";

	private const float MIN_EDGE_THRESHOLD = 0.01f;

	private const float MAX_EDGE_THRESHOLD = 1f;

	private const float DEFAULT_BLUR = 1.5f;

	private const float DEFAULT_COMPLEXITY = 1f;

	private const float DEFAULT_CONTOURS = 0f;

	private const float DEFAULT_FEATHERING = 1.5f;

	private const byte DEFAULT_NOISE_REDUCTION = 0;

	private const byte DEFAULT_BOUNDARY_SIZE = 3;

	private const bool DEFAULT_LOOK_FOR_OPEN_BOUNDARY = false;

	private Texture2D _sourceTexture;

	private Texture2D _outputTexture;

	private BinarySearchData _edgeThresholdBSD;

	private Rect _meshBounds;

	private Vector2 _meshPivot;

	private float _meshThickness;

	private int _maxOutputResolution = 512;

	private Stack<BinarySearchData> _edgeThresholdHistory;

	private byte noiseReduction;

	private float blurAmount = 1.5f;

	private float feathering = 1.5f;

	[UIField("Mesh Complexity", 101u, 0, 1, 0.01f, 1f, "Advanced Settings", "UI/Slider Advanced", false, null, 5, false, null)]
	private float meshComplexity = 1f;

	private float keepWeakContours;

	private byte boundarySize = 3;

	[UIField("Look For Open Boundary", 105u, null, null, null, null, "Advanced Settings", null, false, null, 5, false, null)]
	private bool lookForOpenBoundary;

	private Color32 backgroundColor = new Color(0.725f, 0.675f, 0.6f);

	public UnityEvent OnLooksGood;

	public ObjectEvent OnRequestMeshParameters;

	public ObjectEvent OnRequestUI;

	public Texture2DEvent OnImageGenerated;

	public BoolEvent OnAlphaGenerated;

	public MeshEvent OnMeshGenerated;

	public float edgeThreshold
	{
		get
		{
			return _edgeThresholdBSD.windowedValue;
		}
		set
		{
			_edgeThresholdBSD.windowedValue = Mathf.Clamp(value, 0.01f, 1f);
		}
	}

	public Rect meshBounds
	{
		set
		{
			_meshBounds = value;
		}
	}

	public Vector2 meshPivot
	{
		set
		{
			_meshPivot = value;
		}
	}

	public float meshThickness
	{
		set
		{
			_meshThickness = value;
		}
	}

	public Texture2D outputTexture => _outputTexture;

	private void ResetThresholdBinarySearch()
	{
		_edgeThresholdBSD = new BinarySearchData(0.025f);
		_edgeThresholdHistory = new Stack<BinarySearchData>();
	}

	private void DefaultAdvancedSettings()
	{
		noiseReduction = 0;
		blurAmount = 1.5f;
		meshComplexity = 1f;
		keepWeakContours = 0f;
		feathering = 1.5f;
		boundarySize = 3;
		lookForOpenBoundary = false;
	}

	public void SetTexture(Texture2D texture)
	{
		_sourceTexture = texture;
		DefaultAdvancedSettings();
		ResetThresholdBinarySearch();
		_ProcessImage();
	}

	[UIField("Looks good.", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public void LooksGood()
	{
		OnLooksGood.Invoke();
	}

	[UIField("Parts were left out.", 1u, null, null, null, null, null, null, false, null, 5, false, null)]
	public void PartsWereLeftOut()
	{
		_CommonResponse(delegate(BinarySearchData data)
		{
			data.Down();
		});
	}

	[UIField("Too much was included.", 2u, null, null, null, null, null, null, false, null, 5, false, null)]
	public void TooMuchWasIncluded()
	{
		_CommonResponse(delegate(BinarySearchData data)
		{
			data.Up();
		});
	}

	[UIField("Undo.", 3u, null, null, null, null, null, null, false, null, 5, false, null)]
	public void LastResultLookedBetter()
	{
		if (_edgeThresholdHistory.Count != 0)
		{
			_edgeThresholdBSD = _edgeThresholdHistory.Pop();
			_ProcessImage();
		}
	}

	[UIField("Start over.", 4u, null, null, null, null, null, null, false, null, 5, false, null)]
	public void LetsStartFromTheBeginning()
	{
		ResetThresholdBinarySearch();
		_ProcessImage();
	}

	[UIField("Retry with new settings.", 80u, null, null, null, null, "Advanced Settings", null, false, null, 5, false, null)]
	public void Retry()
	{
		_ProcessImage();
	}

	[UIField("Revert to default settings.", 300u, null, null, null, null, "Advanced Settings", null, false, null, 5, false, null)]
	public void RevetToDefaultSettings()
	{
		DefaultAdvancedSettings();
		_ProcessImage();
	}

	public void SetMeshParameters(Rect bounds, Vector2 pivot, float thickness, int maxOutputResolution = 512)
	{
		meshBounds = bounds;
		meshPivot = pivot;
		meshThickness = thickness;
		_maxOutputResolution = maxOutputResolution;
	}

	private void _CommonResponse(Action<BinarySearchData> action)
	{
		if (!(_sourceTexture == null))
		{
			_edgeThresholdHistory.Push(Serializer.DeepClone(_edgeThresholdBSD));
			action(_edgeThresholdBSD);
			_ProcessImage();
		}
	}

	private void _ProcessImage()
	{
		OnRequestMeshParameters.Invoke(this);
		Debug.Log("FindAlphaShapeView: _ProcessImage(): EdgeThreshold = " + _edgeThresholdBSD.currentValue);
		bool alreadyHadAlpha = false;
		UIUtil.BeginProcessJob(base.transform).Afterward().DoResult(() => GraphicsUtil.FindAlphaShape(out alreadyHadAlpha, _sourceTexture, backgroundColor, blurAmount, _edgeThresholdBSD, Mathf.Lerp(20f, 1f, meshComplexity), feathering, noiseReduction, (int)boundarySize, lookForOpenBoundary, keepWeakContours, 0.25f, _maxOutputResolution), Department.Content)
			.Afterward()
			.ResultProcess<Texture2D>(TextureToCutout)
			.Afterward()
			.ResultAction(delegate(Mesh mesh)
			{
				OnMeshGenerated.Invoke(mesh);
			})
			.Then()
			.Do(delegate
			{
				OnImageGenerated.Invoke(_outputTexture);
				OnAlphaGenerated.Invoke(!alreadyHadAlpha);
				if (!alreadyHadAlpha)
				{
					OnRequestUI.Invoke(this);
				}
				else
				{
					OnLooksGood.Invoke();
				}
			})
			.Then()
			.Do(UIUtil.EndProcess);
	}

	private IEnumerator TextureToCutout(Texture2D texture)
	{
		_outputTexture = texture;
		return GraphicsUtil.TextureTo3DCutout(texture, _meshBounds, _meshPivot, _meshThickness);
	}
}
