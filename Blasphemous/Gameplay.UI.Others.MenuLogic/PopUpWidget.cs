using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class PopUpWidget : SerializedMonoBehaviour
{
	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private GameObject messageRoot;

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private GameObject objectRoot;

	[SerializeField]
	[BoxGroup("Item Get", true, false, 0)]
	private Text objectMessage;

	[SerializeField]
	[BoxGroup("Item Get", true, false, 0)]
	private Text objectText;

	[SerializeField]
	[BoxGroup("Item Get", true, false, 0)]
	private Image objectImage;

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private GameObject pressKeyText;

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private GameObject areaRoot;

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private GameObject cherubRoot;

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private Dictionary<InventoryManager.ItemType, string> itemAddSounds = new Dictionary<InventoryManager.ItemType, string>
	{
		{
			InventoryManager.ItemType.Relic,
			"event:/Key Event/RelicCollected"
		},
		{
			InventoryManager.ItemType.Quest,
			"event:/Key Event/Quest Item"
		},
		{
			InventoryManager.ItemType.Prayer,
			"event:/Key Event/PrayerCollected"
		},
		{
			InventoryManager.ItemType.Bead,
			"event:/Key Event/PrayerCollected"
		},
		{
			InventoryManager.ItemType.Collectible,
			"event:/Key Event/Quest Item"
		},
		{
			InventoryManager.ItemType.Sword,
			"event:/Key Event/PrayerCollected"
		}
	};

	[SerializeField]
	[BoxGroup("Sounds", true, false, 0)]
	private string areaEventSound = "event:/Key Event/ZoneInfo";

	[SerializeField]
	[TutorialId]
	[BoxGroup("Tutorial", true, false, 0)]
	private string TutorialSword;

	[SerializeField]
	[TutorialId]
	[BoxGroup("Tutorial", true, false, 0)]
	private string TutorialBead;

	[SerializeField]
	[TutorialId]
	[BoxGroup("Tutorial", true, false, 0)]
	private string TutorialRelic;

	private const string ANIMATOR_VARIBLE = "IsEnabled";

	private Animator animator;

	private Text messageText;

	private Text areaText;

	private Text cherubText;

	private bool waitingEnd;

	private bool waitingForKey;

	private float timeToWait;

	private string pendingTutorial = string.Empty;

	public bool IsShowing { get; private set; }

	public bool WaitingToShowArea { get; internal set; }

	public static event Core.SimpleEvent OnDialogClose;

	private void Awake()
	{
		IsShowing = false;
		waitingEnd = false;
		waitingForKey = false;
		timeToWait = 0f;
		animator = GetComponent<Animator>();
		messageText = messageRoot.GetComponentInChildren<Text>();
		areaText = areaRoot.GetComponentInChildren<Text>();
		messageRoot.SetActive(value: false);
		objectRoot.SetActive(value: false);
		cherubRoot.SetActive(value: false);
		cherubText = cherubRoot.GetComponentInChildren<Text>();
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		if (!Core.Events.GetFlag("CHERUB_RESPAWN"))
		{
			Core.Input.SetBlocker("POP_UP", blocking: false);
		}
	}

	private void Update()
	{
		if (waitingForKey && IsShowing && !waitingEnd)
		{
			Player player = ReInput.players.GetPlayer(0);
			if (player.GetButtonDown(8) || player.GetButtonDown(5) || player.GetButtonDown(7) || player.GetButtonDown(6))
			{
				waitingEnd = true;
				waitingForKey = false;
				StartCoroutine(SafeEnd());
			}
		}
		if (timeToWait > 0f && IsShowing && !waitingEnd)
		{
			timeToWait -= Time.unscaledDeltaTime;
			if (timeToWait <= 0f)
			{
				waitingEnd = true;
				timeToWait = 0f;
				StartCoroutine(SafeEnd());
			}
		}
	}

	public void ShowPopUp(string message, string eventSound, float timeToWait = 0f, bool blockPlayer = true)
	{
		pendingTutorial = string.Empty;
		messageRoot.SetActive(value: true);
		objectRoot.SetActive(value: false);
		areaRoot.SetActive(value: false);
		cherubRoot.SetActive(value: false);
		messageText.text = message;
		CommonShow(eventSound, timeToWait, blockPlayer);
	}

	public void ShowCherubPopUp(string message, string eventSound, float timeToWait = 0f, bool blockPlayer = true)
	{
		pendingTutorial = "13_CHERUBS";
		messageRoot.SetActive(value: false);
		objectRoot.SetActive(value: false);
		areaRoot.SetActive(value: false);
		cherubRoot.SetActive(value: true);
		cherubText.text = message;
		CommonShow(eventSound, timeToWait, blockPlayer);
	}

	public void ShowAreaPopUp(string area, float timeToWait = 3f, bool blockPlayer = false)
	{
		if (WaitingToShowArea)
		{
			pendingTutorial = string.Empty;
			messageRoot.SetActive(value: false);
			objectRoot.SetActive(value: false);
			areaRoot.SetActive(value: true);
			cherubRoot.SetActive(value: false);
			areaText.text = area;
			CommonShow(areaEventSound, timeToWait, blockPlayer);
		}
	}

	public void HideAreaPopup()
	{
		WaitingToShowArea = false;
		CanvasGroup component = GetComponent<CanvasGroup>();
		areaRoot.SetActive(value: false);
		areaText.text = string.Empty;
		component.alpha = 0f;
		IsShowing = false;
		animator.SetBool("IsEnabled", value: false);
	}

	public void ShowItemGet(string message, string itemName, Sprite image, InventoryManager.ItemType objType, float timeToWait = 3f, bool blockPlayer = false)
	{
		switch (objType)
		{
		case InventoryManager.ItemType.Sword:
			pendingTutorial = TutorialSword;
			break;
		case InventoryManager.ItemType.Bead:
			pendingTutorial = TutorialBead;
			break;
		case InventoryManager.ItemType.Relic:
			pendingTutorial = TutorialRelic;
			break;
		default:
			pendingTutorial = string.Empty;
			break;
		}
		if (pendingTutorial != string.Empty && !Core.TutorialManager.IsTutorialUnlocked(pendingTutorial))
		{
			blockPlayer = true;
		}
		messageRoot.SetActive(value: false);
		objectRoot.SetActive(value: true);
		areaRoot.SetActive(value: false);
		cherubRoot.SetActive(value: false);
		objectText.text = itemName;
		objectImage.sprite = image;
		objectMessage.text = message;
		CommonShow(itemAddSounds[objType], timeToWait, blockPlayer);
	}

	private void CommonShow(string eventSound, float timeToWait, bool blockPlayer)
	{
		waitingForKey = false;
		if (blockPlayer)
		{
			Core.Input.SetBlocker("POP_UP", blocking: true);
		}
		this.timeToWait = timeToWait;
		pressKeyText.SetActive(timeToWait == 0f);
		if (timeToWait == 0f)
		{
			StartCoroutine(SetInputSafe());
		}
		IsShowing = true;
		waitingEnd = false;
		animator.SetBool("IsEnabled", value: true);
		if (eventSound != string.Empty)
		{
			Core.Audio.PlayOneShot(eventSound);
		}
	}

	private IEnumerator SafeEnd()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		animator.SetBool("IsEnabled", value: false);
		IsShowing = false;
		waitingEnd = false;
		waitingForKey = false;
		timeToWait = 0f;
		yield return new WaitForSecondsRealtime(0.15f);
		Core.Input.SetBlocker("POP_UP", blocking: false);
		if (PopUpWidget.OnDialogClose != null)
		{
			PopUpWidget.OnDialogClose();
		}
		if (pendingTutorial != string.Empty && !Core.TutorialManager.IsTutorialUnlocked(pendingTutorial))
		{
			StartCoroutine(Core.TutorialManager.ShowTutorial(pendingTutorial));
			pendingTutorial = string.Empty;
		}
	}

	private IEnumerator SetInputSafe()
	{
		yield return new WaitForSecondsRealtime(0.2f);
		waitingForKey = true;
	}
}
