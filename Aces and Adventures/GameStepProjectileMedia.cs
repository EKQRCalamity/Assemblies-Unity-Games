using System.Collections;

public class GameStepProjectileMedia : GameStep
{
	public ProjectileMediaPack media;

	public ATarget actor;

	public ATarget target;

	public bool createImpactMedia;

	private ProjectileMediaView _projectileMediaView;

	protected override bool shouldBeCanceled => !media;

	public GameStepProjectileMedia(ProjectileMediaPack media, ATarget actor, ATarget target, bool createImpactMedia = true)
	{
		this.media = media;
		this.actor = actor;
		this.target = target;
		this.createImpactMedia = createImpactMedia;
	}

	public GameStepProjectileMedia(ProjectileMediaPack media, ActionContext context, bool createImpactMedia = true)
		: this(media, context.actor, context.target, createImpactMedia)
	{
	}

	protected override IEnumerator Update()
	{
		IEnumerator wait = (_projectileMediaView = ProjectileMediaView.Create(base.state.cosmeticRandom, media.data, actor.view, target.view, media.startDataOverride, media.endDataOverride, media.finishedAtOverride, createImpactMedia)).WaitTillFinished();
		while (wait.MoveNext())
		{
			yield return null;
		}
	}

	protected override void OnCanceled()
	{
		Stop();
	}

	public void Stop()
	{
		_projectileMediaView?.Stop();
	}
}
