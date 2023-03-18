using Framework.Managers;

namespace Tools.Gameplay;

public class BasicPersistence : PersistentManager.PersistentData
{
	public bool triggered;

	public BasicPersistence(string id)
		: base(id)
	{
	}
}
