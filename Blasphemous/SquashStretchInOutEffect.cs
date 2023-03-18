using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using UnityEngine;

public class SquashStretchInOutEffect : MonoBehaviour
{
	private MasterShaderEffects shaderEffects;

	private SpriteRenderer sRenderer;

	public GameObject instantiateOnDissappear;

	public Vector2 effectOffset;

	public float colorizeSeconds = 0.2f;

	public float scaleSeconds = 0.3f;

	public float fadeSeconds = 0.4f;

	public Vector2 squashScaleXY;

	private void Start()
	{
		shaderEffects = GetComponent<MasterShaderEffects>();
		sRenderer = GetComponent<SpriteRenderer>();
		if (instantiateOnDissappear != null)
		{
			PoolManager.Instance.CreatePool(instantiateOnDissappear, 1);
		}
	}

	public void Dissappear()
	{
		shaderEffects.TriggerColorizeLerp(0f, 0.75f, colorizeSeconds, SquashStretchOut);
		shaderEffects.TriggerColorTintLerp(0f, 1f, colorizeSeconds);
	}

	private void SquashStretchOut()
	{
		base.transform.DOScaleX(squashScaleXY.x, scaleSeconds).SetEase(Ease.InBack);
		base.transform.DOScaleY(squashScaleXY.y, scaleSeconds).SetEase(Ease.InBack).OnComplete(delegate
		{
			sRenderer.DOFade(0f, fadeSeconds);
		});
		shaderEffects.TriggerColorizeLerp(0.75f, 0f, colorizeSeconds);
		if (instantiateOnDissappear != null)
		{
			PoolManager.Instance.ReuseObject(instantiateOnDissappear, (Vector2)base.transform.position + effectOffset, Quaternion.identity);
		}
	}

	private void SquashStretchIn()
	{
		base.transform.localScale = new Vector3(squashScaleXY.x, squashScaleXY.y, 1f);
		sRenderer.color = Color.white;
		sRenderer.DOFade(1f, fadeSeconds);
		base.transform.DOScaleX(1f, scaleSeconds).SetEase(Ease.InBack);
		base.transform.DOScaleY(1f, scaleSeconds).SetEase(Ease.InBack);
	}

	private void OnDrawGizmosSelected()
	{
		if (effectOffset != Vector2.zero)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere((Vector2)base.transform.position + effectOffset, 0.1f);
		}
	}
}
