public class CellGroupEventSnapshot : WorldEventSnapshot {

	public long GroupId;
	public CellGroupSnapshot GroupSnapshot;

	public CellGroupEventSnapshot (CellGroupEvent e) : base (e) {

		GroupId = e.GroupId;
		GroupSnapshot = e.Group.GetSnapshot ();
	}
}
