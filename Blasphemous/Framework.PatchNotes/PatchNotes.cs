using System;

namespace Framework.PatchNotes;

[Serializable]
public class PatchNotes
{
	public string version;

	public PatchNotes(string version)
	{
		this.version = version;
	}
}
