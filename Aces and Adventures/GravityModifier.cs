using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityModifier : MonoBehaviour
{
	public float gravityMultiplier = 1f;

	private Rigidbody _body;

	private void Awake()
	{
		_body = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (Math.Abs(gravityMultiplier - 1f) > MathUtil.BigEpsilon)
		{
			_body.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
		}
	}
}
