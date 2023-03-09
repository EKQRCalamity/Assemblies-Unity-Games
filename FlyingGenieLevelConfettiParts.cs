using UnityEngine;

public class FlyingGenieLevelConfettiParts : SpriteDeathParts
{
	[SerializeField]
	private SpriteRenderer purpleVersion;

	public FlyingGenieLevelConfettiParts CreatePart(Vector3 position, Color purpleColor)
	{
		FlyingGenieLevelConfettiParts flyingGenieLevelConfettiParts = CreatePart(position) as FlyingGenieLevelConfettiParts;
		flyingGenieLevelConfettiParts.purpleVersion.color = purpleColor;
		return flyingGenieLevelConfettiParts;
	}
}
