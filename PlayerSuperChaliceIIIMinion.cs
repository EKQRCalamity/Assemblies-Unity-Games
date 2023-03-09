using System;
using UnityEngine;

public class PlayerSuperChaliceIIIMinion : BasicProjectileContinuesOnLevelEnd
{
	public int elementIndex;

	private float wavelength = 180f;

	private float amplitude = 20f;

	private float t;

	private float startY;

	public bool wave;

	[SerializeField]
	private Effect impactFX;

	protected override void OnDieLifetime()
	{
	}

	protected override void OnDieDistance()
	{
	}

	protected override void Start()
	{
		base.Start();
		startY = base.transform.position.y;
		t = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		wavelength = UnityEngine.Random.Range(150f, 300f);
		damageDealer.SetDamageSource(DamageDealer.DamageSource.Super);
		MeterScoreTracker meterScoreTracker = new MeterScoreTracker(MeterScoreTracker.Type.Super);
		meterScoreTracker.Add(damageDealer);
	}

	protected override void OnDealDamage(float damage, DamageReceiver receiver, DamageDealer damageDealer)
	{
		base.OnDealDamage(damage, receiver, damageDealer);
		impactFX.Create(Vector3.Lerp(base.transform.position, receiver.transform.position, UnityEngine.Random.Range(0f, 1f)) + UnityEngine.Random.insideUnitSphere * 25f);
		AudioManager.Play("player_super_chalice_barrage_impact");
	}

	protected override void Move()
	{
		base.transform.position += Direction * Speed * CupheadTime.FixedDelta;
		if (wave)
		{
			t += Speed * CupheadTime.FixedDelta;
			base.transform.position = new Vector3(base.transform.position.x, startY + Mathf.Sin(t / wavelength) * amplitude);
		}
	}
}
