namespace SM.TRAPPIST1.Models;

public class Attraction
{
    public required string Name { get; init; }
    public required string Action { get; init; }
    public required string Tagline { get; init; }
    public required string Description { get; init; }
    public bool IsFictional { get; init; }
    public IReadOnlyList<string> Highlights { get; init; } = [];
    public string VisitorAdvice { get; init; } = "";
}
