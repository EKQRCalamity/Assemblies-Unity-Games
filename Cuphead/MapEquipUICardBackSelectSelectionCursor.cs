using UnityEngine;

public class MapEquipUICardBackSelectSelectionCursor : MapEquipUICursor
{
	public int selectedIndex = -1;

	public override void SetPosition(Vector3 position)
	{
		base.SetPosition(position);
		Show();
	}

	public override void Show()
	{
		base.Show();
		base.animator.Play("Idle");
	}

	public void Select()
	{
		base.animator.Play("Select");
	}
}
