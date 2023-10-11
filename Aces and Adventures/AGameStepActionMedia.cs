using System;
using System.Collections;
using System.Collections.Generic;

public abstract class AGameStepActionMedia : GameStepAAction
{
	protected int _count;

	protected abstract ActionMedia media { get; }

	protected AGameStepActionMedia(AAction action, ActionContext context)
		: base(action, context)
	{
	}

	protected override IEnumerator Update()
	{
		ActionContext tContext = base.context;
		switch (media.sequencing)
		{
		case ActionMedia.Sequencing.AllAtOnce:
		{
			PoolKeepItemListHandle<IEnumerator> poolKeepItemListHandle = Pools.UseKeepItemList<IEnumerator>();
			foreach (ATarget target in base.context.targets)
			{
				_count++;
				tContext = tContext.SetTarget(target);
				poolKeepItemListHandle.Add(ProjectileMediaView.Create(base.state.cosmeticRandom, media, media.activator.GetProjectileExtrema(tContext), media.target.GetProjectileExtrema(tContext), media.projectileMedia.startDataOverride, media.projectileMedia.endDataOverride, media.projectileMedia.finishedAtOverride).WaitTillFinished());
			}
			IEnumerator wait2 = Job.ParallelProcesses(poolKeepItemListHandle);
			while (wait2.MoveNext())
			{
				yield return null;
			}
			break;
		}
		case ActionMedia.Sequencing.OneAtATime:
			foreach (ATarget target2 in base.context.targets)
			{
				_count++;
				tContext = tContext.SetTarget(target2);
				IEnumerator waitForProjectile = ProjectileMediaView.Create(base.state.cosmeticRandom, media, media.activator.GetProjectileExtrema(tContext), media.target.GetProjectileExtrema(tContext), media.projectileMedia.startDataOverride, media.projectileMedia.endDataOverride, media.projectileMedia.finishedAtOverride).WaitTillFinished();
				while (waitForProjectile.MoveNext())
				{
					yield return null;
				}
			}
			break;
		case ActionMedia.Sequencing.PlayOnce:
		{
			using (IEnumerator<ATarget> enumerator2 = base.context.targets.GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					ATarget current4 = enumerator2.Current;
					_count++;
					tContext = tContext.SetTarget(current4);
					IEnumerator waitForProjectile = ProjectileMediaView.Create(base.state.cosmeticRandom, media, media.activator.GetProjectileExtrema(tContext), media.target.GetProjectileExtrema(tContext), media.projectileMedia.startDataOverride, media.projectileMedia.endDataOverride, media.projectileMedia.finishedAtOverride).WaitTillFinished();
					while (waitForProjectile.MoveNext())
					{
						yield return null;
					}
					break;
				}
			}
			break;
		}
		case ActionMedia.Sequencing.AllAtOnceNormalizeEmission:
		{
			PoolKeepItemListHandle<IEnumerator> poolKeepItemListHandle = Pools.UseKeepItemList<IEnumerator>();
			PoolKeepItemListHandle<ATarget> poolKeepItemListHandle2 = Pools.UseKeepItemList(base.context.targets);
			_count = poolKeepItemListHandle2.Count;
			float emissionMultiplierMod = 1f / (float)Math.Max(1, _count);
			foreach (ATarget item in poolKeepItemListHandle2)
			{
				tContext = tContext.SetTarget(item);
				poolKeepItemListHandle.Add(ProjectileMediaView.Create(base.state.cosmeticRandom, media, media.activator.GetProjectileExtrema(tContext), media.target.GetProjectileExtrema(tContext), media.projectileMedia.startDataOverride, media.projectileMedia.endDataOverride, media.projectileMedia.finishedAtOverride, createImpactMedia: true, emissionMultiplierMod).WaitTillFinished());
			}
			IEnumerator wait2 = Job.ParallelProcesses(poolKeepItemListHandle);
			while (wait2.MoveNext())
			{
				yield return null;
			}
			break;
		}
		}
	}
}
