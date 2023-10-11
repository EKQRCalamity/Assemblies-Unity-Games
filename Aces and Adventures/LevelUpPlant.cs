using ProtoBuf;

[ProtoContract]
public class LevelUpPlant : ATarget
{
	[ProtoMember(1)]
	private DataRef<CharacterData> _character;

	[ProtoMember(2)]
	private IdDeck<LevelUpLeafPile, LevelUpLeaf> _leafs;

	public DataRef<CharacterData> character => _character;

	public int level => ProfileManager.progress.experience.read.GetLevelWithRebirth(character);

	public int rebirth => ProfileManager.progress.experience.read.GetRebirth(character);

	public IdDeck<LevelUpLeafPile, LevelUpLeaf> leafs => _leafs ?? (_leafs = new IdDeck<LevelUpLeafPile, LevelUpLeaf>());

	public LevelUpPlantView plantView => base.view as LevelUpPlantView;

	private LevelUpPlant()
	{
	}

	public LevelUpPlant(DataRef<CharacterData> character)
	{
		_character = character;
		int num = level;
		for (int i = 0; i < num; i++)
		{
			leafs.Add(new LevelUpLeaf());
		}
	}
}
