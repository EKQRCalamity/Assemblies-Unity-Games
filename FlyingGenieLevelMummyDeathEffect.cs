using UnityEngine;

public class FlyingGenieLevelMummyDeathEffect : Effect
{
	[SerializeField]
	private FlyingGenieLevelConfettiParts[] parts;

	private Color purpleColor;

	public FlyingGenieLevelMummyDeathEffect Create(Vector3 pos, Color purpleColor)
	{
		FlyingGenieLevelMummyDeathEffect flyingGenieLevelMummyDeathEffect = base.Create(pos) as FlyingGenieLevelMummyDeathEffect;
		flyingGenieLevelMummyDeathEffect.transform.position = pos;
		flyingGenieLevelMummyDeathEffect.purpleColor = purpleColor;
		return flyingGenieLevelMummyDeathEffect;
	}

	private void CreateConfetti()
	{
		FlyingGenieLevelConfettiParts[] array = parts;
		foreach (FlyingGenieLevelConfettiParts flyingGenieLevelConfettiParts in array)
		{
			flyingGenieLevelConfettiParts.CreatePart(base.transform.position, purpleColor);
		}
	}
}
