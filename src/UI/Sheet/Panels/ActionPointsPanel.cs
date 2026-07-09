using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// A tracked/editable Action Point pool (current/max), plus a static reference list of the
/// non-weapon-specific AP costs from the rulebook. Reset is a manual button for V1 — there's no
/// encounter/scene concept modeled yet to trigger it automatically (confirmed assumption).
/// Per-weapon AP costs are out of scope until a structured weapon catalog exists (later phase).
/// </summary>
public partial class ActionPointsPanel : FramedPanel
{
	private SpinBox _currentSpin = null!;
	private SpinBox _maxSpin = null!;
	private bool _isLoading;

	private static readonly (string Action, string Cost)[] ReferenceCosts =
	{
		("Movement", "2 AP per 10 ft"),
		("Parkour action (vault/roll/slide/jump)", "3 AP base, +1 AP per chained action"),
		("Healing (item use)", "2 AP"),
		("Throwing a grenade", "3 AP"),
		("Reloading", "Weapon-dependent (see weapon card)"),
		("Attacking", "Weapon-dependent (see weapon card)"),
	};

	private static CharacterSheet Sheet => GameState.Instance.CurrentCharacter!.Sheet;

	public override void _Ready()
	{
		base._Ready();
		BuildUi();
		LoadFromCharacter();
	}

	private void BuildUi()
	{
		var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		vbox.AddThemeConstantOverride("separation", 12);
		Content.AddChild(vbox);

		vbox.AddChild(new Label { Text = "Action Points" });

		var grid = new GridContainer { Columns = 2 };
		grid.AddThemeConstantOverride("h_separation", 12);
		grid.AddThemeConstantOverride("v_separation", 6);
		vbox.AddChild(grid);

		grid.AddChild(new Label { Text = "Current" });
		_currentSpin = new SpinBox { MinValue = 0, MaxValue = 999, Step = 1 };
		_currentSpin.ValueChanged += OnCurrentChanged;
		grid.AddChild(_currentSpin);

		grid.AddChild(new Label { Text = "Max" });
		_maxSpin = new SpinBox { MinValue = 1, MaxValue = 999, Step = 1 };
		_maxSpin.ValueChanged += OnMaxChanged;
		grid.AddChild(_maxSpin);

		var resetButton = new Button { Text = "Reset to Max" };
		resetButton.Pressed += OnResetPressed;
		vbox.AddChild(resetButton);

		vbox.AddChild(new HSeparator());
		vbox.AddChild(new Label { Text = "AP Reference" });

		var refGrid = new GridContainer { Columns = 2 };
		refGrid.AddThemeConstantOverride("h_separation", 16);
		refGrid.AddThemeConstantOverride("v_separation", 4);
		vbox.AddChild(refGrid);

		foreach (var (action, cost) in ReferenceCosts)
		{
			refGrid.AddChild(new Label { Text = action });
			refGrid.AddChild(new Label { Text = cost });
		}
	}

	private void LoadFromCharacter()
	{
		_isLoading = true;
		_currentSpin.Value = Sheet.Ap.Current;
		_maxSpin.Value = Sheet.Ap.Max;
		_isLoading = false;
	}

	private void OnCurrentChanged(double value)
	{
		if (_isLoading)
		{
			return;
		}

		Sheet.Ap.Current = (int)value;
		GameState.Instance.NotifyApChanged();
	}

	private void OnMaxChanged(double value)
	{
		if (_isLoading)
		{
			return;
		}

		Sheet.Ap.Max = (int)value;
		GameState.Instance.NotifyApChanged();
	}

	private void OnResetPressed()
	{
		Sheet.Ap.Current = Sheet.Ap.Max;
		LoadFromCharacter();
		GameState.Instance.NotifyApChanged();
	}
}
