using System;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Audio;

[RequireComponent(typeof(CircleCollider2D))]
public class AreaModifier : AudioTool
{
	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[Range(0f, 1f)]
	private float maxParamValue = 1f;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[Range(0f, 1f)]
	private float originSize = 0.5f;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[Range(0f, 2f)]
	private float transitionSpeed = 1f;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private CollisionSensor sensor;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private AudioParamName[] parameters = new AudioParamName[0];

	private CircleCollider2D col;

	private bool allParamsAtZero;

	private bool insideActivationRange;

	private bool InsideInfluenceArea => TargetDistance <= InfluenceDistance;

	private float TargetDistance
	{
		get
		{
			if (!Core.ready || Core.Logic.Penitent == null)
			{
				return -1f;
			}
			Vector2 a = Core.Logic.Penitent.transform.position;
			return Vector2.Distance(a, OriginPosition);
		}
	}

	private Vector2 OriginPosition => base.transform.position;

	private float OriginSize
	{
		get
		{
			if (col == null)
			{
				return 0f;
			}
			return col.radius * originSize;
		}
	}

	private float InfluenceDistance
	{
		get
		{
			if (col == null)
			{
				return 0f;
			}
			return col.radius;
		}
	}

	protected override void BaseStart()
	{
		base.IsEmitter = false;
		col = GetComponent<CircleCollider2D>();
		if (sensor != null)
		{
			sensor.OnPenitentEnter += OnPenitentEnter;
			sensor.OnPenitentExit += OnPenitentExit;
		}
	}

	protected override void BaseDestroy()
	{
		if (sensor != null)
		{
			sensor.OnPenitentEnter -= OnPenitentEnter;
			sensor.OnPenitentExit -= OnPenitentExit;
		}
	}

	private void OnPenitentEnter()
	{
		insideActivationRange = true;
	}

	private void OnPenitentExit()
	{
		insideActivationRange = false;
	}

	protected override void BaseUpdate()
	{
		if (InsideInfluenceArea || !allParamsAtZero)
		{
			UpdateParameters();
		}
	}

	protected override void BaseDrawGizmos()
	{
		Gizmos.color = Color.gray;
		Gizmos.DrawWireSphere(base.transform.position, OriginSize);
	}

	private void UpdateParameters()
	{
		bool flag = true;
		float value = ((!insideActivationRange) ? 0f : CalculateParameterValue(TargetDistance, InfluenceDistance));
		Core.Audio.Ambient.AddAreaModifier(base.name, value);
		for (int i = 0; i < parameters.Length; i++)
		{
			string param = parameters[i].name;
			StepParamValue(param, value);
			parameters[i].currentValue = Core.Audio.Ambient.GetParameterValue(param);
			if (parameters[i].currentValue > 0f)
			{
				flag = false;
			}
		}
		allParamsAtZero = flag;
	}

	private void StepParamValue(string param, float value)
	{
		float parameterValue = Core.Audio.Ambient.GetParameterValue(param);
		if (Mathf.Approximately(parameterValue, value))
		{
			Core.Audio.Ambient.SetParameter(param, value);
		}
		else if (parameterValue > value)
		{
			Core.Audio.Ambient.SetParameter(param, parameterValue - Time.deltaTime * transitionSpeed);
		}
		else if (parameterValue < value)
		{
			Core.Audio.Ambient.SetParameter(param, parameterValue + Time.deltaTime * transitionSpeed);
		}
	}

	private float CalculateParameterValue(float originDistance, float influenceArea)
	{
		float num = Math.Max(0f, originDistance - OriginSize);
		float num2 = influenceArea - OriginSize;
		return maxParamValue * (1f - num / num2);
	}
}
