public class MapLevelLoaderChaliceTutorial : MapLevelLoader
{
	protected override void Activate(MapPlayerController player)
	{
		if (PlayerData.Data.Loadouts.GetPlayerLoadout(player.id).charm == Charm.charm_chalice)
		{
			base.Activate(player);
		}
		else
		{
			MapEventNotification.Current.ShowTooltipEvent(TooltipEvent.ChaliceTutorialEquipCharm);
		}
	}
}
