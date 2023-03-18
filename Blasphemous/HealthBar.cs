using Gameplay.GameControllers.Entities;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
	public SpriteRenderer healthBar;

	public SpriteRenderer missingHealthBar;

	public Enemy attachedEnemy;

	private void Start()
	{
		attachedEnemy = GetComponentInParent<Enemy>();
	}

	private void Update()
	{
		float x = Mathf.Lerp(healthBar.transform.localScale.x, attachedEnemy.Stats.Life.MissingRatio, 0.5f);
		float x2 = Mathf.Lerp(missingHealthBar.transform.localScale.x, attachedEnemy.Stats.Life.MissingRatio, 0.1f);
		healthBar.transform.localScale = new Vector2(x, healthBar.transform.localScale.y);
		missingHealthBar.transform.localScale = new Vector2(x2, missingHealthBar.transform.localScale.y);
	}
}
