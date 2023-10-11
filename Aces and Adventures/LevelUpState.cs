using System.Collections.Generic;

public class LevelUpState
{
	private IdDeck<LevelUpPile, ATarget> _main;

	public IdDeck<LevelUpPile, ATarget> main => _main ?? (_main = new IdDeck<LevelUpPile, ATarget>());

	public Experience readExperience => ProfileManager.progress.experience.read;

	public Experience writeExperience => ProfileManager.progress.experience.write;

	public DataRef<CharacterData> selectedCharacter
	{
		get
		{
			return ProfileManager.prefs.selectedCharacter;
		}
		set
		{
			ProfileManager.prefs.selectedCharacter = value;
		}
	}

	public PlayerClass selectedClass => selectedCharacter.data.characterClass;

	public IEnumerable<DataRef<CharacterData>> unlockedCharacters => ProfileManager.progress.characters.read.GetUnlockedCharacters(DevData.Unlocks.characters);

	public bool enabled => readExperience.enabled;

	public LevelUpState()
	{
		_Initialize();
	}

	private bool _LockedForDemo(DataRef<CharacterData> characterDataRef)
	{
		if (IOUtil.IsDemo)
		{
			return !characterDataRef.data.unlockedForDemo;
		}
		return false;
	}

	private void _Initialize()
	{
		if (!enabled)
		{
			return;
		}
		main.Add(new ExperienceVial(readExperience.currentVialXP), LevelUpPile.Vial);
		main.Add(new ClassSeal(selectedCharacter), LevelUpPile.ActiveSeal);
		foreach (DataRef<CharacterData> unlockedCharacter in unlockedCharacters)
		{
			if (!ContentRef.Equal(selectedCharacter, unlockedCharacter) && !_LockedForDemo(unlockedCharacter))
			{
				main.Add(new ClassSeal(unlockedCharacter), LevelUpPile.Seals);
			}
		}
		main.Add(new LevelUpPlant(selectedCharacter), LevelUpPile.Pot);
	}

	public IEnumerable<ATarget> CreateLevelUps(DataRef<CharacterData> character)
	{
		yield return main.Add(new Ability(AbilityData.GetAbility(character.data.characterClass, AbilityData.Category.Level1Trait, AbilityData.Rank.Normal)), LevelUpPile.LevelUpsTransition);
		yield return main.Add(new Ability(AbilityData.GetAbility(character.data.characterClass, AbilityData.Category.Level2Trait, AbilityData.Rank.Normal)), LevelUpPile.LevelUpsTransition);
		yield return main.Add(new Ability(AbilityData.GetAbility(character.data.characterClass, AbilityData.Category.Level3Trait, AbilityData.Rank.Normal)), LevelUpPile.LevelUpsTransition);
		foreach (ATarget levelUpCard in character.data.GetLevelUpCards(ProfileManager.progress.experience.read.GetLevel(character)))
		{
			yield return main.Add(levelUpCard, LevelUpPile.LevelUpsTransition);
		}
	}
}
