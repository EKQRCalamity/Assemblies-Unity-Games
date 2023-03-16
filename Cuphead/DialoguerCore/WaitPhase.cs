using System.Collections.Generic;
using DialoguerEditor;
using UnityEngine;

namespace DialoguerCore;

public class WaitPhase : AbstractDialoguePhase
{
	public readonly DialogueEditorWaitTypes type;

	public readonly float duration;

	public WaitPhase(DialogueEditorWaitTypes type, float duration, List<int> outs)
		: base(outs)
	{
		this.type = type;
		this.duration = duration;
	}

	protected override void onStart()
	{
		DialoguerEventManager.dispatchOnWaitStart();
		if (type != DialogueEditorWaitTypes.Continue)
		{
			GameObject gameObject = new GameObject("Dialoguer WaitPhaseTimer");
			WaitPhaseComponent waitPhaseComponent = gameObject.AddComponent<WaitPhaseComponent>();
			waitPhaseComponent.Init(this, type, duration);
		}
	}

	public void waitComplete()
	{
		DialoguerEventManager.dispatchOnWaitComplete();
		base.state = PhaseState.Complete;
	}

	public override void Continue(int outId)
	{
		if (type == DialogueEditorWaitTypes.Continue)
		{
			DialoguerEventManager.dispatchOnWaitComplete();
			base.Continue(outId);
		}
	}

	public override string ToString()
	{
		object[] obj = new object[5] { "Wait Phase\nType: ", null, null, null, null };
		DialogueEditorWaitTypes dialogueEditorWaitTypes = type;
		obj[1] = dialogueEditorWaitTypes.ToString();
		obj[2] = "\nDuration: ";
		obj[3] = duration;
		obj[4] = "\n";
		return string.Concat(obj);
	}
}
