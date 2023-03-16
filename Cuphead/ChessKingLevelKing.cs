using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessKingLevelKing : LevelProperties.ChessKing.Entity
{
	public static float multiplayerDamageNerf = 8f;

	private const float Y_SPAWN = -300f;

	[SerializeField]
	private ChessKingLevelRat ratPrefab;

	private List<ChessKingLevelRat> rats;

	[SerializeField]
	private Transform ratSpawn;

	[SerializeField]
	private GameObject beamAttack;

	[SerializeField]
	private GameObject fullAttack;

	[SerializeField]
	private ChessKingLevelGroundTrigger groundTrigger;

	[SerializeField]
	private ChessKingLevelParryPoint parryPoint;

	private List<ChessKingLevelParryPoint> parryPoints;

	private int kingAttackStringMainIndex;

	private int kingAttackStringIndex;

	private int trialPoolMainIndex;

	private bool challengeActivated;

	private bool isAttacking;

	public bool GOT_PARRIED { get; private set; }

	public void StartGame()
	{
		rats = new List<ChessKingLevelRat>();
		StartCoroutine(timer_cr());
		LevelProperties.ChessKing.King king = base.properties.CurrentState.king;
		trialPoolMainIndex = Random.Range(0, base.properties.CurrentState.king.trialPool.Length);
		kingAttackStringMainIndex = Random.Range(0, king.kingAttackString.Length);
		string[] array = king.kingAttackString[kingAttackStringMainIndex].Split(',');
		kingAttackStringIndex = Random.Range(0, array.Length);
		StartCoroutine(create_trial_cr());
	}

	private void Update()
	{
	}

	public override void LevelInit(LevelProperties.ChessKing properties)
	{
		base.LevelInit(properties);
	}

	public void StateChange()
	{
		LevelProperties.ChessKing.King king = base.properties.CurrentState.king;
		trialPoolMainIndex = Random.Range(0, base.properties.CurrentState.king.trialPool.Length);
		kingAttackStringMainIndex = Random.Range(0, king.kingAttackString.Length);
		string[] array = king.kingAttackString[kingAttackStringMainIndex].Split(',');
		kingAttackStringIndex = Random.Range(0, array.Length);
	}

	public override void OnParry(AbstractPlayerController player)
	{
		base.OnParry(player);
		GOT_PARRIED = true;
		base.properties.DealDamage((!PlayerManager.BothPlayersActive()) ? 10f : multiplayerDamageNerf);
	}

	public void BecomeParryable()
	{
		GOT_PARRIED = false;
		base.animator.Play("Parryable");
	}

	private IEnumerator create_trial_cr()
	{
		parryPoints = new List<ChessKingLevelParryPoint>();
		LevelProperties.ChessKing.King p = base.properties.CurrentState.king;
		string[] trial = p.trialPool[trialPoolMainIndex].Split(',');
		GOT_PARRIED = false;
		for (int j = 0; j < trial.Length; j++)
		{
			string[] array = trial[j].Split(':');
			Vector3 dir = Vector3.zero;
			float result = 0f;
			float result2 = 0f;
			float result3 = 0f;
			bool flag = false;
			for (int k = 0; k < array.Length; k++)
			{
				switch (k)
				{
				case 0:
					Parser.FloatTryParse(array[k], out result);
					break;
				case 1:
					Parser.FloatTryParse(array[k], out result2);
					break;
				case 2:
					flag = true;
					dir = GetDir(array[k]);
					break;
				case 3:
					Parser.FloatTryParse(array[k], out result3);
					break;
				}
			}
			ChessKingLevelParryPoint chessKingLevelParryPoint = Object.Instantiate(parryPoint);
			Vector3 pos = new Vector3(Level.Current.Left, Level.Current.Ground) + new Vector3(result, result2);
			if (flag)
			{
				chessKingLevelParryPoint.Init(pos, dir, p.bluePointSpeed, result3);
			}
			else
			{
				chessKingLevelParryPoint.Init(pos);
			}
			parryPoints.Add(chessKingLevelParryPoint);
		}
		for (int i = 0; i < parryPoints.Count; i++)
		{
			parryPoints[i].Activate();
			while (!parryPoints[i].GOT_PARRIED && !groundTrigger.PLAYER_FALLEN)
			{
				yield return null;
			}
			if (!challengeActivated)
			{
				groundTrigger.CheckPlayer(checkPlayer: true);
				MoveBluePoints();
				challengeActivated = true;
			}
		}
		EndChallenge();
		yield return null;
	}

	private void EndChallenge()
	{
		StartCoroutine(end_challenge());
	}

	private IEnumerator end_challenge()
	{
		if (!groundTrigger.PLAYER_FALLEN)
		{
			BecomeParryable();
			while (!GOT_PARRIED && !groundTrigger.PLAYER_FALLEN)
			{
				yield return null;
			}
		}
		challengeActivated = false;
		base.animator.Play("Idle");
		groundTrigger.CheckPlayer(checkPlayer: false);
		ClearPoints();
		if (!GOT_PARRIED)
		{
			Attack();
		}
		while (isAttacking)
		{
			yield return null;
		}
		LevelPlayerController player = PlayerManager.GetPlayer(PlayerId.PlayerOne) as LevelPlayerController;
		while (!player.motor.Grounded)
		{
			yield return null;
		}
		trialPoolMainIndex = (trialPoolMainIndex + 1) % base.properties.CurrentState.king.trialPool.Length;
		StartCoroutine(create_trial_cr());
	}

	private void ClearPoints()
	{
		for (int i = 0; i < parryPoints.Count; i++)
		{
			Object.Destroy(parryPoints[i].gameObject);
		}
		parryPoints.Clear();
	}

	private void MoveBluePoints()
	{
		for (int i = 0; i < parryPoints.Count; i++)
		{
			parryPoints[i].MovePoint();
		}
	}

	private IEnumerator timer_cr()
	{
		float t = 0f;
		float time = base.properties.CurrentState.king.kingAttackTimer;
		while (true)
		{
			if (!challengeActivated && !isAttacking)
			{
				if (t < time)
				{
					t += (float)CupheadTime.Delta;
				}
				else
				{
					Attack();
					t = 0f;
				}
			}
			else
			{
				t = 0f;
			}
			yield return null;
		}
	}

	private Vector3 GetDir(string part)
	{
		if (part[0] == 'R')
		{
			return Vector3.right;
		}
		if (part[0] == 'L')
		{
			return Vector3.left;
		}
		if (part[0] == 'U')
		{
			return Vector3.up;
		}
		return Vector3.down;
	}

	private void Attack()
	{
		isAttacking = true;
		LevelProperties.ChessKing.King king = base.properties.CurrentState.king;
		string[] array = king.kingAttackString[kingAttackStringMainIndex].Split(',');
		switch (array[kingAttackStringIndex])
		{
		case "F":
			StartCoroutine(full_screen_attack_cr());
			break;
		case "B":
			StartCoroutine(beam_attack_cr());
			break;
		case "R":
			StartCoroutine(rat_attack_cr());
			break;
		}
		if (kingAttackStringIndex < array.Length - 1)
		{
			kingAttackStringIndex++;
			return;
		}
		kingAttackStringMainIndex = (kingAttackStringMainIndex + 1) % king.kingAttackString.Length;
		kingAttackStringIndex = 0;
	}

	private IEnumerator full_screen_attack_cr()
	{
		LevelProperties.ChessKing.Full p = base.properties.CurrentState.full;
		base.animator.SetBool("isAnti", value: true);
		yield return CupheadTime.WaitForSeconds(this, p.fullAttackAnti);
		fullAttack.SetActive(value: true);
		yield return CupheadTime.WaitForSeconds(this, p.fullAttackDuration);
		fullAttack.SetActive(value: false);
		base.animator.SetBool("isAnti", value: false);
		yield return CupheadTime.WaitForSeconds(this, p.fullAttackRecovery);
		isAttacking = false;
		yield return null;
	}

	private IEnumerator beam_attack_cr()
	{
		LevelProperties.ChessKing.Beam p = base.properties.CurrentState.beam;
		base.animator.SetBool("isAnti", value: true);
		yield return CupheadTime.WaitForSeconds(this, p.beamAttackAnti);
		beamAttack.SetActive(value: true);
		yield return CupheadTime.WaitForSeconds(this, p.beamAttackDuration);
		beamAttack.SetActive(value: false);
		base.animator.SetBool("isAnti", value: false);
		yield return CupheadTime.WaitForSeconds(this, p.beamAttackRecovery);
		isAttacking = false;
		yield return null;
	}

	private IEnumerator rat_attack_cr()
	{
		LevelProperties.ChessKing.Rat p = base.properties.CurrentState.rat;
		base.animator.SetBool("isAnti", value: true);
		yield return CupheadTime.WaitForSeconds(this, p.ratSummonAnti);
		if (rats.Count < p.maxRatNumber)
		{
			ChessKingLevelRat chessKingLevelRat = Object.Instantiate(ratPrefab);
			chessKingLevelRat.Init(ratSpawn.position, p.ratSpeed);
			rats.Add(chessKingLevelRat);
		}
		yield return CupheadTime.WaitForSeconds(this, p.ratSummonDuration);
		base.animator.SetBool("isAnti", value: false);
		yield return CupheadTime.WaitForSeconds(this, p.ratSummonRecovery);
		isAttacking = false;
		yield return null;
	}
}
