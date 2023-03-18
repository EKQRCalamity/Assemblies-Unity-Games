using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI.Others.Buttons;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class NewInventory_Skill : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Main", true, false, 0)]
	[ValueDropdown("SkillsValues")]
	private string skill;

	[SerializeField]
	[BoxGroup("Main", true, false, 0)]
	private Color nomalColor;

	[SerializeField]
	[BoxGroup("Main", true, false, 0)]
	private Color disabledColor;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private GameObject backgorundLocked;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private GameObject backgorundUnLocked;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private GameObject backgorundEnabled;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private Image skillImage;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private GameObject focus;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private GameObject cost;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private Text costText;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Text tierText;

	public string GetSkillId()
	{
		return skill;
	}

	public EventsButton GetEventButton()
	{
		return GetComponent<EventsButton>();
	}

	private IList<string> SkillsValues()
	{
		return null;
	}

	public void Awake()
	{
		if ((bool)focus)
		{
			focus.SetActive(value: false);
		}
	}

	public void UpdateStatus()
	{
		UnlockableSkill unlockableSkill = Core.SkillManager.GetSkill(skill);
		backgorundLocked.SetActive(value: false);
		backgorundUnLocked.SetActive(value: false);
		backgorundEnabled.SetActive(value: false);
		tierText.text = string.Empty;
		bool flag = false;
		UnlockableSkill unlockableSkill2 = Core.SkillManager.GetSkill(skill);
		skillImage.sprite = unlockableSkill2.smallImage;
		if (Core.SkillManager.IsSkillUnlocked(skill))
		{
			backgorundUnLocked.SetActive(value: true);
			skillImage.sprite = unlockableSkill2.smallImageBuyed;
		}
		else if (Core.SkillManager.CanUnlockSkillNoCheckPoints(skill))
		{
			backgorundEnabled.SetActive(value: true);
		}
		else
		{
			tierText.text = unlockableSkill.tier.ToString();
			tierText.color = disabledColor;
			flag = true;
		}
		backgorundLocked.SetActive(flag);
		skillImage.gameObject.SetActive(!flag);
	}

	public void SetFocus(bool bFocus, bool editMode)
	{
		UnlockableSkill unlockableSkill = Core.SkillManager.GetSkill(skill);
		bool flag = bFocus && !Core.SkillManager.IsSkillUnlocked(skill) && (editMode || Core.SkillManager.CanUnlockSkillNoCheckPoints(skill));
		focus.SetActive(bFocus);
		cost.SetActive(flag);
		costText.gameObject.SetActive(flag);
		bool flag2 = true;
		if (flag)
		{
			if (!Core.SkillManager.CanUnlockSkillNoCheckPoints(skill))
			{
				costText.text = "???";
			}
			else
			{
				costText.text = unlockableSkill.cost.ToString();
				flag2 = !Core.SkillManager.CanUnlockSkill(skill) || !editMode;
			}
		}
		costText.color = ((!flag2) ? nomalColor : disabledColor);
	}
}
