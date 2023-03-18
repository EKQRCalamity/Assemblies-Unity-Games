using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Widgets;

public class CustomScrollBar : MonoBehaviour
{
	[SerializeField]
	private Image filledImage;

	[SerializeField]
	private RectTransform slidingdBar;

	[SerializeField]
	private RectTransform pointer;

	[SerializeField]
	[OnValueChanged("OnEditorChanged", false)]
	private float initialValue;

	private float _currentValue;

	public float CurrentValue
	{
		get
		{
			return _currentValue;
		}
		set
		{
			_currentValue = Mathf.Clamp01(value);
			if (filledImage != null && pointer != null && slidingdBar != null)
			{
				filledImage.fillAmount = _currentValue;
				float x = slidingdBar.sizeDelta.x * (_currentValue - 0.5f);
				pointer.localPosition = new Vector3(x, 0f, 0f);
			}
		}
	}

	private void OnEditorChanged()
	{
		CurrentValue = initialValue;
	}

	private void Awake()
	{
		CurrentValue = initialValue;
	}
}
