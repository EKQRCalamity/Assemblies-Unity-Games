using UnityEngine;
using UnityEngine.Events;

public class CardPackView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/Rewards/CardPackView";

	[Header("Card Pack")]
	public AppliedAbilityDeckLayout abilitiesLayout;

	[Range(0.1f, 5f)]
	public float openAnimationTime = 1f;

	public FloatEvent onGlowStrengthChange;

	public UnityEvent onBeginOpenAnimation;

	public FloatEvent onOpenAnimationChange;

	public UnityEvent onFinishOpenAnimation;

	public UnityEvent onOpen;

	private float _glowStrength;

	private float _openAnimation = -1f;

	private bool _readyToOpen;

	public float glowStrength
	{
		get
		{
			return _glowStrength;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _glowStrength, value))
			{
				onGlowStrengthChange?.Invoke(value);
			}
		}
	}

	public bool readyToOpen => _readyToOpen;

	public bool opening
	{
		get
		{
			if (!(_openAnimation >= 0f))
			{
				return readyToOpen;
			}
			return true;
		}
	}

	public static CardPackView Create(CardPack cardPack, Transform parent = null)
	{
		return DirtyPools.Unpool(Blueprint, parent).GetComponent<CardPackView>().SetData(cardPack) as CardPackView;
	}

	private void Update()
	{
		if (!(_openAnimation < 0f))
		{
			_openAnimation += Time.deltaTime / openAnimationTime;
			_openAnimation = Mathf.Clamp01(_openAnimation);
			onOpenAnimationChange?.Invoke(_openAnimation);
			if (!(_openAnimation < 1f))
			{
				_readyToOpen = true;
				onFinishOpenAnimation?.Invoke();
				_openAnimation = -1f;
			}
		}
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (newTarget is CardPack cardPack)
		{
			abilitiesLayout.deck = cardPack.abilities;
		}
	}

	public void BeginOpen()
	{
		onBeginOpenAnimation?.Invoke();
		_openAnimation = 0f;
	}

	public void Open()
	{
		onOpen?.Invoke();
	}

	public void SetGlowStrength(float newGlowStrength)
	{
		onGlowStrengthChange?.Invoke(_glowStrength = newGlowStrength);
	}
}
