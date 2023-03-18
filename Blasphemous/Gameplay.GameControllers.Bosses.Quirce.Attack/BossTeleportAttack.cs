using System;
using System.Collections;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class BossTeleportAttack : EnemyAttack
{
	public float teleportTime;

	public GameObject instantiateOnTeleportOut;

	public GameObject instantiateOnTeleportIn;

	private Transform _parentToMove;

	private Vector3 _targetPoint;

	private Coroutine _currentCoroutine;

	private WaitForSeconds _waitForSeconds;

	public event Action OnTeleportInEvent;

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_waitForSeconds = new WaitForSeconds(teleportTime);
	}

	public void Use(Transform parentToMove, Transform targetTransform, Vector3 offset)
	{
		_parentToMove = parentToMove;
		OnTeleportOut();
		StartCoroutine(TeleportCoroutine(parentToMove, targetTransform, offset, OnTeleportIn));
	}

	private IEnumerator TeleportCoroutine(Transform parentToMove, Transform target, Vector3 offset, Action callback = null)
	{
		yield return _waitForSeconds;
		parentToMove.position = target.position + offset;
		callback?.Invoke();
	}

	private void OnTeleportOut()
	{
		Debug.Log("TELEPORT OUT");
		if (instantiateOnTeleportOut != null)
		{
			InstantiateArea(instantiateOnTeleportOut);
		}
	}

	private void OnTeleportIn()
	{
		Debug.Log("TELEPORT IN");
		if (instantiateOnTeleportIn != null)
		{
			InstantiateArea(instantiateOnTeleportIn);
		}
		if (this.OnTeleportInEvent != null)
		{
			this.OnTeleportInEvent();
		}
	}

	private void InstantiateArea(GameObject toInstantiate)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(toInstantiate, _parentToMove.position, Quaternion.identity);
		BossSpawnedAreaAttack component = gameObject.GetComponent<BossSpawnedAreaAttack>();
		if (component != null)
		{
			component.SetOwner(base.EntityOwner);
		}
	}
}
