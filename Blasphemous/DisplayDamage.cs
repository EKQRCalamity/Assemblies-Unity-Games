using Gameplay.GameControllers.Entities;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDamage : MonoBehaviour
{
	public Entity entity;

	public Text text;

	public float resetTime = 2f;

	private float accumDamage;

	private float lastHitTime;

	private int totalHits;

	private void Awake()
	{
		entity.OnDamageTaken += Entity_OnDamaged;
		text = GetComponent<Text>();
	}

	private void Entity_OnDamaged(float dmg)
	{
		if (Time.time > lastHitTime + resetTime)
		{
			accumDamage = 0f;
			totalHits = 0;
		}
		totalHits++;
		accumDamage += dmg;
		text.text = dmg.ToString("0000");
		text.text += $"\n<color=red>{accumDamage:0000} ({totalHits})</color>";
		lastHitTime = Time.time;
	}
}
