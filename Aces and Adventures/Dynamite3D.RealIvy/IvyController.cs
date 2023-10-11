using System;
using UnityEngine;

namespace Dynamite3D.RealIvy;

public class IvyController : MonoBehaviour
{
	public enum State
	{
		GROWTH_NOT_STARTED,
		WAITING_FOR_DELAY,
		PAUSED,
		GROWING,
		GROWTH_FINISHED
	}

	private float currentTimer;

	public RTIvy rtIvy;

	public IvyContainer ivyContainer;

	public IvyParameters ivyParameters;

	public RuntimeGrowthParameters growthParameters;

	private State state;

	public event Action OnGrowthStarted;

	public event Action OnGrowthPaused;

	public event Action OnGrowthFinished;

	private void Awake()
	{
		rtIvy.AwakeInit();
		state = State.GROWTH_NOT_STARTED;
		if (growthParameters.startGrowthOnAwake)
		{
			StartGrowth();
		}
	}

	[ContextMenu("Start Growth")]
	public void StartGrowth()
	{
		if (state == State.GROWTH_NOT_STARTED)
		{
			rtIvy.InitIvy(growthParameters, ivyContainer, ivyParameters);
			if (growthParameters.delay > 0f)
			{
				state = State.WAITING_FOR_DELAY;
			}
			else
			{
				state = State.GROWING;
			}
			if (this.OnGrowthStarted != null)
			{
				this.OnGrowthStarted();
			}
		}
	}

	[ContextMenu("Pause Growth")]
	public void PauseGrowth()
	{
		if (state == State.GROWING || state == State.PAUSED)
		{
			state = State.PAUSED;
		}
		if (this.OnGrowthPaused != null)
		{
			this.OnGrowthPaused();
		}
	}

	[ContextMenu("Resume Growth")]
	public void ResumeGrowth()
	{
		if (state == State.GROWING || state == State.PAUSED)
		{
			state = State.GROWING;
		}
	}

	public State GetState()
	{
		return state;
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		switch (state)
		{
		case State.WAITING_FOR_DELAY:
			UpdateWaitingForDelayState(deltaTime);
			break;
		case State.GROWING:
			UpdateGrowingState(deltaTime);
			break;
		}
	}

	private void UpdateWaitingForDelayState(float deltaTime)
	{
		currentTimer += deltaTime;
		if (currentTimer > growthParameters.delay)
		{
			state = State.GROWING;
			currentTimer = 0f;
		}
	}

	private void UpdateGrowingState(float deltaTime)
	{
		if (!rtIvy.IsGrowingFinished() && !rtIvy.IsVertexLimitReached())
		{
			rtIvy.UpdateIvy(deltaTime);
			return;
		}
		state = State.GROWTH_FINISHED;
		if (this.OnGrowthFinished != null)
		{
			this.OnGrowthFinished();
		}
	}
}
