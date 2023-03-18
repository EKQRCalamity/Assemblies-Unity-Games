using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

public class EnemyHealthBar : PoolObject
{
	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private AnimationCurve HealthLossAnimationCurve;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private int MaxSizeBar = 16;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float PositionStep = -1f / 32f;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float FadeInTime = 0.2f;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float FadeOutTime = 0.5f;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private GameObject barRoot;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Transform instantBar;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Transform animatedBar;

	private int currentAnimated;

	private float _damageTimeElapsed;

	private SpriteRenderer backgroundSprite;

	private SpriteRenderer instantBarSprite;

	private SpriteRenderer animatedBarSprite;

	private TweenerCore<float, float, FloatOptions> tweenOut;

	private const string FLAG_ID = "SHOW_ENEMY_BAR";

	private Enemy Owner;

	private EntityOrientation currentOrientation;

	private Vector2 leftOffset = new Vector2(0f, 2f);

	private Vector2 rightOffset = new Vector2(0f, 2f);

	private float _spritesAlpha;

	public bool IsEnabled { get; private set; }

	public float SpritesAlpha
	{
		get
		{
			return _spritesAlpha;
		}
		set
		{
			_spritesAlpha = value;
			Color color = new Color(1f, 1f, 1f, _spritesAlpha);
			backgroundSprite.color = color;
			instantBarSprite.color = color;
			animatedBarSprite.color = color;
		}
	}

	public void UpdateParent(Enemy parent)
	{
		Owner = parent;
		barRoot.gameObject.SetActive(value: false);
		Owner.OnDamaged += OnDamaged;
		Owner.OnDeath += OnDead;
		leftOffset = new Vector2(0f - Owner.healthOffset.x, Owner.healthOffset.y);
		rightOffset = Owner.healthOffset;
		UpdateOffset();
	}

	private void UpdateOffset()
	{
		currentOrientation = Owner.Status.Orientation;
		base.transform.localPosition = ((currentOrientation != EntityOrientation.Left) ? rightOffset : leftOffset);
	}

	private void Awake()
	{
		IsEnabled = false;
		SetBarSizeAndPos(instantBar, MaxSizeBar);
		SetBarSizeAndPos(animatedBar, MaxSizeBar);
		currentAnimated = MaxSizeBar;
		backgroundSprite = barRoot.GetComponent<SpriteRenderer>();
		instantBarSprite = instantBar.GetComponent<SpriteRenderer>();
		animatedBarSprite = animatedBar.GetComponent<SpriteRenderer>();
		SpritesAlpha = 0f;
	}

	private void Update()
	{
		if (IsEnabled || (tweenOut != null && tweenOut.IsActive()))
		{
			if (tweenOut == null || !tweenOut.IsActive())
			{
				SpritesAlpha = Owner.SpriteRenderer.color.a;
			}
			if (currentOrientation != Owner.Status.Orientation)
			{
				UpdateOffset();
			}
			int num = Mathf.RoundToInt(Owner.Stats.Life.Current / Owner.Stats.Life.Final * (float)MaxSizeBar);
			SetBarSizeAndPos(instantBar, num);
			if (currentAnimated != num)
			{
				_damageTimeElapsed += Time.deltaTime;
				currentAnimated = Mathf.RoundToInt(Mathf.Lerp(currentAnimated, num, HealthLossAnimationCurve.Evaluate(_damageTimeElapsed)));
				SetBarSizeAndPos(animatedBar, currentAnimated);
			}
		}
	}

	private unsafe void OnDamaged()
	{
		if (!Core.Events.GetFlag("SHOW_ENEMY_BAR"))
		{
			return;
		}
		if (!Owner.UseHealthBar)
		{
			barRoot.gameObject.SetActive(value: false);
			return;
		}
		if (!IsEnabled)
		{
			UpdateOffset();
			DOTween.To(new DOGetter<float>(this, (nint)__ldftn(EnemyHealthBar.get_SpritesAlpha)), delegate(float x)
			{
				SpritesAlpha = x;
			}, 1f, FadeInTime);
		}
		IsEnabled = true;
		_damageTimeElapsed = 0f;
		barRoot.gameObject.SetActive(value: true);
	}

	private unsafe void OnDead()
	{
		IsEnabled = false;
		tweenOut = DOTween.To(new DOGetter<float>(this, (nint)__ldftn(EnemyHealthBar.get_SpritesAlpha)), delegate(float x)
		{
			SpritesAlpha = x;
		}, 0f, FadeOutTime);
		Owner.OnDamaged -= OnDamaged;
		Owner.OnDeath -= OnDead;
	}

	private void SetBarSizeAndPos(Transform bar, int size)
	{
		bar.localScale = new Vector3(size, 1f, 1f);
		bar.localPosition = new Vector3((float)(MaxSizeBar - size) * PositionStep, 0f, 0f);
	}
}
