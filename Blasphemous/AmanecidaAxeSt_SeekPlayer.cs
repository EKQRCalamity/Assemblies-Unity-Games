using Maikel.StatelessFSM;

public class AmanecidaAxeSt_SeekPlayer : State<AmanecidaAxeBehaviour>
{
	public override void Enter(AmanecidaAxeBehaviour owner)
	{
		owner.ActivateAgent(active: true);
	}

	public override void Execute(AmanecidaAxeBehaviour owner)
	{
		owner.SeekTarget();
	}

	public override void Exit(AmanecidaAxeBehaviour owner)
	{
		owner.ActivateAgent(active: false);
	}
}
