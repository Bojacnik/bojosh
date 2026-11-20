using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using bojosh.Constants;
using bojosh.Models;
using bojosh.Options;
using bojosh.Parsers;

namespace bojosh.Services;

/// <summary>
/// Service class for fetching and parsing study data from VUT FIT website.
/// Handles HTTP requests and coordinates HTML parsing for programmes and classes.
/// </summary>
public record StudyDataService
{
    /// <summary>
    /// HTTP client instance used for making requests to VUT FIT website.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the StudyDataService class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for web requests.</param>
    public StudyDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Fetches and returns a list of study programmes from VUT FIT.
    /// </summary>
    /// <param name="options">Optional filtering options (e.g., by year). Pass null for no filtering.</param>
    /// <returns>A list of StudyProgramme objects.</returns>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    public List<StudyProgramme> GetStudyProgrammes(GetStudyProgrammesOpts? options)
    {
        List<StudyProgramme> programmes;

        // Build the request URL with optional year parameter
        string url = options switch
        {
            null => VutFitStaticUris.StudyProgrammesUri,
            _ => VutFitStaticUris.StudyProgrammesUri 
                 + "." + Enum.GetName(options.Language) +
                 "?year=" + options.Year +
                 '&' + "type=" + Enum.GetName(options.Type),
        };

        // Make the HTTP request (blocking operation)
        try
        {
            Task<HttpResponseMessage> response = _httpClient.GetAsync(url);
            HttpResponseMessage responseResult = response.Result;

            try
            {
                // Ensure the response is successful
                responseResult.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                // Log and re-throw the exception
                Console.Error.WriteLine(
                    $"Unable to get study programmes: {responseResult.StatusCode}\n{responseResult}"
                );
                throw;
            }

            // Read and parse the response content
            string responseContent = responseResult.Content.ReadAsStringAsync().Result;
            programmes = StudyProgrammeHtmlParser.Parse(responseContent);
        }
        catch (HttpRequestException requestException)
        {
            // Log the exception
            Console.Error.WriteLine(requestException.Message);
            throw;
        }

        return programmes;
    }

    /// <summary>
    /// Fetches and returns a list of study classes from VUT FIT.
    /// </summary>
    /// <returns>A list of StudyClass objects.</returns>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    public List<StudyClass> GetStudyClasses()
    {
        List<StudyClass> classes;

        // Make the HTTP request (blocking operation)
        try
        {
            Task<HttpResponseMessage> response = _httpClient.GetAsync(VutFitStaticUris.StudyClassesUri);
            HttpResponseMessage responseResult = response.Result;

            try
            {
                // Ensure the response is successful
                responseResult.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException requestException)
            {
                Console.Error.WriteLine(requestException.Message);
                throw;
            }

            // Read and parse the response content
            string responseContent = responseResult.Content.ReadAsStringAsync().Result;
            classes = StudyClassesHtmlParser.Parse(responseContent);
        }
        catch (HttpRequestException requestException)
        {
            // Log the exception
            Console.Error.WriteLine(requestException.Message);
            throw;
        }

        return classes;
    }
}

