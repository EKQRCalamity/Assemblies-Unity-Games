using System.Collections;
using UnityEngine;

public class ShopSceneItem : AbstractMonoBehaviour
{
	public enum State
	{
		Ready,
		Busy
	}

	public enum SpriteState
	{
		Inactive,
		Selected,
		Purchased
	}

	private const float FLOAT_TIME = 0.3f;

	public ItemType itemType;

	[Space(5f)]
	public Weapon weapon = Weapon.None;

	public Super super = Super.None;

	public Charm charm = Charm.None;

	[Header("Sprites")]
	public SpriteRenderer spriteInactive;

	public SpriteRenderer spriteSelected;

	public SpriteRenderer spritePurchased;

	public SpriteRenderer spriteShadowObject;

	public Sprite spriteShadow;

	public float cantPurchaseYMovementPosition;

	public float cantPurchaseYMovementValue;

	public Vector3 poofOffset;

	[HideInInspector]
	public Vector3 endPosition;

	public PlayerId player;

	[HideInInspector]
	public Vector3 startPosition;

	public Vector3 originalShadowScale;

	public SpriteRenderer buyAnimation;

	private Coroutine selectionCoroutine;

	[SerializeField]
	private bool isDLCItem;

	public State state { get; private set; }

	public bool Purchased => isPurchased(player);

	public string DisplayName => itemType switch
	{
		ItemType.Weapon => WeaponProperties.GetDisplayName(weapon), 
		ItemType.Super => WeaponProperties.GetDisplayName(super), 
		ItemType.Charm => WeaponProperties.GetDisplayName(charm), 
		_ => string.Empty, 
	};

	public string Subtext => itemType switch
	{
		ItemType.Weapon => WeaponProperties.GetSubtext(weapon), 
		ItemType.Super => WeaponProperties.GetSubtext(super), 
		ItemType.Charm => WeaponProperties.GetSubtext(charm), 
		_ => string.Empty, 
	};

	public string Description => itemType switch
	{
		ItemType.Weapon => WeaponProperties.GetDescription(weapon), 
		ItemType.Super => WeaponProperties.GetDescription(super), 
		ItemType.Charm => WeaponProperties.GetDescription(charm), 
		_ => string.Empty, 
	};

	public int Value => itemType switch
	{
		ItemType.Weapon => WeaponProperties.GetValue(weapon), 
		ItemType.Super => WeaponProperties.GetValue(super), 
		ItemType.Charm => WeaponProperties.GetValue(charm), 
		_ => 0, 
	};

	public bool IsAvailable => !isDLCItem || (isDLCItem && DLCManager.DLCEnabled());

	private bool isPurchased(PlayerId player)
	{
		return itemType switch
		{
			ItemType.Weapon => PlayerData.Data.IsUnlocked(player, weapon), 
			ItemType.Super => PlayerData.Data.IsUnlocked(player, super), 
			ItemType.Charm => PlayerData.Data.IsUnlocked(player, charm), 
			_ => false, 
		};
	}

	public bool isPurchasedForBuyAllItemsAchievement(PlayerId player)
	{
		if (isDLCItem)
		{
			return true;
		}
		return isPurchased(player);
	}

	public void Init(PlayerId player)
	{
		startPosition = base.transform.localPosition;
		endPosition = startPosition;
		endPosition.y += 40f;
		this.player = player;
		if (Purchased)
		{
			SetSprite(SpriteState.Purchased);
		}
		else
		{
			SetSprite(SpriteState.Inactive);
		}
	}

	private void SetSprite(SpriteState spriteState)
	{
		spriteInactive.enabled = false;
		spriteSelected.enabled = false;
		spritePurchased.enabled = false;
		switch (spriteState)
		{
		case SpriteState.Inactive:
			spriteInactive.enabled = true;
			break;
		case SpriteState.Selected:
			spriteSelected.enabled = true;
			break;
		case SpriteState.Purchased:
			spritePurchased.enabled = true;
			break;
		}
	}

	public void Select()
	{
		if (state == State.Ready)
		{
			if (!Purchased)
			{
				SetSprite(SpriteState.Selected);
			}
			StopAllCoroutines();
			StartCoroutine(float_cr(base.transform.localPosition, endPosition, spriteShadowObject.transform.localScale, originalShadowScale * 0.8f));
		}
	}

	public void Deselect()
	{
		if (state == State.Ready)
		{
			if (!Purchased)
			{
				SetSprite(SpriteState.Inactive);
			}
			StopAllCoroutines();
			StartCoroutine(float_cr(base.transform.localPosition, startPosition, spriteShadowObject.transform.localScale, originalShadowScale));
		}
	}

	private void UpdateFloat(float value)
	{
		base.transform.localPosition = Vector3.Lerp(startPosition, endPosition, value);
	}

	private void UpdatePurchasedColor(float value)
	{
		Color white = Color.white;
		Color black = Color.black;
		spritePurchased.color = Color.Lerp(white, black, value);
	}

	public bool Purchase()
	{
		if (state != 0)
		{
			return false;
		}
		if (Purchased)
		{
			return false;
		}
		bool flag = false;
		switch (itemType)
		{
		case ItemType.Weapon:
			flag = PlayerData.Data.Buy(player, weapon);
			break;
		case ItemType.Super:
			flag = PlayerData.Data.Buy(player, super);
			break;
		case ItemType.Charm:
			flag = PlayerData.Data.Buy(player, charm);
			break;
		}
		if (flag)
		{
			StartCoroutine(purchase_cr());
			if (ShopScene.Current.HasBoughtEverythingForAchievement(player))
			{
				OnlineManager.Instance.Interface.UnlockAchievement(player, "BoughtAllItems");
			}
			if (!PlayerData.Data.hasMadeFirstPurchase)
			{
				PlayerData.Data.shouldShowShopkeepTooltip = true;
				PlayerData.Data.hasMadeFirstPurchase = true;
				PlayerData.SaveCurrentFile();
			}
		}
		return flag;
	}

	private IEnumerator float_cr(Vector3 start, Vector3 end, Vector3 startShadowScale, Vector3 endShadowScale)
	{
		float t = 0f;
		float time = 0.3f * (Vector3.Distance(start, end) / Vector3.Distance(startPosition, endPosition));
		while (t < time)
		{
			float val = t / time;
			base.transform.localPosition = Vector3.Lerp(start, end, EaseUtils.Ease(EaseUtils.EaseType.easeOutSine, 0f, 1f, val));
			spriteShadowObject.transform.localScale = Vector3.Lerp(startShadowScale, endShadowScale, EaseUtils.Ease(EaseUtils.EaseType.easeOutSine, 0f, 1f, val));
			t += base.LocalDeltaTime;
			yield return null;
		}
		base.transform.localPosition = end;
		yield return null;
	}

	private IEnumerator purchase_cr()
	{
		state = State.Busy;
		SetSprite(SpriteState.Purchased);
		SpriteRenderer buyAnim = Object.Instantiate(buyAnimation, GetComponentInChildren<SpriteRenderer>().bounds.center, Quaternion.identity);
		buyAnim.sortingOrder = GetComponentInChildren<SpriteRenderer>().sortingOrder;
		spriteShadowObject.gameObject.SetActive(value: false);
		yield return TweenValue(0f, 1f, 0.0001f, EaseUtils.EaseType.linear, UpdatePurchasedColor);
		state = State.Ready;
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		buyAnimation = null;
		spriteShadow = null;
	}
}
