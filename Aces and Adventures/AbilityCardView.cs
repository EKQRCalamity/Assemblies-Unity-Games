using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AbilityCardView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/AbilityCardView";

	[Header("Ability")]
	public TextMeshProUGUI nameText;

	public CardTooltipLayout tooltipLayout;

	public CardTooltipLayout costTooltipLayout;

	public StringEvent onNameChange;

	public StringEvent onDescriptionChange;

	public StringEvent onTagsChange;

	public BoolEvent onHasTagsChange;

	public MaterialEvent onCardFrontChange;

	public MaterialEvent onCardBackChange;

	public RectTransform costContainer;

	public BoolEvent onHasCostChange;

	public Texture2DEvent onImageChange;

	public RectEvent onImageUVChange;

	public BoolEvent onTappedChange;

	public BoolEvent onHasUsesRemainingChange;

	public BoolEvent onHasModifiedCostChange;

	private bool _hasUsesRemaining = true;

	private bool _hasModifiedCost;

	public Ability ability
	{
		get
		{
			return base.target as Ability;
		}
		set
		{
			base.target = value;
		}
	}

	private bool hasUsesRemaining
	{
		set
		{
			if (SetPropertyUtility.SetStruct(ref _hasUsesRemaining, value))
			{
				_OnHasUsesRemainingChange(value);
			}
		}
	}

	private bool hasModifiedCost
	{
		set
		{
			if (SetPropertyUtility.SetStruct(ref _hasModifiedCost, value))
			{
				_OnHasModifiedCostChange(value);
			}
		}
	}

	public static AbilityCardView Create(Ability card, Transform parent = null)
	{
		return Pools.Unpool(Blueprint, parent).GetComponent<AbilityCardView>()._SetData(card);
	}

	private AbilityCardView _SetData(Ability ability)
	{
		this.ability = ability;
		return this;
	}

	private void _OnResourceCostChange(AResourceCosts resourceCost)
	{
		costContainer.gameObject.DestroyChildren();
		foreach (GameObject costIcon in resourceCost.GetCostIcons())
		{
			costIcon.transform.SetParent(costContainer, worldPositionStays: false);
		}
		onHasCostChange?.Invoke(costContainer.transform.childCount > 0);
		hasModifiedCost = !ability.naturalCost.Equals(resourceCost);
	}

	private void _OnTappedChange(bool tapped)
	{
		onTappedChange?.Invoke(tapped);
	}

	private void _OnHasUsesRemainingChange(bool setHasUsesRemaining)
	{
		onHasUsesRemainingChange?.Invoke(setHasUsesRemaining);
	}

	private void _OnHasModifiedCostChange(bool setHasModifiedCost)
	{
		onHasModifiedCostChange?.Invoke(setHasModifiedCost);
	}

	private IEnumerable<string> _GetTooltips()
	{
		if (tooltipLayout.onShow != null)
		{
			return Enumerable.Empty<string>();
		}
		return ability.GetTooltips();
	}

	private IEnumerable<(string, Func<GameObject>)> _GetCostTooltips()
	{
		if (tooltipLayout.onShow != null)
		{
			return Enumerable.Empty<(string, Func<GameObject>)>();
		}
		using PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<string>();
		PoolKeepItemListHandle<(string, Func<GameObject>)> poolKeepItemListHandle = Pools.UseKeepItemList<(string, Func<GameObject>)>();
		foreach (LocalizedStringRef localizedStringRef in costContainer.gameObject.GetComponentsInChildrenPooled<LocalizedStringRef>())
		{
			string text = localizedStringRef.localizedString.Localize();
			if (text != null && poolKeepItemHashSetHandle.Add(text))
			{
				poolKeepItemListHandle.Add((text, () => UnityEngine.Object.Instantiate(localizedStringRef.gameObject)));
			}
		}
		return poolKeepItemListHandle.AsEnumerable();
	}

	private void _SetHasUsesRemaining(bool hasUses)
	{
		hasUsesRemaining = hasUses;
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (oldTarget is Ability ability)
		{
			ability.onResourceCostChange -= _OnResourceCostChange;
			ability.tapped.onValueChanged -= _OnTappedChange;
			ability.onHasUsesRemainingChange -= _SetHasUsesRemaining;
		}
		if (!(newTarget is Ability ability2))
		{
			return;
		}
		onNameChange?.InvokeLocalized(this, () => this.ability?.data.name);
		onDescriptionChange?.InvokeLocalized(this, () => this.ability?.data.description);
		_OnResourceCostChange(ability2.cost);
		ability2.onResourceCostChange += _OnResourceCostChange;
		AbilityCardSkins.AbilityCardSkin abilityCardSkin = AbilityCardSkins.Default[ability2.data.characterClass ?? (ability2.owner as Player)?.characterClass];
		onCardFrontChange?.Invoke(AbilityCardSkins.Default[ability2.itemType]?.cardFronts.normal ?? abilityCardSkin.cardFronts[ability2.data.rank]);
		onCardBackChange?.Invoke(abilityCardSkin.cardBack);
		onTagsChange?.InvokeLocalized(this, () => this.ability?.GetLocalizedTagString());
		onHasTagsChange?.Invoke(ability2.GetTagKeywords().Any());
		if ((bool)ability2.data.cosmetic.image)
		{
			ability2.data.cosmetic.image.image.GetTexture2D(delegate(Texture2D texture)
			{
				this.InvokeIfAlive(onImageChange, texture);
			});
			onImageUVChange?.Invoke(ability2.data.cosmetic.image);
		}
		if (ability2 is ItemCard data)
		{
			GetComponent<ItemCardView>()?.SetData(data);
		}
		ability2.tapped.onValueChanged += _OnTappedChange;
		_OnTappedChange(ability2.tapped);
		ability2.onHasUsesRemainingChange += _SetHasUsesRemaining;
		hasUsesRemaining = ability2.hasUsesRemaining;
	}

	public override void ShowTooltips()
	{
		tooltipLayout.Show(_GetTooltips());
		costTooltipLayout?.Show(_GetCostTooltips(), CardTooltipLayout.DirectionAlongAxis.Positive);
	}

	public void ShowTooltips(CardTooltipLayout.DirectionAlongAxis directionAlongAxis)
	{
		tooltipLayout.Show(_GetTooltips(), directionAlongAxis);
		costTooltipLayout?.Show(_GetCostTooltips(), CardTooltipLayout.DirectionAlongAxis.Positive);
	}

	public override void HideTooltips()
	{
		tooltipLayout.Hide();
		costTooltipLayout?.Hide();
	}

	public void SetHasUsesRemaining(bool hasUses)
	{
		hasUsesRemaining = hasUses;
	}

	public override string ToString()
	{
		return (ability?.ToString() ?? base.ToString()) + " [View]";
	}
}
