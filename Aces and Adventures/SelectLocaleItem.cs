using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SelectLocaleItem : MonoBehaviour
{
	[SerializeField]
	protected Locale _locale;

	public LocaleEvent onLocaleChange;

	private Button _button;

	public Locale locale
	{
		get
		{
			return _locale;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _locale, value))
			{
				_OnLocaleChange();
			}
		}
	}

	public Button button => this.CacheComponent(ref _button);

	private void _OnLocaleChange()
	{
		if ((bool)locale)
		{
			onLocaleChange?.Invoke(locale);
		}
	}

	private void Awake()
	{
		_OnLocaleChange();
		button.onClick.AddListener(delegate
		{
			if ((bool)locale)
			{
				LocalizationSettings.SelectedLocale = locale;
			}
		});
	}
}
