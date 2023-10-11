using UnityEngine;

public class PhysicsFoleyGroup : MonoBehaviour
{
	[SerializeField]
	protected bool _includeChildrenFoley;

	[SerializeField]
	[Range(0f, 2f)]
	[Space]
	protected float _volumeMultiplier = 1f;

	[SerializeField]
	[Range(1f, 100f)]
	protected float _soundDistance = 10f;

	[SerializeField]
	[Range(0.1f, 10f)]
	[Space]
	protected float _minMaxThresholdScale = 1f;

	[SerializeField]
	[Range(0f, 100f)]
	protected float _impactThresholdMin = 1f;

	[SerializeField]
	[Range(0f, 100f)]
	protected float _impactThresholdMax = 10f;

	private PhysicsFoley[] _foleys;

	public float volumeMultiplier
	{
		get
		{
			return _volumeMultiplier;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _volumeMultiplier, value))
			{
				PhysicsFoley[] array = foleys;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].volumeMultiplier = value;
				}
			}
		}
	}

	public float soundDistance
	{
		get
		{
			return _soundDistance;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _soundDistance, value))
			{
				PhysicsFoley[] array = foleys;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].soundDistance = value;
				}
			}
		}
	}

	public float minMaxThresholdScale
	{
		get
		{
			return _minMaxThresholdScale;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _minMaxThresholdScale, value))
			{
				PhysicsFoley[] array = foleys;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].minMaxThresholdScale = value;
				}
			}
		}
	}

	public float impactThresholdMin
	{
		get
		{
			return _impactThresholdMin;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _impactThresholdMin, value))
			{
				PhysicsFoley[] array = foleys;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].impactThresholdMin = value;
				}
			}
		}
	}

	public float impactThresholdMax
	{
		get
		{
			return _impactThresholdMax;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _impactThresholdMax, value))
			{
				PhysicsFoley[] array = foleys;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].impactThresholdMax = value;
				}
			}
		}
	}

	public PhysicsFoley[] foleys
	{
		get
		{
			return _foleys ?? (_foleys = (_includeChildrenFoley ? GetComponentsInChildren<PhysicsFoley>() : GetComponents<PhysicsFoley>()));
		}
		set
		{
			_foleys = value;
		}
	}

	private void _UpdateFoleyValues()
	{
		PhysicsFoley[] array = foleys;
		foreach (PhysicsFoley obj in array)
		{
			obj.volumeMultiplier = _volumeMultiplier;
			obj.soundDistance = _soundDistance;
			obj.minMaxThresholdScale = _minMaxThresholdScale;
			obj.impactThresholdMin = _impactThresholdMin;
			obj.impactThresholdMax = _impactThresholdMax;
		}
	}
}
