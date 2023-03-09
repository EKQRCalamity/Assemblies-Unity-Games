using UnityEngine;

public class SallyStagePlayLevelBowingAnimation : AbstractPausableComponent
{
	[SerializeField]
	private Animator[] animators;

	private int counter;

	private int maxCounter;

	private void Start()
	{
		Level.Current.OnWinEvent += OnDeath;
	}

	private void PickNumber()
	{
		maxCounter = Random.Range(12, 21);
	}

	private void Counter()
	{
		if (counter < maxCounter)
		{
			counter++;
			return;
		}
		Animator[] array = animators;
		foreach (Animator animator in array)
		{
			animator.SetTrigger("OnBow");
			counter = 0;
		}
	}

	private void OnDeath()
	{
		Animator[] array = animators;
		foreach (Animator animator in array)
		{
			animator.SetTrigger("OnDeath");
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Level.Current.OnWinEvent -= OnDeath;
	}
}
