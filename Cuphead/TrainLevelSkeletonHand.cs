using UnityEngine;

public class TrainLevelSkeletonHand : AbstractTrainLevelSkeletonPart
{
	[SerializeField]
	private Transform effectRoot;

	[SerializeField]
	private Effect effectPrefab;

	private void PlaySlapFX()
	{
		effectPrefab.Create(effectRoot.position, base.transform.localScale);
		CupheadLevelCamera.Current.Shake(20f, 0.6f);
	}

	public void Slap()
	{
		base.animator.Play("Slap");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		effectPrefab = null;
	}
}
