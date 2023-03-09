using UnityEngine;

public class EffectLayerRotationRandomizer : MonoBehaviour
{
	[SerializeField]
	private bool randomizeRotation = true;

	[SerializeField]
	private bool randomizeXFlip = true;

	[SerializeField]
	private bool randomizeYFlip = true;

	private void Awake()
	{
		if (randomizeRotation)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		}
		base.transform.localScale = new Vector3((!randomizeXFlip) ? base.transform.localScale.x : ((float)MathUtils.PlusOrMinus()), (!randomizeYFlip) ? base.transform.localScale.y : ((float)MathUtils.PlusOrMinus()));
		base.enabled = false;
	}
}
