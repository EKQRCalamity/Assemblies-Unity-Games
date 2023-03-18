using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Widgets;

[RequireComponent(typeof(Image))]
public class GlowWidget : UIWidget
{
	private Image image;

	private Color OFF_COLOR = new Color(0f, 0f, 0f, 0f);

	public Color color = new Color(1f, 1f, 1f, 1f);

	private void Start()
	{
		image = GetComponent<Image>();
		image.color = OFF_COLOR;
	}

	public void Show(float duration = 1f, int loops = 1)
	{
		DOTween.Sequence().Append(image.DOColor(color, duration)).Append(image.DOColor(OFF_COLOR, duration))
			.SetLoops(loops)
			.OnComplete(delegate
			{
				color = Color.white;
			})
			.Play();
	}
}
