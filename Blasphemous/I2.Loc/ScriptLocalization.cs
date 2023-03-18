using UnityEngine;

namespace I2.Loc;

public static class ScriptLocalization
{
	public static class UI
	{
		public static string GET_GUILTDROP_TEXT => Get("UI/GET_GUILTDROP_TEXT");

		public static string ENABLED_TEXT => Get("UI/ENABLED_TEXT");

		public static string DISABLED_TEXT => Get("UI/DISABLED_TEXT");

		public static string ISIDORA_MENU_FORBIDDEN => Get("UI/ISIDORA_MENU_FORBIDDEN");
	}

	public static class UI_BossRush
	{
		public static string COURSE_A_1 => Get("UI_BossRush/COURSE_A_1");

		public static string COURSE_A_2 => Get("UI_BossRush/COURSE_A_2");

		public static string COURSE_A_3 => Get("UI_BossRush/COURSE_A_3");

		public static string COURSE_B_1 => Get("UI_BossRush/COURSE_B_1");

		public static string COURSE_C_1 => Get("UI_BossRush/COURSE_C_1");

		public static string COURSE_D_1 => Get("UI_BossRush/COURSE_D_1");

		public static string TEXT_BESTTIME => Get("UI_BossRush/TEXT_BESTTIME");

		public static string TEXT_HARD_UNLOCKED => Get("UI_BossRush/TEXT_HARD_UNLOCKED");

		public static string COMPLETED_SUFFIX => Get("UI_BossRush/COMPLETED_SUFFIX");

		public static string FAILEDED_SUFFIX => Get("UI_BossRush/FAILEDED_SUFFIX");

		public static string LABEL_UNLOCK_COURSE_A_2 => Get("UI_BossRush/LABEL_UNLOCK_COURSE_A_2");

		public static string LABEL_UNLOCK_COURSE_A_3 => Get("UI_BossRush/LABEL_UNLOCK_COURSE_A_3");

		public static string LABEL_UNLOCK_COURSE_B_1 => Get("UI_BossRush/LABEL_UNLOCK_COURSE_B_1");

		public static string LABEL_UNLOCK_COURSE_C_1 => Get("UI_BossRush/LABEL_UNLOCK_COURSE_C_1");

		public static string LABEL_UNLOCK_COURSE_D_1 => Get("UI_BossRush/LABEL_UNLOCK_COURSE_D_1");
	}

	public static class UI_Extras
	{
		public static string BACKGROUND_1_LABEL => Get("UI_Extras/BACKGROUND_1_LABEL");

		public static string BACKGROUND_0_LABEL => Get("UI_Extras/BACKGROUND_0_LABEL");

		public static string BACKGROUND_2_LABEL => Get("UI_Extras/BACKGROUND_2_LABEL");

		public static string BACKGROUND_3_LABEL => Get("UI_Extras/BACKGROUND_3_LABEL");
	}

	public static class UI_Inventory
	{
		public static string TEXT_DOOR_NEED_OBJECT => Get("UI_Inventory/TEXT_DOOR_NEED_OBJECT");

		public static string TEXT_DOOR_USE_OBJECT => Get("UI_Inventory/TEXT_DOOR_USE_OBJECT");

		public static string TEXT_DOOR_CLOSED => Get("UI_Inventory/TEXT_DOOR_CLOSED");

		public static string TEXT_ITEM_FOUND => Get("UI_Inventory/TEXT_ITEM_FOUND");

		public static string TEXT_ITEM_FOUND_BEAD => Get("UI_Inventory/TEXT_ITEM_FOUND_BEAD");

		public static string TEXT_ITEM_FOUND_BEADS => Get("UI_Inventory/TEXT_ITEM_FOUND_BEADS");

		public static string GRID_LABEL_BEADS => Get("UI_Inventory/GRID_LABEL_BEADS");

		public static string GRID_LABEL_QUESTITEM => Get("UI_Inventory/GRID_LABEL_QUESTITEM");

		public static string GRID_LABEL_RELICS => Get("UI_Inventory/GRID_LABEL_RELICS");

		public static string GRID_LABEL_COLLECTIBLE => Get("UI_Inventory/GRID_LABEL_COLLECTIBLE");

		public static string MEACULPA_LABEL => Get("UI_Inventory/MEACULPA_LABEL");

		public static string TEXT_ITEM_GIVE => Get("UI_Inventory/TEXT_ITEM_GIVE");

		public static string TEXT_QUESTION_GIVE_ITEM => Get("UI_Inventory/TEXT_QUESTION_GIVE_ITEM");

		public static string TEXT_QUESTION_GIVE_PURGE => Get("UI_Inventory/TEXT_QUESTION_GIVE_PURGE");

		public static string TEXT_QUESTION_BUY_ITEM => Get("UI_Inventory/TEXT_QUESTION_BUY_ITEM");

		public static string TEXT_QI75_FILLS => Get("UI_Inventory/TEXT_QI75_FILLS");

		public static string TEXT_QI76_FILLS => Get("UI_Inventory/TEXT_QI76_FILLS");

		public static string TEXT_QI76_OR_QI77_UNFILLS => Get("UI_Inventory/TEXT_QI76_OR_QI77_UNFILLS");
	}

	public static class UI_Map
	{
		public static string LABEL_BUTTON_APPLY => Get("UI_Map/LABEL_BUTTON_APPLY");

		public static string LABEL_BUTTON_ACCEPT => Get("UI_Map/LABEL_BUTTON_ACCEPT");

		public static string LABEL_MENU_VIDEO_SCALE => Get("UI_Map/LABEL_MENU_VIDEO_SCALE");

		public static string LABEL_MENU_VIDEO_PIXELPERFECT => Get("UI_Map/LABEL_MENU_VIDEO_PIXELPERFECT");

		public static string LABEL_MENU_VIDEO_WINDOWED => Get("UI_Map/LABEL_MENU_VIDEO_WINDOWED");

		public static string LABEL_MENU_VIDEO_FULLSCREEN => Get("UI_Map/LABEL_MENU_VIDEO_FULLSCREEN");

		public static string LABEL_BUTTON_BACK => Get("UI_Map/LABEL_BUTTON_BACK");
	}

	public static class UI_Penitences
	{
		public static string PE01_INFO => Get("UI_Penitences/PE01_INFO");

		public static string PE02_INFO => Get("UI_Penitences/PE02_INFO");

		public static string PE03_INFO => Get("UI_Penitences/PE03_INFO");

		public static string NO_PENITENCE_INFO => Get("UI_Penitences/NO_PENITENCE_INFO");

		public static string PE01_NAME => Get("UI_Penitences/PE01_NAME");

		public static string PE02_NAME => Get("UI_Penitences/PE02_NAME");

		public static string PE03_NAME => Get("UI_Penitences/PE03_NAME");

		public static string NO_PENITENCE => Get("UI_Penitences/NO_PENITENCE");

		public static string CHOOSE_PENITENCE_CONFIRMATION => Get("UI_Penitences/CHOOSE_PENITENCE_CONFIRMATION");

		public static string CHOOSE_NO_PENITENCE_CONFIRMATION => Get("UI_Penitences/CHOOSE_NO_PENITENCE_CONFIRMATION");

		public static string CHOOSE_PENITENCE_ABANDON => Get("UI_Penitences/CHOOSE_PENITENCE_ABANDON");
	}

	public static class UI_Slot
	{
		public static string TEXT_SLOT_INFO => Get("UI_Slot/TEXT_SLOT_INFO");
	}

	public static string Get(string Term, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null)
	{
		return LocalizationManager.GetTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage);
	}
}
