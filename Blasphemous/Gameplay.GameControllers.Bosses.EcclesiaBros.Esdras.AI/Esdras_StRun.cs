using Maikel.StatelessFSM;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Esdras.AI;

public class Esdras_StRun : State<EsdrasBehaviour>
{
	public override void Enter(EsdrasBehaviour owner)
	{
		owner.SetRunAnimation(run: true);
	}

	public override void Execute(EsdrasBehaviour owner)
	{
		owner.RunToPoint(owner.GetRunPoint());
		if (owner.CloseToPointX(owner.GetRunPoint()) || (owner.CloseToTarget() && owner.AttackIfEnemyClose()) || owner.Esdras.MotionChecker.HitsBlock)
		{
			owner.ChangeToAction();
		}
	}

	public override void Exit(EsdrasBehaviour owner)
	{
		owner.StopRunning();
		if (!owner.KeepRunningAnimation())
		{
			owner.SetRunAnimation(run: false);
		}
	}
}
