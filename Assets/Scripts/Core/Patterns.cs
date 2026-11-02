using System.Collections.Generic;

namespace GauntletRun.Core
{
    /// <summary>
    /// A hand-made stretch of road. Rows run from near to far, three lanes each:
    /// <c>.</c> empty, <c>H</c> hurdle, <c>h</c> hurdle with coins over it, <c>B</c> beam,
    /// <c>b</c> beam with coins under it, <c>W</c> wall, <c>A</c> axe, <c>c</c> coin, <c>g</c> gem,
    /// <c>*</c> a random power-up.
    /// </summary>
    public sealed class Pattern
    {
        public Pattern(string name, int minTier, float spacing, string rows)
        {
            Name = name;
            MinTier = minTier;
            Spacing = spacing;
            Rows = rows.Split('|');
        }

        public string Name { get; }
        public int MinTier { get; }
        /// <summary>Metres between rows at the starting speed.</summary>
        public float Spacing { get; }
        public string[] Rows { get; }

        public static bool IsBlocking(char cell) => cell == 'W' || cell == 'A';

        /// <summary>Why the pattern is unfair or malformed, or null when it is fine.</summary>
        public string Problem()
        {
            foreach (string row in Rows)
            {
                if (row.Length != Tuning.LaneCount)
                {
                    return $"row '{row}' does not have {Tuning.LaneCount} lanes";
                }
                bool open = false;
                foreach (char cell in row)
                {
                    if (".HhBbWAcg*".IndexOf(cell) < 0)
                    {
                        return $"unknown cell '{cell}'";
                    }
                    open |= !IsBlocking(cell);
                }
                if (!open)
                {
                    return $"row '{row}' blocks every lane";
                }
            }
            return null;
        }
    }

    /// <summary>The pattern library, from the gentle opening pieces to the late gauntlets.</summary>
    public static class Patterns
    {
        public static readonly IReadOnlyList<Pattern> All = new[]
        {
            // Tier 0: one idea at a time.
            new Pattern("coin trail", 0, 2.5f, "c..|c..|c..|c..|c..|c.."),
            new Pattern("first hurdle", 0, 3f, ".h.|...|...|c..|c..|c.."),
            new Pattern("first beam", 0, 3f, "..b|...|...|.c.|.c.|.c."),
            new Pattern("side wall", 0, 3f, "W..|W..|...|..c|..c|..c"),
            new Pattern("hurdle line", 0, 3f, "c.c|...|HHH|...|.c."),
            new Pattern("gem detour", 0, 3f, "W.W|...|.g.|...|c.c"),
            // Tier 1: two moves in a row.
            new Pattern("switchback", 1, 3f, "WW.|...|..c|...|.WW|...|c.."),
            new Pattern("duck and jump", 1, 3f, "BBB|...|...|...|hhh"),
            new Pattern("middle run", 1, 3f, "W.W|.c.|.c.|.c.|W.W|...|HHH"),
            new Pattern("first axe", 1, 3f, "A..|...|..A|...|c.c"),
            new Pattern("power up", 1, 3f, "...|.*.|...|c.c|c.c"),
            // Tier 2: mixed rows.
            new Pattern("crossfire", 2, 3f, "W.H|...|...|B.W|...|...|W.H"),
            new Pattern("axe hall", 2, 3f, "A.A|...|...|.W.|...|...|A.A"),
            new Pattern("choose", 2, 3f, "HBH|...|...|BHB|...|c.c"),
            // Tier 3: zigzags.
            new Pattern("zigzag", 3, 3.5f, "WW.|...|.WW|...|WW.|...|.g."),
            new Pattern("gauntlet", 3, 3f, "B.B|.H.|...|W.W|.B.|...|*.."),
            // Tier 4: everything at once.
            new Pattern("blades", 4, 3f, "A.W|...|...|W.A|...|...|.A.|...|h.h"),
            new Pattern("last stand", 4, 3f, "WH.|...|.BW|...|HW.|...|.c."),
        };
    }
}
