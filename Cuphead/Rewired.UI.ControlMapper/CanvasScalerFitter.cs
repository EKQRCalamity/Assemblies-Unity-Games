using System;
using Rewired.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[RequireComponent(typeof(CanvasScalerExt))]
public class CanvasScalerFitter : MonoBehaviour
{
	[Serializable]
	private class BreakPoint
	{
		[SerializeField]
		public string name;

		[SerializeField]
		public float screenAspectRatio;

		[SerializeField]
		public Vector2 referenceResolution;
	}

	[SerializeField]
	private BreakPoint[] breakPoints;

	private CanvasScalerExt canvasScaler;

	private int screenWidth;

	private int screenHeight;

	private Action ScreenSizeChanged;

	private void OnEnable()
	{
		canvasScaler = GetComponent<CanvasScalerExt>();
		Update();
		canvasScaler.ForceRefresh();
	}

	private void Update()
	{
		if (Screen.width != screenWidth || Screen.height != screenHeight)
		{
			screenWidth = Screen.width;
			screenHeight = Screen.height;
			UpdateSize();
		}
	}

	private void UpdateSize()
	{
		if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize || breakPoints == null)
		{
			return;
		}
		float num = (float)Screen.width / (float)Screen.height;
		float num2 = float.PositiveInfinity;
		int num3 = 0;
		for (int i = 0; i < breakPoints.Length; i++)
		{
			float num4 = Mathf.Abs(num - breakPoints[i].screenAspectRatio);
			if ((!(num4 > breakPoints[i].screenAspectRatio) || MathTools.IsNear(breakPoints[i].screenAspectRatio, 0.01f)) && num4 < num2)
			{
				num2 = num4;
				num3 = i;
			}
		}
		canvasScaler.referenceResolution = breakPoints[num3].referenceResolution;
	}
}
