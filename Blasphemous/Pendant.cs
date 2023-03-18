using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

public class Pendant : MonoBehaviour
{
	private Rigidbody2D rigidBody;

	private float force = 10f;

	private void Start()
	{
		rigidBody = GetComponent<Rigidbody2D>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		rigidBody.AddTorque((Core.Logic.Penitent.Status.Orientation != EntityOrientation.Left) ? force : (0f - force));
	}
}
