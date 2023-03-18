using System.Collections.Generic;
using Framework.Managers;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class PatchNotesWidget : BaseMenuScreen
{
	public static Color newPatchNotesColor = Color.white;

	public static Color seenPatchNotesColor = new Color(0.5254902f, 0.4627451f, 0.4f);

	public RectTransform contentTransform;

	public ScrollRect scrollRect;

	public Scrollbar scrollbar;

	public List<GameObject> gameObjectsToHide;

	public List<PatchNotesElement> patchNotesElements;

	public bool isOpen;

	private const int maxNumberOfSkippedFrames = 3;

	private const float axisThreshold = 0.3f;

	private const float delaySecondsForFastScroll = 1f;

	private const float delaySecondsForVeryFastScroll = 2f;

	private const float slowScrollingSpeed = 0.004f;

	private const float fastScrollingSpeed = 0.01f;

	private const float veryFastScrollingSpeed = 0.02f;

	private CanvasGroup canvasGroup;

	private Player rewiredPlayer;

	private int framesSkipped;

	public override void Open()
	{
		base.Open();
		base.gameObject.SetActive(value: true);
		gameObjectsToHide.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: false);
		});
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 1f;
		List<string> patchNotesToBeMarkedAsNew = Core.PatchNotesManager.GetPatchNotesToBeMarkedAsNew();
		foreach (PatchNotesElement patchNotesElement in patchNotesElements)
		{
			if (patchNotesToBeMarkedAsNew.Contains(patchNotesElement.versionText.text))
			{
				patchNotesElement.DisplayAsNew();
			}
			else
			{
				patchNotesElement.DisplayAsSeen();
			}
		}
		rewiredPlayer = ReInput.players.GetPlayer(0);
		isOpen = true;
	}

	public override void Close()
	{
		canvasGroup.alpha = 0f;
		scrollbar.value = 1f;
		scrollRect.verticalNormalizedPosition = 1f;
		framesSkipped = 0;
		isOpen = false;
		gameObjectsToHide.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: true);
		});
		base.gameObject.SetActive(value: false);
		Core.PatchNotesManager.MarkPatchNotesAsSeen();
		base.Close();
	}

	private void Update()
	{
		if (rewiredPlayer == null || !isOpen)
		{
			return;
		}
		if (rewiredPlayer.GetButtonDown(51))
		{
			Close();
			return;
		}
		float axisRaw = rewiredPlayer.GetAxisRaw(49);
		if (Mathf.Abs(axisRaw) > 0.3f)
		{
			ProcessScrollInput(axisRaw);
		}
	}

	private void ProcessScrollInput(float scrollAxis)
	{
		float axisRawPrev = rewiredPlayer.GetAxisRawPrev(49);
		if (axisRawPrev == 0f)
		{
			framesSkipped = 0;
			ScrollContent(scrollAxis, 0.004f);
			return;
		}
		float axisTimeActive = rewiredPlayer.GetAxisTimeActive(49);
		framesSkipped++;
		if (framesSkipped % 3 == 0)
		{
			framesSkipped = 0;
			float scrollingSpeed = ((axisTimeActive > 2f) ? 0.02f : ((!(axisTimeActive > 1f)) ? 0.004f : 0.01f));
			ScrollContent(scrollAxis, scrollingSpeed);
		}
	}

	private void ScrollContent(float scrollAxis, float scrollingSpeed)
	{
		float num = Mathf.Clamp01(scrollbar.value + scrollAxis * scrollingSpeed);
		scrollbar.value = num;
		scrollRect.verticalNormalizedPosition = num;
	}
}
