using System.Runtime.CompilerServices;
using System.Text;

namespace ApiUsageAnalyzer.Tests;

public class GetMissingMembersTests : BaseUnitTest
{
    [Fact]
    public void ClassLibraryV1IsCompatibleWithClassLibraryV1()
    {
        var analyzer = new ApiAnalyzer();

        var result = analyzer.GetMissingMembers(
            new()
            {
                FileName = LibraryBuiltAgainstV1,
                SearchPaths = [Path.GetDirectoryName(LibraryBuiltAgainstV1)!]
            },
            new()
            {
                FileName = ClassLibraryV1,
                SearchPaths = [Path.GetDirectoryName(ClassLibraryV1)!]
            });

        AssertMatchesExpectedResults(result);
    }

    [Fact]
    public void ClassLibraryV2IsPartiallyCompatibleWithClassLibraryV1()
    {
        var analyzer = new ApiAnalyzer();

        var result = analyzer.GetMissingMembers(
            new()
            {
                FileName = LibraryBuiltAgainstV1,
                SearchPaths = [Path.GetDirectoryName(LibraryBuiltAgainstV1)!]
            },
            new()
            {
                FileName = ClassLibraryV2,
                SearchPaths = [Path.GetDirectoryName(ClassLibraryV2)!]
            });

        AssertMatchesExpectedResults(result);
    }

    [Fact]
    public void SvgSkiaIsCompatibleWithSkiaSharpV2()
    {
        var v2 = Path.GetDirectoryName(SkiaSharpV2Downloader)!;

        var analyzer = new ApiAnalyzer();

        var result = analyzer.GetMissingMembers(
            new()
            {
                FileName = Path.Combine(v2, "Svg.Skia.dll"),
                SearchPaths = [v2]
            },
            new()
            {
                FileName = Path.Combine(v2, "SkiaSharp.dll"),
                SearchPaths = [v2]
            });

        AssertMatchesExpectedResults(result);
    }

    [Fact]
    public void SvgSkiaIsPartiallyCompatibleWithSkiaSharpV3()
    {
        var v2 = Path.GetDirectoryName(SkiaSharpV2Downloader)!;
        var v3 = Path.GetDirectoryName(SkiaSharpV3Downloader)!;

        var analyzer = new ApiAnalyzer();

        var result = analyzer.GetMissingMembers(
            new()
            {
                FileName = Path.Combine(v2, "Svg.Skia.dll"),
                SearchPaths = [v2]
            },
            new()
            {
                FileName = Path.Combine(v3, "SkiaSharp.dll"),
                SearchPaths = [v3]
            });

        AssertMatchesExpectedResults(result);
    }

    private static void WriteExpectedResults(MissingSymbols result, [CallerMemberName] string? methodName = null)
    {
        var actual = GetActualContents(result);

        File.WriteAllText($"../../../ExpectedResults/{methodName}.txt", actual);
    }

    private static void AssertMatchesExpectedResults(MissingSymbols result, [CallerMemberName] string? methodName = null)
    {
        var actual = GetActualContents(result);

        var expected = File.ReadAllText($"ExpectedResults/{methodName}.txt");

        Assert.Equal(expected, actual);
    }

    private static string GetActualContents(MissingSymbols result)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Mising Types:");
        if (result.Types.Count == 0)
        {
            sb.AppendLine("/*None*/");
        }
        else
        {
            foreach (var type in result.Types)
            {
                sb.AppendLine(type);
            }
        }

        sb.AppendLine("");
        sb.AppendLine("Missing Members:");
        if (result.Members.Count == 0)
        {
            sb.AppendLine("/*None*/");
        }
        else
        {
            foreach (var member in result.Members)
            {
                sb.AppendLine(member);
            }
        }

        var actual = sb.ToString();

        return actual;
    }
}
