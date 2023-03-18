using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;

namespace Framework.Inventory;

public class IncreaseSpeedBeadEffect : ObjectEffect
{
	public Dash.MoveSetting BeadMoveSettings;

	private Dash.MoveSetting DefaultMoveSettings;

	protected override void OnAwake()
	{
		base.OnAwake();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		SaveDefaultMoveSettings(penitent.Dash.DefaultMoveSetting);
	}

	protected override bool OnApplyEffect()
	{
		LoadBeadMoveSettings();
		return true;
	}

	protected override void OnRemoveEffect()
	{
		base.OnRemoveEffect();
		LoadDefaultMoveSettings();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	private void SaveDefaultMoveSettings(Dash.MoveSetting moveSetting)
	{
		DefaultMoveSettings = new Dash.MoveSetting(moveSetting.Drag, moveSetting.Speed);
	}

	private void LoadBeadMoveSettings()
	{
		Dash.MoveSetting moveSetting = new Dash.MoveSetting(BeadMoveSettings.Drag, BeadMoveSettings.Speed);
		Core.Logic.Penitent.Dash.DefaultMoveSetting = moveSetting;
		SetPlayerSpeed(moveSetting);
	}

	private void LoadDefaultMoveSettings()
	{
		Core.Logic.Penitent.Dash.DefaultMoveSetting = DefaultMoveSettings;
		SetPlayerSpeed(DefaultMoveSettings);
	}

	private void SetPlayerSpeed(Dash.MoveSetting moveSetting)
	{
		PlatformCharacterController platformCharacterController = Core.Logic.Penitent.PlatformCharacterController;
		if ((bool)platformCharacterController)
		{
			platformCharacterController.MaxWalkingSpeed = moveSetting.Speed;
			platformCharacterController.WalkingDrag = moveSetting.Drag;
		}
	}
}
