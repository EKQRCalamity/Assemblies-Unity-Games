using System.Collections;
using FMODUnity;
using Framework.Managers;
using Framework.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

public class TileableBeamLauncher : PoolObject
{
	public LayerMask beamCollisionMask;

	public SpriteRenderer bodySprite;

	public Transform endSprite;

	public Transform endSprite2;

	public Transform permanentEndSprite;

	public Transform growSprite;

	public Animator endAnimator;

	public Animator bodyAnimator;

	public float maxRange;

	public bool stretchWidth = true;

	public bool stretchCollider;

	[ShowIf("stretchCollider", true)]
	public BoxCollider2D collider;

	public bool displayEndAnimation = true;

	public bool hasSfxOnActivation;

	[ShowIf("hasSfxOnActivation", true)]
	[EventRef]
	public string sfxOnActivation;

	private static readonly int Active = Animator.StringToHash("ACTIVE");

	private static readonly int Warning = Animator.StringToHash("WARNING");

	private static readonly int Beam = Animator.StringToHash("BEAM");

	private RaycastHit2D[] results;

	private void Start()
	{
		results = new RaycastHit2D[1];
	}

	public float GetDistance()
	{
		if (Physics2D.RaycastNonAlloc(base.transform.position, base.transform.right, results, maxRange, beamCollisionMask) > 0)
		{
			Vector2 point = results[0].point;
			Debug.DrawLine(base.transform.position, point, Color.magenta, 6f);
			return Vector2.Distance(point, base.transform.position);
		}
		return maxRange;
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		results = new RaycastHit2D[1];
	}

	private void Update()
	{
		LaunchBeam();
	}

	public void ActivateEndAnimation(bool active, bool applyChanges = false)
	{
		displayEndAnimation = active;
		if (applyChanges && (bool)endAnimator)
		{
			endAnimator.SetBool(Active, displayEndAnimation);
		}
	}

	[Button(ButtonSizes.Small)]
	public void TriggerBeamBodyAnim()
	{
		Animator componentInChildren = bodySprite.GetComponentInChildren<Animator>();
		if ((bool)componentInChildren)
		{
			componentInChildren.SetTrigger(Beam);
		}
	}

	public void ActivateDelayedBeam(float delay, bool warningAnimation)
	{
		if (warningAnimation)
		{
			bodyAnimator.SetTrigger(Warning);
		}
		if (hasSfxOnActivation)
		{
			Core.Audio.PlayOneShot(sfxOnActivation);
		}
		displayEndAnimation = false;
		StartCoroutine(ActivateBeamCoroutine(delay));
	}

	private IEnumerator ActivateBeamCoroutine(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		ActivateBeamAnimation(active: true);
	}

	public void ClearAll()
	{
		StopAllCoroutines();
	}

	public void ActivateBeamAnimation(bool active)
	{
		displayEndAnimation = active;
		bodyAnimator.SetBool(Active, active);
	}

	private void LaunchBeam()
	{
		Vector2 vector;
		if (Physics2D.RaycastNonAlloc(base.transform.position, base.transform.right, results, maxRange, beamCollisionMask) > 0)
		{
			vector = results[0].point;
			if ((bool)endSprite)
			{
				endSprite.position = vector;
				endSprite.up = results[0].normal;
			}
			if ((bool)endSprite2)
			{
				endSprite2.position = vector;
				endSprite2.up = results[0].normal;
			}
			if (displayEndAnimation)
			{
				if ((bool)endAnimator)
				{
					endAnimator.SetBool(Active, value: true);
				}
				endSprite.gameObject.SetActive(value: true);
				if ((bool)endSprite2)
				{
					endSprite2.gameObject.SetActive(value: true);
				}
			}
			else if ((bool)endSprite)
			{
				endSprite.gameObject.SetActive(value: false);
				if ((bool)endSprite2)
				{
					endSprite2.gameObject.SetActive(value: false);
				}
			}
			if ((bool)growSprite)
			{
				growSprite.gameObject.SetActive(value: false);
			}
			GizmoExtensions.DrawDebugCross(base.transform.position, Color.green, 0.1f);
			GizmoExtensions.DrawDebugCross(vector, Color.green, 0.1f);
		}
		else
		{
			if (displayEndAnimation)
			{
				if ((bool)endAnimator)
				{
					endAnimator.SetBool(Active, value: false);
				}
			}
			else if ((bool)endSprite)
			{
				endSprite.gameObject.SetActive(value: false);
				if ((bool)endSprite2)
				{
					endSprite2.gameObject.SetActive(value: false);
				}
			}
			vector = base.transform.position + base.transform.right * maxRange;
			if ((bool)growSprite)
			{
				growSprite.gameObject.SetActive(value: true);
				growSprite.position = vector;
			}
			if ((bool)permanentEndSprite)
			{
				permanentEndSprite.transform.position = vector;
			}
			Color c = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
			GizmoExtensions.DrawDebugCross(base.transform.position, c, 0.1f);
			GizmoExtensions.DrawDebugCross(vector, c, 0.1f);
		}
		DrawBeam(base.transform.position, vector);
	}

	private void DrawBeam(Vector2 origin, Vector2 end)
	{
		float magnitude = (end - origin).magnitude;
		if (stretchWidth)
		{
			bodySprite.size = new Vector2(magnitude, bodySprite.size.y);
			if (stretchCollider)
			{
				collider.size = new Vector2(magnitude, collider.size.y);
			}
		}
		else
		{
			bodySprite.size = new Vector2(bodySprite.size.x, magnitude);
			if (stretchCollider)
			{
				collider.size = new Vector2(collider.size.x, magnitude);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.right * maxRange);
	}
}
