using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DG.Tweening;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.UI.Others.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class UpgradeFlasksWidget : BaseMenuScreen
{
	public enum UpgradeFlasksAnswer
	{
		None,
		Upgrade,
		Back
	}

	public GameObject errorMessageGameObject;

	public GameObject upgradeMessageGameObject;

	public Text qiText;

	public Text flaskText;

	public Text priceText;

	public Color defaultColor;

	public Color errorColor;

	private readonly List<string> givableQuestItemIds = new List<string>(new string[5] { "QI101", "QI102", "QI103", "QI104", "QI105" });

	private CanvasGroup canvasGroup;

	private Action onUpgradeFlask;

	private Action onContinueWithoutUpgrading;

	private float price;

	private UpgradeFlasksAnswer chosenAnswer;

	public void Open(float price, Action onUpgradeFlask, Action onContinueWithoutUpgrading)
	{
		this.price = price;
		this.onUpgradeFlask = onUpgradeFlask;
		this.onContinueWithoutUpgrading = onContinueWithoutUpgrading;
		Open();
	}

	public override void Open()
	{
		base.Open();
		base.gameObject.SetActive(value: true);
		canvasGroup = GetComponent<CanvasGroup>();
		UpdateQIText();
		UpdateFlaskText();
		UpdatePriceText();
		UpdateAnswerMessages();
		chosenAnswer = UpgradeFlasksAnswer.None;
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 1f, 1f);
	}

	public override void Close()
	{
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 0f, 1f).OnComplete(OnClose);
		base.Close();
	}

	protected override void OnClose()
	{
		if (chosenAnswer == UpgradeFlasksAnswer.Upgrade)
		{
			onUpgradeFlask();
		}
		else
		{
			onContinueWithoutUpgrading();
		}
		base.gameObject.SetActive(value: false);
	}

	public void Option_UpgradeFlasks()
	{
		if (!(canvasGroup.alpha < 0.9f) && chosenAnswer == UpgradeFlasksAnswer.None)
		{
			chosenAnswer = UpgradeFlasksAnswer.Upgrade;
			Close();
		}
	}

	public void Option_Back()
	{
		if (!(canvasGroup.alpha < 0.9f) && chosenAnswer == UpgradeFlasksAnswer.None)
		{
			chosenAnswer = UpgradeFlasksAnswer.Back;
			Close();
		}
	}

	private void UpdateQIText()
	{
		bool flag = CanGiveQuestItem();
		qiText.color = ((!flag) ? errorColor : defaultColor);
	}

	private void UpdateFlaskText()
	{
		bool flag = CanSacrificeFlask();
		flaskText.color = ((!flag) ? errorColor : defaultColor);
	}

	private void UpdatePriceText()
	{
		bool flag = CanAffordUpgrade();
		priceText.text = price.ToString();
		priceText.color = ((!flag) ? errorColor : defaultColor);
	}

	private void UpdateAnswerMessages()
	{
		bool flag = IsFlasksUpgradePossible();
		errorMessageGameObject.SetActive(!flag);
		upgradeMessageGameObject.SetActive(flag);
		upgradeMessageGameObject.GetComponent<MenuButton>().OnDeselect(null);
	}

	private bool CanGiveQuestItem()
	{
		bool result = false;
		ReadOnlyCollection<QuestItem> questItemOwned = Core.InventoryManager.GetQuestItemOwned();
		foreach (string givableQuestItemId in givableQuestItemIds)
		{
			QuestItem questItem = Core.InventoryManager.GetQuestItem(givableQuestItemId);
			if (questItem != null && questItemOwned.Contains(questItem))
			{
				return true;
			}
		}
		return result;
	}

	private bool CanSacrificeFlask()
	{
		return Core.Logic.Penitent.Stats.Flask.PermanetBonus > -1f;
	}

	private bool CanAffordUpgrade()
	{
		return price <= Core.Logic.Penitent.Stats.Purge.Current;
	}

	private bool IsFlasksUpgradePossible()
	{
		return CanAffordUpgrade() && CanSacrificeFlask() && CanGiveQuestItem();
	}
}
