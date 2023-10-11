using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LibTessDotNet;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class GraphicsUtil
{
	public const int MAX_TEXTURE_RESOLUTION = 8192;

	public const int MAX_MESH_VERTICES = 65000;

	public static readonly int MAIN_TEXTURE_ID;

	public static readonly string[] STANDARD_SHADER_TEXTURE_NAMES;

	private const LayerMaskTypes IGNORE_WORLD_LIGHT_LAYERS = LayerMaskTypes.Processing | LayerMaskTypes.UIOverlay;

	public static readonly LayerMaskTypes WORLD_LIGHT_LAYERS;

	public static readonly Rect ViewSpaceRect;

	public static readonly int BASE_COLOR_MAP_ID;

	public static readonly int MASK_MAP_ID;

	public static readonly int NORMAL_MAP_ID;

	public static bool StaticConstructorTrigger;

	private static string[] _MATERIAL_COLOR_NAMES;

	private static GameObject _RenderMeshIdentityBlueprint;

	private static int _SrcBlendA;

	private static int _DstBlendA;

	private static Material _UIDefaultCompositeMaterial;

	private static Shader _DefaultUIShader;

	public static readonly Rect EQUIPMENT_MESH_BOUNDS;

	public static readonly Vector2 EQUIPMENT_MESH_PIVOT;

	public static readonly float EQUIPMENT_MESH_THICKNESS;

	public static readonly Rect PROJECTILE_MESH_BOUNDS;

	public static readonly Vector2 PROJECTILE_MESH_PIVOT;

	public static readonly float PROJECTILE_MESH_THICKNESS;

	public static readonly Rect ENEMY_MESH_BOUNDS;

	public static readonly Vector2 ENEMY_MESH_PIVOT;

	public static readonly float ENEMY_MESH_THICKNESS;

	private static Dictionary<string, int> _QualityLevelsByName;

	private static Dictionary<int, QualitySettingData> _QualitySettingsData;

	public static int antiAliasing => Math.Max(1, QualitySettings.antiAliasing);

	private static GameObject RenderMeshIdentityBlueprint
	{
		get
		{
			if (!(_RenderMeshIdentityBlueprint != null))
			{
				return _RenderMeshIdentityBlueprint = Resources.Load<GameObject>("Graphics/RenderMeshIdentity");
			}
			return _RenderMeshIdentityBlueprint;
		}
	}

	private static Material UIDefaultCompositeMaterial
	{
		get
		{
			if (!_UIDefaultCompositeMaterial)
			{
				return _UIDefaultCompositeMaterial = new Material(Shader.Find("UI/Default Composite"));
			}
			return _UIDefaultCompositeMaterial;
		}
	}

	private static Shader DefaultUIShader
	{
		get
		{
			if (!_DefaultUIShader)
			{
				return _DefaultUIShader = Shader.Find("UI/Default");
			}
			return _DefaultUIShader;
		}
	}

	static GraphicsUtil()
	{
		MAIN_TEXTURE_ID = Shader.PropertyToID("_MainTex");
		STANDARD_SHADER_TEXTURE_NAMES = new string[9] { "_MainTex", "_MetallicGlossMap", "_BumpMap", "_ParallaxMap", "_OcclusionMap", "_EmissionMap", "_DetailMask", "_DetailAlbedoMap", "_DetailNormalMap" };
		WORLD_LIGHT_LAYERS = EnumUtil<LayerMaskTypes>.AllFlagsExcept(LayerMaskTypes.Processing | LayerMaskTypes.UIOverlay);
		ViewSpaceRect = new Rect(0f, 0f, 1f, 1f);
		BASE_COLOR_MAP_ID = Shader.PropertyToID("_BaseColorMap");
		MASK_MAP_ID = Shader.PropertyToID("_MaskMap");
		NORMAL_MAP_ID = Shader.PropertyToID("_NormalMap");
		_MATERIAL_COLOR_NAMES = new string[4] { "_Color", "_TintColor", "_Tint", "_Emission" };
		_SrcBlendA = Shader.PropertyToID("_SrcBlendA");
		_DstBlendA = Shader.PropertyToID("_DstBlendA");
		EQUIPMENT_MESH_BOUNDS = new Rect(-0.5f, -0.5f, 1f, 1f);
		EQUIPMENT_MESH_PIVOT = new Vector2(0.5f, 0.5f);
		EQUIPMENT_MESH_THICKNESS = 0.006f;
		PROJECTILE_MESH_BOUNDS = new Rect(-0.5f, -0.5f, 1f, 1f);
		PROJECTILE_MESH_PIVOT = new Vector2(0.5f, 0.5f);
		PROJECTILE_MESH_THICKNESS = 0.006f;
		ENEMY_MESH_BOUNDS = new Rect(-0.5f, 0f, 1f, 1.5f);
		ENEMY_MESH_PIVOT = new Vector2(0.5f, 0f);
		ENEMY_MESH_THICKNESS = 0.006f;
		_QualityLevelsByName = new Dictionary<string, int>();
		_QualitySettingsData = new Dictionary<int, QualitySettingData>();
		string[] names = QualitySettings.names;
		for (int i = 0; i < names.Length; i++)
		{
			_QualityLevelsByName.Add(names[i], i);
			_QualitySettingsData.Add(i, new QualitySettingData(i));
		}
	}

	public static void Clear(this AnimationCurve curve)
	{
		for (int num = curve.length - 1; num >= 0; num--)
		{
			curve.RemoveKey(num);
		}
	}

	public static AnimationCurve Clone(this AnimationCurve curve)
	{
		return new AnimationCurve(curve.keys);
	}

	public static float minValue(this AnimationCurve curve)
	{
		float num = float.MaxValue;
		for (int i = 0; i < curve.length; i++)
		{
			if (curve[i].value < num)
			{
				num = curve[i].value;
			}
		}
		return num;
	}

	public static float maxValue(this AnimationCurve curve)
	{
		float num = float.MinValue;
		for (int i = 0; i < curve.length; i++)
		{
			if (curve[i].value > num)
			{
				num = curve[i].value;
			}
		}
		return num;
	}

	public static float minTime(this AnimationCurve curve)
	{
		float num = float.MaxValue;
		for (int i = 0; i < curve.length; i++)
		{
			if (curve[i].time < num)
			{
				num = curve[i].time;
			}
		}
		return num;
	}

	public static float maxTime(this AnimationCurve curve)
	{
		float num = float.MinValue;
		for (int i = 0; i < curve.length; i++)
		{
			if (curve[i].time > num)
			{
				num = curve[i].time;
			}
		}
		return num;
	}

	public static float EvaluateWithExtrapolation(this AnimationCurve curve, float t, float minTime = 0f, float maxTime = 1f, float sampleOffset = 0.01f)
	{
		if (t >= minTime && t <= maxTime)
		{
			return curve.Evaluate(t);
		}
		if (t < minTime)
		{
			return curve.Evaluate(minTime) + (curve.Evaluate(minTime + sampleOffset) - curve.Evaluate(minTime)) * ((t - minTime) / sampleOffset);
		}
		if (t > maxTime)
		{
			return curve.Evaluate(maxTime) + (curve.Evaluate(maxTime) - curve.Evaluate(maxTime - sampleOffset)) * ((t - maxTime) / sampleOffset);
		}
		return t;
	}

	public static Vector2 range(this AnimationCurve curve)
	{
		return new Vector2(curve.minValue(), curve.maxValue());
	}

	public static Vector2 domain(this AnimationCurve curve)
	{
		return new Vector2(curve.minTime(), curve.maxTime());
	}

	public static float ApproximateAverage(this AnimationCurve curve)
	{
		return curve.range().Average();
	}

	public static void Multiply(this AnimationCurve curve, float multiplier)
	{
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			keys[i].value *= multiplier;
		}
		curve.keys = keys;
	}

	public static void Shift(this AnimationCurve curve, AnimationCurve output, Vector2 range, AnimationType animType, float t, int fidelity = 10)
	{
		output.Clear();
		int num = fidelity - 1;
		for (int i = 0; i < fidelity; i++)
		{
			float num2 = (float)i / (float)num;
			output.AddKey(num2, Mathf.Lerp(range.x, range.y, curve.Evaluate(SampleAtSimple(animType, num2 + t))));
		}
	}

	public static AnimationCurve DisplayFunction(this AnimationCurve curve, Func<float, float> func, Vector2 range, int numKeys)
	{
		Keyframe[] array = new Keyframe[numKeys];
		numKeys = Math.Max(1, numKeys - 1);
		float num = 1f / (float)numKeys;
		float num2 = range.Range() * num;
		for (int i = 0; i <= numKeys; i++)
		{
			float num3 = (float)i * num;
			float arg = Math.Max(num3 - num2, range.x);
			float arg2 = Math.Min(num3 + num2, range.y);
			float num4 = func(num3);
			array[i] = new Keyframe(Mathf.Lerp(range.x, range.y, num3), num4, (num4 - func(arg)) / num2, (func(arg2) - num4) / num2);
		}
		curve.keys = array;
		return curve;
	}

	public static void Add(this Gradient g, Color color, bool addAlpha = true)
	{
		GradientColorKey[] colorKeys = g.colorKeys;
		GradientAlphaKey[] alphaKeys = g.alphaKeys;
		for (int i = 0; i < colorKeys.Length; i++)
		{
			colorKeys[i].color += color;
		}
		if (addAlpha)
		{
			for (int j = 0; j < alphaKeys.Length; j++)
			{
				alphaKeys[j].alpha += color.a;
			}
		}
		g.SetKeys(colorKeys, alphaKeys);
	}

	public static void Multiply(this Gradient g, Color color, bool multiplyAlpha = true)
	{
		GradientColorKey[] colorKeys = g.colorKeys;
		GradientAlphaKey[] alphaKeys = g.alphaKeys;
		for (int i = 0; i < colorKeys.Length; i++)
		{
			colorKeys[i].color *= color;
		}
		if (multiplyAlpha)
		{
			for (int j = 0; j < alphaKeys.Length; j++)
			{
				alphaKeys[j].alpha *= color.a;
			}
		}
		g.SetKeys(colorKeys, alphaKeys);
	}

	public static void Lerp(this Gradient g, Color color, float lerp, bool lerpAlpha = false)
	{
		GradientColorKey[] colorKeys = g.colorKeys;
		GradientAlphaKey[] alphaKeys = g.alphaKeys;
		for (int i = 0; i < colorKeys.Length; i++)
		{
			colorKeys[i].color = colorKeys[i].color.Lerp(color, lerp);
		}
		if (lerpAlpha)
		{
			for (int j = 0; j < alphaKeys.Length; j++)
			{
				alphaKeys[j].alpha = MathUtil.Lerp(alphaKeys[j].alpha, color.a, lerp);
			}
		}
		g.SetKeys(colorKeys, alphaKeys);
	}

	public static void Shift(this Gradient gradient, Gradient output, AnimationType animType, float t, int fidelity = 10, bool shiftAlpha = false)
	{
		GradientColorKey[] array = new GradientColorKey[fidelity];
		GradientAlphaKey[] array2 = (shiftAlpha ? new GradientAlphaKey[fidelity] : gradient.alphaKeys);
		int num = fidelity - 1;
		for (int i = 0; i < fidelity; i++)
		{
			float num2 = (float)i / (float)num;
			array[i].time = num2;
			array[i].color = gradient.Evaluate(SampleAtSimple(animType, num2 + t));
			if (shiftAlpha)
			{
				array2[i].time = num2;
				array2[i].alpha = array[i].color.a;
			}
		}
		output.SetKeys(array, array2);
	}

	public static string FindColor(this Material material)
	{
		for (int i = 0; i < _MATERIAL_COLOR_NAMES.Length; i++)
		{
			if (material.HasProperty(_MATERIAL_COLOR_NAMES[i]))
			{
				return _MATERIAL_COLOR_NAMES[i];
			}
		}
		return null;
	}

	public static void AddSharedMaterial(this Renderer renderer, Material sharedMaterial, bool insureUnique = true)
	{
		int? num = null;
		Material[] array = renderer.sharedMaterials;
		for (int i = 0; i < array.Length; i++)
		{
			if (!num.HasValue && !array[i])
			{
				num = i;
			}
			if (insureUnique && array[i] == sharedMaterial)
			{
				return;
			}
		}
		if (!num.HasValue)
		{
			num = array.Length;
			Array.Resize(ref array, array.Length + 1);
		}
		array[num.Value] = sharedMaterial;
		renderer.sharedMaterials = array;
	}

	public static void RemoveSharedMaterial(this Renderer renderer, Material sharedMaterial)
	{
		Material[] sharedMaterials = renderer.sharedMaterials;
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			if (!(sharedMaterials[i] != sharedMaterial))
			{
				sharedMaterials[i] = null;
				break;
			}
		}
		renderer.sharedMaterials = sharedMaterials.Where((Material m) => m != null).ToArray();
	}

	public static void DebugDisplay(this Mesh mesh, string name, Texture2D texture = null, Vector3? position = null, Transform parent = null, bool addCollider = false, Quaternion? rotation = null)
	{
		GameObject gameObject = new GameObject(name);
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		if (texture != null)
		{
			meshRenderer.material.mainTexture = texture;
		}
		meshFilter.mesh = mesh;
		if (position.HasValue)
		{
			gameObject.transform.position = position.Value;
		}
		if (rotation.HasValue)
		{
			gameObject.transform.rotation = rotation.Value;
		}
		gameObject.transform.parent = parent;
		if (addCollider)
		{
			gameObject.AddComponent<BoxCollider>();
		}
	}

	public static void SetPivot(this Mesh mesh, Vector3 pivotLerpAlongBounds, Vector3 origin)
	{
		Bounds bounds = mesh.bounds;
		Vector3 vector = origin - bounds.LerpedPosition(pivotLerpAlongBounds);
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] += vector;
		}
		mesh.vertices = vertices;
		mesh.bounds = bounds.Translate(vector);
	}

	public static void Rotate(this Mesh mesh, Quaternion rotation)
	{
		Bounds bounds = mesh.bounds;
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = mesh.tangents;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = rotation * vertices[i];
		}
		for (int j = 0; j < normals.Length; j++)
		{
			normals[j] = rotation * normals[j];
		}
		for (int k = 0; k < tangents.Length; k++)
		{
			tangents[k] = rotation * tangents[k];
		}
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.bounds = bounds.Rotate(rotation);
	}

	public static void InvertFaces(this int[] triangles)
	{
		for (int i = 0; i < triangles.Length / 3; i++)
		{
			int num = triangles[i * 3];
			triangles[i * 3] = triangles[i * 3 + 1];
			triangles[i * 3 + 1] = num;
		}
	}

	public static PoolKeepItemListHandle<Vector3> GetVertices(this Mesh mesh)
	{
		PoolKeepItemListHandle<Vector3> poolKeepItemListHandle = Pools.UseKeepItemList<Vector3>();
		if (mesh.isReadable)
		{
			mesh.GetVertices(poolKeepItemListHandle);
		}
		else
		{
			mesh.bounds.GetVertices(poolKeepItemListHandle);
		}
		return poolKeepItemListHandle;
	}

	public static void Map(this Texture2D t, Func<Vector2, Color> mapping)
	{
		Color[] array = new Color[t.width * t.height];
		float num = Math.Max(1, t.width - 1);
		float num2 = Math.Max(1, t.height - 1);
		for (int i = 0; i < t.height; i++)
		{
			for (int j = 0; j < t.width; j++)
			{
				array[j + i * t.width] = mapping(new Vector2((float)j / num, (float)i / num2));
			}
		}
		t.SetPixels(array);
		t.Apply();
	}

	public static Int2 PixelSize(this Texture2D t)
	{
		return new Int2(t.width, t.height);
	}

	public static Int2 PixelDimensions(this RenderTexture rt)
	{
		return new Int2(rt.width, rt.height);
	}

	public static int GetMemoryUsed(this Texture2D t)
	{
		return t.PixelSize().area * t.format.GetBytesPerPixel();
	}

	public static Resolution FromString(this Resolution resolution, string resolutionToString)
	{
		List<int> list = resolutionToString.ToIntegers();
		Resolution result = default(Resolution);
		result.width = list[0];
		result.height = list[1];
		result.refreshRate = list[2];
		return result;
	}

	public static bool EqualResolution(this Resolution a, Resolution b)
	{
		if (a.width == b.width)
		{
			return a.height == b.height;
		}
		return false;
	}

	public static Resolution GetNativeResolution()
	{
		Resolution result = default(Resolution);
		result.width = Display.main.systemWidth;
		result.height = Display.main.systemHeight;
		result.refreshRate = Screen.currentResolution.refreshRate;
		return result;
	}

	public static bool IsValidResolution(this Resolution resolution)
	{
		return Screen.resolutions.Contains(resolution);
	}

	public static float Aspect(this Resolution resolution)
	{
		return (float)resolution.width / (float)resolution.height;
	}

	public static void SetScissorRectFromViewPortRect(this Camera camera, Rect r)
	{
		if (r.x < 0f)
		{
			r.width += r.x;
			r.x = 0f;
		}
		if (r.y < 0f)
		{
			r.height += r.y;
			r.y = 0f;
		}
		r.width = Mathf.Min(1f - r.x, r.width);
		r.height = Mathf.Min(1f - r.y, r.height);
		camera.rect = new Rect(0f, 0f, 1f, 1f);
		camera.ResetProjectionMatrix();
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		camera.rect = r;
		float num = 1f / r.width;
		float num2 = 1f / r.height;
		Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(num - 1f, num2 - 1f, 0f), Quaternion.identity, new Vector3(num, num2, 1f));
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(new Vector3((0f - r.x) * 2f * num, (0f - r.y) * 2f * num2, 0f), Quaternion.identity, Vector3.one);
		camera.projectionMatrix = matrix4x2 * matrix4x * projectionMatrix;
	}

	public static void ResetMatrices(this Camera camera)
	{
		camera.ResetWorldToCameraMatrix();
		camera.ResetProjectionMatrix();
		camera.ResetCullingMatrix();
	}

	public static Int2 PixelDimensions(this Camera camera)
	{
		return new Int2(camera.pixelWidth, camera.pixelHeight);
	}

	public static Vector2 GetFrustumRectSizeDelta(this Camera camera)
	{
		using PoolStructArrayHandle<Vector3> poolStructArrayHandle = Pools.UseArray<Vector3>(4);
		camera.CalculateFrustumCorners(ViewSpaceRect, 1f, Camera.MonoOrStereoscopicEye.Mono, poolStructArrayHandle);
		return poolStructArrayHandle[0].Project(AxisType.Z).Abs() * 2f;
	}

	public static Rect3D GetFrustumRectAtDepth(this Camera camera, float depth)
	{
		using PoolStructArrayHandle<Vector3> poolStructArrayHandle = Pools.UseArray<Vector3>(4);
		camera.CalculateFrustumCorners(ViewSpaceRect, depth, Camera.MonoOrStereoscopicEye.Mono, poolStructArrayHandle);
		return new Rect3D(poolStructArrayHandle[0], poolStructArrayHandle[1], poolStructArrayHandle[2]);
	}

	public static Rect3D GetWorldFrustumRectAtDepth(this Camera camera, float depth)
	{
		using PoolStructArrayHandle<Vector3> poolStructArrayHandle = Pools.UseArray<Vector3>(4);
		camera.CalculateFrustumCorners(ViewSpaceRect, depth, Camera.MonoOrStereoscopicEye.Mono, poolStructArrayHandle);
		return new Rect3D(camera.transform.position + camera.transform.TransformVector(poolStructArrayHandle[0]), camera.transform.position + camera.transform.TransformVector(poolStructArrayHandle[1]), camera.transform.position + camera.transform.TransformVector(poolStructArrayHandle[2]));
	}

	public static PoolKeepItemListHandle<Vector3> GetFrustumCornerPositions(this Camera camera, Matrix4x4? transformation = null, float? nearClipOverride = null, float? farClipOverride = null)
	{
		PoolKeepItemListHandle<Vector3> poolKeepItemListHandle = Pools.UseKeepItemList<Vector3>();
		using (PoolStructArrayHandle<Vector3> poolStructArrayHandle = Pools.UseArray<Vector3>(4))
		{
			camera.CalculateFrustumCorners(ViewSpaceRect, nearClipOverride ?? camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, poolStructArrayHandle);
			Vector3[] value = poolStructArrayHandle.value;
			foreach (Vector3 vector in value)
			{
				poolKeepItemListHandle.Add(camera.transform.position + camera.transform.TransformVector(vector));
			}
			camera.CalculateFrustumCorners(ViewSpaceRect, farClipOverride ?? camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, poolStructArrayHandle);
			value = poolStructArrayHandle.value;
			foreach (Vector3 vector2 in value)
			{
				poolKeepItemListHandle.Add(camera.transform.position + camera.transform.TransformVector(vector2));
			}
		}
		if (transformation.HasValue)
		{
			for (int j = 0; j < poolKeepItemListHandle.Count; j++)
			{
				poolKeepItemListHandle[j] = transformation.Value.MultiplyPoint(poolKeepItemListHandle[j]);
			}
		}
		return poolKeepItemListHandle;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void _GetMinMax(Vector3 point, ref Vector3 min, ref Vector3 max)
	{
		min = new Vector3((min.x >= point.x) ? point.x : min.x, (min.y >= point.y) ? point.y : min.y, (min.z >= point.z) ? point.z : min.z);
		max = new Vector3((max.x <= point.x) ? point.x : max.x, (max.y <= point.y) ? point.y : max.y, (max.z <= point.z) ? point.z : max.z);
	}

	public static RectZ GetScreenRect(this Bounds worldBounds, Camera camera)
	{
		Vector3 center = worldBounds.center;
		Vector3 extents = worldBounds.extents;
		Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		_GetMinMax(camera.WorldToScreenPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToScreenPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToScreenPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToScreenPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToScreenPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToScreenPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToScreenPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToScreenPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z)), ref min, ref max);
		return new RectZ(new Rect(min.x, min.y, max.x - min.x, max.y - min.y), (min.z + max.z) * 0.5f);
	}

	public static Rect3D GetWorldRect(this Bounds worldBounds, Camera camera)
	{
		PoolStructListHandle<Vector3> poolStructListHandle = worldBounds.GetViewPortRect(camera).Corners();
		for (int i = 0; i < poolStructListHandle.Count; i++)
		{
			poolStructListHandle[i] = camera.ViewportToWorldPoint(poolStructListHandle[i]);
		}
		using (poolStructListHandle)
		{
			return new Rect3D(poolStructListHandle);
		}
	}

	public static RectZ GetViewPortRect(this Bounds worldBounds, Camera camera)
	{
		Vector3 center = worldBounds.center;
		Vector3 extents = worldBounds.extents;
		Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		_GetMinMax(camera.WorldToViewportPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToViewportPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToViewportPoint(new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToViewportPoint(new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToViewportPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToViewportPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToViewportPoint(new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z)), ref min, ref max);
		_GetMinMax(camera.WorldToViewportPoint(new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z)), ref min, ref max);
		return new RectZ(new Rect(min.x, min.y, max.x - min.x, max.y - min.y), (min.z + max.z) * 0.5f);
	}

	public static Rect ScreenToViewPortRect(this Rect screenRect, Camera camera)
	{
		return new Rect(screenRect.x / (float)camera.pixelWidth, screenRect.y / (float)camera.pixelHeight, screenRect.width / (float)camera.pixelWidth, screenRect.height / (float)camera.pixelHeight);
	}

	public static void ClearRenderTarget(this CommandBuffer buffer, RenderTargetIdentifier renderTargetId, bool clearDepth = true, bool clearColor = true, Color? backgroundColor = null, float depth = 1f)
	{
		buffer.SetRenderTarget(renderTargetId);
		buffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor ?? Color.clear, depth);
	}

	public static Vector2 UVPixelDimensions(this RawImage rawImage)
	{
		if (!rawImage || !rawImage.mainTexture)
		{
			return Vector2.zero;
		}
		return new Vector2((float)rawImage.mainTexture.width * rawImage.uvRect.width, (float)rawImage.mainTexture.height * rawImage.uvRect.height);
	}

	public static float UVPixelAspectRatio(this RawImage rawImage)
	{
		Vector2 vector = rawImage.UVPixelDimensions();
		return vector.x / vector.y.InsureNonZero();
	}

	public static void AddAnimation(this IAnimatedUI animatedUI, Vector3 endPositionLocal, Vector3 endRotationLocal, Vector3 endScaleLocal, float duration = 0.25f, float delay = 0f)
	{
		animatedUI.animating = true;
		animatedUI.animations.Add(new UIAnimation(endPositionLocal, endRotationLocal, endScaleLocal, Time.time + delay, duration));
	}

	public static void AddStaggeredAnimations(this IAnimatedUI animatedUI, Vector3 endPositionLocal, Vector3 endRotationLocal, Vector3 endScaleLocal, float duration, float delay, int count)
	{
		for (int i = 0; i < count; i++)
		{
			animatedUI.AddAnimation(endPositionLocal, endRotationLocal, endScaleLocal, duration, (float)i * delay);
		}
	}

	public static void PopulateMesh(this IAnimatedUI animatedUI, VertexHelper toFill)
	{
		animatedUI.animating = !animatedUI.animations.IsNullOrEmpty();
		if (!animatedUI.animating)
		{
			return;
		}
		int currentVertCount = toFill.currentVertCount;
		int currentIndexCount = toFill.currentIndexCount;
		List<int> indices = toFill.GetIndices();
		int num = 0;
		for (int num2 = animatedUI.animations.Count - 1; num2 >= 0; num2--)
		{
			UIAnimation uIAnimation = animatedUI.animations[num2];
			float num3 = Time.time - uIAnimation.startTime;
			if (!(num3 <= 0f))
			{
				num++;
				float num4 = Mathf.Clamp01(num3 / uIAnimation.duration);
				float num5 = animatedUI.animationCurve.Evaluate(num4);
				Matrix4x4 matrix4x = Matrix4x4.TRS(uIAnimation.position * num5, Quaternion.Euler(uIAnimation.rotation * num5), Vector3.Lerp(Vector3.one, uIAnimation.scale, num5));
				byte alpha = (byte)Mathf.RoundToInt(animatedUI.alphaCurve.Evaluate(num4) * 255f);
				for (int i = 0; i < currentVertCount; i++)
				{
					UIVertex vertex = default(UIVertex);
					toFill.PopulateUIVertex(ref vertex, i);
					vertex.position = matrix4x.MultiplyPoint(vertex.position);
					vertex.color = vertex.color.SetAlpha32(alpha);
					toFill.AddVert(vertex);
				}
				int num6 = currentVertCount * num;
				for (int j = 0; j < currentIndexCount; j += 3)
				{
					toFill.AddTriangle(indices[j] + num6, indices[j + 1] + num6, indices[j + 2] + num6);
				}
				if (num4 >= 1f)
				{
					animatedUI.animations.RemoveAt(num2);
				}
			}
		}
	}

	public static void PopulateMesh(this IAnimatedUI animatedUI, Mesh mesh, Vector3[] originalVertices, Color32[] originalColors, Vector2[] originalUvs, Vector2[] originalUvs2, int[] originalIndices, Vector3[] originalNormals, Vector4[] originalTangents, Vector3 center)
	{
		animatedUI.animating = !animatedUI.animations.IsNullOrEmpty();
		if (!animatedUI.animating)
		{
			return;
		}
		using PoolKeepItemListHandle<Vector3> poolKeepItemListHandle = Pools.UseKeepItemList((IEnumerable<Vector3>)originalVertices);
		List<Vector3> value = poolKeepItemListHandle.value;
		using PoolKeepItemListHandle<int> poolKeepItemListHandle2 = Pools.UseKeepItemList((IEnumerable<int>)originalIndices);
		List<int> value2 = poolKeepItemListHandle2.value;
		using PoolKeepItemListHandle<Color32> poolKeepItemListHandle3 = Pools.UseKeepItemList((IEnumerable<Color32>)originalColors);
		List<Color32> value3 = poolKeepItemListHandle3.value;
		using PoolKeepItemListHandle<Vector2> poolKeepItemListHandle4 = Pools.UseKeepItemList((IEnumerable<Vector2>)originalUvs);
		List<Vector2> value4 = poolKeepItemListHandle4.value;
		using PoolKeepItemListHandle<Vector2> poolKeepItemListHandle5 = Pools.UseKeepItemList((IEnumerable<Vector2>)originalUvs2);
		List<Vector2> value5 = poolKeepItemListHandle5.value;
		using PoolKeepItemListHandle<Vector3> poolKeepItemListHandle6 = Pools.UseKeepItemList((IEnumerable<Vector3>)originalNormals);
		List<Vector3> value6 = poolKeepItemListHandle6.value;
		using PoolKeepItemListHandle<Vector4> poolKeepItemListHandle7 = Pools.UseKeepItemList((IEnumerable<Vector4>)originalTangents);
		List<Vector4> value7 = poolKeepItemListHandle7.value;
		int num = 0;
		Matrix4x4 matrix4x = Matrix4x4.Translate(center);
		Matrix4x4 matrix4x2 = Matrix4x4.Translate(-center);
		for (int num2 = animatedUI.animations.Count - 1; num2 >= 0; num2--)
		{
			UIAnimation uIAnimation = animatedUI.animations[num2];
			float num3 = Time.time - uIAnimation.startTime;
			if (!(num3 <= 0f))
			{
				num++;
				float num4 = Mathf.Clamp01(num3 / uIAnimation.duration);
				float num5 = animatedUI.animationCurve.Evaluate(num4);
				Matrix4x4 matrix4x3 = matrix4x * Matrix4x4.Scale(Vector3.Lerp(Vector3.one, uIAnimation.scale, num5)) * matrix4x2 * Matrix4x4.Rotate(Quaternion.Euler(uIAnimation.rotation * num5)) * Matrix4x4.Translate(uIAnimation.position * num5);
				byte alpha = (byte)Mathf.RoundToInt(animatedUI.alphaCurve.Evaluate(num4) * 255f);
				for (int i = 0; i < originalVertices.Length; i++)
				{
					value.Add(matrix4x3.MultiplyPoint(value[i]));
					value3.Add(value3[i].SetAlpha32(alpha));
					value4.Add(value4[i]);
					value5.Add(value5[i]);
					value6.Add(value6[i]);
					value7.Add(value7[i]);
				}
				int num6 = originalVertices.Length * num;
				for (int j = 0; j < originalIndices.Length; j += 3)
				{
					value2.Add(value2[j] + num6);
					value2.Add(value2[j + 1] + num6);
					value2.Add(value2[j + 2] + num6);
				}
				if (num4 >= 1f)
				{
					animatedUI.animations.RemoveAt(num2);
				}
			}
		}
		mesh.SetVertices(value);
		mesh.SetColors(value3);
		mesh.SetUVs(0, value4);
		mesh.SetUVs(1, value5);
		mesh.SetNormals(value6);
		mesh.SetTangents(value7);
		mesh.SetIndices(value2, MeshTopology.Triangles, 0);
	}

	private static float GetRelativeHeight(LODGroup lodGroup, Camera camera)
	{
		float magnitude = (lodGroup.transform.TransformPoint(lodGroup.localReferencePoint) - camera.transform.position).magnitude;
		return DistanceToRelativeHeight(camera, magnitude / QualitySettings.lodBias, lodGroup.GetWorldSpaceSize());
	}

	private static float DistanceToRelativeHeight(Camera camera, float distance, float size)
	{
		if (camera.orthographic)
		{
			return size * 0.5f / camera.orthographicSize;
		}
		float num = Mathf.Tan(MathF.PI / 180f * camera.fieldOfView * 0.5f);
		return size * 0.5f / (distance * num);
	}

	private static int GetMaxLOD(this LODGroup lodGroup)
	{
		return lodGroup.lodCount - 1;
	}

	private static float GetWorldSpaceSize(this LODGroup lodGroup)
	{
		return GetWorldSpaceScale(lodGroup.transform) * lodGroup.size;
	}

	private static float GetWorldSpaceScale(Transform t)
	{
		Vector3 lossyScale = t.lossyScale;
		return Mathf.Max(Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y)), Mathf.Abs(lossyScale.z));
	}

	public static int GetVisibleLOD(this LODGroup lodGroup, Camera camera = null)
	{
		LOD[] lODs = lodGroup.GetLODs();
		float relativeHeight = GetRelativeHeight(lodGroup, camera ?? Camera.main ?? Camera.current);
		int result = lodGroup.GetMaxLOD();
		for (int i = 0; i < lODs.Length; i++)
		{
			LOD lOD = lODs[i];
			if (relativeHeight >= lOD.screenRelativeTransitionHeight)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public static void RemoveUnusedDetails(this TerrainData terrainData)
	{
	}

	public static void RemoveUnusedTrees(this TerrainData terrainData)
	{
	}

	public static int GetSelectedDetailIndex(this Terrain terrain)
	{
		return -1;
	}

	public static float SampleAt(AnimationType animType, float t, float repeat, out int additiveCount)
	{
		float num = t * repeat;
		int num2 = (int)num;
		float num3 = num - (float)num2;
		additiveCount = 0;
		switch (animType)
		{
		case AnimationType.LoopAdditive:
			additiveCount = num2;
			break;
		case AnimationType.Pendulum:
			if (num2 % 2 != 0)
			{
				num3 = 1f - num3;
			}
			break;
		}
		return num3;
	}

	public static float Evaluate(this AnimationType animationType, AnimationCurve curve, float t)
	{
		int additiveCount;
		float num = SampleAt(animationType, t, 1f, out additiveCount);
		float time = curve[curve.length - 1].time;
		return curve.Evaluate(num * time) + curve.Evaluate(time) * (float)additiveCount;
	}

	public static float SampleAtSimple(AnimationType animType, float t)
	{
		int num = (int)t;
		float num2 = t - (float)num;
		switch (animType)
		{
		case AnimationType.LoopAdditive:
			num2 = t;
			break;
		case AnimationType.Pendulum:
			if (num % 2 != 0)
			{
				num2 = 1f - num2;
			}
			break;
		}
		return num2;
	}

	public static void FitUVsIntoRect(this Mesh mesh, Rect rect, bool flipHorizontal = false, bool flipVertical = false)
	{
		Vector2[] uv = mesh.uv;
		Vector2[] array = new Vector2[uv.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Vector2(uv[i].x * rect.width + rect.x, uv[i].y * rect.height + rect.y);
		}
		if (flipHorizontal)
		{
			for (int j = 0; j < array.Length; j++)
			{
				array[j].x = 1f - array[j].x;
			}
		}
		if (flipVertical)
		{
			for (int k = 0; k < array.Length; k++)
			{
				array[k].y = 1f - array[k].y;
			}
		}
		mesh.uv = array;
	}

	public static void SetUVsIntoRect(this Mesh mesh, Rect rect)
	{
		Vector2[] uv = mesh.uv;
		Vector2[] array = new Vector2[uv.Length];
		Rect r = new Rect(uv[0], Vector2.zero);
		Vector2[] array2 = uv;
		foreach (Vector2 position in array2)
		{
			r = r.Encapsulate(position);
		}
		Vector2 vector = rect.min - r.min;
		Vector2 multiplier = rect.size.Multiply(r.size.InsureNonZero().Inverse());
		for (int j = 0; j < uv.Length; j++)
		{
			array[j] = uv[j].Multiply(multiplier) + vector;
		}
		mesh.uv = array;
	}

	public static Texture2D RenderMeshIdentity(MeshRenderer meshRenderer, int resolution, float padding = 0f, Bounds? boundsToRender = null, string shader = "Process/Identity", bool mipmap = true, bool linear = true, Vector3? scale = null)
	{
		GameObject gameObject = meshRenderer.gameObject;
		int layer = gameObject.layer;
		Material[] materials = meshRenderer.materials;
		meshRenderer.materials = new Material[1] { GPUImage.GetMaterial(shader) };
		meshRenderer.material.mainTexture = materials[0].mainTexture;
		meshRenderer.material.color = Color.white;
		gameObject.layer = 31;
		GameObject gameObject2 = new GameObject("RenderMeshIdentityCamera");
		Camera camera = gameObject2.AddComponent<Camera>();
		camera.renderingPath = RenderingPath.Forward;
		camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		camera.clearFlags = CameraClearFlags.Color;
		camera.cullingMask = int.MinValue;
		camera.worldToCameraMatrix = meshRenderer.worldToLocalMatrix;
		Bounds bounds = boundsToRender ?? meshRenderer.GetComponent<MeshFilter>().sharedMesh.bounds;
		Vector2 v = bounds.size.Project(AxisType.Z);
		float num = v.AbsMin();
		float num2 = v.AbsMax();
		AxisType axisType = v.AbsMaxAxis();
		int num3 = Mathf.RoundToInt((float)resolution * (num / num2));
		Int2 @int = new Int2((axisType == AxisType.X) ? resolution : num3, (axisType == AxisType.Y) ? resolution : num3);
		Vector2 vector = new Vector2(1f / (float)@int.x, 1f / (float)@int.y);
		bounds = bounds.Pad(new Vector3(padding * vector.x, padding * vector.y, 0f));
		camera.projectionMatrix = Matrix4x4.Ortho(bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y, bounds.min.z - 1f, bounds.max.z + 1f) * Matrix4x4.Scale(scale ?? Vector3.one);
		camera.targetTexture = GPUImage.GetTempRenderTexture(@int.x, @int.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 8);
		camera.Render();
		Texture2D texture2D = camera.targetTexture.IntoTexture2D(new Texture2D(1, 1, TextureFormat.RGBA32, mipmap, linear));
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.filterMode = FilterMode.Trilinear;
		texture2D.anisoLevel = 16;
		camera.targetTexture = null;
		GPUImage.ReleaseTempRenderTextures();
		gameObject2.Destroy();
		meshRenderer.materials = materials;
		gameObject.layer = layer;
		return texture2D;
	}

	public static Texture2D RenderMeshIdentity(Mesh mesh, Texture2D texture, int resolution, float padding = 0f, Bounds? boundsToRender = null, string shader = "Process/Identity", bool mipmap = true, bool linear = true, Vector3? scale = null)
	{
		GameObject gameObject = Pools.Unpool(RenderMeshIdentityBlueprint, setActive: true, autoRepool: false);
		gameObject.GetComponent<MeshFilter>().mesh = mesh;
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		component.material.mainTexture = texture;
		Texture2D result = RenderMeshIdentity(component, resolution, padding, boundsToRender, shader, mipmap, linear, scale);
		Pools.Repool(gameObject);
		return result;
	}

	public static Texture2D RenderUIObject(GameObject uiObjectToRender, bool mipmap = false)
	{
		uiObjectToRender.layer = 31;
		Camera camera = new GameObject("RenderUIObject").AddComponent<Camera>();
		camera.gameObject.layer = 31;
		camera.renderingPath = RenderingPath.Forward;
		camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		camera.clearFlags = CameraClearFlags.Color;
		camera.cullingMask = int.MinValue;
		Canvas canvas = camera.gameObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		canvas.worldCamera = camera;
		canvas.transform.localScale = Vector3.one;
		ContentSizeFitter contentSizeFitter = canvas.gameObject.AddComponent<HorizontalLayoutGroup>().gameObject.AddComponent<ContentSizeFitter>();
		ContentSizeFitter.FitMode horizontalFit = (contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize);
		contentSizeFitter.horizontalFit = horizontalFit;
		uiObjectToRender.transform.SetParent(canvas.transform, worldPositionStays: false);
		LayoutRebuilder.MarkLayoutForRebuild(contentSizeFitter.transform as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentSizeFitter.transform as RectTransform);
		uiObjectToRender.transform.localPosition = new Vector3(0f, 0f, 1f);
		RectTransform obj = uiObjectToRender.transform as RectTransform;
		Rect3D worldRect3DRecursive = obj.GetWorldRect3DRecursive();
		Vector2 preferredSize = obj.GetPreferredSize();
		Int2 @int = new Int2(new Vector2(worldRect3DRecursive.width, worldRect3DRecursive.height));
		RectTransform obj2 = canvas.transform as RectTransform;
		obj2.SetAnchors(Vector2.zero);
		obj2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, @int.x);
		obj2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, @int.y);
		camera.projectionMatrix = Matrix4x4.Ortho(worldRect3DRecursive.min.x, worldRect3DRecursive.max.x, worldRect3DRecursive.min.y, worldRect3DRecursive.max.y, 0.5f, 1.5f);
		camera.targetTexture = GPUImage.GetTempRenderTexture(@int.x, @int.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 8);
		obj.Stretch();
		foreach (Graphic item in obj.gameObject.GetComponentsInChildrenPooled<Graphic>())
		{
			if (item.material.shader == DefaultUIShader)
			{
				item.material = UIDefaultCompositeMaterial;
			}
			else if (item.material.HasProperty(_SrcBlendA))
			{
				item.material = UnityEngine.Object.Instantiate(item.material);
				item.material.SetInt(_SrcBlendA, 8);
				item.material.SetInt(_DstBlendA, 1);
			}
		}
		camera.Render();
		Texture2D texture2D = camera.targetTexture.IntoTexture2D(new Texture2D(1, 1, TextureFormat.ARGB32, mipmap, linear: false));
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.filterMode = FilterMode.Trilinear;
		texture2D.anisoLevel = 16;
		camera.targetTexture = null;
		GPUImage.ReleaseTempRenderTextures();
		UnityEngine.Object.Destroy(camera.gameObject);
		Texture2D texture2D2 = texture2D;
		if ((worldRect3DRecursive.size - preferredSize).Max() >= 1f)
		{
			texture2D2 = texture2D.AutoCropAlphaCPU(0, 25);
		}
		if (texture2D2 != texture2D)
		{
			UnityEngine.Object.Destroy(texture2D);
		}
		return texture2D2;
	}

	public static RenderTexture AutoAlpha(RenderTexture source, float blurAmount = 1f, float edgeThreshold = 0.075f, float distanceMultiplier = 1f, float alphaBlur = 2f, byte noiseReduction = 2, float boundaryThresholdPixels = 3f, bool lookForOpenBoundaries = false, float keepWeakContours = 0f)
	{
		RenderTexture source2 = source;
		source = GPUImage.CannyEdgeDetect(source, blurAmount, edgeThreshold, noiseReduction);
		float num = (float)source.width / (float)source.height;
		float num2 = 1f / num;
		float num3 = ((num2 > num) ? num : 1f);
		float num4 = ((num > num2) ? num2 : 1f);
		Rect rect = new Rect(-1f * num3, -1f * num4, 2f * num3, 2f * num4);
		int num5 = (int)((float)source.MaxDimension() / 240f);
		float num6 = rect.width / (float)source.width;
		float num7 = num6 * (float)Math.Max(3, num5 + num5 - 1) * MathUtil.SqrtTwo;
		List<Vector2> list = source.ToTexture2D(mipmap: false, 0, FilterMode.Point).BinaryToPoints(num5, rect);
		if (list.Count < 3)
		{
			Debug.Log("~Binary To Points result only contained [" + list.Count + "] points, replacing with default circle.");
			list = MathUtil.CreatePointsOnCircle(Vector2.zero, 0.25f);
		}
		distanceMultiplier = Math.Max(1f, distanceMultiplier);
		List<Vector2> poly = MathUtil.ConcaveHull(list, num7 * distanceMultiplier, rect, num6 * boundaryThresholdPixels, lookForOpenBoundaries ? new float?(boundaryThresholdPixels + 1f) : null, Mathf.Lerp(num7, rect.size.Max() * 0.1f, keepWeakContours));
		GameObject gameObject = new GameObject("PolyToPoints");
		gameObject.AddComponent<MeshFilter>();
		gameObject.AddComponent<MeshRenderer>();
		gameObject.GetComponent<MeshFilter>().mesh = MathUtil.PolyToMesh(poly, Color.white);
		gameObject.GetComponent<Renderer>().material = GPUImage.GetMaterial("Process/Identity");
		gameObject.GetComponent<Renderer>().material.mainTexture = null;
		gameObject.GetComponent<Renderer>().material.color = Color.white;
		gameObject.layer = 31;
		GameObject gameObject2 = new GameObject("AlphaMeshCamera");
		Camera camera = gameObject2.AddComponent<Camera>();
		camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		camera.clearFlags = CameraClearFlags.Color;
		camera.cullingMask = int.MinValue;
		camera.worldToCameraMatrix = Matrix4x4.identity;
		camera.projectionMatrix = Matrix4x4.Ortho(rect.xMin, rect.xMax, rect.yMin, rect.yMax, -1f, 1f);
		RenderTexture source3 = (camera.targetTexture = GPUImage.GetTempRenderTexture(source.width * 2, source.height * 2));
		camera.Render();
		source3 = GPUImage.Blur(source3, alphaBlur);
		gameObject.Destroy();
		gameObject2.Destroy();
		return GPUImage.SetAlpha(source2, source3);
	}

	public static Texture2D FindAlphaShape(out bool textureAlreadyHadAlpha, Texture2D texture, Color alphaColor, float blurAmount = 1.5f, float edgeThreshold = 0.025f, float distanceMultiplier = 1f, float feathering = 2f, byte noiseReduction = 2, float boundaryThresholdPixels = 3f, bool lookForOpenBoundaries = false, float keepWeakContours = 0f, float alphaThreshold = 0.25f, int outputMaxResolution = 512)
	{
		GPUImage.BeginProcess();
		RenderTexture source = texture.ToRenderTexture();
		textureAlreadyHadAlpha = true;
		if (texture.format == TextureFormat.RGB24 || !texture.HasTransparency())
		{
			source = AutoAlpha(source, blurAmount, edgeThreshold, distanceMultiplier, feathering, noiseReduction, boundaryThresholdPixels, lookForOpenBoundaries, keepWeakContours);
			textureAlreadyHadAlpha = false;
			if (feathering > 1f)
			{
				source = GPUImage.AlphaCorrode(source, Mathf.CeilToInt(feathering) - 1);
			}
		}
		else
		{
			source = RenderMeshIdentity(TextureTo3DCutoutImmediate(texture, null, Vector2.zero, 0f), texture, texture.MaxDimension(), 0f, UVCoords.Default).ToRenderTexture().Flip(flipHorizontal: true, flipVertical: false);
		}
		source = GPUImage.AutoCropAlpha(source.ToTexture2D(), 2, outputMaxResolution, alphaThreshold).ToRenderTexture();
		if (!textureAlreadyHadAlpha)
		{
			source = GPUImage.SetAlphaColor(source, alphaColor);
		}
		texture = source.ToTexture2D();
		GPUImage.EndProcess();
		return texture;
	}

	public static SpriteRenderer TextureToSprite(this GameObject go, Texture2D t)
	{
		SpriteRenderer spriteRenderer = go.gameObject.AddComponent<SpriteRenderer>();
		Sprite sprite = Sprite.Create(pixelsPerUnit: t.MaxDimension(), texture: t, rect: new Rect(0f, 0f, t.width, t.height), pivot: new Vector2(0f, 0f), extrude: 0u, meshType: SpriteMeshType.Tight);
		spriteRenderer.sprite = sprite;
		go.transform.localScale = new Vector3(1f, 1f, 1f);
		return spriteRenderer;
	}

	public static MeshData PolyDataToMeshData(PolygonColliderData poly, Rect? bounds, Vector2 pivot, float textureAspectRatio)
	{
		Rect boundingRect = poly.GetBoundingRect();
		Rect rect = (bounds.HasValue ? bounds.Value.GetOptimalInscirbedAspectRatioRect(boundingRect.width / boundingRect.height, pivot) : boundingRect);
		rect.center = rect.center.Multiply(new Vector2(-1f, 1f));
		float num = rect.width / boundingRect.width;
		float num2 = rect.height / boundingRect.height;
		ContourOrientation forceOrientation = ContourOrientation.Original;
		WindingRule windingRule = WindingRule.Positive;
		ElementType elementType = ElementType.Polygons;
		int polySize = 3;
		Tess tess = new Tess();
		for (int i = 0; i < poly.pathCount; i++)
		{
			Vector2[] path = poly.GetPath(i);
			ContourVertex[] array = new ContourVertex[path.Length];
			for (int j = 0; j < path.Length; j++)
			{
				array[j].Position.X = path[j].x;
				array[j].Position.Y = path[j].y;
				array[j].Position.Z = 0f;
			}
			tess.AddContour(array, forceOrientation);
		}
		tess.Tessellate(windingRule, elementType, polySize);
		Vector3[] array2 = new Vector3[tess.VertexCount];
		Vector3[] array3 = new Vector3[tess.VertexCount];
		Vector2[] array4 = new Vector2[tess.VertexCount];
		float num3 = ((textureAspectRatio < 1f) ? (1f / textureAspectRatio) : 1f);
		float num4 = ((textureAspectRatio > 1f) ? textureAspectRatio : 1f);
		for (int k = 0; k < tess.VertexCount; k++)
		{
			Vector2 vector = new Vector2(tess.Vertices[k].Position.X, tess.Vertices[k].Position.Y);
			array2[k] = new Vector3(0f - (rect.xMin + num * (vector.x - boundingRect.xMin)), rect.yMin + num2 * (vector.y - boundingRect.yMin), 0f);
			array3[k] = Vector3.forward;
			array4[k] = new Vector2(vector.x * num3, vector.y * num4);
		}
		return new MeshData(tess.Elements, array2, array3, array4);
	}

	public static IEnumerator TextureTo3DCutout(Texture2D texture, Rect? boundingRect, Vector2 pivot, float thickness = 0.0075f, bool flipXAxis = false)
	{
		GameObject obj = new GameObject
		{
			layer = 31
		};
		GPUImage.BeginProcess();
		Texture2D texture2D = GPUImage.ScaleTexture(texture, texture.MaxDimension() * 2).ToTexture2D();
		SpriteRenderer spriteRenderer = obj.TextureToSprite(texture2D ?? texture);
		PolygonColliderData poly = new PolygonColliderData(obj.AddComponent<PolygonCollider2D>());
		float textureAspectRatio = spriteRenderer.sprite.texture.AspectRatio();
		obj.Destroy();
		if ((bool)texture2D)
		{
			UnityEngine.Object.Destroy(texture2D);
		}
		GPUImage.EndProcess();
		Quaternion rotation = (flipXAxis ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity);
		thickness *= (float)((!flipXAxis) ? 1 : (-1));
		MeshData meshData = PolyDataToMeshData(poly, boundingRect, pivot, textureAspectRatio);
		if (Math.Abs(thickness) > 0f)
		{
			yield return null;
			MeshExtrusion.ExtrudeMesh(meshData, new Matrix4x4[2]
			{
				Matrix4x4.TRS(Vector3.forward * (0f - thickness), rotation, Vector3.one),
				Matrix4x4.TRS(Vector3.forward * thickness, rotation, Vector3.one)
			}, invertFaces: false, calculateNormals: false);
		}
		Mesh mesh = meshData.ToMesh();
		yield return null;
		mesh.RecalculateNormals();
		yield return null;
		mesh.RecalculateTangents();
		yield return mesh;
	}

	public static Mesh TextureTo3DCutoutImmediate(Texture2D texture, Rect? boundingRect, Vector2 pivot, float thickness = 0.0075f, bool flipXAxis = false)
	{
		IEnumerator enumerator = TextureTo3DCutout(texture, boundingRect, pivot, thickness, flipXAxis);
		Mesh result = null;
		while (enumerator.MoveNext())
		{
			result = enumerator.Current as Mesh;
		}
		return result;
	}

	public static IEnumerator TextureToEquipmentCutout(Texture2D texture)
	{
		return TextureTo3DCutout(texture, EQUIPMENT_MESH_BOUNDS, EQUIPMENT_MESH_PIVOT, EQUIPMENT_MESH_THICKNESS, flipXAxis: true);
	}

	public static int GetQualitySettingIndex(string qualitySettingName)
	{
		if (!_QualityLevelsByName.ContainsKey(qualitySettingName))
		{
			return 0;
		}
		return _QualityLevelsByName[qualitySettingName];
	}

	public static string GetQualitySettingName(int qualitySettingIndex)
	{
		if (QualitySettings.names.Length <= qualitySettingIndex)
		{
			return QualitySettings.names[0];
		}
		return QualitySettings.names[qualitySettingIndex];
	}

	public static string GetCurrentQualitySettingName()
	{
		return GetQualitySettingName(QualitySettings.GetQualityLevel());
	}

	public static QualitySettingData GetQualitySettingsData(int qualitySettingIndex)
	{
		return _QualitySettingsData[qualitySettingIndex];
	}

	public static QualitySettingData GetQualitySettingsData(string qualitySettingName)
	{
		return GetQualitySettingsData(GetQualitySettingIndex(qualitySettingName));
	}

	public static int GetRecommendedQualitySettingIndex()
	{
		int num = QualitySettings.names.Length - 2;
		int num2 = SystemInfo.graphicsMemorySize / 1000;
		if (num2 >= 4)
		{
			return Mathf.Max(0, num);
		}
		if (num2 >= 3)
		{
			return Mathf.Max(0, num - 1);
		}
		if (num2 >= 2)
		{
			return Mathf.Max(0, num - 2);
		}
		if (num2 >= 1)
		{
			return Mathf.Max(0, num - 3);
		}
		return Mathf.Max(0, num - 4);
	}
}
