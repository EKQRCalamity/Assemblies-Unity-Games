using UnityEngine;

public class ColorIndexer : MonoBehaviour
{
	public Color[] colors = new Color[3]
	{
		Color.white,
		Color.white,
		Color.white
	};

	[SerializeField]
	protected ColorEvent _onColorChange;

	public ColorEvent onColorChange => _onColorChange ?? (_onColorChange = new ColorEvent());

	public void SetColorIndex(int index)
	{
		if (colors.Length != 0)
		{
			onColorChange.Invoke(colors[Mathf.Clamp(index, 0, colors.Length - 1)]);
		}
	}
}
