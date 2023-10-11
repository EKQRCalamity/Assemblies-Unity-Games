public class GameStepRest : GameStep
{
	private bool _isChosen;

	private GameStep _voiceStep;

	public GameStepRest(bool isChosen)
	{
		_isChosen = isChosen;
	}

	protected override void OnFirstEnabled()
	{
		_voiceStep = ParallelStep(new GameStepVoice(base.state.player.audio.character.restore));
		AppendStep(new GameStepProjectileMedia(ContentRef.Defaults.media.restore[_isChosen.ToInt(base.state.parameters.numberOfRestUpgrades)], base.state.player, base.state.player));
	}

	public override void Start()
	{
		if (_isChosen && base.state.parameters.restHealthGain > 0)
		{
			AppliedAction.Apply(new ActionContext(base.state.player, null, base.state.player), new StatAction(StatType.Health, new AAction.DynamicNumber.Constant(base.state.parameters.restHealthGain), StatAction.Function.Add));
		}
		base.state.Heal(new ActionContext(base.state.player, null, base.state.player), base.state.player.HPMissing);
		AppendStep(new GameStepWaitForParallelStep(_voiceStep));
	}
}
