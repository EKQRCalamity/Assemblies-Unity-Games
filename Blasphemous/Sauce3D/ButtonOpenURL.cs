using UnityEngine;

namespace Sauce3D;

public class ButtonOpenURL : MonoBehaviour
{
	public string url = "http://3dsauce.com/";

	private void OpenUrl()
	{
		Application.OpenURL(url);
	}
}
