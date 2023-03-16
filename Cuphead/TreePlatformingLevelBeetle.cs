using System.Collections;
using UnityEngine;

public class TreePlatformingLevelBeetle : PlatformingLevelPathMovementEnemy
{
	[SerializeField]
	private Effect explosion;

	[SerializeField]
	private SpriteRenderer sprite;

	public bool isActivated { get; private set; }

	public bool onCamera { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		isActivated = false;
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(check_hit_box_cr());
		if (sprite != null)
		{
			StartCoroutine(motion_cr());
		}
	}

	protected override void Die()
	{
		if (explosion != null)
		{
			explosion.Create(base.transform.position);
		}
		hasStarted = false;
		onCamera = false;
		AudioManager.Stop("level_platform_beetle_idle_loop");
		AudioManager.Play("level_platform_beetle_death");
		emitAudioFromObject.Add("level_platform_beetle_death");
		GetComponent<SpriteRenderer>().enabled = false;
	}

	public void Activate()
	{
		isActivated = true;
		PrepareBeetle();
	}

	public void Deactivate()
	{
		isActivated = false;
		GetComponent<SpriteRenderer>().enabled = false;
		ResetStartingCondition();
	}

	private void PrepareBeetle()
	{
		pathIndex = 1;
		startPosition = base.allValues[0];
		StartFromCustom();
		StartCoroutine(check_for_death_cr());
		if ((pathIndex - 1) % 2 != 0)
		{
			base.transform.SetScale(-1f);
		}
		else
		{
			base.transform.SetScale(1f);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		GetComponent<SpriteRenderer>().enabled = true;
	}

	protected override void EndPath()
	{
		base.EndPath();
		Deactivate();
	}

	private void Flip()
	{
		base.transform.SetScale(0f - base.transform.localScale.x, base.transform.localScale.y, base.transform.localScale.z);
	}

	public void PlayIdleSFX()
	{
		if (!AudioManager.CheckIfPlaying("level_platform_beetle_idle_loop"))
		{
			AudioManager.PlayLoop("level_platform_beetle_idle_loop");
		}
		emitAudioFromObject.Add("level_platform_beetle_idle_loop");
	}

	private IEnumerator check_for_death_cr()
	{
		while (base.transform.position.y > CupheadLevelCamera.Current.Bounds.yMin - 50f)
		{
			yield return null;
		}
		hasStarted = false;
		onCamera = false;
		yield return null;
	}

	private IEnumerator check_hit_box_cr()
	{
		while (true)
		{
			if (base.transform.position.y > CupheadLevelCamera.Current.Bounds.yMin - 50f && hasStarted)
			{
				sprite.GetComponent<SpriteRenderer>().enabled = true;
				if (base.transform.position.y < CupheadLevelCamera.Current.Bounds.yMax + 50f)
				{
					if (hasStarted)
					{
						onCamera = true;
					}
				}
				else
				{
					onCamera = false;
				}
			}
			else
			{
				sprite.GetComponent<SpriteRenderer>().enabled = false;
			}
			yield return null;
		}
	}

	private IEnumerator motion_cr()
	{
		float time = 0.1f;
		float t = 0f;
		float amount = 2f;
		while (true)
		{
			if (t < time)
			{
				while (!isActivated || PauseManager.state == PauseManager.State.Paused)
				{
					yield return null;
				}
				t += (float)CupheadTime.Delta;
				sprite.transform.AddPosition(0f, amount);
				yield return null;
			}
			else
			{
				t = 0f;
				amount = 0f - amount;
				yield return null;
			}
		}
	}
}
