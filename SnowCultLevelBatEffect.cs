using UnityEngine;

public class SnowCultLevelBatEffect : Effect
{
	[SerializeField]
	private SpriteRenderer secondaryRenderer;

	[SerializeField]
	private string baseAnimName;

	protected string colorString;

	public void SetColor(string s)
	{
		colorString = s;
		base.animator.Play(baseAnimName + s);
		if ((bool)secondaryRenderer)
		{
			secondaryRenderer.flipX = Rand.Bool();
		}
	}
}
