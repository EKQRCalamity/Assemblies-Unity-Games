using DG.Tweening;
using UnityEngine;

public class IsidoraMaskLight : MonoBehaviour
{
	public float initialRadius = 1f;

	public float timeToMaxRadius = 1f;

	public float maxRadius = 3f;

	public float fluctuation = 0.5f;

	public float fluctuationTime = 0.5f;

	private void OnEnable()
	{
		base.transform.localScale = Vector3.one * initialRadius * 2f;
		base.transform.DOScale(maxRadius * 2f, timeToMaxRadius).SetEase(Ease.OutQuad).OnComplete(delegate
		{
			base.transform.DOScale((maxRadius - fluctuation) * 2f, fluctuationTime).SetDelay(0.6f).SetEase(Ease.InOutQuad)
				.SetLoops(-1, LoopType.Yoyo);
		});
	}

	private void OnDisable()
	{
		base.transform.DOKill();
	}
}
