using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class VersionNumber : MonoBehaviour
{
	private void Start()
	{
		Text component = GetComponent<Text>();
		component.text = "v. " + Application.version;
	}
}
