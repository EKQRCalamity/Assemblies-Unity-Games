using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Framework.Pooling;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Protection;

public class PenitentShieldSystem : PoolObject
{
	public Vector2 OffSetPosition;

	public float Radious = 2f;

	[Range(0.1f, 5f)]
	public float Frequency = 2f;

	public float FollowSpeed = 3f;

	public float deployTime = 5f;

	public CircularMovingObjects CircularMovingObjects { get; private set; }

	private void Awake()
	{
		CircularMovingObjects = GetComponent<CircularMovingObjects>();
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		DeployShields(deployTime);
	}

	private void LateUpdate()
	{
		FollowPlayer();
	}

	private void DeployShields(float time)
	{
		CircularMovingObjects.Frequency = Frequency;
		CircularMovingObjects.Radious = 0f;
		DOTween.To(delegate(float x)
		{
			CircularMovingObjects.Radious = x;
		}, CircularMovingObjects.Radious, Radious, time).SetEase(Ease.Linear);
	}

	private void FollowPlayer()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		if ((bool)penitent)
		{
			Vector3 b = penitent.GetPosition() + (Vector3)OffSetPosition;
			base.transform.position = Vector3.Slerp(base.transform.position, b, Time.deltaTime * FollowSpeed);
		}
	}

	public void SetShieldsOwner(Gameplay.GameControllers.Entities.Entity owner)
	{
		List<Transform> objectList = CircularMovingObjects.ObjectList;
		for (int i = 0; i < objectList.Count; i++)
		{
			PenitentShield component = objectList[i].GetComponent<PenitentShield>();
			component.WeaponOwner = owner;
			component.AttackArea.Entity = owner;
		}
	}

	public void DisposeShield(float time)
	{
		if (!CircularMovingObjects)
		{
			CircularMovingObjects = GetComponent<CircularMovingObjects>();
		}
		DOTween.To(delegate(float x)
		{
			CircularMovingObjects.Radious = x;
		}, CircularMovingObjects.Radious, 0f, time).OnComplete(base.Destroy);
	}
}
