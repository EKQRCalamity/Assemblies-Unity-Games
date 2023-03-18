using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.UI.Widgets;

public class HideUIOnCommand : SerializedMonoBehaviour
{
	public bool debugUI;

	[SerializeField]
	private List<GameObject> controls = new List<GameObject>();

	private bool lastValue = true;

	private void Start()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
	}

	private void OnLevelLoaded(Level oldlevel, Level newlevel)
	{
		EnableUiControls(lastValue);
	}

	private void Update()
	{
		bool flag = ((!debugUI) ? Core.UI.MustShowGamePlayUI() : Core.UI.ConsoleShowDebugUI);
		if (flag != lastValue)
		{
			lastValue = flag;
			EnableUiControls(lastValue);
		}
	}

	private void EnableUiControls(bool enable)
	{
		controls.ForEach(delegate(GameObject p)
		{
			p.SetActive(enable);
		});
	}
}
