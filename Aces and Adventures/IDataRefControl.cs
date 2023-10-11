using UnityEngine;

public interface IDataRefControl
{
	ContentRef dataRef { get; }

	object data { get; }

	Transform transform { get; }

	bool isValid { get; }

	bool showData { get; }

	void SetDataRefIfValid(ContentRef dataRef);

	bool HasUnsavedChanges();

	void OnSaveChanges();

	void OnSaveChanges(Transform parent);

	void Refresh();
}
