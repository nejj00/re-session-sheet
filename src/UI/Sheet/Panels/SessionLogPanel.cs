using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// An append-only session journal: session number, XP awarded (which also feeds XpLedger.Available,
/// so the award immediately becomes spendable in the Attributes &amp; Skills panel), a running XP
/// total captured at entry time, and freeform notes. This is the direct replacement for the manual
/// char_info.txt workflow — the biggest gap in the old sheet.
/// </summary>
public partial class SessionLogPanel : FramedPanel
{
	private VBoxContainer _entriesContainer = null!;
	private SpinBox _sessionNumberSpin = null!;
	private SpinBox _xpAwardedSpin = null!;
	private TextEdit _notesEdit = null!;

	private static CharacterSheet Sheet => GameState.Instance.CurrentCharacter!.Sheet;

	public override void _Ready()
	{
		base._Ready();
		BuildUi();
		RefreshEntries();
	}

	private void BuildUi()
	{
		var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		vbox.AddThemeConstantOverride("separation", 10);
		Content.AddChild(vbox);

		vbox.AddChild(new Label { Text = "Session Log" });

		var scroll = new ScrollContainer
		{
			SizeFlagsVertical = SizeFlags.ExpandFill,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
		};
		vbox.AddChild(scroll);

		_entriesContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		_entriesContainer.AddThemeConstantOverride("separation", 6);
		scroll.AddChild(_entriesContainer);

		vbox.AddChild(new HSeparator());
		vbox.AddChild(new Label { Text = "New Entry" });

		var formGrid = new GridContainer { Columns = 2 };
		formGrid.AddThemeConstantOverride("h_separation", 12);
		formGrid.AddThemeConstantOverride("v_separation", 6);
		vbox.AddChild(formGrid);

		formGrid.AddChild(new Label { Text = "Session #" });
		_sessionNumberSpin = new SpinBox { MinValue = 1, MaxValue = 9999, Step = 1 };
		formGrid.AddChild(_sessionNumberSpin);

		formGrid.AddChild(new Label { Text = "XP Awarded" });
		_xpAwardedSpin = new SpinBox { MinValue = 0, MaxValue = 9999, Step = 1 };
		formGrid.AddChild(_xpAwardedSpin);

		vbox.AddChild(new Label { Text = "Notes" });
		_notesEdit = new TextEdit
		{
			CustomMinimumSize = new Vector2(0, 100),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			WrapMode = TextEdit.LineWrappingMode.Boundary,
		};
		vbox.AddChild(_notesEdit);

		var addButton = new Button { Text = "Add Entry" };
		addButton.Pressed += OnAddEntryPressed;
		vbox.AddChild(addButton);
	}

	private void RefreshEntries()
	{
		foreach (Node child in _entriesContainer.GetChildren())
		{
			child.QueueFree();
		}

		foreach (var entry in Sheet.SessionLog)
		{
			_entriesContainer.AddChild(BuildEntryRow(entry));
		}

		int nextSession = Sheet.SessionLog.Count > 0 ? Sheet.SessionLog[^1].SessionNumber + 1 : 1;
		_sessionNumberSpin.Value = nextSession;
		_xpAwardedSpin.Value = 0;
		_notesEdit.Text = "";
	}

	private Control BuildEntryRow(SessionLogEntry entry)
	{
		var box = new VBoxContainer();

		var headerBox = new HBoxContainer();
		headerBox.AddThemeConstantOverride("separation", 8);

		string played = entry.PlayedOnUtc.HasValue ? entry.PlayedOnUtc.Value.ToLocalTime().ToString("yyyy-MM-dd") : "";
		var header = new Label
		{
			Text = $"Session {entry.SessionNumber} — +{entry.XpAwarded} XP (Total: {entry.RunningXpTotal}){(played != "" ? "  —  " + played : "")}",
		};
		headerBox.AddChild(header);

		var remove = new Button { Text = "Remove" };
		remove.Pressed += () => OnRemoveEntryPressed(entry);
		headerBox.AddChild(remove);

		box.AddChild(headerBox);

		if (!string.IsNullOrWhiteSpace(entry.Notes))
		{
			var notes = new Label { Text = entry.Notes, AutowrapMode = TextServer.AutowrapMode.WordSmart };
			box.AddChild(notes);
		}

		box.AddChild(new HSeparator());
		return box;
	}

	private void OnAddEntryPressed()
	{
		int previousTotal = Sheet.SessionLog.Count > 0 ? Sheet.SessionLog[^1].RunningXpTotal : 0;
		int xpAwarded = (int)_xpAwardedSpin.Value;

		var entry = new SessionLogEntry
		{
			SessionNumber = (int)_sessionNumberSpin.Value,
			XpAwarded = xpAwarded,
			RunningXpTotal = previousTotal + xpAwarded,
			Notes = _notesEdit.Text,
			PlayedOnUtc = DateTime.UtcNow,
		};

		Sheet.SessionLog.Add(entry);
		Sheet.Xp.Available += xpAwarded;

		RefreshEntries();
		GameState.Instance.NotifyXpChanged();
		GameState.Instance.NotifyCharacterEdited();
	}

	private void OnRemoveEntryPressed(SessionLogEntry entry)
	{
		Sheet.SessionLog.Remove(entry);
		Sheet.Xp.Available -= entry.XpAwarded;

		RefreshEntries();
		GameState.Instance.NotifyXpChanged();
		GameState.Instance.NotifyCharacterEdited();
	}
}
