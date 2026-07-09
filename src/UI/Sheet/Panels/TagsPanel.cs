using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// Freeform Merits/Flaws/Traits/Backgrounds for V1 (Name + Description rows, add/remove) — the
/// same section builder is reused across all four categories. TaggedEntry.CatalogId is reserved
/// for a later phase where these link back to a structured catalog with auto-applied mechanical
/// effects instead of being purely descriptive text.
/// </summary>
public partial class TagsPanel : FramedPanel
{
	private static CharacterSheet Sheet => GameState.Instance.CurrentCharacter!.Sheet;

	public override void _Ready()
	{
		base._Ready();
		BuildUi();
	}

	private void BuildUi()
	{
		var scroll = new ScrollContainer
		{
			SizeFlagsVertical = SizeFlags.ExpandFill,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
		};
		Content.AddChild(scroll);

		var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		vbox.AddThemeConstantOverride("separation", 14);
		scroll.AddChild(vbox);

		BuildTaggedEntrySection(vbox, "Merits", Sheet.Merits);
		vbox.AddChild(new HSeparator());
		BuildTaggedEntrySection(vbox, "Flaws", Sheet.Flaws);
		vbox.AddChild(new HSeparator());
		BuildTaggedEntrySection(vbox, "Traits", Sheet.Traits);
		vbox.AddChild(new HSeparator());
		BuildTaggedEntrySection(vbox, "Backgrounds", Sheet.Backgrounds);
	}

	private void BuildTaggedEntrySection(VBoxContainer parent, string title, List<TaggedEntry> entries)
	{
		parent.AddChild(new Label { Text = title });

		var listContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		listContainer.AddThemeConstantOverride("separation", 4);
		parent.AddChild(listContainer);

		void Refresh()
		{
			foreach (Node child in listContainer.GetChildren())
			{
				child.QueueFree();
			}

			foreach (var entry in entries)
			{
				listContainer.AddChild(BuildEntryRow(entry, entries, Refresh));
			}
		}

		var addButton = new Button { Text = $"Add {title.TrimEnd('s')}" };
		addButton.Pressed += () =>
		{
			entries.Add(new TaggedEntry { Name = "" });
			Refresh();
			GameState.Instance.NotifyCharacterEdited();
		};
		parent.AddChild(addButton);

		Refresh();
	}

	private static Control BuildEntryRow(TaggedEntry entry, List<TaggedEntry> entries, Action refresh)
	{
		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 8);

		var name = new LineEdit { Text = entry.Name, PlaceholderText = "Name", CustomMinimumSize = new Vector2(160, 0) };
		name.TextChanged += t => { entry.Name = t; GameState.Instance.NotifyCharacterEdited(); };
		row.AddChild(name);

		var description = new LineEdit { Text = entry.Description, PlaceholderText = "Description", SizeFlagsHorizontal = SizeFlags.ExpandFill };
		description.TextChanged += t => { entry.Description = t; GameState.Instance.NotifyCharacterEdited(); };
		row.AddChild(description);

		var remove = new Button { Text = "Remove" };
		remove.Pressed += () =>
		{
			entries.Remove(entry);
			refresh();
			GameState.Instance.NotifyCharacterEdited();
		};
		row.AddChild(remove);

		return row;
	}
}
