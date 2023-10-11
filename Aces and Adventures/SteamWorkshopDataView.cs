using UnityEngine;

public class SteamWorkshopDataView : SteamWorkshopItemView
{
	public static readonly ResourceBlueprint<GameObject> Blueprint = "UI/Content/SteamWorkshopDataView";

	public StringEvent onNameChange;

	public StringEvent onCreatorChange;

	public ColorEvent onCreatorColorChange;

	protected override void _OnResultSetUnique()
	{
		onNameChange.Invoke(base.result.name);
		ContentCreatorType contentCreatorType = ((base.result.details.m_ulSteamIDOwner == Steam.SteamId.m_SteamID) ? ContentCreatorType.Yours : ContentCreatorType.Others);
		onCreatorChange.Invoke(EnumUtil.FriendlyName(contentCreatorType));
		onCreatorColorChange.Invoke(contentCreatorType.GetTint());
	}
}
