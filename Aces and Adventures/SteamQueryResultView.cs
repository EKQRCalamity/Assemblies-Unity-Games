using UnityEngine;

public class SteamQueryResultView : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/SteamQueryResultView";

	public StringEvent onLabelChange;

	public RectTransform votesContainer;

	public static SteamQueryResultView Create(Steam.Ugc.Query.Result result, string label, Transform parent, Steam.Ugc.ItemType itemType = Steam.Ugc.ItemType.Standard)
	{
		return DirtyPools.Unpool(_Blueprint, parent).GetComponent<SteamQueryResultView>()._SetData(result, label, itemType);
	}

	private SteamQueryResultView _SetData(Steam.Ugc.Query.Result result, string label, Steam.Ugc.ItemType itemType)
	{
		SteamWorkshopVotesView.Create(result, votesContainer, itemType);
		_SetLabel(result, label, itemType);
		return this;
	}

	private async void _SetLabel(Steam.Ugc.Query.Result result, string label, Steam.Ugc.ItemType itemType)
	{
		StringEvent stringEvent = onLabelChange;
		string text = (label.IsNullOrEmpty() ? "" : (label + ": "));
		string text2 = ((itemType != 0) ? (await Steam.Friends.GetPersonaNameAsync(result.ownerId)) : result.name);
		stringEvent.Invoke(text + text2);
	}
}
