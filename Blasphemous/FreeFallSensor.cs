using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

public class FreeFallSensor : MonoBehaviour
{
	public float maxFallingTime = 5f;

	public float minVerticalFallSpeed = -1f;

	private PlatformCharacterController controller;

	private float fallingTime;

	private void Awake()
	{
		controller = GetComponent<PlatformCharacterController>();
	}

	private void Start()
	{
		Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, (Core.SimpleEvent)delegate
		{
			base.enabled = false;
		});
	}

	private void FixedUpdate()
	{
		if (controller.InstantVelocity.y > minVerticalFallSpeed)
		{
			fallingTime = 0f;
			return;
		}
		fallingTime += Time.fixedDeltaTime;
		if (fallingTime > maxFallingTime)
		{
			base.enabled = false;
			KillPlayer();
		}
	}

	private void KillPlayer()
	{
		Penitent penitent = Core.Logic.Penitent;
		if (!(penitent == null))
		{
			if (penitent.Stats.Life.Current > 0f)
			{
				penitent.Stats.Life.Current = 0f;
			}
			penitent.KillInstanteneously();
			if ((bool)controller)
			{
				controller.enabled = false;
			}
			Core.Logic.CameraManager.ProCamera2D.FollowVertical = false;
		}
	}
}
