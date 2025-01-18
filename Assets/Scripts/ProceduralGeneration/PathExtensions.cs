using NeonLadder.ProceduralGeneration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.ProceduralGeneration
{
    public static class PathSerialization
    {
        public static string ToIndentedText(Dictionary<string, Path> paths, Dictionary<string, string> bossLocations)
        {
            var text = new StringBuilder();

            text.AppendLine("City Entrance");
            foreach (var path in paths)
            {
                text.AppendLine("====================");
                text.AppendLine($"- {path.Key.ToUpper()}");
                foreach (var node in path.Value.Nodes)
                {
                    text.AppendLine($"  - {node.GameObjectName}");
                    var description = $"    - {node.Encounter.Description}";
                    if (node.Encounter.Description.Contains("final boss") && bossLocations.ContainsKey(path.Key))
                    {
                        description += $" - Location: {bossLocations[path.Key]}";
                    }
                    text.AppendLine(description);
                }
                text.AppendLine(); // Add empty line after each path
            }

            return text.ToString();
        }

        public static string ToJson(Dictionary<string, Path> paths)
        {
            return JsonConvert.SerializeObject(new
            {
                legend = new
                {
                    arrows = "-> / <- (Optional rooms you can enter within the scene)",
                    rooms = "() = (room, e.g., Shop or Rest)",
                    encounters = new
                    {
                        ME = "Major Enemy (1-3 larger, more powerful foes)",
                        mE = "Minor Enemy (1-5 smaller, less dangerous foes)",
                        Event = new
                        {
                            TreasureChest = "Treasure chest containing loot",
                            MysteriousAltar = "A mysterious altar offering a choice",
                            Riddle = "Statue challenges you with a riddle"
                        }
                    }
                },
                map = new
                {
                    paths = paths,
                    start = new
                    {
                        location = "City Entrance",
                        staging = true,
                        scene = "transition",
                        gameObject = "city-transition-1"
                    },
                    backtracking = true
                }
            }, Formatting.Indented);
        }
    }
}
