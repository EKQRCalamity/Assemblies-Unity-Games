using UnityEngine;

public class AbilityDeckView : ADeckView
{
	public static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/Decks/AbilityDeckView";

	public static readonly ResourceBlueprint<GameObject> CreationBlueprint = "GameState/Decks/AbilityDeckCreationView";

	public StringEvent onClassNameChange;

	private AbilityDeckViewCreation _creation;

	public AbilityDeck abilityDeck
	{
		get
		{
			return base.target as AbilityDeck;
		}
		set
		{
			base.target = value;
		}
	}

	public AbilityDeckViewCreation creation => this.CacheComponent(ref _creation);

	public static CardLayoutElement GetCreationDeck(AbilityDeck deck, Transform parent = null)
	{
		return Pools.Unpool(CreationBlueprint, parent).GetComponent<ATargetView>().SetData(deck);
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		base._OnTargetChange(oldTarget, newTarget);
		if (abilityDeck != null)
		{
			onMaterialChange?.Invoke(CharacterCardSkins.Default[abilityDeck.deckRef.data.characterClass].deck);
			onClassNameChange?.InvokeLocalized(this, () => abilityDeck?.deckRef.data.characterClass.GetText());
		}
	}
}
