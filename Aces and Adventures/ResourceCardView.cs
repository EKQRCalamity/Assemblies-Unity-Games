using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerScroll3D))]
public class ResourceCardView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/ResourceCardView";

	private static readonly int HOLO_MASK_ID = Shader.PropertyToID("HolofoilMask");

	private static readonly int OVERLAY_INTENSITY = Shader.PropertyToID("_OverlayIntensity");

	private static readonly int MASTER_DISTORTION_SCALE = Shader.PropertyToID("_MasterDistortionScale");

	private static readonly int HOLO_DISTORT_SCALING = Shader.PropertyToID("_HoloDistortScaling");

	private static readonly int LUM_HOLO_POWER = Shader.PropertyToID("LuminanceHoloPower");

	[Header("Resource")]
	public Transform wildValueSelectionContainer;

	public Renderer cardFrontRenderer;

	public MaterialEvent onCardBackMaterialChange;

	public BoolEvent onIsJokerChange;

	public MaterialEvent onJokerMaterialChange;

	[Range(0.1f, 2f)]
	public float hasWildOverlayIntensity = 1f;

	[Range(0.1f, 2f)]
	public float isWildDistortionScale = 1f;

	[Range(1f, 10f)]
	public float materialEaseSpeed = 3f;

	[Range(0.1f, 2f)]
	public float nonFaceCardEffectScale = 1.2f;

	[Range(0f, 2f)]
	public float holoDistortScaling = 0.42f;

	private PointerScroll3D _pointerScroll;

	private float _targetOverlayIntensity;

	private float _overlayIntensity;

	private float _targetDistortionScale;

	private float _distortionScale;

	private float _targetLuminancePower;

	private float _luminancePower;

	private Material _material;

	private TooltipVisibility _tooltipVisibility;

	private bool _materialNeedsToUpdate;

	public ResourceCard resourceCard
	{
		get
		{
			return (ResourceCard)base.target;
		}
		set
		{
			base.target = value;
		}
	}

	private TooltipVisibility tooltipVisibility
	{
		get
		{
			if (!_tooltipVisibility)
			{
				return _tooltipVisibility = this.GetOrAddComponent<TooltipVisibility>().SetEvents(_ShowWildValueSelection);
			}
			return _tooltipVisibility;
		}
	}

	public static ResourceCardView Create(ResourceCard card, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<ResourceCardView>()._SetData(card);
	}

	private ResourceCardView _SetData(ResourceCard newCard)
	{
		resourceCard = newCard;
		return this;
	}

	private void _OnWildValueChange(PlayingCard? wildValue)
	{
		PlayingCardSkin resource = EnumUtil<PlayingCardSkinType>.GetResource<PlayingCardSkin>(resourceCard.skin);
		Material material = resource[resourceCard];
		if (!_material)
		{
			Material material3 = (cardFrontRenderer.material = UnityEngine.Object.Instantiate(resourceCard.ephemeral ? resource.transparent : material));
			_material = material3;
		}
		_material.SetTexture(GraphicsUtil.BASE_COLOR_MAP_ID, material.GetTexture(GraphicsUtil.BASE_COLOR_MAP_ID));
		_material.SetTexture(GraphicsUtil.MASK_MAP_ID, material.GetTexture(GraphicsUtil.MASK_MAP_ID));
		_material.SetTexture(GraphicsUtil.NORMAL_MAP_ID, material.GetTexture(GraphicsUtil.NORMAL_MAP_ID));
		_material.SetTexture(HOLO_MASK_ID, material.GetTexture(HOLO_MASK_ID));
		_material.SetFloat(HOLO_DISTORT_SCALING, holoDistortScaling);
		_materialNeedsToUpdate |= SetPropertyUtility.SetStruct(ref _targetLuminancePower, resourceCard.skin.HoloLuminancePower(resourceCard.value.IsFace()));
		_materialNeedsToUpdate |= SetPropertyUtility.SetStruct(ref _targetDistortionScale, resourceCard.isWild ? (isWildDistortionScale * resourceCard.value.IsFace().ToFloat(1f, nonFaceCardEffectScale)) : 0f);
		_OnWildsChange(resourceCard);
		onCardBackMaterialChange?.Invoke(resource.cardBack);
		ResourceCard obj = resourceCard;
		if (obj != null && obj.isJoker)
		{
			onJokerMaterialChange?.Invoke(resource.joker);
		}
	}

	private void _OnWildsChange(PlayingCardTypes wilds)
	{
		_materialNeedsToUpdate |= SetPropertyUtility.SetStruct(ref _targetOverlayIntensity, resourceCard.hasWild ? (hasWildOverlayIntensity * resourceCard.skin.HoloIntensityMultiplier(resourceCard.value.IsFace()) * resourceCard.value.IsFace().ToFloat(1f, nonFaceCardEffectScale)) : 0f);
	}

	private void _OnRightClick(PointerEventData eventData)
	{
		if (GameStateView.Instance.WildInputEnabled(resourceCard))
		{
			resourceCard.suitUserInput = EnumUtil<PlayingCardSuits>.NextConvertWrap(resourceCard.suits, resourceCard.suit);
		}
	}

	private void _OnMiddleClick(PointerEventData eventData)
	{
		if (GameStateView.Instance.WildInputEnabled(resourceCard))
		{
			resourceCard.wildValueUserInput = null;
		}
	}

	private void _OnScrollUp()
	{
		if (GameStateView.Instance.WildInputEnabled(resourceCard))
		{
			resourceCard.valueUserInput = EnumUtil<PlayingCardValues>.NextConvert(resourceCard.values, resourceCard.value);
		}
	}

	private void _OnScrollDown()
	{
		if (GameStateView.Instance.WildInputEnabled(resourceCard))
		{
			resourceCard.valueUserInput = EnumUtil<PlayingCardValues>.PreviousConvert(resourceCard.values, resourceCard.value);
		}
	}

	private void _ShowWildValueSelection()
	{
		WildValueSelection.Create(resourceCard, wildValueSelectionContainer);
	}

	private void _HideWildValueSelection()
	{
		if (wildValueSelectionContainer.childCount <= 0)
		{
			return;
		}
		foreach (WildValueSelection item in wildValueSelectionContainer.gameObject.GetComponentsInChildrenPooled<WildValueSelection>())
		{
			item.Hide();
		}
	}

	private void _OnBeingDraggedChange(bool beingDragged)
	{
		if (beingDragged)
		{
			OnPointerExit();
		}
		else if (base.layout?.pointerOver == this)
		{
			OnPointerEnter();
		}
	}

	private void _OnPlayingCardDeckChange(PlayingCardSkinType skin)
	{
		resourceCard.skin = skin;
		if ((bool)_material)
		{
			UnityEngine.Object.Destroy(_material);
		}
		_material = null;
		_OnWildValueChange(resourceCard.wildValue);
		_material.SetFloat(OVERLAY_INTENSITY, _overlayIntensity);
		_material.SetFloat(MASTER_DISTORTION_SCALE, _distortionScale);
		_material.SetFloat(LUM_HOLO_POWER, _luminancePower);
	}

	protected override void Awake()
	{
		base.Awake();
		this.CacheComponent(ref _pointerScroll);
		base.pointerClick.OnRightClick.AddListener(_OnRightClick);
		base.pointerClick.OnMiddleClick.AddListener(_OnMiddleClick);
		_pointerScroll.onScrollUp.AddListener(_OnScrollUp);
		_pointerScroll.onScrollDown.AddListener(_OnScrollDown);
		onBeingDraggedChange.AddListener(_OnBeingDraggedChange);
	}

	private void Update()
	{
		if (_materialNeedsToUpdate)
		{
			bool flag = false;
			if ((flag |= SetPropertyUtility.SetStruct(ref _overlayIntensity, MathUtil.DeltaSnap(MathUtil.Ease(_overlayIntensity, _targetOverlayIntensity, materialEaseSpeed, Time.deltaTime), _targetOverlayIntensity))) && (bool)_material)
			{
				_material.SetFloat(OVERLAY_INTENSITY, _overlayIntensity);
			}
			if ((flag |= SetPropertyUtility.SetStruct(ref _distortionScale, MathUtil.DeltaSnap(MathUtil.Ease(_distortionScale, _targetDistortionScale, materialEaseSpeed, Time.deltaTime), _targetDistortionScale))) && (bool)_material)
			{
				_material.SetFloat(MASTER_DISTORTION_SCALE, _distortionScale);
			}
			if ((flag |= SetPropertyUtility.SetStruct(ref _luminancePower, MathUtil.DeltaSnap(MathUtil.Ease(_luminancePower, _targetLuminancePower, materialEaseSpeed, Time.deltaTime), _targetLuminancePower))) && (bool)_material)
			{
				_material.SetFloat(LUM_HOLO_POWER, _luminancePower);
			}
			_materialNeedsToUpdate = flag;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)_material)
		{
			UnityEngine.Object.Destroy(_material);
		}
		_material = null;
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (oldTarget is ResourceCard resourceCard)
		{
			resourceCard.onWildValueChange = (Action<PlayingCard?>)Delegate.Remove(resourceCard.onWildValueChange, new Action<PlayingCard?>(_OnWildValueChange));
			resourceCard.onWildsChange = (Action<PlayingCardTypes>)Delegate.Remove(resourceCard.onWildsChange, new Action<PlayingCardTypes>(_OnWildsChange));
			if (resourceCard.faction == Faction.Player)
			{
				ProfileOptions.CosmeticOptions.OnPlayingCardDeckChange -= _OnPlayingCardDeckChange;
			}
		}
		if (newTarget is ResourceCard resourceCard2)
		{
			_OnWildValueChange(resourceCard2.wildValue);
			_OnWildsChange(resourceCard2.wilds);
			resourceCard2.onWildValueChange = (Action<PlayingCard?>)Delegate.Combine(resourceCard2.onWildValueChange, new Action<PlayingCard?>(_OnWildValueChange));
			resourceCard2.onWildsChange = (Action<PlayingCardTypes>)Delegate.Combine(resourceCard2.onWildsChange, new Action<PlayingCardTypes>(_OnWildsChange));
			onIsJokerChange?.Invoke(resourceCard2.isJoker);
			if (resourceCard2.faction == Faction.Player)
			{
				ProfileOptions.CosmeticOptions.OnPlayingCardDeckChange += _OnPlayingCardDeckChange;
			}
		}
	}

	public override string ToString()
	{
		return $"Resource Card View: {resourceCard}";
	}

	public void OnPointerEnter()
	{
		if ((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(resourceCard?.pile) || !(resourceCard?.view.layout != base.view.adventureDeckLayout.inspectHand))
		{
			WildValueSelection componentInChildren = wildValueSelectionContainer.GetComponentInChildren<WildValueSelection>();
			if ((object)componentInChildren != null)
			{
				componentInChildren.Show();
			}
			else
			{
				tooltipVisibility.StartTimer();
			}
		}
	}

	public void OnPointerExit()
	{
		_HideWildValueSelection();
		_tooltipVisibility?.EndTimer();
	}
}
