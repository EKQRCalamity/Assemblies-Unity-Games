using System.Collections;

public class GameStepCastMedia : GameStepAAction
{
	public GameStepCastMedia(AAction action, ActionContext context)
		: base(action, context)
	{
	}

	protected override IEnumerator Update()
	{
		AbilityData.Cosmetic.CastMedia castMedia = base.context.ability.data.cosmetic.castMedia;
		if (base.context.target == null && (castMedia.activator == CardTargetType.Target || castMedia.target == CardTargetType.Target))
		{
			yield break;
		}
		IEnumerator vocalWait = castMedia.vocal.Play(base.context.actor as ACombatant).Wait(castMedia.vocal.waitRatio);
		while (vocalWait.MoveNext())
		{
			yield return null;
		}
		if ((bool)castMedia.media)
		{
			IEnumerator wait = ProjectileMediaView.Create(base.state.cosmeticRandom, castMedia.media.data, castMedia.activator.GetProjectileExtrema(base.context), castMedia.target.GetProjectileExtrema(base.context), castMedia.media.startDataOverride, castMedia.media.endDataOverride, castMedia.media.finishedAtOverride).WaitTillFinished();
			while (wait.MoveNext())
			{
				yield return null;
			}
		}
	}
}
