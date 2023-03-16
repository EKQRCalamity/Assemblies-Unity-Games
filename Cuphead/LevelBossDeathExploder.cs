using System.Collections;
using UnityEngine;

public class LevelBossDeathExploder : AbstractMonoBehaviour
{
	private enum State
	{
		Steady,
		Random
	}

	public Effect ExplosionPrefabOverride;

	[SerializeField]
	private float STEADY_DELAY = 0.3f;

	[SerializeField]
	private float MIN_DELAY = 0.4f;

	[SerializeField]
	private float MAX_DELAY = 1f;

	public Vector2 offset = Vector2.zero;

	[SerializeField]
	private float radius = 100f;

	[SerializeField]
	private Vector2 scaleFactor = new Vector2(1f, 1f);

	private State state;

	protected Effect effectPrefab;

	[SerializeField]
	private bool disableSound;

	protected override void Awake()
	{
		base.Awake();
	}

	protected virtual void Start()
	{
		if ((bool)ExplosionPrefabOverride)
		{
			effectPrefab = ExplosionPrefabOverride;
		}
		else
		{
			effectPrefab = Level.Current.LevelResources.levelBossDeathExplosion;
		}
		Level.Current.OnBossDeathExplosionsEvent += StartExplosion;
		Level.Current.OnBossDeathExplosionsFalloffEvent += OnExplosionsRand;
		Level.Current.OnBossDeathExplosionsEndEvent += StopExplosions;
	}

	private void OnDestroy()
	{
		ExplosionPrefabOverride = null;
		effectPrefab = null;
		try
		{
			Level.Current.OnBossDeathExplosionsEvent -= StartExplosion;
		}
		catch
		{
		}
		try
		{
			Level.Current.OnBossDeathExplosionsFalloffEvent -= OnExplosionsRand;
		}
		catch
		{
		}
		try
		{
			Level.Current.OnBossDeathExplosionsEndEvent -= StopExplosions;
		}
		catch
		{
		}
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere((Vector2)base.baseTransform.position + offset, radius);
	}

	public void StartExplosion()
	{
		StartExplosion(bypassCameraShakeEvent: false);
	}

	public void StartExplosion(bool bypassCameraShakeEvent)
	{
		if (!(this == null) && base.enabled && base.isActiveAndEnabled)
		{
			StartCoroutine(go_cr(bypassCameraShakeEvent));
		}
	}

	public void OnExplosionsRand()
	{
		state = State.Random;
	}

	public void StopExplosions()
	{
		StopAllCoroutines();
	}

	private Vector2 GetRandomPoint()
	{
		Vector2 vector = (Vector2)base.transform.position + offset;
		Vector2 vector2 = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized * (radius * Random.value) * 2f;
		vector2.x *= scaleFactor.x;
		vector2.y *= scaleFactor.y;
		return vector + vector2;
	}

	private IEnumerator go_cr(bool bypassCameraShakeEvent)
	{
		HitFlash flash = GetComponent<HitFlash>();
		if (!disableSound)
		{
			AudioManager.Play("level_explosion_boss_death");
		}
		while (true)
		{
			effectPrefab.Create(GetRandomPoint());
			if (flash != null)
			{
				flash.Flash();
			}
			CupheadLevelCamera.Current.Shake(10f, 0.4f, bypassCameraShakeEvent);
			State state = this.state;
			if (state != State.Random)
			{
				yield return CupheadTime.WaitForSeconds(this, STEADY_DELAY);
			}
			else
			{
				yield return CupheadTime.WaitForSeconds(this, Random.Range(MIN_DELAY, MAX_DELAY));
			}
		}
	}
}
