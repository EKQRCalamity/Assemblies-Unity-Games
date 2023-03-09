using UnityEngine;

public class SaltbakerLevelMintHandSFXHandler : MonoBehaviour
{
	[SerializeField]
	private SaltbakerLevelSaltbaker main;

	private void AniEvent_SFXLeafRustle()
	{
		main.SFXLeafRustle();
	}

	private void AniEvent_SFXLaunchThrow()
	{
		main.SFXLaunchThrow();
	}
}
