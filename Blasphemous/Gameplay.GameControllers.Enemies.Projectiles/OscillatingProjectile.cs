using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Projectiles;

public class OscillatingProjectile : StraightProjectile
{
	public float maxAmplitude = 1f;

	public float minAmplitude;

	public float maxFrequency = 1f;

	public float minFrequency;

	private float amplitude = 1f;

	private float frequency = 1f;

	private Vector2 origin;

	[Tooltip("Randomizes both the amplitude and frequency from zero to value set.")]
	public bool RandSettings;

	private void OnEnable()
	{
		amplitude = Random.Range(minAmplitude, maxAmplitude);
		frequency = Random.Range(minFrequency, maxFrequency);
		origin = base.transform.position;
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		float num = Mathf.Sin(Time.time * frequency) * amplitude;
		base.transform.position = new Vector2(base.transform.position.x, origin.y + num);
	}
}
