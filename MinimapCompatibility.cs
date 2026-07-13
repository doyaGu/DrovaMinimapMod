namespace DrovaMinimapMod
{
    /// <summary>
    /// The Drova UI identifiers verified against the supported game build.
    /// Keep game-specific naming in this profile so updates are isolated and reviewable.
    /// </summary>
    internal static class MinimapCompatibility
    {
        private const string MapDefinitionPrefix = "MapDefinition_";

        internal const string MainWorldDefinitionName = "MapDefinition_World";
        internal const string DetailedMainWorldDefinitionName = "MapDefinition_World_Detailed";

        internal static bool IsMainWorldDefinition(string definitionName)
        {
            return string.Equals(definitionName, MainWorldDefinitionName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definitionName, DetailedMainWorldDefinitionName, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsDetailedMainWorldDefinition(string definitionName)
        {
            return string.Equals(definitionName, DetailedMainWorldDefinitionName, StringComparison.OrdinalIgnoreCase);
        }

        internal static string GetMapContainerName(string definitionName)
        {
            return definitionName.StartsWith(MapDefinitionPrefix, StringComparison.Ordinal)
                ? "Map_" + definitionName[MapDefinitionPrefix.Length..]
                : definitionName;
        }
    }
}
