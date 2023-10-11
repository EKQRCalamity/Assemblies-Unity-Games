using ProtoBuf;

[ProtoContract]
public enum Orient8 : byte
{
	Right,
	RightUp,
	Up,
	LeftUp,
	Left,
	LeftDown,
	Down,
	RightDown
}
