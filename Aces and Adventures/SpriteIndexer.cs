using UnityEngine;
using UnityEngine.UI;

public class SpriteIndexer : MonoBehaviour
{
	public Sprite[] sprites;

	public SpriteEvent OnSpriteChange;

	public void SetSpriteIndex(int index)
	{
		if (!sprites.IsNullOrEmpty())
		{
			Sprite sprite = sprites[Mathf.Clamp(index, 0, sprites.Length - 1)];
			Image component = GetComponent<Image>();
			if ((object)component != null)
			{
				component.sprite = sprite;
			}
			UIGlow component2 = GetComponent<UIGlow>();
			if ((object)component2 != null)
			{
				component2.sprite = sprite;
			}
			OnSpriteChange?.Invoke(sprite);
		}
	}

	public void SetSpriteToggle(bool toggle)
	{
		SetSpriteIndex(toggle.ToInt(0, 1));
	}
}
