using UnityEngine;

namespace Tools.Level.Layout;

[ExecuteInEditMode]
public class LevelBuilder : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Switches between level building modes. Shortcut: Ctrl + E")]
	private Category mode;

	public Category Mode => mode;
}
