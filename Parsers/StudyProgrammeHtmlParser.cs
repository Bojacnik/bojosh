using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using bojosh.Models;

namespace bojosh.Parsers;

/// <summary>
/// Parser for extracting study programme information from VUT FIT HTML pages.
/// Identifies and parses bachelor, master, and doctoral programmes with their metadata.
/// </summary>
public static class StudyProgrammeHtmlParser
{
    /// <summary>
    /// Parses HTML content containing study programme information and extracts all programmes.
    /// </summary>
    /// <param name="studyProgrammeHtml">The raw HTML content from the VUT FIT study programmes page.</param>
    /// <returns>A list of parsed StudyProgramme objects extracted from the HTML.</returns>
    public static List<StudyProgramme> Parse(string studyProgrammeHtml)
    {
        List<StudyProgramme> programmes = [];
        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(studyProgrammeHtml);

        // Define programme types and their corresponding container IDs on the HTML page
        // Each programme type (Bachelor, Master, Doctoral) has its own section
        Dictionary<string, string> programmeTypeMap = new()
        {
            { "bc", "Bachelor" },
            { "mgr", "Master" },
            { "dr", "Doctoral" }
        };

        // Iterate through each programme type and parse its programmes
        foreach ((string containerId, string programmeType) in programmeTypeMap)
        {
            // Find the container element for this programme type
            HtmlNode? container = htmlDoc.GetElementbyId(containerId);

            // Find all programme items within the container (typically list items)
            HtmlNodeCollection? items = container?.SelectNodes(".//li[@class='c-programmes__item']");
            if (items == null)
                continue;

            // Process each individual programme item
            foreach (HtmlNode? item in items)
            {
                try
                {
                    StudyProgramme? programme = ParseProgrammeItem(item, programmeType);
                    if (programme != null)
                        programmes.Add(programme);
                }
                catch (Exception ex)
                {
                    // Log parsing errors but continue processing other items
                    Console.Error.WriteLine($"Error parsing programme item: {ex.Message}");
                }
            }
        }

        return programmes;
    }

    /// <summary>
    /// Parses a single programme item HTML element to extract all relevant data.
    /// </summary>
    /// <param name="item">The HTML node representing a single programme.</param>
    /// <param name="programmeType">The type of programme (Bachelor, Master, Doctoral).</param>
    /// <returns>A StudyProgramme object with parsed data, or null if parsing fails.</returns>
    private static StudyProgramme? ParseProgrammeItem(HtmlNode item, string programmeType)
    {
        StudyProgramme programme = new()
        {
            ProgrammeType = programmeType,
            Specializations = [],
        };

        // Extract the title element containing programme name and tags
        HtmlNode? titleElement = item.SelectSingleNode(".//h3[@class='b-programme__title']");
        if (titleElement == null)
            return null;

        // Extract programme name from the link
        HtmlNode? nameLink = titleElement.SelectSingleNode(".//a[@class='b-programme__link color-secondary']");
        if (nameLink != null)
        {
            programme.Name = nameLink.InnerText.Trim();
            programme.Url = nameLink.GetAttributeValue("href", "");
        }

        // Extract tags (code, valid period, language information)
        HtmlNodeCollection? tags = titleElement.SelectNodes(".//span[@class]");
        if (tags != null)
        {
            foreach (HtmlNode? tag in tags)
            {
                string? classes = tag.GetAttributeValue("class", "");
                string text = tag.InnerText.Trim();

                // Identify tag type by CSS class and extract corresponding data
                if (classes.Contains("tag--fit"))
                {
                    // Programme code (e.g., "BIT", "MIT")
                    programme.Code = text;
                }
                else if (classes.Contains("tag--outline") && text != "Programme")
                {
                    // Valid period (e.g., "2019-2029")
                    programme.ValidPeriod = text;
                }
                else if (text == "English" && classes.Contains("tag"))
                {
                    // English language flag
                    programme.IsEnglish = true;
                }
            }
        }

        // Extract metadata: duration, degree title, and study type
        HtmlNode? metaElement = item.SelectSingleNode(".//p[@class='b-branch__meta']");
        if (metaElement == null) return programme;
        
        HtmlNodeCollection? metaItems = metaElement.SelectNodes(".//span[@class='b-branch__meta-item']");
        
        if (metaItems == null || metaItems.Count < 3) return programme;
        
        // Extract duration (e.g., "3 years")
        programme.Duration = metaItems[0].InnerText.Trim();
                
        // Extract degree title (e.g., "Bc.", "Ing.", "Ph.D.")
        string titleText = metaItems[1].InnerText.Trim();
        if (titleText.StartsWith("title "))
        {
            programme.DegreeTitle = titleText.Substring(6);
        }

        // Extract study type (e.g., "full-time study", "part-time study")
        programme.StudyType = metaItems[2].InnerText.Trim();

        return programme;
    }
}