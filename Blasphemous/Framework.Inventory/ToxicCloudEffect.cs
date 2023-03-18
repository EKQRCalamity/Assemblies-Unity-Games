using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Environment.AreaEffects;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Framework.Inventory;

public class ToxicCloudEffect : ObjectEffect_Stat
{
	public GameObject ToxicCloudPrefab;

	[EventRef]
	public string InstantiateToxicCloudFx;

	public float InstantiateLapse = 2f;

	public Vector2 OffsetInstantiationPosition;

	private float _currentLapse;

	public float DamageAmount = 1f;

	private PoisonAreaEffect PoisonAreaEffect { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		if ((bool)ToxicCloudPrefab)
		{
			PoolManager.Instance.CreatePool(ToxicCloudPrefab, 3);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_currentLapse += Time.deltaTime;
		if (_currentLapse > InstantiateLapse)
		{
			InstantiateToxicCloud();
			_currentLapse = 0f;
		}
	}

	protected override bool OnApplyEffect()
	{
		InstantiateToxicCloud();
		_currentLapse = 0f;
		return base.OnApplyEffect();
	}

	private void InstantiateToxicCloud()
	{
		if ((bool)ToxicCloudPrefab && (bool)Core.Logic.Penitent)
		{
			Vector3 position = Core.Logic.Penitent.transform.position + (Vector3)OffsetInstantiationPosition;
			PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(ToxicCloudPrefab, position, Quaternion.identity);
			if (objectInstance != null)
			{
				PoisonAreaEffect = objectInstance.GameObject.GetComponentInChildren<PoisonAreaEffect>();
				PoisonAreaEffect.DamageAmount = Core.Logic.Penitent.Stats.PrayerStrengthMultiplier.Final * DamageAmount;
				PlayAudioEffect();
			}
		}
	}

	private void PlayAudioEffect()
	{
		if (!string.IsNullOrEmpty(InstantiateToxicCloudFx))
		{
			Core.Audio.PlaySfx(InstantiateToxicCloudFx);
		}
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}
}
