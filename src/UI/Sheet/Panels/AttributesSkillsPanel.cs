using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Rules;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// The 9 Attributes and 30 Skills, each a reusable DotRating instance wired through XpEngine.
/// DotRating never mutates data itself — this panel decides whether to accept a ValueRequested
/// (charging/refunding XpLedger via XpEngine) and only then updates the dot's displayed Value.
/// </summary>
public partial class AttributesSkillsPanel : FramedPanel
{
	private Label _statusLabel = null!;
	private readonly Dictionary<AttributeId, DotRating> _attributeDots = new();
	private readonly Dictionary<SkillId, DotRating> _skillDots = new();
	private readonly Dictionary<SkillId, LineEdit> _skillSpecialties = new();

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
		vbox.AddThemeConstantOverride("separation", 12);
		scroll.AddChild(vbox);

		vbox.AddChild(new Label { Text = "Attributes" });
		var attributeRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		attributeRow.AddThemeConstantOverride("separation", 24);
		vbox.AddChild(attributeRow);

		foreach (var category in new[] { AttributeCategory.Physical, AttributeCategory.Social, AttributeCategory.Mental })
		{
			var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			column.AddChild(new Label { Text = category.ToString() });

			foreach (var (id, cat, label) in AttributeCatalog.All)
			{
				if (cat != category)
				{
					continue;
				}

				var row = new HBoxContainer();
				row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2(110, 0) });

				var dot = new DotRating { MaxValue = 6 };
				var capturedId = id;
				dot.ValueRequested += v => OnAttributeValueRequested(capturedId, v);
				row.AddChild(dot);

				_attributeDots[id] = dot;
				column.AddChild(row);
			}

			attributeRow.AddChild(column);
		}

		vbox.AddChild(new HSeparator());
		vbox.AddChild(new Label { Text = "Skills" });
		var skillRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		skillRow.AddThemeConstantOverride("separation", 24);
		vbox.AddChild(skillRow);

		foreach (var category in new[] { SkillCategory.Talent, SkillCategory.Skill, SkillCategory.Knowledge })
		{
			var column = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
			column.AddChild(new Label { Text = category + "s" });

			foreach (var (id, cat, label) in SkillCatalog.All)
			{
				if (cat != category)
				{
					continue;
				}

				var row = new HBoxContainer();
				row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2(90, 0) });

				var dot = new DotRating { MaxValue = 6 };
				var capturedId = id;
				dot.ValueRequested += v => OnSkillValueRequested(capturedId, v);
				row.AddChild(dot);

				var specialty = new LineEdit
				{
					PlaceholderText = "Specialty",
					CustomMinimumSize = new Vector2(90, 0),
					SizeFlagsHorizontal = SizeFlags.ExpandFill,
				};
				specialty.TextChanged += _ => OnSpecialtyChanged(capturedId, specialty.Text);
				row.AddChild(specialty);

				_skillDots[id] = dot;
				_skillSpecialties[id] = specialty;
				column.AddChild(row);
			}

			skillRow.AddChild(column);
		}

		_statusLabel = new Label { Text = "" };
		vbox.AddChild(_statusLabel);
	}

	private void LoadFromCharacter()
	{
		var sheet = Sheet;

		foreach (var (id, dot) in _attributeDots)
		{
			dot.Value = sheet.Attributes.Get(id);
		}

		foreach (var (id, dot) in _skillDots)
		{
			dot.Value = sheet.Skills.Get(id).Rating;
		}

		foreach (var (id, field) in _skillSpecialties)
		{
			field.Text = sheet.Skills.Get(id).Specialty;
		}
	}

	private void OnAttributeValueRequested(AttributeId id, int targetLevel)
	{
		var sheet = Sheet;
		int current = sheet.Attributes.Get(id);
		bool ok = XpEngine.TrySetAttribute(sheet.Xp, ref current, targetLevel);

		if (ok)
		{
			sheet.Attributes.Set(id, current);
			_attributeDots[id].Value = current;
			_statusLabel.Text = "";
			GameState.Instance.NotifyXpChanged();
		}
		else
		{
			_statusLabel.Text = $"Not enough XP to raise {id} to {targetLevel}.";
		}
	}

	private void OnSkillValueRequested(SkillId id, int targetLevel)
	{
		var sheet = Sheet;
		var skill = sheet.Skills.Get(id);
		bool ok = XpEngine.TrySetSkill(sheet.Xp, skill, targetLevel);

		if (ok)
		{
			_skillDots[id].Value = skill.Rating;
			_statusLabel.Text = "";
			GameState.Instance.NotifyXpChanged();
			GameState.Instance.NotifySkillChanged();
		}
		else
		{
			_statusLabel.Text = $"Not enough XP to raise {id} to {targetLevel}.";
		}
	}

	private void OnSpecialtyChanged(SkillId id, string text)
	{
		Sheet.Skills.Get(id).Specialty = text;
		GameState.Instance.NotifyCharacterEdited();
	}
}
