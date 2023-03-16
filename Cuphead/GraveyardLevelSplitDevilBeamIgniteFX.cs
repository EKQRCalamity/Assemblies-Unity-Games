using UnityEngine;

public class GraveyardLevelSplitDevilBeamIgniteFX : Effect
{
	[SerializeField]
	private Animator fireBeamAnimator;

	[SerializeField]
	private SpriteRenderer[] groundRends;

	[SerializeField]
	private SpriteRenderer[] noGroundRends;

	private float frameTimer;

	public Effect Create(Vector3 position, Animator fireBeamAnimator)
	{
		GraveyardLevelSplitDevilBeamIgniteFX graveyardLevelSplitDevilBeamIgniteFX = base.Create(position) as GraveyardLevelSplitDevilBeamIgniteFX;
		graveyardLevelSplitDevilBeamIgniteFX.fireBeamAnimator = fireBeamAnimator;
		graveyardLevelSplitDevilBeamIgniteFX.UpdateFade(1f);
		return graveyardLevelSplitDevilBeamIgniteFX;
	}

	private void Update()
	{
		frameTimer += CupheadTime.Delta;
		while (frameTimer > 1f / 24f)
		{
			frameTimer -= 1f / 24f;
			UpdateFade(0.25f);
		}
	}

	private void UpdateFade(float amount)
	{
		bool @bool = fireBeamAnimator.GetBool("Smoke");
		SpriteRenderer[] array = groundRends;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Clamp(spriteRenderer.color.a + ((!@bool) ? (0f - amount) : amount), 0f, 1f));
		}
		SpriteRenderer[] array2 = noGroundRends;
		foreach (SpriteRenderer spriteRenderer2 in array2)
		{
			spriteRenderer2.color = new Color(spriteRenderer2.color.r, spriteRenderer2.color.g, spriteRenderer2.color.b, Mathf.Clamp(spriteRenderer2.color.a + ((!@bool) ? amount : (0f - amount)), 0f, 1f));
		}
	}
}
