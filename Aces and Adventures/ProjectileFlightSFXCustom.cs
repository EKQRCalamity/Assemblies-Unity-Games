using System;
using UnityEngine;

public class ProjectileFlightSFXCustom : ProjectileFlightSFX
{
	private static GameObject _CustomBlueprint;

	private ImageRef _imageRef;

	private PositionAngle _positionAngle;

	private Alpha2? _front;

	private Alpha2? _back;

	private Vector3? _frontLocalPosition;

	private Vector3? _backLocalPosition;

	private MeshFilter _meshFilter;

	private MeshRenderer _meshRenderer;

	private MaterialInstancer _materialInstancer;

	private Action<Texture2D> _getTextureAction;

	private Action<Mesh> _getMeshAction;

	public static GameObject CustomBlueprint
	{
		get
		{
			if (!_CustomBlueprint)
			{
				return _CustomBlueprint = Resources.Load<GameObject>("Gameplay/Ability/Projectiles/ProjectileFlightSFXCustom");
			}
			return _CustomBlueprint;
		}
	}

	private MeshFilter meshFilter
	{
		get
		{
			if (!_meshFilter)
			{
				return _meshFilter = GetComponentInChildren<MeshFilter>();
			}
			return _meshFilter;
		}
	}

	private MeshRenderer meshRenderer
	{
		get
		{
			if (!_meshRenderer)
			{
				return _meshRenderer = GetComponentInChildren<MeshRenderer>();
			}
			return _meshRenderer;
		}
	}

	private MaterialInstancer materialInstancer
	{
		get
		{
			if (!_materialInstancer)
			{
				return _materialInstancer = GetComponentInChildren<MaterialInstancer>();
			}
			return _materialInstancer;
		}
	}

	private Action<Texture2D> getTextureAction => _GetTexture;

	private Action<Mesh> getMeshAction => _GetMesh;

	private void _GetTexture(Texture2D texture)
	{
		if (base.isActiveAndEnabled)
		{
			materialInstancer.SetMainTexture(texture);
			ProjectileViewUtil.GetCutoutMesh(_imageRef, getMeshAction);
		}
	}

	private void _GetMesh(Mesh mesh)
	{
		if (base.isActiveAndEnabled)
		{
			meshFilter.sharedMesh = mesh;
			meshRenderer.enabled = true;
			meshRenderer.transform.localRotation = Quaternion.Euler(0f, -90f, 0f - _positionAngle.angle * 57.29578f);
			Rect r = meshFilter.sharedMesh.bounds.Project(AxisType.Z);
			Vector2 v = r.Lerp(_positionAngle);
			base.innerTransform.localPosition = meshRenderer.transform.localRotation * -v.Multiply((Vector2)meshRenderer.transform.localScale);
			_frontLocalPosition = _frontLocalPosition ?? base.frontTransform.localPosition;
			_backLocalPosition = _backLocalPosition ?? base.backTransform.localPosition;
			base.frontTransform.localPosition = (_front.HasValue ? (meshRenderer.transform.localRotation * r.Lerp(_front.Value) * _defaultRadius * 2f) : _frontLocalPosition.Value);
			base.backTransform.localPosition = (_back.HasValue ? (meshRenderer.transform.localRotation * r.Lerp(_back.Value) * _defaultRadius * 2f) : _backLocalPosition.Value);
			Quaternion localRotation = base.innerTransform.localRotation;
			base.innerTransform.localRotation = Quaternion.identity;
			if ((bool)_emitterTransform)
			{
				_emitterTransform.position = _innerTransform.position - base.transform.position + _innerTransform.position;
				_emitterTransform.rotation = base.transform.rotation.Opposite();
			}
			if ((bool)_impactTransform)
			{
				_impactTransform.position = base.transform.position;
				_impactTransform.rotation = base.transform.rotation;
			}
			base.innerTransform.localRotation = localRotation;
			base.innerTransform.localPosition = base.innerTransform.localPosition.Project(AxisType.Y).Unproject(AxisType.Y);
		}
	}

	private void OnEnable()
	{
		meshRenderer.enabled = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_imageRef = null;
	}

	public void SetImageRef(ImageRef imageRef, ImageRefPositionAngle positionAngle, Alpha2? front, Alpha2? back)
	{
		_imageRef = imageRef;
		_positionAngle = positionAngle;
		_front = front;
		_back = back;
		_imageRef.GetTexture2D(getTextureAction);
	}
}
