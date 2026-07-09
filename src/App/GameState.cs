using Godot;
using RESessionSheet.App.Services;
using RESessionSheet.Domain.Model;

namespace RESessionSheet.App;

/// <summary>
/// Godot Autoload singleton — owns the current CharacterFile and SaveService, and is the source
/// of the cross-panel reactivity signals (the idiomatic Godot equivalent of Flare's manual
/// session.py listener lists). Panels write mutations through this class's methods and subscribe
/// to its signals to stay in sync with each other.
/// </summary>
public partial class GameState : Node
{
	private const float AutosaveDebounceSeconds = 0.5f;

	public static GameState Instance { get; private set; } = null!;

	[Signal] public delegate void CharacterLoadedEventHandler();
	[Signal] public delegate void XpChangedEventHandler();
	[Signal] public delegate void HealthChangedEventHandler();
	[Signal] public delegate void ApChangedEventHandler();
	[Signal] public delegate void InventoryChangedEventHandler();
	[Signal] public delegate void SkillChangedEventHandler();

	public SaveService SaveService { get; private set; } = null!;
	public CharacterFile? CurrentCharacter { get; private set; }

	private Godot.Timer _autosaveTimer = null!;

	public override void _Ready()
	{
		Instance = this;

		string charactersPath = ProjectSettings.GlobalizePath("user://characters");
		SaveService = new SaveService(charactersPath);

		_autosaveTimer = new Godot.Timer { OneShot = true, WaitTime = AutosaveDebounceSeconds };
		_autosaveTimer.Timeout += OnAutosaveTimeout;
		AddChild(_autosaveTimer);
	}

	public IReadOnlyList<CharacterFile> ListRoster() => SaveService.ListRoster();

	public void LoadCharacter(Guid id)
	{
		CurrentCharacter = SaveService.Load(id);
		EmitSignal(SignalName.CharacterLoaded);
	}

	public CharacterFile CreateCharacter(string name)
	{
		var file = SaveService.CreateNew(name);
		CurrentCharacter = file;
		EmitSignal(SignalName.CharacterLoaded);
		return file;
	}

	public void DeleteCharacter(Guid id)
	{
		SaveService.Delete(id);
		if (CurrentCharacter?.Id == id)
		{
			CurrentCharacter = null;
		}
	}

	/// <summary>Reliable, immediate save — always available regardless of the autosave debounce.</summary>
	public void SaveNow()
	{
		if (CurrentCharacter != null)
		{
			SaveService.Save(CurrentCharacter);
		}
	}

	private void RequestAutosave()
	{
		_autosaveTimer.Start(AutosaveDebounceSeconds);
	}

	/// <summary>Autosave trigger for edits that don't need to notify other panels (e.g. bio text fields).</summary>
	public void NotifyCharacterEdited() => RequestAutosave();

	private void OnAutosaveTimeout() => SaveNow();

	public void NotifyXpChanged()
	{
		RequestAutosave();
		EmitSignal(SignalName.XpChanged);
	}

	public void NotifyHealthChanged()
	{
		RequestAutosave();
		EmitSignal(SignalName.HealthChanged);
	}

	public void NotifyApChanged()
	{
		RequestAutosave();
		EmitSignal(SignalName.ApChanged);
	}

	public void NotifyInventoryChanged()
	{
		RequestAutosave();
		EmitSignal(SignalName.InventoryChanged);
	}

	public void NotifySkillChanged()
	{
		RequestAutosave();
		EmitSignal(SignalName.SkillChanged);
	}
}
