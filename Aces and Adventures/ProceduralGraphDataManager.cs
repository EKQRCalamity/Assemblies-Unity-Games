using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralGraphDataManager : MonoBehaviour
{
	public UIGeneratorType dataRefGenerator;

	public UIGeneratorType selectedGenerator;

	public ProceduralMapEditorView mapView;

	public BoolEvent onShowDataRefChange;

	private DataRef<ProceduralGraphData> _editing;

	private void _OnDataRefChange(DataRefControl dataRefControl)
	{
		if (dataRefControl.dataRef is DataRef<ProceduralGraphData> dataRef && dataRefControl.GetComponentInParent<UIGeneratorType>() == dataRefGenerator && (_editing = dataRef).IsValid())
		{
			mapView.map = new ProceduralMap(((ProceduralGraphData)dataRefControl.data).graph, dataRef);
		}
	}

	private void _OnSelectedNodesChange(List<uint> selectedNodeIds)
	{
		selectedGenerator.GenerateFromObject((selectedNodeIds == null || selectedNodeIds.Count != 1) ? null : mapView?.map?.graph?[selectedNodeIds[0]]);
		onShowDataRefChange?.Invoke(selectedNodeIds.IsNullOrEmpty());
	}

	private void _OnSelectedValueChange()
	{
		mapView.SetDirty();
		if (mapView.map.graph.GetSelectedNodes().Count() != 1)
		{
			return;
		}
		foreach (ProceduralNode selectedNode in mapView.map.graph.GetSelectedNodes())
		{
			mapView[selectedNode.id].UpdateNodeType();
		}
	}

	private void _OnMapDirty()
	{
		DataRefControl.GetControl(_editing)?.Refresh();
	}

	private void Awake()
	{
		InputManager.I.hideCursorWhileRightClickDragging = false;
		DataRefControl.OnDataRefChanged += _OnDataRefChange;
		mapView.onSelectedNodesChange += _OnSelectedNodesChange;
		mapView.onDirty.AddListener(_OnMapDirty);
		selectedGenerator.OnValueChanged.AddListener(_OnSelectedValueChange);
	}

	private void OnDestroy()
	{
		DataRefControl.OnDataRefChanged -= _OnDataRefChange;
	}
}
