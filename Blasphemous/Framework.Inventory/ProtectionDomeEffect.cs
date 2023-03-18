using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Framework.Inventory;

public class ProtectionDomeEffect : ObjectEffect_Stat
{
	public GameObject ProtectionDomePrefab;

	[EventRef]
	public string InstantiateProtectionDomeFx;

	public float MaxDistanceFromDome = 2f;

	private GameObject protectionDomeInstance;

	private bool setInvulnerable;

	protected override void OnAwake()
	{
		base.OnAwake();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		if ((bool)ProtectionDomePrefab)
		{
			PoolManager.Instance.CreatePool(ProtectionDomePrefab, 1);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!protectionDomeInstance)
		{
			return;
		}
		Penitent penitent = Core.Logic.Penitent;
		if (!protectionDomeInstance.activeInHierarchy)
		{
			if (setInvulnerable)
			{
				setInvulnerable = false;
				penitent.Status.Invulnerable = setInvulnerable;
			}
		}
		else
		{
			if (Vector2.Distance(penitent.GetPosition(), protectionDomeInstance.transform.position) < MaxDistanceFromDome)
			{
				setInvulnerable = true;
			}
			else
			{
				setInvulnerable = false;
			}
			penitent.Status.Invulnerable = setInvulnerable;
		}
	}

	protected override bool OnApplyEffect()
	{
		InstantiateProtectionDome();
		return base.OnApplyEffect();
	}

	private void InstantiateProtectionDome()
	{
		if ((bool)ProtectionDomePrefab && (bool)Core.Logic.Penitent)
		{
			Vector3 position = Core.Logic.Penitent.transform.position + Vector3.up * 1.5f;
			protectionDomeInstance = PoolManager.Instance.ReuseObject(ProtectionDomePrefab, position, Quaternion.identity).GameObject;
			if (!(protectionDomeInstance == null))
			{
				PlayAudioEffect();
			}
		}
	}

	private void PlayAudioEffect()
	{
		if (!string.IsNullOrEmpty(InstantiateProtectionDomeFx))
		{
			Core.Audio.PlaySfx(InstantiateProtectionDomeFx);
		}
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}
}
