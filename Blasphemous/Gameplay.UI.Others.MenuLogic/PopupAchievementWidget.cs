using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Framework.Achievements;
using Framework.Managers;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class PopupAchievementWidget : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Localize HeaderLoc;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Image SpriteImage;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private RectTransform InitialPos;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private RectTransform EndPos;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private RectTransform PopUp;

	[SerializeField]
	[BoxGroup("Sound", true, false, 0)]
	[EventRef]
	private string ShowSound;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float startDelay = 0.2f;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float animationInTime = 1f;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private Ease animationInEase = Ease.OutQuad;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float popupShowTime = 5f;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float animationOutTime = 1f;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private Ease animationOutEase = Ease.OutQuad;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float endTime = 1f;

	private bool IsShowing;

	private Queue<Achievement> pendingAchievement;

	public void Awake()
	{
		pendingAchievement = new Queue<Achievement>();
		PopUp.localPosition = InitialPos.localPosition;
		IsShowing = false;
	}

	public void ShowPopup(Achievement achievement)
	{
		if (IsShowing)
		{
			pendingAchievement.Enqueue(achievement);
		}
		else
		{
			StartCoroutine(ShowPopupCorrutine(achievement));
		}
	}

	private IEnumerator ShowPopupCorrutine(Achievement achievement)
	{
		IsShowing = true;
		HeaderLoc.SetTerm(achievement.GetNameLocalizationTerm());
		SpriteImage.sprite = achievement.Image;
		Core.Audio.PlayOneShot(ShowSound);
		PopUp.localPosition = InitialPos.localPosition;
		yield return new WaitForSecondsRealtime(startDelay);
		Tweener tween2 = PopUp.DOLocalMove(EndPos.localPosition, animationInTime).SetEase(animationInEase).SetUpdate(isIndependentUpdate: true);
		yield return tween2.WaitForCompletion();
		yield return new WaitForSecondsRealtime(popupShowTime);
		tween2 = PopUp.DOLocalMove(InitialPos.localPosition, animationOutTime).SetEase(animationOutEase).SetUpdate(isIndependentUpdate: true);
		yield return tween2.WaitForCompletion();
		yield return new WaitForSecondsRealtime(endTime);
		if (pendingAchievement.Count > 0)
		{
			StartCoroutine(ShowPopupCorrutine(pendingAchievement.Dequeue()));
		}
		else
		{
			IsShowing = false;
		}
	}
}
