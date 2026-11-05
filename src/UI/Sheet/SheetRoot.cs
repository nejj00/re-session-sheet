using Godot;
using RESessionSheet.App;

namespace RESessionSheet.UI.Sheet;

/// <summary>
/// Top-level tab container for the character sheet. Owns the header (name, back, save) and hosts
/// one tab per sheet panel; each panel reads/writes the current character via GameState.
/// </summary>
public partial class SheetRoot : Control
{
	[Signal]
	public delegate void BackRequestedEventHandler();

	private Label _statusLabel = null!;

	public override void _Ready()
	{
		GetNode<Label>("%CharacterNameLabel").Text = GameState.Instance.CurrentCharacter?.Sheet.Name ?? "(unnamed)";
		_statusLabel = GetNode<Label>("%StatusLabel");

		GetNode<Button>("%BackButton").Pressed += () => EmitSignal(SignalName.BackRequested);
		GetNode<Button>("%SaveButton").Pressed += OnSavePressed;

		var tabs = GetNode<TabContainer>("VBox/Tabs");
		SetTabIcon(tabs, 0, "res://assets/ui/tabs/biohazard-sign.png");
		SetTabIcon(tabs, 1, "res://assets/ui/tabs/biohazard-sign.png");
		SetTabIcon(tabs, 2, "res://assets/ui/tabs/biohazard-sign.png");
		SetTabIcon(tabs, 3, "res://assets/ui/tabs/biohazard-sign.png");
		SetTabIcon(tabs, 4, "res://assets/ui/tabs/biohazard-sign.png");
		SetTabIcon(tabs, 5, "res://assets/ui/tabs/biohazard-sign.png");
		SetTabIcon(tabs, 6, "res://assets/ui/tabs/biohazard-sign.png");
		SetTabIcon(tabs, 7, "res://assets/ui/tabs/biohazard-sign.png");
		SetTabIcon(tabs, 8, "res://assets/ui/tabs/biohazard-sign.png");
	}
	private static void SetTabIcon(TabContainer tabs, int index, string path)
	{
		tabs.SetTabTitle(index, "");
		tabs.SetTabIcon(index, GD.Load<Texture2D>(path));
	}
	private void OnSavePressed()
	{
		GameState.Instance.SaveNow();
		_statusLabel.Text = $"Saved at {DateTime.Now:HH:mm:ss}.";
	}
}
