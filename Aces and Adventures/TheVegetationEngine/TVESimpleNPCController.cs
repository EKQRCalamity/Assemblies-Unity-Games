using UnityEngine;

namespace TheVegetationEngine;

public class TVESimpleNPCController : MonoBehaviour
{
	private float timeToChangeDirection;

	private Vector3 direction;

	public void Start()
	{
		ChangeDirection();
	}

	public void Update()
	{
		timeToChangeDirection -= Time.deltaTime;
		if (timeToChangeDirection <= 0f)
		{
			ChangeDirection();
		}
		base.transform.Translate(direction, Space.World);
	}

	private void ChangeDirection()
	{
		float num = Random.Range(0.005f, 0.01f);
		direction = new Vector3(Random.Range(-1f, 1f) * num, 0f, Random.Range(-1f, 1f) * num);
		timeToChangeDirection = Random.Range(0.5f, 2f);
	}
}
