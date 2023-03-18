using Framework.Inventory;
using Framework.Managers;
using UnityEngine;

public class ItemTester : MonoBehaviour
{
	public string AddPrayer;

	public string AddRelic;

	public string RemovePrayer;

	public string RemoveRelic;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Penitent"))
		{
			Core.Logic.Penitent.Stats.Fervour.SetToCurrentMax();
			Prayer prayer = Core.InventoryManager.GetPrayer(AddPrayer);
			Core.InventoryManager.SetPrayerInSlot(0, prayer);
			Core.InventoryManager.RemovePrayer(RemovePrayer);
			if (prayer != null)
			{
				prayer.AddDecipher(prayer.decipherMax);
			}
			Relic relic = Core.InventoryManager.GetRelic(AddRelic);
			Core.InventoryManager.SetRelicInSlot(0, relic);
			Core.InventoryManager.RemoveRelic(RemoveRelic);
		}
	}
}
