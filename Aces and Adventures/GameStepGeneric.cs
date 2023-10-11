using System;
using System.Collections;

public class GameStepGeneric : GameStep
{
	public static IEnumerator EmptyUpdate
	{
		get
		{
			yield break;
		}
	}

	public Action onAwake { get; set; }

	public Action onEnable { get; set; }

	public Action onStart { get; set; }

	public Func<IEnumerator> onUpdate { get; set; }

	public Action onDisable { get; set; }

	public Action onFinished { get; set; }

	public Action onEnd { get; set; }

	public Action onCompletedSuccessfully { get; set; }

	public Action onCanceled { get; set; }

	public Action onDestroy { get; set; }

	protected override void Awake()
	{
		onAwake?.Invoke();
	}

	protected override void OnEnable()
	{
		onEnable?.Invoke();
	}

	public override void Start()
	{
		onStart?.Invoke();
	}

	protected override IEnumerator Update()
	{
		return onUpdate?.Invoke() ?? EmptyUpdate;
	}

	protected override void OnDisable()
	{
		onDisable?.Invoke();
	}

	protected override void OnFinish()
	{
		onFinished?.Invoke();
	}

	protected override void End()
	{
		onEnd?.Invoke();
	}

	public override void OnCompletedSuccessfully()
	{
		onCompletedSuccessfully?.Invoke();
	}

	protected override void OnCanceled()
	{
		onCanceled?.Invoke();
	}

	protected override void OnDestroy()
	{
		onDestroy?.Invoke();
	}
}
