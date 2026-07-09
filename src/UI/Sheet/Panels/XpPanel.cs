using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// Available/Spent/Total XP display. Available is manually adjustable (used to award session XP,
/// matching the real-world workflow already visible in the reference character's activity log:
/// set Available to the creation/session budget, then spend it down via Attributes &amp; Skills).
/// Total is always computed (Spent + Available), never stored redundantly. Subscribes to
/// GameState.XpChanged so it stays in sync when other panels spend/refund XP.
/// </summary>
public partial class XpPanel : FramedPanel
{
	private Label _spentLabel = null!;
	private Label _totalLabel = null!;
	private SpinBox _availableSpin = null!;
	private bool _isLoading;

	private static CharacterSheet Sheet => GameState.Instance.CurrentCharacter!.Sheet;

	public override void _Ready()
	{
		base._Ready();
		BuildUi();
		LoadFromCharacter();
		GameState.Instance.XpChanged += LoadFromCharacter;
	}

	public override void _ExitTree()
	{
		if (GameState.Instance != null)
		{
			GameState.Instance.XpChanged -= LoadFromCharacter;
		}
	}

	private void BuildUi()
	{
		var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		vbox.AddThemeConstantOverride("separation", 10);
		Content.AddChild(vbox);

		vbox.AddChild(new Label { Text = "Experience" });

		var grid = new GridContainer { Columns = 2 };
		grid.AddThemeConstantOverride("h_separation", 12);
		grid.AddThemeConstantOverride("v_separation", 6);
		vbox.AddChild(grid);

		grid.AddChild(new Label { Text = "Total (Spent + Available)" });
		_totalLabel = new Label { Text = "0" };
		grid.AddChild(_totalLabel);

		grid.AddChild(new Label { Text = "Spent" });
		_spentLabel = new Label { Text = "0" };
		grid.AddChild(_spentLabel);

		grid.AddChild(new Label { Text = "Available" });
		_availableSpin = new SpinBox { MinValue = 0, MaxValue = 100000, Step = 1 };
		_availableSpin.ValueChanged += OnAvailableChanged;
		grid.AddChild(_availableSpin);
	}

	private void LoadFromCharacter()
	{
		_isLoading = true;
		var xp = Sheet.Xp;
		_spentLabel.Text = xp.Spent.ToString();
		_totalLabel.Text = xp.Total.ToString();
		_availableSpin.Value = xp.Available;
		_isLoading = false;
	}

	private void OnAvailableChanged(double value)
	{
		if (_isLoading)
		{
			return;
		}

		Sheet.Xp.Available = (int)value;
		GameState.Instance.NotifyXpChanged();
	}
}
