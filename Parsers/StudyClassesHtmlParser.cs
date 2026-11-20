using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using bojosh.Models;

namespace bojosh.Parsers;

/// <summary>
/// Parser for extracting study class (course) information from VUT FIT HTML pages.
/// Supports parsing from both table-based and list-based HTML layouts.
/// </summary>
public static class StudyClassesHtmlParser
{
    /// <summary>
    /// Parses HTML content containing study class information and extracts all classes.
    /// Attempts to parse from tables first, then falls back to list items if no tables found.
    /// </summary>
    /// <param name="studyClassHtml">The raw HTML content from the VUT FIT study classes page.</param>
    /// <returns>A list of parsed StudyClass objects extracted from the HTML.</returns>
    public static List<StudyClass> Parse(string studyClassHtml)
    {
        List<StudyClass> classes = [];
        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(studyClassHtml);

        try
        {
            // Try to find class items in tables (common format for structured data)
            HtmlNodeCollection? tableRows = htmlDoc.DocumentNode.SelectNodes(".//table//tr[position()>1]");
            if (tableRows is { Count: > 0 })
            {
                foreach (HtmlNode? row in tableRows)
                {
                    try
                    {
                        StudyClass? studyClass = ParseClassFromTableRow(row);
                        if (studyClass != null)
                            classes.Add(studyClass);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error parsing class row: {ex.Message}");
                    }
                }
            }
            
            // If no table rows found, try to find list items (alternative layout)
            if (classes.Count == 0)
            {
                HtmlNodeCollection? listItems = htmlDoc.DocumentNode.SelectNodes(".//li[contains(@class, 'class') or contains(@class, 'course')]");
                if (listItems is { Count: > 0 })
                {
                    foreach (HtmlNode? item in listItems)
                    {
                        try
                        {
                            StudyClass? studyClass = ParseClassFromListItem(item);
                            if (studyClass != null)
                                classes.Add(studyClass);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Error parsing class item: {ex.Message}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing study classes HTML: {ex.Message}");
        }

        return classes;
    }

    /// <summary>
    /// Parses a single table row to extract class information.
    /// Expected column order: Code, Name, Type, Semester, Credits, Instructor
    /// </summary>
    /// <param name="row">The HTML table row node containing class data.</param>
    /// <returns>A StudyClass object with parsed data, or null if parsing fails.</returns>
    private static StudyClass? ParseClassFromTableRow(HtmlNode row)
    {
        // Extract all cells (table data elements) from the row
        HtmlNodeCollection? cells = row.SelectNodes(".//td");
        if (cells == null || cells.Count < 2)
            return null;

        // Create a new StudyClass with the extracted data
        StudyClass studyClass = new()
        {
            Code = cells[0].InnerText.Trim(),
            Name = cells[1].InnerText.Trim()
        };

        // Extract optional fields if available
        if (cells.Count > 2)
            studyClass.Type = cells[2].InnerText.Trim();

        if (cells.Count > 3)
            studyClass.Semester = cells[3].InnerText.Trim();

        if (cells.Count > 4)
            studyClass.Credits = cells[4].InnerText.Trim();

        if (cells.Count > 5)
            studyClass.Lecturer = cells[5].InnerText.Trim();

        // Try to extract URL from a link element if present
        HtmlNode? link = row.SelectSingleNode(".//a");
        if (link != null)
        {
            studyClass.Url = link.GetAttributeValue("href", "");
        }

        return studyClass;
    }

    /// <summary>
    /// Parses a single list item to extract class information.
    /// Looks for name links and span elements containing metadata.
    /// </summary>
    /// <param name="item">The HTML list item node containing class data.</param>
    /// <returns>A StudyClass object with parsed data, or null if no name is found.</returns>
    private static StudyClass? ParseClassFromListItem(HtmlNode item)
    {
        StudyClass studyClass = new();

        // Try to find the class name in a link element
        HtmlNode? nameLink = item.SelectSingleNode(".//a");
        if (nameLink != null)
        {
            studyClass.Name = nameLink.InnerText.Trim();
            studyClass.Url = nameLink.GetAttributeValue("href", "");
        }

        // Try to find additional metadata in span elements
        HtmlNodeCollection? spans = item.SelectNodes(".//span");
        if (spans is { Count: > 0 })
        {
            // First span typically contains the class code
            studyClass.Code = spans[0].InnerText.Trim();
            
            // Additional spans may contain type, semester, and other info
            if (spans.Count > 1)
                studyClass.Type = spans[1].InnerText.Trim();
            
            if (spans.Count > 2)
                studyClass.Semester = spans[2].InnerText.Trim();
        }

        // Only return the class if a name was found
        return !string.IsNullOrEmpty(studyClass.Name) ? studyClass : null;
    }
}

