namespace RESessionSheet.Domain.Rules;

/// <summary>
/// Cumulative XP cost to REACH a given dot level (index 0..6). Confirmed from the old sheet's
/// dot-buying rules: cost to raise from level N to N+1 is the difference between consecutive entries.
/// Virtues use the same curve as Attributes (unconfirmed by the rulebook, but the WoD-derived default).
/// </summary>
public static class XpCostTables
{
	public static readonly int[] AttributeCumulative = { 0, 5, 15, 30, 50, 75, 105 };
	public static readonly int[] SkillCumulative = { 0, 3, 9, 18, 30, 45, 63 };
	public static readonly int[] VirtueCumulative = AttributeCumulative;
}
