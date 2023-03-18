using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.PlayMaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Saves the game in the selected level and priedieu persistent ID")]
public class SaveManually : FsmStateAction
{
	public FsmString levelID;

	public FsmString prieDieuPersistentID;

	public override void OnEnter()
	{
		Core.SpawnManager.SetActivePriedieuManually(levelID.ToString(), prieDieuPersistentID.ToString());
		Core.Persistence.SaveGame();
		Finish();
	}
}
