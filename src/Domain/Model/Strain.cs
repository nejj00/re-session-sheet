namespace RESessionSheet.Domain.Model;

public class Strain
{
	public int Value { get; set; }

	/// <summary>Every 10 accumulated Strain auto-applies one additional wound-tier tick (visual overlay only).</summary>
	public int TicksFromStrain => Value / 10;
}
