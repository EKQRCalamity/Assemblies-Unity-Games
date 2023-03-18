using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class NewInventory_Description : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private Image objectImage;

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private Text caption;

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private Text description;

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private Color colorDisable = new Color(0.243f, 0.243f, 0.243f);

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private Color colorTextNormal = new Color(0.545f, 0.522f, 0.376f);

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private CustomScrollView customScrollView;

	[SerializeField]
	[BoxGroup("Skill", true, false, 0)]
	private Text instructions;

	[SerializeField]
	[BoxGroup("Skill", true, false, 0)]
	private TextMeshProUGUI instructionsPro;

	[SerializeField]
	[BoxGroup("Inventory", true, false, 0)]
	private GameObject buttonEquip;

	[SerializeField]
	[BoxGroup("Inventory", true, false, 0)]
	private GameObject buttonUnEquip;

	[SerializeField]
	[BoxGroup("Inventory", true, false, 0)]
	private GameObject buttonDechiper;

	[SerializeField]
	[BoxGroup("Inventory", true, false, 0)]
	private GameObject buttonLore;

	[SerializeField]
	[BoxGroup("Skill", true, false, 0)]
	private GameObject buttonUnlock;

	[SerializeField]
	[BoxGroup("Skill", true, false, 0)]
	private GameObject buttonExit;

	[SerializeField]
	[BoxGroup("Skill", true, false, 0)]
	private GameObject helpText;

	[SerializeField]
	[BoxGroup("Skill", true, false, 0)]
	private Image bigImageSkill;

	public void SetObject(BaseInventoryObject invObj, NewInventoryWidget.EquipAction action)
	{
		if ((bool)invObj)
		{
			objectImage.sprite = invObj.picture;
			objectImage.enabled = true;
			caption.text = invObj.caption;
			description.text = invObj.description;
			buttonLore.SetActive(invObj.HasLore());
		}
		else
		{
			objectImage.enabled = false;
			caption.text = string.Empty;
			description.text = string.Empty;
			buttonLore.SetActive(value: false);
		}
		buttonEquip.SetActive((bool)invObj && invObj.IsEquipable() && action != NewInventoryWidget.EquipAction.UnEquip);
		buttonUnEquip.SetActive((bool)invObj && invObj.IsEquipable() && action == NewInventoryWidget.EquipAction.UnEquip);
		buttonDechiper.SetActive(value: false);
		if ((bool)invObj && action == NewInventoryWidget.EquipAction.Equip)
		{
			SetColorButton(buttonEquip, Color.white, colorTextNormal);
		}
		else
		{
			SetColorButton(buttonEquip, colorDisable, colorDisable);
		}
		customScrollView.NewContentSetted();
	}

	public void SetKill(string skillId, bool editmode)
	{
		if ((bool)objectImage)
		{
			objectImage.enabled = false;
		}
		if ((bool)buttonEquip)
		{
			buttonEquip.SetActive(value: false);
		}
		if ((bool)buttonUnEquip)
		{
			buttonUnEquip.SetActive(value: false);
		}
		if ((bool)buttonDechiper)
		{
			buttonDechiper.SetActive(value: false);
		}
		if ((bool)buttonLore)
		{
			buttonLore.SetActive(value: false);
		}
		UnlockableSkill skill = Core.SkillManager.GetSkill(skillId);
		caption.text = skill.caption;
		description.text = skill.description + "\n";
		if ((bool)instructionsPro)
		{
			instructionsPro.text = LocalizationManager.ParseMeshPro(skill.instructions, "SKILL_" + skillId, instructionsPro);
		}
		else
		{
			instructions.text = skill.instructions;
		}
		helpText.SetActive(!editmode);
		if (editmode && Core.SkillManager.CanUnlockSkill(skillId))
		{
			buttonUnlock.SetActive(value: true);
		}
		else
		{
			buttonUnlock.SetActive(value: false);
		}
		if ((bool)bigImageSkill)
		{
			bigImageSkill.gameObject.SetActive(skill.bigImage != null);
			bigImageSkill.sprite = skill.bigImage;
		}
		buttonExit.SetActive(editmode);
		customScrollView.NewContentSetted();
	}

	public void SetColorButton(GameObject button, Color buttonColor, Color textColor)
	{
		if ((bool)button)
		{
			button.transform.GetComponentInChildren<Image>().color = buttonColor;
			button.transform.GetComponentInChildren<Text>().color = textColor;
		}
	}

	public void EnabledInput(bool value)
	{
		customScrollView.InputEnabled = value;
	}
}
