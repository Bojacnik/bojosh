namespace bojosh.Constants;

/// <summary>
/// Static URIs for VUT FIT (Brno University of Technology - Faculty of Information Technology) resources.
/// These are the base URLs used for scraping study programmes and classes data.
/// </summary>
internal static class VutFitStaticUris
{
    /// <summary>
    /// URI for fetching study programmes information from VUT FIT website.
    /// </summary>
    public const string StudyProgrammesUri = "https://www.fit.vut.cz/study/programs/";

    /// <summary>
    /// URI for fetching study classes and related study information from VUT FIT website.
    /// </summary>
    public const string StudyClassesUri = "https://www.fit.vut.cz/study/";
}

