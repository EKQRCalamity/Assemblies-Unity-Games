using ProtoBuf;

[ProtoContract]
public class StoryCard : AdventureTarget
{
	public override bool canBePooled => true;
}
