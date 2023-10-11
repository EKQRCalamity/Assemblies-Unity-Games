using System;
using System.Linq;
using UnityEngine.Localization.Tables;

[Serializable]
public class GenerateAbilityDeckNameEntryMeta : GenerateEntryMeta
{
	public static string GetKey(string name)
	{
		return "Generated/AbilityDeckName/" + name;
	}

	protected override void _GenerateEntries(StringTable table)
	{
		foreach (string item in (from dRef in DataRef<AbilityDeckData>.Search()
			select dRef.data into d
			where d.unlockedByDefault
			select d.name into d
			where d.HasVisibleCharacter()
			select d).Distinct())
		{
			table.AddEntry(GetKey(item), item);
		}
	}
}
