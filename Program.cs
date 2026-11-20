using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using bojosh.Models;
using bojosh.Options;
using bojosh.Services;

using Microsoft.Extensions.DependencyInjection;

namespace bojosh;

/// <summary>
/// Main entry point for the Bojosh application.
/// This is an ASP.NET Core web application that provides endpoints to retrieve
/// and display study programmes and classes from VUT FIT (Brno University of Technology).
/// </summary>
public static class Program
{
    /// <summary>
    /// Main method - configures and runs the web application.
    /// Sets up HTTP endpoints and starts the Kestrel server.
    /// </summary>
    public static void Main()
    {
        // Create a web application builder with default configuration
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        // Register the StudyDataService as a scoped service for dependency injection.
        // This allows the service lifetime to be managed by the dependency injection container
        builder.Services.AddHttpClient<StudyDataService>();

        // Build the web application
        WebApplication app = builder.Build();

        // Enable serving static files from the wwwroot directory and current directory
        app.UseStaticFiles();

        /// <summary>
        /// Maps GET / endpoint - redirects to the scheduler UI.
        /// </summary>
        app.MapGet("/", () =>
        {
            StudyDataService studyDataService = app.Services.GetRequiredService<StudyDataService>();
            
            // Create options for retrieving programmes for the current year
            GetStudyProgrammesOpts options = new(DateTime.Now.Year, ProgrammeType.all, Language.cs);

            // Build the response text using StringBuilder for efficient string concatenation
            StringBuilder sb = new();

            // Fetch and add study programmes to the response
            try
            {
                foreach (StudyProgramme programme in studyDataService.GetStudyProgrammes(options))
                {
                    sb.AppendLine(programme.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching study programmes: {ex.Message}");
            }

            // Add a blank line separator between programmes and classes
            sb.AppendLine();

            // Fetch and add study classes to the response
            try
            {
                foreach (StudyClass studyClass in studyDataService.GetStudyClasses())
                {
                    sb.AppendLine(studyClass.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching study classes: {ex.Message}");
            }

            return sb.ToString();
        });

        // Start the application and listen for HTTP requests on the configured endpoints
        app.Run();
    }
}