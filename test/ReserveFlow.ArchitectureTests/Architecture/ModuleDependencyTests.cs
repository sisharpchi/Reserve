using System.Xml.Linq;

namespace ReserveFlow.ArchitectureTests.Architecture;

public static class ModuleDependencyTests
{
    private static readonly string[] ForbiddenInnerLayerProjectReferences =
    [
        ".Application",
        ".Infrastructure",
        ".Presentation",
        ".IntegrationEvents"
    ];

    private static readonly string[] ForbiddenOuterLayerProjectReferences =
    [
        ".Infrastructure",
        ".Presentation"
    ];

    private static readonly string[] ForbiddenDomainPackageReferences =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Npgsql"
    ];

    private static readonly string[] ForbiddenDomainSourceTokens =
    [
        "using Microsoft.AspNetCore",
        "using Microsoft.EntityFrameworkCore",
        "using Npgsql",
        ": DbContext",
        "IEndpointRouteBuilder"
    ];

    [Fact]
    public static void DomainProjectsDoNotReferenceApplicationInfrastructureOrPresentation()
    {
        ProjectFile[] projects = FindProjects("*.Domain.csproj");

        List<string> failures = [];

        foreach (ProjectFile project in projects)
        {
            failures.AddRange(FindForbiddenProjectReferences(project, ForbiddenInnerLayerProjectReferences));
            failures.AddRange(FindForbiddenPackageReferences(project, ForbiddenDomainPackageReferences));
        }

        AssertNoFailures(failures);
    }

    [Fact]
    public static void ApplicationProjectsDoNotReferenceInfrastructureOrPresentation()
    {
        ProjectFile[] projects = FindProjects("*.Application.csproj");

        List<string> failures = [];

        foreach (ProjectFile project in projects)
        {
            failures.AddRange(FindForbiddenProjectReferences(project, ForbiddenOuterLayerProjectReferences));
        }

        AssertNoFailures(failures);
    }

    [Fact]
    public static void PresentationProjectsDoNotReferenceInfrastructure()
    {
        ProjectFile[] projects = FindProjects("*.Presentation.csproj");

        List<string> failures = [];

        foreach (ProjectFile project in projects)
        {
            failures.AddRange(FindForbiddenProjectReferences(project, [".Infrastructure"]));
        }

        AssertNoFailures(failures);
    }

    [Fact]
    public static void DomainSourceDoesNotUseInfrastructureFrameworks()
    {
        ProjectFile[] projects = FindProjects("*.Domain.csproj");

        List<string> failures = [];

        foreach (ProjectFile project in projects)
        {
            foreach (string sourceFile in EnumerateSourceFiles(project.DirectoryPath))
            {
                string source = File.ReadAllText(sourceFile);

                foreach (string forbiddenToken in ForbiddenDomainSourceTokens)
                {
                    if (source.Contains(forbiddenToken, StringComparison.Ordinal))
                    {
                        failures.Add($"{RelativePath(sourceFile)} contains forbidden domain token '{forbiddenToken}'.");
                    }
                }
            }
        }

        AssertNoFailures(failures);
    }

    private static ProjectFile[] FindProjects(string searchPattern)
    {
        string sourceRoot = Path.Combine(RepositoryRoot.Value, "src");

        return Directory
            .EnumerateFiles(sourceRoot, searchPattern, SearchOption.AllDirectories)
            .Select(static path => new ProjectFile(path))
            .OrderBy(static project => project.RelativePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<string> FindForbiddenProjectReferences(
        ProjectFile project,
        IReadOnlyCollection<string> forbiddenReferences)
    {
        foreach (string reference in project.ProjectReferences)
        {
            foreach (string forbiddenReference in forbiddenReferences)
            {
                if (reference.Contains(forbiddenReference, StringComparison.OrdinalIgnoreCase))
                {
                    yield return $"{project.RelativePath} references forbidden project '{reference}'.";
                }
            }
        }
    }

    private static IEnumerable<string> FindForbiddenPackageReferences(
        ProjectFile project,
        IReadOnlyCollection<string> forbiddenPackages)
    {
        foreach (string package in project.PackageReferences)
        {
            foreach (string forbiddenPackage in forbiddenPackages)
            {
                if (package.Contains(forbiddenPackage, StringComparison.OrdinalIgnoreCase))
                {
                    yield return $"{project.RelativePath} references forbidden package '{package}'.";
                }
            }
        }
    }

    private static IEnumerable<string> EnumerateSourceFiles(string projectDirectory)
    {
        return Directory
            .EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(static path =>
                !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) &&
                !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }

    private static void AssertNoFailures(IReadOnlyCollection<string> failures)
    {
        Assert.True(
            failures.Count == 0,
            $"Architecture boundary violations:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    private static string RelativePath(string path)
    {
        return Path.GetRelativePath(RepositoryRoot.Value, path).Replace(Path.DirectorySeparatorChar, '/');
    }

    private static readonly Lazy<string> RepositoryRoot = new(static () =>
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ReserveFlow.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate ReserveFlow.sln from the test output directory.");
    });

    private sealed class ProjectFile
    {
        private readonly XDocument _document;

        public ProjectFile(string path)
        {
            Path = path;
            DirectoryPath = System.IO.Path.GetDirectoryName(path) ??
                throw new InvalidOperationException($"Project path has no directory: {path}");
            RelativePath = ModuleDependencyTests.RelativePath(path);
            _document = XDocument.Load(path);
        }

        public string Path { get; }

        public string DirectoryPath { get; }

        public string RelativePath { get; }

        public string[] ProjectReferences => ReadIncludes("ProjectReference");

        public string[] PackageReferences => ReadIncludes("PackageReference");

        private string[] ReadIncludes(string elementName)
        {
            return _document
                .Descendants(elementName)
                .Select(static element => element.Attribute("Include")?.Value)
                .OfType<string>()
                .ToArray();
        }
    }
}
