using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class AchievementToastManager : AbstractMonoBehaviour
{
	public class WaitForSecondsRealtime : CustomYieldInstruction
	{
		private float m_WaitUntilTime = -1f;

		public float waitTime { get; set; }

		public override bool keepWaiting
		{
			get
			{
				if (m_WaitUntilTime < 0f)
				{
					m_WaitUntilTime = Time.realtimeSinceStartup + waitTime;
				}
				bool flag = Time.realtimeSinceStartup < m_WaitUntilTime;
				if (!flag)
				{
					m_WaitUntilTime = -1f;
				}
				return flag;
			}
		}

		public WaitForSecondsRealtime(float time)
		{
			waitTime = time;
		}
	}

	private static readonly int CameraDepth = 91;

	private static readonly Vector2 InitialPosition = new Vector2(0f, -460f);

	private static readonly Vector2 FinalPosition = new Vector2(0f, -280f);

	public static readonly float AnimationDuration = 0.34f;

	private static readonly float HoldDuration = 2f;

	[SerializeField]
	private GameObject uiCameraPrefab;

	[SerializeField]
	private GameObject visual;

	[SerializeField]
	private RectTransform toastTransform;

	[SerializeField]
	private LocalizationHelper titleLocalization;

	[SerializeField]
	private Image icon;

	private List<LocalAchievementsManager.Achievement> queuedAchievements = new List<LocalAchievementsManager.Achievement>();

	private Coroutine currentAnimation;

	private GameObject uiCamera;

	private bool _defaultAtlasCached;

	private SpriteAtlas _defaultAtlas;

	private bool _dlcAtlasCached;

	private SpriteAtlas _dlcAtlas;

	private SpriteAtlas defaultAtlas
	{
		get
		{
			if (!_defaultAtlasCached)
			{
				_defaultAtlas = AssetLoader<SpriteAtlas>.GetCachedAsset("Achievements");
				_defaultAtlasCached = true;
			}
			return _defaultAtlas;
		}
	}

	private SpriteAtlas dlcAtlas
	{
		get
		{
			if (!_dlcAtlasCached && DLCManager.DLCEnabled())
			{
				_dlcAtlas = AssetLoader<SpriteAtlas>.GetCachedAsset("Achievements_DLC");
				_dlcAtlasCached = true;
			}
			return _dlcAtlas;
		}
	}

	private void OnEnable()
	{
		LocalAchievementsManager.AchievementUnlockedEvent += UnlockAchievement;
	}

	private void OnDisable()
	{
		LocalAchievementsManager.AchievementUnlockedEvent -= UnlockAchievement;
	}

	private void Start()
	{
		uiCamera = UnityEngine.Object.Instantiate(uiCameraPrefab);
		uiCamera.transform.SetParent(base.transform);
		uiCamera.transform.ResetLocalTransforms();
		Camera component = uiCamera.GetComponent<Camera>();
		component.cullingMask = 65536;
		component.depth = CameraDepth;
		Canvas componentInChildren = GetComponentInChildren<Canvas>(includeInactive: true);
		componentInChildren.worldCamera = component;
		componentInChildren.sortingLayerName = SpriteLayer.AchievementToast.ToString();
		uiCamera.SetActive(value: false);
	}

	public void UnlockAchievement(LocalAchievementsManager.Achievement achievement)
	{
		if (currentAnimation != null)
		{
			queuedAchievements.Add(achievement);
		}
		else
		{
			currentAnimation = StartCoroutine(showUnlock(achievement));
		}
	}

	private IEnumerator showUnlock(LocalAchievementsManager.Achievement achievement)
	{
		AudioManager.Play("achievement_unlocked");
		string achievementName = achievement.ToString();
		string titleKey = "Achievement" + achievementName + "Toast";
		titleLocalization.ApplyTranslation(Localization.Find(titleKey));
		string spriteName = achievementName + "_toast";
		Sprite sprite = getAtlas(achievement).GetSprite(spriteName);
		icon.sprite = sprite;
		toastTransform.position = InitialPosition;
		visual.SetActive(value: true);
		uiCamera.SetActive(value: true);
		Vector2 displacement = FinalPosition - InitialPosition;
		float elapsed2 = 0f;
		while (elapsed2 < AnimationDuration)
		{
			elapsed2 += Time.unscaledDeltaTime;
			float factor = easeOutBack(elapsed2, 0f, 1f, AnimationDuration);
			toastTransform.localPosition = InitialPosition + factor * displacement;
			yield return null;
		}
		toastTransform.localPosition = FinalPosition;
		yield return new WaitForSecondsRealtime(HoldDuration);
		elapsed2 = 0f;
		while (elapsed2 < AnimationDuration)
		{
			elapsed2 += Time.unscaledDeltaTime;
			float factor2 = easeInBack(elapsed2, 1f, -1f, AnimationDuration);
			toastTransform.localPosition = InitialPosition + factor2 * displacement;
			yield return null;
		}
		if (queuedAchievements.Count > 0)
		{
			LocalAchievementsManager.Achievement achievement2 = queuedAchievements[0];
			queuedAchievements.RemoveAt(0);
			currentAnimation = StartCoroutine(showUnlock(achievement2));
		}
		else
		{
			currentAnimation = null;
			visual.SetActive(value: false);
			uiCamera.SetActive(value: false);
		}
	}

	private float easeOutBack(float t, float initial, float change, float duration)
	{
		float num = 1.70158f;
		return change * ((t = t / duration - 1f) * t * ((num + 1f) * t + num) + 1f) + initial;
	}

	private float easeInBack(float t, float initial, float change, float duration)
	{
		float num = 1.70158f;
		float num2 = (t /= duration);
		return change * num2 * t * ((num + 1f) * t - num) + initial;
	}

	private SpriteAtlas getAtlas(LocalAchievementsManager.Achievement achievement)
	{
		if (Array.IndexOf(LocalAchievementsManager.DLCAchievements, achievement) >= 0)
		{
			return dlcAtlas;
		}
		return defaultAtlas;
	}
}
