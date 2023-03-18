using System;
using System.Collections;
using DG.Tweening;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class TriggerBasedTrap : MonoBehaviour, IActionable, IDamageable
{
	public enum TRIGGER_TRAP_STATES
	{
		IDLE,
		CHARGING,
		ACTIVE
	}

	public GameObject toInstantiate;

	public Animator animator;

	public string triggerID;

	public Vector2 offset;

	public string playerLayer = "Water";

	public bool reactToPlayer = true;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	protected string OnHitSound;

	private Collider2D lastArea;

	public TRIGGER_TRAP_STATES currentState;

	public float cooldown = 1f;

	private float _cdCounter;

	public float chargeTime = 0.5f;

	private float _chargeCounter;

	public float minDelay;

	public float maxDelay;

	public float lastDelay;

	private const string ATTACK_TRIGGER = "ATTACK";

	public bool Locked { get; set; }

	public event Action<TriggerBasedTrap> OnUsedEvent;

	private void Start()
	{
		PoolManager.Instance.CreatePool(toInstantiate, 1);
	}

	protected virtual void OnUsed()
	{
		if (maxDelay == 0f)
		{
			ActivateTrap();
		}
		else
		{
			lastDelay = UnityEngine.Random.Range(minDelay, maxDelay);
			StartCoroutine(DelayedActivateTrap(lastDelay));
		}
		if (this.OnUsedEvent != null)
		{
			this.OnUsedEvent(this);
		}
	}

	private IEnumerator DelayedActivateTrap(float s)
	{
		yield return new WaitForSeconds(s);
		ActivateTrap();
	}

	private void ActivateTrap()
	{
		ShockwaveArea component = PoolManager.Instance.ReuseObject(toInstantiate, base.transform.position + (Vector3)offset, Quaternion.identity).GameObject.GetComponent<ShockwaveArea>();
		if (animator != null)
		{
			animator.SetTrigger("ATTACK");
		}
		lastArea = component.GetComponentInChildren<Collider2D>();
		_cdCounter = cooldown;
		currentState = TRIGGER_TRAP_STATES.ACTIVE;
	}

	public void Use()
	{
		OnUsed();
	}

	private void Update()
	{
		if (currentState == TRIGGER_TRAP_STATES.ACTIVE)
		{
			UpdateActive();
		}
		else if (currentState == TRIGGER_TRAP_STATES.CHARGING)
		{
			UpdateCharging();
		}
	}

	private void UpdateActive()
	{
		if (_cdCounter > 0f)
		{
			_cdCounter -= Time.deltaTime;
		}
		else
		{
			currentState = TRIGGER_TRAP_STATES.IDLE;
		}
	}

	private void UpdateCharging()
	{
		if (_chargeCounter > 0f)
		{
			_chargeCounter -= Time.deltaTime;
		}
		else
		{
			Use();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(base.transform.position + (Vector3)offset, 0.1f);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (currentState == TRIGGER_TRAP_STATES.IDLE && !(collision == lastArea))
		{
			TrapTriggererArea component = collision.gameObject.GetComponent<TrapTriggererArea>();
			if (component != null && component.triggerID == triggerID)
			{
				Use();
			}
		}
	}

	public void Damage(Hit hit)
	{
		if (currentState == TRIGGER_TRAP_STATES.IDLE && reactToPlayer)
		{
			currentState = TRIGGER_TRAP_STATES.CHARGING;
			_chargeCounter = chargeTime;
			base.transform.DOPunchScale(Vector3.one * 0.2f, chargeTime);
		}
		Core.Audio.PlayOneShot(OnHitSound);
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return true;
	}
}
