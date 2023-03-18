namespace Tools.Level;

public interface IActionable
{
	bool Locked { get; set; }

	void Use();
}
