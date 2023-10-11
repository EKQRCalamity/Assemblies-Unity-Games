using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class CharacterProgress
{
	[ProtoMember(1)]
	private bool _unlocked;

	[ProtoMember(2, OverwriteList = true)]
	private Dictionary<uint, int> _bonuses;

	public bool unlocked
	{
		get
		{
			return _unlocked;
		}
		set
		{
			_unlocked = value;
		}
	}

	private Dictionary<uint, int> bonuses => _bonuses ?? (_bonuses = new Dictionary<uint, int>());

	public bool HasUnlockedBonus(DataRef<BonusCardData> bonus)
	{
		return bonuses.ContainsKey(bonus);
	}

	public void UnlockBonus(DataRef<BonusCardData> bonus)
	{
		bonuses[bonus] = bonuses.GetValueOrDefault(bonus) + 1;
	}

	public IEnumerable<DataRef<BonusCardData>> GetUnlockedBonuses()
	{
		foreach (uint key in bonuses.Keys)
		{
			DataRef<BonusCardData> dataRef = DataRef<BonusCardData>.FromFileId(key);
			if (dataRef != null)
			{
				yield return dataRef;
			}
		}
	}
}
