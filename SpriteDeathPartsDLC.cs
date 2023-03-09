using UnityEngine;

public class SpriteDeathPartsDLC : SpriteDeathParts
{
	private const float UPDATE_TIMING_ADJUST = 1.2f;

	[SerializeField]
	private bool progressiveBlur;

	[SerializeField]
	private float blurIncreaseSpeed = 3f;

	[SerializeField]
	private bool progressiveDim;

	[SerializeField]
	private float dimIncreaseSpeed = 3f;

	private Color startColor;

	private float dimTimer;

	[SerializeField]
	private SpriteRenderer rend;

	private void Start()
	{
		if (progressiveBlur)
		{
			rend.material.SetFloat("_BlurAmount", 0f);
			rend.material.SetFloat("_BlurLerp", 0f);
		}
		if (progressiveDim)
		{
			startColor = rend.color;
		}
	}

	public void SetVelocity(Vector3 vel)
	{
		velocity = vel;
	}

	private void FixedUpdate()
	{
		if (CupheadTime.FixedDelta > 0f)
		{
			Step(CupheadTime.FixedDelta * 1.2f);
		}
	}

	protected override void Step(float deltaTime)
	{
		base.Step(deltaTime);
		if (progressiveBlur)
		{
			rend.material.SetFloat("_BlurAmount", rend.material.GetFloat("_BlurAmount") + deltaTime * blurIncreaseSpeed);
			rend.material.SetFloat("_BlurLerp", rend.material.GetFloat("_BlurLerp") + deltaTime * blurIncreaseSpeed);
		}
		if (progressiveDim)
		{
			dimTimer += deltaTime * dimIncreaseSpeed;
			rend.color = Color.Lerp(startColor, Color.black, dimTimer);
		}
	}

	protected override void Update()
	{
	}
}
