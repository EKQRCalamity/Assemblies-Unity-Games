using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameStepSaveGameState : GameStep
{
	private IEnumerable<AdventureCard.SelectInstruction> _instructions;

	public GameStepSaveGameState(IEnumerable<AdventureCard.SelectInstruction> instructions = null)
	{
		_instructions = instructions;
	}

	private void _OnGameStateSerializedIntoMemoryStream(PoolHandle<MemoryStream> memoryStreamHandle)
	{
		base.finished = true;
		base.state.saving = false;
		base.view.CheckForExiledCardsAtRest();
		AsyncTask<bool>.Do(delegate
		{
			lock (FileLock.Lock(ProfileManager.progress.runPath))
			{
				return IOUtil.WriteBytesBackup(ProfileManager.progress.runPath, memoryStreamHandle.value);
			}
		}).GetResult(delegate
		{
			memoryStreamHandle.Dispose();
		});
	}

	protected override void OnFirstEnabled()
	{
		if (_instructions != null)
		{
			base.state.loadInstructions.AddRange(_instructions);
		}
	}

	public override void Start()
	{
		base.state.saving = true;
		AsyncTask<PoolHandle<MemoryStream>>.Do(() => ProtoUtil.Serialize(GameState.Instance)).GetResult(_OnGameStateSerializedIntoMemoryStream);
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void OnFinish()
	{
		if (_instructions != null)
		{
			base.state.loadInstructions.Clear();
		}
	}
}
