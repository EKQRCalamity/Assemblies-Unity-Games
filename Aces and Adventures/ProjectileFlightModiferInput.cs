using System;
using UnityEngine;

public struct ProjectileFlightModiferInput
{
	public readonly System.Random random;

	public Vector3 start;

	public Vector3 end;

	public Vector3 up;

	public float radius;

	public float lifetime;

	public float distance => (end - start).magnitude;

	public ProjectileFlightModiferInput(System.Random random, Vector3 start, Vector3 end, Vector3 up, float radius, float lifetime)
	{
		this.random = random;
		this.start = start;
		this.end = end;
		this.up = up;
		this.radius = radius;
		this.lifetime = lifetime;
	}
}
