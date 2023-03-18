using System.Collections.Generic;
using UnityEngine;

public class SetRandomSprite : MonoBehaviour
{
	public List<Sprite> sprites;

	public SpriteRenderer sprRenderer;

	public void OnEnable()
	{
		sprRenderer.sprite = sprites[Random.Range(0, sprites.Count)];
	}
}
