using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopScenePlayer : AbstractMonoBehaviour
{
	public enum State
	{
		Init,
		Selecting,
		Viewing,
		Purchasing,
		Exiting,
		Exited
	}

	private const float DOOR_TIME = 1f;

	private const float START_DELAY = 1f;

	[SerializeField]
	private PlayerId player;

	[Header("Visuals")]
	[SerializeField]
	private Transform door;

	[Header("Items")]
	[SerializeField]
	private List<ShopSceneItem> items;

	[SerializeField]
	private ShopSceneItem[] weaponItemPrefabs;

	[SerializeField]
	private ShopSceneItem[] charmItemPrefabs;

	[Header("UI Elements")]
	[SerializeField]
	private TMP_Text currencyText;

	[Space(10f)]
	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TextMeshProUGUI subText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private List<Sprite> coinSprites;

	[SerializeField]
	private Image currencyNbImage;

	[SerializeField]
	private Image coinImage;

	[SerializeField]
	private Transform doubleDigitCoinPosition;

	private Vector3 singleDigitCoinPosition;

	[SerializeField]
	private Transform currencyCanvas;

	[SerializeField]
	private float currencyCanvasScaleValue;

	[SerializeField]
	private float currencyCanvasMultiplier;

	[SerializeField]
	private SpriteRenderer poofPrefab;

	[SerializeField]
	private Sprite[] priceSprites;

	[SerializeField]
	private SpriteRenderer priceSpriteRenderer;

	[SerializeField]
	private SpriteRenderer chalkCoinSpriteRenderer;

	private Player input;

	private float doorPositionClosed;

	private float doorPositionOpen;

	public State state;

	private int index;

	private float currencyCanvasOriginalScale;

	private Coroutine scaleCoinCoroutine;

	private Coroutine moveItemCantPurchaseCoroutine;

	private bool exitingShop;

	private bool firstStart = true;

	private bool playerLeft;

	private bool isMoneyDoubleDigit;

	private int weaponIndex;

	private int charmIndex;

	private ShopSceneItem CurrentItem
	{
		get
		{
			index = Mathf.Clamp(index, 0, items.Count - 1);
			return items[index];
		}
	}

	public event Action OnPurchaseEvent;

	public event Action OnExitEvent;

	protected override void Awake()
	{
		base.Awake();
		doorPositionOpen = door.transform.localPosition.x;
		door.transform.SetLocalPosition(0f);
		doorPositionClosed = door.transform.localPosition.x;
		currencyCanvasOriginalScale = currencyCanvas.localScale.x;
		if (!PlayerManager.Multiplayer && player == PlayerId.PlayerTwo)
		{
			items[0].gameObject.SetActive(value: false);
			return;
		}
		weaponIndex = 0;
		charmIndex = 0;
		for (int i = 0; i < items.Count; i++)
		{
			ShopSceneItem shopSceneItem = null;
			ItemType itemType = items[i].itemType;
			if (itemType != 0)
			{
				if (itemType == ItemType.Charm)
				{
					while (charmIndex < charmItemPrefabs.Length && (PlayerData.Data.IsUnlocked(player, charmItemPrefabs[charmIndex].charm) || !charmItemPrefabs[charmIndex].IsAvailable))
					{
						charmIndex++;
					}
					if (charmIndex < charmItemPrefabs.Length)
					{
						shopSceneItem = charmItemPrefabs[charmIndex];
						charmIndex++;
					}
				}
			}
			else
			{
				while (weaponIndex < weaponItemPrefabs.Length && (PlayerData.Data.IsUnlocked(player, weaponItemPrefabs[weaponIndex].weapon) || !weaponItemPrefabs[weaponIndex].IsAvailable))
				{
					weaponIndex++;
				}
				if (weaponIndex < weaponItemPrefabs.Length)
				{
					shopSceneItem = weaponItemPrefabs[weaponIndex];
					weaponIndex++;
				}
			}
			if (shopSceneItem == null)
			{
				items[i].gameObject.SetActive(value: false);
				items.RemoveAt(i);
				i--;
			}
			else
			{
				ShopSceneItem shopSceneItem2 = items[i];
				shopSceneItem2.gameObject.SetActive(value: false);
				items[i] = UnityEngine.Object.Instantiate(shopSceneItem);
				items[i].transform.position = shopSceneItem2.transform.position;
				items[i].spriteShadowObject.transform.SetParent(null);
			}
		}
		foreach (ShopSceneItem item in items)
		{
			item.Init(player);
		}
	}

	private void Start()
	{
		if (player != 0 && !PlayerManager.Multiplayer)
		{
			base.enabled = false;
			base.gameObject.SetActive(value: false);
			return;
		}
		singleDigitCoinPosition = coinImage.transform.position;
		if (PlayerData.Data.GetCurrency(player) >= 10)
		{
			isMoneyDoubleDigit = true;
			coinImage.transform.position = doubleDigitCoinPosition.position;
		}
		PlayerManager.OnPlayerLeaveEvent += OnPlayerLeft;
		displayNameText.font = Localization.Instance.fonts[(int)Localization.language][41].fontAsset;
		subText.font = Localization.Instance.fonts[(int)Localization.language][41].fontAsset;
		descriptionText.font = Localization.Instance.fonts[(int)Localization.language][11].fontAsset;
	}

	private void Update()
	{
		if (InterruptingPrompt.IsInterrupting())
		{
			return;
		}
		if (PlayerData.Data.GetCurrency(player) >= 10 && !isMoneyDoubleDigit)
		{
			isMoneyDoubleDigit = true;
			coinImage.transform.position = doubleDigitCoinPosition.position;
		}
		else if (PlayerData.Data.GetCurrency(player) < 10 && isMoneyDoubleDigit)
		{
			isMoneyDoubleDigit = false;
			coinImage.transform.position = singleDigitCoinPosition;
		}
		int currency = PlayerData.Data.GetCurrency(player);
		Sprite sprite = ((currency < 0) ? coinSprites[0] : ((currency <= coinSprites.Count - 1) ? coinSprites[currency] : coinSprites[coinSprites.Count - 1]));
		currencyNbImage.sprite = sprite;
		switch (state)
		{
		case State.Init:
			break;
		case State.Selecting:
			if (items.Count > 0 && CurrentItem.state != 0)
			{
				break;
			}
			if (items.Count > 1 && input.GetButtonDown(18))
			{
				AudioManager.Play("shop_selection_change");
				index--;
				UpdateSelection();
				break;
			}
			if (items.Count > 1 && input.GetButtonDown(20))
			{
				AudioManager.Play("shop_selection_change");
				index++;
				UpdateSelection();
				break;
			}
			if (items.Count > 0 && input.GetButtonDown(13))
			{
				if (CurrentItem.Purchase())
				{
					Purchase();
				}
				else
				{
					CantPurchase();
				}
			}
			if (input.GetButtonDown(14) || playerLeft)
			{
				Exit();
			}
			break;
		case State.Viewing:
			break;
		case State.Purchasing:
			break;
		case State.Exiting:
			break;
		case State.Exited:
			if (input.GetButtonDown(13) && !exitingShop)
			{
				StopAllCoroutines();
				state = State.Init;
				OnStart();
			}
			break;
		}
	}

	public ShopSceneItem[] GetWeaponItemPrefabs()
	{
		return weaponItemPrefabs;
	}

	public ShopSceneItem[] GetCharmItemPrefabs()
	{
		return charmItemPrefabs;
	}

	public void OnStart()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(in_cr());
		}
	}

	private void Purchase()
	{
		AudioManager.Play("shop_purchase");
		UpdateSelection();
		if (scaleCoinCoroutine != null)
		{
			StopCoroutine(scaleCoinCoroutine);
		}
		scaleCoinCoroutine = StartCoroutine(scaleCoin_cr());
		state = State.Purchasing;
		if (this.OnPurchaseEvent != null)
		{
			this.OnPurchaseEvent();
		}
	}

	private void CantPurchase()
	{
		AudioManager.Play("shop_cantpurchase");
		if (moveItemCantPurchaseCoroutine != null)
		{
			StopCoroutine(moveItemCantPurchaseCoroutine);
		}
		moveItemCantPurchaseCoroutine = StartCoroutine(cantBuy_cr());
	}

	private void Exit()
	{
		if (base.gameObject.activeInHierarchy)
		{
			state = State.Exiting;
			if (moveItemCantPurchaseCoroutine != null)
			{
				StopCoroutine(moveItemCantPurchaseCoroutine);
			}
			StartCoroutine(out_cr());
			if (this.OnExitEvent != null)
			{
				this.OnExitEvent();
			}
		}
	}

	public void OnExit()
	{
		exitingShop = true;
	}

	private void UpdateSelection()
	{
		if (items.Count == 0)
		{
			Localization.Translation translation = Localization.Translate("out_of_stock_name");
			displayNameText.text = translation.text;
			displayNameText.font = translation.fonts.fontAsset;
			Localization.Translation translation2 = Localization.Translate("out_of_stock_subtext");
			subText.text = translation2.text;
			subText.font = translation2.fonts.fontAsset;
			Localization.Translation translation3 = Localization.Translate("out_of_stock_description");
			descriptionText.text = translation3.text;
			descriptionText.font = translation3.fonts.fontAsset;
			priceSpriteRenderer.enabled = false;
			chalkCoinSpriteRenderer.enabled = false;
			return;
		}
		foreach (ShopSceneItem item in items)
		{
			item.Deselect();
		}
		CurrentItem.Select();
		if (CurrentItem.Purchased)
		{
			displayNameText.text = Localization.Translate("item_purchased_name").text;
			subText.text = Localization.Translate("item_purchased_subtext").text;
			descriptionText.text = Localization.Translate("item_purchased_description").text;
			return;
		}
		displayNameText.text = CurrentItem.DisplayName.ToUpper();
		priceSpriteRenderer.sprite = priceSprites[CurrentItem.Value - 1];
		subText.text = CurrentItem.Subtext;
		descriptionText.text = CurrentItem.Description;
		if (CurrentItem.charm == Charm.charm_curse)
		{
			displayNameText.text = Localization.Translate("charm_broken_name").text.ToUpper();
			subText.text = Localization.Translate("charm_broken_subtext").text;
			descriptionText.text = Localization.Translate("charm_broken_description").text;
		}
	}

	private void OnDoorTweened(float value)
	{
		door.SetLocalPosition(Mathf.Lerp(doorPositionClosed, doorPositionOpen, value));
	}

	private IEnumerator in_cr()
	{
		if (firstStart)
		{
			yield return new WaitForSeconds((player != 0) ? 1.4f : 1.1f);
		}
		firstStart = false;
		input = PlayerManager.GetPlayerInput(player);
		UpdateSelection();
		if (player == PlayerId.PlayerOne)
		{
			AudioManager.Play("shop_slide_open_cuphead");
		}
		else
		{
			AudioManager.Play("shop_slide_open_mugman");
		}
		yield return TweenValue(0f, 1f, 1f, EaseUtils.EaseType.easeOutBounce, OnDoorTweened);
		state = State.Selecting;
	}

	private IEnumerator out_cr()
	{
		if (player == PlayerId.PlayerOne)
		{
			AudioManager.Play("shop_slide_close_cuphead");
		}
		else
		{
			AudioManager.Play("shop_slide_close_mugman");
		}
		foreach (ShopSceneItem item in items)
		{
			item.Deselect();
		}
		yield return TweenValue(1f, 0f, 1f, EaseUtils.EaseType.easeOutBounce, OnDoorTweened);
		state = State.Exited;
	}

	private IEnumerator scaleCoin_cr()
	{
		while (currencyCanvas.localScale.x < currencyCanvasOriginalScale * currencyCanvasMultiplier)
		{
			currencyCanvas.localScale = new Vector2(currencyCanvas.localScale.x + currencyCanvasScaleValue, currencyCanvas.localScale.y + currencyCanvasScaleValue);
			yield return null;
		}
		while (currencyCanvas.localScale.x > currencyCanvasOriginalScale)
		{
			currencyCanvas.localScale = new Vector2(currencyCanvas.localScale.x - currencyCanvasScaleValue, currencyCanvas.localScale.y - currencyCanvasScaleValue);
			yield return null;
		}
		scaleCoinCoroutine = null;
		StartCoroutine(addNewItem_cr());
	}

	private IEnumerator cantBuy_cr()
	{
		float startPositionY = CurrentItem.endPosition.y;
		while (CurrentItem.transform.localPosition.y > startPositionY - CurrentItem.cantPurchaseYMovementPosition)
		{
			CurrentItem.transform.localPosition = new Vector2(CurrentItem.transform.localPosition.x, CurrentItem.transform.localPosition.y - CurrentItem.cantPurchaseYMovementValue);
			yield return null;
		}
		yield return CupheadTime.WaitForSeconds(this, 0.1f);
		while (CurrentItem.transform.position.y < startPositionY)
		{
			CurrentItem.transform.localPosition = new Vector2(CurrentItem.transform.localPosition.x, CurrentItem.transform.localPosition.y + CurrentItem.cantPurchaseYMovementValue * 1.5f);
			yield return null;
		}
		moveItemCantPurchaseCoroutine = null;
	}

	private IEnumerator addNewItem_cr()
	{
		ItemType type = CurrentItem.itemType;
		ShopSceneItem originalItem = CurrentItem;
		switch (type)
		{
		case ItemType.Charm:
		{
			bool foundItem = false;
			for (int i = charmIndex; i < charmItemPrefabs.Length; i++)
			{
				if (!PlayerData.Data.IsUnlocked(player, charmItemPrefabs[i].charm) && charmItemPrefabs[i].IsAvailable)
				{
					foundItem = true;
					int itemIndex = items.IndexOf(CurrentItem);
					items[itemIndex] = UnityEngine.Object.Instantiate(charmItemPrefabs[i]);
					items[itemIndex].player = player;
					items[itemIndex].startPosition = originalItem.startPosition;
					items[itemIndex].endPosition = originalItem.endPosition;
					Vector3 startPosition = items[itemIndex].startPosition;
					startPosition.y += 800f;
					items[itemIndex].transform.position = items[itemIndex].startPosition;
					items[itemIndex].spriteShadowObject.transform.SetParent(null);
					items[itemIndex].transform.position = startPosition;
					Vector3 originalShadowScale = items[itemIndex].spriteShadowObject.transform.localScale;
					items[itemIndex].spriteShadowObject.transform.localScale = Vector3.zero;
					items[itemIndex].TweenLocalPositionY(items[itemIndex].transform.position.y, items[itemIndex].startPosition.y, 0.5f, EaseUtils.EaseType.linear);
					float t = 0f;
					float TIME = 0.5f;
					while (t < TIME)
					{
						Vector3 newScale2 = Vector3.Lerp(t: EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t / TIME), a: Vector3.zero, b: originalShadowScale);
						items[itemIndex].spriteShadowObject.transform.localScale = newScale2;
						t += Time.deltaTime;
						yield return null;
					}
					SpriteRenderer dustPoof2 = UnityEngine.Object.Instantiate(poofPrefab, items[itemIndex].transform.position + items[itemIndex].poofOffset, Quaternion.identity);
					AudioManager.Play("item_drop");
					dustPoof2.sortingOrder = 501;
					UnityEngine.Object.Destroy(dustPoof2.gameObject, 3f);
					yield return items[itemIndex].TweenLocalPositionY(items[itemIndex].transform.position.y, items[itemIndex].transform.position.y + 30f, 0.1f, EaseUtils.EaseType.linear);
					yield return items[itemIndex].TweenLocalPositionY(items[itemIndex].transform.position.y, items[itemIndex].transform.position.y - 30f, 0.1f, EaseUtils.EaseType.linear);
					yield return CupheadTime.WaitForSeconds(this, 0.2f);
					charmIndex = i + 1;
					UpdateSelection();
					break;
				}
			}
			if (!foundItem)
			{
				CurrentItem.spriteShadowObject.gameObject.SetActive(value: false);
				items.Remove(CurrentItem);
				UpdateSelection();
			}
			break;
		}
		case ItemType.Weapon:
		{
			bool foundItem2 = false;
			for (int j = weaponIndex; j < weaponItemPrefabs.Length; j++)
			{
				if (!PlayerData.Data.IsUnlocked(player, weaponItemPrefabs[j].weapon) && weaponItemPrefabs[j].IsAvailable)
				{
					foundItem2 = true;
					int itemIndex2 = items.IndexOf(CurrentItem);
					items[itemIndex2] = UnityEngine.Object.Instantiate(weaponItemPrefabs[j]);
					items[itemIndex2].player = player;
					items[itemIndex2].startPosition = originalItem.startPosition;
					items[itemIndex2].endPosition = originalItem.endPosition;
					Vector3 startPosition2 = items[itemIndex2].startPosition;
					startPosition2.y += 800f;
					items[itemIndex2].transform.position = items[itemIndex2].startPosition;
					items[itemIndex2].spriteShadowObject.transform.SetParent(null);
					items[itemIndex2].transform.position = startPosition2;
					Vector3 originalShadowScale2 = items[itemIndex2].spriteShadowObject.transform.localScale;
					items[itemIndex2].spriteShadowObject.transform.localScale = Vector3.zero;
					items[itemIndex2].TweenLocalPositionY(items[itemIndex2].transform.position.y, items[itemIndex2].startPosition.y, 0.5f, EaseUtils.EaseType.linear);
					float t2 = 0f;
					float TIME2 = 0.5f;
					while (t2 < TIME2)
					{
						Vector3 newScale = Vector3.Lerp(t: EaseUtils.Ease(EaseUtils.EaseType.linear, 0f, 1f, t2 / TIME2), a: Vector3.zero, b: originalShadowScale2);
						items[itemIndex2].spriteShadowObject.transform.localScale = newScale;
						t2 += Time.deltaTime;
						yield return null;
					}
					SpriteRenderer dustPoof = UnityEngine.Object.Instantiate(poofPrefab, items[itemIndex2].transform.position + items[itemIndex2].poofOffset, Quaternion.identity);
					AudioManager.Play("item_drop");
					dustPoof.sortingOrder = 401;
					UnityEngine.Object.Destroy(dustPoof.gameObject, 3f);
					yield return items[itemIndex2].TweenLocalPositionY(items[itemIndex2].transform.position.y, items[itemIndex2].transform.position.y + 30f, 0.1f, EaseUtils.EaseType.linear);
					yield return items[itemIndex2].TweenLocalPositionY(items[itemIndex2].transform.position.y, items[itemIndex2].transform.position.y - 30f, 0.1f, EaseUtils.EaseType.linear);
					yield return CupheadTime.WaitForSeconds(this, 0.2f);
					weaponIndex = j + 1;
					UpdateSelection();
					break;
				}
			}
			if (!foundItem2)
			{
				CurrentItem.spriteShadowObject.gameObject.SetActive(value: false);
				items.Remove(CurrentItem);
				UpdateSelection();
			}
			break;
		}
		}
		state = State.Selecting;
	}

	private void OnPlayerLeft(PlayerId playerId)
	{
		if (playerId == player)
		{
			playerLeft = true;
		}
	}

	private void OnDestroy()
	{
		weaponItemPrefabs = null;
		charmItemPrefabs = null;
		currencyNbImage = null;
		coinImage = null;
		priceSprites = null;
		poofPrefab = null;
		items = null;
	}
}
