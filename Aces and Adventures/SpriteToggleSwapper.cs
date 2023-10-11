using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteToggleSwapper : MonoBehaviour
{
	public Sprite onSprite;

	public Sprite offSprite;

	private bool? _isOn;

	public bool isOn
	{
		get
		{
			return _isOn.GetValueOrDefault();
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _isOn, value))
			{
				_UpdateSprite();
			}
		}
	}

	private void _UpdateSprite()
	{
		GetComponent<Image>().sprite = (isOn ? onSprite : offSprite);
	}
}
