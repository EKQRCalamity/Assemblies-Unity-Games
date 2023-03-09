using System.Collections;
using UnityEngine;

public class ChaliceTutorialLevel : Level
{
	private LevelProperties.ChaliceTutorial properties;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	[SerializeField]
	private Animator backgroundAnimator;

	[SerializeField]
	private ChaliceTutorialLevelParryable[] parrybles;

	private bool finishedPuzzle;

	public bool resetParryables;

	[SerializeField]
	private PlayerDeathEffect[] playerExitEffects;

	public override Levels CurrentLevel => Levels.ChaliceTutorial;

	public override Scenes CurrentScene => Scenes.scene_level_chalice_tutorial;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.ChaliceTutorial.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void Start()
	{
		base.Start();
		foreach (AbstractPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null)
			{
				Transform[] componentsInChildren = allPlayer.GetComponentsInChildren<Transform>();
				foreach (Transform transform in componentsInChildren)
				{
					transform.gameObject.layer = 31;
				}
			}
		}
		StartCoroutine(parryables_cr());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	protected override void OnLevelStart()
	{
		StartCoroutine(intro_cr());
		StartCoroutine(chalicetutorialPattern_cr());
	}

	private IEnumerator intro_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		backgroundAnimator.Play("Zoom");
	}

	private IEnumerator parryables_cr()
	{
		while (true)
		{
			for (int j = 0; j < parrybles.Length; j++)
			{
				parrybles[j].Deactivated();
			}
			while (!backgroundAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				Effect[] es = Object.FindObjectsOfType<Effect>();
				Effect[] array = es;
				foreach (Effect effect in array)
				{
					effect.gameObject.layer = 31;
					if (effect.transform.childCount > 0)
					{
						Transform[] childTransforms = effect.transform.GetChildTransforms();
						foreach (Transform transform in childTransforms)
						{
							transform.gameObject.layer = 31;
						}
					}
				}
				AbstractProjectile[] ps = Object.FindObjectsOfType<AbstractProjectile>();
				AbstractProjectile[] array2 = ps;
				foreach (AbstractProjectile abstractProjectile in array2)
				{
					abstractProjectile.gameObject.layer = 31;
					if (abstractProjectile.transform.childCount > 0)
					{
						Transform[] childTransforms2 = abstractProjectile.transform.GetChildTransforms();
						foreach (Transform transform2 in childTransforms2)
						{
							transform2.gameObject.layer = 31;
						}
					}
				}
				PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: false, promptBeforeJoin: false);
				yield return null;
			}
			PlayerManager.SetPlayerCanJoin(PlayerId.PlayerTwo, canJoin: true, promptBeforeJoin: true);
			for (int i = 0; i < parrybles.Length; i++)
			{
				parrybles[i].Activated();
				while (!parrybles[i].isDeactivated && !resetParryables)
				{
					yield return null;
				}
				if (resetParryables)
				{
					break;
				}
			}
			resetParryables = false;
			yield return null;
		}
	}

	public void Exit()
	{
		AbstractPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		playerExitEffects[0].gameObject.SetActive(value: true);
		playerExitEffects[0].transform.position = player.transform.position;
		player.gameObject.SetActive(value: false);
		playerExitEffects[0].animator.SetTrigger("OnStartTutorial");
		player = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		if (player != null)
		{
			playerExitEffects[1].gameObject.SetActive(value: true);
			playerExitEffects[1].transform.position = player.transform.position;
			player.gameObject.SetActive(value: false);
			playerExitEffects[1].animator.SetTrigger("OnStartTutorial");
		}
	}

	private IEnumerator chalicetutorialPattern_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		while (true)
		{
			yield return StartCoroutine(nextPattern_cr());
			yield return null;
		}
	}

	private IEnumerator nextPattern_cr()
	{
		LevelProperties.ChaliceTutorial.Pattern p = properties.CurrentState.NextPattern;
		yield return CupheadTime.WaitForSeconds(this, 1f);
	}

	private void WORKAROUND_NullifyFields()
	{
		_bossPortrait = null;
		_bossQuote = null;
		backgroundAnimator = null;
		parrybles = null;
		playerExitEffects = null;
	}
}
