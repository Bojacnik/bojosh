namespace bojosh.Options;

/// <summary>
/// Configuration options for retrieving study programmes.
/// Used to filter programmes by specific year, type, or language.
/// </summary>
public record GetStudyProgrammesOpts(int Year, ProgrammeType Type, Language Language);

public enum ProgrammeType
{
    all,
    B,
    N,
    D,
}