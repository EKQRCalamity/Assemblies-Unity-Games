using System;
using System.Collections.Generic;
using CreativeSpore.SmartColliders;
using DG.Tweening;
using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using Tools.Audio;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.AreaEffects;

public class WindAreaEffect : AreaEffect
{
	public enum Direction
	{
		Right,
		Left,
		Random,
		Alternate
	}

	protected Vector3 CurrentRandomWindDirection;

	protected Vector3 CurrentAlternateWindDirection;

	[FoldoutGroup("Wind Settings", 0)]
	public Direction WindDirection;

	[FoldoutGroup("Wind Settings", 0)]
	public float MaxWindForce;

	[FoldoutGroup("Wind Settings", 0)]
	[Tooltip("Time to reach max force")]
	public float WindAccelerationTime;

	[FoldoutGroup("Wind Settings", 0)]
	[Tooltip("Wind impulse in mid-air")]
	[Range(1f, 50f)]
	public float WindImpulse = 10f;

	[FoldoutGroup("Wind Settings", 0)]
	public bool useTimers = true;

	[FoldoutGroup("Wind Settings", 0)]
	[ShowIf("useTimers", true)]
	public float WindGustTime;

	[FoldoutGroup("Wind Settings", 0)]
	[ShowIf("useTimers", true)]
	public float WindPauseTime;

	protected bool IsWindAcceleration;

	protected float CurrentWindGustTime;

	protected float CurrentWindPauseTime;

	protected List<PlatformCharacterController> Controllers;

	protected List<Rigidbody2D> RigidBodies;

	protected Gameplay.GameControllers.Penitent.Penitent _penitent;

	public float CurrentWindForce { get; private set; }

	private AmbientMusicSettings AmbientMusic { get; set; }

	private float NormalizedWindForce => CurrentWindForce / MaxWindForce;

