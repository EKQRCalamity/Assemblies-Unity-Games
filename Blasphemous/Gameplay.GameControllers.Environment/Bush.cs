using UnityEngine;

namespace Gameplay.GameControllers.Environment;

public class Bush : Vegetation
{
	public float timeShakingEntityPassing;

	private float deltaPlayTime;

	private void Awake()
	{
		plantAnimator = GetComponent<Animator>();
		plantCollider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		plantAnimator.speed = 0f;
	}

	private void Update()
	{
		if (isShaking)
		{
			Shaking(timeShakingEntityPassing);
		}
	}

	public override void Shaking(float timeShaking)
	{
		deltaPlayTime += Time.deltaTime;
		if (deltaPlayTime <= timeShaking)
		{
			plantAnimator.speed = 1f;
			return;
		}
		isShaking = !isShaking;
		deltaPlayTime = 0f;
		plantAnimator.speed = 0f;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Penitent") && !isShaking)
		{
			isShaking = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Penitent") && !isShaking)
		{
			isShaking = true;
		}
	}
}
