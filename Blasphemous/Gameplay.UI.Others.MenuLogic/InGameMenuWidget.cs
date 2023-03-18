using System;
using System.Collections.Generic;
using Framework.Managers;
using Gameplay.UI.Widgets;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class InGameMenuWidget : MonoBehaviour
{
	private Animator animator;

	public GameObject firstSelected;

	public bool currentlyActive { get; private set; }

	private void Awake()
	{
		animator = GetComponent<Animator>();
		currentlyActive = false;
	}

	public void Show(bool b)
	{
		if (!(SceneManager.GetActiveScene().name == "MainMenu") && !FadeWidget.instance.Fading && !UIController.instance.Paused && (!b || !Core.Input.InputBlocked || Core.Input.HasBlocker("PLAYER_LOGIC")))
		{
			Array.ForEach(GetComponentsInChildren<Button>(), delegate(Button bto)
			{
				bto.OnDeselect(null);
			});
			Core.Input.SetBlocker("INGAME_MENU", b);
			currentlyActive = b;
			if (b)
			{
				Core.Logic.SetState(LogicStates.Unresponsive);
				EventSystem.current.SetSelectedGameObject(firstSelected);
			}
			else
			{
				Core.Logic.SetState(LogicStates.Playing);
			}
			animator.SetBool("ACTIVE", b);
		}
	}

	public void MainMenu()
	{
		if (currentlyActive)
		{
			Show(b: false);
			Core.Logic.LoadMenuScene();
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Scene", SceneManager.GetActiveScene().name);
			Analytics.CustomEvent("QUIT_GAME", dictionary);
		}
	}

	public void Reset()
	{
		if (currentlyActive)
		{
			Show(b: false);
			Core.Logic.Penitent.KillInstanteneously();
		}
	}

	public void Configuration()
	{
		if (currentlyActive)
		{
			Show(b: false);
		}
	}

	public void Exit()
	{
		if (!currentlyActive)
		{
		}
	}

	public void ChangeLanguage()
	{
		Core.Localization.SetNextLanguage();
	}
}
