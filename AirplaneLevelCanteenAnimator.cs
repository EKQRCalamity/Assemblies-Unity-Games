using System.Collections;
using UnityEngine;

public class AirplaneLevelCanteenAnimator : LevelProperties.Airplane.Entity
{
	private const int MIN_IDLE_LOOPS = 3;

	private const int MAX_IDLE_LOOPS = 6;

	private const int MIN_LOOK_LOOPS = 7;

	private const int MAX_LOOK_LOOPS = 9;

	private const float PHASE_ONE_LOOK_ANGLE_THRESHOLD = 250f;

	private const float PHASE_THREE_LOOK_ANGLE_THRESHOLD = 100f;

	private int idleLoops;

	private int idleClipPos;

	private int lookLoops;

	private Vector3 forceLookTarget;

	private AbstractPlayerController player1;

	private AbstractPlayerController player2;

	private int p1health = -1;

	private int p2health = -1;

	private bool playerHitAltAnim;

	public bool triggerCheer;

	private LevelProperties.Airplane.States curState;

	private AirplaneLevel level;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	public override void LevelInit(LevelProperties.Airplane properties)
	{
		base.LevelInit(properties);
		level = Level.Current as AirplaneLevel;
		curState = properties.CurrentState.stateName;
		idleLoops = Random.Range(3, 6);
		StartCoroutine(check_players_cr());
		StartCoroutine(handle_canteen_cr());
	}

	private IEnumerator check_players_cr()
	{
		while (p1health == -1)
		{
			player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
			player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
			p1health = ((!player1) ? (-1) : PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.Health);
			p2health = ((!player2) ? (-1) : PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats.Health);
			yield return null;
		}
	}

	private void OnPlayerHit(bool dead)
	{
		base.animator.Play(dead ? "CanteenOnePlayerDied" : ((!playerHitAltAnim) ? "CanteenHitB" : "CanteenHitA"));
		base.animator.SetBool("CanteenTrackBoss", value: false);
		if (!dead)
		{
			playerHitAltAnim = !playerHitAltAnim;
		}
	}

	public override void OnLevelEnd()
	{
		base.OnLevelEnd();
		if (!Level.Won)
		{
			base.animator.SetBool("CanteenTrackBoss", value: false);
			base.animator.Play("CanteenAllPlayersDied");
		}
	}

	private IEnumerator handle_canteen_cr()
	{
		while (true)
		{
			if (Level.Won)
			{
				base.animator.SetBool("CanteenTrackBoss", value: false);
				base.animator.Play("CanteenWin");
			}
			else if (triggerCheer)
			{
				base.animator.SetBool("CanteenTrackBoss", value: false);
				base.animator.Play("CanteenCheer");
				triggerCheer = false;
			}
			else
			{
				player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
				player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
				if ((bool)player1)
				{
					if (PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.Health < p1health)
					{
						p1health = PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.Health;
						OnPlayerHit(p1health == 0);
					}
					else if (PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.Health > p1health)
					{
						base.animator.SetBool("CanteenTrackBoss", value: false);
						base.animator.Play("CanteenCheer");
					}
					p1health = PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.Health;
				}
				if ((bool)player2)
				{
					if (PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats.Health < p2health)
					{
						p2health = PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats.Health;
						OnPlayerHit(p2health == 0);
					}
					else if (PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats.Health > p2health)
					{
						base.animator.SetBool("CanteenTrackBoss", value: false);
						base.animator.Play("CanteenCheer");
					}
					p2health = PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats.Health;
				}
			}
			yield return null;
		}
	}

	private void LookAtBoss()
	{
		switch (base.properties.CurrentState.stateName)
		{
		case LevelProperties.Airplane.States.Main:
		case LevelProperties.Airplane.States.Generic:
		case LevelProperties.Airplane.States.Rocket:
			if (level.CurrentEnemyPos().x - base.transform.position.x < -250f)
			{
				base.animator.Play("CanteenLookUpLeft");
				base.animator.SetInteger("CanteenLookUpDir", -1);
				base.animator.SetBool("CanteenTrackBoss", value: true);
			}
			else if (level.CurrentEnemyPos().x - base.transform.position.x > 250f)
			{
				base.animator.Play("CanteenLookUpRight");
				base.animator.SetInteger("CanteenLookUpDir", 1);
				base.animator.SetBool("CanteenTrackBoss", value: true);
			}
			else
			{
				base.animator.Play("CanteenLookUp");
				base.animator.SetInteger("CanteenLookUpDir", 0);
				base.animator.SetBool("CanteenTrackBoss", value: true);
			}
			break;
		case LevelProperties.Airplane.States.Terriers:
			base.animator.SetBool("CanteenTrackBoss", value: false);
			switch (Random.Range(0, 6))
			{
			case 0:
				base.animator.Play("CanteenLookUpLeft");
				break;
			case 1:
				base.animator.Play("CanteenLookUp");
				break;
			case 2:
				base.animator.Play("CanteenLookUpRight");
				break;
			case 3:
				base.animator.Play("CanteenLookDownRight");
				break;
			case 4:
				base.animator.Play("CanteenLookDown");
				break;
			case 5:
				base.animator.Play("CanteenLookDownLeft");
				break;
			}
			break;
		case LevelProperties.Airplane.States.Leader:
			if (level.ScreenHorizontal())
			{
				if (level.CurrentEnemyPos().x - base.transform.position.x < -100f)
				{
					base.animator.Play("CanteenLookUpLeft");
					base.animator.SetInteger("CanteenLookUpDir", -1);
					base.animator.SetBool("CanteenTrackBoss", value: true);
				}
				else if (level.CurrentEnemyPos().x - base.transform.position.x > 100f)
				{
					base.animator.Play("CanteenLookUpRight");
					base.animator.SetInteger("CanteenLookUpDir", 1);
					base.animator.SetBool("CanteenTrackBoss", value: true);
				}
				else
				{
					base.animator.Play("CanteenLookUp");
					base.animator.SetInteger("CanteenLookUpDir", 0);
					base.animator.SetBool("CanteenTrackBoss", value: true);
				}
			}
			else
			{
				switch (Random.Range(0, 2))
				{
				case 0:
					base.animator.Play("CanteenLookUpLeft");
					break;
				case 1:
					base.animator.Play("CanteenLookUpRight");
					break;
				}
			}
			break;
		}
		lookLoops = Random.Range(7, 9);
	}

