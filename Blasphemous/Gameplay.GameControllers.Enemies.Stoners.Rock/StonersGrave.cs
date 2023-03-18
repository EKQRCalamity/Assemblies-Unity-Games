using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Stoners.Rock;

[RequireComponent(typeof(SpriteRenderer))]
public class StonersGrave : MonoBehaviour
{
	private SpriteRenderer _spriteRenderer;

	private float _timeInvisible;

	private void Start()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (!_spriteRenderer.isVisible)
		{
			_timeInvisible += Time.deltaTime;
			if (_timeInvisible >= 2f)
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			_timeInvisible = 0f;
		}
	}
}
