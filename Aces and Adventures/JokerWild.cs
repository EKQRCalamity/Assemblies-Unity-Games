using ProtoBuf;

[ProtoContract]
public class JokerWild : AWild
{
	protected override void _Process(ref PlayingCardTypes output)
	{
		output |= EnumUtil<PlayingCardTypes>.AllFlags;
	}

	public override bool Equals(AWild other)
	{
		return other is JokerWild;
	}

	public override AWild CombineWith(AWild other)
	{
		return this;
	}
}
