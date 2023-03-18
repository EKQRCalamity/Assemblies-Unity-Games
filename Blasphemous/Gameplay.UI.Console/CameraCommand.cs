using System.Collections.Generic;
using System.Text;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.UI.Console;

public class CameraCommand : ConsoleCommand
{
	private readonly string SEPARATOR = "-------------------";

	public override string GetName()
	{
		return "camera";
	}

	public override bool HasLowerParameters()
	{
		return false;
	}

	public override void Execute(string command, string[] parameters)
	{
		List<string> listParameters;
		string subcommand = GetSubcommand(parameters, out listParameters);
		if (command != null && command == "camera")
		{
			ShowCamera(subcommand);
		}
	}

	private void ShowAllCameras()
	{
		ShowCamera("game");
		ShowCamera("virtual");
		ShowCamera("ui");
	}

	private void ShowCamera(string camera)
	{
		switch (camera.ToUpper())
		{
		case "SCENE":
			ShowSceneCameras();
			return;
		case "ALL":
			ShowAllCameras();
			return;
		case "UI":
			ShowCamera(Core.Screen.UICamera);
			return;
		case "GAME":
			ShowCamera(Core.Screen.GameCamera);
			return;
		case "VIRTUAL":
			ShowCamera(Core.Screen.VirtualCamera);
			return;
		}
		base.Console.Write("Shows camera information.");
		base.Console.Write("USAGE: camera ARGUMENT");
		base.Console.Write("Valid values for ARGUMENT are:");
		base.Console.Write("ui: Shows info for the UI camera");
		base.Console.Write("game: Shows info for the game camera");
		base.Console.Write("virtual: Shows info for the virtual (final composition) camera");
		base.Console.Write("all: Shows info for the ui, game and virtual cameras");
		base.Console.Write("scene: Shows info every loaded camera (dynamic)");
	}

	private void ShowSceneCameras()
	{
		Camera[] array = Object.FindObjectsOfType<Camera>();
		foreach (Camera cam in array)
		{
			ShowCamera(cam);
		}
	}

	private void ShowCamera(Camera cam)
	{
		StringBuilder stringBuilder = new StringBuilder();
		base.Console.Write(SEPARATOR);
		base.Console.WriteFormat("Camera name: {0}", cam.name);
		base.Console.WriteFormat("Camera position: {0}", cam.transform.position);
		base.Console.WriteFormat("Camera aspect: {0}", cam.aspect);
		base.Console.WriteFormat("Camera rect: {0}", cam.rect);
		base.Console.WriteFormat("Camera isOrtho: {0}", cam.orthographic);
		base.Console.WriteFormat("Camera orthoSize: {0}", cam.orthographicSize);
		base.Console.WriteFormat("Camera pixelSize: ({0}, {1})", cam.pixelWidth, cam.pixelHeight);
		base.Console.WriteFormat("Camera far plane: {0}", cam.farClipPlane);
		base.Console.WriteFormat("Camera near plane: {0}", cam.nearClipPlane);
		base.Console.WriteFormat("Camera FOV: {0}", cam.fieldOfView);
		base.Console.Write(SEPARATOR);
		base.Console.Write(stringBuilder.ToString());
	}
}
