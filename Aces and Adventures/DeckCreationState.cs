using System.Linq;

public class DeckCreationState
{
	private IdDeck<DeckCreationPile, Player> _characters;

	private IdDeck<DeckCreationPile, AbilityDeck> _decks;

	private IdDeck<DeckCreationPile, Ability> _abilities;

	private IdDeck<ExilePile, ATarget> _exile;

	public IdDeck<DeckCreationPile, Player> characters => _characters ?? (_characters = new IdDeck<DeckCreationPile, Player>());

	public IdDeck<DeckCreationPile, AbilityDeck> decks => _decks ?? (_decks = new IdDeck<DeckCreationPile, AbilityDeck>());

	public IdDeck<DeckCreationPile, Ability> abilities => _abilities ?? (_abilities = new IdDeck<DeckCreationPile, Ability>());

	public IdDeck<ExilePile, ATarget> exile => _exile ?? (_exile = new IdDeck<ExilePile, ATarget>());

	public Player selectedCharacter => characters.GetCards(DeckCreationPile.List).FirstOrDefault();

	public AbilityDeck selectedDeck => decks.GetCards(DeckCreationPile.List).FirstOrDefault();
}
