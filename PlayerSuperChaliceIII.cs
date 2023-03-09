using System.Collections;
using UnityEngine;

public class PlayerSuperChaliceIII : AbstractPlayerSuper
{
	[SerializeField]
	private BasicProjectile minionPrefab;

	private float mainTimer = 100f;

	private float direction;

	[SerializeField]
	private int minionCount = 50;

	[SerializeField]
	private bool wave = true;

	[SerializeField]
	private float aimSinkRate = 100f;

	[SerializeField]
	private SpriteRenderer chaliceSprite;

	[SerializeField]
	private SpriteRenderer fxRenderer;

	[SerializeField]
	private MinMax[] minionVerticalRange;

	[SerializeField]
	private MinMax[] minionScaleRange;

	[SerializeField]
	private float[] minionSpeed;

	[SerializeField]
	private float[] minionDamage;

	[SerializeField]
	private string minionSpawnString;

	private PatternString minionSpawn;

	[SerializeField]
	private string minionSpawnTimingString;

	private PatternString minionSpawnTiming;

	[SerializeField]
	private int[] minionTypeCount = new int[6];

	[SerializeField]
	private PlayerSuperChaliceIIISpear spear;

	[SerializeField]
	private Transform target;

	private float zoomFactor;

	[SerializeField]
	private bool linkSpeedToZoom = true;

	[SerializeField]
	private bool linkScaleToZoom;

	[SerializeField]
	private bool linkRangeToZoom;

	[SerializeField]
	private bool linkDamageToZoom;

	[SerializeField]
	private bool linkSpawnRateToZoom;

	[SerializeField]
	private float spawnRateZoomModifier = 1f;

	private bool chaliceRespawned;

	protected override void StartSuper()
	{
		base.StartSuper();
		if (!player.motor.Grounded)
		{
			base.animator.Play("StartAir");
		}
		base.animator.Update(0f);
		minionSpawn = new PatternString(minionSpawnString);
		minionSpawnTiming = new PatternString(minionSpawnTimingString);
		direction = base.transform.localScale.x;
		zoomFactor = Camera.main.orthographicSize / 360f;
		AudioManager.Play("player_super_chalice_barrage_start");
	}

	private IEnumerator super_cr()
	{
		Fire();
		while (true)
		{
			if (player != null)
			{
				if (target.position.y < player.transform.position.y)
				{
					target.position = new Vector3(target.position.x, player.transform.position.y);
				}
				else
				{
					target.position += Vector3.down * aimSinkRate * CupheadTime.Delta;
				}
			}
			else
			{
				EndSuper();
				StopAllCoroutines();
			}
			yield return null;
		}
	}

	private void StartMinions()
	{
		StartCoroutine(shoot_cr());
		StartCoroutine(super_cr());
	}

	private IEnumerator shoot_cr()
	{
		mainTimer = WeaponProperties.LevelSuperChaliceIII.superDuration;
		yield return null;
		for (int i = 0; i < minionCount; i++)
		{
			int spawnDataOffset = minionSpawn.GetSubStringIndex();
			int spawnType = minionSpawn.PopInt();
			float num = ((!(direction > 0f)) ? 180f : 0f);
			BasicProjectile bullet = minionPrefab.Create(rotation: num, position: new Vector3(CupheadLevelCamera.Current.transform.position.x - 1000f * Mathf.Sign(direction), target.position.y + minionVerticalRange[spawnDataOffset].RandomFloat() * ((!linkRangeToZoom) ? 1f : zoomFactor)), speed: minionSpeed[spawnDataOffset] * ((!linkSpeedToZoom) ? 1f : zoomFactor));
			bullet.Damage = minionDamage[spawnDataOffset] / ((!linkDamageToZoom) ? 1f : zoomFactor);
			float s = minionScaleRange[spawnDataOffset].RandomFloat() * ((!linkScaleToZoom) ? 1f : zoomFactor);
			bullet.transform.localScale = new Vector3(s, s);
			((PlayerSuperChaliceIIIMinion)bullet).elementIndex = spawnDataOffset;
			((PlayerSuperChaliceIIIMinion)bullet).wave = wave;
			SpriteRenderer r = bullet.GetComponent<SpriteRenderer>();
			r.flipY = direction < 0f;
			r.sortingOrder = ((!(s < 1f)) ? (100 - spawnType) : (-100 - spawnType));
			bullet.transform.position = new Vector3(bullet.transform.position.x, bullet.transform.position.y, (s - 1f) * 0.0001f);
			bullet.GetComponent<Animator>().Play(spawnType.ToString());
			bullet.DamageRate = 0.2f;
			bullet.PlayerId = player.id;
			bullet.GetComponent<Collider2D>().isTrigger = true;
			minionTypeCount[spawnType] = (minionTypeCount[spawnType] + 1) % 3;
			yield return CupheadTime.WaitForSeconds(this, minionSpawnTiming.PopFloat() * ((!linkSpawnRateToZoom) ? 1f : (zoomFactor * spawnRateZoomModifier)));
		}
		while ((bool)spear && !spear.gameObject.activeInHierarchy)
		{
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	private void RespawnChalice()
	{
		EndSuper();
		fxRenderer.sortingLayerName = "Player";
		fxRenderer.sortingOrder = -10;
		chaliceSprite.sortingLayerName = "Player";
		chaliceSprite.sortingOrder = -20;
		chaliceRespawned = true;
	}

	private void ActivateSpear()
	{
		if (player != null)
		{
			spear.DetachFromSuper(player);
		}
		else
		{
			spear.transform.parent = null;
		}
		spear.gameObject.SetActive(value: true);
	}
}
