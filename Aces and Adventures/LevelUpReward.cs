using ProtoBuf;

[ProtoContract]
public class LevelUpReward : ATarget
{
	[ProtoMember(1)]
	private LevelUpData _data;

	public LevelUpData data => _data;

	public LevelUpReward()
	{
	}

	public LevelUpReward(LevelUpData data)
	{
		_data = data;
	}
}
