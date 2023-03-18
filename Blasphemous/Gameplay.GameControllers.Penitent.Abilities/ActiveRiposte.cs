using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class ActiveRiposte : Ability
{
	[FoldoutGroup("Active Riposte Settings", 0)]
	[Range(0f, 2f)]
	public float OpportunityWindowOffset;

	private float _opportunityOffsetLapse;

	[FoldoutGroup("Active Riposte Settings", 0)]
	public Vector2 RiposteEffectOffset;

	[FoldoutGroup("Active Riposte Settings", 0)]
	public GameObject RiposteEffectObject;

	[FoldoutGroup("Active Riposte Settings", 0)]
	public int RiposteEffectPoolSize = 1;

	private float _opportunityWindowLapse;

	private WaitForSeconds _unsetActiveRiposteDelay;

	private const int TotalNumberOfRighteousRipostesForAc28 = 5;

	public bool IsTriggeredRiposte { get; set; }

	private bool AttackTrigger => Core.Logic.Penitent.PlatformCharacterInput.Rewired.GetButtonDown(5);

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)RiposteEffectObject)
		{
			PoolManager.Instance.CreatePool(RiposteEffectObject, RiposteEffectPoolSize);
		}
		_unsetActiveRiposteDelay = new WaitForSeconds(1f);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!base.Casting)
		{
			return;
		}
		_opportunityOffsetLapse -= Time.deltaTime;
		if (_opportunityOffsetLapse <= 0f)
		{
			_opportunityWindowLapse -= Time.deltaTime;
			if (_opportunityWindowLapse >= 0f)
			{
				if (AttackTrigger)
				{
					TriggerActiveRiposte();
				}
			}
			else
			{
				StopCast();
			}
		}
		else if (AttackTrigger)
		{
			StopCast();
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		ResetWindowOpportunityTimers();
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
	}

	private void ResetWindowOpportunityTimers()
	{
		_opportunityOffsetLapse = OpportunityWindowOffset;
		_opportunityWindowLapse = base.EntityOwner.Stats.ActiveRiposteWindow.Final;
	}

	private void TriggerActiveRiposte()
	{
		if (!IsTriggeredRiposte)
		{
			IsTriggeredRiposte = true;
			StartCoroutine(UnsetActiveRiposte());
		}
	}

	private IEnumerator UnsetActiveRiposte()
	{
		yield return _unsetActiveRiposteDelay;
		if (IsTriggeredRiposte)
		{
			IsTriggeredRiposte = !IsTriggeredRiposte;
		}
	}

	public void MakeRiposte()
	{
		AddProgressToAC28();
		InstantiateRiposteEffect();
	}

	private void AddProgressToAC28()
	{
		if (!Core.AchievementsManager.Achievements["AC28"].IsGranted())
		{
			Core.AchievementsManager.Achievements["AC28"].AddProgress(20f);
		}
	}

	private void InstantiateRiposteEffect()
	{
		if (!(RiposteEffectObject == null))
		{
			PoolManager.Instance.ReuseObject(RiposteEffectObject, GetRiposteEffectPosition(), Quaternion.identity);
		}
	}

	private Vector2 GetRiposteEffectPosition()
	{
		Vector3 position = base.EntityOwner.transform.position;
		EntityOrientation orientation = base.EntityOwner.Status.Orientation;
		float x = ((orientation != EntityOrientation.Left) ? (position.x + RiposteEffectOffset.x) : (position.x - RiposteEffectOffset.x));
		return new Vector2(x, position.y + RiposteEffectOffset.y);
	}
}
