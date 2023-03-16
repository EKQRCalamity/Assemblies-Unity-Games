using UnityEngine;

public class HouseElderKettlePlayerRun : MonoBehaviour
{
	[SerializeField]
	private Effect runEffect;

	[SerializeField]
	private Transform runDustRoot;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.localPosition += new Vector3(-490f, 0f, 0f) * CupheadTime.FixedDelta;
	}

	private void OnRunDust()
	{
		runEffect.Create(runDustRoot.position);
	}
}
