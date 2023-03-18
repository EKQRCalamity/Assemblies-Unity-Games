using DG.Tweening;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class FaithPlatform : MonoBehaviour, IActionable, INoSafePosition
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool firstPlatform;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private GameObject[] target;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[Range(0f, 2f)]
	private float swichTime = 1f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	private string appearSound;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[EventRef]
	private string disappearSound;

	public bool showing;

	private Tween currentTween;

	private float activationDelay;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[Range(0f, 10f)]
	private float deactivationDelay;

	private bool penitentTouching;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Color activeColor;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Color disabledColor;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private CollisionSensor collisionSensor;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private Collider2D collision;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private Animator animator;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private GameObject nextPlatformParticles;

	public bool Locked { get; set; }

	private void Awake()
	{
		if (collision != null)
		{
			collision.enabled = false;
		}
		Core.Events.OnFlagChanged += OnFlagChanged;
		if (collisionSensor != null)
		{
			collisionSensor.SensorTriggerEnter += TriggerEnter2D;
			collisionSensor.SensorTriggerExit += TriggerExit2D;
		}
	}

	private void Start()
	{
		PoolManager.Instance.CreatePool(nextPlatformParticles, target.Length);
	}

	private void OnDestroy()
	{
		Core.Events.OnFlagChanged -= OnFlagChanged;
		if (collisionSensor != null)
		{
			collisionSensor.SensorTriggerEnter -= TriggerEnter2D;
			collisionSensor.SensorTriggerExit -= TriggerExit2D;
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		Debug.Log(other.collider.tag);
	}

	private void TriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("PenitentFeet"))
		{
			Show(activationDelay);
			ShowNextPlatform(activationDelay);
		}
	}

	private void TriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("PenitentFeet"))
		{
			Hide(deactivationDelay);
			HideNextPlatform(deactivationDelay);
		}
	}

	private void OnFlagChanged(string flag, bool flagActive)
	{
		if (flag == "REVEAL_FAITH_PLATFORMS" && flagActive && firstPlatform)
		{
			ShowAction();
		}
		if (flag == "REVEAL_FAITH_PLATFORMS" && !flagActive && showing)
		{
			HideAction();
		}
	}

	private void Show(float delay)
	{
		CancelInvoke("HideAction");
		Invoke("ShowAction", delay);
	}

	public void Hide(float delay)
	{
		if (!firstPlatform)
		{
			CancelInvoke("ShowAction");
			Invoke("HideAction", delay);
		}
	}

	private void ShowAction()
	{
		if (spriteRenderer.isVisible && !showing)
		{
			Core.Audio.PlaySfx(appearSound);
		}
		animator.SetBool("ENABLED", value: true);
		showing = true;
		spriteRenderer.DOColor(activeColor, swichTime);
		collision.enabled = true;
	}

	private void HideAction()
	{
		if (spriteRenderer.isVisible && showing)
		{
			Core.Audio.PlaySfx(disappearSound);
		}
		animator.SetBool("ENABLED", value: false);
		showing = false;
		spriteRenderer.DOColor(disabledColor, swichTime);
		collision.enabled = false;
	}

	private void ShowNextPlatform(float delay)
	{
		for (int i = 0; i < target.Length; i++)
		{
			if (!(target[i] == null))
			{
				FaithPlatform component = target[i].GetComponent<FaithPlatform>();
				if (component != null)
				{
					EffectToNextPlatform(component);
					component.Show(delay);
				}
			}
		}
	}

	private void EffectToNextPlatform(FaithPlatform p)
	{
		Vector3 position = base.transform.position;
		Vector3 position2 = p.transform.position;
		GameObject gameObject = PoolManager.Instance.ReuseObject(nextPlatformParticles, position, Quaternion.identity).GameObject;
		gameObject.transform.DOMove(position2, 0.2f).SetEase(Ease.OutCubic);
	}

	private void HideNextPlatform(float delay)
	{
		for (int i = 0; i < target.Length; i++)
		{
			if (!(target[i] == null))
			{
				FaithPlatform component = target[i].GetComponent<FaithPlatform>();
				if (component != null)
				{
					component.Hide(delay);
				}
			}
		}
	}

	public void Use()
	{
		Invoke("Show", activationDelay);
	}
}
