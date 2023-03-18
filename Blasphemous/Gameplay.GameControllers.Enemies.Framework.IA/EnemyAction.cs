using System.Collections;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class EnemyAction
{
	public delegate void EnemyActionCallback(EnemyAction e);

	protected EnemyBehaviour owner;

	protected Coroutine currentCoroutine;

	public CustomYieldInstruction waitForCompletion;

	public CustomYieldInstruction waitForCallback;

	private int id;

	public static int actionsId;

	public bool Finished { get; set; }

	public bool CallbackCalled { get; set; }

	public event EnemyActionCallback OnActionStarts;

	public event EnemyActionCallback OnActionIsStopped;

	public event EnemyActionCallback OnActionEnds;

	public EnemyAction()
	{
		Finished = true;
		waitForCompletion = new WaitUntilActionFinishes(this);
		waitForCallback = new WaitUntilActionCustomCallback(this);
	}

	public EnemyAction StartAction(EnemyBehaviour e)
	{
		id = actionsId;
		actionsId++;
		Debug.Log(string.Format("<color=blue>[{2:D6}]/---------- EA START: ID{0}_{1}</color>", id, ToString(), Time.frameCount));
		Finished = false;
		CallbackCalled = false;
		if (this.OnActionStarts != null)
		{
			this.OnActionStarts(this);
		}
		owner = e;
		DoOnStart();
		currentCoroutine = e.StartCoroutine(BaseCoroutine());
		return this;
	}

	protected void Callback()
	{
		CallbackCalled = true;
		DoOnCallback();
	}

	protected void FinishAction()
	{
		Debug.Log(string.Format("<color=green>[{2:D6}]\\---------- EA FINISH: ID{0}_{1}</color>", id, ToString(), Time.frameCount));
		Finished = true;
		DoOnEnd();
		if (this.OnActionEnds != null)
		{
			this.OnActionEnds(this);
		}
	}

	public void StopAction()
	{
		if (!Finished && currentCoroutine != null)
		{
			Finished = true;
			Debug.Log(string.Format("<color=red>[{2:D6}]/----------!EA STOPPED: ID{0}_{1}</color>", id, ToString(), Time.frameCount));
			DoOnStop();
			if (this.OnActionIsStopped != null)
			{
				this.OnActionIsStopped(this);
			}
			owner.StopCoroutine(currentCoroutine);
		}
	}

	protected virtual void DoOnStart()
	{
	}

	protected virtual void DoOnEnd()
	{
	}

	protected virtual void DoOnCallback()
	{
	}

	protected virtual void DoOnStop()
	{
	}

	protected virtual IEnumerator BaseCoroutine()
	{
		yield return null;
	}

	public override string ToString()
	{
		return GetType().Name;
	}
}
