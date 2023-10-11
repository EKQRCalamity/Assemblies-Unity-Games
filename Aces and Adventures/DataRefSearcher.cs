using UnityEngine;

public abstract class DataRefSearcher<C> : MonoBehaviour where C : IDataContent
{
	public Vector2 center = new Vector2(0.5f, 0.5f);

	public Vector2 pivot = new Vector2(0.5f, 0.5f);

	public Color rayCastBlockerColor = new Color(1f, 1f, 1f, 0.31f);

	public bool closeOnSelect;

	private DataRef<C> _dataRef;

	private UIPopupControl _popup;

	private DataRef<C> dataRef => _dataRef ?? (_dataRef = new DataRef<C>());

	protected abstract DataRefEvent<C> _onDataRefSelected { get; }

	protected abstract DataRefValueEvent<C> _onDataSelected { get; }

	protected virtual string popupTitleText => "Select " + typeof(C).GetUILabel();

	private void _OnDataRefSelected(DataRef<C> dataRef)
	{
		dataRef = ProtoUtil.Clone(dataRef);
		_onDataRefSelected.Invoke(dataRef);
		if (dataRef.data != null)
		{
			_onDataSelected.Invoke(dataRef.data);
			if (closeOnSelect && (bool)_popup)
			{
				_popup.Close();
			}
		}
	}

	public void OpenSearch()
	{
		_popup = UIUtil.CreateDataSearchPopup<C>(_OnDataRefSelected, base.transform).GetComponentInChildren<UIPopupControl>();
		_popup.onClose.AddListener(delegate
		{
			_popup = null;
		});
	}
}
