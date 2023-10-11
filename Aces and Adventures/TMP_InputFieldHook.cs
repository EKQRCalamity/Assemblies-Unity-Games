using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class TMP_InputFieldHook : MonoBehaviour
{
	[SerializeField]
	protected StringEvent _onSubmit;

	public StringEvent onSubmit => _onSubmit ?? (_onSubmit = new StringEvent());

	private void Awake()
	{
		GetComponent<TMP_InputField>().onSubmit.AddListener(onSubmit.Invoke);
	}
}
