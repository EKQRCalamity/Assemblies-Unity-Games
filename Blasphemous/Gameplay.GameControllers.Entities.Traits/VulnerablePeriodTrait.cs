using System.Collections;
using Framework.FrameworkCore;
using UnityEngine;

namespace Gameplay.GameControllers.Entities.Traits;

public class VulnerablePeriodTrait : Trait
{
	public float vulnerablePeriodDuration;

	private IEnumerator VulnerablePeriod(Enemy e, float seconds)
	{
		e.IsVulnerable = true;
		float counter = 0f;
		while (counter < seconds)
		{
			counter += Time.deltaTime;
			yield return null;
		}
		e.IsVulnerable = false;
	}

	internal void StartVulnerablePeriod(Enemy e)
	{
		StartCoroutine(VulnerablePeriod(e, vulnerablePeriodDuration));
	}
}
