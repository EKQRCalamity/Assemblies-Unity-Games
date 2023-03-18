using System.Collections.Generic;
using System.Collections.ObjectModel;
using Framework.FrameworkCore.Attributes;
using Framework.Inventory;
using Framework.Managers;

namespace Gameplay.UI.Console;

public class InventoryCommand : ConsoleCommand
{
	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		switch (command)
		{
		case "relic":
			ParseRelic(subcommand, listParameters);
			break;
		case "questitem":
			ParseQuest(subcommand, listParameters);
			break;
		case "collectible":
			ParseCollectible(subcommand, listParameters);
			break;
		case "bead":
			ParseBead(subcommand, listParameters);
			break;
		case "prayer":
			ParsePrayer(subcommand, listParameters);
			break;
		case "sword":
			ParseSword(subcommand, listParameters);
			break;
		case "key":
			ParseKey(subcommand, listParameters);
			break;
		case "invtest":
			base.Console.Write("Adding all items:");
			Core.InventoryManager.TestAddAllObjects();
			break;
		}
	}

	public override List<string> GetNames()
	{
		List<string> list = new List<string>();
		list.Add("relic");
		list.Add("questitem");
		list.Add("bead");
		list.Add("prayer");
		list.Add("collectible");
		list.Add("invtest");
		list.Add("sword");
		list.Add("key");
		return list;
	}

	private void ParseRelic(string command, List<string> paramList)
	{
		string command2 = "relic " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available RELIC commands:");
				base.Console.Write("relic list: List all relics");
				base.Console.Write("relic listowned: List all relics owned by player");
				base.Console.Write("relic add [IDRELIC|all]: Add a relic (or all)");
				base.Console.Write("relic remove [IDRELIC|all]: Remove the relic (or all)");
				base.Console.Write("relic equiped: Show the eqquiped relics");
				base.Console.Write("relic equip IDRELIC SLOT: Equip the relic in the slot");
				base.Console.Write("relic unequip SLOT: Unequip the relic in the slot");
			}
			break;
		case "list":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("All relics:", Core.InventoryManager.GetAllRelics());
			}
			break;
		case "listowned":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("Owned relics:", Core.InventoryManager.GetRelicsOwned());
			}
			break;
		case "add":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.AddAll(InventoryManager.ItemType.Relic);
					base.Console.Write("Adding all relics");
				}
				else if (CheckInvObject("relic", paramList[0], Core.InventoryManager.GetRelic(paramList[0])))
				{
					WriteCommandResult("Add relic", Core.InventoryManager.AddRelic(paramList[0]));
				}
			}
			break;
		case "remove":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.RemoveAll(InventoryManager.ItemType.Relic);
					base.Console.Write("Removing all relics");
				}
				else if (CheckInvObject("relic", paramList[0], Core.InventoryManager.GetRelic(paramList[0])))
				{
					WriteCommandResult("Remove relic", Core.InventoryManager.RemoveRelic(paramList[0]));
				}
			}
			break;
		case "equiped":
		{
			base.Console.Write("Relics slots");
			for (int i = 0; i < 3; i++)
			{
				Relic relicInSlot = Core.InventoryManager.GetRelicInSlot(i);
				string text = "Slot " + i + ": ";
				text = ((!relicInSlot) ? (text + "empty") : (text + relicInSlot.id + "  - " + relicInSlot.caption));
				base.Console.Write(text);
			}
			break;
		}
		case "equip":
		{
			if (ValidateParams(command2, 2, paramList) && CheckInvObject("relic", paramList[0], Core.InventoryManager.GetRelic(paramList[0])) && ValidateParam(paramList[1], out var resultValue2, 0, 2))
			{
				WriteCommandResult("Equip relic", Core.InventoryManager.SetRelicInSlot(resultValue2, paramList[0]));
			}
			break;
		}
		case "unequip":
		{
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out var resultValue, 0, 2))
			{
				Relic relic = null;
				WriteCommandResult("Unrquip relic", Core.InventoryManager.SetRelicInSlot(resultValue, relic));
			}
			break;
		}
		default:
			base.Console.Write("Command unknow, use relic help");
			break;
		}
	}

	private void ParseQuest(string command, List<string> paramList)
	{
		string command2 = "questitem " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available QUEST ITEM commands:");
				base.Console.Write("questitem list: List all quest items");
				base.Console.Write("questitem listowned: List all quest items owned by player");
				base.Console.Write("questitem add [IDQUESTITEM|all]: Add a quest item (or all)");
				base.Console.Write("questitem remove [IDQUESTITEM|all]: Remove the quest item (or all)");
			}
			break;
		case "list":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("All quest items:", Core.InventoryManager.GetAllQuestItems());
			}
			break;
		case "listowned":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("Owned quest items:", Core.InventoryManager.GetQuestItemOwned());
			}
			break;
		case "add":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.AddAll(InventoryManager.ItemType.Quest);
					base.Console.Write("Adding all quest items");
				}
				else if (CheckInvObject("quest item", paramList[0], Core.InventoryManager.GetQuestItem(paramList[0])))
				{
					WriteCommandResult("Add quest item", Core.InventoryManager.AddQuestItem(paramList[0]));
				}
			}
			break;
		case "remove":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.RemoveAll(InventoryManager.ItemType.Quest);
					base.Console.Write("Removing all quest items");
				}
				else if (CheckInvObject("quest item", paramList[0], Core.InventoryManager.GetQuestItem(paramList[0])))
				{
					WriteCommandResult("Remove quest item", Core.InventoryManager.RemoveQuestItem(paramList[0]));
				}
			}
			break;
		default:
			base.Console.Write("Command unknow, use questitem help");
			break;
		}
	}

	private void ParseCollectible(string command, List<string> paramList)
	{
		string command2 = "collectible " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available COLLECTIBLE commands:");
				base.Console.Write("collectible list: List all collectible items");
				base.Console.Write("collectible listowned: List all collectible items owned by player");
				base.Console.Write("collectible add [IDCOLLECTIBLEITEM|all]: Add a collectible item (or all)");
				base.Console.Write("collectible remove [IDCOLLECTIBLEITEM|all]: Remove the collectible item (or all)");
			}
			break;
		case "list":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("All collectible items:", Core.InventoryManager.GetAllCollectibleItems());
			}
			break;
		case "listowned":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("Owned collectible items:", Core.InventoryManager.GetCollectibleItemOwned());
			}
			break;
		case "add":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.AddAll(InventoryManager.ItemType.Collectible);
					base.Console.Write("Adding all collectibles");
				}
				else if (CheckInvObject("collectible item", paramList[0], Core.InventoryManager.GetCollectibleItem(paramList[0])))
				{
					WriteCommandResult("Add collectible item", Core.InventoryManager.AddCollectibleItem(paramList[0]));
				}
			}
			break;
		case "remove":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.RemoveAll(InventoryManager.ItemType.Collectible);
					base.Console.Write("Removing all collectibles");
				}
				else if (CheckInvObject("collectible item", paramList[0], Core.InventoryManager.GetCollectibleItem(paramList[0])))
				{
					WriteCommandResult("Remove collectible item", Core.InventoryManager.RemoveCollectibleItem(paramList[0]));
				}
			}
			break;
		default:
			base.Console.Write("Command unknow, use collectible help");
			break;
		}
	}

	private void ParseKey(string command, List<string> paramList)
	{
		string command2 = "key " + command;
		int resultValue = 0;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available KEY commands:");
				base.Console.Write("key list: List all key status");
				base.Console.Write("key add [idx|all]: Add the idx key (or all)");
				base.Console.Write("key remove [idx|all]: Remove the idx key (or all)");
			}
			break;
		case "list":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Keys:");
				for (int j = 0; j < 4; j++)
				{
					string message = "Slot " + j + ": " + Core.InventoryManager.CheckBossKey(j);
					base.Console.Write(message);
				}
			}
			break;
		case "add":
			if (!ValidateParams(command2, 1, paramList))
			{
				break;
			}
			if (paramList[0].ToLower() == "all")
			{
				for (int k = 0; k < 4; k++)
				{
					Core.InventoryManager.AddBossKey(k);
				}
				base.Console.Write("Adding all Boss Keys");
			}
			else if (ValidateParam(paramList[0], out resultValue, 0, 3))
			{
				Core.InventoryManager.AddBossKey(resultValue);
				base.Console.Write("BossKey added");
			}
			break;
		case "remove":
			if (!ValidateParams(command2, 1, paramList))
			{
				break;
			}
			if (paramList[0].ToLower() == "all")
			{
				for (int i = 0; i < 4; i++)
				{
					Core.InventoryManager.RemoveBossKey(i);
				}
				base.Console.Write("Removing all Boss Keys");
			}
			else if (ValidateParam(paramList[0], out resultValue, 0, 3))
			{
				Core.InventoryManager.RemoveBossKey(resultValue);
				base.Console.Write("BossKey removed");
			}
			break;
		default:
			base.Console.Write("Command unknow, use key help");
			break;
		}
	}

	private void ParseBead(string command, List<string> paramList)
	{
		string command2 = "bead " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available BEADS commands:");
				base.Console.Write("bead list: List all beads");
				base.Console.Write("bead listowned: List all beads owned by player");
				base.Console.Write("bead setslots SLOTS: Sets the beads slots");
				base.Console.Write("bead add [IDBEAD|all]: Add a bead (or all)");
				base.Console.Write("bead remove [IDBEAD|all]: Remove the bead (or all)");
				base.Console.Write("bead equiped: List the eqquiped beads");
				base.Console.Write("bead equip IDBEAD SLOT: Equip the bead in the slot");
				base.Console.Write("bead unequip SLOT: Unequip the bead in the slot");
			}
			break;
		case "list":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("All beads:", Core.InventoryManager.GetAllRosaryBeads());
			}
			break;
		case "listowned":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("Owned beads:", Core.InventoryManager.GetRosaryBeadOwned());
			}
			break;
		case "add":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.AddAll(InventoryManager.ItemType.Bead);
					base.Console.Write("Adding all rosary beads");
				}
				else if (CheckInvObject("rosary bead", paramList[0], Core.InventoryManager.GetRosaryBead(paramList[0])))
				{
					WriteCommandResult(command2, Core.InventoryManager.AddRosaryBead(paramList[0]));
				}
			}
			break;
		case "setslots":
		{
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out var resultValue3, 2, 7))
			{
				BeadSlots beadSlots = base.Penitent.Stats.BeadSlots;
				beadSlots.SetPermanentBonus(resultValue3);
				WriteCommandResult(command2, result: true);
			}
			break;
		}
		case "remove":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.RemoveAll(InventoryManager.ItemType.Bead);
					base.Console.Write("Removing all rosary beads");
				}
				else if (CheckInvObject("rosary bead", paramList[0], Core.InventoryManager.GetRosaryBead(paramList[0])))
				{
					WriteCommandResult(command2, Core.InventoryManager.RemoveRosaryBead(paramList[0]));
				}
			}
			break;
		case "equiped":
		{
			base.Console.Write("Rosary Beads slots");
			for (int i = 0; i < Core.InventoryManager.GetRosaryBeadSlots(); i++)
			{
				RosaryBead rosaryBeadInSlot = Core.InventoryManager.GetRosaryBeadInSlot(i);
				string text = "Slot " + i + ": ";
				text = ((!rosaryBeadInSlot) ? (text + "empty") : (text + rosaryBeadInSlot.id + "  - " + rosaryBeadInSlot.caption));
				base.Console.Write(text);
			}
			break;
		}
		case "equip":
		{
			if (ValidateParams(command2, 2, paramList) && CheckInvObject("rosary bead", paramList[0], Core.InventoryManager.GetRosaryBead(paramList[0])) && ValidateParam(paramList[1], out var resultValue2, 0, Core.InventoryManager.GetRosaryBeadSlots() - 1))
			{
				WriteCommandResult(command2, Core.InventoryManager.SetRosaryBeadInSlot(resultValue2, paramList[0]));
			}
			break;
		}
		case "unequip":
		{
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out var resultValue, 0, Core.InventoryManager.GetRosaryBeadSlots() - 1))
			{
				RosaryBead bead = null;
				WriteCommandResult(command2, Core.InventoryManager.SetRosaryBeadInSlot(resultValue, bead));
			}
			break;
		}
		default:
			base.Console.Write("Command unknow, use bead help");
			break;
		}
	}

	private void ParseSword(string command, List<string> paramList)
	{
		string command2 = "sword " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available SWORD commands:");
				base.Console.Write("sword list: List all sword");
				base.Console.Write("sword listowned: List all sword owned by player");
				base.Console.Write("sword add [IDSWORD|all]: Add a sword (or all)");
				base.Console.Write("sword remove [IDSWORD|all]: Remove the sword (or all)");
				base.Console.Write("sword equiped: Show the eqquiped swords");
				base.Console.Write("sword equip IDSWORD SLOT: Equip the sword in the slot");
				base.Console.Write("sword unequip SLOT: Unequip the sword in the slot");
			}
			break;
		case "list":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("All swords:", Core.InventoryManager.GetAllSwords());
			}
			break;
		case "listowned":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("Owned swords:", Core.InventoryManager.GetSwordsOwned());
			}
			break;
		case "add":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.AddAll(InventoryManager.ItemType.Sword);
					base.Console.Write("Adding all swords");
				}
				else if (CheckInvObject("sword", paramList[0], Core.InventoryManager.GetSword(paramList[0])))
				{
					WriteCommandResult("Add sword", Core.InventoryManager.AddSword(paramList[0]));
				}
			}
			break;
		case "remove":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.RemoveAll(InventoryManager.ItemType.Sword);
					base.Console.Write("Removing all swords");
				}
				else if (CheckInvObject("sword", paramList[0], Core.InventoryManager.GetSword(paramList[0])))
				{
					WriteCommandResult("Remove sword", Core.InventoryManager.RemoveSword(paramList[0]));
				}
			}
			break;
		case "equiped":
		{
			base.Console.Write("Sword slots");
			for (int i = 0; i < 1; i++)
			{
				Sword swordInSlot = Core.InventoryManager.GetSwordInSlot(i);
				string text = "Slot " + i + ": ";
				text = ((!swordInSlot) ? (text + "empty") : (text + swordInSlot.id + "  - " + swordInSlot.caption));
				base.Console.Write(text);
			}
			break;
		}
		case "equip":
		{
			if (ValidateParams(command2, 2, paramList) && CheckInvObject("sword", paramList[0], Core.InventoryManager.GetSword(paramList[0])) && ValidateParam(paramList[1], out var resultValue2, 0, 0))
			{
				WriteCommandResult("Equip sword", Core.InventoryManager.SetSwordInSlot(resultValue2, paramList[0]));
			}
			break;
		}
		case "unequip":
		{
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out var resultValue, 0, 0))
			{
				Sword sword = null;
				WriteCommandResult("Unrquip sword", Core.InventoryManager.SetSwordInSlot(resultValue, sword));
			}
			break;
		}
		default:
			base.Console.Write("Command unknow, use relic help");
			break;
		}
	}

	private void ParsePrayer(string command, List<string> paramList)
	{
		string command2 = "prayer " + command;
		switch (command)
		{
		case "help":
			if (ValidateParams(command2, 0, paramList))
			{
				base.Console.Write("Available PRAYER commands:");
				base.Console.Write("prayer list: List all prayers");
				base.Console.Write("prayer listowned: List all prayers owned by player");
				base.Console.Write("prayer add [IDPRAYER|all]: Add a prayer (or all)");
				base.Console.Write("prayer remove [IDPRAYER|all]: Remove the prayer (or all)");
				base.Console.Write("prayer equiped: List the eqquiped prayers");
				base.Console.Write("prayer equip IDPRAYER SLOT: Equip the prayer");
				base.Console.Write("prayer unequip SLOT: Unequip the prayer type");
				base.Console.Write("prayer decipher IDPRAYER NUMBER: Add decipher");
			}
			break;
		case "list":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("All prayers:", Core.InventoryManager.GetAllPrayers());
			}
			break;
		case "listowned":
			if (ValidateParams(command2, 0, paramList))
			{
				ListInventoryList("Owned prayers:", Core.InventoryManager.GetPrayersOwned());
			}
			break;
		case "add":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.AddAll(InventoryManager.ItemType.Prayer);
					base.Console.Write("Adding all prayers");
				}
				else if (CheckInvObject("prayer", paramList[0], Core.InventoryManager.GetPrayer(paramList[0])))
				{
					WriteCommandResult("Add prayer", Core.InventoryManager.AddPrayer(paramList[0]));
				}
			}
			break;
		case "remove":
			if (ValidateParams(command2, 1, paramList))
			{
				if (paramList[0].ToLower() == "all")
				{
					Core.InventoryManager.RemoveAll(InventoryManager.ItemType.Prayer);
					base.Console.Write("Removing all prayers");
				}
				else if (CheckInvObject("prayer", paramList[0], Core.InventoryManager.GetPrayer(paramList[0])))
				{
					WriteCommandResult("Remove prayer", Core.InventoryManager.RemovePrayer(paramList[0]));
				}
			}
			break;
		case "equiped":
		{
			base.Console.Write("Prayer slots");
			for (int i = 0; i < 1; i++)
			{
				Prayer prayerInSlot = Core.InventoryManager.GetPrayerInSlot(i);
				string text = "Slot " + i + ": ";
				text = ((!prayerInSlot) ? (text + "empty") : (text + prayerInSlot.id + "  - " + prayerInSlot.caption));
				base.Console.Write(text);
			}
			break;
		}
		case "equip":
		{
			if (ValidateParams(command2, 2, paramList) && CheckInvObject("prayer", paramList[0], Core.InventoryManager.GetPrayer(paramList[0])) && ValidateParam(paramList[1], out var _, 0, 0))
			{
				Prayer prayer2 = Core.InventoryManager.GetPrayer(paramList[0]);
				Core.InventoryManager.AddPrayer(paramList[0]);
				if (!(prayer2 == null))
				{
					prayer2.AddDecipher(prayer2.decipherMax);
					Core.Logic.Penitent.Stats.Fervour.SetToCurrentMax();
					bool result = Core.InventoryManager.SetPrayerInSlot(0, prayer2);
					WriteCommandResult(command2, result);
				}
			}
			break;
		}
		case "unequip":
		{
			if (ValidateParams(command2, 1, paramList) && ValidateParam(paramList[0], out var resultValue3, 0, 0))
			{
				Prayer prayer3 = null;
				WriteCommandResult(command2, Core.InventoryManager.SetPrayerInSlot(resultValue3, prayer3));
			}
			break;
		}
		case "decipher":
		{
			if (!ValidateParams(command2, 2, paramList))
			{
				break;
			}
			Prayer prayer = Core.InventoryManager.GetPrayer(paramList[0]);
			if (CheckInvObject("prayer", paramList[0], prayer) && ValidateParam(paramList[1], out var resultValue, 1, 20))
			{
				bool flag = !prayer.IsDeciphered();
				if (flag)
				{
					prayer.AddDecipher(resultValue);
				}
				WriteCommandResult("Decipher prayer", flag);
			}
			break;
		}
		default:
			base.Console.Write("Command unknow, use prayer help");
			break;
		}
	}

	private bool GetParamPrayerType(string param, out Prayer.PrayerType prayerType)
	{
		bool result = true;
		prayerType = Prayer.PrayerType.Hymn;
		switch (param.ToLower())
		{
		case "hymn":
			prayerType = Prayer.PrayerType.Hymn;
			break;
		case "lament":
			prayerType = Prayer.PrayerType.Laments;
			break;
		case "thanks":
			prayerType = Prayer.PrayerType.Thanksgiving;
			break;
		default:
			base.Console.Write("The prayer type must be hymn, lament, or thanks");
			result = false;
			break;
		}
		return result;
	}

	private void ListInventoryList<T>(string caption, ReadOnlyCollection<T> list) where T : BaseInventoryObject
	{
		base.Console.Write(caption);
		foreach (T item in list)
		{
			base.Console.Write(item.id + " - " + item.caption);
		}
	}

	private bool CheckInvObject(string invType, string name, BaseInventoryObject invObj)
	{
		bool flag = invObj != null;
		if (!flag)
		{
			base.Console.Write(name + " is not a valid " + invType + " ID, please use list subcommand");
		}
		return flag;
	}
}
