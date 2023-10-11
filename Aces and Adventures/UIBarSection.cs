using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIBarSection : AUIBarSection
{
	[SerializeField]
	protected Image _referenceBar;

	private Image _image;

	public Image referenceBar
	{
		get
		{
			return _referenceBar;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _referenceBar, value))
			{
				_SetDirty();
			}
		}
	}

	public Image image
	{
		get
		{
			if (!_image)
			{
				return _image = GetComponent<Image>();
			}
			return _image;
		}
	}

	public override Graphic graphic => image;

	protected override void _UpdateLayoutUnique()
	{
		if ((bool)referenceBar && referenceBar.fillMethod <= Image.FillMethod.Vertical)
		{
			int fillMethod = (int)referenceBar.fillMethod;
			int index = 1 - fillMethod;
			Vector2 v = ((referenceBar.fillOrigin == 0) ? base.normalizedMinMax : base.normalizedMinMax.OneMinus());
			Vector2 anchorMin = default(Vector2);
			anchorMin[index] = 0f;
			anchorMin[fillMethod] = v.Min();
			base.rect.anchorMin = anchorMin;
			Vector2 anchorMax = default(Vector2);
			anchorMax[index] = 1f;
			anchorMax[fillMethod] = v.Max();
			base.rect.anchorMax = anchorMax;
		}
	}
}
