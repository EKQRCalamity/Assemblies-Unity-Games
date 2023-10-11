using ProtoBuf;

[ProtoContract]
public class ClassSeal : ATarget
{
	[ProtoMember(1)]
	private DataRef<CharacterData> _character;

	public DataRef<CharacterData> character => _character;

	public PlayerClass characterClass => _character.data.characterClass;

	private bool _characterSpecified => _character.ShouldSerialize();

	public ClassSeal(DataRef<CharacterData> character)
	{
		_character = character;
	}
}
