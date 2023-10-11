using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using UnityEngine.Sprites;
using UnityEngine.UI;

public static class GameUtil
{
	private static List<MeshFilter> _MeshFilters = new List<MeshFilter>();

	private static List<Vector3> _MeshVertices = new List<Vector3>();

	private static Collider[] _Colliders;

	private static readonly Vector2[] s_VertScratch = new Vector2[4];

	private static readonly Vector2[] s_UVScratch = new Vector2[4];

	private static Func<VertexHelper, List<int>> _GetVertexHelperIndices;

	public static bool isPlaying { get; private set; }

	private static Collider[] Colliders => _Colliders ?? (_Colliders = new Collider[100]);

	private static Func<VertexHelper, List<int>> GetVertexHelperIndices => _GetVertexHelperIndices ?? (_GetVertexHelperIndices = typeof(VertexHelper).GetField("m_Indices", BindingFlags.Instance | BindingFlags.NonPublic).GetValueGetter<VertexHelper, List<int>>());

	public static float smoothUnscaledDeltaTime => Time.unscaledDeltaTime;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void _Initialize()
	{
		isPlaying = Application.isPlaying;
	}

	public static T FindComponentWithTag<T>(string tag) where T : Component
	{
		GameObject gameObject = GameObject.FindWithTag(tag);
		if (!gameObject)
		{
			return null;
		}
		return gameObject.GetComponent<T>();
	}

	public static T InstantiateIfNotAnInstance<T>(T obj) where T : UnityEngine.Object
	{
		if (obj.GetInstanceID() >= 0)
		{
			return UnityEngine.Object.Instantiate(obj);
		}
		return obj;
	}

	public static void Destroy(this GameObject obj)
	{
		UnityEngine.Object.DestroyImmediate(obj);
	}

