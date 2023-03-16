public class DLCIntroBoatman : AbstractPausableComponent
{
	private void AniEvent_Paddle_SFX()
	{
		AudioManager.Play("sfx_DLC_Intro_PaddleWater");
		emitAudioFromObject.Add("sfx_DLC_Intro_PaddleWater");
	}
}
