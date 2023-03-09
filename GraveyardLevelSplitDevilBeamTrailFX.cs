using UnityEngine;

public class GraveyardLevelSplitDevilBeamTrailFX : Effect
{
	[SerializeField]
	private GraveyardLevelSplitDevilBeam main;

	[SerializeField]
	private SpriteRenderer rend;

	private float frameTimer;

	public Effect Create(Vector3 position, Vector3 scale, GraveyardLevelSplitDevilBeam main, int anim)
	{
		GraveyardLevelSplitDevilBeamTrailFX graveyardLevelSplitDevilBeamTrailFX = base.Create(position) as GraveyardLevelSplitDevilBeamTrailFX;
		graveyardLevelSplitDevilBeamTrailFX.transform.localScale = scale;
		graveyardLevelSplitDevilBeamTrailFX.main = main;
		graveyardLevelSplitDevilBeamTrailFX.animator.Play(anim.ToString());
		graveyardLevelSplitDevilBeamTrailFX.animator.Update(0f);
		graveyardLevelSplitDevilBeamTrailFX.rend.sortingOrder = -5 + anim;
		graveyardLevelSplitDevilBeamTrailFX.UpdateFade(1f);
		return graveyardLevelSplitDevilBeamTrailFX;
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
		rend.color = new Color(rend.color.r, rend.color.g, rend.color.b, Mathf.Clamp(rend.color.a + (main.devil.isAngel ? (0f - amount) : amount), 0f, 1f));
	}
}
