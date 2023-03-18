using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class CurvedProjectile : Projectile
{
	[MinMaxSlider(-2f, 2f, true)]
	public Vector2 xDisplacementAtTarget;

	public float yDisplacementAtPeak = 4f;

	public bool faceVelocityDirection;

	protected int originalDamage;

	private Vector2 lastPos;

	private float speed;

	private ResetTrailRendererOnEnable trailRendererCleaner;

	public int OriginalDamage
	{
		get
		{
			return originalDamage;
		}
		set
		{
			originalDamage = value;
		}
	}

	public virtual void Init(Vector3 origin, Vector3 target, float speed)
	{
		this.speed = speed;
		target.x += Random.Range(xDisplacementAtTarget.x, xDisplacementAtTarget.y);
		Vector2 vector = target - origin;
		Vector2 vector2 = new Vector2(origin.x + vector.x / 2f, Mathf.Max(target.y, base.transform.position.y) + yDisplacementAtPeak);
		float num = Vector2.Distance(origin, vector2) + Vector2.Distance(vector2, target);
		float time = num / speed;
		base.transform.DOMoveX(target.x, time).SetEase(Ease.Linear);
		base.transform.DOMoveY(vector2.y, time / 2f).SetEase(Ease.OutQuad).OnComplete(delegate
		{
			base.transform.DOMoveY(target.y, time / 2f).SetEase(Ease.InQuad);
		});
		lastPos = base.transform.position;
		if (!trailRendererCleaner)
		{
			trailRendererCleaner = GetComponentInChildren<ResetTrailRendererOnEnable>();
		}
		if ((bool)trailRendererCleaner)
		{
			trailRendererCleaner.Clean();
		}
	}

	protected override void OnUpdate()
	{
		UpdateOrientation();
		if (DOTween.IsTweening(base.transform))
		{
			velocity = ((Vector2)base.transform.position - lastPos).normalized * speed;
			lastPos = base.transform.position;
		}
		if (faceVelocityDirection)
		{
			float z = 57.29578f * Mathf.Atan2(velocity.y, velocity.x);
			base.transform.eulerAngles = new Vector3(0f, 0f, z);
		}
		if (!DOTween.IsTweening(base.transform))
		{
			base.transform.Translate(velocity * Time.deltaTime, Space.World);
			velocity.y *= 1.1f;
		}
		_currentTTL -= Time.deltaTime;
		if (_currentTTL < 0f)
		{
			OnLifeEnded();
		}
	}
}
