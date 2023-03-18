using Framework.Managers;

namespace Framework.Inventory;

public class CollectibleItem : BaseInventoryObject
{
	public bool UsePercentageCompletition = true;

	public bool ClaimedInOssuary
	{
		get
		{
			string text = $"CLAIMED_{id}";
			return Core.Events.GetFlag(text);
		}
		set
		{
			string text = $"CLAIMED_{id}";
			Core.Events.SetFlag(text, value);
		}
	}

	public override bool AskForPercentageCompletition()
	{
		return true;
	}

	public override bool AddPercentageCompletition()
	{
		return UsePercentageCompletition;
	}

	public override InventoryManager.ItemType GetItemType()
	{
		return InventoryManager.ItemType.Collectible;
	}

	public override bool HasLore()
	{
		return false;
	}
}