	protected override void OnAwake()
	{
		base.OnAwake();
		Controllers = new List<PlatformCharacterController>();
		RigidBodies = new List<Rigidbody2D>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		CurrentRandomWindDirection = ((!((double)UnityEngine.Random.value > 0.5)) ? Vector3.left : Vector3.right);
		CurrentAlternateWindDirection = Vector3.right;
		AmbientMusic = Core.Audio.Ambient;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!useTimers)
		{
			return;
		}
		if (CurrentWindPauseTime < WindPauseTime)
		{
			CurrentWindPauseTime += Time.deltaTime;
			return;
		}
		if (CurrentWindForce < MaxWindForce && !IsWindAcceleration)
		{
			SetWindForce(MaxWindForce);
		}
		if (CurrentWindGustTime < WindGustTime && CurrentWindForce >= MaxWindForce)
		{
			CurrentWindGustTime += Time.deltaTime;
			return;
		}
		if (CurrentWindForce > 0f && !IsWindAcceleration)
		{
			SetWindForce(0f);
		}
		SetAmbientWindParam(NormalizedWindForce);
	}

	public void SetMaxForce()
	{
		SetWindForce(MaxWindForce);
	}

	public void SetMinForce()
	{
		SetWindForce(0f);
	}

	protected override void OnEnterAreaEffect(Collider2D other)
	{
		base.OnEnterAreaEffect(other);
		PlatformCharacterController componentInChildren = other.transform.root.GetComponentInChildren<PlatformCharacterController>();
		Rigidbody2D componentInChildren2 = other.transform.root.GetComponentInChildren<Rigidbody2D>();
		if (!Controllers.Contains(componentInChildren))
		{
			Controllers.Add(componentInChildren);
		}
		if (!RigidBodies.Contains(componentInChildren2))
		{
			RigidBodies.Add(componentInChildren2);
		}
		_penitent = other.transform.root.GetComponentInChildren<Gameplay.GameControllers.Penitent.Penitent>();
	}

	protected override void OnStayAreaEffect()
	{
		base.OnStayAreaEffect();
		if (_penitent == null || Controllers.Count == 0 || RigidBodies.Count == 0)
		{
			_penitent = Core.Logic.Penitent;
			PlatformCharacterController componentInChildren = _penitent.transform.root.GetComponentInChildren<PlatformCharacterController>();
			Rigidbody2D componentInChildren2 = _penitent.transform.root.GetComponentInChildren<Rigidbody2D>();
			if (!Controllers.Contains(componentInChildren))
			{
				Controllers.Add(componentInChildren);
			}
			if (!RigidBodies.Contains(componentInChildren2))
			{
				RigidBodies.Add(componentInChildren2);
			}
		}
		if (_penitent != null)
		{
			if (_penitent.ThrowBack.IsThrown)
			{
				return;
			}
			if (_penitent.IsGrabbingCliffLede || _penitent.IsClimbingCliffLede || _penitent.IsClimbingLadder || _penitent.IsStickedOnWall)
			{
				SetDynamicMode(_penitent.RigidBody, dynamicMode: false);
				return;
			}
		}
		foreach (PlatformCharacterController controller in Controllers)
		{
			ApplyWindForce(controller);
			ApplyPhysicsForce(controller.IsGrounded);
		}
	}

	protected override void OnExitAreaEffect(Collider2D other)
	{
		base.OnExitAreaEffect(other);
		PlatformCharacterController componentInChildren = other.transform.root.GetComponentInChildren<PlatformCharacterController>();
		Rigidbody2D componentInChildren2 = other.transform.root.GetComponentInChildren<Rigidbody2D>();
		Gameplay.GameControllers.Penitent.Penitent componentInParent = other.GetComponentInParent<Gameplay.GameControllers.Penitent.Penitent>();
		if (!componentInParent || !componentInParent.ThrowBack.IsThrown)
		{
			SetDynamicMode(componentInChildren2, dynamicMode: false);
		}
		if (Controllers.Contains(componentInChildren))
		{
			Controllers.Remove(componentInChildren);
		}
		if (RigidBodies.Contains(componentInChildren2))
		{
			RigidBodies.Remove(componentInChildren2);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Controllers.Clear();
		SetAmbientWindParam(0f);
	}

	public void ApplyWindForce(PlatformCharacterController controller)
	{
		Vector3 windForce = GetWindForce();
		controller.PlatformCharacterPhysics.AddAcceleration(windForce);
	}

	public void ApplyPhysicsForce(bool apply)
	{
		foreach (Rigidbody2D rigidBody in RigidBodies)
		{
			if (apply)
			{
				SetDynamicMode(rigidBody, dynamicMode: false);
				continue;
			}
			SetDynamicMode(rigidBody, dynamicMode: true);
			rigidBody.AddForce(GetWindForce().normalized * WindImpulse, ForceMode2D.Force);
			rigidBody.velocity = Vector2.ClampMagnitude(rigidBody.velocity, 3.5f);
		}
	}

	private void SetDynamicMode(Rigidbody2D rigidBody, bool dynamicMode)
	{
		if (dynamicMode)
		{
			rigidBody.isKinematic = false;
			rigidBody.gravityScale = 0f;
			rigidBody.mass = 1f;
		}
		else
		{
			rigidBody.mass = 10f;
			rigidBody.velocity = Vector2.zero;
			rigidBody.gravityScale = 3f;
			rigidBody.isKinematic = true;
		}
	}

	public Vector3 GetWindForce()
	{
		return WindDirection switch
		{
			Direction.Right => Vector3.right * CurrentWindForce, 
			Direction.Left => Vector3.left * CurrentWindForce, 
			Direction.Random => CurrentRandomWindDirection * CurrentWindForce, 
			Direction.Alternate => CurrentAlternateWindDirection * CurrentWindForce, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void SetWindForce(float finalWindForce)
	{
		DOTween.To(delegate(float x)
		{
			CurrentWindForce = x;
		}, CurrentWindForce, finalWindForce, WindAccelerationTime).OnStart(OnAccelerationWindForce).OnComplete(OnStopAccelerationWindForce)
			.SetEase(Ease.OutSine);
	}

	private void OnAccelerationWindForce()
	{
		IsWindAcceleration = true;
	}

	private void OnStopAccelerationWindForce()
	{
		IsWindAcceleration = false;
		if (CurrentWindForce <= 0f)
		{
			CurrentWindGustTime = 0f;
			CurrentWindPauseTime = 0f;
			CurrentRandomWindDirection = ((!((double)UnityEngine.Random.value >= 0.5)) ? Vector3.left : Vector3.right);
			CurrentAlternateWindDirection = ((!(CurrentAlternateWindDirection == Vector3.right)) ? Vector3.right : Vector3.left);
		}
	}

	public void SetAmbientWindParam(float value)
	{
		if (AmbientMusic == null)
		{
			return;
		}
		EventInstance audioInstance = AmbientMusic.AudioInstance;
		if (audioInstance.isValid())
		{
			audioInstance.getParameter("Wind", out var instance);
			if (instance.isValid())
			{
				instance.setValue(value);
			}
		}
	}
}
