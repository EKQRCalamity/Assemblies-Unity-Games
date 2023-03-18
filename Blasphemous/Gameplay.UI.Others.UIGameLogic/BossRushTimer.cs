using Framework.Managers;
using Framework.Util;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace Gameplay.UI.Others.UIGameLogic;

public class BossRushTimer : SerializedMonoBehaviour
{
	public Text text;

	private void Update()
	{
		if (!(Singleton<Core>.Instance == null) && Core.ready)
		{
			if (!Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH))
			{
				Hide();
			}
			text.text = Core.BossRushManager.GetCurrentRunDurationString();
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
