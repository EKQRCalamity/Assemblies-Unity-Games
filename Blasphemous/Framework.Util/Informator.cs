using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace Framework.Util;

[RequireComponent(typeof(BoxCollider2D))]
public class Informator : MonoBehaviour
{
	[SerializeField]
	private bool disabled;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Penitent") && !disabled)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("SCENE", SceneManager.GetActiveScene().name);
			dictionary.Add("SPAWN_POINT", base.gameObject.name);
			Analytics.CustomEvent("TRIGGER_ENTER", dictionary);
			disabled = true;
			Debug.Log("[Analytics] Trigger enter \"" + base.gameObject.name + "\" has been submited.");
		}
	}
}
