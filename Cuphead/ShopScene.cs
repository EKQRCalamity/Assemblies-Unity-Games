using System.Collections;
using UnityEngine;

public class ShopScene : AbstractMonoBehaviour
{
	[SerializeField]
	private ShopScenePlayer playerOne;

	[SerializeField]
	private ShopScenePlayer playerTwo;

	[Space(10f)]
	[SerializeField]
	private ShopScenePig pig;

	public bool isDLCShop;

	public static ShopScene Current { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Cuphead.Init();
		Current = this;
		playerOne.OnPurchaseEvent += OnPurchase;
		playerTwo.OnPurchaseEvent += OnPurchase;
		playerOne.OnExitEvent += OnExit;
		playerTwo.OnExitEvent += OnExit;
		SceneLoader.OnFadeOutEndEvent += OnLoaded;
	}

	private void OnDestroy()
	{
		if (Current == this)
		{
			Current = null;
		}
		SceneLoader.OnFadeOutEndEvent -= OnLoaded;
	}

	private void OnLoaded()
	{
		pig.OnStart();
		playerOne.OnStart();
		playerTwo.OnStart();
		InterruptingPrompt.SetCanInterrupt(canInterrupt: true);
	}

	private void OnPurchase()
	{
		pig.OnPurchase();
	}

	private void OnExit()
	{
		if ((!playerOne.gameObject.activeInHierarchy || playerOne.state == ShopScenePlayer.State.Exiting || playerOne.state == ShopScenePlayer.State.Exited) && (!playerTwo.gameObject.activeInHierarchy || playerTwo.state == ShopScenePlayer.State.Exiting || playerTwo.state == ShopScenePlayer.State.Exited))
		{
			StartCoroutine(exit_cr());
			playerOne.OnExit();
			playerTwo.OnExit();
		}
	}

	private IEnumerator exit_cr()
	{
		if (HasBoughtEverythingForAchievement(PlayerId.PlayerOne))
		{
			OnlineManager.Instance.Interface.UnlockAchievement(PlayerId.PlayerOne, "BoughtAllItems");
		}
		if (HasBoughtEverythingForAchievement(PlayerId.PlayerTwo))
		{
			OnlineManager.Instance.Interface.UnlockAchievement(PlayerId.PlayerTwo, "BoughtAllItems");
		}
		pig.OnExit();
		yield return pig.animator.WaitForAnimationToEnd(this, "Bye");
		SceneLoader.LoadLastMap();
	}

	public ShopSceneItem[] GetCharmItems(PlayerId player)
	{
		if (player == PlayerId.PlayerTwo)
		{
			return playerTwo.GetCharmItemPrefabs();
		}
		return playerOne.GetCharmItemPrefabs();
	}

	public ShopSceneItem[] GetWeaponItems(PlayerId player)
	{
		if (player == PlayerId.PlayerTwo)
		{
			return playerTwo.GetWeaponItemPrefabs();
		}
		return playerOne.GetWeaponItemPrefabs();
	}

	public bool HasBoughtEverythingForAchievement(PlayerId player)
	{
		ShopSceneItem[] charmItems = GetCharmItems(player);
		for (int i = 0; i < charmItems.Length; i++)
		{
			if (!charmItems[i].isPurchasedForBuyAllItemsAchievement(player))
			{
				return false;
			}
		}
		charmItems = GetWeaponItems(player);
		for (int j = 0; j < charmItems.Length; j++)
		{
			if (!charmItems[j].isPurchasedForBuyAllItemsAchievement(player))
			{
				return false;
			}
		}
		return true;
	}
}
