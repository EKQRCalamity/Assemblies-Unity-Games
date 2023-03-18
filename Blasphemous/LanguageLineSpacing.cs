using Framework.FrameworkCore;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageLineSpacing : MonoBehaviour
{
	private Text textComponent;

	private TextMeshProUGUI textProComponent;

	private float textLineSpacing;

	private float textMeshProLineSpacing;

	public void Start()
	{
		textComponent = null;
		textProComponent = null;
		LocalizationManager.OnLocalizeEvent += OnLocalize;
		textComponent = GetComponent<Text>();
		if (textComponent != null)
		{
			textLineSpacing = textComponent.lineSpacing;
		}
		else
		{
			textProComponent = GetComponent<TextMeshProUGUI>();
			if (textProComponent != null)
			{
				textMeshProLineSpacing = textProComponent.lineSpacing;
			}
		}
		OnLocalize();
	}

	private void OnDestroy()
	{
		LocalizationManager.OnLocalizeEvent -= OnLocalize;
	}

	private void OnLocalize()
	{
		if (textComponent != null)
		{
			float num = 1f;
			if (GameConstants.LanguageLineSpacingFactor.ContainsKey(LocalizationManager.CurrentLanguageCode))
			{
				num = GameConstants.LanguageLineSpacingFactor[LocalizationManager.CurrentLanguageCode];
			}
			textComponent.lineSpacing = textLineSpacing * num;
		}
		if (textProComponent != null)
		{
			float num2 = 0f;
			if (GameConstants.LanguageLineSpacingTextPro.ContainsKey(LocalizationManager.CurrentLanguageCode))
			{
				num2 = GameConstants.LanguageLineSpacingTextPro[LocalizationManager.CurrentLanguageCode];
			}
			textProComponent.lineSpacing = textMeshProLineSpacing + num2;
		}
	}
}
