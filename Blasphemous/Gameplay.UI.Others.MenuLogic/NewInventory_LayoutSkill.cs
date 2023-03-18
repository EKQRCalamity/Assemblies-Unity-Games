using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.UI.Others.Buttons;
using Gameplay.UI.Others.UIGameLogic;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class NewInventory_LayoutSkill : NewInventory_Layout
{
	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private GameObject skillsRoot;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private NewInventory_Description description;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private Text maxTier;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private PlayerPurgePoints purgeControl;

	[BoxGroup("Sounds", true, false, 0)]
	[SerializeField]
	private const string loadingPurchaseFx = "event:/SFX/UI/BuySkill";

	private EventInstance _loadingPurchaseFxEvent;

	[SerializeField]
	[BoxGroup("Unlock", true, false, 0)]
	private float timeToUnlockSKill = 2f;

	[SerializeField]
	[BoxGroup("Unlock", true, false, 0)]
	private Image unlockMask;

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private string soundElementChange = "event:/SFX/UI/ChangeSelection";

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private string soundUnlockSkill = "event:/SFX/UI/EquipPrayer";

	[SerializeField]
	[BoxGroup("Main", true, false, 0)]
	[ValueDropdown("SkillsValues")]
	private string initialSkill;

	private List<NewInventory_Skill> cachedSkills;

	private bool objectsCached;

	private int currentSelected = -1;

	private bool ignoreSelectSound;

	private bool inEditMode;

	private int initialSlot;

	private float timePressingUnlock;

	private Player rewired;

	private IList<string> SkillsValues()
	{
		return null;
	}

	private void Awake()
	{
		unlockMask.fillAmount = 0f;
		timePressingUnlock = 0f;
		ignoreSelectSound = false;
		inEditMode = false;
		if (objectsCached)
		{
			return;
		}
		cachedSkills = new List<NewInventory_Skill>();
		objectsCached = true;
		int num = 0;
		NewInventory_Skill[] componentsInChildren = skillsRoot.GetComponentsInChildren<NewInventory_Skill>();
		foreach (NewInventory_Skill newInventory_Skill in componentsInChildren)
		{
			if (newInventory_Skill.GetSkillId() == initialSkill)
			{
				initialSlot = num;
			}
			int elementNumber = num;
			EventsButton eventButton = newInventory_Skill.GetEventButton();
			eventButton.onClick = new EventsButton.ButtonClickedEvent();
			eventButton.onClick.AddListener(delegate
			{
				ActivateSkill(elementNumber);
			});
			eventButton.onSelected = new EventsButton.ButtonSelectedEvent();
			eventButton.onSelected.AddListener(delegate
			{
				SelectSkill(elementNumber);
			});
			cachedSkills.Add(newInventory_Skill);
			num++;
		}
	}

	private void Update()
	{
		NewInventory_Skill newInventory_Skill = null;
		if (!inEditMode)
		{
			return;
		}
		bool flag = false;
		if (currentSelected != -1 && currentSelected < cachedSkills.Count)
		{
			newInventory_Skill = cachedSkills[currentSelected];
			flag = Core.SkillManager.CanUnlockSkill(newInventory_Skill.GetSkillId());
		}
		if (flag)
		{
			if (rewired == null)
			{
				rewired = ReInput.players.GetPlayer(0);
			}
			if (rewired.GetButton(52))
			{
				timePressingUnlock += Time.unscaledDeltaTime;
				PlayLoadingPurchaseFx(out _loadingPurchaseFxEvent);
				if (timePressingUnlock >= timeToUnlockSKill)
				{
					timePressingUnlock = 0f;
					Core.SkillManager.UnlockSkill(newInventory_Skill.GetSkillId());
					UpdateAllSKills();
					SelectSkill(currentSelected, playSound: false);
					Core.Audio.PlayOneShot(soundUnlockSkill);
					StopLoadingPurchaseFx(ref _loadingPurchaseFxEvent);
				}
			}
			else
			{
				timePressingUnlock = 0f;
				StopLoadingPurchaseFx(ref _loadingPurchaseFxEvent);
			}
		}
		else
		{
			timePressingUnlock = 0f;
		}
		unlockMask.fillAmount = timePressingUnlock / timeToUnlockSKill;
	}

	public override void RestoreFromLore()
	{
		StartCoroutine(FocusSkillSecure((currentSelected != -1) ? currentSelected : 0));
	}

	public override void RestoreSlotPosition(int slotPosition)
	{
		StartCoroutine(FocusSkillSecure(slotPosition));
	}

	public override int GetLastSlotSelected()
	{
		return currentSelected;
	}

	public void CancelEditMode()
	{
		inEditMode = false;
	}

	public override void ShowLayout(NewInventoryWidget.TabType tabType, bool editMode)
	{
		if ((bool)purgeControl)
		{
			purgeControl.RefreshPoints(inmediate: true);
		}
		inEditMode = editMode;
		UpdateAllSKills();
		maxTier.text = Core.SkillManager.GetCurrentMeaCulpa().ToString();
		StartCoroutine(FocusSkillSecure(initialSlot));
	}

	public override void GetSelectedLoreData(out string caption, out string lore)
	{
		caption = string.Empty;
		lore = string.Empty;
	}

	public override int GetItemPosition(BaseInventoryObject item)
	{
		for (int i = 0; i < cachedSkills.Count; i++)
		{
			if (cachedSkills[i] == item)
			{
				return i;
			}
		}
		return GetLastSlotSelected();
	}

	public void ActivateSkill(int skill)
	{
	}

	public void SelectSkill(int skill, bool playSound = true)
	{
		timePressingUnlock = 0f;
		if (currentSelected != -1 && currentSelected < cachedSkills.Count)
		{
			cachedSkills[currentSelected].SetFocus(bFocus: false, inEditMode);
		}
		currentSelected = skill;
		NewInventory_Skill newInventory_Skill = cachedSkills[currentSelected];
		if ((bool)description)
		{
			description.SetKill(newInventory_Skill.GetSkillId(), inEditMode);
		}
		if (playSound)
		{
			if (ignoreSelectSound)
			{
				ignoreSelectSound = false;
			}
			else
			{
				Core.Audio.PlayOneShot(soundElementChange);
			}
		}
		newInventory_Skill.SetFocus(bFocus: true, inEditMode);
	}

	private IEnumerator FocusSkillSecure(int skill, bool ignoreSound = true)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (cachedSkills.Count > skill)
		{
			GameObject selectedGameObject = cachedSkills[skill].gameObject;
			ignoreSelectSound = ignoreSound;
			EventSystem.current.SetSelectedGameObject(selectedGameObject);
			SelectSkill(skill, playSound: false);
		}
	}

	private void UpdateAllSKills()
	{
		foreach (NewInventory_Skill cachedSkill in cachedSkills)
		{
			cachedSkill.UpdateStatus();
			cachedSkill.SetFocus(bFocus: false, inEditMode);
		}
	}

	public void PlayLoadingPurchaseFx(out EventInstance eventInstance)
	{
		if (string.IsNullOrEmpty("event:/SFX/UI/BuySkill"))
		{
			eventInstance = default(EventInstance);
			return;
		}
		if (_loadingPurchaseFxEvent.isValid())
		{
			eventInstance = _loadingPurchaseFxEvent;
			return;
		}
		eventInstance = Core.Audio.CreateEvent("event:/SFX/UI/BuySkill");
		eventInstance.start();
	}

	public void StopLoadingPurchaseFx(ref EventInstance eventInstance)
	{
		if (eventInstance.isValid())
		{
			eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			eventInstance.release();
		}
	}
}
