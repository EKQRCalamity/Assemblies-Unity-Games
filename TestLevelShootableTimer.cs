using System.Collections;
using UnityEngine;

public class TestLevelShootableTimer : AbstractCollidableObject
{
	[SerializeField]
	private float maxTime = 3f;

	[SerializeField]
	private DamageReceiver child;

	private DamageReceiver damageReceiver;

	private float damageTaken;

	private bool timerStarted;

	private void Start()
	{
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		child.OnDamageTaken += OnDamageTaken;
		StartCoroutine(timer_cr());
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			timerStarted = true;
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (timerStarted)
		{
			damageTaken += info.damage;
		}
	}

	private IEnumerator timer_cr()
	{
		while (true)
		{
			float t = 0f;
			if (timerStarted)
			{
				while (t < maxTime)
				{
					t += (float)CupheadTime.Delta;
					yield return null;
				}
				yield return null;
				damageTaken = 0f;
				timerStarted = false;
			}
			yield return null;
		}
	}
}
