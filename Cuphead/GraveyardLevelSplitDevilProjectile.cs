using System.Collections;
using UnityEngine;

public class GraveyardLevelSplitDevilProjectile : BasicProjectileContinuesOnLevelEnd
{
	private GraveyardLevelSplitDevil devil;

	[SerializeField]
	private SpriteRenderer[] fireRend;

	[SerializeField]
	private SpriteRenderer[] lightRend;

	[SerializeField]
	private Effect fireFX;

	[SerializeField]
	private Effect lightFX;

	[SerializeField]
	private Collider2D coll;

	private float frameTimer;

	private new bool dead;

	private bool impacted;

	[SerializeField]
	private float fxSpawnDelay = 0.15f;

	[SerializeField]
	private MinMax fxAngleShiftRange = new MinMax(60f, 300f);

	[SerializeField]
	private MinMax fxDistanceRange = new MinMax(0f, 20f);

	private float fxAngle;

	protected override bool DestroyedAfterLeavingScreen => true;

	public GraveyardLevelSplitDevilProjectile Create(Vector2 position, float rotation, float speed, GraveyardLevelSplitDevil devil)
	{
		GraveyardLevelSplitDevilProjectile graveyardLevelSplitDevilProjectile = base.Create(position, rotation, speed) as GraveyardLevelSplitDevilProjectile;
		graveyardLevelSplitDevilProjectile.devil = devil;
		graveyardLevelSplitDevilProjectile.animator.SetInteger("FireVariant", Random.Range(0, 3));
		graveyardLevelSplitDevilProjectile.animator.SetInteger("LightVariant", Random.Range(0, 2));
		graveyardLevelSplitDevilProjectile.SetBool("IsFire", !devil.isAngel);
		graveyardLevelSplitDevilProjectile.coll.enabled = !devil.isAngel;
		graveyardLevelSplitDevilProjectile.UpdateFade(2f);
		graveyardLevelSplitDevilProjectile.StartCoroutine(graveyardLevelSplitDevilProjectile.spawn_fx_cr());
		return graveyardLevelSplitDevilProjectile;
	}

	private IEnumerator spawn_fx_cr()
	{
		fxAngle = Random.Range(0, 360);
		while (!dead && !impacted)
		{
			yield return CupheadTime.WaitForSeconds(this, fxSpawnDelay);
			int count = 1;
			if (fxSpawnDelay < (float)CupheadTime.Delta)
			{
				count = (int)((float)CupheadTime.Delta / fxSpawnDelay);
			}
			for (int i = 0; i < count; i++)
			{
				Effect effect = ((!devil.isAngel) ? fireFX.Create(base.transform.position + (Vector3)MathUtils.AngleToDirection(fxAngle) * fxDistanceRange.RandomFloat()) : lightFX.Create(base.transform.position + (Vector3)MathUtils.AngleToDirection(fxAngle) * fxDistanceRange.RandomFloat()));
				if (!devil.isAngel)
				{
					effect.transform.eulerAngles = new Vector3(0f, 0f, base.transform.eulerAngles.z + 50f);
				}
				fxAngle = (fxAngle + fxAngleShiftRange.RandomFloat()) % 360f;
			}
		}
	}

	protected override void Update()
	{
		if (!impacted)
		{
			base.Update();
		}
		if (!dead)
		{
			coll.enabled = !devil.isAngel;
		}
		if (!impacted)
		{
			if (base.animator.GetBool("IsFire") == devil.isAngel)
			{
				base.animator.Play("LightTransition" + Random.Range(0, 3), 2, 0f);
			}
			base.animator.SetBool("IsFire", !devil.isAngel);
			frameTimer += CupheadTime.Delta;
			while (frameTimer > 1f / 24f)
			{
				frameTimer -= 1f / 24f;
				UpdateFade(0.25f);
			}
		}
		if (!impacted && Mathf.Abs(base.transform.position.x) < 550f && base.transform.position.y < -297f)
		{
			impacted = true;
			Speed = 0f;
			base.transform.eulerAngles = Vector3.zero;
			fireRend[0].transform.eulerAngles = Vector3.zero;
			lightRend[0].transform.eulerAngles = Vector3.zero;
			base.animator.Play((!Rand.Bool()) ? "ImpactB" : "ImpactA", devil.isAngel ? 1 : 0);
			UpdateFade(2f);
			Die();
		}
	}

	private void UpdateFade(float amount)
	{
		SpriteRenderer[] array = fireRend;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Clamp(spriteRenderer.color.a + ((!coll.enabled) ? (0f - amount) : amount), 0f, 1f));
		}
		SpriteRenderer[] array2 = lightRend;
		foreach (SpriteRenderer spriteRenderer2 in array2)
		{
			spriteRenderer2.color = new Color(spriteRenderer2.color.r, spriteRenderer2.color.g, spriteRenderer2.color.b, Mathf.Clamp(spriteRenderer2.color.a + ((!coll.enabled) ? amount : ((!(spriteRenderer2.gameObject.name == "Ring")) ? (0f - amount) : ((0f - amount) * 0.7f))), 0f, 1f));
		}
	}

	protected override void Die()
	{
		dead = true;
		coll.enabled = false;
	}
}
