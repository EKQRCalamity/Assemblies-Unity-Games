using System.Collections.Generic;

namespace Framework.Dialog;

public class MsgObject : BaseObject
{
	public List<string> msgLines = new List<string>();

	public override string GetPrefix()
	{
		return "MSG_";
	}
}
