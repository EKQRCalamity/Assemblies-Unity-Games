using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DEBUG_LocalAchievementsPrinter : MonoBehaviour
{
	private static readonly string[] AllAchievements = Enum.GetNames(typeof(LocalAchievementsManager.Achievement));

	private StringBuilder builder = new StringBuilder();

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		if (Time.frameCount >= 120)
		{
			IList<LocalAchievementsManager.Achievement> unlockedAchievements = LocalAchievementsManager.GetUnlockedAchievements();
			string[] allAchievements = AllAchievements;
			foreach (string text in allAchievements)
			{
				bool flag = unlockedAchievements.Contains((LocalAchievementsManager.Achievement)Enum.Parse(typeof(LocalAchievementsManager.Achievement), text));
				builder.AppendFormat("{0}....{1}\n", (!flag) ? "L" : "U", text);
			}
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
			gUIStyle.alignment = TextAnchor.UpperLeft;
			GUI.Box(new Rect(0f, 0f, 200f, 500f), builder.ToString(), gUIStyle);
			builder.Length = 0;
		}
	}
}
