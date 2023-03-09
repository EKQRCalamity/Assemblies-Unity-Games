using System.Collections;
using System.Linq;
using UnityEngine;

public class MouseLevelSawBladeManager : AbstractPausableComponent
{
	public enum State
	{
		Init,
		Idle,
		Warning,
		Attack
	}

	[SerializeField]
	private MouseLevelSawBladeSide leftSawBlades;

	[SerializeField]
	private MouseLevelSawBladeSide rightSawBlades;

	private LevelProperties.Mouse properties;

	public void Begin(LevelProperties.Mouse properties)
	{
		this.properties = properties;
		leftSawBlades.Begin(properties);
		rightSawBlades.Begin(properties);
		StartCoroutine(pattern_cr());
		StartCoroutine(fullAttack_cr());
	}

	public void Leave()
	{
		StopAllCoroutines();
		leftSawBlades.Leave();
		rightSawBlades.Leave();
	}

	private IEnumerator pattern_cr()
	{
		LevelProperties.Mouse.State patternState = properties.CurrentState;
		if (patternState.brokenCanSawBlades.patternString.Length == 0)
		{
			yield break;
		}
		string patternString2 = patternState.brokenCanSawBlades.patternString.RandomChoice();
		leftSawBlades.SetPattern(patternString2);
		rightSawBlades.SetPattern(patternString2);
		while (true)
		{
			if (properties.CurrentState != patternState)
			{
				if (!patternState.brokenCanSawBlades.patternString.SequenceEqual(properties.CurrentState.brokenCanSawBlades.patternString))
				{
					patternString2 = properties.CurrentState.brokenCanSawBlades.patternString.RandomChoice();
					leftSawBlades.SetPattern(patternString2);
					rightSawBlades.SetPattern(patternString2);
				}
				patternState = properties.CurrentState;
			}
			yield return null;
		}
	}

	private IEnumerator fullAttack_cr()
	{
		while (true)
		{
			if (leftSawBlades.state == MouseLevelSawBladeSide.State.Pattern && rightSawBlades.state == MouseLevelSawBladeSide.State.Pattern)
			{
				yield return CupheadTime.WaitForSeconds(this, properties.CurrentState.brokenCanSawBlades.fullAttackTime.RandomFloat());
				AbstractPlayerController player = PlayerManager.GetNext();
				if (!(player.transform.position.x > 0f))
				{
					leftSawBlades.FullAttack();
				}
				else
				{
					rightSawBlades.FullAttack();
				}
			}
			else
			{
				yield return null;
			}
		}
	}
}
