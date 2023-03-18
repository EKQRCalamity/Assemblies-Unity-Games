using System.Collections.Generic;
using Framework.FrameworkCore.Attributes;
using Framework.Inventory;
using Framework.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.UIGameLogic;

public class PlayerFlask : MonoBehaviour
{
	[SerializeField]
	public List<Sprite> flasksFull;

	[SerializeField]
	public List<Sprite> flasksEmpty;

	[SerializeField]
	public List<Sprite> flasksFullFervour;

	private List<Image> flasks = new List<Image>();

	private float currentFlaskNumber = -1f;

	private float currentFlaskFull = -1f;

	private float currentFlaskLevel = -1f;

	private bool currentFlaskIsFervour;

	private Sword swordHeart06;

	private void Start()
	{
		int num = 0;
		Transform transform = null;
		do
		{
			transform = base.transform.Find("Flask" + num);
			if ((bool)transform)
			{
				flasks.Add(transform.GetComponent<Image>());
			}
			num++;
		}
		while (transform != null);
		RefreshFlask();
	}

	private void Update()
	{
		RefreshFlask();
	}

	private void RefreshFlask()
	{
		if (Core.Logic == null || Core.Logic.Penitent == null)
		{
			return;
		}
		Flask flask = Core.Logic.Penitent.Stats.Flask;
		int num = (int)(Core.Logic.Penitent.Stats.FlaskHealth.PermanetBonus / Core.Logic.Penitent.Stats.FlaskHealthUpgrade);
		if (num > flasksEmpty.Count)
		{
			Debug.LogError("PlayerFlask::RefreshFlask: You have upgraded the flasks more than " + flasksEmpty.Count + " times and we don't have sprites for those upgraded flasks! Current flask upgrade level: " + num);
			num = flasksEmpty.Count;
		}
		if (swordHeart06 == null)
		{
			swordHeart06 = Core.InventoryManager.GetSword("HE06");
		}
		if (swordHeart06 != null && swordHeart06.IsEquiped)
		{
			for (int i = 0; i < flasks.Count; i++)
			{
				flasks[i].gameObject.SetActive(value: false);
			}
			flask.Current = 0f;
		}
		else
		{
			if (currentFlaskNumber == flask.Final && currentFlaskFull == flask.Current && currentFlaskLevel == (float)num && flasks[0].gameObject.activeInHierarchy && currentFlaskIsFervour == Core.PenitenceManager.UseFervourFlasks)
			{
				return;
			}
			currentFlaskIsFervour = Core.PenitenceManager.UseFervourFlasks;
			currentFlaskNumber = flask.Final;
			currentFlaskFull = flask.Current;
			currentFlaskLevel = num;
			for (int j = 0; j < flasks.Count; j++)
			{
				if ((float)j < currentFlaskFull)
				{
					if (Core.PenitenceManager.UseFervourFlasks)
					{
						flasks[j].sprite = flasksFullFervour[num];
					}
					else
					{
						flasks[j].sprite = flasksFull[num];
					}
					flasks[j].gameObject.SetActive(value: true);
				}
				else if ((float)j < currentFlaskNumber)
				{
					flasks[j].sprite = flasksEmpty[num];
					flasks[j].gameObject.SetActive(value: true);
				}
				else
				{
					flasks[j].gameObject.SetActive(value: false);
				}
			}
		}
	}
}
