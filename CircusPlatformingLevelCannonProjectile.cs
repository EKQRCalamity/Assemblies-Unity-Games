using UnityEngine;

public class CircusPlatformingLevelCannonProjectile : BasicProjectile
{
	private const string VariationParameterName = "Variation";

	private const string Pink = "P";

	private const string Green = "G";

	private const string Orange = "O";

	public void SetColor(string color)
	{
		int num = Random.Range(0, 2);
		switch (color)
		{
		case "P":
			SetParryable(parryable: true);
			base.animator.SetInteger("Variation", num);
			break;
		case "G":
			base.animator.SetInteger("Variation", num + 2);
			break;
		case "O":
			base.animator.SetInteger("Variation", num + 4);
			break;
		}
	}
}
