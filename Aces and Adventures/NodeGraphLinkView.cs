using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class NodeGraphLinkView : MonoBehaviour
{
	private string _name;

	private NodeGraph _nodeGraph;

	private ReflectTreeData<UIFieldAttribute> _reflectTreeData;

	private byte[] _nodeGraphBytesAtOpen;

	private NodeGraphMasterView _nodeGraphMasterView;

	public RectTransform rectTransform => base.transform as RectTransform;

	private void _OnGraphWindowClose()
	{
		if (!_nodeGraphBytesAtOpen.SequenceEqual(IOUtil.ToByteArray(_nodeGraph)))
		{
			_reflectTreeData.OnValueChanged();
		}
	}

	public NodeGraphLinkView SetData(string name, NodeGraph nodeGraph, ReflectTreeData<UIFieldAttribute> reflectTreeData)
	{
		base.gameObject.SetUILabel(name);
		_name = name;
		_nodeGraph = nodeGraph;
		_nodeGraph.name = _nodeGraph.name ?? name;
		_reflectTreeData = reflectTreeData;
		return this;
	}

	public void OpenGraphWindow()
	{
		UPools.Clear();
		_nodeGraphBytesAtOpen = IOUtil.ToByteArray(_nodeGraph);
		Canvas canvas = rectTransform.GetCanvas();
		_nodeGraphMasterView = DirtyPools.Unpool(NodeGraphMasterView.Blueprint, canvas.transform.parent).GetComponent<NodeGraphMasterView>().SetData(_name, _nodeGraph, canvas.worldCamera ? canvas.worldCamera : CameraManager.Instance.GetUICamera(), canvas.sortingOrder + 1);
		_nodeGraphMasterView.onClose.AddSingleFireListener(_OnGraphWindowClose);
	}
}
