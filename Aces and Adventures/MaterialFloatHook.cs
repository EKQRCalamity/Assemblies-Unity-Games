using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialFloatHook : MonoBehaviour
{
	[Serializable]
	public class PropertyData
	{
		[SerializeField]
		protected string _propertyName;

		public bool asMultiplier = true;

		private int? _id;

		private float? _value;

		private float _initialValue;

		private Renderer _renderer;

		public Material sharedMaterial
		{
			get
			{
				return _renderer.sharedMaterial;
			}
			set
			{
				_OnPropertyNameChange();
			}
		}

		public Renderer renderer
		{
			get
			{
				return _renderer;
			}
			set
			{
				if (SetPropertyUtility.SetObject(ref _renderer, value))
				{
					_OnPropertyNameChange();
				}
			}
		}

		public string propertyName
		{
			get
			{
				return _propertyName ?? (_propertyName = "");
			}
			set
			{
				if (SetPropertyUtility.SetObject(ref _propertyName, value))
				{
					_OnPropertyNameChange();
				}
			}
		}

		public float value
		{
			get
			{
				return _value ?? float.MinValue;
			}
			set
			{
				if (SetPropertyUtility.SetStruct(ref _value, value))
				{
					_OnValueChanged();
				}
			}
		}

		private void _OnPropertyNameChange()
		{
			_id = (renderer.sharedMaterial.HasProperty(_propertyName) ? new int?(Shader.PropertyToID(_propertyName)) : null);
			if (_id.HasValue)
			{
				_value = renderer.material.GetFloat(_id.Value);
				_initialValue = renderer.sharedMaterial.GetFloat(_id.Value);
			}
		}

		private void _OnValueChanged()
		{
			if (_value.HasValue && _id.HasValue)
			{
				renderer.material.SetFloat(_id.Value, asMultiplier ? (_initialValue * _value.Value) : _value.Value);
			}
		}
	}

	public PropertyData[] properties;

	public float value0
	{
		get
		{
			return properties[0].value;
		}
		set
		{
			properties[0].value = value;
		}
	}

	public float value1
	{
		get
		{
			return properties[1].value;
		}
		set
		{
			properties[1].value = value;
		}
	}

	public float value2
	{
		get
		{
			return properties[2].value;
		}
		set
		{
			properties[2].value = value;
		}
	}

	public float value3
	{
		get
		{
			return properties[3].value;
		}
		set
		{
			properties[3].value = value;
		}
	}

	public float value4
	{
		get
		{
			return properties[4].value;
		}
		set
		{
			properties[4].value = value;
		}
	}

	public Material sharedMaterial
	{
		get
		{
			return properties[0].sharedMaterial;
		}
		set
		{
			if (!(sharedMaterial == value))
			{
				properties[0].renderer.sharedMaterial = value;
				for (int i = 0; i < properties.Length; i++)
				{
					properties[i].sharedMaterial = value;
				}
			}
		}
	}

	private void Awake()
	{
		PropertyData[] array = properties;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].renderer = GetComponent<Renderer>();
		}
	}
}
