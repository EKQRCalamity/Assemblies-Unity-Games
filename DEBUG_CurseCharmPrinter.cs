using UnityEngine;

public class DEBUG_CurseCharmPrinter : MonoBehaviour
{
	private GUIStyle style;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		if (Time.frameCount >= 120)
		{
			if (style == null)
			{
				style = new GUIStyle(GUI.skin.GetStyle("Box"));
				style.alignment = TextAnchor.UpperLeft;
			}
			if (PlayerData.Data != null)
			{
				string text = $"Curse: {CharmCurse.CalculateLevel(PlayerId.PlayerOne)} / {PlayerData.Data.CalculateCurseCharmAccumulatedValue(PlayerId.PlayerOne, CharmCurse.CountableLevels)} / {CharmCurse.IsMaxLevel(PlayerId.PlayerOne)}";
				GUI.Box(new Rect(0f, 0f, 200f, 100f), text);
			}
		}
	}
}
