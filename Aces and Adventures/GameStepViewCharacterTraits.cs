using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameStepViewCharacterTraits : GameStep
{
	private Player _player;

	private PoolKeepItemListHandle<Ability> _traits;

	private LayoutOffset _layoutOffset;

	private PlayerClass _class => _player.characterClass;

	private IEnumerable<ACardLayout> _layoutsToOffset
	{
		get
		{
			yield return base.view.playerResourceDeckLayout.hand;
			yield return base.view.playerAbilityDeckLayout.hand;
			yield return base.view.playerResourceDeckLayout.activationHand;
			yield return base.view.playerResourceDeckLayout.select;
			yield return base.view.adventureDeckLayout.selectionHand;
		}
	}

	public GameStepViewCharacterTraits(Player player)
	{
		_player = player;
	}

	private void _OnStoneClick(Stone.Pile pile, Stone card)
	{
		if (pile == Stone.Pile.Cancel)
		{
			_OnBackPressed();
		}
	}

	private void _OnBackPressed()
	{
		base.finished = true;
	}

	private void _OnMapClick(ProceduralMap.Pile pile, ProceduralMap card)
	{
		if (pile == ProceduralMap.Pile.Hidden)
		{
			_OnBackPressed();
			GameStepHideProceduralMap.ActiveStep?.ShowMap();
		}
	}

	protected override void OnFirstEnabled()
	{
		_traits = Pools.UseKeepItemList<Ability>();
		foreach (DataRef<AbilityData> item in from d in AbilityData.GetNonStandardAbilitiesByCategory(_class).value.Where((KeyValuePair<AbilityData.Category, PoolKeepItemListHandle<DataRef<AbilityData>>> p) => p.Key.IsTrait()).SelectMany((KeyValuePair<AbilityData.Category, PoolKeepItemListHandle<DataRef<AbilityData>>> p) => p.Value.value)
			orderby d.data.category, d.data.rank descending
			select d)
		{
			base.state.heroDeck.Add(_traits.value.AddReturn(new Ability(item)));
		}
		base.view.heroDeckLayout.SetLayout(HeroDeckPile.SelectionHand, base.view.heroDeckLayout.selectionHandUnrestricted);
		base.state.heroDeck.Transfer(_traits.value, HeroDeckPile.SelectionHand);
		using PoolKeepItemHashSetHandle<DataRef<AbilityData>> poolKeepItemHashSetHandle = _player.GetLockedLevelUpAbilities();
		foreach (Ability card in base.state.heroDeck.GetCards(HeroDeckPile.SelectionHand))
		{
			if (poolKeepItemHashSetHandle.Contains(card.dataRef))
			{
				card.abilityCard.SetHasUsesRemaining(hasUses: false);
			}
		}
	}

	protected override void OnEnable()
	{
		base.state.stoneDeck.layout.onPointerClick += _OnStoneClick;
		base.view.onBackPressed += _OnBackPressed;
		base.view.stoneDeckLayout.SetLayout(Stone.Pile.Cancel, base.view.stoneDeckLayout.cancelFloating);
		base.state.buttonDeck.Layout<ButtonDeckLayout>().Activate(ButtonCardType.Back);
		base.state.stoneDeck.Layout<StoneDeckLayout>()[StoneType.Cancel].view.RequestGlow(this, Colors.TARGET);
		base.view.LogMessage(MessageData.GameTooltips.ViewCharacterTraits.Localize());
		base.view.BlockRaycast(base.view.heroDeckLayout.selectionHandUnrestricted.transform);
		base.view.mapDeckLayout.onPointerClick += _OnMapClick;
		_layoutOffset = new LayoutOffset(_layoutsToOffset, base.view.heroDeckLayout.selectionHandUnrestricted.transform.GetPlane(PlaneAxes.XZ).InvertNormal().TranslateReturn(base.view.heroDeckLayout.selectionHandUnrestricted.transform.up * 0.125f));
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		base.state.stoneDeck.layout.onPointerClick -= _OnStoneClick;
		base.view.onBackPressed -= _OnBackPressed;
		base.state.buttonDeck.Layout<ButtonDeckLayout>().Deactivate(ButtonCardType.Back);
		base.view.stoneDeckLayout.RestoreLayoutToDefault(Stone.Pile.Cancel);
		base.view.ClearMessage();
		base.view.UnblockRaycast();
		base.view.mapDeckLayout.onPointerClick -= _OnMapClick;
	}

	protected override void OnDestroy()
	{
		if ((bool)base.view.deckCreation)
		{
			base.state.deckCreation.exile.Transfer(_traits.value, ExilePile.ClearGameState);
		}
		else
		{
			foreach (Ability item in _traits.value)
			{
				base.view.exileDeckLayout.TransferWithSpecialTransitions(item, ExilePile.ClearGameState);
				item.view.ClearEnterTransitions();
			}
		}
		base.view.heroDeckLayout.RestoreLayoutToDefault(HeroDeckPile.SelectionHand);
		_layoutOffset.ClearLayoutOffsets();
		Pools.Repool(_traits);
	}
}
