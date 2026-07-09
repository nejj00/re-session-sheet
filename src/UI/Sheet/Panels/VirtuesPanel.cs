using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Rules;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// Conscience, Self-Control, and Courage — same DotRating + XpEngine pattern as Attributes,
/// using the same cost curve (XpCostTables.VirtueCumulative, unconfirmed by the rulebook but the
/// WoD-derived default agreed during planning).
/// </summary>
public partial class VirtuesPanel : FramedPanel
{
	private Label _statusLabel = null!;
	private DotRating _conscience = null!;
	private DotRating _selfControl = null!;
	private DotRating _courage = null!;

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
		vbox.AddThemeConstantOverride("separation", 10);
		Content.AddChild(vbox);

		vbox.AddChild(new Label { Text = "Virtues" });

		_conscience = AddVirtueRow(vbox, "Conscience",
			() => Sheet.Virtues.Conscience,
			v => Sheet.Virtues.Conscience = v);
		_selfControl = AddVirtueRow(vbox, "Self-Control",
			() => Sheet.Virtues.SelfControl,
			v => Sheet.Virtues.SelfControl = v);
		_courage = AddVirtueRow(vbox, "Courage",
			() => Sheet.Virtues.Courage,
			v => Sheet.Virtues.Courage = v);

		_statusLabel = new Label { Text = "" };
		vbox.AddChild(_statusLabel);
	}

	private DotRating AddVirtueRow(VBoxContainer parent, string label, Func<int> getter, Action<int> setter)
	{
		var row = new HBoxContainer();
		row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2(120, 0) });

		var dot = new DotRating { MaxValue = 6 };
		dot.ValueRequested += v => OnVirtueValueRequested(dot, getter, setter, label, v);
		row.AddChild(dot);

		parent.AddChild(row);
		return dot;
	}

	private void LoadFromCharacter()
	{
		_conscience.Value = Sheet.Virtues.Conscience;
		_selfControl.Value = Sheet.Virtues.SelfControl;
		_courage.Value = Sheet.Virtues.Courage;
	}

	private void OnVirtueValueRequested(DotRating dot, Func<int> getter, Action<int> setter, string name, int targetLevel)
	{
		int current = getter();
		bool ok = XpEngine.TrySetAttribute(Sheet.Xp, ref current, targetLevel, XpCostTables.VirtueCumulative);

		if (ok)
		{
			setter(current);
			dot.Value = current;
			_statusLabel.Text = "";
			GameState.Instance.NotifyXpChanged();
		}
		else
		{
			_statusLabel.Text = $"Not enough XP to raise {name} to {targetLevel}.";
		}
	}
}
