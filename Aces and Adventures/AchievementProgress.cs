using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class AchievementProgress
{
	[ProtoMember(1, OverwriteList = true)]
	private Dictionary<uint, UInt2> _progress;

	private Dictionary<uint, UInt2> progress => _progress ?? (_progress = new Dictionary<uint, UInt2>());

	private UInt2 this[uint achievement] => progress.GetValueOrDefault(achievement);

	public bool IsComplete(uint achievement)
	{
		UInt2 uInt = this[achievement];
		if (uInt.x >= uInt.y)
		{
			return uInt.y != 0;
		}
		return false;
	}

	public uint GetProgress(uint achievement)
	{
		return this[achievement].x;
	}

	public void SetProgress(uint achievement, UInt2 p)
	{
		progress[achievement] = p;
	}
}
