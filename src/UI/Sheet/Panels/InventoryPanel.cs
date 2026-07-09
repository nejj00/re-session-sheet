using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// A structured item list (Name/Category/Quantity/Equipped/Notes) replacing the old sheet's
/// freeform inventory textarea. Grid-related fields on InventoryItem (GridWidth/Height/Rotation/
/// IconPath/StackId) exist on the model for the future V2 grid inventory but aren't exposed here.
/// Item identity is always by Guid, never list position.
/// </summary>
public partial class InventoryPanel : FramedPanel
{
	private VBoxContainer _itemsContainer = null!;

	private static CharacterSheet Sheet => GameState.Instance.CurrentCharacter!.Sheet;

	public override void _Ready()
	{
		base._Ready();
		BuildUi();
		RefreshItems();
	}

	private void BuildUi()
	{
		var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		vbox.AddThemeConstantOverride("separation", 10);
		Content.AddChild(vbox);

		vbox.AddChild(new Label { Text = "Inventory" });

		var scroll = new ScrollContainer
		{
			SizeFlagsVertical = SizeFlags.ExpandFill,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
		};
		vbox.AddChild(scroll);

		_itemsContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		_itemsContainer.AddThemeConstantOverride("separation", 4);
		scroll.AddChild(_itemsContainer);

		var addButton = new Button { Text = "Add Item" };
		addButton.Pressed += OnAddItemPressed;
		vbox.AddChild(addButton);
	}

	private void RefreshItems()
	{
		foreach (Node child in _itemsContainer.GetChildren())
		{
			child.QueueFree();
		}

		foreach (var item in Sheet.Inventory.Items)
		{
			_itemsContainer.AddChild(BuildItemRow(item));
		}
	}

	private Control BuildItemRow(InventoryItem item)
	{
		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 8);

		var name = new LineEdit { Text = item.Name, PlaceholderText = "Name", CustomMinimumSize = new Vector2(140, 0) };
		name.TextChanged += t => { item.Name = t; SaveInventory(); };
		row.AddChild(name);

		var category = new LineEdit { Text = item.Category, PlaceholderText = "Category", CustomMinimumSize = new Vector2(100, 0) };
		category.TextChanged += t => { item.Category = t; SaveInventory(); };
		row.AddChild(category);

		var quantity = new SpinBox { MinValue = 0, MaxValue = 999, Step = 1, Value = item.Quantity, CustomMinimumSize = new Vector2(70, 0) };
		quantity.ValueChanged += v => { item.Quantity = (int)v; SaveInventory(); };
		row.AddChild(quantity);

		var equipped = new CheckBox { Text = "Equipped", ButtonPressed = item.Equipped };
		equipped.Toggled += v => { item.Equipped = v; SaveInventory(); };
		row.AddChild(equipped);

		var notes = new LineEdit { Text = item.Notes, PlaceholderText = "Notes", SizeFlagsHorizontal = SizeFlags.ExpandFill };
		notes.TextChanged += t => { item.Notes = t; SaveInventory(); };
		row.AddChild(notes);

		var remove = new Button { Text = "Remove" };
		remove.Pressed += () => OnRemoveItemPressed(item);
		row.AddChild(remove);

		return row;
	}

	private void OnAddItemPressed()
	{
		Sheet.Inventory.Items.Add(new InventoryItem { Name = "" });
		RefreshItems();
		SaveInventory();
	}

	private void OnRemoveItemPressed(InventoryItem item)
	{
		Sheet.Inventory.Items.Remove(item);
		RefreshItems();
		SaveInventory();
	}

	private static void SaveInventory() => GameState.Instance.NotifyInventoryChanged();
}
