using UnityEngine;

public class ChaliceTutorialLevelParryable : ParrySwitch
{
	private const float FADE_SPEED = 5f;

	[SerializeField]
	private bool firstOne;

	[SerializeField]
	private SpriteRenderer rend;

	public bool isDeactivated { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Deactivated();
	}

	public override void OnParryPostPause(AbstractPlayerController player)
	{
		base.OnParryPostPause(player);
		Deactivated();
	}

	public override void OnParryPrePause(AbstractPlayerController player)
	{
		base.OnParryPrePause(player);
		AudioManager.Play("sfx_parry_pink_shows");
	}

	public void Deactivated()
	{
		GetComponent<Collider2D>().enabled = false;
		isDeactivated = true;
	}

	public void Activated()
	{
		GetComponent<Collider2D>().enabled = true;
		isDeactivated = false;
	}

	private void Update()
	{
		rend.color = new Color(1f, 1f, 1f, Mathf.Clamp(rend.color.a + ((!isDeactivated) ? ((float)CupheadTime.Delta) : (0f - (float)CupheadTime.Delta)) * 5f, 0f, 1f));
	}
}
