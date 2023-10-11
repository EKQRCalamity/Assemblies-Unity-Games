using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ScriptOrder(32767)]
public class AfterImageManager : MonoBehaviour
{
	public class AfterImageObject : IComparable<AfterImageObject>
	{
		private static List<MeshRenderer> _MeshRendererOutput = new List<MeshRenderer>();

		private static List<SkinnedMeshRenderer> _SkinnedMeshRendererOutput = new List<SkinnedMeshRenderer>();

		private static List<Vector3> _MeshVertices = new List<Vector3>();

		private static readonly MaterialPropertyBlock _PropertyBlock = new MaterialPropertyBlock();

		private static Vector3[] _QuadVerts = new Vector3[4]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f)
		};

		private static Vector3[] _QuadNormals = new Vector3[4]
		{
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward,
			-Vector3.forward
		};

		private static int[] _QuadTris = new int[6] { 0, 2, 1, 2, 3, 1 };

		private PoolListHandle<AfterImageMesh> _meshes;

		private float _timeOfCreation;

		private float _lifetime;

		private Gradient _colorOverLifetime;

		private Color _tintColor;

		private Material _objectMaterialOverride;

		private Material _afterImageMaterialOverride;

		private AnimationCurve _scaleOverLifetime;

		public Rect viewRect;

		public Rect uvRect;

		private float _z;

		private Bounds _worldBounds;

		private float _clipValue;

		private Matrix4x4 _projectionMatrix;

		private Matrix4x4 _viewProjectionMatrix;

		private Vector2? _scaleRange;

		private float _scale;

		private AnimationCurve _translationOverLifetime;

		private Vector3 _startTranslation;

		private Vector3 _endTranslation;

		private bool _hasTranslation;

		private Vector3 _translation;

		private Mesh _quad;

		private List<Vector3> _vertices;

		private List<Color32> _vertexColor;

		private List<Vector2> _uvs;

		public bool invalid => _meshes.Count == 0;

		public bool isDead => Time.time - _timeOfCreation > _lifetime;

		public int count => _meshes.Count;

		private float _lifeRatio => (Time.time - _timeOfCreation) / _lifetime;

		public AfterImageObject()
		{
			_quad = new Mesh();
			_quad.vertices = _QuadVerts;
			_quad.normals = _QuadNormals;
			_quad.triangles = _QuadTris;
			_vertices = new List<Vector3>(4).FillToCapacityWithDefault();
			_vertexColor = new List<Color32>(4).FillToCapacityWithDefault();
			_uvs = new List<Vector2>(4).FillToCapacityWithDefault();
		}

		public void Clear()
		{
			Pools.Repool(ref _meshes);
			_colorOverLifetime = null;
			_objectMaterialOverride = null;
			_afterImageMaterialOverride = null;
			_scaleOverLifetime = null;
			_translationOverLifetime = null;
		}

		private void _CalculateWorldBounds()
		{
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			foreach (AfterImageMesh item in _meshes.value)
			{
				Matrix4x4 transform = item.transform;
				item.mesh.GetVertices(_MeshVertices);
				for (int i = 0; i < _MeshVertices.Count; i++)
				{
					Vector3 vector = transform.MultiplyPoint3x4(_MeshVertices[i]);
					min.x = ((min.x < vector.x) ? min.x : vector.x);
					min.y = ((min.y < vector.y) ? min.y : vector.y);
					min.z = ((min.z < vector.z) ? min.z : vector.z);
					max.x = ((max.x > vector.x) ? max.x : vector.x);
					max.y = ((max.y > vector.y) ? max.y : vector.y);
					max.z = ((max.z > vector.z) ? max.z : vector.z);
				}
			}
			_worldBounds = new Bounds
			{
				min = min,
				max = max
			};
		}

		public AfterImageObject SetData(GameObject go, float lifetime, Gradient colorOverLifetime, Color tintColor, Material objectMaterialOverride, Material afterImageMaterialOverride, AnimationCurve scaleOverLifetime, Vector2? scaleRange, AnimationCurve translationOverLifetime, Vector3 startTranslation, Vector3 endTranslation)
		{
			_meshes = Pools.UseList<AfterImageMesh>();
			go.GetComponentsInChildren(_MeshRendererOutput);
			foreach (MeshRenderer item in _MeshRendererOutput)
			{
				if (item.gameObject.activeInHierarchy && item.enabled && (bool)item.GetComponent<MeshFilter>().sharedMesh)
				{
					_meshes.Add(Pools.Unpool<AfterImageMesh>().SetData(item));
				}
			}
			go.GetComponentsInChildren(_SkinnedMeshRendererOutput);
			foreach (SkinnedMeshRenderer item2 in _SkinnedMeshRendererOutput)
			{
				if (item2.gameObject.activeInHierarchy && item2.enabled && (bool)item2.sharedMesh)
				{
					_meshes.Add(Pools.Unpool<AfterImageMesh>().SetData(item2));
				}
			}
			if (invalid)
			{
				return this;
			}
			_meshes.value.Sort();
			_CalculateWorldBounds();
			_timeOfCreation = Time.time;
			_lifetime = lifetime;
			_colorOverLifetime = colorOverLifetime;
			_tintColor = tintColor;
			_objectMaterialOverride = objectMaterialOverride;
			_afterImageMaterialOverride = afterImageMaterialOverride;
			_scaleOverLifetime = scaleOverLifetime;
			_scaleRange = scaleRange;
			_translationOverLifetime = translationOverLifetime;
			_startTranslation = startTranslation;
			_endTranslation = endTranslation;
			_hasTranslation = endTranslation != Vector3.zero || startTranslation != Vector3.zero;
			return this;
		}

		public bool CalculateRects(Camera camera, Plane[] frustumPlanes, Vector2 viewRectPadding)
		{
			float lifeRatio = _lifeRatio;
			_translation = (_hasTranslation ? Vector3.Lerp(_startTranslation, _endTranslation, _translationOverLifetime.Evaluate(lifeRatio)) : Vector3.zero);
			_scale = (_scaleRange.HasValue ? _scaleRange.Value.Lerp(_scaleOverLifetime.Evaluate(lifeRatio)) : 1f);
			Bounds bounds = _worldBounds.ScaleAndTranslate(_scale, _translation);
			if (!GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
			{
				return false;
			}
			RectZ viewPortRect = bounds.GetViewPortRect(camera);
			_z = viewPortRect.z;
			uvRect = (viewRect = viewPortRect.rect.Pad(viewRectPadding));
			_clipValue = viewPortRect.rect.Clip(ViewSpaceRect).Area() / viewPortRect.rect.Area();
			if (_clipValue < 1f)
			{
				ScaleUVRect(_clipValue *= _clipValue);
			}
			return _clipValue > ClipValueThreshold;
		}

		public void ScaleUVRect(float scale)
		{
			uvRect.size *= scale;
		}

		public void CalculateCameraMatrices(ref Matrix4x4 projectionMatrix, ref Matrix4x4 viewProjectionMatrix)
		{
			float num = uvRect.width / viewRect.width;
			Matrix4x4 matrix4x = Matrix4x4.Translate(new Vector3(num - 1f, 1f - num, 0f) + new Vector3(uvRect.x + uvRect.x, 0f - (uvRect.y + uvRect.y), 0f)) * Matrix4x4.Scale(new Vector3(num, num, 1f)) * Matrix4x4.Translate(new Vector3(0f - (viewRect.x + viewRect.x), viewRect.y + viewRect.y, 0f));
			_projectionMatrix = matrix4x * projectionMatrix;
			_viewProjectionMatrix = matrix4x * viewProjectionMatrix;
		}

		public void RenderImpostersIntoAtlas(Camera camera, ref Matrix4x4 viewMatrix, Material materialOverride = null)
		{
			_PropertyBlock.Clear();
			_PropertyBlock.SetMatrix(VIEW_MATRIX_ID, viewMatrix);
			_PropertyBlock.SetMatrix(PROJECTION_MATRIX_ID, _projectionMatrix);
			_PropertyBlock.SetMatrix(VIEW_PROJECTION_MATRIX_ID, _viewProjectionMatrix);
			foreach (AfterImageMesh item in _meshes.value)
			{
				item.DrawMesh(camera, _PropertyBlock, materialOverride ?? _objectMaterialOverride, _worldBounds.center, _scaleRange.HasValue ? new float?(_scale) : null, _hasTranslation ? new Vector3?(_translation) : null);
			}
		}

		public void RenderAfterImageQuad(Camera camera, Material material)
		{
			Color32 color = _colorOverLifetime.Evaluate(_lifeRatio).Multiply(_tintColor);
			if (_clipValue < 1f)
			{
				color = color.SetAlpha(color.Alpha() * _clipValue * _clipValue);
			}
			for (int i = 0; i < _vertexColor.Count; i++)
			{
				_vertexColor[i] = color;
			}
			_quad.SetColors(_vertexColor);
			float xMin = uvRect.xMin;
			float yMin = uvRect.yMin;
			float xMax = uvRect.xMax;
			float yMax = uvRect.yMax;
			_uvs[0] = new Vector2(xMin, yMin);
			_uvs[1] = new Vector2(xMax, yMin);
			_uvs[2] = new Vector2(xMin, yMax);
			_uvs[3] = new Vector2(xMax, yMax);
			_quad.SetUVs(0, _uvs);
			float xMin2 = viewRect.xMin;
			float yMin2 = viewRect.yMin;
			float xMax2 = viewRect.xMax;
			float yMax2 = viewRect.yMax;
			_vertices[0] = camera.ViewportToWorldPoint(new Vector3(xMin2, yMin2, _z));
			_vertices[1] = camera.ViewportToWorldPoint(new Vector3(xMax2, yMin2, _z));
			_vertices[2] = camera.ViewportToWorldPoint(new Vector3(xMin2, yMax2, _z));
			_vertices[3] = camera.ViewportToWorldPoint(new Vector3(xMax2, yMax2, _z));
			_quad.SetVertices(_vertices);
			_quad.RecalculateBounds();
			Graphics.DrawMesh(_quad, Matrix4x4.identity, _afterImageMaterialOverride ?? material, 0, camera, 0, null, castShadows: false, receiveShadows: false, useLightProbes: false);
		}

		public int CompareTo(AfterImageObject other)
		{
			int num = MathUtil.Compare(uvRect.width, other.uvRect.width);
			if (num != 0)
			{
				return num;
			}
			return GetHashCode() - other.GetHashCode();
		}
	}

	public class AfterImageMesh : IComparable<AfterImageMesh>
	{
		public Matrix4x4 transform;

		public Mesh sharedMesh;

		public Mesh mesh;

		public Material material;

		private Vector3 _translation;

		public void Clear()
		{
			if (sharedMesh != mesh && (bool)Instance)
			{
				Instance._RepoolBakedSkinnedMesh(sharedMesh, mesh);
			}
			sharedMesh = (mesh = null);
			material = null;
		}

		private void _SetData(Renderer r)
		{
			transform = r.transform.localToWorldMatrix;
			material = r.sharedMaterial;
			_translation = transform.GetTranslation();
		}

		public AfterImageMesh SetData(MeshRenderer meshRenderer)
		{
			_SetData(meshRenderer);
			sharedMesh = (mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh);
			return this;
		}

		public AfterImageMesh SetData(SkinnedMeshRenderer skinnedMeshRenderer)
		{
			_SetData(skinnedMeshRenderer);
			sharedMesh = skinnedMeshRenderer.sharedMesh;
			mesh = Instance._GetBakedSkinnedMesh(sharedMesh, skinnedMeshRenderer);
			return this;
		}

		public void DrawMesh(Camera camera, MaterialPropertyBlock propertyBlock, Material materialOverride, Vector3 center, float? scale, Vector3? translation)
		{
			Matrix4x4 matrix = transform;
			if (scale.HasValue)
			{
				matrix = matrix.SetTranslation(_translation + (_translation - center) * (scale.Value - 1f)) * Matrix4x4.Scale(new Vector3(scale.Value, scale.Value, scale.Value));
			}
			if (translation.HasValue)
			{
				matrix = matrix.Translation(translation.Value);
			}
			Graphics.DrawMesh(mesh, matrix, materialOverride ?? material, 30, camera, 0, propertyBlock, ShadowCastingMode.Off, receiveShadows: false);
		}

		public int CompareTo(AfterImageMesh other)
		{
			return material.renderQueue - other.material.renderQueue;
		}
	}

	private static readonly Rect ViewSpaceRect;

	private const string _Color = "_AfterImageColor";

	private static readonly int _ColorId;

	private const string _Depth = "_AfterImageDepth";

	private static readonly int _DepthId;

	private const string VIEW_MATRIX = "unity_MatrixV";

	private const string PROJECTION_MATRIX = "unity_MatrixP";

	private const string VIEW_PROJECTION_MATRIX = "unity_MatrixVP";

	private static readonly int VIEW_MATRIX_ID;

	private static readonly int PROJECTION_MATRIX_ID;

	private static readonly int VIEW_PROJECTION_MATRIX_ID;

	private static readonly Matrix4x4 FLIP_PROJECTION_Y;

	private static readonly float ClipValueThreshold;

	private static readonly Gradient _DefaultColorOverLifetime;

	private static readonly AnimationCurve _DefaultScaleOverLifetime;

	private static readonly AnimationCurve _DefaultTranslationOverLifetime;

	private static AfterImageManager _Instance;

	private static Material _AfterImageMaterial;

	private static Material _DepthOnlyMaterial;

	private List<AfterImageObject> _afterImages = new List<AfterImageObject>();

	private List<AfterImageObject> _afterImagesToRender = new List<AfterImageObject>();

	private Queue<Rect> _pendingFillRects = new Queue<Rect>();

	private Dictionary<Mesh, Queue<Mesh>> _bakedMeshCache = new Dictionary<Mesh, Queue<Mesh>>();

	private Camera _camera;

	private Plane[] _frustumPlanes = new Plane[6];

	private RenderTexture _targetColor;

	private RenderTexture _targetDepth;

	private Vector2 _viewRectPadding;

	public static AfterImageManager Instance => ManagerUtil.GetSingletonInstance(ref _Instance, createSeparateGameObject: true, null, dontDestroyOnLoad: false);

	private static Material AfterImageMaterial
	{
		get
		{
			if (!_AfterImageMaterial)
			{
				return _AfterImageMaterial = Resources.Load<Material>("Graphics/Materials/AfterImage/AfterImage");
			}
			return _AfterImageMaterial;
		}
	}

	private static Material DepthOnlyMaterial
	{
		get
		{
			if (!_DepthOnlyMaterial)
			{
				return _DepthOnlyMaterial = Resources.Load<Material>("Graphics/Materials/DepthOnly");
			}
			return _DepthOnlyMaterial;
		}
	}

	static AfterImageManager()
	{
		ViewSpaceRect = new Rect(0f, 0f, 1f, 1f);
		_ColorId = Shader.PropertyToID("_AfterImageColor");
		_DepthId = Shader.PropertyToID("_AfterImageDepth");
		VIEW_MATRIX_ID = Shader.PropertyToID("unity_MatrixV");
		PROJECTION_MATRIX_ID = Shader.PropertyToID("unity_MatrixP");
		VIEW_PROJECTION_MATRIX_ID = Shader.PropertyToID("unity_MatrixVP");
		FLIP_PROJECTION_Y = Matrix4x4.Scale(new Vector3(1f, -1f, 1f));
		ClipValueThreshold = Mathf.Sqrt(0.007843138f);
		_DefaultColorOverLifetime = new Gradient
		{
			colorKeys = new GradientColorKey[2]
			{
				new GradientColorKey(Color.white, 0f),
				new GradientColorKey(Color.white, 1f)
			},
			alphaKeys = new GradientAlphaKey[2]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(0f, 1f)
			},
			mode = GradientMode.Blend
		};
		_DefaultScaleOverLifetime = AnimationCurve.Constant(0f, 1f, 1f);
		_DefaultTranslationOverLifetime = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new AfterImageObject(), delegate(AfterImageObject a)
		{
			a.Clear();
		}, delegate
		{
		});
		Pools.CreatePool(createHierarchy: false, setAsProtoFactory: false, () => new AfterImageMesh(), delegate(AfterImageMesh a)
		{
			a.Clear();
		}, delegate
		{
		});
	}

	private bool _CalculateAfterImageUVRects(List<AfterImageObject> afterImageObjects)
	{
		Vector2 lhs = new Vector2(float.MaxValue, float.MaxValue);
		for (int i = 0; i < afterImageObjects.Count; i++)
		{
			lhs = Vector2.Min(lhs, afterImageObjects[i].uvRect.size);
		}
		using PoolKeepItemListHandle<AfterImageObject> poolKeepItemListHandle = Pools.UseKeepItemList(afterImageObjects);
		_pendingFillRects.Clear();
		_pendingFillRects.Enqueue(ViewSpaceRect);
		while (_pendingFillRects.Count > 0 && poolKeepItemListHandle.Count > 0)
		{
			Rect rect = _pendingFillRects.Dequeue();
			float num = rect.yMin;
			float xMin = rect.xMin;
			float width = rect.width;
			float num2 = rect.height;
			Rect rect2 = rect;
			for (int num3 = poolKeepItemListHandle.Count - 1; num3 >= 0; num3--)
			{
				AfterImageObject afterImageObject = poolKeepItemListHandle[num3];
				Vector2 size = afterImageObject.uvRect.size;
				float x = size.x;
				if (x > width)
				{
					break;
				}
				float y = size.y;
				if (!(y > num2))
				{
					afterImageObject.uvRect = new Rect(xMin, num, x, y);
					poolKeepItemListHandle.RemoveAt(num3);
					if (width >= lhs.x && num2 >= lhs.y)
					{
						_pendingFillRects.Enqueue(new Rect(afterImageObject.uvRect.xMax, afterImageObject.uvRect.yMin, rect2.xMax - afterImageObject.uvRect.xMax, rect.yMax - afterImageObject.uvRect.yMin));
					}
					num += y;
					num2 -= y;
					rect2 = afterImageObject.uvRect;
					if (num2 < lhs.y)
					{
						break;
					}
				}
			}
		}
		return poolKeepItemListHandle.Count == 0;
	}

	private void _GenerateTargetColorTexture(Camera camera)
	{
		if ((bool)camera)
		{
			if ((bool)_targetColor)
			{
				_targetColor.Release();
			}
			_targetColor = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
			{
				name = "_AfterImageColor",
				antiAliasing = GraphicsUtil.antiAliasing,
				filterMode = FilterMode.Bilinear,
				useMipMap = false,
				autoGenerateMips = false
			};
			if ((bool)_targetDepth)
			{
				_targetDepth.Release();
			}
			_targetDepth = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 32, RenderTextureFormat.Depth, RenderTextureReadWrite.sRGB)
			{
				name = "_AfterImageDepth",
				filterMode = FilterMode.Bilinear,
				useMipMap = false,
				autoGenerateMips = false
			};
			Shader.SetGlobalTexture(_ColorId, _targetColor);
			Shader.SetGlobalTexture(_DepthId, _targetDepth);
			_viewRectPadding = new Vector2(1f / (float)camera.pixelWidth, 1f / (float)camera.pixelHeight);
		}
	}

	private Mesh _GetBakedSkinnedMesh(Mesh sharedMesh, SkinnedMeshRenderer skinnedMeshRenderer)
	{
		if (!_bakedMeshCache.ContainsKey(sharedMesh))
		{
			_bakedMeshCache.Add(sharedMesh, new Queue<Mesh>());
		}
		Queue<Mesh> queue = _bakedMeshCache[sharedMesh];
		if (queue.Count == 0)
		{
			queue.Enqueue(new Mesh());
		}
		Mesh mesh = queue.Dequeue();
		skinnedMeshRenderer.BakeMesh(mesh);
		return mesh;
	}

	private void _RepoolBakedSkinnedMesh(Mesh sharedMesh, Mesh bakedMesh)
	{
		if (_bakedMeshCache.ContainsKey(sharedMesh))
		{
			_bakedMeshCache[sharedMesh].Enqueue(bakedMesh);
		}
	}

	private void Awake()
	{
		_camera = new GameObject("After Image Camera").AddComponent<Camera>();
		_camera.cullingMask = 1073741824;
		_camera.clearFlags = CameraClearFlags.Color;
		_camera.backgroundColor = Color.clear;
		_camera.renderingPath = RenderingPath.Forward;
		_camera.transform.SetParent(base.transform);
		_camera.enabled = false;
		_GenerateTargetColorTexture(CameraManager.Instance.mainCamera);
	}

	private void LateUpdate()
	{
		for (int num = _afterImages.Count - 1; num >= 0; num--)
		{
			if (_afterImages[num].isDead)
			{
				Pools.Repool(_afterImages[num]);
				_afterImages.RemoveAt(num);
			}
		}
		if (_afterImages.Count == 0)
		{
			return;
		}
		Camera mainCamera = CameraManager.Instance.mainCamera;
		GeometryUtility.CalculateFrustumPlanes(mainCamera, _frustumPlanes);
		for (int i = 0; i < _afterImages.Count; i++)
		{
			if (_afterImages[i].CalculateRects(mainCamera, _frustumPlanes, _viewRectPadding))
			{
				_afterImagesToRender.Add(_afterImages[i]);
			}
		}
		if (_afterImagesToRender.Count == 0)
		{
			return;
		}
		if (mainCamera.PixelDimensions() != _targetColor.PixelDimensions())
		{
			_GenerateTargetColorTexture(mainCamera);
		}
		_afterImagesToRender.Sort();
		while (!_CalculateAfterImageUVRects(_afterImagesToRender))
		{
			for (int j = 0; j < _afterImagesToRender.Count; j++)
			{
				_afterImagesToRender[j].ScaleUVRect(0.75f);
			}
		}
		CameraSync.CopyFrom(_camera, mainCamera, transform: true, aspect: true, fieldOfView: true, nearClipPlane: true, farClipPlane: true, hdr: true, msaa: true, renderPath: false);
		Matrix4x4 viewMatrix = _camera.worldToCameraMatrix;
		Matrix4x4 projectionMatrix = FLIP_PROJECTION_Y * GL.GetGPUProjectionMatrix(_camera.projectionMatrix, renderIntoTexture: false);
		Matrix4x4 viewProjectionMatrix = projectionMatrix * viewMatrix;
		for (int k = 0; k < _afterImagesToRender.Count; k++)
		{
			_afterImagesToRender[k].CalculateCameraMatrices(ref projectionMatrix, ref viewProjectionMatrix);
			_afterImagesToRender[k].RenderImpostersIntoAtlas(_camera, ref viewMatrix);
		}
		_camera.targetTexture = _targetColor;
		_camera.Render();
		Material depthOnlyMaterial = DepthOnlyMaterial;
		for (int l = 0; l < _afterImagesToRender.Count; l++)
		{
			_afterImagesToRender[l].RenderImpostersIntoAtlas(_camera, ref viewMatrix, depthOnlyMaterial);
		}
		_camera.targetTexture = _targetDepth;
		_camera.Render();
		Material afterImageMaterial = AfterImageMaterial;
		for (int m = 0; m < _afterImagesToRender.Count; m++)
		{
			_afterImagesToRender[m].RenderAfterImageQuad(mainCamera, afterImageMaterial);
		}
		_afterImagesToRender.Clear();
	}

	public int CreateAfterImage(GameObject go, float lifetime, Gradient colorOverLifetime = null, Color? tintColor = null, Material objectMaterialOverride = null, Material afterImageMaterialOverride = null, AnimationCurve scaleOverLifetime = null, Vector2? scaleRange = null, AnimationCurve translationOverLifetime = null, Vector3? startTranslation = null, Vector3? endTranslation = null)
	{
		AfterImageObject afterImageObject = Pools.Unpool<AfterImageObject>().SetData(go, lifetime, colorOverLifetime ?? _DefaultColorOverLifetime, tintColor ?? Color.white, objectMaterialOverride ? objectMaterialOverride : null, afterImageMaterialOverride ? afterImageMaterialOverride : null, scaleOverLifetime ?? _DefaultScaleOverLifetime, scaleRange, translationOverLifetime ?? _DefaultTranslationOverLifetime, startTranslation.HasValue ? go.transform.TransformDirection(startTranslation.Value) : Vector3.zero, endTranslation.HasValue ? go.transform.TransformDirection(endTranslation.Value) : Vector3.zero);
		if (afterImageObject.invalid)
		{
			Pools.Repool(afterImageObject);
			return 0;
		}
		_afterImages.Add(afterImageObject);
		return afterImageObject.count;
	}
}
