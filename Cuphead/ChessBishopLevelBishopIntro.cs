public class ChessBishopLevelBishopIntro : AbstractCollidableObject
{
	private void AniEvent_BishopIntroSFX()
	{
	}

	private void AnimationEvent_SFX_KOG_Bishop_Intro_Vocal()
	{
		AudioManager.Play("sfx_dlc_kog_bishop_intro_vocal");
		emitAudioFromObject.Add("sfx_dlc_kog_bishop_intro_vocal");
	}

	private void AnimationEvent_SFX_KOG_Bishop_Intro_Sfx()
	{
		AudioManager.Play("sfx_dlc_kog_bishop_intro_sfx");
		emitAudioFromObject.Add("sfx_dlc_kog_bishop_intro_sfx");
	}
}
