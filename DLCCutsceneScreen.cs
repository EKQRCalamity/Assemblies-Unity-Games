using UnityEngine;

public class DLCCutsceneScreen : AbstractMonoBehaviour
{
	[SerializeField]
	protected DLCGenericCutscene cutscene;

	private void AniEvent_ShowText()
	{
		cutscene.ShowText();
	}

	private void AniEvent_ShowArrow()
	{
		cutscene.ShowArrow();
	}

	private void AniEvent_IrisIn()
	{
		cutscene.IrisIn();
	}

	private void AniEvent_IrisOut()
	{
		cutscene.IrisOut();
	}

	private void AnimEvent_SFX_IntroStart_SeagullCall_1()
	{
		AudioManager.Play("sfx_dlc_intro_seagullcall_1");
	}

	private void AnimEvent_SFX_IntroStart_SeagullCall_2()
	{
		AudioManager.Play("sfx_dlc_intro_seagullcall_2");
	}

	private void AnimEvent_SFX_IntroStart_SeagullCall_3()
	{
		AudioManager.Play("sfx_dlc_intro_seagullcall_3");
	}

	private void SFX_IntroStart_OceanAmbLoopStart()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_intro_oceanamb_loop", 0.5f, 1f);
	}

	private void SFX_IntroStart_OceanAmbLoopStop()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_intro_oceanamb_loop", 0f, 0.1f);
	}

	private void AnimEvent_SFX_Intro_ChalliceAppear()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_challiceappear");
	}

	private void AnimEvent_SFX_Intro_Challiceglows()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_challiceglows");
	}

	private void AnimEvent_SFX_Intro_EatCookie()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_eatcookie");
	}

	private void AnimEvent_SFX_Intro_EnterBakery()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_enterbakery");
	}

	private void AnimEvent_SFX_Intro_FollowChallice()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_followchallice");
	}

	private void AnimEvent_SFX_Intro_Recipeaccept()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_recipeaccept");
	}

	private void AnimEvent_SFX_Intro_SaltBakerRecipe()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_saltbakerrecipe");
	}

	private void AnimEvent_SFX_Intro_CookieMagic()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_cookiemagic");
	}

	private void AnimEvent_SFX_Intro_FirstSwapGhost()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_firstswapghost");
	}

	private void AnimEvent_SFX_Intro_SecondSwapGhost()
	{
		AudioManager.Play("sfx_dlc_cutscene_intro_secondswapghost");
	}

	private void AnimEvent_SFX_SaltBakerPre_ChaliceReveal()
	{
		AudioManager.Play("sfx_dlc_cutscene_saltbakerpre_chalicereveal");
	}

	private void AnimEvent_SFX_SaltBakerPre_EndLeanIn()
	{
		AudioManager.Play("sfx_dlc_cutscene_saltbakerpre_endleanin");
	}

	private void AnimEvent_SFX_SaltBakerPre_HelpClose()
	{
		AudioManager.Play("sfx_dlc_cutscene_saltbakerpre_helpclose");
	}

	private void AnimEvent_SFX_SaltBakerPre_KnifeOakTableLol()
	{
		AudioManager.Play("sfx_dlc_cutscene_saltbakerpre_knifedefinitelyoaktable");
	}

	private void AnimEvent_SFX_SaltBakerPre_KnifeSwipe()
	{
		AudioManager.Play("sfx_dlc_cutscene_saltbakerpre_knifeswipe");
	}
}
