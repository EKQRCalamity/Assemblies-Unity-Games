namespace DialoguerCore;

public class EmptyPhase : AbstractDialoguePhase
{
	public EmptyPhase()
		: base(null)
	{
	}

	public override string ToString()
	{
		return "Empty Phase\nEmpty Phases should not be generated, something went wrong.\n";
	}
}
