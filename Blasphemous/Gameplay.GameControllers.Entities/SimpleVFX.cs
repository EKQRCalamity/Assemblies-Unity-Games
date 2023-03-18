using DG.Tweening;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class SimpleVFX : PoolObject
{
	public float maxTTL;

	private float currentTTL;

	[FoldoutGroup("Snap settings", false, 0)]
	public bool snapToGround;

	[ShowIf("snapToGround", true)]
	[FoldoutGroup("Snap settings", false, 0)]
	public LayerMask groundMask;

	[ShowIf("snapToGround", true)]
	[FoldoutGroup("Snap settings", false, 0)]
	public float rangeGroundDetection = 10f;

	public bool setTriggerOnReuse;

	[FoldoutGroup("Orientation settings", false, 0)]
	public bool FlipByPlayerOrientation;

	[ShowIf("FlipByPlayerOrientation", true)]
	[FoldoutGroup("Orientation settings", false, 0)]
	public bool useReversedPlayerOrientation;

	[ShowIf("FlipByPlayerOrientation", true)]
	[FoldoutGroup("Orientation settings", false, 0)]
	public bool useScaleToFlip;

	public string trigger;

	[EventRef]
	public string fxOneshotSound;

	private RaycastHit2D[] _bottomHits;

	private Animator _animator;

	private Tween fadeTween;

	protected SpriteRenderer[] _spriteRenderers;

	public bool fadeWithAlpha;

	public float alphaTime = 1f;

	private float originAlpha;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		if (_spriteRenderers.Length > 0)
		{
			originAlpha = _spriteRenderers[0].color.a;
		}
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		if (_spriteRenderers.Length > 0 && fadeWithAlpha)
		{
			SpriteRenderer[] spriteRenderers = _spriteRenderers;
			foreach (SpriteRenderer spriteRenderer in spriteRenderers)
			{
				Color color = spriteRenderer.color;
				color = new Color(color.r, color.g, color.b, originAlpha);
				spriteRenderer.color = color;
			}
		}
		if ((bool)_animator && setTriggerOnReuse)
		{
			_animator.SetTrigger(trigger);
		}
		if (snapToGround)
		{
			_bottomHits = new RaycastHit2D[1];
			SnapToGround();
		}
		currentTTL = maxTTL;
		if (FlipByPlayerOrientation)
		{
			SetOrientationByPlayer();
		}
		PlayExplosionFx();
	}

	private void Update()
	{
		currentTTL -= Time.deltaTime;
		if (currentTTL < 0f)
		{
			Destroy();
		}
		else if (_spriteRenderers.Length > 0 && currentTTL < alphaTime && fadeWithAlpha && (fadeTween == null || !fadeTween.IsPlaying()))
		{
			SpriteRenderer[] spriteRenderers = _spriteRenderers;
			foreach (SpriteRenderer target in spriteRenderers)
			{
				fadeTween = target.DOFade(0f, currentTTL);
			}
		}
	}

	protected void SetOrientationByPlayer()
	{
		SetOrientation(Core.Logic.Penitent.GetOrientation(), useReversedPlayerOrientation);
	}

	public void SetOrientation(EntityOrientation orientation, bool reverse = false)
	{
		EntityOrientation entityOrientation = ((!reverse) ? EntityOrientation.Left : EntityOrientation.Right);
		if (_spriteRenderers.Length > 0)
		{
			if (useScaleToFlip)
			{
				base.transform.localScale = new Vector3((orientation != entityOrientation) ? 1 : (-1), 1f, 1f);
				return;
			}
			SpriteRenderer[] spriteRenderers = _spriteRenderers;
			foreach (SpriteRenderer spriteRenderer in spriteRenderers)
			{
				spriteRenderer.flipX = orientation == entityOrientation;
			}
		}
		else
		{
			Debug.LogWarning("FlipX shouldn't be activated if the SimpleVFX doesn't have a SpriteRenderer controller");
		}
	}

	private void SnapToGround()
	{
		Vector2 vector = base.transform.position;
		if (Physics2D.LinecastNonAlloc(vector, vector + Vector2.down * rangeGroundDetection, _bottomHits, groundMask) > 0)
		{
			base.transform.position += Vector3.down * _bottomHits[0].distance;
		}
	}

	public void SetMaxTTL(float seconds)
	{
		maxTTL = seconds;
		currentTTL = seconds;
	}

	private void PlayExplosionFx()
	{
		if (!string.IsNullOrEmpty(fxOneshotSound))
		{
			Core.Audio.PlayOneShot(fxOneshotSound);
		}
	}
}
