public class OldMan_FX_DeathPuff : AbstractPausableComponent
{
	private void AnimationEvent_SFX_OMM_GnomeDeathPuff()
	{
		AudioManager.Play("sfx_dlc_omm_gnome_popper_deathpoof");
		emitAudioFromObject.Add("sfx_dlc_omm_gnome_popper_deathpoof");
	}
}
