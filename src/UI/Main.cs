using Godot;
using RESessionSheet.UI.Sheet;

namespace RESessionSheet.UI;

/// <summary>
/// Entry scene: routes between the roster screen and the character sheet. Fully-qualified type
/// names are used for RosterScreen to avoid the namespace/type name collision
/// (RESessionSheet.UI.RosterScreen.RosterScreen).
/// </summary>
public partial class Main : Control
{
	private readonly PackedScene _rosterScene = GD.Load<PackedScene>("res://src/UI/RosterScreen/RosterScreen.tscn");
	private readonly PackedScene _sheetScene = GD.Load<PackedScene>("res://src/UI/Sheet/SheetRoot.tscn");
	private Control? _current;

	public override void _Ready()
	{
		ShowRoster();
	}

	private void ShowRoster()
	{
		var roster = _rosterScene.Instantiate<RESessionSheet.UI.RosterScreen.RosterScreen>();
		roster.CharacterOpened += ShowSheet;
		SwapContent(roster);
	}

	private void ShowSheet()
	{
		var sheet = _sheetScene.Instantiate<SheetRoot>();
		sheet.BackRequested += ShowRoster;
		SwapContent(sheet);
	}

	private void SwapContent(Control node)
	{
		_current?.QueueFree();
		node.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(node);
		_current = node;
	}
}
