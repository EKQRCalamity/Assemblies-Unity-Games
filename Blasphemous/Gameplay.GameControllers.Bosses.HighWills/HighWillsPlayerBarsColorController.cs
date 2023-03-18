using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.GameControllers.Bosses.HighWills;

[RequireComponent(typeof(Image))]
public class HighWillsPlayerBarsColorController : MonoBehaviour
{
	[Serializable]
	private struct ColorSegment
	{
		public Color StartColor;

		public Color EndColor;

		public Ease Ease;

		public float TweeningTime;
	}

	[SerializeField]
	private List<ColorSegment> ColorSegments = new List<ColorSegment>();

	private Image image;

	private int colorSegmentIndex = -1;

	private bool stoppingColorTweening;

	private void Awake()
	{
		image = GetComponent<Image>();
	}

	private void Start()
	{
		RecursiveColorTweening();
	}

	[BoxGroup("Setup Button", true, false, 0)]
	[InfoBox("Use this button to add more color segments,as it automatically sets the Starting Color of the new segment and the same Ease as the previous one.", InfoMessageType.Info, null)]
	[Button(ButtonSizes.Small)]
	private void AddNewColorSegment()
	{
		ColorSegment item = default(ColorSegment);
		if (ColorSegments.Count > 0)
		{
			item.StartColor = ColorSegments[ColorSegments.Count - 1].EndColor;
			item.TweeningTime = ColorSegments[ColorSegments.Count - 1].TweeningTime;
			item.Ease = ColorSegments[ColorSegments.Count - 1].Ease;
		}
		ColorSegments.Add(item);
	}

	[BoxGroup("Debugging Buttons", true, false, 0)]
	[InfoBox("This button serves debugging purpouses, as the Gameobject should be deactivated when not in use.", InfoMessageType.Info, null)]
	[Button(ButtonSizes.Small)]
	public void StopColorTweening()
	{
		stoppingColorTweening = true;
	}

	[BoxGroup("Debugging Buttons", true, false, 0)]
	[InfoBox("This button serves debugging purpouses, as the color tweening should start with the Gameobject's Start.", InfoMessageType.Info, null)]
	[Button(ButtonSizes.Small)]
	public void StartColorTweening()
	{
		RecursiveColorTweening();
	}

	private void RecursiveColorTweening()
	{
		if (stoppingColorTweening)
		{
			stoppingColorTweening = false;
			return;
		}
		colorSegmentIndex++;
		if (colorSegmentIndex == ColorSegments.Count)
		{
			colorSegmentIndex = 0;
		}
		ColorSegment colorSegment = ColorSegments[colorSegmentIndex];
		image.color = colorSegment.StartColor;
		image.DOColor(colorSegment.EndColor, colorSegment.TweeningTime).SetEase(colorSegment.Ease).OnComplete(RecursiveColorTweening);
	}
}
