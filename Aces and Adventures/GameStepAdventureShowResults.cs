using System.Collections;
using System.Linq;

public class GameStepAdventureShowResults : GameStep
{
	private PoolKeepItemListHandle<BonusCard> _bonuses;

	private bool _adventureHasBeenBeatenPreviously;

	protected override void OnFirstEnabled()
	{
		if ((bool)base.state.game.data.victoryMusic)
		{
			AppendStep(new GameStepMusic(MusicPlayType.Play, base.state.game.data.victoryMusic, base.state.game.data.victoryMusicVolume));
		}
		if ((bool)base.state.game.data.victoryLighting)
		{
			AppendStep(GameStepLighting.Create(base.state.game.data.victoryLighting.data));
		}
		if ((bool)base.state.game.data.victoryAmbient)
		{
			AppendStep(new GameStepAmbient(MusicPlayType.Resume, base.state.game.data.victoryAmbient, base.state.game.data.victoryAmbientVolume));
		}
	}

	public override void Start()
	{
		_adventureHasBeenBeatenPreviously = ProfileManager.progress.games.read.HasCompleted(base.state.game, base.state.adventure);
		_bonuses = Pools.UseKeepItemList((from b in base.state.targets.Values<BonusCard>().AsEnumerable()
			where b.IsSuccessful()
			orderby b.isNew.ToInt() descending, b.experience
			select b).AsEnumerable());
		foreach (BonusCard item in _bonuses.value)
		{
			item.Unlock();
		}
		ProfileManager.progress.games.write.Add(base.state.game, base.state.adventure, base.state.player.characterClass, new AdventureCompletion.Data(base.state.totalTime, base.state.strategyTime));
	}

	protected override IEnumerator Update()
	{
		base.state.adventureDeck.TransferPile(AdventureCard.Pile.TurnOrder, AdventureCard.Pile.ActiveHand);
		base.state.player.view.ClearEnterTransitions();
		base.state.player.resourceDeck.TransferPile(ResourceCard.Pile.Hand, ResourceCard.Pile.DiscardPile);
		base.state.player.abilityDeck.TransferPile(Ability.Pile.Hand, Ability.Pile.Discard);
		yield return AppendStep(new GameStepWait(0.333f, null, canSkip: false));
		foreach (BonusCard item in _bonuses.value)
		{
			yield return AppendStep(new GameStepPresentBonus(item));
		}
		yield return AppendStep(new GameStepPresentResultCard(new AdventureResultCard(base.state)));
		TransitionTo(new GameStepAdventureConfirmResults(_adventureHasBeenBeatenPreviously && base.state.adventure.TrackedByAchievements()));
	}

	protected override void OnDestroy()
	{
		Pools.Repool(ref _bonuses);
	}
}