	public void ForceLook(Vector3 target, int loops)
	{
		lookLoops = loops;
		idleLoops = 1;
		idleClipPos = -1;
		forceLookTarget = target;
	}

	private void LookInDirection()
	{
		base.animator.SetBool("CanteenTrackBoss", value: false);
		switch ((int)(((double)Vector3.SignedAngle(Vector3.up, forceLookTarget - base.transform.position, Vector3.back) + 202.5) % 360.0) / 45)
		{
		case 0:
			base.animator.Play("CanteenLookDown");
			break;
		case 7:
			base.animator.Play("CanteenLookDownRight");
			break;
		case 5:
		case 6:
			base.animator.Play("CanteenLookUpRight");
			break;
		case 4:
			base.animator.Play("CanteenLookUp");
			break;
		case 2:
		case 3:
			base.animator.Play("CanteenLookUpLeft");
			break;
		case 1:
			base.animator.Play("CanteenLookDownLeft");
			break;
		}
	}

	private void OnCanteenIdleLoop()
	{
		idleLoops--;
		if (idleLoops != 0)
		{
			return;
		}
		switch (idleClipPos)
		{
		case -1:
			LookInDirection();
			base.animator.SetBool("CanteenTrackBoss", value: false);
			break;
		case 0:
			base.animator.SetTrigger("CanteenBlink");
			base.animator.SetBool("CanteenTrackBoss", value: false);
			break;
		case 1:
			if (Random.Range(0, (base.properties.CurrentState.stateName != LevelProperties.Airplane.States.Terriers) ? 10 : 4) == 0)
			{
				LookAtBoss();
				break;
			}
			base.animator.SetTrigger("CanteenGlanceAround");
			base.animator.SetBool("CanteenTrackBoss", value: false);
			break;
		case 2:
			base.animator.SetTrigger("CanteenBlink");
			base.animator.SetBool("CanteenTrackBoss", value: false);
			break;
		case 3:
			LookAtBoss();
			break;
		}
		idleClipPos = (idleClipPos + 1) % 4;
		idleLoops = Random.Range(3, 6);
	}

	private void OnCanteenLookLoop()
	{
		lookLoops--;
		if (lookLoops <= 0)
		{
			base.animator.SetTrigger("CanteenEndLookLoop");
		}
	}

	private void Update()
	{
		if (!base.animator.GetBool("CanteenTrackBoss"))
		{
			return;
		}
		switch (base.properties.CurrentState.stateName)
		{
		case LevelProperties.Airplane.States.Main:
		case LevelProperties.Airplane.States.Generic:
		case LevelProperties.Airplane.States.Rocket:
			if (level.CurrentEnemyPos().x - base.transform.position.x < -250f)
			{
				base.animator.SetInteger("CanteenLookUpDir", -1);
			}
			else if (level.CurrentEnemyPos().x - base.transform.position.x > 250f)
			{
				base.animator.SetInteger("CanteenLookUpDir", 1);
			}
			else
			{
				base.animator.SetInteger("CanteenLookUpDir", 0);
			}
			break;
		case LevelProperties.Airplane.States.Leader:
			if (level.ScreenHorizontal())
			{
				if (level.CurrentEnemyPos().x - base.transform.position.x < -100f)
				{
					base.animator.SetInteger("CanteenLookUpDir", -1);
				}
				else if (level.CurrentEnemyPos().x - base.transform.position.x > 100f)
				{
					base.animator.SetInteger("CanteenLookUpDir", 1);
				}
				else
				{
					base.animator.SetInteger("CanteenLookUpDir", 0);
				}
			}
			break;
		case LevelProperties.Airplane.States.Terriers:
			break;
		}
	}

	private void WORKAROUND_NullifyFields()
	{
		player1 = null;
		player2 = null;
		level = null;
	}
}
