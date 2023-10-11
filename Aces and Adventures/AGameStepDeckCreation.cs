using System.Collections;

public abstract class AGameStepDeckCreation : GameStep
{
	public DeckCreationState creation => base.state.deckCreation;

	public IdDeck<DeckCreationPile, Player> characters => creation.characters;

	public IdDeck<DeckCreationPile, Ability> abilities => creation.abilities;

	public IdDeck<DeckCreationPile, AbilityDeck> decks => creation.decks;

	public IdDeck<ExilePile, ATarget> exile => creation.exile;

	public Player selectedCharacter => creation.selectedCharacter;

	public AbilityDeck selectedDeck => creation.selectedDeck;

	public DeckCreationStateView creationView => base.state.view.deckCreation;

	public DeckCreationCharacterDeckLayout characterLayout => creationView.characters;

	public DeckCreationDeckDeckLayout deckLayout => creationView.decks;

	public DeckCreationAbilityDeckLayout abilityLayout => creationView.abilities;

	public ExileDeckLayout exileLayout => creationView.exile;

	public int pageSize => abilityLayout.GetLayout(DeckCreationPile.Results).maxCount;

	public int pageNumber
	{
		get
		{
			return creationView.pageNumber;
		}
		set
		{
			creationView.pageNumber = value;
		}
	}

	public int pageIndex => pageNumber - 1;

	protected virtual void _OnDonePressed()
	{
		base.finished = true;
	}

	protected virtual void _OnBackPressed()
	{
		_OnDonePressed();
	}

	protected override void OnEnable()
	{
		creationView.onDonePressed += _OnDonePressed;
		base.view.onBackPressed += _OnBackPressed;
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
		creationView.onDonePressed -= _OnDonePressed;
		base.view.onBackPressed -= _OnBackPressed;
	}
}