	public static void DestroyChildren(this GameObject obj, bool immediate = true)
	{
		foreach (Transform item in obj.transform.ChildrenSafe())
		{
			if (immediate)
			{
				UnityEngine.Object.DestroyImmediate(item.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
	}

	public static bool IsDestroyed(this UnityEngine.Object obj)
	{
		if (!(obj == null))
		{
			return obj.Equals(null);
		}
		return true;
	}

	public static void DestroySafe<T>(this UnityEngine.Object obj, ref T objToDestroy) where T : UnityEngine.Object
	{
		if ((bool)(UnityEngine.Object)objToDestroy)
		{
			UnityEngine.Object.Destroy(objToDestroy);
		}
		objToDestroy = null;
	}

	public static void DestroyHierarchyBottomUp(this GameObject gameObject)
	{
		foreach (Transform item in Pools.UseKeepItemList(gameObject.transform.GetHierarchyBottomUp()))
		{
			UnityEngine.Object.DestroyImmediate(item.gameObject);
		}
	}

	public static GameObject Clone(this GameObject obj, bool worldPositionStays = true)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(obj, obj.transform.position, obj.transform.rotation);
		gameObject.transform.SetParent(obj.transform.parent, worldPositionStays);
		gameObject.transform.localScale = obj.transform.localScale;
		return gameObject;
	}

	public static Rect3D GetBillboardedWorldRect3DMeshBounds(this GameObject go, Camera camera, float zPositionLerp = 0.5f, int maxVertexCount = 15000)
	{
		go.GetComponentsInChildren(_MeshFilters);
		if (_MeshFilters.Count == 0)
		{
			if (!(go.transform is RectTransform))
			{
				return new Rect3D(go.transform, Vector2.one);
			}
			return new Rect3D((RectTransform)go.transform);
		}
		Vector3 min = new Vector3(0f, 0f, camera.nearClipPlane);
		Vector3 max = new Vector3(1f, 1f, camera.farClipPlane);
		Vector3 v = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 v2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		int num = 0;
		foreach (MeshFilter meshFilter in _MeshFilters)
		{
			num += meshFilter.sharedMesh.vertexCount;
		}
		foreach (MeshFilter meshFilter2 in _MeshFilters)
		{
			Matrix4x4 localToWorldMatrix = meshFilter2.transform.localToWorldMatrix;
			if (num <= maxVertexCount)
			{
				meshFilter2.sharedMesh.GetVertices(_MeshVertices);
			}
			else
			{
				meshFilter2.sharedMesh.bounds.GetVertices(_MeshVertices);
			}
			for (int i = 0; i < _MeshVertices.Count; i++)
			{
				Vector3 v3 = camera.WorldToViewportPoint(localToWorldMatrix.MultiplyPoint3x4(_MeshVertices[i]));
				if (v3.IsBetweenRange(min, max))
				{
					v.x = ((v.x < v3.x) ? v.x : v3.x);
					v.y = ((v.y < v3.y) ? v.y : v3.y);
					v.z = ((v.z < v3.z) ? v.z : v3.z);
					v2.x = ((v2.x > v3.x) ? v2.x : v3.x);
					v2.y = ((v2.y > v3.y) ? v2.y : v3.y);
					v2.z = ((v2.z > v3.z) ? v2.z : v3.z);
				}
			}
		}
		v = v.Clamp(min, max);
		v2 = v2.Clamp(min, max);
		PoolStructListHandle<Vector3> poolStructListHandle = new RectZ(new Rect(v.x, v.y, v2.x - v.x, v2.y - v.y), Mathf.Lerp(v.z, v2.z, zPositionLerp)).Corners();
		for (int j = 0; j < poolStructListHandle.Count; j++)
		{
			poolStructListHandle[j] = camera.ViewportToWorldPoint(poolStructListHandle[j]);
		}
		using (poolStructListHandle)
		{
			return new Rect3D(poolStructListHandle);
		}
	}

	public static PoolKeepItemListHandle<Triangle> GetTriangleData(this Mesh mesh)
	{
		PoolKeepItemListHandle<Triangle> poolKeepItemListHandle = Pools.UseKeepItemList<Triangle>();
		using PoolKeepItemListHandle<Vector3> poolKeepItemListHandle2 = Pools.UseKeepItemList<Vector3>();
		using PoolKeepItemListHandle<int> poolKeepItemListHandle3 = Pools.UseKeepItemList<int>();
		mesh.GetVertices(poolKeepItemListHandle2);
		mesh.GetTriangles(poolKeepItemListHandle3, 0);
		for (int i = 0; i < poolKeepItemListHandle3.Count; i += 3)
		{
			poolKeepItemListHandle.Add(new Triangle(poolKeepItemListHandle2, poolKeepItemListHandle3, i));
		}
		return poolKeepItemListHandle;
	}

	public static bool ContainsPoint(this Collider collider, Vector3 point)
	{
		int num = Physics.OverlapSphereNonAlloc(point, 0f, Colliders, 1 << collider.gameObject.layer, (!collider.isTrigger) ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
		for (int i = 0; i < num; i++)
		{
			if (Colliders[i] == collider)
			{
				return true;
			}
		}
		return false;
	}

	public static Collider CopyColliderTo(this Collider collider, GameObject gameObject)
	{
		if (collider is BoxCollider)
		{
			BoxCollider boxCollider = collider as BoxCollider;
			BoxCollider boxCollider2 = gameObject.AddComponent<BoxCollider>();
			boxCollider2.center = boxCollider.center;
			boxCollider2.size = boxCollider.size;
			return boxCollider2;
		}
		if (collider is SphereCollider)
		{
			SphereCollider sphereCollider = collider as SphereCollider;
			SphereCollider sphereCollider2 = gameObject.AddComponent<SphereCollider>();
			sphereCollider2.center = sphereCollider.center;
			sphereCollider2.radius = sphereCollider.radius;
			return sphereCollider2;
		}
		if (collider is MeshCollider)
		{
			MeshCollider meshCollider = collider as MeshCollider;
			MeshCollider meshCollider2 = gameObject.AddComponent<MeshCollider>();
			meshCollider2.cookingOptions = meshCollider.cookingOptions;
			meshCollider2.convex = meshCollider.convex;
			meshCollider2.sharedMesh = meshCollider.sharedMesh;
			return meshCollider2;
		}
		if (collider is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			CapsuleCollider capsuleCollider2 = gameObject.AddComponent<CapsuleCollider>();
			capsuleCollider2.center = capsuleCollider.center;
			capsuleCollider2.direction = capsuleCollider.direction;
			capsuleCollider2.height = capsuleCollider.height;
			capsuleCollider2.radius = capsuleCollider.radius;
			return capsuleCollider2;
		}
		return null;
	}

	public static IEnumerable<Transform> Children(this Transform transform)
	{
		foreach (Transform item in transform)
		{
			yield return item;
		}
	}

	public static PoolKeepItemListHandle<Transform> ChildrenSafe(this Transform transform)
	{
		PoolKeepItemListHandle<Transform> poolKeepItemListHandle = Pools.UseKeepItemList<Transform>();
		for (int i = 0; i < transform.childCount; i++)
		{
			poolKeepItemListHandle.Add(transform.GetChild(i));
		}
		return poolKeepItemListHandle;
	}

	public static void SetChildrenActive(this Transform transform, bool active)
	{
		foreach (Transform item in transform)
		{
			item.gameObject.SetActive(active);
		}
	}

	public static IEnumerable<Transform> Siblings(this Transform transform)
	{
		if (transform.parent == null)
		{
			yield return transform;
		}
		foreach (Transform item in transform.parent.Children())
		{
			yield return item;
		}
	}

	public static Transform GetSiblingTransformRelativeTo<T>(this Transform transform) where T : Component
	{
		Transform transform2 = transform;
		while (transform2.parent != null)
		{
			if ((bool)(UnityEngine.Object)transform2.parent.GetComponent<T>())
			{
				return transform2;
			}
			transform2 = transform2.parent;
		}
		return transform;
	}

	public static Transform GetSiblingTransformRelativeTo(this Transform transform, Func<Transform, bool> validParent, bool returnNullOnFail = false)
	{
		Transform transform2 = transform;
		while (transform2.parent != null)
		{
			if (validParent(transform2.parent))
			{
				return transform2;
			}
			transform2 = transform2.parent;
		}
		if (!returnNullOnFail)
		{
			return transform;
		}
		return null;
	}

	public static int GetSiblingIndexRelativeTo<T>(this Transform transform) where T : Component
	{
		return transform.GetSiblingTransformRelativeTo<T>().GetSiblingIndex();
	}

	public static IEnumerable<Transform> Parents(this Transform transform, bool includeSelf = false)
	{
		if (includeSelf)
		{
			yield return transform;
		}
		while (transform.parent != null)
		{
			yield return transform.parent;
			transform = transform.parent;
		}
	}

	public static IEnumerable<Transform> ChildrenRecursive(this Transform transform)
	{
		foreach (Transform child in transform)
		{
			yield return child;
			foreach (Transform item in child.ChildrenRecursive())
			{
				yield return item;
			}
		}
	}

	public static PoolKeepItemListHandle<Transform> ChildrenRecursiveNonAlloc(this Transform transform, bool includeSelf = false, Func<Transform, bool> stopAt = null, PoolKeepItemListHandle<Transform> output = null)
	{
		output = output ?? Pools.UseKeepItemList<Transform>();
		if (includeSelf)
		{
			output.Add(transform);
		}
		foreach (Transform item in transform)
		{
			if (stopAt == null || !stopAt(item))
			{
				item.ChildrenRecursiveNonAlloc(includeSelf: true, stopAt, output);
			}
		}
		return output;
	}

	public static IEnumerable<Transform> GetHierarchyBottomUp(this Transform transform)
	{
		foreach (Transform item in transform)
		{
			foreach (Transform item2 in item.GetHierarchyBottomUp())
			{
				yield return item2;
			}
		}
		yield return transform;
	}

	public static Vector3 GetAxis(this Transform t, AxisType axis)
	{
		return axis switch
		{
			AxisType.X => t.right, 
			AxisType.Y => t.up, 
			AxisType.Z => t.forward, 
			_ => t.right, 
		};
	}

	public static Vector3 GetWorldScale(this Transform transform)
	{
		return transform.lossyScale;
	}

	public static void SetWorldScale(this Transform transform, Vector3 worldScale)
	{
		if (!transform.parent)
		{
			transform.localScale = worldScale;
			return;
		}
		Vector3 lossyScale = transform.parent.lossyScale;
		transform.localScale = new Vector3(worldScale.x / lossyScale.x, worldScale.y / lossyScale.y, worldScale.z / lossyScale.z);
	}

	public static void FillRect(this RectTransform rect, RectTransform fillRect)
	{
		Vector2 multiplier = rect.localScale.Sign().Project(AxisType.Z);
		Vector2 v = rect.rect.min.Multiply(multiplier);
		Vector2 v2 = rect.rect.max.Multiply(multiplier);
		Vector3 vector = Vector2.Max(Vector2.zero, rect.TransformPoint(v.Unproject(AxisType.Z)).Project(AxisType.Z) - fillRect.TransformPoint(fillRect.rect.min.Unproject(AxisType.Z)).Project(AxisType.Z)).Unproject(AxisType.Z);
		vector -= Vector2.Max(Vector2.zero, fillRect.TransformPoint(fillRect.rect.max.Unproject(AxisType.Z)).Project(AxisType.Z) - rect.TransformPoint(v2.Unproject(AxisType.Z)).Project(AxisType.Z)).Unproject(AxisType.Z);
		rect.transform.position -= vector;
	}

	public static void Stretch(this RectTransform rect)
	{
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.pivot = new Vector2(0.5f, 0.5f);
		Vector2 offsetMin = (rect.offsetMax = Vector2.zero);
		rect.offsetMin = offsetMin;
	}

	public static void CopyRect(this RectTransform rect, RectTransform copyFrom)
	{
		Transform transform = null;
		if (rect.parent != copyFrom.parent)
		{
			transform = rect.parent;
			rect.SetParent(copyFrom.parent, worldPositionStays: false);
		}
		rect.transform.CopyFrom(copyFrom);
		rect.anchorMin = copyFrom.anchorMin;
		rect.anchorMax = copyFrom.anchorMax;
		rect.pivot = copyFrom.pivot;
		rect.offsetMin = copyFrom.offsetMin;
		rect.offsetMax = copyFrom.offsetMax;
		rect.sizeDelta = copyFrom.sizeDelta;
		if ((bool)transform)
		{
			rect.SetParent(transform, worldPositionStays: true);
		}
	}

	public static void CopyRect3D(this RectTransform rect, RectTransform copyFrom)
	{
		Vector3 lossyScale = copyFrom.lossyScale;
		rect.SetWorldCorners(copyFrom.GetWorldRect3D().Scale(lossyScale.Project(AxisType.Z).InsureNonZero().Inverse()));
		rect.SetWorldScale(lossyScale);
	}

	public static IEnumerator RotateTo(this Transform transform, Vector3 targetForwardDirection, float maxAngularSpeed, float angularAcceleration, bool useScaledTime = true)
	{
		float angularSpeed2 = 0f;
		while (true)
		{
			float num = (useScaledTime ? Time.deltaTime : Time.fixedDeltaTime);
			Vector3 forward = transform.forward;
			float num2 = Vector3.Angle(forward, targetForwardDirection) * (MathF.PI / 180f);
			if (num2 <= angularSpeed2 * num)
			{
				break;
			}
			angularSpeed2 += ((MathUtil.StoppingDistance(angularSpeed2, angularAcceleration) < num2) ? angularAcceleration : (0f - angularAcceleration)) * num;
			angularSpeed2 = Mathf.Clamp(angularSpeed2, 0f, maxAngularSpeed);
			forward = Vector3.RotateTowards(forward, targetForwardDirection, angularSpeed2 * num, 0f);
			transform.forward = forward;
			yield return null;
		}
		transform.forward = targetForwardDirection;
	}

	public static void Billboard(this Transform transform, Transform billboardTo)
	{
		transform.LookAt(transform.position + billboardTo.transform.rotation * Vector3.forward, billboardTo.transform.rotation * Vector3.up);
	}

	public static Quaternion GetBillboardRotation(this Transform transform, Transform billboardTo)
	{
		Quaternion rotation = transform.rotation;
		transform.Billboard(billboardTo);
		Quaternion rotation2 = transform.rotation;
		transform.rotation = rotation;
		return rotation2;
	}

	public static Canvas GetCanvas(this RectTransform rect, bool rootCanvas = true)
	{
		if (!rootCanvas)
		{
			return rect.GetComponentInParent<Canvas>();
		}
		return rect.gameObject.GetRootComponent<Canvas>();
	}

	public static void SetAnchors(this RectTransform rect, Vector2 anchorPosition)
	{
		rect.anchorMin = anchorPosition;
		rect.anchorMax = anchorPosition;
	}

	public static Rect GetWorldRect(this RectTransform rect, bool recursive = false)
	{
		if (recursive)
		{
			return rect.GetWorldRectRecursive();
		}
		using PoolStructArrayHandle<Vector3> poolStructArrayHandle = Pools.UseArray<Vector3>(4);
		rect.GetWorldCorners(poolStructArrayHandle);
		return new Rect(poolStructArrayHandle[0], poolStructArrayHandle[2] - poolStructArrayHandle[0]);
	}

	public static Rect3D GetWorldRect3D(this RectTransform rect)
	{
		return new Rect3D(rect);
	}

	public static Rect GetWorldRectRecursive(this RectTransform rectTransform)
	{
		Rect rect = rectTransform.GetWorldRect();
		foreach (Graphic item in rectTransform.gameObject.GetComponentsInChildrenPooled<Graphic>())
		{
			rect = rect.Encapsulate(item.rectTransform.GetWorldRect());
		}
		return rect;
	}

	public static Rect3D GetWorldRect3DRecursive(this RectTransform rectTransform)
	{
		Rect3D result = rectTransform.GetWorldRect3D();
		foreach (Graphic item in rectTransform.gameObject.GetComponentsInChildrenPooled<Graphic>())
		{
			result = result.Encapsulate(item.rectTransform.GetWorldRect3D());
		}
		return result;
	}

	public static void SetWorldCorners(this RectTransform rect, Rect3D worldRect)
	{
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.SetAnchors(rect.pivot);
		rect.SetWorldScale(Vector3.one);
		rect.rotation = worldRect.uiRotation;
		rect.offsetMax = worldRect.size * 0.5f;
		rect.offsetMin = -rect.offsetMax;
		rect.position = worldRect.center;
	}

	public static void SetWorldCornersPreserveScale(this RectTransform rect, Rect3D worldRect)
	{
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.SetAnchors(rect.pivot);
		rect.rotation = worldRect.uiRotation;
		rect.offsetMax = (worldRect.size * 0.5f).Multiply(rect.GetWorldScale().Project(AxisType.Z).Inverse()
			.InsureNonZero());
		rect.offsetMin = -rect.offsetMax;
		rect.position = worldRect.center;
	}

	public static Vector3 GetOutOfCameraBoundsWorldOffsetCorrection(this RectTransform rect, Camera camera)
	{
		Rect3D rect3D = rect.GetWorldRect3D().WorldToViewportRect(camera);
		return camera.ViewportToWorldPoint(rect3D.FitIntoRange(new Vector3(0f, 0f, camera.nearClipPlane), new Vector3(1f, 1f, camera.farClipPlane)).center) - camera.ViewportToWorldPoint(rect3D.center);
	}

	public static void PadLocal(this RectTransform rect, Vector2 padding)
	{
		rect.offsetMax += padding;
		rect.offsetMin -= padding;
	}

	public static void PadLocal(this RectTransform rect, Padding padding)
	{
		rect.offsetMax += new Vector2(padding.right, padding.top);
		rect.offsetMin -= new Vector2(padding.left, padding.bottom);
	}

	public static bool IsMouseInRect(this RectTransform rect, Camera camera = null)
	{
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, Input.mousePosition, camera ?? Camera.main, out var worldPoint))
		{
			return rect.GetWorldRect3D().ContainsProjection(worldPoint);
		}
		return false;
	}

	public static Vector2 GetPreferredSize(this RectTransform rectTransform)
	{
		return new Vector2(LayoutUtility.GetPreferredWidth(rectTransform), LayoutUtility.GetPreferredHeight(rectTransform));
	}

	public static bool LayoutIsReady(this RectTransform rectTransform)
	{
		return rectTransform.GetComponent<ILayoutReady>()?.layoutIsReady ?? true;
	}

	public static Transform CopyFrom(this Transform transform, Transform copyFrom, bool copyPosition = true, bool copyRotation = true, bool copyScale = true)
	{
		if (!copyFrom)
		{
			return transform;
		}
		if (copyPosition)
		{
			transform.position = copyFrom.position;
		}
		if (copyRotation)
		{
			transform.rotation = copyFrom.rotation;
		}
		if (copyScale)
		{
			transform.localScale = copyFrom.localScale;
		}
		return transform;
	}

	public static void CopyFromLocal(this Transform transform, Transform copyFrom, bool copyPosition = true, bool copyRotation = true, bool copyScale = true)
	{
		if ((bool)transform && (bool)copyFrom)
		{
			if (copyPosition)
			{
				transform.localPosition = copyFrom.localPosition;
			}
			if (copyRotation)
			{
				transform.localRotation = copyFrom.localRotation;
			}
			if (copyScale)
			{
				transform.localScale = copyFrom.localScale;
			}
		}
	}

	public static void SetLocalToIdentity(this Transform transform)
	{
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
	}

	public static void SetWorldToIdentity(this Transform transform)
	{
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.SetWorldScale(Vector3.one);
	}

	public static Transform SetParentAndReturn(this Transform transform, Transform parent, bool worldPositionStays)
	{
		transform.SetParent(parent, worldPositionStays);
		return transform;
	}

	public static Transform SetAsFirstSiblingAndReturn(this Transform transform)
	{
		transform.SetAsFirstSibling();
		return transform;
	}

	public static Plane GetPlane(this Transform transform, PlaneAxes axes)
	{
		return new Plane(transform.GetAxis(axes.NormalAxis()), transform.position);
	}

	public static Plane GetPlane(this Camera camera, float distance)
	{
		return new Plane(camera.transform.forward, camera.transform.position + camera.transform.forward * distance);
	}

	public static Vector3 GetPositionOnPlaneFromADifferentDistance(this Camera camera, Vector3 originalPoint, Vector3 offset)
	{
		return camera.GetWorldFrustumRectAtDepth(Vector3.Dot(originalPoint + offset - camera.transform.position, camera.transform.forward)).Lerp(camera.GetWorldFrustumRectAtDepth(Vector3.Dot(originalPoint - camera.transform.position, camera.transform.forward)).GetLerpAmount(originalPoint));
	}

	public static AxisType GetAxisClosestToDirection(this Transform transform, Vector3 direction)
	{
		AxisType result = AxisType.X;
		float num = float.MinValue;
		AxisType[] values = EnumUtil<AxisType>.Values;
		foreach (AxisType axisType in values)
		{
			float num2 = Vector3.Dot(direction, transform.GetAxis(axisType));
			if (!(num2 <= num))
			{
				num = num2;
				result = axisType;
			}
		}
		return result;
	}

	public static GameObject GetRoot(this GameObject obj, Func<GameObject, bool> stopAt = null)
	{
		if (stopAt != null && stopAt(obj))
		{
			return obj;
		}
		if (obj.transform.parent != null)
		{
			return obj.transform.parent.gameObject.GetRoot(stopAt);
		}
		return obj;
	}

	public static T CacheComponent<T>(this GameObject go, ref T cachedComponent) where T : Component
	{
		if (!(UnityEngine.Object)cachedComponent)
		{
			return cachedComponent = go.GetComponent<T>();
		}
		return cachedComponent;
	}

	public static T CacheComponentSafe<T>(this GameObject go, ref T cachedComponent) where T : Component
	{
		if (!(UnityEngine.Object)cachedComponent)
		{
			return cachedComponent = go.GetOrAddComponent<T>();
		}
		return cachedComponent;
	}

	public static T CacheComponentInChildren<T>(this GameObject go, ref T cachedComponent, bool includeInactive = false) where T : Component
	{
		if (!(UnityEngine.Object)cachedComponent)
		{
			return cachedComponent = go.GetComponentInChildren<T>(includeInactive);
		}
		return cachedComponent;
	}

	public static T CacheComponentInParent<T>(this GameObject go, ref T cachedComponent, bool includeInactive = false) where T : Component
	{
		if (!(UnityEngine.Object)cachedComponent)
		{
			return cachedComponent = go.GetComponentInParent<T>(includeInactive);
		}
		return cachedComponent;
	}

	public static PoolKeepItemListHandle<T> GetComponentsInChildrenPooled<T>(this GameObject go, bool includeInactive = false)
	{
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		go.GetComponentsInChildren(includeInactive, poolKeepItemListHandle.value);
		return poolKeepItemListHandle;
	}

	public static PoolKeepItemListHandle<T> GetComponentsInImmediateChildrenPooled<T>(this GameObject go, bool includeInactive = false) where T : Component
	{
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		foreach (Transform item in go.transform.ChildrenSafe())
		{
			foreach (T item2 in item.gameObject.GetComponentsPooled<T>(includeInactive))
			{
				poolKeepItemListHandle.Add(item2);
			}
		}
		return poolKeepItemListHandle;
	}

	public static PoolKeepItemListHandle<T> GetComponentsPooled<T>(this GameObject go, bool includeInactive) where T : Component
	{
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		go.GetComponents(poolKeepItemListHandle.value);
		if (!includeInactive)
		{
			for (int num = poolKeepItemListHandle.Count - 1; num >= 0; num--)
			{
				if (poolKeepItemListHandle[num] is Behaviour && !(poolKeepItemListHandle[num] as Behaviour).isActiveAndEnabled)
				{
					poolKeepItemListHandle.RemoveAt(num);
				}
			}
		}
		return poolKeepItemListHandle;
	}

	public static PoolKeepItemListHandle<T> GetInterfacesPooled<T>(this GameObject go, bool includeInactive = false) where T : class
	{
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		foreach (Component item in go.GetComponentsPooled<Component>(includeInactive))
		{
			if (item is T)
			{
				poolKeepItemListHandle.Add(item as T);
			}
		}
		return poolKeepItemListHandle;
	}

	public static PoolKeepItemListHandle<T> GetComponentsInParentsPooled<T>(this GameObject go, bool includeInactive = false, bool includeSelf = true)
	{
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		if (includeSelf)
		{
			go.GetComponentsInParent<T>(includeInactive, poolKeepItemListHandle);
		}
		else if ((bool)go.transform.parent)
		{
			go.transform.parent.GetComponentsInParent<T>(includeInactive, poolKeepItemListHandle);
		}
		return poolKeepItemListHandle;
	}

	public static GameObject CreateParentGameObject(this GameObject go, string parentName = null)
	{
		GameObject gameObject = new GameObject(parentName ?? (go.name + " Parent"));
		gameObject.transform.CopyFrom(go.transform);
		Transform parent = go.transform.parent;
		int siblingIndex = go.transform.GetSiblingIndex();
		go.transform.SetParent(gameObject.transform, worldPositionStays: true);
		gameObject.transform.SetParent(parent, worldPositionStays: true);
		gameObject.transform.SetSiblingIndex(siblingIndex);
		return gameObject;
	}

	public static T GetComponentInParent<T>(this Component component, bool includeInactive) where T : Component
	{
		return component.gameObject.GetComponentInParent<T>(includeInactive);
	}

	public static T GetRootComponent<T>(this GameObject go) where T : Component
	{
		Transform transform = go.transform;
		T val = null;
		T componentInParent;
		do
		{
			val = (((UnityEngine.Object)(componentInParent = transform.GetComponentInParent<T>())) ? componentInParent : val);
		}
		while ((bool)(transform = (((UnityEngine.Object)componentInParent) ? componentInParent.transform : transform).parent));
		return val;
	}

	public static int GetComponentsInParentCount<T>(this GameObject go) where T : Component
	{
		Transform transform = go.transform;
		int num = 0;
		T componentInParent;
		do
		{
			componentInParent = transform.GetComponentInParent<T>();
		}
		while ((bool)(UnityEngine.Object)componentInParent && ++num > 0 && (bool)(transform = componentInParent.transform.parent));
		return num;
	}

	public static GameObject SetActiveAndReturn(this GameObject go, bool active)
	{
		go.SetActive(active);
		return go;
	}

	public static string NamePath(this GameObject go)
	{
		return new Stack<GameObject>(go.Parents(includeSelf: true)).ToStringSmart((GameObject g) => g.name, "/");
	}

	public static GameObject SetParentAndReturn(this GameObject go, Transform parent, bool worldPositionStays = true)
	{
		go.transform.SetParent(parent, worldPositionStays);
		return go;
	}

	public static IEnumerable<GameObject> Children(this GameObject go)
	{
		return from child in go.transform.Children()
			select child.gameObject;
	}

	public static IEnumerable<GameObject> Siblings(this GameObject go)
	{
		return from child in go.transform.Siblings()
			select child.gameObject;
	}

	public static IEnumerable<GameObject> Parents(this GameObject go, bool includeSelf = false)
	{
		return from child in go.transform.Parents(includeSelf)
			select child.gameObject;
	}

	public static IEnumerable<GameObject> ChildrenRecursive(this GameObject go)
	{
		return from child in go.transform.ChildrenRecursive()
			select child.gameObject;
	}

	public static T GetComponentInChildrenOnly<T>(this GameObject obj) where T : Component
	{
		return obj.ChildrenRecursive().SelectValid((GameObject child) => child.GetComponent<T>()).FirstOrDefault();
	}

	public static GameObject FindChild(this GameObject obj, Func<GameObject, bool> validChild, bool includeInactive = true, bool countsAsChild = false)
	{
		if (countsAsChild && validChild(obj))
		{
			return obj;
		}
		countsAsChild = true;
		foreach (Transform item in obj.transform)
		{
			GameObject gameObject = item.gameObject;
			if (includeInactive || gameObject.activeInHierarchy)
			{
				GameObject gameObject2 = gameObject.FindChild(validChild, includeInactive, countsAsChild);
				if (gameObject2 != null)
				{
					return gameObject2;
				}
			}
		}
		return null;
	}

	public static List<GameObject> GetChildren(this GameObject obj, bool includeInactive = false, bool countAsChild = false, List<GameObject> children = null)
	{
		children = children ?? new List<GameObject>();
		foreach (Transform item in obj.transform)
		{
			GameObject gameObject = item.gameObject;
			if (includeInactive || gameObject.activeInHierarchy)
			{
				if (countAsChild)
				{
					children.Add(gameObject);
				}
				gameObject.GetChildren(includeInactive, countAsChild: true, children);
			}
		}
		return children;
	}

	public static void SetActiveChildren(this GameObject obj, bool active)
	{
		foreach (Transform item in obj.transform.ChildrenSafe())
		{
			item.gameObject.SetActive(active);
		}
	}

	public static PoolKeepItemListHandle<C> GetComponentsInChildrenNonAlloc<C>(this GameObject go, bool includeSelf = true, bool includeInactive = true, Func<Transform, bool> stopAt = null) where C : Component
	{
		PoolKeepItemListHandle<C> poolKeepItemListHandle = Pools.UseKeepItemList<C>();
		foreach (Transform item in go.transform.ChildrenRecursiveNonAlloc(includeSelf, stopAt))
		{
			C component = item.GetComponent<C>();
			if ((bool)(UnityEngine.Object)component && (includeInactive || item.gameObject.activeInHierarchy))
			{
				poolKeepItemListHandle.Add(component);
			}
		}
		return poolKeepItemListHandle;
	}

	[Conditional("UNITY_EDITOR")]
	public static void NameInEditor(this GameObject obj, string name)
	{
		obj.name = name;
	}

	[Conditional("UNITY_EDITOR")]
	public static void NameReplaceInEditor(this GameObject obj, string oldString = "(Clone)", string newString = "")
	{
		obj.name = obj.name.Replace(oldString, newString);
	}

	public static bool HasMissingScript(this GameObject obj)
	{
		Component[] components = obj.GetComponents<Component>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] == null)
			{
				return true;
			}
		}
		return false;
	}

	public static T SetDirtyForEditor<T>(this T obj, bool useContentSystem = true) where T : UnityEngine.Object
	{
		return obj;
	}

	public static StringTableEntry SetDirtyForEditor(this StringTableEntry entry, bool useContentSystem = true)
	{
		entry.Table.SetDirtyForEditor(useContentSystem);
		return entry;
	}

	public static bool SetIsSmart(this StringTableEntry entry, bool isSmart)
	{
		if (entry.IsSmart == isSmart)
		{
			return false;
		}
		entry.IsSmart = isSmart;
		entry.SetDirtyForEditor();
		return true;
	}

	public static bool UpdateIsSmart(this StringTableEntry entry)
	{
		bool flag = entry.Value.Contains("{");
		entry.SetIsSmart(flag);
		return flag;
	}

	public static bool SetValue(this StringTableEntry entry, string value)
	{
		if (entry.Value == value)
		{
			return false;
		}
		entry.Value = value;
		entry.SetDirtyForEditor();
		return true;
	}

	public static StringTableEntry GetTableEntry(this LocalizedString localizedString)
	{
		return new LocalizedStringData.TableEntryId(localizedString).tableEntry;
	}

	public static string Localize(this LocalizedString localizedString)
	{
		try
		{
			return localizedString.GetLocalizedString().ParseNullSource();
		}
		catch
		{
			return localizedString.GetTableEntry()?.Value;
		}
	}

	public static string Localize(this LocalizedString localizedString, Locale locale)
	{
		if (locale == null)
		{
			return localizedString.Localize();
		}
		Locale localeOverride = localizedString.LocaleOverride;
		try
		{
			localizedString.LocaleOverride = locale;
			return localizedString.GetLocalizedString().ParseNullSource();
		}
		catch
		{
			return localizedString.GetTableEntry()?.Value;
		}
		finally
		{
			localizedString.LocaleOverride = localeOverride;
		}
	}

	public static string Value(this LocalizedString localizedString, Locale locale)
	{
		return localizedString.TableReference.GetStringTableEntry(localizedString.TableEntryReference, locale)?.Value ?? "";
	}

	public static LocalizedString SetArguments(this LocalizedString localizedString, params object[] args)
	{
		localizedString.Arguments = args;
		localizedString.RefreshString();
		return localizedString;
	}

	public static LocalizedString SetArgumentsCloned(this LocalizedString localizedString, params object[] args)
	{
		return JsonClone(localizedString).SetArguments(args);
	}

	public static LocalizedString SetVariables(this LocalizedString localizedString, params (LocalizedVariableName, object)[] localVariables)
	{
		localizedString.Clear();
		for (int i = 0; i < localVariables.Length; i++)
		{
			(LocalizedVariableName, object) tuple = localVariables[i];
			IVariable variable = LocalizedStringData.CastToIVariable(tuple.Item2);
			if (variable != null)
			{
				localizedString[EnumUtil.Name(tuple.Item1)] = variable;
			}
		}
		localizedString.RefreshString();
		return localizedString;
	}

	public static LocalizedString SetVariables(this LocalizedString localizedString, params (string, object)[] localVariables)
	{
		localizedString.Clear();
		for (int i = 0; i < localVariables.Length; i++)
		{
			(string, object) tuple = localVariables[i];
			IVariable variable = LocalizedStringData.CastToIVariable(tuple.Item2);
			if (variable != null)
			{
				localizedString[tuple.Item1] = variable;
			}
		}
		localizedString.RefreshString();
		return localizedString;
	}

	private static string _TryGetLocalizedString(this ILocalizedString localizedString, Locale locale)
	{
		Locale selectedLocale = LocalizationSettings.SelectedLocale;
		try
		{
			LocalizationSettings.SelectedLocale = locale;
			return localizedString.localizedString.GetLocalizedString();
		}
		catch
		{
			return localizedString.localizedString.GetTableEntry()?.Value;
		}
		finally
		{
			LocalizationSettings.SelectedLocale = selectedLocale;
		}
	}

	public static string GetLocalizedStringError(this LocalizedString localizedString)
	{
		try
		{
			localizedString.GetLocalizedString();
			return null;
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	public static string GetPath(this LocalizedString localizedString)
	{
		StringTableEntry tableEntry = localizedString.GetTableEntry();
		if (tableEntry == null)
		{
			return "NULL";
		}
		return tableEntry.Table.TableCollectionName + "/" + tableEntry.Key;
	}

	public static PoolKeepItemDictionaryHandle<Locale, string> GetAllLocalizedStrings(this ILocalizedString localizedString)
	{
		PoolKeepItemDictionaryHandle<Locale, string> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<Locale, string>();
		foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
		{
			poolKeepItemDictionaryHandle[locale] = localizedString._TryGetLocalizedString(locale);
		}
		return poolKeepItemDictionaryHandle;
	}

	public static string AutoLocalize(this StringTable table, string s, bool returnNullIfMissing = false)
	{
		string text = (returnNullIfMissing ? null : s);
		string text2;
		if ((bool)table)
		{
			StringTableEntry stringTableEntry = table[s.ToLower()];
			if (stringTableEntry != null)
			{
				text2 = stringTableEntry.GetLocalizedString();
				goto IL_0029;
			}
		}
		text2 = text;
		goto IL_0029;
		IL_0029:
		string text3 = text2;
		if (!text3.HasVisibleCharacter())
		{
			return text;
		}
		return text3;
	}

	public static string TryAutoLocalize(this StringTable table, string s)
	{
		if ((bool)table)
		{
			StringTableEntry stringTableEntry = table[s.ToLower()];
			if (stringTableEntry != null)
			{
				return stringTableEntry.GetLocalizedString();
			}
		}
		return null;
	}

	[Conditional("UNITY_EDITOR")]
	public static void Save(this UnityEngine.Object obj)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public static void SaveAssetInEditMode(this UnityEngine.Object obj)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public static void SaveIfDirty(this UnityEngine.Object obj)
	{
	}

	public static long LastModified(this UnityEngine.Object obj)
	{
		return 0L;
	}

	[Conditional("UNITY_EDITOR")]
	public static void SetLastModified(this UnityEngine.Object obj, long fileTimeUtc)
	{
	}

	public static string GetHash128(this UnityEngine.Object obj)
	{
		return Hash128.Compute(JsonUtility.ToJson(obj)).ToString();
	}

	public static T JsonClone<T>(T obj)
	{
		return JsonUtility.FromJson<T>(JsonUtility.ToJson(obj));
	}

	public static T GetOrAddMetadata<T>(this LocalizationTable table) where T : IMetadata, new()
	{
		T metadata = table.GetMetadata<T>();
		if (metadata != null)
		{
			return metadata;
		}
		T val = new T();
		table.AddMetadata(val);
		table.SetDirtyForEditor(useContentSystem: false);
		return val;
	}

	public static T GetOrAddMetadata<T>(this StringTableEntry entry) where T : IMetadata
	{
		T metadata = entry.GetMetadata<T>();
		if (metadata != null)
		{
			return metadata;
		}
		T val = ConstructorCache<T>.Constructor();
		entry.AddMetadata(val);
		entry.SetDirtyForEditor(useContentSystem: false);
		return val;
	}

	public static T GetOrAddMetadata<T>(this Locale locale) where T : IMetadata, new()
	{
		T metadata = locale.Metadata.GetMetadata<T>();
		if (metadata != null)
		{
			return metadata;
		}
		T val = new T();
		locale.Metadata.AddMetadata(val);
		return val;
	}

	public static T GetOrAddMetadata<T>(this MetadataCollection metadataCollection) where T : IMetadata, new()
	{
		T metadata = metadataCollection.GetMetadata<T>();
		if (metadata != null)
		{
			return metadata;
		}
		T val = new T();
		metadataCollection.AddMetadata(val);
		return val;
	}

	public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
	{
		T val = obj.GetComponent<T>();
		if (val.IsDestroyed())
		{
			val = obj.AddComponent<T>();
		}
		return val;
	}

	public static T GetOrAddComponent<T>(this Component component) where T : Component
	{
		return component.gameObject.GetOrAddComponent<T>();
	}

	public static Component GetOrAddComponent(this GameObject obj, Type type)
	{
		Component component = obj.GetComponent(type);
		if (component.IsDestroyed())
		{
			component = obj.AddComponent(type);
		}
		return component;
	}

	public static T CacheComponent<T>(this Component component, ref T cachedComponent) where T : Component
	{
		return component.gameObject.CacheComponent(ref cachedComponent);
	}

	public static T CacheComponentSafe<T>(this Component component, ref T cachedComponent) where T : Component
	{
		return component.gameObject.CacheComponentSafe(ref cachedComponent);
	}

	public static T CacheComponentInChildren<T>(this Component component, ref T cachedComponent, bool includeInactive = false) where T : Component
	{
		return component.gameObject.CacheComponentInChildren(ref cachedComponent, includeInactive);
	}

	public static T CacheComponentInParent<T>(this Component component, ref T cachedComponent, bool includeInactive = false) where T : Component
	{
		return component.gameObject.CacheComponentInParent(ref cachedComponent, includeInactive);
	}

	public static T CacheScriptObject<T>(this Component component, ref T scriptObject) where T : ScriptableObject
	{
		return scriptObject ?? (scriptObject = ScriptableObject.CreateInstance<T>());
	}

	public static bool HasComponent<T>(this GameObject obj) where T : Component
	{
		return (UnityEngine.Object)obj.GetComponent<T>() != (UnityEngine.Object)null;
	}

	public static void DoOnCompleteAction(this Behaviour comp, OnCompleteAction onCompleteAction)
	{
		switch (onCompleteAction)
		{
		case OnCompleteAction.DisableSelf:
			comp.enabled = false;
			break;
		case OnCompleteAction.DeactivateGameObject:
			comp.gameObject.SetActive(value: false);
			break;
		case OnCompleteAction.DestroyGameObject:
			comp.gameObject.Destroy();
			break;
		case OnCompleteAction.None:
			break;
		}
	}

	public static void ForceLayoutUpdate(this ILayoutElement layoutElement)
	{
		layoutElement.CalculateLayoutInputHorizontal();
		layoutElement.CalculateLayoutInputVertical();
		if (layoutElement is ILayoutSelfController layoutSelfController)
		{
			layoutSelfController.SetLayoutHorizontal();
			layoutSelfController.SetLayoutVertical();
		}
	}

	public static void ForceLayoutUpdateControl(this ILayoutSelfController controller)
	{
		if (controller is ILayoutElement layoutElement)
		{
			layoutElement.CalculateLayoutInputHorizontal();
			layoutElement.CalculateLayoutInputVertical();
		}
		controller.SetLayoutHorizontal();
		controller.SetLayoutVertical();
	}

	public static Vector2 GetPreferredSize(this LayoutElement layoutElement)
	{
		return new Vector2(layoutElement.preferredWidth, layoutElement.preferredHeight);
	}

	public static void SetPreferredSize(this LayoutElement layoutElement, Vector2 size)
	{
		layoutElement.preferredWidth = size.x;
		layoutElement.preferredHeight = size.y;
	}

	public static void SetActiveSafe(this Component component, bool active)
	{
		if ((bool)component)
		{
			component.gameObject.SetActive(active);
		}
	}

	public static T InsureValid<T>(this T scriptableObject, ref T value) where T : ScriptableObject
	{
		if (!(UnityEngine.Object)value)
		{
			return value = ScriptableObject.CreateInstance<T>();
		}
		return value;
	}

	public static bool IsAsset(this UnityEngine.Object obj)
	{
		return obj.GetInstanceID() >= 0;
	}

	public static bool IsActiveAndEnabled(this Behaviour behaviour)
	{
		if ((bool)behaviour)
		{
			return behaviour.isActiveAndEnabled;
		}
		return false;
	}

	public static bool InvokeEventIfActive<T>(this Behaviour behaviour, UnityEvent<T> unityEvent, T value)
	{
		if (!behaviour.IsActiveAndEnabled())
		{
			return false;
		}
		unityEvent.Invoke(value);
		return true;
	}

	public static bool InvokeIfAlive<T>(this Component component, UnityEvent<T> unityEvent, T value)
	{
		if (!component)
		{
			return false;
		}
		unityEvent?.Invoke(value);
		return true;
	}

	public static GameObject SetName(this GameObject go, string name)
	{
		go.name = name;
		return go;
	}

	public static void InvokeNextFrame(this MonoBehaviour m, Action action)
	{
		m.StartCoroutine(Job.InvokeNextFrame(action));
	}

	public static void RefreshEnabled(this Behaviour behaviour)
	{
		behaviour.enabled = false;
		behaviour.enabled = true;
	}

	public static GameObject SetUILabel(this GameObject go, string label, bool includeInactive = true)
	{
		if (!go)
		{
			return go;
		}
		TextMeshProUGUI componentInChildren = go.GetComponentInChildren<TextMeshProUGUI>(includeInactive);
		if ((bool)componentInChildren)
		{
			componentInChildren.text = label;
			return go;
		}
		Text componentInChildren2 = go.GetComponentInChildren<Text>(includeInactive);
		if ((bool)componentInChildren2)
		{
			componentInChildren2.text = label;
			return go;
		}
		return go;
	}

	public static void SetUITooltip(this GameObject go, string tooltip, bool includeInactive = true)
	{
		if (tooltip.HasVisibleCharacter() && (bool)go)
		{
			TextMeshProUGUI componentInChildren = go.GetComponentInChildren<TextMeshProUGUI>(includeInactive);
			if ((bool)componentInChildren)
			{
				TooltipCreator.CreateTextTooltip(componentInChildren.transform, tooltip, beginShowTimer: false, 0.5f, backgroundEnabled: true, TextAlignmentOptions.Left, 0f, 0f, TooltipDirection.Vertical, TooltipOrthogonalDirection.Min, 1f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator: true, ignoreEventsWhileDragging: false, ignorePointerEvents: false, blockRayCasts: false, 30, useProxyTooltipCreator: false, ignoreClear: false, useOppositeDirection: false, clearOnDisable: false, null, 1);
				componentInChildren.raycastTarget = true;
			}
		}
	}

	public static string GetUILabel(this GameObject go, bool includeInactive = true)
	{
		TextMeshProUGUI componentInChildren = go.GetComponentInChildren<TextMeshProUGUI>(includeInactive);
		if ((bool)componentInChildren)
		{
			return componentInChildren.text;
		}
		Text componentInChildren2 = go.GetComponentInChildren<Text>(includeInactive);
		if ((bool)componentInChildren2)
		{
			return componentInChildren2.text;
		}
		return "";
	}

	public static void SetMaterialAndVerticesDirty(this Graphic graphic)
	{
		graphic.SetVerticesDirty();
		graphic.SetMaterialDirty();
	}

	private static void AddQuad(this VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
	{
		int currentVertCount = vertexHelper.currentVertCount;
		vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uvMin.x, uvMin.y));
		vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uvMin.x, uvMax.y));
		vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uvMax.x, uvMax.y));
		vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uvMax.x, uvMin.y));
		vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
		vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
	}

	private static Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
	{
		for (int i = 0; i <= 1; i++)
		{
			float num = border[i] + border[i + 2];
			if (rect.size[i] < num && num != 0f)
			{
				float num2 = rect.size[i] / num;
				border[i] *= num2;
				border[i + 2] *= num2;
			}
		}
		return border;
	}

	public static Vector4 GetDrawingDimensions(this Graphic graphic, Sprite sprite, Rect rect, bool shouldPreserveAspect)
	{
		Vector4 vector = ((sprite == null) ? Vector4.zero : DataUtility.GetPadding(sprite));
		Vector2 vector2 = ((sprite == null) ? Vector2.zero : new Vector2(sprite.rect.width, sprite.rect.height));
		RectTransform rectTransform = graphic.rectTransform;
		int num = Mathf.RoundToInt(vector2.x);
		int num2 = Mathf.RoundToInt(vector2.y);
		Vector4 vector3 = new Vector4(vector.x / (float)num, vector.y / (float)num2, ((float)num - vector.z) / (float)num, ((float)num2 - vector.w) / (float)num2);
		if (shouldPreserveAspect && vector2.sqrMagnitude > 0f)
		{
			float num3 = vector2.x / vector2.y;
			float num4 = rect.width / rect.height;
			if (num3 > num4)
			{
				float height = rect.height;
				rect.height = rect.width * (1f / num3);
				rect.y += (height - rect.height) * rectTransform.pivot.y;
			}
			else
			{
				float width = rect.width;
				rect.width = rect.height * num3;
				rect.x += (width - rect.width) * rectTransform.pivot.x;
			}
		}
		return new Vector4(rect.x + rect.width * vector3.x, rect.y + rect.height * vector3.y, rect.x + rect.width * vector3.z, rect.y + rect.height * vector3.w);
	}

	public static void GenerateSimpleSprite(this VertexHelper vh, Graphic graphic, Sprite sprite, Rect rect, bool shouldPreserveAspect)
	{
		Vector4 drawingDimensions = graphic.GetDrawingDimensions(sprite, rect, shouldPreserveAspect);
		Vector4 vector = ((sprite != null) ? DataUtility.GetOuterUV(sprite) : Vector4.zero);
		Color color = graphic.color;
		vh.Clear();
		vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.y), color, new Vector2(vector.x, vector.y));
		vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.w), color, new Vector2(vector.x, vector.w));
		vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.w), color, new Vector2(vector.z, vector.w));
		vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.y), color, new Vector2(vector.z, vector.y));
		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
	}

	public static void GenerateSlicedSprite(this VertexHelper vh, Graphic graphic, Sprite sprite, Rect rect, bool fillCenter, bool preserveAspect)
	{
		if (sprite == null || sprite.border == Vector4.zero)
		{
			vh.GenerateSimpleSprite(graphic, sprite, rect, preserveAspect);
			return;
		}
		Vector4 outerUV = DataUtility.GetOuterUV(sprite);
		Vector4 innerUV = DataUtility.GetInnerUV(sprite);
		Vector4 padding = DataUtility.GetPadding(sprite);
		Vector4 border = sprite.border;
		float num = sprite.pixelsPerUnit / (graphic.canvas ? graphic.canvas.referencePixelsPerUnit : 100f);
		border = GetAdjustedBorders(border / num, rect);
		padding /= num;
		s_VertScratch[0] = new Vector2(padding.x, padding.y);
		s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);
		s_VertScratch[1].x = border.x;
		s_VertScratch[1].y = border.y;
		s_VertScratch[2].x = rect.width - border.z;
		s_VertScratch[2].y = rect.height - border.w;
		for (int i = 0; i < 4; i++)
		{
			s_VertScratch[i].x += rect.x;
			s_VertScratch[i].y += rect.y;
		}
		s_UVScratch[0] = new Vector2(outerUV.x, outerUV.y);
		s_UVScratch[1] = new Vector2(innerUV.x, innerUV.y);
		s_UVScratch[2] = new Vector2(innerUV.z, innerUV.w);
		s_UVScratch[3] = new Vector2(outerUV.z, outerUV.w);
		vh.Clear();
		Color color = graphic.color;
		for (int j = 0; j < 3; j++)
		{
			int num2 = j + 1;
			for (int k = 0; k < 3; k++)
			{
				if (fillCenter || j != 1 || k != 1)
				{
					int num3 = k + 1;
					vh.AddQuad(new Vector2(s_VertScratch[j].x, s_VertScratch[k].y), new Vector2(s_VertScratch[num2].x, s_VertScratch[num3].y), color, new Vector2(s_UVScratch[j].x, s_UVScratch[k].y), new Vector2(s_UVScratch[num2].x, s_UVScratch[num3].y));
				}
			}
		}
	}

	public static float ClickHeldTime(this PointerEventData eventData)
	{
		return Time.unscaledTime - eventData.clickTime;
	}

	public static bool IsClick(this PointerEventData eventData)
	{
		if (InputManager.I.IsClick(eventData.ClickHeldTime()))
		{
			return !eventData.dragging;
		}
		return false;
	}

	public static bool IsPointerOverGameObject(this PointerEventData eventData, GameObject gameObject)
	{
		RectTransform rectTransform = gameObject.transform as RectTransform;
		if (rectTransform == null)
		{
			return false;
		}
		return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, eventData.position, eventData.enterEventCamera);
	}

	public static void ExecutePointerEnterOnEndDrag(this PointerEventData eventData)
	{
		GameObject pointerDrag = eventData.pointerDrag;
		bool dragging = eventData.dragging;
		GameObject pointerEnter = eventData.pointerEnter;
		eventData.pointerDrag = null;
		eventData.dragging = false;
		eventData.pointerEnter = EventSystem.current.GetLastPointerEnter();
		if ((bool)eventData.pointerEnter)
		{
			ExecuteEvents.Execute(eventData.pointerEnter, eventData, ExecuteEvents.pointerEnterHandler);
		}
		eventData.pointerDrag = pointerDrag;
		eventData.dragging = dragging;
		eventData.pointerEnter = pointerEnter;
	}

	public static Vector3? GetWorldPositionOfPress(this PointerEventData eventData)
	{
		if (eventData == null || !eventData.pressEventCamera)
		{
			return null;
		}
		if (!eventData.pointerPressRaycast.gameObject)
		{
			return eventData.pressEventCamera.transform.position + eventData.pressEventCamera.transform.forward * (eventData.pressEventCamera.nearClipPlane + eventData.pressEventCamera.farClipPlane) * 0.5f;
		}
		if (!(eventData.pointerPressRaycast.gameObject.transform is RectTransform))
		{
			return eventData.pointerPressRaycast.worldPosition;
		}
		RectTransformUtility.ScreenPointToWorldPointInRectangle(eventData.pointerPressRaycast.gameObject.transform as RectTransform, eventData.pressPosition, eventData.pressEventCamera, out var worldPoint);
		return worldPoint;
	}

	public static Vector3 GetWorldPositionOnPlane(this PointerEventData eventData, Transform transform, PlaneAxes planeAxes = PlaneAxes.XY)
	{
		return transform.GetPlane(planeAxes).ClosestPointOnPlane(eventData.pressEventCamera.ScreenPointToRay(Input.mousePosition));
	}

	private static GameObject _BubbledEventHandler<T>(this PointerEventData eventData, GameObject currentEventHandler) where T : IEventSystemHandler
	{
		List<RaycastResult> cachedRaycastResults = eventData.GetCachedRaycastResults(rayCastAllOnUnpool: false);
		EventSystem.current.RaycastAll(eventData, cachedRaycastResults);
		for (int i = 0; i < cachedRaycastResults.Count; i++)
		{
			if (cachedRaycastResults[i].gameObject != currentEventHandler && cachedRaycastResults[i].gameObject.GetComponent<T>() != null)
			{
				return cachedRaycastResults[i].gameObject;
			}
		}
		return null;
	}

	private static GameObject _BubbledEventHandlerCached<T>(this PointerEventData eventData, GameObject currentEventHandler) where T : IEventSystemHandler
	{
		List<RaycastResult> cachedRaycastResults = eventData.GetCachedRaycastResults();
		if (cachedRaycastResults == null)
		{
			return eventData._BubbledEventHandler<T>(currentEventHandler);
		}
		int num = 1;
		for (int i = 0; i < cachedRaycastResults.Count - 1; i++)
		{
			if (cachedRaycastResults[i].gameObject == currentEventHandler)
			{
				num = i + 1;
				break;
			}
		}
		for (int j = num; j < cachedRaycastResults.Count; j++)
		{
			if (cachedRaycastResults[j].gameObject.GetComponent<T>() != null)
			{
				return cachedRaycastResults[j].gameObject;
			}
		}
		return null;
	}

	private static GameObject _GetBubbledEventHandler<T>(this PointerEventData eventData, GameObject currentEventHandler, bool useCachedRaycasts = true) where T : IEventSystemHandler
	{
		if (!useCachedRaycasts)
		{
			return eventData._BubbledEventHandler<T>(currentEventHandler);
		}
		return eventData._BubbledEventHandlerCached<T>(currentEventHandler);
	}

	public static GameObject BubbleEvent<T>(this PointerEventData eventData, GameObject currentEventHandler, ExecuteEvents.EventFunction<T> eventFunction, bool useCachedRaycasts = true) where T : IEventSystemHandler
	{
		return ExecuteEvents.ExecuteHierarchy(eventData._GetBubbledEventHandler<T>(currentEventHandler, useCachedRaycasts), eventData, eventFunction);
	}

	public static GameObject BubbleEvent<T>(this PointerEventData eventData, Component currentEventHandler, ExecuteEvents.EventFunction<T> eventFunction, bool useCachedRaycasts = true) where T : IEventSystemHandler
	{
		return eventData.BubbleEvent(currentEventHandler.gameObject, eventFunction, useCachedRaycasts);
	}

	public static void PostProcess(this PointerEventData eventData, Action<PointerEventData> postProcess)
	{
		EventSystem.current.PostProcessPointerEventData(postProcess, eventData.button);
	}

	public static void CancelDrag(this PointerEventData eventData)
	{
		if ((bool)eventData?.pointerDrag)
		{
			(EventSystem.current.currentInputModule as SmartInputModule)?.ForceEndDrag();
			eventData.pointerDrag = null;
		}
	}

	public static void Signal(this PointerEvent pointerEvent, GameObject pointerEnterSignal)
	{
		PointerEventData pointerData = EventSystem.current.GetPointerData();
		if (pointerData == null)
		{
			throw new InvalidOperationException("PointerEvent.Signal requires SmartInputModule in order to work.");
		}
		GameObject pointerEnter = pointerData.pointerEnter;
		pointerData.pointerEnter = pointerEnterSignal;
		pointerEvent.Invoke(pointerData);
		pointerData.pointerEnter = pointerEnter;
	}

	public static PointerEventData GetPointerData(this EventSystem eventSystem, int pointerId = -1)
	{
		SmartInputModule smartInputModule = eventSystem.currentInputModule as SmartInputModule;
		if (!(smartInputModule != null))
		{
			return null;
		}
		return smartInputModule.GetPointerData(pointerId);
	}

	public static bool IsPointerDragging(this EventSystem eventSystem)
	{
		SmartInputModule smartInputModule = eventSystem.currentInputModule as SmartInputModule;
		if (smartInputModule != null)
		{
			return smartInputModule.IsDragging(-1);
		}
		return false;
	}

	public static bool IsPointerDraggingCheckAllButtons(this EventSystem eventSystem)
	{
		if (eventSystem?.currentInputModule is SmartInputModule smartInputModule)
		{
			if (!smartInputModule.IsDragging(-1) && !smartInputModule.IsDragging(-2))
			{
				return smartInputModule.IsDragging(-3);
			}
			return true;
		}
		return false;
	}

	public static bool IsOverShowCanDragItem(this EventSystem eventSystem)
	{
		SmartInputModule smartInputModule = eventSystem.currentInputModule as SmartInputModule;
		if (smartInputModule == null)
		{
			return false;
		}
		GameObject potentialDragObject = smartInputModule.GetPotentialDragObject(-1);
		if ((bool)potentialDragObject && potentialDragObject.GetComponent<IShowCanDrag>().ShouldShow())
		{
			return potentialDragObject.GetComponentInParent<IIgnoreShowCanDrag>().ShouldShow();
		}
		return false;
	}

	public static bool IsShowPointerDragging(this EventSystem eventSystem)
	{
		SmartInputModule smartInputModule = eventSystem.currentInputModule as SmartInputModule;
		if (smartInputModule == null)
		{
			return false;
		}
		GameObject dragObject = smartInputModule.GetDragObject(-1);
		if ((bool)dragObject && dragObject.GetComponentInParent<IIgnoreShowCanDrag>().ShouldShow())
		{
			return ExecuteEvents.GetEventHandler<IInitializePotentialDragHandler>(dragObject);
		}
		return false;
	}

	public static GameObject GetLastPointerEnter(this EventSystem eventSystem)
	{
		SmartInputModule smartInputModule = eventSystem.currentInputModule as SmartInputModule;
		if (!(smartInputModule != null))
		{
			return null;
		}
		return smartInputModule.GetLastPointerEnter(-1);
	}

	public static void PostProcessPointerEventData(this EventSystem eventSystem, Action<PointerEventData> postProcess, PointerEventData.InputButton mouseButton)
	{
		SmartInputModule smartInputModule = eventSystem.currentInputModule as SmartInputModule;
		if ((bool)smartInputModule)
		{
			smartInputModule.AddPointerEventPostProcess(postProcess, mouseButton);
		}
	}

	private static List<RaycastResult> GetCachedRaycastResults(this PointerEventData eventData, bool rayCastAllOnUnpool = true)
	{
		SmartInputModule smartInputModule = EventSystem.current.currentInputModule as SmartInputModule;
		if (!(smartInputModule != null))
		{
			return null;
		}
		return smartInputModule.GetRaycastResults(eventData, rayCastAllOnUnpool);
	}

	public static bool InputFieldHasFocus(this EventSystem eventSystem)
	{
		if ((bool)eventSystem && (bool)eventSystem.currentSelectedGameObject && (bool)eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>())
		{
			return eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>().isFocused;
		}
		return false;
	}

	public static void SetEnabled(this EventSystem eventSystem, bool enabled)
	{
		if (!eventSystem || eventSystem.enabled == enabled)
		{
			return;
		}
		eventSystem.enabled = enabled;
		if (enabled)
		{
			EventSystem.current = eventSystem;
			return;
		}
		while ((bool)EventSystem.current)
		{
			EventSystem.current.enabled = false;
		}
	}

	public static void SetDeepPointerEventsEnabled(this EventSystem eventSystem, bool enabled)
	{
		if ((bool)eventSystem && eventSystem.currentInputModule is SmartInputModule)
		{
			((SmartInputModule)eventSystem.currentInputModule).enableDeepPointerEvents = enabled;
		}
	}

	public static void SetCollapserIndentRatio(this GameObject gameObject, float indentRatio)
	{
		CollapseFitter componentInChildren = gameObject.GetComponentInChildren<CollapseFitter>();
		if ((bool)componentInChildren)
		{
			IndentFitter component = componentInChildren.gameObject.GetComponent<IndentFitter>();
			if ((bool)component)
			{
				component.SetIndentRatio(indentRatio);
			}
		}
	}

	public static Image SetSprite(this Image image, Sprite sprite)
	{
		image.sprite = sprite;
		return image;
	}

	public static TextMeshProUGUI SetTextReturn(this TextMeshProUGUI textMesh, string text)
	{
		textMesh.text = text;
		return textMesh;
	}

	public static TextMeshProUGUI SetTextReturnLocalized(this TextMeshProUGUI textMesh, Func<string> text)
	{
		textMesh.text = text();
		textMesh.gameObject.GetOrAddComponent<StringLocalizer>().updateString = delegate
		{
			textMesh.text = text();
		};
		return textMesh;
	}

	public static void Disable(this CanvasGroup group, bool refreshGameObjectActive)
	{
		group.alpha = 1f;
		bool interactable = (group.blocksRaycasts = true);
		group.interactable = interactable;
		group.ignoreParentGroups = false;
		if (refreshGameObjectActive && group.gameObject.activeInHierarchy)
		{
			group.gameObject.SetActiveAndReturn(active: false).SetActive(value: true);
		}
	}

	public static PoolKeepItemListHandle<T> GetComponentsInScene<T>(this Scene scene, bool includeInactive = true) where T : Component
	{
		PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList<T>();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			foreach (T item in rootGameObjects[i].GetComponentsInChildrenPooled<T>(includeInactive))
			{
				poolKeepItemListHandle.Add(item);
			}
		}
		return poolKeepItemListHandle;
	}

	public static PoolKeepItemListHandle<T> GetComponentsInActiveScene<T>() where T : Component
	{
		return SceneManager.GetActiveScene().GetComponentsInScene<T>();
	}

	public static void ForceOpenSafe(this CollapseFitter collapseFitter)
	{
		if ((bool)collapseFitter)
		{
			collapseFitter.ForceOpen();
		}
	}

	public static void FocusAndMoveToEnd(this TMP_InputField inputField, bool selectAll = false)
	{
		inputField.onFocusSelectAll = selectAll;
		InputManager.I.eventSystem.SetSelectedGameObject(inputField.gameObject);
		inputField.MoveTextEnd(shift: false);
	}

	public static void FreezeAutoFontSize(this TextMeshProUGUI text)
	{
		if (text.enableAutoSizing)
		{
			float fontSize = text.fontSize;
			text.enableAutoSizing = false;
			text.fontSize = fontSize;
		}
	}

	public static void RecalculateAutoFontSize(this TextMeshProUGUI text)
	{
		if (!text.enableAutoSizing)
		{
			text.enableAutoSizing = true;
			text.ForceMeshUpdate();
			text.FreezeAutoFontSize();
		}
	}

	public static Vector4 ToVector4(this RectOffset rectOffset)
	{
		return new Vector4(rectOffset.left, rectOffset.right, rectOffset.bottom, rectOffset.top);
	}

	public static void SetPadding(this RectOffset rectOffset, Vector4 padding)
	{
		rectOffset.left = Mathf.RoundToInt(padding.x);
		rectOffset.right = Mathf.RoundToInt(padding.y);
		rectOffset.bottom = Mathf.RoundToInt(padding.z);
		rectOffset.top = Mathf.RoundToInt(padding.w);
	}

	public static List<int> GetIndices(this VertexHelper vh)
	{
		return GetVertexHelperIndices(vh);
	}

	public static void SetDollyTrack(this CinemachineVirtualCamera camera, CinemachinePathBase track)
	{
		camera.GetCinemachineComponent<CinemachineTrackedDolly>().m_Path = track;
	}

	public static void ScrollTo(this ScrollRect scrollRect, RectTransform scrollTo)
	{
		Rect3D rect3D = new Rect3D(scrollRect.content);
		Rect3D rect3D2 = new Rect3D(scrollRect.viewport);
		Rect3D rect3D3 = new Rect3D(scrollTo);
		float y = rect3D.GetLerpAmount(rect3D2.bottomLeft).y;
		float y2 = rect3D.GetLerpAmount(rect3D2.topLeft).y;
		float y3 = rect3D.GetLerpAmount(rect3D3.bottomLeft).y;
		float y4 = rect3D.GetLerpAmount(rect3D3.topLeft).y;
		float num = 1f / (1f - (y2 - y)).InsureNonZero();
		if (y4 > y2)
		{
			scrollRect.verticalNormalizedPosition += (y4 - y2) * num;
		}
		else if (y3 < y)
		{
			scrollRect.verticalNormalizedPosition += (y3 - y) * num;
		}
	}

	public static void ExitApplication()
	{
		Application.Quit();
	}

	public static void ExitApplicationPopup(string message = null, Transform parent = null, string title = "Save & Exit To Desktop", string confirmText = "Save & Exit To Desktop", string cancelText = "Cancel", Action onExit = null, Action onClose = null)
	{
		UIUtil.CreatePopup(title, UIUtil.CreateMessageBox(message ?? "Would you like to exit game to desktop?"), null, onClose: cancelText.IsNullOrEmpty() ? new Action(ExitApplication) : onClose, parent: parent, buttons: (!cancelText.IsNullOrEmpty()) ? new string[2] { confirmText, cancelText } : new string[1] { confirmText }, size: null, centerReferece: null, center: null, pivot: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (!(s != confirmText))
			{
				if (onExit != null)
				{
					onExit();
				}
				ExitApplication();
			}
		});
	}

	public static float GetDeltaTime(bool useScaledTime, bool smoothed = true)
	{
		if (!useScaledTime)
		{
			if (!smoothed)
			{
				return Time.unscaledDeltaTime;
			}
			return smoothUnscaledDeltaTime;
		}
		if (!smoothed)
		{
			return Time.deltaTime;
		}
		return Time.smoothDeltaTime;
	}

	public static float GetTime(bool useScaledTime)
	{
		if (!useScaledTime)
		{
			return Time.unscaledTime;
		}
		return Time.time;
	}

	[Conditional("UNITY_EDITOR")]
	public static void CreateFolder(string folderPath)
	{
	}
}
