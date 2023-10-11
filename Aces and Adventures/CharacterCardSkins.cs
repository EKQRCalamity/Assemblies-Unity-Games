using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Character/CharacterCardSkins")]
public class CharacterCardSkins : ScriptableObject
{
	[Serializable]
	public class CharacterCardSkin
	{
		public Material cardFront;

		public Material cardFrontRebirth1;

		public Material cardFrontRebirth2;

		public Material cardBack;

		public Material deck;

		public Material seal;

		public Material this[RebirthLevel rebirth]
		{
			get
			{
				Material material = rebirth switch
				{
					RebirthLevel.One => cardFrontRebirth1, 
					RebirthLevel.Two => cardFrontRebirth2, 
					_ => null, 
				};
				if ((object)material == null || !material)
				{
					return cardFront;
				}
				return material;
			}
		}
	}

	private static readonly ResourceBlueprint<CharacterCardSkins> _Default = "GameState/Character/CharacterCardSkins";

	public CharacterCardSkin warrior;

	public CharacterCardSkin rogue;

	public CharacterCardSkin mage;

	public CharacterCardSkin hunter;

	public CharacterCardSkin enchantress;

	public static CharacterCardSkins Default => _Default;

	public CharacterCardSkin this[PlayerClass c] => (CharacterCardSkin)((c switch
	{
		PlayerClass.Warrior => warrior, 
		PlayerClass.Rogue => rogue, 
		PlayerClass.Mage => mage, 
		PlayerClass.Enchantress => enchantress, 
		PlayerClass.Hunter => hunter, 
		_ => warrior, 
	}) ?? warrior);
}
