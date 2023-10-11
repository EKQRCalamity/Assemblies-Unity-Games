using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerOver3D))]
public class PointerOver3DColliderScaler : MonoBehaviour
{
	public const float DEFAULT_SCALE = 1.75f;

	[Range(1f, 5f)]
	public float scale = 1.75f;

	public Vector3 perAxisScale = Vector3.one;

	[EnumFlags]
	public AxesFlags scaleAxes = (AxesFlags)(-1);

	public Collider[] onlyGenerateCollidersFor;

	private List<GameObject> _colliderGameObjects;

	private bool? _pointerIsOver;

	private PointerOver3D _pointerOver3D;

	private PointerOver3D pointerOver3D
	{
		get
		{
			if (!_pointerOver3D)
			{
				return _pointerOver3D = GetComponent<PointerOver3D>();
			}
			return _pointerOver3D;
		}
	}

	public bool pointerIsOver
	{
		get
		{
			return _pointerIsOver.GetValueOrDefault();
		}
		private set
		{
			if (SetPropertyUtility.SetStruct(ref _pointerIsOver, value))
			{
				_OnPointerIsOverChange();
			}
		}
	}

	private void _OnPointerIsOverChange()
	{
		if (base.enabled)
		{
			if (pointerIsOver)
			{
				_DoPointerOverLogic();
			}
			else
			{
				_DoPointerExitLogic();
			}
		}
	}

	private void _DoPointerOverLogic()
	{
		_InitColliderGameObjects();
		foreach (GameObject colliderGameObject in _colliderGameObjects)
		{
			colliderGameObject.SetActive(value: true);
		}
	}

	private void _DoPointerExitLogic()
	{
		if (_colliderGameObjects == null)
		{
			return;
		}
		foreach (GameObject colliderGameObject in _colliderGameObjects)
		{
			colliderGameObject.SetActive(value: false);
		}
	}

	private void _InitColliderGameObjects()
	{
		if (_colliderGameObjects != null)
		{
			return;
		}
		_colliderGameObjects = new List<GameObject>();
		Vector3 multiplier = scaleAxes.ToVector3(scale, 1f).Multiply(perAxisScale);
		Collider[] array = (onlyGenerateCollidersFor.IsNullOrEmpty() ? GetComponentsInChildren<Collider>() : onlyGenerateCollidersFor);
		foreach (Collider collider in array)
		{
			GameObject gameObject = new GameObject("PointerOver3DColliderScaler");
			gameObject.transform.SetParent(collider.transform, worldPositionStays: false);
			gameObject.transform.localScale = gameObject.transform.localScale.Multiply(multiplier);
			Collider collider2 = collider.CopyColliderTo(gameObject);
			if (collider2 is MeshCollider)
			{
				((MeshCollider)collider2).convex = true;
			}
			collider2.isTrigger = true;
			gameObject.transform.position += collider.bounds.center - collider2.bounds.center;
			_colliderGameObjects.Add(gameObject);
		}
	}

	private void _RegisterEvents()
	{
		pointerOver3D.OnEnter.AddListener(_OnPointerEnter);
		pointerOver3D.OnExit.AddListener(_OnPointerExit);
	}

	private void _UnregisterEvents()
	{
		pointerOver3D.OnEnter.RemoveListener(_OnPointerEnter);
		pointerOver3D.OnExit.RemoveListener(_OnPointerExit);
	}

	private void _OnPointerEnter(PointerEventData eventData)
	{
		pointerIsOver = true;
	}

	private void _OnPointerExit(PointerEventData eventData)
	{
		pointerIsOver = false;
	}

	private void Awake()
	{
		_RegisterEvents();
	}

	private void OnEnable()
	{
		if (pointerIsOver)
		{
			_DoPointerOverLogic();
		}
	}

	private void OnDisable()
	{
		if (pointerIsOver)
		{
			_DoPointerExitLogic();
		}
	}

	private void OnDestroy()
	{
		_UnregisterEvents();
	}
}
