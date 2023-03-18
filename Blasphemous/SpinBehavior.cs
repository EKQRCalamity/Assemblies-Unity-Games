using UnityEngine;

public class SpinBehavior : MonoBehaviour
{
	public float angularSpeed;

	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, 0f, angularSpeed * Time.deltaTime));
	}
}
