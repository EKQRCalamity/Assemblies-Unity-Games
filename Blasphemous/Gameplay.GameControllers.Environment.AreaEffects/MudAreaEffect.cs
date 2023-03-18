using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Abilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.AreaEffects;

public class MudAreaEffect : AreaEffect
{
	[FoldoutGroup("Area player's movement settings", 0)]
	public float JumpingSpeed;

	[FoldoutGroup("Area player's movement settings", 0)]
	public float WalkingDrag;

	[FoldoutGroup("Area player's movement settings", 0)]
	public float WalkingAcceleration;

	[FoldoutGroup("Area player's movement settings", 0)]
	public float MaxWalkingSpeed;

	[FoldoutGroup("Area player's movement settings", 0)]
	public Dash.MoveSetting DashSettings;

	private float _defaultJumpingSpeed;

	private float _defaultWalkingDrag;

	private float _defaultWalkingAcceleration;

	private float _defaultMaxWalkingSpeed;

	private Dash.MoveSetting _defaultDashSettings;

	public bool unafectedByRelic;

	protected PlatformCharacterController Controller;

	protected Dash Dash;

	protected Animator Animator;

	public GameObject vfxOnEnter;

	public GameObject vfxOnExit;

	public float vfxYOffset;

	protected override void OnStart()
	{
		base.OnStart();
		if (vfxOnEnter != null)
		{
			PoolManager.Instance.CreatePool(vfxOnEnter, 2);
		}
		if (vfxOnExit != null)
		{
			PoolManager.Instance.CreatePool(vfxOnExit, 2);
		}
	}

	protected override void OnEnterAreaEffect(Collider2D other)
	{
		base.OnEnterAreaEffect(other);
		if (vfxOnEnter != null)
		{
			ShowEffect(new Vector3(other.transform.position.x, base.transform.position.y + vfxYOffset, base.transform.position.z), vfxOnEnter);
		}
		foreach (GameObject item in Population)
		{
			Entity componentInParent = item.GetComponentInParent<Entity>();
			Controller = componentInParent.GetComponentInChildren<PlatformCharacterController>();
			Dash = componentInParent.GetComponentInChildren<Dash>();
			Animator = componentInParent.Animator;
			if (Controller != null)
			{
				_defaultJumpingSpeed = Controller.JumpingSpeed;
				_defaultWalkingAcceleration = Controller.WalkingAcc;
				_defaultMaxWalkingSpeed = Dash.DefaultMoveSetting.Speed;
				_defaultWalkingDrag = Dash.DefaultMoveSetting.Drag;
			}
			if ((bool)Dash)
			{
				_defaultDashSettings = new Dash.MoveSetting
				{
					Drag = Dash.DashDrag,
					Speed = Dash.DashMaxWalkingSpeed
				};
			}
		}
	}

	protected override void OnStayAreaEffect()
	{
		base.OnStayAreaEffect();
		ApplyMudEffects();
	}

	protected override void OnExitAreaEffect(Collider2D other)
	{
		base.OnExitAreaEffect(other);
		if (vfxOnExit != null)
		{
			ShowEffect(new Vector3(other.transform.position.x, base.transform.position.y + vfxYOffset, base.transform.position.z), vfxOnExit);
		}
		ApplyDefaultWalkValues();
		ApplyDefaultDashValues();
		ApplyDefaultAnimationSpeed();
	}

	private void ShowEffect(Vector3 position, GameObject fx)
	{
		PoolManager.Instance.ReuseObject(fx, position, Quaternion.identity);
	}

	public override void EnableEffect(bool enableEffect = true)
	{
		base.EnableEffect(enableEffect);
		ApplyDefaultWalkValues();
		ApplyDefaultDashValues();
		ApplyDefaultAnimationSpeed();
	}

	public void ApplyMudEffects()
	{
		if (Controller == null)
		{
			return;
		}
		Controller.JumpingSpeed = JumpingSpeed;
		Controller.WalkingDrag = WalkingDrag;
		Controller.WalkingAcc = WalkingAcceleration;
		Controller.MaxWalkingSpeed = MaxWalkingSpeed;
		if (!(Dash == null))
		{
			Dash.DashMoveSetting.Speed = DashSettings.Speed;
			Dash.DashMoveSetting.Drag = DashSettings.Drag;
			if (!(Animator == null))
			{
				Animator.speed = ((!Animator.GetCurrentAnimatorStateInfo(0).IsName("Run")) ? 1f : 0.7f);
			}
		}
	}

	private void ApplyDefaultWalkValues()
	{
		if (!(Controller == null))
		{
			Controller.JumpingSpeed = _defaultJumpingSpeed;
			Controller.WalkingDrag = _defaultWalkingDrag;
			Controller.WalkingAcc = _defaultWalkingAcceleration;
			Controller.MaxWalkingSpeed = _defaultMaxWalkingSpeed;
		}
	}

	private void ApplyDefaultDashValues()
	{
		if (!(Dash == null))
		{
			Dash.DashMoveSetting = _defaultDashSettings;
		}
	}

	private void ApplyDefaultAnimationSpeed()
	{
		if (!(Animator == null))
		{
			Animator.speed = 1f;
		}
	}

	private void OnDisable()
	{
		ApplyDefaultWalkValues();
		ApplyDefaultDashValues();
		ApplyDefaultAnimationSpeed();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawLine(base.transform.position + Vector3.left + Vector3.up * vfxYOffset, base.transform.position + Vector3.right + Vector3.up * vfxYOffset);
	}
}
