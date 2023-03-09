using System;
using UnityEngine;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
public class ThemedElement : MonoBehaviour
{
	[Serializable]
	public class ElementInfo
	{
		[SerializeField]
		private string _themeClass;

		[SerializeField]
		private Component _component;

		public string themeClass => _themeClass;

		public Component component => _component;
	}

	[SerializeField]
	private ElementInfo[] _elements;

	private void Start()
	{
		ApplyTheme();
		ControlMapper.OnPlayerChange += ApplyTheme;
	}

	private void OnDestroy()
	{
		ControlMapper.OnPlayerChange -= ApplyTheme;
	}

	private void OnEnable()
	{
		ControlMapper.ApplyTheme(_elements);
	}

	private void ApplyTheme()
	{
		ControlMapper.ApplyTheme(_elements);
	}
}
