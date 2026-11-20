namespace bojosh.Models;

/// <summary>
/// Represents a study class (course) offered by VUT FIT.
/// Contains information about the class code, name, type, schedule, and instructor details.
/// </summary>
public class StudyClass
{
    /// <summary>
    /// The name of the study class/course.
    /// Example: "Introduction to Programming", "Data Structures"
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The unique web identifier for the study class.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The code identifying the class.
    /// Example: "BIT001", "MIT302"
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// The type of class/course.
    /// Example: "Lecture", "Practical", "Lab", "Seminar"
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// The semester when the class is offered.
    /// Example: "Winter", "Summer", "1st", "2nd"
    /// </summary>
    public string? Semester { get; set; }

    /// <summary>
    /// The number of credits awarded for completing this class.
    /// Example: "3", "5", "6"
    /// </summary>
    public string? Credits { get; set; }

    /// <summary>
    /// The name or identifier of the class instructor/lecturer.
    /// </summary>
    public string? Lecturer { get; set; }

    /// <summary>
    /// The URL link to the class details page.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Returns a formatted string representation of the study class.
    /// </summary>
    public override string ToString()
    {
        return $"{Name} ({Code}) - {Type} - {Semester} - {Credits}";
    }
}