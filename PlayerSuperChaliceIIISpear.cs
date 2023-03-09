using UnityEngine;

public class PlayerSuperChaliceIIISpear : AbstractProjectile
{
	private const float EXPIRE_TIME = 10f;

	[SerializeField]
	private BoxCollider2D coll;

	[SerializeField]
	private float floatAmplitude = 20f;

	[SerializeField]
	private float floatT;

	[SerializeField]
	private float floatSpeed = 1f;

	private Vector3 basePos;

	private float timer;

	public LevelPlayerController sourcePlayer;

	protected override void OnDieLifetime()
	{
	}

	protected override void Start()
	{
		_countParryTowardsScore = false;
		basePos = base.transform.position;
	}

	public void DetachFromSuper(LevelPlayerController p)
	{
		sourcePlayer = p;
		sourcePlayer.weaponManager.OnSuperStart += Die;
		base.transform.parent = null;
	}

	public override void OnParry(AbstractPlayerController player)
	{
		AudioManager.Play("player_super_chalice_barrage_spearparry");
		base.OnParry(player);
	}

	public override void OnParryDie()
	{
		Die();
	}

	protected override void Die()
	{
		coll.enabled = false;
		base.animator.Play("Die");
	}

	protected override void OnDestroy()
	{
		if (sourcePlayer != null)
		{
			sourcePlayer.weaponManager.OnSuperStart -= Die;
		}
		base.OnDestroy();
	}

	protected override void FixedUpdate()
	{
	}

	protected override void Update()
	{
		floatT += (float)CupheadTime.Delta * floatSpeed;
		base.transform.position = new Vector3(basePos.x, basePos.y + Mathf.Sin(floatT) * floatAmplitude);
		timer += CupheadTime.Delta;
		if (timer > 10f)
		{
			Die();
		}
		if (base.transform.parent == null && sourcePlayer == null)
		{
			Die();
		}
	}
}
