using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class WildValueSelectionItem : MonoBehaviour
{
	[SerializeField]
	protected BoolEvent _onIsNaturalValueChange;

	[SerializeField]
	[HideInInspector]
	private bool _isNaturalValue;

	private Toggle _toggle;

	public Toggle toggle => this.CacheComponent(ref _toggle);

	public BoolEvent onIsNaturalValueChange => _onIsNaturalValueChange ?? (_onIsNaturalValueChange = new BoolEvent());

	public bool isNaturalValue
	{
		get
		{
			return _isNaturalValue;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _isNaturalValue, value))
			{
				onIsNaturalValueChange.Invoke(value);
			}
		}
	}

	public bool isOn
	{
		get
		{
			return toggle.isOn;
		}
		set
		{
			toggle.isOn = value;
		}
	}

	public Toggle.ToggleEvent onValueChanged => toggle.onValueChanged;

	public WildValueSelectionItem SetIsNatural(bool isNatural)
	{
		isNaturalValue = isNatural;
		return this;
	}
}
