using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Ability/AbilityCardSkins")]
public class AbilityCardSkins : ScriptableObject
{
	[Serializable]
	public class AbilityCardSkin
	{
		[Serializable]
		public class Fronts
		{
			public Material normal;

			public Material elite;

			public Material legendary;

			public Material this[AbilityData.Rank rank] => rank switch
			{
				AbilityData.Rank.Elite => elite, 
				AbilityData.Rank.Legendary => legendary, 
				_ => normal, 
			};
		}

		public Fronts cardFronts;

		public Material cardBack;
	}

	private static readonly ResourceBlueprint<AbilityCardSkins> _Default = "GameState/Ability/AbilityCardSkins";

	public AbilityCardSkin warrior;

	public AbilityCardSkin rogue;

	public AbilityCardSkin mage;

	public AbilityCardSkin hunter;

	public AbilityCardSkin enchantress;

	public AbilityCardSkin classless;

	public AbilityCardSkin equipment;

	public AbilityCardSkin condition;

	public static AbilityCardSkins Default => _Default;

	public AbilityCardSkin this[PlayerClass? c] => (AbilityCardSkin)((c switch
	{
		PlayerClass.Warrior => warrior, 
		PlayerClass.Rogue => rogue, 
		PlayerClass.Mage => mage, 
		PlayerClass.Enchantress => enchantress, 
		PlayerClass.Hunter => hunter, 
		_ => classless, 
	}) ?? warrior);

	public AbilityCardSkin this[ItemCardType? t] => t switch
	{
		ItemCardType.Item => equipment, 
		ItemCardType.Encounter => condition, 
		ItemCardType.Condition => condition, 
		_ => null, 
	};
}
