using System.Collections;
using UnityEngine;

public class RumRunnersLevelMine : AbstractProjectile
{
	private const float START_HEIGHT = 800f;

	private LevelProperties.RumRunners.Mine properties;

	private RumRunnersLevelSpider parent;

	private Vector3 targetPos;

	private bool checkingPlayers;

	private bool exploding;

	private float timer;

	[SerializeField]
	private GameObject webRenderer;

	[SerializeField]
	private GameObject explosionRenderer;

	[SerializeField]
	private GameObject smokeRenderer;

	public int xPos { get; private set; }

	public int yPos { get; private set; }

	public int endPhaseExplodePriority { get; private set; }

	protected override float DestroyLifetime => 0f;

	public RumRunnersLevelMine Init(Vector3 targetPos, LevelProperties.RumRunners.Mine properties, RumRunnersLevelSpider parent, int xPos, int yPos)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = new Vector3(targetPos.x, 800f, (0f - targetPos.y) * 1E-05f);
		this.targetPos = targetPos;
		this.targetPos.z = (0f - targetPos.y) * 1E-05f;
		this.xPos = xPos;
		this.yPos = yPos;
		if (this.xPos == 3 && this.yPos == 2)
		{
			endPhaseExplodePriority = 0;
		}
		else if (this.xPos == 2)
		{
			endPhaseExplodePriority = 1;
		}
		else
		{
			endPhaseExplodePriority = 2;
		}
		this.properties = properties;
		base.animator.Play("Drop");
		GetComponent<SpriteRenderer>().enabled = true;
		this.parent = parent;
		StartCoroutine(lifetime_cr());
		MoveDown();
		webRenderer.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		explosionRenderer.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		smokeRenderer.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		webRenderer.transform.localScale = new Vector3(MathUtils.PlusOrMinus(), MathUtils.PlusOrMinus(), 1f);
		explosionRenderer.transform.localScale = new Vector3(MathUtils.PlusOrMinus(), MathUtils.PlusOrMinus(), 1f);
		smokeRenderer.transform.localScale = new Vector3(MathUtils.PlusOrMinus(), MathUtils.PlusOrMinus(), 1f);
		switch (yPos)
		{
		case 0:
			AudioManager.Play("sfx_dlc_rumrun_mine_drop_high");
			emitAudioFromObject.Add("sfx_dlc_rumrun_mine_drop_high");
			break;
		case 1:
			AudioManager.Play("sfx_dlc_rumrun_mine_drop_mid");
			emitAudioFromObject.Add("sfx_dlc_rumrun_mine_drop_mid");
			break;
		default:
			AudioManager.Play("sfx_dlc_rumrun_mine_drop_low");
			emitAudioFromObject.Add("sfx_dlc_rumrun_mine_drop_low");
			break;
		}
		return this;
	}

	private void MoveDown()
	{
		StartCoroutine(move_down_cr());
	}

	private IEnumerator move_down_cr()
	{
		base.transform.position = targetPos;
		yield return base.animator.WaitForAnimationToEnd(this, "Drop");
		StartCoroutine(check_distance_cr());
	}

	private IEnumerator check_distance_cr()
	{
		damageDealer.SetDamage(properties.mineBossDamage);
		damageDealer.SetRate(0f);
		checkingPlayers = true;
		while (checkingPlayers)
		{
			if ((bool)parent && parent.moving && !parent.isSummoning && Vector3.Distance(parent.transform.position, base.transform.position) < properties.mineDistToExplode)
			{
				base.animator.Play((!parent.goingLeft) ? "SwingRight" : "SwingLeft");
			}
			LevelPlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne) as LevelPlayerController;
			LevelPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo) as LevelPlayerController;
			float player1Dist = Vector3.Distance(player1.center, base.transform.position);
			if (!player1.IsDead && player1Dist < properties.mineDistToExplode)
			{
				checkingPlayers = false;
			}
			if (player2 != null)
			{
				float num = Vector3.Distance(player2.center, base.transform.position);
				if (!player2.IsDead && num < properties.mineDistToExplode)
				{
					checkingPlayers = false;
				}
			}
			yield return null;
		}
		if (!exploding)
		{
			StartCoroutine(explosion_cr(timedOut: false));
		}
	}

	private IEnumerator explosion_cr(bool timedOut)
	{
		exploding = true;
		base.animator.Play("PreExplode");
		AudioManager.Play("sfx_dlc_rumrun_mine_preexplode");
		emitAudioFromObject.Add("sfx_dlc_rumrun_mine_preexplode");
		yield return CupheadTime.WaitForSeconds(this, properties.mineExplosionWarning * (float)((!timedOut) ? 1 : 2));
		AudioManager.Play("sfx_dlc_rumrun_mine_explode");
		emitAudioFromObject.Add("sfx_dlc_rumrun_mine_explode");
		base.animator.Play("Explode");
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void SetTimer(float t)
	{
		if (!exploding)
		{
			timer = t;
		}
	}

	private IEnumerator lifetime_cr()
	{
		timer = properties.mineTimer;
		while (timer > 0f)
		{
			timer -= CupheadTime.Delta;
			yield return null;
		}
		checkingPlayers = false;
		if (!exploding)
		{
			StartCoroutine(explosion_cr(timedOut: true));
		}
	}

	private void Death()
	{
		StopAllCoroutines();
		this.Recycle();
	}
}
