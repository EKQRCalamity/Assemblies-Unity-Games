using System.Text;
using Rewired;
using UnityEngine;

public class DEBUG_RewiredPrinter : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		if (!ReInput.isReady)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("===PLAYERS===");
		foreach (Player allPlayer in ReInput.players.AllPlayers)
		{
			stringBuilder.AppendLine(allPlayer.name);
			foreach (Joystick joystick in allPlayer.controllers.Joysticks)
			{
				appendControllerInfo(joystick, stringBuilder);
			}
		}
		stringBuilder.AppendLine("===UNASSIGNED===");
		foreach (Joystick joystick2 in ReInput.controllers.Joysticks)
		{
			if (!ReInput.controllers.IsJoystickAssigned(joystick2))
			{
				appendControllerInfo(joystick2, stringBuilder);
			}
		}
		stringBuilder.AppendLine("===BUTTONS===");
		GUI.Box(new Rect(0f, 0f, 700f, 400f), stringBuilder.ToString());
	}

	private static void appendControllerInfo(Joystick j, StringBuilder builder)
	{
		string arg = j.name;
		int id = j.id;
		builder.AppendFormat("{0} :: {1}", arg, id.ToString());
		builder.Append("\n");
	}
}
