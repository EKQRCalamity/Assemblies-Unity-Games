using UnityEngine;
using UnityEngine.Events;

public class LegalAgreementView : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/LegalAgreementView";

	public StringEvent onTextChange;

	public UnityEvent onTermsAccepted;

	public static LegalAgreementView Create(string text, Transform parent)
	{
		return Pools.Unpool(_Blueprint, parent).GetComponent<LegalAgreementView>()._SetData(text);
	}

	private LegalAgreementView _SetData(string text)
	{
		onTextChange.Invoke(text);
		return this;
	}

	public void AcceptTerms()
	{
		onTermsAccepted.Invoke();
	}
}
