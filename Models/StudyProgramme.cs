namespace bojosh.Models;

/// <summary>
/// Represents a study programme offered by VUT FIT.
/// Contains information about degree type, duration, language, and other programme metadata.
/// </summary>
public class StudyProgramme
{
    /// <summary>
    /// The name of the study programme.
    /// Example: "Information Technology", "Artificial Intelligence"
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The code identifying the programme.
    /// Example: "BIT" (Bachelor of Information Technology), "MIT" (Master of IT)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// The type of programme: Bachelor, Master, or Doctoral.
    /// </summary>
    public string? ProgrammeType { get; set; }

    /// <summary>
    /// The degree title awarded upon completion.
    /// Example: "Bc." (Bachelor), "Ing." (Engineer/Master), "Ph.D." (Doctorate)
    /// </summary>
    public string? DegreeTitle { get; set; }

    /// <summary>
    /// The duration of the programme.
    /// Example: "3 years", "2 years"
    /// </summary>
    public string? Duration { get; set; }

    /// <summary>
    /// The type of study offered.
    /// Example: "full-time study", "combined study", "part-time study"
    /// </summary>
    public string? StudyType { get; set; }

    /// <summary>
    /// The valid period for this programme.
    /// Example: "2019-2029" indicates the programme is valid from 2019 to 2029
    /// </summary>
    public string? ValidPeriod { get; set; }

    /// <summary>
    /// Indicates whether the programme is taught in English.
    /// </summary>
    public bool IsEnglish { get; set; }

    /// <summary>
    /// The URL link to the programme details page.
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// An array of specializations or tracks available within the programme.
    /// </summary>
    public required string[] Specializations { get; init; }

    /// <summary>
    /// Returns a formatted string representation of the study programme.
    /// </summary>
    public override string ToString()
    {
        return $"{Name} ({Code}) - {Duration} - {DegreeTitle} - {StudyType}{(IsEnglish ? " (English)" : "")}";
    }
}