using Godot;

namespace RESessionSheet.UI.Components;

/// <summary>
/// Wraps panel content with a background art layer drawn behind it — the direct translation of
/// Flare's frames.py idiom (decorative skin separate from functional widgets, keyed by a pack
/// path). V1 ships no real decorative art: SkinBackground stays hidden and the PanelContainer's
/// base_theme.tres StyleBoxFlat provides the visual frame instead. Every sheet panel is wrapped in
/// FramedPanel from day one so swapping in real art later (setting SkinTexturePath) touches zero
/// functional panel code.
/// </summary>
public partial class FramedPanel : PanelContainer
{
	private string _skinTexturePath = "";

	[Export]
	public string SkinTexturePath
	{
		get => _skinTexturePath;
		set
		{
			_skinTexturePath = value;
			ApplySkin();
		}
	}

	private TextureRect? _skinBackground;

	/// <summary>The container callers should add their actual panel content into.</summary>
	public Control Content { get; private set; } = null!;

	public override void _Ready()
	{
		_skinBackground = GetNode<TextureRect>("%SkinBackground");
		Content = GetNode<Control>("%Content");
		ApplySkin();
	}

	private void ApplySkin()
	{
		if (_skinBackground == null)
		{
			return; // not ready yet; _Ready calls this again once nodes are resolved
		}

		if (string.IsNullOrEmpty(_skinTexturePath) || !ResourceLoader.Exists(_skinTexturePath))
		{
			_skinBackground.Visible = false;
			return;
		}

		_skinBackground.Texture = GD.Load<Texture2D>(_skinTexturePath);
		_skinBackground.Visible = true;
	}
}
