using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LabelCollapser : MonoBehaviour
{
	public const char COLLAPSE_CHAR = '>';

	public const string COLLAPSE_PREFIX = ">";

	public RectTransform rectToExpandIntoLabelOnCollapse;

	public UnityEvent OnCollapse;

	private string _GetTextString()
	{
		TextMeshProUGUI component = GetComponent<TextMeshProUGUI>();
		if ((bool)component)
		{
			return component.text;
		}
		return GetComponent<Text>().text;
	}

	private void _DisableText()
	{
		((Behaviour)(((object)GetComponent<TextMeshProUGUI>()) ?? ((object)GetComponent<Text>()))).enabled = false;
	}

	private void OnEnable()
	{
		string text = _GetTextString();
		if (text.IsNullOrEmpty() || text[0] == '>')
		{
			RectTransform obj = base.transform as RectTransform;
			Vector2 anchorMin2 = (obj.anchorMax = obj.anchorMin);
			if (rectToExpandIntoLabelOnCollapse != null)
			{
				rectToExpandIntoLabelOnCollapse.anchorMin = anchorMin2;
			}
			_DisableText();
			if (OnCollapse != null)
			{
				OnCollapse.Invoke();
			}
		}
	}
}
