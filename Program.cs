using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace bojosh;

internal static class VutFitStaticUris
{
    public const string StudyProgrammesUri = "https://www.fit.vut.cz/study/programs/";
    public const string StudyClassesUri = "https://www.fit.vut.cz/study/";
}

public class StudyProgramme
{
}

public class StudyClass
{
}

public record GetStudyProgrammesOpts(int Year);

public static class StudyProgrammeHtmlParser
{
    public static List<StudyProgramme> Parse(string studyProgrammeHtml)
    {
        throw new NotImplementedException();
    }
}

public static class StudyClassesHtmlParser
{
    public static List<StudyClass> Parse(string studyClassHtml)
    {
        throw new NotImplementedException();
    }
}

public static class Program
{
    private static List<StudyProgramme> GetStudyProgrammes(GetStudyProgrammesOpts? options)
    {
        List<StudyProgramme> programmes;

        HttpClient client = new HttpClient();

        Task<HttpResponseMessage> response = options switch
        {
            // if we have options
            null => client.GetAsync(VutFitStaticUris.StudyProgrammesUri),
            _ => client.GetAsync(VutFitStaticUris.StudyProgrammesUri + "?year=" + options.Year)
        };

        // careful blocking
        try
        {
            HttpResponseMessage responseResult = response.Result;

            try
            {
                responseResult.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                // log exception
                Console.Error.WriteLine(
                    $"Unable to get study programmes: {responseResult.StatusCode}\n{responseResult}"
                );

                // rethrow
                throw;
            }

            // parse programmes
            // TODO: handle this better
            string responseStringified = responseResult.Content.ReadAsStringAsync().Result;

            programmes = StudyProgrammeHtmlParser.Parse(
                responseStringified
            );
        }
        catch (HttpRequestException requestException)
        {
            // log exception
            Console.Error.WriteLine(requestException.Message);

            // rethrow
            throw;
        }

        return programmes;
    }

    private static List<StudyClass> GetStudyClasses()
    {
        List<StudyClass> classes;

        HttpClient client = new HttpClient();

        Task<HttpResponseMessage> response = client.GetAsync(VutFitStaticUris.StudyClassesUri);

        try
        {
            HttpResponseMessage responseResult = response.Result;

            try
            {
                responseResult.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException requestException)
            {
                Console.Error.WriteLine(requestException.Message);
                throw;
            }

            // TODO: handle this better
            classes = StudyClassesHtmlParser.Parse(
                responseResult.Content.ReadAsStringAsync().Result
            );
        }
        catch (HttpRequestException requestException)
        {
            Console.Error.WriteLine(requestException.Message);
            throw;
        }

        return classes;
    }

    public static void Main()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        WebApplication app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.MapGet("/bojosh", string () =>
        {
            GetStudyProgrammesOpts options = new GetStudyProgrammesOpts(DateTime.Now.Year);

            StringBuilder sb = new StringBuilder();

            foreach (StudyProgramme programme in GetStudyProgrammes(options))
            {
                sb.AppendLine(programme.ToString());
            }

            sb.AppendLine();

            foreach (StudyClass studyClass in GetStudyClasses())
            {
                sb.AppendLine(studyClass.ToString());
            }

            return sb.ToString();
        });

        app.Run();
    }
}