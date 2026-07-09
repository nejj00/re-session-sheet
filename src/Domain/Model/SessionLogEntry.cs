namespace RESessionSheet.Domain.Model;

public class SessionLogEntry
{
	public int SessionNumber { get; set; }
	public int XpAwarded { get; set; }
	/// <summary>Computed at entry-time and stored, so historical entries display correctly even as later sessions are added.</summary>
	public int RunningXpTotal { get; set; }
	public string Notes { get; set; } = "";
	public DateTime? PlayedOnUtc { get; set; }
}
