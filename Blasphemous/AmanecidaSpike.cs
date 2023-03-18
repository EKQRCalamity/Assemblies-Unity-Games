using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class AmanecidaSpike : MonoBehaviour
{
	private const string PLATFORM_LAYER = "OneWayDown";

	private const string FLOOR_LAYER = "Floor";

	[EventRef]
	public string appearSound;

	public float minHeight = 1f;

	public float maxHeight = 3f;

	private float initialLocalHeight;

	private float currentHeightTarget;

	private float currentHeightPercentage;

	public GameObject dustPrefab;

	public Vector2 effectOffset;

	private List<SpriteRenderer> renderers;

	private void ToPlatformLayer()
	{
		base.gameObject.layer = LayerMask.NameToLayer("OneWayDown");
	}

	private void ToFloorLayer()
	{
		base.gameObject.layer = LayerMask.NameToLayer("Floor");
	}

	private void Awake()
	{
		renderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
	}

	[Button("Show spike", ButtonSizes.Small)]
	public void Show(float timeToShow = 0.2f, float delay = 0f, float heightPercentage = 1f)
	{
		initialLocalHeight = base.transform.localPosition.y;
		base.transform.DOKill();
		ShowRenderers(show: true);
		currentHeightTarget = Mathf.Lerp(minHeight, maxHeight, heightPercentage);
		currentHeightPercentage = heightPercentage;
		ToPlatformLayer();
		Tweener tweener = base.transform.DOLocalMoveY(initialLocalHeight + currentHeightTarget, timeToShow).SetEase(Ease.OutCubic).SetDelay(0.1f + delay);
		tweener.onComplete = ToFloorLayer;
		tweener.onPlay = OnShow;
	}

	private void OnShow()
	{
		Core.Audio.PlaySfx(appearSound);
		Vector2 vector = (Vector2)base.transform.position + effectOffset;
		vector.y = initialLocalHeight - 1f;
		PoolManager.Instance.ReuseObject(dustPrefab, vector, Quaternion.identity);
	}

	[Button("Hide spike", ButtonSizes.Small)]
	public void Hide()
	{
		base.transform.DOKill();
		Tweener tweener = base.transform.DOLocalMoveY(initialLocalHeight, 0.4f).SetEase(Ease.InCubic);
		tweener.onComplete = OnHide;
	}

	private void ShowRenderers(bool show)
	{
		foreach (SpriteRenderer renderer in renderers)
		{
			renderer.enabled = show;
		}
	}

	private void OnHide()
	{
		ShowRenderers(show: false);
	}
}
