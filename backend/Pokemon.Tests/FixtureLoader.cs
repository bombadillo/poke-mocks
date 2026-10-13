namespace Pokemon.Tests;

internal static class FixtureLoader
{
    public static string Read(string scenario, string layer = "backend")
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "testfixtures",
            scenario,
            layer,
            "test-data.json");
        return File.ReadAllText(path);
    }
}
