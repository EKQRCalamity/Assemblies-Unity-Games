using Framework.Util;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Traps.FireTrap;

[RequireComponent(typeof(AttackArea))]
public class BeamAttack : MonoBehaviour
{
	private AttackArea _attackArea;

	public TileableBeamLauncher BeamLauncher;

	[SerializeField]
	public Hit lightningHit;

	public bool UseAttackCooldown;

	[ShowIf("UseAttackCooldown", true)]
	public float TimeBetweenTicks = 0.3f;

	private bool isAttackWindowOpen;

	private float currentAttackCd;

	private void OnEnable()
	{
		if ((bool)BeamLauncher)
		{
			SetColliderWidth(BeamLauncher.maxRange);
		}
	}

	private void Awake()
	{
		_attackArea = GetComponent<AttackArea>();
		_attackArea.OnStay += AttackAreaOnStay;
		if (!BeamLauncher)
		{
			Debug.LogError("The Beam launcher member is not set!");
		}
	}

	private void Update()
	{
		if (isAttackWindowOpen && UseAttackCooldown && currentAttackCd > 0f)
		{
			currentAttackCd -= Time.deltaTime;
		}
	}

	private void SetColliderWidth(float width)
	{
		BoxCollider2D boxCollider2D = (BoxCollider2D)_attackArea.WeaponCollider;
		boxCollider2D.size = new Vector2(width, boxCollider2D.size.y);
	}

	private void AttackAreaOnStay(object sender, Collider2DParam e)
	{
		IDamageable componentInParent = e.Collider2DArg.GetComponentInParent<IDamageable>();
		Attack(componentInParent);
	}

	private void Attack(IDamageable damageable)
	{
		if (!isAttackWindowOpen || damageable == null)
		{
			return;
		}
		if (UseAttackCooldown)
		{
			if (currentAttackCd <= 0f)
			{
				currentAttackCd = TimeBetweenTicks;
				damageable.Damage(lightningHit);
			}
		}
		else
		{
			damageable.Damage(lightningHit);
		}
	}

	public void OpenAttackWindow()
	{
		isAttackWindowOpen = true;
	}

	public void CloseAttackWindow()
	{
		isAttackWindowOpen = false;
	}

	private void OnDestroy()
	{
		if ((bool)_attackArea)
		{
			_attackArea.OnEnter -= AttackAreaOnStay;
		}
	}
}
