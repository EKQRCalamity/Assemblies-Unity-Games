using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCardView : CombatantCardView
{
	public new static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/PlayerCardView";

	private static Dictionary<RebirthLevel, Sprite> _RebirthSprites;

	[Header("Player")]
	public BoolEvent onShowLeafChange;

	public StringEvent onLeafChange;

	public SpriteEvent onRebirthSpriteChange;

	private static Dictionary<RebirthLevel, Sprite> RebirthSprites => _RebirthSprites ?? (_RebirthSprites = ReflectionUtil.CreateEnumResourceMap<RebirthLevel, Sprite>("GameState/RebirthLevelIcon"));

	public Player player => (Player)base.target;

	private void _OnNumberOfAttacksChanged(int previous, int current)
	{
		player.gameState.chipDeck.Layout<ChipDeckLayout>().SetCountInPile(Chip.Pile.Attack, ChipType.Attack, current);
	}

	private void _OnScryPlayingCardChanged(int previous, int current)
	{
		if (GameStateView.Instance?.playerResourceDeckLayout?.GetLayout(ResourceCard.Pile.DrawPile) is CardStackLayout cardStackLayout)
		{
			cardStackLayout.dragCount = current;
		}
	}

	private void _OnScryAbilityCardChanged(int previous, int current)
	{
		if (GameStateView.Instance?.playerAbilityDeckLayout?.GetLayout(Ability.Pile.Draw) is CardStackLayout cardStackLayout)
		{
			cardStackLayout.dragCount = current;
		}
	}

	private void _OnScryEnemyPlayingCardChanged(int previous, int current)
	{
		if (GameStateView.Instance?.enemyResourceDeckLayout?.GetLayout(ResourceCard.Pile.DrawPile) is CardStackLayout cardStackLayout)
		{
			cardStackLayout.dragCount = current;
		}
	}

	private void _OnLeafChanged()
	{
		int level = player.level;
		int levelWithRebirth = ProfileManager.progress.experience.read.GetLevelWithRebirth(player.characterDataRef);
		int? num = ProfileManager.options.rebirth.GetLevelOverride(player.characterDataRef);
		if (num == levelWithRebirth)
		{
			num = null;
		}
		RebirthLevel rebirth = player.rebirth;
		string text = (num ?? levelWithRebirth).ToString();
		onShowLeafChange?.Invoke(level > 0 || rebirth > RebirthLevel.Zero);
		onLeafChange?.Invoke(num.HasValue ? ("<alpha=#CD><i>" + text) : ("<b>" + text));
		onRebirthSpriteChange?.Invoke(RebirthSprites[rebirth]);
	}

	private void _OnRebirthLevelOverrideChange(int level)
	{
		if (!this.player.gameState.parameters.adventureBeganInitialize)
		{
			_OnLeafChanged();
			Player player = new Player(this.player.characterDataRef);
			BInt hP = this.player.HP;
			int value = (this.player.stats.health.value = player.stats.health);
			hP.value = value;
			this.player.playerStats.abilityHandSize.value = player.playerStats.abilityHandSize;
		}
	}

	private void _OnRightClick(PointerEventData eventData)
	{
		Player obj = player;
		if (obj != null && obj.gameState.stack.activeStep?.canInspect == true && !player.gameState.heroDeck.Any(HeroDeckPile.SelectionHand) && !InputManager.I[KeyModifiers.Shift] && !eventData.dragging)
		{
			player.gameState.stack.Append(new GameStepViewCharacterTraits(player));
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.pointerClick.OnRightClick.AddListener(_OnRightClick);
	}

	protected override IEnumerable<string> _GetTooltips()
	{
		foreach (DataRef<AbilityData> trait in player.combatantData.traits)
		{
			yield return ATargetView.Builder.Bold().Append(trait.data.name).EndBold()
				.Append(": ")
				.Append(trait.data.description);
		}
	}

	protected override void _OnCardChange()
	{
		base._OnCardChange();
		onCardFrontChange?.Invoke(CharacterCardSkins.Default[player.characterClass][player.rebirth]);
		onCardBackChange?.Invoke(CharacterCardSkins.Default[player.characterClass].cardBack);
		player.numberOfAttacks.onValueChanged += _OnNumberOfAttacksChanged;
		_OnScryPlayingCardChanged(0, player.playerStats.scryPlayingCards);
		player.playerStats.scryPlayingCards.onValueChanged += _OnScryPlayingCardChanged;
		_OnScryAbilityCardChanged(0, player.playerStats.scryAbilityCards);
		player.playerStats.scryAbilityCards.onValueChanged += _OnScryAbilityCardChanged;
		_OnScryEnemyPlayingCardChanged(0, player.playerStats.scryEnemyPlayingCards);
		player.playerStats.scryEnemyPlayingCards.onValueChanged += _OnScryEnemyPlayingCardChanged;
		_OnLeafChanged();
		if (ProfileManager.progress.experience.read.CanOverrideLevel(player.characterDataRef))
		{
			ProfileOptions.RebirthOptions.OnLevelChange += _OnRebirthLevelOverrideChange;
		}
		ProfileOptions.DevOptions.OnLevelChange = (Action<int>)Delegate.Combine(ProfileOptions.DevOptions.OnLevelChange, new Action<int>(_OnRebirthLevelOverrideChange));
	}

	protected override void _UnregisterCombatant(ACombatant combatant)
	{
		Player obj = (Player)combatant;
		obj.numberOfAttacks.onValueChanged -= _OnNumberOfAttacksChanged;
		obj.playerStats.scryPlayingCards.onValueChanged -= _OnScryPlayingCardChanged;
		obj.playerStats.scryAbilityCards.onValueChanged -= _OnScryAbilityCardChanged;
		obj.playerStats.scryEnemyPlayingCards.onValueChanged -= _OnScryEnemyPlayingCardChanged;
		ProfileOptions.RebirthOptions.OnLevelChange -= _OnRebirthLevelOverrideChange;
		ProfileOptions.DevOptions.OnLevelChange = (Action<int>)Delegate.Remove(ProfileOptions.DevOptions.OnLevelChange, new Action<int>(_OnRebirthLevelOverrideChange));
		base._UnregisterCombatant(combatant);
	}

	public IEnumerable<ATargetView> GetAdditionalResourceViews(AdditionalResourceCosts costs)
	{
		if (costs.hp > 0 && (bool)_hpDice)
		{
			ATargetView hpView = _hpDice.GetComponent<ATargetView>();
			if ((object)hpView != null)
			{
				for (int x2 = 0; x2 < costs.hp; x2++)
				{
					yield return hpView;
				}
			}
		}
		if (costs.shield > 0 && (bool)_shieldDice)
		{
			ATargetView shieldView = _shieldDice.GetComponent<ATargetView>();
			if ((object)shieldView != null)
			{
				for (int x2 = 0; x2 < costs.shield; x2++)
				{
					yield return shieldView;
				}
			}
		}
		if (costs.attack <= 0)
		{
			yield break;
		}
		foreach (Chip item in player.gameState.chipDeck.GetCards(Chip.Pile.Attack).Reverse().Take(costs.attack))
		{
			yield return item.view;
		}
	}
}
