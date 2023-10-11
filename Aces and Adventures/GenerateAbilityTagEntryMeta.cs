using System;
using System.Linq;
using UnityEngine.Localization.Tables;

[Serializable]
public class GenerateAbilityTagEntryMeta : GenerateEntryMeta
{
	public static string GetKey(string tags)
	{
		return "Generated/AbilityTag/" + tags;
	}

	protected override void _GenerateEntries(StringTable table)
	{
		foreach (string item in (from dRef in DataRef<AbilityData>.Search()
			select dRef.data.GetDisplayedTagString() into s
			where s.HasVisibleCharacter()
			select s).Distinct())
		{
			table.AddEntry(GetKey(item), item);
		}
		GameState state = GameState.GetTempState();
		foreach (string item2 in (from i in DataRef<GameData>.Search().SelectMany((DataRef<GameData> g) => g.data.adventures).SelectMany((DataRef<AdventureData> a) => a.data.cards)
				.OfType<AdventureCard.Item>()
				.SelectMany((AdventureCard.Item i) => i.GenerateCards(state))
				.OfType<ItemCard>()
			select i.GetDisplayedTagString() into s
			where s.HasVisibleCharacter()
			select s).Distinct())
		{
			table.AddEntry(GetKey(item2), item2);
		}
		state.Destroy();
	}
}
