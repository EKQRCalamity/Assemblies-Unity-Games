using Framework.Managers;
using HutongGames.PlayMaker;
using Tools.DataContainer;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Pauses the game state and starts a video reproduction.")]
public class CutscenePlay : FsmStateAction
{
	public CutsceneData cutscene;

	public FsmBool muteAudio;

	public FsmBool useStartFade;

	public FsmFloat timeStartFade;

	public FsmBool useEndFade;

	public FsmFloat timeEndFade;

	public FsmBool useSubtitleBackground;

	public FsmEvent onSuccess;

	public FsmEvent onFailure;

	private const float DEFAULT_FADE = 1.5f;

	public override void Reset()
	{
		muteAudio = new FsmBool();
		muteAudio.UseVariable = false;
		muteAudio.Value = true;
		useStartFade = new FsmBool();
		useStartFade.UseVariable = false;
		useStartFade.Value = true;
		useEndFade = new FsmBool();
		useEndFade.UseVariable = false;
		useEndFade.Value = true;
		timeStartFade = new FsmFloat();
		timeStartFade.Value = 1.5f;
		timeStartFade.UseVariable = false;
		timeEndFade = new FsmFloat();
		timeEndFade.Value = 1.5f;
		timeEndFade.UseVariable = false;
		useSubtitleBackground = new FsmBool();
		useSubtitleBackground.UseVariable = false;
		useSubtitleBackground.Value = true;
	}

	public override void OnEnter()
	{
		bool flag = muteAudio == null || muteAudio.Value;
		bool flag2 = useStartFade == null || useStartFade.Value;
		bool flag3 = useEndFade == null || useEndFade.Value;
		float fadeStart = ((timeStartFade == null) ? 1.5f : timeStartFade.Value);
		float fadeEnd = ((timeEndFade == null) ? 1.5f : timeEndFade.Value);
		if (!flag2)
		{
			fadeStart = 0f;
		}
		if (!flag3)
		{
			fadeEnd = 0f;
		}
		bool useBackground = useSubtitleBackground == null || useSubtitleBackground.Value;
		if (cutscene != null)
		{
			Core.Cinematics.CinematicEnded += OnCinematicEnded;
			Core.Cinematics.StartCutscene(cutscene, flag, fadeStart, fadeEnd, useBackground);
		}
		Finish();
	}

	public override void OnUpdate()
	{
	}

	public override void OnExit()
	{
		Core.Cinematics.CinematicEnded -= OnCinematicEnded;
	}

	private void OnCinematicEnded(bool cancelled)
	{
		base.Fsm.Event((!cancelled) ? onSuccess : onFailure);
	}
}
