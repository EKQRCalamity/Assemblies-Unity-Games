using System;

public class DecksLayout : ADeckLayout<DeckPile, ADeck>
{
	public ACardLayout inactiveSelectAdventure;

	public ACardLayout inactiveSelectAdventureLarge;

	public ACardLayout inactiveSelectAbility;

	public ACardLayout select;

	public ACardLayout selectLarge;

	public ACardLayout adventure;

	public ACardLayout ability;

	public ACardLayout adventureOpen;

	public ACardLayout abilityOpen;

	public ACardLayout exile;

	protected override ACardLayout this[DeckPile? pile]
	{
		get
		{
			return pile switch
			{
				DeckPile.InactiveSelectAdventure => inactiveSelectAdventure, 
				DeckPile.InactiveSelectAbility => inactiveSelectAbility, 
				DeckPile.Select => select, 
				DeckPile.SelectLarge => selectLarge, 
				DeckPile.Adventure => adventure, 
				DeckPile.Ability => ability, 
				DeckPile.Exile => exile, 
				DeckPile.AdventureOpen => adventureOpen, 
				DeckPile.AbilityOpen => abilityOpen, 
				_ => null, 
			};
		}
		set
		{
			switch (pile)
			{
			case DeckPile.InactiveSelectAdventure:
				inactiveSelectAdventure = value;
				break;
			case DeckPile.InactiveSelectAbility:
				inactiveSelectAbility = value;
				break;
			case DeckPile.Select:
				select = value;
				break;
			case DeckPile.SelectLarge:
				selectLarge = value;
				break;
			case DeckPile.Adventure:
				adventure = value;
				break;
			case DeckPile.Ability:
				ability = value;
				break;
			case DeckPile.AdventureOpen:
				adventureOpen = value;
				break;
			case DeckPile.AbilityOpen:
				abilityOpen = value;
				break;
			case DeckPile.Exile:
				exile = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("pile", pile, null);
			}
		}
	}

	protected override CardLayoutElement _CreateView(ADeck value)
	{
		return ADeckView.Create(value);
	}
}
