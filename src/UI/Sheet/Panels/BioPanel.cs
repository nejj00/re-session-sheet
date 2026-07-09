using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// The first real end-to-end sheet panel: bio fields, backstory, and the archetype tags
/// (Role/Talent/Specialist/ArchetypeMerits/ArchetypeFlaws) that were the biggest gap in the old
/// sheet. Deliberately the simplest panel, so it validates the whole data -&gt; UI -&gt; save pipeline
/// before the more complex XP-gated panels are built on top of the same pattern.
/// </summary>
public partial class BioPanel : FramedPanel
{
	private LineEdit _name = null!;
	private LineEdit _age = null!;
	private LineEdit _gender = null!;
	private LineEdit _height = null!;
	private LineEdit _weight = null!;
	private LineEdit _eyeColor = null!;
	private LineEdit _hairColor = null!;
	private LineEdit _skinTone = null!;
	private LineEdit _demeanor = null!;
	private LineEdit _rank = null!;
	private LineEdit _firstTeam = null!;

	private LineEdit _role = null!;
	private LineEdit _talent = null!;
	private LineEdit _specialist = null!;
	private LineEdit _archetypeMerits = null!;
	private LineEdit _archetypeFlaws = null!;

	private TextEdit _backstory = null!;

	private bool _isLoading;

	private static CharacterSheet Sheet => GameState.Instance.CurrentCharacter!.Sheet;

	public override void _Ready()
	{
		base._Ready();
		BuildUi();
		LoadFromCharacter();
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
		vbox.AddThemeConstantOverride("separation", 10);
		scroll.AddChild(vbox);

		vbox.AddChild(SectionLabel("Identity"));
		var identityGrid = NewGrid(vbox);
		_name = AddField(identityGrid, "Name");
		_age = AddField(identityGrid, "Age");
		_gender = AddField(identityGrid, "Gender");
		_height = AddField(identityGrid, "Height");
		_weight = AddField(identityGrid, "Weight");
		_eyeColor = AddField(identityGrid, "Eye Color");
		_hairColor = AddField(identityGrid, "Hair Color");
		_skinTone = AddField(identityGrid, "Skin Tone");
		_demeanor = AddField(identityGrid, "Demeanor");
		_rank = AddField(identityGrid, "Rank");
		_firstTeam = AddField(identityGrid, "First Team");

		vbox.AddChild(new HSeparator());
		vbox.AddChild(SectionLabel("Archetype"));
		var archetypeGrid = NewGrid(vbox);
		_role = AddField(archetypeGrid, "Role");
		_talent = AddField(archetypeGrid, "Talent");
		_specialist = AddField(archetypeGrid, "Specialist");
		_archetypeMerits = AddField(archetypeGrid, "Merits", "Comma-separated, e.g. Medical Intuition, Urban Evasion");
		_archetypeFlaws = AddField(archetypeGrid, "Flaws", "Comma-separated, e.g. Wanted by Umbrella");

		vbox.AddChild(new HSeparator());
		vbox.AddChild(SectionLabel("Backstory"));
		_backstory = new TextEdit
		{
			CustomMinimumSize = new Vector2(0, 160),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			WrapMode = TextEdit.LineWrappingMode.Boundary,
		};
		_backstory.TextChanged += OnAnyFieldChanged;
		vbox.AddChild(_backstory);
	}

	private static Label SectionLabel(string text) => new()
	{
		Text = text,
		ThemeTypeVariation = "HeaderSmall",
	};

	private static GridContainer NewGrid(Control parent)
	{
		var grid = new GridContainer { Columns = 2, SizeFlagsHorizontal = SizeFlags.ExpandFill };
		grid.AddThemeConstantOverride("h_separation", 12);
		grid.AddThemeConstantOverride("v_separation", 6);
		parent.AddChild(grid);
		return grid;
	}

	private LineEdit AddField(GridContainer grid, string labelText, string placeholder = "")
	{
		grid.AddChild(new Label { Text = labelText });
		var field = new LineEdit { SizeFlagsHorizontal = SizeFlags.ExpandFill, PlaceholderText = placeholder };
		field.TextChanged += _ => OnAnyFieldChanged();
		grid.AddChild(field);
		return field;
	}

	private void LoadFromCharacter()
	{
		_isLoading = true;
		var sheet = Sheet;

		_name.Text = sheet.Name;
		_age.Text = sheet.Age;
		_gender.Text = sheet.Gender;
		_height.Text = sheet.Height;
		_weight.Text = sheet.Weight;
		_eyeColor.Text = sheet.EyeColor;
		_hairColor.Text = sheet.HairColor;
		_skinTone.Text = sheet.SkinTone;
		_demeanor.Text = sheet.Demeanor;
		_rank.Text = sheet.Rank;
		_firstTeam.Text = sheet.FirstTeam;

		_role.Text = sheet.Role;
		_talent.Text = sheet.Talent;
		_specialist.Text = sheet.Specialist;
		_archetypeMerits.Text = string.Join(", ", sheet.ArchetypeMerits);
		_archetypeFlaws.Text = string.Join(", ", sheet.ArchetypeFlaws);

		_backstory.Text = sheet.Backstory;

		_isLoading = false;
	}

	private void OnAnyFieldChanged()
	{
		if (_isLoading)
		{
			return;
		}

		var sheet = Sheet;
		sheet.Name = _name.Text;
		sheet.Age = _age.Text;
		sheet.Gender = _gender.Text;
		sheet.Height = _height.Text;
		sheet.Weight = _weight.Text;
		sheet.EyeColor = _eyeColor.Text;
		sheet.HairColor = _hairColor.Text;
		sheet.SkinTone = _skinTone.Text;
		sheet.Demeanor = _demeanor.Text;
		sheet.Rank = _rank.Text;
		sheet.FirstTeam = _firstTeam.Text;

		sheet.Role = _role.Text;
		sheet.Talent = _talent.Text;
		sheet.Specialist = _specialist.Text;
		sheet.ArchetypeMerits = SplitTags(_archetypeMerits.Text);
		sheet.ArchetypeFlaws = SplitTags(_archetypeFlaws.Text);

		sheet.Backstory = _backstory.Text;

		GameState.Instance.NotifyCharacterEdited();
	}

	private static List<string> SplitTags(string text) =>
		text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
