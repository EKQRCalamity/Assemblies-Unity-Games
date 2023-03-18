using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Dash;

public class DashDustGenerator : Trait
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public float StopDustSpawnOffsetPos;

	[SerializeField]
	private GameObject startDashDustPrefab;

	[SerializeField]
	private GameObject stopDashDustPrefab;

	[SerializeField]
	private int poolsize = 5;

	protected override void OnStart()
	{
		base.OnStart();
		_penitent = (Gameplay.GameControllers.Penitent.Penitent)base.EntityOwner;
		if ((bool)startDashDustPrefab)
		{
			PoolManager.Instance.CreatePool(startDashDustPrefab, poolsize);
		}
		if ((bool)stopDashDustPrefab)
		{
			PoolManager.Instance.CreatePool(stopDashDustPrefab, poolsize);
		}
	}

	public GameObject GetStartDashDust()
	{
		return PoolManager.Instance.ReuseObject(startDashDustPrefab, _penitent.transform.position, Quaternion.identity).GameObject;
	}

	public void GetStopDashDust(float delay)
	{
		StopAllCoroutines();
		StartCoroutine(DelayStopDash(delay));
	}

	public void GetStopDashDust()
	{
		PoolManager.Instance.ReuseObject(stopDashDustPrefab, GetDashDustPosition(), Quaternion.identity);
	}

	private IEnumerator DelayStopDash(float d)
	{
		yield return new WaitForSeconds(d);
		GetStopDashDust();
	}

	public Vector3 GetDashDustPosition()
	{
		if (_penitent.DamageArea == null)
		{
			return Vector3.zero;
		}
		float y = _penitent.DamageArea.DamageAreaCollider.bounds.min.y - _penitent.PlatformCharacterController.GroundDist;
		float num = ((_penitent.Status.Orientation != 0) ? (_penitent.DamageArea.DamageAreaCollider.bounds.min.x - 0.2f) : (_penitent.DamageArea.DamageAreaCollider.bounds.max.x + 0.2f));
		StopDustSpawnOffsetPos *= ((_penitent.Status.Orientation != 0) ? (-1f) : 1f);
		return new Vector2(num + StopDustSpawnOffsetPos, y);
	}
}
