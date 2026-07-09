using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;

namespace RESessionSheet.UI.RosterScreen;

public partial class RosterScreen : Control
{
	[Signal]
	public delegate void CharacterOpenedEventHandler();

	private ItemList _list = null!;
	private LineEdit _nameInput = null!;
	private Label _statusLabel = null!;
	private List<CharacterFile> _roster = new();

	public override void _Ready()
	{
		_list = GetNode<ItemList>("%RosterList");
		_nameInput = GetNode<LineEdit>("%NameInput");
		_statusLabel = GetNode<Label>("%StatusLabel");

		GetNode<Button>("%NewButton").Pressed += OnNewPressed;
		GetNode<Button>("%OpenButton").Pressed += OnOpenPressed;
		GetNode<Button>("%DeleteButton").Pressed += OnDeletePressed;
		GetNode<Button>("%ImportButton").Pressed += OnImportPressed;

		RefreshRoster();
	}

	private void RefreshRoster()
	{
		_roster = GameState.Instance.ListRoster()
			.OrderByDescending(c => c.LastModifiedUtc)
			.ToList();

		_list.Clear();
		foreach (var character in _roster)
		{
			string label = $"{character.Sheet.Name}   —   last played {character.LastModifiedUtc:yyyy-MM-dd HH:mm}";
			_list.AddItem(label);
		}
	}

	private void OnNewPressed()
	{
		string name = _nameInput.Text.Trim();
		if (string.IsNullOrEmpty(name))
		{
			_statusLabel.Text = "Enter a character name first.";
			return;
		}

		var file = GameState.Instance.CreateCharacter(name);
		_nameInput.Text = "";
		_statusLabel.Text = $"Created \"{file.Sheet.Name}\".";
		RefreshRoster();
	}

	private void OnOpenPressed()
	{
		int selected = GetSelectedIndex();
		if (selected < 0)
		{
			_statusLabel.Text = "Select a character first.";
			return;
		}

		var character = _roster[selected];
		GameState.Instance.LoadCharacter(character.Id);
		_statusLabel.Text = $"Loaded \"{character.Sheet.Name}\".";
		EmitSignal(SignalName.CharacterOpened);
	}

	private void OnDeletePressed()
	{
		int selected = GetSelectedIndex();
		if (selected < 0)
		{
			_statusLabel.Text = "Select a character first.";
			return;
		}

		var character = _roster[selected];
		GameState.Instance.DeleteCharacter(character.Id);
		_statusLabel.Text = $"Deleted \"{character.Sheet.Name}\".";
		RefreshRoster();
	}

	private void OnImportPressed()
	{
		var dialog = new FileDialog
		{
			FileMode = FileDialog.FileModeEnum.OpenFile,
			Access = FileDialog.AccessEnum.Filesystem,
			Title = "Import Legacy JSON",
			Filters = new[] { "*.json ; Legacy Character JSON" }
		};

		dialog.FileSelected += path =>
		{
			try
			{
				string json = System.IO.File.ReadAllText(path);
				var migrated = Domain.Serialization.LegacyImporter.Import(json);
				GameState.Instance.SaveService.Save(migrated);
				_statusLabel.Text = $"Imported \"{migrated.Sheet.Name}\".";
				RefreshRoster();
			}
			catch (Exception ex)
			{
				_statusLabel.Text = $"Import failed: {ex.Message}";
			}
			finally
			{
				dialog.QueueFree();
			}
		};

		dialog.Canceled += () => dialog.QueueFree();

		AddChild(dialog);
		dialog.PopupCentered(new Vector2I(600, 400));
	}

	private int GetSelectedIndex()
	{
		var selected = _list.GetSelectedItems();
		return selected.Length > 0 ? selected[0] : -1;
	}
}
