namespace RESessionSheet.Domain.Rules;

public enum HerbColor
{
	Green,
	Red,
	Blue
}

/// <summary>
/// The result of consuming a herb combo (or First Aid Spray). Application of this effect to a
/// character's HealthTrack/Strain (deciding which specific boxes get healed) is the Inventory/Health
/// panel's job, not this rules layer's.
/// </summary>
public record HerbEffect(
	int HpHealed = 0,
	int StrainHealed = 0,
	bool GrantsSoak = false,
	bool GrantsPoisonImmunity = false,
	bool FullReset = false);

/// <summary>
/// Data-driven resolver for the rulebook's confirmed herb combos. Combo order doesn't matter —
/// Green+Red+Blue resolves the same as Blue+Green+Red.
/// </summary>
public static class HerbRecipes
{
	public static readonly HerbEffect FirstAidSpray = new(FullReset: true);

	private static readonly Dictionary<string, HerbEffect> Recipes = BuildRecipes();

	private static Dictionary<string, HerbEffect> BuildRecipes()
	{
		var dict = new Dictionary<string, HerbEffect>();

		void Add(HerbEffect effect, params HerbColor[] herbs) => dict[CanonicalKey(herbs)] = effect;

		Add(new HerbEffect(HpHealed: 5), HerbColor.Green, HerbColor.Green);
		Add(new HerbEffect(HpHealed: 6), HerbColor.Green, HerbColor.Green, HerbColor.Green);
		Add(new HerbEffect(HpHealed: 5, GrantsSoak: true), HerbColor.Green, HerbColor.Green, HerbColor.Red);
		Add(new HerbEffect(HpHealed: 4, GrantsPoisonImmunity: true), HerbColor.Green, HerbColor.Blue);
		Add(new HerbEffect(HpHealed: 5, GrantsPoisonImmunity: true), HerbColor.Green, HerbColor.Green, HerbColor.Blue);
		Add(new HerbEffect(StrainHealed: 4, GrantsPoisonImmunity: true), HerbColor.Red, HerbColor.Blue);
		Add(new HerbEffect(HpHealed: 4, StrainHealed: 4, GrantsSoak: true, GrantsPoisonImmunity: true), HerbColor.Red, HerbColor.Green, HerbColor.Blue);

		return dict;
	}

	private static string CanonicalKey(IEnumerable<HerbColor> herbs) =>
		string.Join("+", herbs.OrderBy(c => (int)c));

	/// <summary>Returns the matching combo's effect, or null if the given herbs don't form a known combo.</summary>
	public static HerbEffect? Resolve(IEnumerable<HerbColor> herbsUsed)
	{
		var list = herbsUsed.ToList();
		if (list.Count == 0)
		{
			return null;
		}

		return Recipes.TryGetValue(CanonicalKey(list), out var effect) ? effect : null;
	}
}
