using System;
using System.Collections;
using UnityEngine;

public class GameStepWait : GameStep
{
	public float wait;

	public Action enableAction;

	public readonly bool canSkip;

	public GameStepWait(float wait, Action enableAction = null, bool canSkip = true)
	{
		this.wait = wait;
		this.enableAction = enableAction;
		this.canSkip = canSkip;
	}

	private void _OnConfirmPressed()
	{
		base.finished = true;
	}

	protected override void OnEnable()
	{
		enableAction?.Invoke();
		if (canSkip)
		{
			base.view.onClick += _OnConfirmPressed;
			base.view.onConfirmPressed += _OnConfirmPressed;
		}
	}

	public override void Start()
	{
		wait += Time.deltaTime;
	}

	protected override IEnumerator Update()
	{
		while ((wait -= Time.deltaTime) > 0f)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		if (canSkip)
		{
			base.view.onClick -= _OnConfirmPressed;
			base.view.onConfirmPressed -= _OnConfirmPressed;
		}
	}
}
