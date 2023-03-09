using System.Collections;
using UnityEngine;

public class DicePalaceMainLevelCard : AbstractProjectile
{
	private LevelProperties.DicePalaceMain.Cards properties;

	private bool onLeft;

	private Vector3 direction;

	[SerializeField]
	private float coolDown = 0.4f;

	[SerializeField]
	private GameObject chaliceParryableHearts;

	[SerializeField]
	private Animator[] risingHeartAnimator;

	private int nextRisingHeart;

	[SerializeField]
	private SpriteRenderer[] risingHeartRenderer;

	[SerializeField]
	private MinMax risingHeartSpawnTimeRange = new MinMax(0.1667f, 0.2333f);

	public override float ParryMeterMultiplier => 0.25f;

	public DicePalaceMainLevelCard Create(Vector3 pos, LevelProperties.DicePalaceMain.Cards properties, bool onLeft)
	{
		DicePalaceMainLevelCard dicePalaceMainLevelCard = base.Create() as DicePalaceMainLevelCard;
		dicePalaceMainLevelCard.properties = properties;
		dicePalaceMainLevelCard.transform.position = pos;
		dicePalaceMainLevelCard.onLeft = onLeft;
		return dicePalaceMainLevelCard;
	}

	protected override void Start()
	{
		base.Start();
		direction = ((!onLeft) ? (-base.transform.right) : base.transform.right);
		if (base.CanParry && (PlayerManager.GetPlayer(PlayerId.PlayerOne).stats.isChalice || (PlayerManager.Multiplayer && PlayerManager.GetPlayer(PlayerId.PlayerTwo).stats.isChalice)))
		{
			nextRisingHeart = Random.Range(0, risingHeartAnimator.Length);
			chaliceParryableHearts.SetActive(value: true);
			StartCoroutine(rising_hearts_cr());
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

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		base.transform.position += direction * properties.cardSpeed * CupheadTime.FixedDelta;
		if (base.CanParry && risingHeartRenderer[0].sortingOrder == -1 && Mathf.Abs(base.transform.position.x) < 230f)
		{
			for (int i = 0; i < risingHeartRenderer.Length; i++)
			{
				risingHeartRenderer[i].sortingOrder = 2;
			}
		}
	}

	private IEnumerator rising_hearts_cr()
	{
		risingHeartAnimator[nextRisingHeart].Play(Random.Range(0, 6).ToString(), 0, 0.25f);
		nextRisingHeart = (nextRisingHeart + 1) % risingHeartAnimator.Length;
		risingHeartAnimator[nextRisingHeart].Play(Random.Range(0, 6).ToString(), 0, 0.5f);
		nextRisingHeart = (nextRisingHeart + 1) % risingHeartAnimator.Length;
		risingHeartAnimator[nextRisingHeart].Play(Random.Range(0, 6).ToString(), 0, 0.75f);
		nextRisingHeart = (nextRisingHeart + 1) % risingHeartAnimator.Length;
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, risingHeartSpawnTimeRange.RandomFloat());
			risingHeartAnimator[nextRisingHeart].Play(Random.Range(0, 6).ToString());
			nextRisingHeart = (nextRisingHeart + 1) % risingHeartAnimator.Length;
		}
	}

	public override void OnParry(AbstractPlayerController player)
	{
		SetParryable(parryable: false);
		StartCoroutine(parryCooldown_cr());
	}

	private IEnumerator parryCooldown_cr()
	{
		float t = 0f;
		while (t < coolDown)
		{
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		SetParryable(parryable: true);
		yield return null;
	}
}
