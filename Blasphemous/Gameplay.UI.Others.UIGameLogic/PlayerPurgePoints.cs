using System;
using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.FrameworkCore.Attributes;
using Framework.Managers;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.UIGameLogic;

public class PlayerPurgePoints : SerializedMonoBehaviour
{
	public const float EXECUTION_PURGE_BONUS = 0.5f;

	public const string PurgePointUpdateFx = "event:/SFX/UI/PurgeCounter";

	public Text text;

	public float animationDuration = 1f;

	public PlayerGuiltPanel guiltPanel;

	public bool IsDemakeVersion;

	public bool IsAffectedByGameMode = true;

	[SerializeField]
	private Image background;

	[OdinSerialize]
	private List<Sprite> levelList = new List<Sprite>();

	private float currentPoints;

	private float targetPoints;

	private float numbersPerSeconds;

	private Coroutine refreshCoroutine;

	private void Awake()
	{
		currentPoints = 0f;
		targetPoints = 0f;
		text.text = "0";
		RefreshPoints(inmediate: true);
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		if (IsAffectedByGameMode && ((IsDemakeVersion && !Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE)) || (!IsDemakeVersion && Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE))))
		{
			base.gameObject.SetActive(value: false);
		}
		if (newLevel.LevelName.StartsWith("Main"))
		{
			currentPoints = 0f;
			targetPoints = 0f;
			text.text = "0";
		}
		if (Core.Logic != null && Core.Logic.Penitent != null)
		{
			Core.Logic.Penitent.Stats.Purge.OnChanged += RunRefreshPointsCoroutine;
		}
		RefreshPoints(inmediate: true);
	}

	private void Start()
	{
		RefreshPoints(inmediate: true);
	}

	private void RunRefreshPointsCoroutine()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (refreshCoroutine != null)
			{
				StopCoroutine(refreshCoroutine);
			}
			refreshCoroutine = StartCoroutine(RefreshPointsCoroutine());
		}
	}

	private IEnumerator RefreshPointsCoroutine()
	{
		if (Core.Logic == null || Core.Logic.Penitent == null || Core.Logic.Penitent.Stats == null)
		{
			yield break;
		}
		Purge points = Core.Logic.Penitent.Stats.Purge;
		if (points != null)
		{
			if (Math.Abs(targetPoints - points.Current) > Mathf.Epsilon)
			{
				targetPoints = points.Current;
				numbersPerSeconds = (targetPoints - currentPoints) / animationDuration;
				PlayPurgePointsFx();
			}
			float timeLeft = animationDuration;
			float inc = numbersPerSeconds * Time.unscaledDeltaTime;
			while (timeLeft > 0f)
			{
				currentPoints += inc;
				timeLeft -= Time.unscaledDeltaTime;
				text.text = ((int)currentPoints).ToString();
				yield return null;
			}
			currentPoints = targetPoints;
			text.text = ((int)currentPoints).ToString();
		}
	}

	public void RefreshGuilt(bool whenDead)
	{
		if (whenDead)
		{
			StartCoroutine(RefreshGuiltWhenDead());
		}
		else
		{
			guiltPanel.SetGuiltLevel(Core.GuiltManager.GetDropsCount());
		}
	}

	private IEnumerator RefreshGuiltWhenDead()
	{
		guiltPanel.SetGuiltLevel(0, instantly: true);
		yield return new WaitForSeconds(2f);
		guiltPanel.SetGuiltLevel(Core.GuiltManager.GetDropsCount());
	}

	public void RefreshPoints(bool inmediate)
	{
		if (Core.Logic == null || Core.Logic.Penitent == null || Core.Logic.Penitent.Stats == null)
		{
			return;
		}
		Purge purge = Core.Logic.Penitent.Stats.Purge;
		if (purge == null)
		{
			return;
		}
		if (Math.Abs(targetPoints - purge.Current) > Mathf.Epsilon)
		{
			targetPoints = purge.Current;
			numbersPerSeconds = (targetPoints - currentPoints) / animationDuration;
			PlayPurgePointsFx();
		}
		if (inmediate)
		{
			currentPoints = targetPoints;
			numbersPerSeconds = 0f;
			text.text = ((int)currentPoints).ToString();
		}
		else if (!(Math.Abs(targetPoints - currentPoints) < Mathf.Epsilon))
		{
			float f = targetPoints - currentPoints;
			float num = numbersPerSeconds * Time.unscaledDeltaTime;
			if (Mathf.Abs(num) < Mathf.Abs(f))
			{
				currentPoints += num;
			}
			else
			{
				currentPoints = targetPoints;
				numbersPerSeconds = 0f;
			}
			text.text = ((int)currentPoints).ToString();
		}
	}

	private void PlayPurgePointsFx()
	{
		if (!string.IsNullOrEmpty("event:/SFX/UI/PurgeCounter"))
		{
			Core.Audio.PlaySfx("event:/SFX/UI/PurgeCounter");
		}
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}
}
