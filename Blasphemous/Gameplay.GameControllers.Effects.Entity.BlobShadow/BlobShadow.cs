using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Tools.Level.Layout;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Entity.BlobShadow;

[RequireComponent(typeof(SpriteRenderer))]
public class BlobShadow : MonoBehaviour
{
	private float _blobHeight;

	private RaycastHit2D[] _bottomHits;

	private LevelInitializer _currentLevel;

	[SerializeField]
	private Gameplay.GameControllers.Entities.Entity _entity;

	private Collider2D _entityCollider;

	private EntityShadow _entityShadow;

	private bool _isOnFloor;

	private bool _isScaleReduce;

	private const float ShadowRotation = 10f;

	private SpriteRenderer blobSpriteRenderer;

	public LayerMask floorLayer;

	public bool ManuallyControllingAlpha;

	public Gameplay.GameControllers.Entities.Entity Owner => _entity;

	private void Awake()
	{
		blobSpriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		if (_entity == null)
		{
			Debug.LogError("Blob shadow needs an entity to works");
			return;
		}
		_currentLevel = Core.Logic.CurrentLevelConfig;
		SetLevelColor(_currentLevel.levelShadowColor);
		SetHidden(_currentLevel.hideShadows);
		_blobHeight = blobSpriteRenderer.bounds.extents.y;
		_bottomHits = new RaycastHit2D[1];
		_isScaleReduce = false;
		_entityShadow = _entity.GetComponentInChildren<EntityShadow>();
		_entity.OnDeath += EntityOnEntityDie;
	}

	private void Update()
	{
		if (!(_entity == null))
		{
			if (_entity.SpriteRenderer.isVisible)
			{
				CheckCliffBorder();
			}
			if (!ManuallyControllingAlpha)
			{
				SetShadowAlpha((!_entity.Status.IsGrounded || _entity.Status.Dead || !_entity.Status.CastShadow) ? 0f : 1f);
			}
			if (_isOnFloor)
			{
				SetRotation(_entity.SlopeAngle);
				ReduceScale(reduce: false);
			}
			else
			{
				ReduceScale();
			}
		}
	}

	private void LateUpdate()
	{
		if (!(_entity == null) && !_entity.Status.Dead)
		{
			SetPosition();
		}
	}

	public void SetEntity(Gameplay.GameControllers.Entities.Entity entity)
	{
		if (!(_entity != null))
		{
			_entity = entity;
			_entityCollider = _entity.EntityDamageArea.DamageAreaCollider;
			Owner.Shadow = this;
			Owner.Status.CastShadow = true;
		}
	}

	private void SetPosition()
	{
		if (!(_entityCollider == null))
		{
			Vector2 vector = new Vector2(_entityCollider.bounds.center.x, _entityCollider.bounds.min.y);
			base.transform.position = vector;
		}
	}

	public void SetShadowAlpha(float alpha)
	{
		float a = Mathf.Clamp01(alpha);
		Color color = blobSpriteRenderer.color;
		color.a = a;
		blobSpriteRenderer.color = color;
	}

	public float GetShadowAlpha()
	{
		return blobSpriteRenderer.color.a;
	}

	private void SetRotation(float slopeAngle)
	{
		if (slopeAngle >= 5f)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, 10f);
		}
		else if (slopeAngle <= -5f)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, -10f);
		}
		else
		{
			base.transform.rotation = Quaternion.identity;
		}
	}

	private void ReduceScale(bool reduce = true)
	{
		if (reduce && !_isScaleReduce)
		{
			_isScaleReduce = true;
			Vector3 localScale = new Vector3(0.5f, 0.5f, 1f);
			base.transform.localScale = localScale;
		}
		else if (!reduce && _isScaleReduce)
		{
			_isScaleReduce = false;
			Vector3 localScale2 = new Vector3(1f, 1f, 1f);
			base.transform.localScale = localScale2;
		}
	}

	private void SetHidden(bool hidden)
	{
		blobSpriteRenderer.enabled = !hidden;
	}

	public void SetLevelColor(Color color)
	{
		if (blobSpriteRenderer.color != color)
		{
			blobSpriteRenderer.color = color;
		}
	}

	private void EntityOnEntityDie()
	{
		_entity = null;
		SetShadowAlpha(0f);
		Core.Logic.CurrentLevelConfig.BlobShadowManager.StoreBlobShadow(base.gameObject);
	}

	private void CheckCliffBorder()
	{
		if (!(_entity == null) && !(_entityShadow == null))
		{
			Vector2 vector = new Vector2(_entityCollider.bounds.min.x + _entityShadow.ShadowXOffset, _entityCollider.bounds.min.y + _entityShadow.ShadowYOffset + _blobHeight);
			Debug.DrawLine(vector, vector - Vector2.up * 0.5f, Color.white);
			bool flag = Physics2D.LinecastNonAlloc(vector, vector - Vector2.up * 1f, _bottomHits, floorLayer) > 0;
			Vector2 vector2 = new Vector2(_entityCollider.bounds.max.x - _entityShadow.ShadowXOffset, _entityCollider.bounds.min.y + _entityShadow.ShadowYOffset + _blobHeight);
			Debug.DrawLine(vector2, vector2 - Vector2.up * 0.5f, Color.white);
			bool flag2 = Physics2D.LinecastNonAlloc(vector2, vector2 - Vector2.up * 1f, _bottomHits, floorLayer) > 0;
			_isOnFloor = flag2 && flag;
		}
	}
}
