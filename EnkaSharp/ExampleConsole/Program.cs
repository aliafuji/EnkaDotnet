using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using EnkaSharp;
using EnkaSharp.Components.Genshin;
using EnkaSharp.Exceptions;
using EnkaSharp.Enums;
using EnkaSharp.Enums.Genshin;
using System.Text.Json; // Added for JSON export

namespace EnkaSharp.ExampleConsole
{
    public class Program
    {
        private static EnkaClient? _client;
        private static bool _showDetailedStats = true;
        private static string _currentLanguage = "en";
        private static GameType _gameType = GameType.Genshin;
        private static string _assetsPath = "enka_assets";
        private static string _userAgent = "EnkaSharp-Example/1.0";
        private static DisplayMode _currentDisplayMode = DisplayMode.Normal;
        private static List<Character> _lastFetchedCharacters = new List<Character>();
        private static PlayerInfo? _lastFetchedPlayerInfo;
        private static int _lastFetchedUid; // Added to store the last fetched UID

        private enum DisplayMode
        {
            Normal,
            Compact,
            Detailed,
            ArtifactsOnly,
            StatsOnly,
            ExportReady
        }

        public static async Task Main(string[] args)
        {
            DisplayHeader();

            // Parse command line args
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (args[i] == "--lang" || args[i] == "-l")
                    {
                        _currentLanguage = args[i + 1];
                    }
                    else if (args[i] == "--assets" || args[i] == "-a")
                    {
                        _assetsPath = args[i + 1];
                    }
                    else if (args[i] == "--user-agent" || args[i] == "-u")
                    {
                        _userAgent = args[i + 1];
                    }
                    else if (args[i] == "--detailed" || args[i] == "-d")
                    {
                        if (bool.TryParse(args[i + 1], out bool detailed))
                        {
                            _showDetailedStats = detailed;
                        }
                    }
                }
            }

            bool clientInitialized = await InitializeClient();
            if (!clientInitialized)
            {
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
                return;
            }

            await MainMenu();

            // Clean up
            _client?.Dispose();

            Console.WriteLine("\nExiting program. Press any key to close.");
            Console.ReadKey();
        }

        private static void DisplayHeader()
        {
            Console.WriteLine(@"
╔═╗┌┐┌┬┌─┌─┐╔═╗┬ ┬┌─┐┬─┐┌─┐
║╣ │││├┴┐├─┤╚═╗├─┤├─┤├┬┘├─┘
╚═╝┘└┘┴ ┴┴ ┴╚═╝┴ ┴┴ ┴┴└─┴  
    ");
            Console.WriteLine("EnkaSharp Example Console");
            Console.WriteLine("========================");
        }

        private static async Task<bool> InitializeClient()
        {
            try
            {
                Console.WriteLine($"Initializing EnkaClient with:");
                Console.WriteLine($"- Assets Path: {_assetsPath}");
                Console.WriteLine($"- Language: {_currentLanguage}");
                Console.WriteLine($"- Game Type: {_gameType}");
                Console.WriteLine($"- User Agent: {_userAgent}\n");

                _client = new EnkaClient(
                    assetsBasePath: _assetsPath,
                    gameType: _gameType,
                    language: _currentLanguage,
                    customUserAgent: _userAgent
                );

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("EnkaClient initialized successfully!");
                Console.ResetColor();
                return true;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Asset Directory Error: {ex.Message}");
                Console.ResetColor();
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Asset Initialization Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Console.ResetColor();
            }
            catch (UnsupportedGameTypeException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Game Type Error: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unexpected Initialization Error: {ex.Message}");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Console.ResetColor();
            }
            return false;
        }

        private static async Task MainMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Main Menu ===");
                Console.WriteLine("1. Fetch Player Profile (Player + Characters)");
                Console.WriteLine("2. Fetch Player Info Only");
                Console.WriteLine("3. Fetch Character Details Only");
                Console.WriteLine("4. Display Mode: " + _currentDisplayMode);
                Console.WriteLine("5. Character Analysis Options");
                Console.WriteLine("6. Team Analysis");
                Console.WriteLine("7. Export Current Data");
                Console.WriteLine("8. Settings");
                Console.WriteLine("9. Debug Information");
                Console.WriteLine("0. Exit");
                Console.Write("\nEnter your choice (0-9): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await FetchProfile(true);
                        break;
                    case "2":
                        await FetchProfile(false);
                        break;
                    case "3":
                        await FetchCharacters();
                        break;
                    case "4":
                        ChangeDisplayMode();
                        break;
                    case "5":
                        await CharacterAnalysisMenu();
                        break;
                    case "6":
                        TeamAnalysis();
                        break;
                    case "7":
                        await ExportCurrentData();
                        break;
                    case "8":
                        await SettingsMenu();
                        break;
                    case "9":
                        DisplayDebugInfo();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private static async Task SettingsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Settings Menu ===");
                Console.WriteLine("1. Toggle Detailed Stats: " + (_showDetailedStats ? "On" : "Off"));
                Console.WriteLine("2. Change Language (Current: " + _currentLanguage + ")");
                Console.WriteLine("3. Change Assets Path (Current: " + _assetsPath + ")");
                Console.WriteLine("4. Change User Agent (Current: " + _userAgent + ")");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("\nEnter your choice (1-5): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        _showDetailedStats = !_showDetailedStats;
                        Console.WriteLine($"Detailed stats display is now {(_showDetailedStats ? "enabled" : "disabled")}");
                        break;
                    case "2":
                        await ChangeLanguage();
                        break;
                    case "3":
                        await ChangeAssetsPath();
                        break;
                    case "4":
                        ChangeUserAgent();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private static void ChangeUserAgent()
        {
            Console.Write("\nEnter new user agent (leave blank to keep current): ");
            string? newUserAgent = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newUserAgent))
            {
                Console.WriteLine("User agent unchanged.");
                return;
            }

            _userAgent = newUserAgent;
            Console.WriteLine($"User agent changed to: {_userAgent}");

            // This requires reinitialization since the user agent is passed to the client constructor
            Console.WriteLine("Client needs to be reinitialized for this change to take effect.");
            Console.Write("Reinitialize now? (y/n): ");
            string? response = Console.ReadLine()?.ToLower();

            if (response == "y" || response == "yes")
            {
                _client?.Dispose();
                _client = null;
                InitializeClient().GetAwaiter().GetResult(); // Using GetAwaiter().GetResult() in a sync method, consider making async if possible
            }
        }

        private static void ChangeDisplayMode()
        {
            Console.WriteLine("\n=== Change Display Mode ===");
            Console.WriteLine("1. Normal - Standard information display");
            Console.WriteLine("2. Compact - Minimal information display");
            Console.WriteLine("3. Detailed - Maximum information display");
            Console.WriteLine("4. Artifacts Only - Focus on artifacts");
            Console.WriteLine("5. Stats Only - Focus on character stats");
            Console.WriteLine("6. Export Ready - Plain text format suitable for export");
            Console.Write("\nEnter your choice (1-6): ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    _currentDisplayMode = DisplayMode.Normal;
                    break;
                case "2":
                    _currentDisplayMode = DisplayMode.Compact;
                    break;
                case "3":
                    _currentDisplayMode = DisplayMode.Detailed;
                    break;
                case "4":
                    _currentDisplayMode = DisplayMode.ArtifactsOnly;
                    break;
                case "5":
                    _currentDisplayMode = DisplayMode.StatsOnly;
                    break;
                case "6":
                    _currentDisplayMode = DisplayMode.ExportReady;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Display mode unchanged.");
                    return;
            }

            Console.WriteLine($"Display mode changed to: {_currentDisplayMode}");

            // If we have data already, display it with the new mode
            if (_lastFetchedPlayerInfo != null)
            {
                Console.WriteLine("\nRedisplaying last fetched player info with new display mode...");
                DisplayPlayerInfo(_lastFetchedPlayerInfo, _lastFetchedUid); // Pass the stored UID
            }

            if (_lastFetchedCharacters.Count > 0)
            {
                Console.WriteLine("\nRedisplaying last fetched characters with new display mode...");
                DisplayCharacters(_lastFetchedCharacters);
            }
        }

        private static async Task CharacterAnalysisMenu()
        {
            if (_lastFetchedCharacters.Count == 0)
            {
                Console.WriteLine("\nNo character data available. Please fetch character data first.");
                return;
            }

            while (true)
            {
                Console.WriteLine("\n=== Character Analysis Menu ===");
                Console.WriteLine("1. Detailed Character Analysis");
                Console.WriteLine("2. Artifact Build Analysis");
                Console.WriteLine("3. Compare Characters");
                Console.WriteLine("4. Character Ranking by Stats");
                Console.WriteLine("5. Filter Characters");
                Console.WriteLine("6. Back to Main Menu");
                Console.Write("\nEnter your choice (1-6): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DetailedCharacterAnalysis();
                        break;
                    case "2":
                        ArtifactBuildAnalysis();
                        break;
                    case "3":
                        CompareCharacters();
                        break;
                    case "4":
                        RankCharactersByStats();
                        break;
                    case "5":
                        FilterCharacters();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private static void DetailedCharacterAnalysis()
        {
            if (_lastFetchedCharacters.Count == 0)
            {
                Console.WriteLine("No character data available. Please fetch character data first.");
                return;
            }

            Console.WriteLine("\n=== Available Characters ===");
            for (int i = 0; i < _lastFetchedCharacters.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_lastFetchedCharacters[i].Name} (Lv. {_lastFetchedCharacters[i].Level})");
            }

            Console.Write("\nEnter character number to analyze (or 0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int charIndex) || charIndex < 1 || charIndex > _lastFetchedCharacters.Count)
            {
                if (charIndex != 0)
                    Console.WriteLine("Invalid selection.");
                return;
            }

            var character = _lastFetchedCharacters[charIndex - 1];

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n=== Detailed Analysis: {character.Name} ===");
            Console.ResetColor();

            // Basic character info
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Character Information:");
            Console.ResetColor();
            Console.WriteLine($"ID: {character.Id}");
            Console.WriteLine($"Level: {character.Level}/{GetMaxLevel(character.Ascension)} (Ascension {character.Ascension})");
            Console.WriteLine($"Element: {character.Element}");
            Console.WriteLine($"Constellation Level: {character.ConstellationLevel}");
            Console.WriteLine($"Friendship Level: {character.Friendship}");
            if (character.CostumeId > 0)
            {
                Console.WriteLine($"Using Costume ID: {character.CostumeId}");
            }

            if (!string.IsNullOrEmpty(character.IconUrl))
            {
                Console.WriteLine($"Icon URL: {character.IconUrl}");
            }

            // Talents
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nTalent Information:");
            Console.ResetColor();
            foreach (var talent in character.Talents)
            {
                Console.WriteLine($"• {talent.Name} (ID: {talent.Id})");
                Console.WriteLine($"  Level: {talent.Level} (Base {talent.BaseLevel}{(talent.ExtraLevel > 0 ? $" + {talent.ExtraLevel} from constellations" : "")})");
                if (!string.IsNullOrEmpty(talent.IconUrl))
                {
                    Console.WriteLine($"  Icon URL: {talent.IconUrl}");
                }
            }

            // Constellations
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nUnlocked Constellations:");
            Console.ResetColor();
            if (character.Constellations.Count > 0)
            {
                foreach (var constId in character.Constellations)
                {
                    Console.WriteLine($"• {constId.Name} (ID: {constId.Id})");
                    Console.WriteLine($"  Position: {constId.Position}");
                    if (!string.IsNullOrEmpty(constId.IconUrl))
                    {
                        Console.WriteLine($"  Icon URL: {constId.IconUrl}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No constellations unlocked.");
            }

            // Weapon
            if (character.Weapon != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nWeapon Information:");
                Console.ResetColor();
                Console.WriteLine($"Name: {character.Weapon.Name} (ID: {character.Weapon.Id})");
                Console.WriteLine($"Type: {character.Weapon.Type}");
                Console.WriteLine($"Rarity: {character.Weapon.Rarity}★");
                Console.WriteLine($"Level: {character.Weapon.Level}/{GetWeaponMaxLevel(character.Weapon.Ascension)} (Ascension {character.Weapon.Ascension})");
                Console.WriteLine($"Refinement: R{character.Weapon.Refinement}");
                Console.WriteLine($"Base ATK: {character.Weapon.BaseAttack}");
                if (character.Weapon.SecondaryStat != null)
                {
                    Console.WriteLine($"Secondary Stat: {character.Weapon.SecondaryStat}");
                }
                if (!string.IsNullOrEmpty(character.Weapon.IconUrl))
                {
                    Console.WriteLine($"Icon URL: {character.Weapon.IconUrl}");
                }
            }

            // Stats
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nKey Statistics:");
            Console.ResetColor();
            DisplayStat(character, StatType.HP, "HP");
            DisplayStat(character, StatType.Attack, "ATK");
            DisplayStat(character, StatType.Defense, "DEF");
            DisplayStat(character, StatType.CriticalRate, "CRIT Rate", true);
            DisplayStat(character, StatType.CriticalDamage, "CRIT DMG", true);
            DisplayStat(character, StatType.EnergyRecharge, "Energy Recharge", true);
            DisplayStat(character, StatType.ElementalMastery, "Elemental Mastery");

            // Element-specific damage bonus
            StatType elementDamageType = character.Element switch
            {
                ElementType.Pyro => StatType.PyroDamageBonus,
                ElementType.Hydro => StatType.HydroDamageBonus,
                ElementType.Electro => StatType.ElectroDamageBonus,
                ElementType.Cryo => StatType.CryoDamageBonus,
                ElementType.Anemo => StatType.AnemoDamageBonus,
                ElementType.Geo => StatType.GeoDamageBonus,
                ElementType.Dendro => StatType.DendroDamageBonus,
                _ => StatType.PhysicalDamageBonus
            };

            string elementName = character.Element.ToString();
            if (character.Element != ElementType.Unknown && character.Element != ElementType.Physical)
            {
                DisplayStat(character, elementDamageType, $"{elementName} DMG Bonus", true);
            }
            DisplayStat(character, StatType.PhysicalDamageBonus, "Physical DMG Bonus", true);
            DisplayStat(character, StatType.HealingBonus, "Healing Bonus", true);

            // Artifacts
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nArtifact Analysis:");
            Console.ResetColor();

            if (character.Artifacts.Count > 0)
            {
                // Track sets
                Dictionary<string, int> artifactSets = new Dictionary<string, int>();
                foreach (var artifact in character.Artifacts)
                {
                    if (!artifactSets.ContainsKey(artifact.SetName))
                        artifactSets[artifact.SetName] = 0;
                    artifactSets[artifact.SetName]++;
                }

                // Display set bonuses
                Console.WriteLine("Set Bonuses:");
                foreach (var setBonus in artifactSets)
                {
                    Console.WriteLine($"• {setBonus.Key}: {setBonus.Value} piece(s)");
                }

                // Artifact stats
                double totalCv = 0;
                Console.WriteLine("\nArtifact Details:");
                foreach (var artifact in character.Artifacts)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"• {artifact.Slot} - {artifact.Name} ({artifact.Rarity} star)");
                    Console.ResetColor();
                    Console.WriteLine($"  Level: +{artifact.Level}");
                    Console.WriteLine($"  Set: {artifact.SetName}");

                    if (!string.IsNullOrEmpty(artifact.IconUrl))
                    {
                        Console.WriteLine($"  Icon URL: {artifact.IconUrl}");
                    }

                    if (artifact.MainStat != null)
                    {
                        Console.WriteLine($"  Main Stat: {artifact.MainStat}");
                    }

                    // Display substats and calculate CV for this artifact
                    if (artifact.SubStats.Count > 0)
                    {
                        double artifactCv = 0;
                        Console.WriteLine($"  Substats:");
                        foreach (var substat in artifact.SubStats)
                        {
                            Console.WriteLine($"    • {substat}");

                            // Calculate CV contribution from this substat
                            if (substat.Type == StatType.CriticalRate)
                                artifactCv += substat.Value * 2; // CR counts as 2 CV
                            else if (substat.Type == StatType.CriticalDamage)
                                artifactCv += substat.Value; // CD counts as 1 CV
                        }

                        if (artifactCv > 0)
                        {
                            Console.WriteLine($"  Artifact CV: {artifactCv:F1}");
                            totalCv += artifactCv;
                        }
                    }
                }

                Console.WriteLine($"\nTotal CV from Artifacts: {totalCv:F1}");

                // CV quality assessment
                string cvRating;
                if (totalCv >= 220) cvRating = "Godly";
                else if (totalCv >= 180) cvRating = "Excellent";
                else if (totalCv >= 140) cvRating = "Very Good";
                else if (totalCv >= 100) cvRating = "Good";
                else if (totalCv >= 60) cvRating = "Average";
                else cvRating = "Needs Improvement";

                Console.WriteLine($"CV Rating: {cvRating}");
            }
            else
            {
                Console.WriteLine("No artifacts equipped.");
            }
        }

        private static void ArtifactBuildAnalysis()
        {
            if (_lastFetchedCharacters.Count == 0)
            {
                Console.WriteLine("No character data available. Please fetch character data first.");
                return;
            }

            Console.WriteLine("\n=== Available Characters ===");
            for (int i = 0; i < _lastFetchedCharacters.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_lastFetchedCharacters[i].Name} (Lv. {_lastFetchedCharacters[i].Level})");
            }

            Console.Write("\nEnter character number to analyze artifacts (or 0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int charIndex) || charIndex < 1 || charIndex > _lastFetchedCharacters.Count)
            {
                if (charIndex != 0)
                    Console.WriteLine("Invalid selection.");
                return;
            }

            var character = _lastFetchedCharacters[charIndex - 1];

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n=== Artifact Build Analysis: {character.Name} ===");
            Console.ResetColor();

            if (character.Artifacts.Count == 0)
            {
                Console.WriteLine("No artifacts equipped on this character.");
                return;
            }

            // Track sets
            Dictionary<string, int> artifactSets = new Dictionary<string, int>();
            foreach (var artifact in character.Artifacts)
            {
                if (!artifactSets.ContainsKey(artifact.SetName))
                    artifactSets[artifact.SetName] = 0;
                artifactSets[artifact.SetName]++;
            }

            // Display set bonuses
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Set Bonuses:");
            Console.ResetColor();
            foreach (var setBonus in artifactSets)
            {
                Console.WriteLine($"• {setBonus.Key}: {setBonus.Value} piece(s)");

                // Common set bonus descriptions (this would be better from assets)
                if (setBonus.Value >= 2)
                {
                    Console.WriteLine($"  2-Piece Bonus Active");
                }
                if (setBonus.Value >= 4)
                {
                    Console.WriteLine($"  4-Piece Bonus Active");
                }
            }

            // Substat distribution
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nSubstat Distribution:");
            Console.ResetColor();

            Dictionary<StatType, double> substatTotals = new Dictionary<StatType, double>();
            int totalRolls = 0;

            foreach (var artifact in character.Artifacts)
            {
                foreach (var substat in artifact.SubStats)
                {
                    if (!substatTotals.ContainsKey(substat.Type))
                        substatTotals[substat.Type] = 0;

                    substatTotals[substat.Type] += substat.Value;

                    // Estimate number of rolls (very approximate)
                    int estimatedRolls = EstimateSubstatRolls(substat.Type, substat.Value, artifact.Rarity);
                    totalRolls += estimatedRolls;
                }
            }

            foreach (var stat in substatTotals.OrderByDescending(s => s.Value))
            {
                string formattedValue;
                bool isPercentage = IsPercentageStat(stat.Key);

                if (isPercentage)
                {
                    formattedValue = $"{stat.Value:P1}";
                }
                else
                {
                    formattedValue = $"{stat.Value:F1}";
                }

                Console.WriteLine($"• {stat.Key}: {formattedValue}");
            }

            // CV Analysis
            double critRate = character.GetStatValue(StatType.CriticalRate);
            double critDmg = character.GetStatValue(StatType.CriticalDamage);
            double critValue = critRate * 2 + critDmg;
            double artifactCritRate = 0;
            double artifactCritDmg = 0;

            if (substatTotals.TryGetValue(StatType.CriticalRate, out double cr))
                artifactCritRate = cr;

            if (substatTotals.TryGetValue(StatType.CriticalDamage, out double cd))
                artifactCritDmg = cd;

            double artifactCV = artifactCritRate * 2 + artifactCritDmg;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nCritical Values:");
            Console.ResetColor();
            Console.WriteLine($"• CRIT Rate: {critRate:P1} (from artifacts: {artifactCritRate:P1})");
            Console.WriteLine($"• CRIT DMG: {critDmg:P1} (from artifacts: {artifactCritDmg:P1})");
            Console.WriteLine($"• CRIT Ratio: {critRate:P1} : {critDmg:P1} = 1 : {critDmg / (critRate > 0 ? critRate : 1):F2}");
            Console.WriteLine($"• Total CV: {critValue * 100:F1} (from artifacts: {artifactCV * 100:F1})");

            // Optimal ratio is usually 1:2
            string ratioAdvice;
            double ratio = critDmg / (critRate > 0 ? critRate : 1);
            if (ratio < 1.7) ratioAdvice = "Consider increasing CRIT DMG";
            else if (ratio > 2.3) ratioAdvice = "Consider increasing CRIT Rate";
            else ratioAdvice = "Good CRIT ratio";

            Console.WriteLine($"• Ratio Advice: {ratioAdvice}");

            // Mainstat Analysis
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nMain Stat Analysis:");
            Console.ResetColor();

            foreach (var artifact in character.Artifacts.OrderBy(a => (int)a.Slot))
            {
                Console.WriteLine($"• {artifact.Slot}: {(artifact.MainStat != null ? artifact.MainStat.ToString() : "None")}");

                // Advice on main stats based on character element and common builds
                if (artifact.Slot == ArtifactSlot.Sands)
                {
                    if (artifact.MainStat?.Type == StatType.HPPercentage)
                        Console.WriteLine($"  Note: HP% is typically for HP-scaling characters");
                    else if (artifact.MainStat?.Type == StatType.AttackPercentage)
                        Console.WriteLine($"  Note: ATK% is a standard DPS choice");
                    else if (artifact.MainStat?.Type == StatType.ElementalMastery)
                        Console.WriteLine($"  Note: EM is good for reaction-based characters");
                    else if (artifact.MainStat?.Type == StatType.EnergyRecharge)
                        Console.WriteLine($"  Note: ER is good for burst-dependent characters");
                }
                else if (artifact.Slot == ArtifactSlot.Goblet)
                {
                    // Element-specific damage bonus
                    StatType elementDamageType = character.Element switch
                    {
                        ElementType.Pyro => StatType.PyroDamageBonus,
                        ElementType.Hydro => StatType.HydroDamageBonus,
                        ElementType.Electro => StatType.ElectroDamageBonus,
                        ElementType.Cryo => StatType.CryoDamageBonus,
                        ElementType.Anemo => StatType.AnemoDamageBonus,
                        ElementType.Geo => StatType.GeoDamageBonus,
                        ElementType.Dendro => StatType.DendroDamageBonus,
                        _ => StatType.PhysicalDamageBonus
                    };

                    if (artifact.MainStat?.Type == elementDamageType)
                        Console.WriteLine($"  Note: {character.Element} DMG Bonus is good for this character");
                    else if (artifact.MainStat?.Type == StatType.PhysicalDamageBonus)
                        Console.WriteLine($"  Note: Physical DMG Bonus is for physical DPS builds");
                    else if (artifact.MainStat?.Type == StatType.ElementalMastery)
                        Console.WriteLine($"  Note: EM is for reaction-focused builds");
                    else if (artifact.MainStat?.Type == StatType.HPPercentage || artifact.MainStat?.Type == StatType.AttackPercentage)
                        Console.WriteLine($"  Note: Consider an elemental DMG bonus goblet if available");
                }
                else if (artifact.Slot == ArtifactSlot.Circlet)
                {
                    if (artifact.MainStat?.Type == StatType.CriticalRate || artifact.MainStat?.Type == StatType.CriticalDamage)
                        Console.WriteLine($"  Note: CRIT circlets are standard for most DPS characters");
                    else if (artifact.MainStat?.Type == StatType.HealingBonus)
                        Console.WriteLine($"  Note: Healing Bonus is for dedicated healers");
                    else if (artifact.MainStat?.Type == StatType.ElementalMastery)
                        Console.WriteLine($"  Note: EM is for reaction-focused builds");
                }
            }

            // Overall Build Assessment
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nBuild Cohesion Assessment:");
            Console.ResetColor();

            var weaponType = character.Weapon?.Type ?? WeaponType.Unknown;
            var element = character.Element;
            bool has4PieceSet = artifactSets.Any(s => s.Value >= 4);
            bool hasElementalDmgGoblet = character.Artifacts.Any(a => a.Slot == ArtifactSlot.Goblet &&
                                                                  a.MainStat?.Type == GetElementalDmgType(element));
            bool hasCritCirclet = character.Artifacts.Any(a => a.Slot == ArtifactSlot.Circlet &&
                                                           (a.MainStat?.Type == StatType.CriticalRate ||
                                                            a.MainStat?.Type == StatType.CriticalDamage));

            // Very simplistic build coherence check
            int cohesionScore = 0;
            if (has4PieceSet) cohesionScore += 1;
            if (hasElementalDmgGoblet) cohesionScore += 1;
            if (hasCritCirclet) cohesionScore += 1;
            if (critValue >= 0.6) cohesionScore += 1; // 60+ CV is decent
            if (substatTotals.ContainsKey(StatType.AttackPercentage) ||
                substatTotals.ContainsKey(StatType.ElementalMastery)) cohesionScore += 1;

            string cohesionRating;
            if (cohesionScore >= 5) cohesionRating = "Excellent";
            else if (cohesionScore >= 4) cohesionRating = "Very Good";
            else if (cohesionScore >= 3) cohesionRating = "Good";
            else if (cohesionScore >= 2) cohesionRating = "Average";
            else cohesionRating = "Needs Improvement";

            Console.WriteLine($"Build Cohesion Rating: {cohesionRating}");
            Console.WriteLine("Note: This is a simplified analysis and may not reflect all nuances of character building.");
        }

        private static StatType GetElementalDmgType(ElementType element)
        {
            return element switch
            {
                ElementType.Pyro => StatType.PyroDamageBonus,
                ElementType.Hydro => StatType.HydroDamageBonus,
                ElementType.Electro => StatType.ElectroDamageBonus,
                ElementType.Cryo => StatType.CryoDamageBonus,
                ElementType.Anemo => StatType.AnemoDamageBonus,
                ElementType.Geo => StatType.GeoDamageBonus,
                ElementType.Dendro => StatType.DendroDamageBonus,
                _ => StatType.PhysicalDamageBonus
            };
        }

        private static int EstimateSubstatRolls(StatType type, double value, int rarity)
        {
            // Very crude approximation of substat roll values
            // In reality, these vary based on rarity and have ranges
            double rollValue = type switch
            {
                StatType.CriticalRate => 0.033, // ~3.3% per roll at 5*
                StatType.CriticalDamage => 0.066, // ~6.6% per roll at 5*
                StatType.AttackPercentage => 0.05, // ~5% per roll at 5*
                StatType.HPPercentage => 0.05, // ~5% per roll at 5*
                StatType.DefensePercentage => 0.063, // ~6.3% per roll at 5*
                StatType.ElementalMastery => 20, // ~20 per roll at 5*
                StatType.EnergyRecharge => 0.055, // ~5.5% per roll at 5*
                StatType.Attack => 16, // ~16 per roll at 5*
                StatType.HP => 254, // ~254 per roll at 5*
                StatType.Defense => 19, // ~19 per roll at 5*
                _ => 1 // Default fallback
            };

            // Adjust for 4* artifacts
            if (rarity < 5)
                rollValue *= 0.8;

            // Avoid division by zero or very small numbers
            if (rollValue <= 0.001) return 1;

            return Math.Max(1, (int)Math.Round(value / rollValue));
        }

        private static bool IsPercentageStat(StatType statType)
        {
            return statType == StatType.HPPercentage ||
                   statType == StatType.AttackPercentage ||
                   statType == StatType.DefensePercentage ||
                   statType == StatType.CriticalRate ||
                   statType == StatType.CriticalDamage ||
                   statType == StatType.EnergyRecharge ||
                   statType == StatType.HealingBonus ||
                   statType.ToString().Contains("DamageBonus");
        }

        private static void CompareCharacters()
        {
            if (_lastFetchedCharacters.Count < 2)
            {
                Console.WriteLine("Need at least 2 characters to compare. Please fetch more character data.");
                return;
            }

            Console.WriteLine("\n=== Available Characters ===");
            for (int i = 0; i < _lastFetchedCharacters.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_lastFetchedCharacters[i].Name} (Lv. {_lastFetchedCharacters[i].Level})");
            }

            Console.Write("\nEnter first character number: ");
            if (!int.TryParse(Console.ReadLine(), out int char1Index) || char1Index < 1 || char1Index > _lastFetchedCharacters.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            Console.Write("Enter second character number: ");
            if (!int.TryParse(Console.ReadLine(), out int char2Index) || char2Index < 1 || char2Index > _lastFetchedCharacters.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            if (char1Index == char2Index)
            {
                Console.WriteLine("Please select two different characters.");
                return;
            }

            var character1 = _lastFetchedCharacters[char1Index - 1];
            var character2 = _lastFetchedCharacters[char2Index - 1];

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n=== Character Comparison: {character1.Name} vs {character2.Name} ===");
            Console.ResetColor();

            // Basic info comparison
            Console.WriteLine("\n[Basic Information]");
            Console.WriteLine($"{"Attribute",-20} | {character1.Name,-20} | {character2.Name,-20}");
            Console.WriteLine(new string('-', 65));
            Console.WriteLine($"{"Level",-20} | {character1.Level}/{GetMaxLevel(character1.Ascension),-20} | {character2.Level}/{GetMaxLevel(character2.Ascension),-20}");
            Console.WriteLine($"{"Element",-20} | {character1.Element,-20} | {character2.Element,-20}");
            Console.WriteLine($"{"Constellation",-20} | C{character1.ConstellationLevel,-19} | C{character2.ConstellationLevel,-19}");
            Console.WriteLine($"{"Friendship",-20} | {character1.Friendship,-20} | {character2.Friendship,-20}");

            // Weapon comparison
            Console.WriteLine("\n[Weapon]");
            string weapon1Name = character1.Weapon?.Name ?? "None";
            string weapon2Name = character2.Weapon?.Name ?? "None"; // Fixed: was character1.Weapon
            string weapon1Level = character1.Weapon != null ? $"{character1.Weapon.Level}/{GetWeaponMaxLevel(character1.Weapon.Ascension)}" : "N/A";
            string weapon2Level = character2.Weapon != null ? $"{character2.Weapon.Level}/{GetWeaponMaxLevel(character2.Weapon.Ascension)}" : "N/A";
            string weapon1Refinement = character1.Weapon != null ? $"R{character1.Weapon.Refinement}" : "N/A";
            string weapon2Refinement = character2.Weapon != null ? $"R{character2.Weapon.Refinement}" : "N/A";

            Console.WriteLine($"{"Attribute",-20} | {character1.Name,-20} | {character2.Name,-20}");
            Console.WriteLine(new string('-', 65));
            Console.WriteLine($"{"Weapon",-20} | {weapon1Name,-20} | {weapon2Name,-20}");
            Console.WriteLine($"{"Level",-20} | {weapon1Level,-20} | {weapon2Level,-20}");
            Console.WriteLine($"{"Refinement",-20} | {weapon1Refinement,-20} | {weapon2Refinement,-20}");

            // Stats comparison
            Console.WriteLine("\n[Key Statistics]");
            Console.WriteLine($"{"Stat",-20} | {character1.Name,-20} | {character2.Name,-20}");
            Console.WriteLine(new string('-', 65));

            CompareStats(character1, character2, StatType.HP, "HP");
            CompareStats(character1, character2, StatType.Attack, "ATK");
            CompareStats(character1, character2, StatType.Defense, "DEF");
            CompareStats(character1, character2, StatType.CriticalRate, "CRIT Rate", true);
            CompareStats(character1, character2, StatType.CriticalDamage, "CRIT DMG", true);
            CompareStats(character1, character2, StatType.EnergyRecharge, "Energy Recharge", true);
            CompareStats(character1, character2, StatType.ElementalMastery, "Elemental Mastery");

            // Element-specific damage bonus
            StatType element1DamageType = GetElementalDmgType(character1.Element);
            StatType element2DamageType = GetElementalDmgType(character2.Element);

            string element1Name = character1.Element.ToString();
            string element2Name = character2.Element.ToString();

            if (character1.Element != ElementType.Unknown && character1.Element != ElementType.Physical)
            {
                double element1Bonus = character1.GetStatValue(element1DamageType);
                string element1Value = $"{element1Bonus:P1}";
                Console.WriteLine($"{element1Name + " DMG Bonus",-20} | {element1Value,-20} | {"-",-20}");
            }

            if (character2.Element != ElementType.Unknown && character2.Element != ElementType.Physical &&
                character2.Element != character1.Element) // Skip if same element
            {
                double element2Bonus = character2.GetStatValue(element2DamageType);
                string element2Value = $"{element2Bonus:P1}";
                Console.WriteLine($"{element2Name + " DMG Bonus",-20} | {"-",-20} | {element2Value,-20}");
            }

            // If same element, show side-by-side
            if (character1.Element == character2.Element &&
                character1.Element != ElementType.Unknown &&
                character1.Element != ElementType.Physical)
            {
                CompareStats(character1, character2, element1DamageType, $"{element1Name} DMG Bonus", true);
            }

            // Physical and healing for both
            CompareStats(character1, character2, StatType.PhysicalDamageBonus, "Physical DMG Bonus", true);
            CompareStats(character1, character2, StatType.HealingBonus, "Healing Bonus", true);

            // CV comparison
            double cv1 = character1.GetStatValue(StatType.CriticalRate) * 2 + character1.GetStatValue(StatType.CriticalDamage);
            double cv2 = character2.GetStatValue(StatType.CriticalRate) * 2 + character2.GetStatValue(StatType.CriticalDamage);
            Console.WriteLine($"{"Critical Value",-20} | {(cv1 * 100):F1,-20} | {(cv2 * 100):F1,-20}");

            // Artifact comparison
            Console.WriteLine("\n[Artifact Sets]");
            Dictionary<string, int> char1Sets = GetArtifactSets(character1);
            Dictionary<string, int> char2Sets = GetArtifactSets(character2);

            Console.WriteLine($"{"Set",-20} | {character1.Name,-20} | {character2.Name,-20}");
            Console.WriteLine(new string('-', 65));

            HashSet<string> allSets = new HashSet<string>(char1Sets.Keys.Concat(char2Sets.Keys));
            foreach (var set in allSets)
            {
                int char1Count = char1Sets.TryGetValue(set, out var count1) ? count1 : 0;
                int char2Count = char2Sets.TryGetValue(set, out var count2) ? count2 : 0;
                Console.WriteLine($"{set,-20} | {char1Count,-20} | {char2Count,-20}");
            }

            Console.WriteLine($"\nTotal pieces: {character1.Artifacts.Count} vs {character2.Artifacts.Count}");
        }

        private static Dictionary<string, int> GetArtifactSets(Character character)
        {
            Dictionary<string, int> artifactSets = new Dictionary<string, int>();
            foreach (var artifact in character.Artifacts)
            {
                if (!artifactSets.ContainsKey(artifact.SetName))
                    artifactSets[artifact.SetName] = 0;
                artifactSets[artifact.SetName]++;
            }
            return artifactSets;
        }

        private static void CompareStats(Character character1, Character character2, StatType statType, string label, bool isPercentage = false)
        {
            double value1 = character1.GetStatValue(statType);
            double value2 = character2.GetStatValue(statType);

            string formattedValue1, formattedValue2;

            if (isPercentage)
            {
                formattedValue1 = $"{value1:P1}";
                formattedValue2 = $"{value2:P1}";
            }
            else if (value1 == (int)value1 && value2 == (int)value2)
            {
                formattedValue1 = $"{value1:N0}";
                formattedValue2 = $"{value2:N0}";
            }
            else
            {
                formattedValue1 = $"{value1:F1}";
                formattedValue2 = $"{value2:F1}";
            }

            Console.WriteLine($"{label,-20} | {formattedValue1,-20} | {formattedValue2,-20}");
        }

        private static void RankCharactersByStats()
        {
            if (_lastFetchedCharacters.Count == 0)
            {
                Console.WriteLine("No character data available. Please fetch character data first.");
                return;
            }

            Console.WriteLine("\n=== Rank Characters By Stat ===");
            Console.WriteLine("1. HP");
            Console.WriteLine("2. ATK");
            Console.WriteLine("3. DEF");
            Console.WriteLine("4. CRIT Rate");
            Console.WriteLine("5. CRIT DMG");
            Console.WriteLine("6. CRIT Value");
            Console.WriteLine("7. Energy Recharge");
            Console.WriteLine("8. Elemental Mastery");
            Console.WriteLine("9. Element DMG Bonus");
            Console.WriteLine("10. Overall Build Score");
            Console.Write("\nEnter your choice (1-10): ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 10)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            var characters = _lastFetchedCharacters;

            switch (choice)
            {
                case 1: // HP
                    RankByStat(characters, StatType.HP, "HP", false);
                    break;
                case 2: // ATK
                    RankByStat(characters, StatType.Attack, "ATK", false);
                    break;
                case 3: // DEF
                    RankByStat(characters, StatType.Defense, "DEF", false);
                    break;
                case 4: // CRIT Rate
                    RankByStat(characters, StatType.CriticalRate, "CRIT Rate", true);
                    break;
                case 5: // CRIT DMG
                    RankByStat(characters, StatType.CriticalDamage, "CRIT DMG", true);
                    break;
                case 6: // CRIT Value
                    RankByCritValue(characters);
                    break;
                case 7: // Energy Recharge
                    RankByStat(characters, StatType.EnergyRecharge, "Energy Recharge", true);
                    break;
                case 8: // Elemental Mastery
                    RankByStat(characters, StatType.ElementalMastery, "Elemental Mastery", false);
                    break;
                case 9: // Element DMG Bonus
                    RankByElementalDmg(characters);
                    break;
                case 10: // Build Score
                    RankByBuildScore(characters);
                    break;
            }
        }

        private static void RankByStat(List<Character> characters, StatType statType, string label, bool isPercentage)
        {
            var ranking = characters
                .Select(c => new { Character = c, Value = c.GetStatValue(statType) })
                .OrderByDescending(c => c.Value)
                .ToList();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n=== Characters Ranked By {label} ===");
            Console.ResetColor();

            Console.WriteLine($"{"Rank",-5} | {"Character",-15} | {label,-15} | {"Level",-10}");
            Console.WriteLine(new string('-', 50));

            for (int i = 0; i < ranking.Count; i++)
            {
                string formattedValue;
                if (isPercentage)
                    formattedValue = $"{ranking[i].Value:P1}";
                else
                    formattedValue = $"{ranking[i].Value:N0}";

                Console.WriteLine($"{i + 1,-5} | {ranking[i].Character.Name,-15} | {formattedValue,-15} | {ranking[i].Character.Level}/{GetMaxLevel(ranking[i].Character.Ascension),-10}");
            }
        }

        private static void RankByCritValue(List<Character> characters)
        {
            var ranking = characters
                .Select(c => new
                {
                    Character = c,
                    CritRate = c.GetStatValue(StatType.CriticalRate),
                    CritDmg = c.GetStatValue(StatType.CriticalDamage),
                    CV = c.GetStatValue(StatType.CriticalRate) * 2 + c.GetStatValue(StatType.CriticalDamage)
                })
                .OrderByDescending(c => c.CV)
                .ToList();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n=== Characters Ranked By CRIT Value ===");
            Console.ResetColor();

            Console.WriteLine($"{"Rank",-5} | {"Character",-15} | {"CRIT Value",-12} | {"CRIT Rate",-12} | {"CRIT DMG",-12}");
            Console.WriteLine(new string('-', 65));

            for (int i = 0; i < ranking.Count; i++)
            {
                Console.WriteLine($"{i + 1,-5} | {ranking[i].Character.Name,-15} | {ranking[i].CV * 100:F1,-12} | {ranking[i].CritRate:P1,-12} | {ranking[i].CritDmg:P1,-12}");
            }
        }

        private static void RankByElementalDmg(List<Character> characters)
        {
            var ranking = characters
                .Select(c => new
                {
                    Character = c,
                    Element = c.Element,
                    DmgBonus = c.Element switch
                    {
                        ElementType.Pyro => c.GetStatValue(StatType.PyroDamageBonus),
                        ElementType.Hydro => c.GetStatValue(StatType.HydroDamageBonus),
                        ElementType.Electro => c.GetStatValue(StatType.ElectroDamageBonus),
                        ElementType.Cryo => c.GetStatValue(StatType.CryoDamageBonus),
                        ElementType.Anemo => c.GetStatValue(StatType.AnemoDamageBonus),
                        ElementType.Geo => c.GetStatValue(StatType.GeoDamageBonus),
                        ElementType.Dendro => c.GetStatValue(StatType.DendroDamageBonus),
                        _ => c.GetStatValue(StatType.PhysicalDamageBonus)
                    }
                })
                .OrderByDescending(c => c.DmgBonus)
                .ToList();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n=== Characters Ranked By Elemental DMG Bonus ===");
            Console.ResetColor();

            Console.WriteLine($"{"Rank",-5} | {"Character",-15} | {"Element",-10} | {"DMG Bonus",-12}");
            Console.WriteLine(new string('-', 50));

            for (int i = 0; i < ranking.Count; i++)
            {
                string dmgType = ranking[i].Element != ElementType.Unknown && ranking[i].Element != ElementType.Physical
                    ? $"{ranking[i].Element} DMG"
                    : "Physical DMG";

                Console.WriteLine($"{i + 1,-5} | {ranking[i].Character.Name,-15} | {ranking[i].Element,-10} | {ranking[i].DmgBonus:P1,-12}");
            }
        }

        private static void RankByBuildScore(List<Character> characters)
        {
            var ranking = characters
                .Select(c =>
                {
                    double cv = c.GetStatValue(StatType.CriticalRate) * 2 + c.GetStatValue(StatType.CriticalDamage);

                    // Element-specific damage bonus
                    double elementalDmg = c.Element switch
                    {
                        ElementType.Pyro => c.GetStatValue(StatType.PyroDamageBonus),
                        ElementType.Hydro => c.GetStatValue(StatType.HydroDamageBonus),
                        ElementType.Electro => c.GetStatValue(StatType.ElectroDamageBonus),
                        ElementType.Cryo => c.GetStatValue(StatType.CryoDamageBonus),
                        ElementType.Anemo => c.GetStatValue(StatType.AnemoDamageBonus),
                        ElementType.Geo => c.GetStatValue(StatType.GeoDamageBonus),
                        ElementType.Dendro => c.GetStatValue(StatType.DendroDamageBonus),
                        _ => c.GetStatValue(StatType.PhysicalDamageBonus)
                    };

                    // Get artifact sets
                    Dictionary<string, int> sets = GetArtifactSets(c);
                    bool has4PieceSet = sets.Any(s => s.Value >= 4);

                    // Calculate a simplified build score
                    double buildScore = cv * 50 // Weight CV heavily
                        + elementalDmg * 30 // Elemental DMG bonus is important
                        + c.GetStatValue(StatType.AttackPercentage) * 20 // ATK% is useful for most characters
                        + (has4PieceSet ? 20 : 0) // Bonus for having a 4-piece set
                        + c.Artifacts.Count * 2; // Small bonus for each artifact equipped

                    return new { Character = c, BuildScore = buildScore };
                })
                .OrderByDescending(c => c.BuildScore)
                .ToList();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n=== Characters Ranked By Overall Build Score ===");
            Console.WriteLine("(Considers CRIT stats, DMG bonus, artifacts sets, and more)");
            Console.ResetColor();

            Console.WriteLine($"{"Rank",-5} | {"Character",-15} | {"Build Score",-12} | {"Level",-10}");
            Console.WriteLine(new string('-', 50));

            for (int i = 0; i < ranking.Count; i++)
            {
                Console.WriteLine($"{i + 1,-5} | {ranking[i].Character.Name,-15} | {ranking[i].BuildScore:F0,-12} | {ranking[i].Character.Level}/{GetMaxLevel(ranking[i].Character.Ascension),-10}");
            }

            Console.WriteLine("\nNote: Build score is a simplified metric and may not reflect optimal builds for specific characters.");
        }

        private static void FilterCharacters()
        {
            if (_lastFetchedCharacters.Count == 0)
            {
                Console.WriteLine("No character data available. Please fetch character data first.");
                return;
            }

            Console.WriteLine("\n=== Filter Characters ===");
            Console.WriteLine("1. By Element");
            Console.WriteLine("2. By Weapon Type");
            Console.WriteLine("3. By Constellation Level");
            Console.WriteLine("4. By Artifact Set");
            Console.WriteLine("5. By Level Range");
            Console.Write("\nEnter your choice (1-5): ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 5)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            List<Character> filteredCharacters = new List<Character>();

            switch (choice)
            {
                case 1: // By Element
                    filteredCharacters = FilterByElement();
                    break;
                case 2: // By Weapon Type
                    filteredCharacters = FilterByWeaponType();
                    break;
                case 3: // By Constellation Level
                    filteredCharacters = FilterByConstellation();
                    break;
                case 4: // By Artifact Set
                    filteredCharacters = FilterByArtifactSet();
                    break;
                case 5: // By Level Range
                    filteredCharacters = FilterByLevelRange();
                    break;
                default:
                    return;
            }

            if (filteredCharacters.Count > 0)
            {
                Console.WriteLine($"\nFound {filteredCharacters.Count} characters matching filter:");
                DisplayCharacters(filteredCharacters);
            }
            else
            {
                Console.WriteLine("No characters match the filter criteria.");
            }
        }

        private static List<Character> FilterByElement()
        {
            Console.WriteLine("\n=== Select Element ===");
            Console.WriteLine("1. Pyro");
            Console.WriteLine("2. Hydro");
            Console.WriteLine("3. Electro");
            Console.WriteLine("4. Cryo");
            Console.WriteLine("5. Anemo");
            Console.WriteLine("6. Geo");
            Console.WriteLine("7. Dendro");
            Console.Write("\nEnter your choice (1-7): ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 7)
            {
                Console.WriteLine("Invalid selection.");
                return new List<Character>();
            }

            ElementType selectedElement = choice switch
            {
                1 => ElementType.Pyro,
                2 => ElementType.Hydro,
                3 => ElementType.Electro,
                4 => ElementType.Cryo,
                5 => ElementType.Anemo,
                6 => ElementType.Geo,
                7 => ElementType.Dendro,
                _ => ElementType.Unknown
            };

            return _lastFetchedCharacters.Where(c => c.Element == selectedElement).ToList();
        }

        private static List<Character> FilterByWeaponType()
        {
            Console.WriteLine("\n=== Select Weapon Type ===");
            Console.WriteLine("1. Sword");
            Console.WriteLine("2. Claymore");
            Console.WriteLine("3. Polearm");
            Console.WriteLine("4. Bow");
            Console.WriteLine("5. Catalyst");
            Console.Write("\nEnter your choice (1-5): ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 5)
            {
                Console.WriteLine("Invalid selection.");
                return new List<Character>();
            }

            WeaponType selectedWeaponType = choice switch
            {
                1 => WeaponType.Sword,
                2 => WeaponType.Claymore,
                3 => WeaponType.Polearm,
                4 => WeaponType.Bow,
                5 => WeaponType.Catalyst,
                _ => WeaponType.Unknown
            };

            return _lastFetchedCharacters.Where(c => c.Weapon != null && c.Weapon.Type == selectedWeaponType).ToList();
        }

        private static List<Character> FilterByConstellation()
        {
            Console.Write("\nEnter constellation level (0-6): ");

            if (!int.TryParse(Console.ReadLine(), out int constellation) || constellation < 0 || constellation > 6)
            {
                Console.WriteLine("Invalid selection.");
                return new List<Character>();
            }

            return _lastFetchedCharacters.Where(c => c.ConstellationLevel == constellation).ToList();
        }

        private static List<Character> FilterByArtifactSet()
        {
            // Get all unique artifact sets
            HashSet<string> allSets = new HashSet<string>();
            foreach (var character in _lastFetchedCharacters)
            {
                foreach (var artifact in character.Artifacts)
                {
                    allSets.Add(artifact.SetName);
                }
            }

            if (allSets.Count == 0)
            {
                Console.WriteLine("No artifact sets found.");
                return new List<Character>();
            }

            // Display available sets
            Console.WriteLine("\n=== Available Artifact Sets ===");
            List<string> setsList = allSets.ToList();
            for (int i = 0; i < setsList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {setsList[i]}");
            }

            Console.Write($"\nEnter set number (1-{setsList.Count}): ");

            if (!int.TryParse(Console.ReadLine(), out int setIndex) || setIndex < 1 || setIndex > setsList.Count)
            {
                Console.WriteLine("Invalid selection.");
                return new List<Character>();
            }

            string selectedSet = setsList[setIndex - 1];

            Console.Write("Minimum number of pieces (1-5): ");
            if (!int.TryParse(Console.ReadLine(), out int minPieces) || minPieces < 1 || minPieces > 5)
            {
                minPieces = 1; // Default to 1 if invalid
            }

            return _lastFetchedCharacters.Where(c =>
            {
                var sets = GetArtifactSets(c);
                return sets.TryGetValue(selectedSet, out int count) && count >= minPieces;
            }).ToList();
        }

        private static List<Character> FilterByLevelRange()
        {
            Console.Write("\nEnter minimum level (1-90): ");

            if (!int.TryParse(Console.ReadLine(), out int minLevel) || minLevel < 1 || minLevel > 90)
            {
                minLevel = 1; // Default to 1 if invalid
            }

            Console.Write("Enter maximum level (1-90): ");

            if (!int.TryParse(Console.ReadLine(), out int maxLevel) || maxLevel < minLevel || maxLevel > 90)
            {
                maxLevel = 90; // Default to 90 if invalid
            }

            return _lastFetchedCharacters.Where(c => c.Level >= minLevel && c.Level <= maxLevel).ToList();
        }

        private static void TeamAnalysis()
        {
            if (_lastFetchedCharacters.Count < 4)
            {
                Console.WriteLine("Need at least 4 characters for team analysis. Please fetch more character data.");
                return;
            }

            Console.WriteLine("\n=== Team Analysis Options ===");
            Console.WriteLine("1. Quick Team Builder");
            Console.WriteLine("2. Custom Team Analysis");
            Console.WriteLine("3. Resonance Analysis");
            Console.WriteLine("4. Team Rotation Simulator");
            Console.Write("\nEnter your choice (1-4): ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 4)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            switch (choice)
            {
                case 1:
                    QuickTeamBuilder();
                    break;
                case 2:
                    CustomTeamAnalysis();
                    break;
                case 3:
                    ResonanceAnalysis();
                    break;
                case 4:
                    TeamRotationSimulator();
                    break;
            }
        }

        private static void QuickTeamBuilder()
        {
            if (_lastFetchedCharacters.Count < 4)
            {
                Console.WriteLine("Need at least 4 characters for team builder. Please fetch more character data.");
                return;
            }

            Console.WriteLine("\n=== Quick Team Builder ===");
            Console.WriteLine("1. Balanced Team");
            Console.WriteLine("2. Elemental Reaction Team");
            Console.WriteLine("3. Double Resonance Team");
            Console.WriteLine("4. Strongest Characters Team");
            Console.Write("\nEnter your choice (1-4): ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > 4)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            List<Character> recommendedTeam = new List<Character>();
            string teamDescription = string.Empty;

            switch (choice)
            {
                case 1: // Balanced Team
                    recommendedTeam = BuildBalancedTeam();
                    teamDescription = "This team provides a balanced mix of DPS, support, and elemental coverage.";
                    break;
                case 2: // Elemental Reaction Team
                    recommendedTeam = BuildReactionTeam();
                    teamDescription = "This team is designed to maximize elemental reactions.";
                    break;
                case 3: // Double Resonance Team
                    recommendedTeam = BuildResonanceTeam();
                    teamDescription = "This team includes two sets of elemental resonance for enhanced effects.";
                    break;
                case 4: // Strongest Characters Team
                    recommendedTeam = BuildStrongestTeam();
                    teamDescription = "This team consists of your strongest characters based on build quality.";
                    break;
            }

            if (recommendedTeam.Count == 4)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n=== Recommended Team ===");
                Console.ResetColor();
                Console.WriteLine(teamDescription);
                Console.WriteLine("\nTeam Composition:");

                for (int i = 0; i < recommendedTeam.Count; i++)
                {
                    var character = recommendedTeam[i];
                    Console.WriteLine($"{i + 1}. {character.Name} (Lv. {character.Level}, {character.Element})");
                }

                // Display resonance for the team
                DisplayTeamResonance(recommendedTeam);

                Console.Write("\nDo you want to see detailed character information for this team? (y/n): ");
                string? response = Console.ReadLine()?.ToLower();
                if (response == "y" || response == "yes")
                {
                    DisplayCharacters(recommendedTeam);
                }
            }
            else
            {
                Console.WriteLine("Failed to build a suitable team. Try another option or check your character data.");
            }
        }

        private static List<Character> BuildBalancedTeam()
        {
            // Simple logic to try to build a balanced team
            var characters = _lastFetchedCharacters;
            List<Character> team = new List<Character>();

            // Try to include different elements
            HashSet<ElementType> includedElements = new HashSet<ElementType>();

            // Try to find a main DPS (prioritize high ATK or damage dealers)
            var mainDPS = characters
                .OrderByDescending(c => c.GetStatValue(StatType.Attack))
                .FirstOrDefault(c => c.Element != ElementType.Unknown);

            if (mainDPS != null)
            {
                team.Add(mainDPS);
                includedElements.Add(mainDPS.Element);
            }

            // Try to find a support character (prioritize high HP or utility)
            var support = characters
                .Except(team)
                .OrderByDescending(c => c.GetStatValue(StatType.HP))
                .FirstOrDefault(c => !includedElements.Contains(c.Element) && c.Element != ElementType.Unknown);

            if (support != null)
            {
                team.Add(support);
                includedElements.Add(support.Element);
            }

            // Try to find a character with high EM for reactions
            var emChar = characters
                .Except(team)
                .OrderByDescending(c => c.GetStatValue(StatType.ElementalMastery))
                .FirstOrDefault(c => !includedElements.Contains(c.Element) && c.Element != ElementType.Unknown);

            if (emChar != null)
            {
                team.Add(emChar);
                includedElements.Add(emChar.Element);
            }

            // Add one more character with a different element if possible
            var fourthChar = characters
                .Except(team)
                .OrderByDescending(c => c.Level)
                .FirstOrDefault(c => !includedElements.Contains(c.Element) && c.Element != ElementType.Unknown);

            // If no different element, just take the best remaining character
            if (fourthChar == null)
            {
                fourthChar = characters
                    .Except(team)
                    .OrderByDescending(c => c.Level)
                    .FirstOrDefault();
            }

            if (fourthChar != null)
            {
                team.Add(fourthChar);
            }

            // Fill the team with any characters if we still don't have 4
            while (team.Count < 4 && characters.Except(team).Any())
            {
                var nextBest = characters
                    .Except(team)
                    .OrderByDescending(c => c.Level)
                    .First();

                team.Add(nextBest);
            }

            return team;
        }

        private static List<Character> BuildReactionTeam()
        {
            // Simple implementation to create reaction-focused teams
            var characters = _lastFetchedCharacters;
            List<Character> team = new List<Character>();

            // Try to find a Pyro character (for reactions)
            var pyroChar = characters
                .FirstOrDefault(c => c.Element == ElementType.Pyro);

            if (pyroChar != null)
            {
                team.Add(pyroChar);
            }

            // Try to find a Hydro character (for vaporize)
            var hydroChar = characters
                .Except(team)
                .FirstOrDefault(c => c.Element == ElementType.Hydro);

            if (hydroChar != null)
            {
                team.Add(hydroChar);
            }

            // Try to find an Electro character (for overload/electrocharged)
            var electroChar = characters
                .Except(team)
                .FirstOrDefault(c => c.Element == ElementType.Electro);

            if (electroChar != null)
            {
                team.Add(electroChar);
            }

            // Try to find a Cryo character (for melt/freeze)
            var cryoChar = characters
                .Except(team)
                .FirstOrDefault(c => c.Element == ElementType.Cryo);

            if (cryoChar != null && team.Count < 4)
            {
                team.Add(cryoChar);
            }

            // If we still need characters, add Anemo for swirl or any others
            while (team.Count < 4 && characters.Except(team).Any())
            {
                var anemoChar = characters
                    .Except(team)
                    .FirstOrDefault(c => c.Element == ElementType.Anemo);

                if (anemoChar != null)
                {
                    team.Add(anemoChar);
                }
                else
                {
                    // Add highest level remaining character
                    var nextBest = characters
                        .Except(team)
                        .OrderByDescending(c => c.Level)
                        .First();

                    team.Add(nextBest);
                }
            }

            return team;
        }

        private static List<Character> BuildResonanceTeam()
        {
            var characters = _lastFetchedCharacters;
            List<Character> team = new List<Character>();

            // Group characters by element
            var charactersByElement = characters
                .Where(c => c.Element != ElementType.Unknown && c.Element != ElementType.Physical)
                .GroupBy(c => c.Element)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Find elements with at least 2 characters
            var elementsWithResonance = charactersByElement
                .Where(kvp => kvp.Value.Count >= 2)
                .OrderByDescending(kvp => kvp.Value.Max(c => c.Level)) // Prioritize higher level characters
                .Take(2) // Take up to 2 elements for double resonance
                .ToList();

            if (elementsWithResonance.Count == 0)
            {
                // Fallback to just the best characters if no resonance is possible
                return BuildStrongestTeam();
            }

            // Add characters from the primary resonance (up to 2)
            var primaryElement = elementsWithResonance.First().Key;
            var primaryCharacters = charactersByElement[primaryElement]
                .OrderByDescending(c => c.Level)
                .Take(2);

            team.AddRange(primaryCharacters);

            // If we have a second resonance, add those characters too
            if (elementsWithResonance.Count > 1)
            {
                var secondaryElement = elementsWithResonance[1].Key;
                var secondaryCharacters = charactersByElement[secondaryElement]
                    .OrderByDescending(c => c.Level)
                    .Take(4 - team.Count); // Fill the team up to 4

                team.AddRange(secondaryCharacters);
            }

            // If we still need characters, add the best remaining ones
            while (team.Count < 4 && characters.Except(team).Any())
            {
                var nextBest = characters
                    .Except(team)
                    .OrderByDescending(c => c.Level)
                    .First();

                team.Add(nextBest);
            }

            return team;
        }

        private static List<Character> BuildStrongestTeam()
        {
            // Uses a simple scoring system to find the strongest characters
            var characters = _lastFetchedCharacters;

            var scoredCharacters = characters
                .Select(c =>
                {
                    double cv = c.GetStatValue(StatType.CriticalRate) * 2 + c.GetStatValue(StatType.CriticalDamage);
                    double atk = c.GetStatValue(StatType.Attack);
                    int level = c.Level;
                    int constellations = c.ConstellationLevel;

                    // Simple score calculation
                    double score = (cv * 100) // CRIT value is important
                        + (atk / 100)  // Attack value (scaled down)
                        + (level * 2)  // Level matters
                        + (constellations * 10); // Constellations give a bonus

                    return new { Character = c, Score = score };
                })
                .OrderByDescending(sc => sc.Score)
                .Take(4)
                .Select(sc => sc.Character)
                .ToList();

            return scoredCharacters;
        }

        private static void DisplayTeamResonance(List<Character> team)
        {
            // Count elements in the team
            var elementCounts = team
                .Where(c => c.Element != ElementType.Unknown && c.Element != ElementType.Physical)
                .GroupBy(c => c.Element)
                .ToDictionary(g => g.Key, g => g.Count());

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nElemental Resonance:");
            Console.ResetColor();

            bool hasResonance = false;

            foreach (var element in elementCounts)
            {
                if (element.Value >= 2)
                {
                    string resonanceEffect = GetResonanceEffect(element.Key);
                    Console.WriteLine($"• {element.Key} Resonance ({element.Value} characters): {resonanceEffect}");
                    hasResonance = true;
                }
            }

            if (!hasResonance)
            {
                Console.WriteLine("No elemental resonance effects (requires 2+ characters of same element).");
            }
        }

        private static string GetResonanceEffect(ElementType element)
        {
            // Simplified resonance descriptions
            return element switch
            {
                ElementType.Pyro => "Increases ATK by 25%",
                ElementType.Hydro => "Increases HP by 25%",
                ElementType.Electro => "Electro reactions generate particles and deal more damage",
                ElementType.Cryo => "Increases CRIT Rate against enemies affected by Cryo by 15%",
                ElementType.Anemo => "Decreases Stamina Consumption by 15%, increases Movement SPD by 10%, and reduces Skill CD by 5%",
                ElementType.Geo => "Increases Shield Strength by 15%, increases DMG by 15% when protected by a shield",
                ElementType.Dendro => "Increases Elemental Mastery by 50 and triggers additional Dendro effects",
                _ => "Unknown resonance effect"
            };
        }

        private static void CustomTeamAnalysis()
        {
            if (_lastFetchedCharacters.Count < 4)
            {
                Console.WriteLine("Need at least 4 characters for team analysis. Please fetch more character data.");
                return;
            }

            Console.WriteLine("\n=== Select Characters for Team Analysis ===");

            // Display available characters
            for (int i = 0; i < _lastFetchedCharacters.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_lastFetchedCharacters[i].Name} (Lv. {_lastFetchedCharacters[i].Level}, {_lastFetchedCharacters[i].Element})");
            }

            List<Character> team = new List<Character>();

            // Select 4 characters
            for (int i = 1; i <= 4; i++)
            {
                Console.Write($"\nSelect character {i} (1-{_lastFetchedCharacters.Count}): ");

                if (!int.TryParse(Console.ReadLine(), out int charIndex) || charIndex < 1 || charIndex > _lastFetchedCharacters.Count)
                {
                    Console.WriteLine("Invalid selection. Try again.");
                    i--; // Retry this position
                    continue;
                }

                var selectedChar = _lastFetchedCharacters[charIndex - 1];

                if (team.Contains(selectedChar))
                {
                    Console.WriteLine("Character already in team. Try again.");
                    i--; // Retry this position
                    continue;
                }

                team.Add(selectedChar);
            }

            // Analyze team
            AnalyzeCustomTeam(team);
        }

        private static void AnalyzeCustomTeam(List<Character> team)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n=== Custom Team Analysis ===");
            Console.ResetColor();

            // Display team members
            Console.WriteLine("Team Composition:");
            foreach (var character in team)
            {
                Console.WriteLine($"• {character.Name} (Lv. {character.Level}, {character.Element})");
            }

            // Elemental Resonance
            DisplayTeamResonance(team);

            // Team Stats Analysis
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nTeam Stats Overview:");
            Console.ResetColor();

            double teamHP = team.Sum(c => c.GetStatValue(StatType.HP));
            double teamATK = team.Sum(c => c.GetStatValue(StatType.Attack));
            double teamDEF = team.Sum(c => c.GetStatValue(StatType.Defense));
            double teamEM = team.Sum(c => c.GetStatValue(StatType.ElementalMastery));
            double avgCR = team.Average(c => c.GetStatValue(StatType.CriticalRate));
            double avgCD = team.Average(c => c.GetStatValue(StatType.CriticalDamage));
            double avgER = team.Average(c => c.GetStatValue(StatType.EnergyRecharge));

            Console.WriteLine($"• Total HP: {teamHP:N0}");
            Console.WriteLine($"• Total ATK: {teamATK:N0}");
            Console.WriteLine($"• Total DEF: {teamDEF:N0}");
            Console.WriteLine($"• Total Elemental Mastery: {teamEM:N0}");
            Console.WriteLine($"• Average CRIT Rate: {avgCR:P1}");
            Console.WriteLine($"• Average CRIT DMG: {avgCD:P1}");
            Console.WriteLine($"• Average Energy Recharge: {avgER:P1}");

            // Elemental Coverage
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nElemental Coverage:");
            Console.ResetColor();

            var elements = team
                .Select(c => c.Element)
                .Where(e => e != ElementType.Unknown && e != ElementType.Physical)
                .Distinct()
                .ToList();

            foreach (var element in elements)
            {
                Console.WriteLine($"• {element}");
            }

            // Check for common elemental reactions
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nPotential Elemental Reactions:");
            Console.ResetColor();

            if (elements.Contains(ElementType.Pyro) && elements.Contains(ElementType.Hydro))
                Console.WriteLine("• Vaporize (Pyro + Hydro): 1.5x or 2x damage multiplier");

            if (elements.Contains(ElementType.Pyro) && elements.Contains(ElementType.Cryo))
                Console.WriteLine("• Melt (Pyro + Cryo): 1.5x or 2x damage multiplier");

            if (elements.Contains(ElementType.Electro) && elements.Contains(ElementType.Hydro))
                Console.WriteLine("• Electro-Charged (Electro + Hydro): DoT damage");

            if (elements.Contains(ElementType.Electro) && elements.Contains(ElementType.Pyro))
                Console.WriteLine("• Overloaded (Electro + Pyro): AoE Pyro damage");

            if (elements.Contains(ElementType.Cryo) && elements.Contains(ElementType.Hydro))
                Console.WriteLine("• Freeze (Cryo + Hydro): Immobilizes enemies");

            if (elements.Contains(ElementType.Electro) && elements.Contains(ElementType.Cryo))
                Console.WriteLine("• Superconduct (Electro + Cryo): AoE Cryo damage, reduces physical RES");

            if (elements.Contains(ElementType.Anemo))
                Console.WriteLine("• Swirl: Spreads elemental effects and increases damage");

            if (elements.Contains(ElementType.Geo))
                Console.WriteLine("• Crystallize: Creates shields that absorb elemental damage");

            if (elements.Contains(ElementType.Dendro))
            {
                if (elements.Contains(ElementType.Hydro))
                    Console.WriteLine("• Bloom (Dendro + Hydro): Creates Dendro Cores that can explode");

                if (elements.Contains(ElementType.Electro))
                    Console.WriteLine("• Quicken (Dendro + Electro): Enables Aggravate and Spread reactions");

                if (elements.Contains(ElementType.Pyro))
                    Console.WriteLine("• Burning (Dendro + Pyro): DoT Pyro damage");
            }

            // Team roles assessment
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nTeam Roles Assessment:");
            Console.ResetColor();

            // Simple heuristic to identify potential roles
            var potentialDPS = team
                .OrderByDescending(c => c.GetStatValue(StatType.Attack))
                .First();

            var potentialSupport = team
                .OrderByDescending(c => c.GetStatValue(StatType.EnergyRecharge))
                .FirstOrDefault(c => c != potentialDPS);

            var potentialHealer = team
                .FirstOrDefault(c => c.GetStatValue(StatType.HealingBonus) > 0);

            var potentialShielder = team
                .FirstOrDefault(c => c.Element == ElementType.Geo);

            Console.WriteLine($"• Potential Main DPS: {potentialDPS.Name}");

            if (potentialSupport != null)
                Console.WriteLine($"• Potential Support: {potentialSupport.Name}");

            if (potentialHealer != null)
                Console.WriteLine($"• Potential Healer: {potentialHealer.Name}");
            else
                Console.WriteLine("• No obvious healer in the team");

            if (potentialShielder != null)
                Console.WriteLine($"• Potential Shielder: {potentialShielder.Name}");

            // General team advice
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nTeam Synergy Assessment:");
            Console.ResetColor();

            int synergy = 0;

            // Check for elemental reactions
            if ((elements.Contains(ElementType.Pyro) && elements.Contains(ElementType.Hydro)) ||
                (elements.Contains(ElementType.Pyro) && elements.Contains(ElementType.Cryo)) ||
                (elements.Contains(ElementType.Electro) && elements.Contains(ElementType.Hydro)) ||
                (elements.Contains(ElementType.Cryo) && elements.Contains(ElementType.Hydro)))
                synergy += 2;

            // Check for resonance
            var elementCounts = team
                .Where(c => c.Element != ElementType.Unknown && c.Element != ElementType.Physical)
                .GroupBy(c => c.Element)
                .ToDictionary(g => g.Key, g => g.Count());

            if (elementCounts.Any(e => e.Value >= 2))
                synergy += 1;

            // Check for anemo support
            if (elements.Contains(ElementType.Anemo))
                synergy += 1;

            // Check for healer
            if (potentialHealer != null)
                synergy += 1;

            string synergyRating;
            if (synergy >= 4) synergyRating = "Excellent";
            else if (synergy >= 3) synergyRating = "Good";
            else if (synergy >= 2) synergyRating = "Average";
            else synergyRating = "Limited";

            Console.WriteLine($"Team Synergy Rating: {synergyRating}");

            // Team-specific advice
            Console.WriteLine("\nTeam Building Advice:");
            if (potentialHealer == null)
                Console.WriteLine("• Consider adding a character with healing capabilities");

            if (!elements.Contains(ElementType.Anemo) && !elements.Contains(ElementType.Geo))
                Console.WriteLine("• Consider adding Anemo or Geo support for utility");

            if (team.All(c => c.GetStatValue(StatType.EnergyRecharge) < 1.5))
                Console.WriteLine("• Your team may benefit from higher Energy Recharge for better Elemental Burst uptime");
        }

        private static void ResonanceAnalysis()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n=== Elemental Resonance Guide ===");
            Console.ResetColor();

            Console.WriteLine("Elemental Resonance effects are activated when you have 2 or more characters of the same element in your team.");

            Console.WriteLine("\nAvailable Resonance Effects:");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nPyro Resonance (2 Pyro characters):");
            Console.ResetColor();
            Console.WriteLine("• Fervent Flames: Increases ATK by 25%");
            Console.WriteLine("• Good for: Teams focusing on amplifying damage output");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nHydro Resonance (2 Hydro characters):");
            Console.ResetColor();
            Console.WriteLine("• Soothing Water: Increases HP by 25%");
            Console.WriteLine("• Good for: Teams with HP-scaling characters or healers");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\nElectro Resonance (2 Electro characters):");
            Console.ResetColor();
            Console.WriteLine("• High Voltage: Electro-related reactions generate an Electro particle (CD: 5s)");
            Console.WriteLine("• Good for: Teams needing Energy Recharge support");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nCryo Resonance (2 Cryo characters):");
            Console.ResetColor();
            Console.WriteLine("• Shattering Ice: Increases CRIT Rate against enemies affected by Cryo by 15%");
            Console.WriteLine("• Good for: Freeze teams and CRIT-focused builds");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nAnemo Resonance (2 Anemo characters):");
            Console.ResetColor();
            Console.WriteLine("• Impetuous Winds: Decreases Stamina Consumption by 15%, increases Movement SPD by 10%, and reduces Skill CD by 5%");
            Console.WriteLine("• Good for: Exploration and open-world gameplay");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nGeo Resonance (2 Geo characters):");
            Console.ResetColor();
            Console.WriteLine("• Enduring Rock: Increases Shield Strength by 15%. Characters protected by a shield deal 15% more DMG and have 15% RES to interruption");
            Console.WriteLine("• Good for: Shield-based teams and dealing consistent damage");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("\nDendro Resonance (2 Dendro characters):");
            Console.ResetColor();
            Console.WriteLine("• Sprawling Greenery: Elemental Mastery increased by 50. After triggering Burning, Quicken, or Bloom reactions, all nearby party members gain 30 Elemental Mastery for 6s");
            Console.WriteLine("• Good for: Reaction-based teams, especially Dendro reactions");

            // If user has characters, analyze current resonance options
            if (_lastFetchedCharacters.Count > 0)
            {
                Console.WriteLine("\n=== Your Current Resonance Options ===");

                // Group characters by element
                var charactersByElement = _lastFetchedCharacters
                    .Where(c => c.Element != ElementType.Unknown && c.Element != ElementType.Physical)
                    .GroupBy(c => c.Element)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var element in charactersByElement)
                {
                    if (element.Value.Count >= 2)
                    {
                        Console.ForegroundColor = GetColorForElement(element.Key);
                        Console.WriteLine($"\n{element.Key} Resonance is available:");
                        Console.ResetColor();

                        foreach (var character in element.Value)
                        {
                            Console.WriteLine($"• {character.Name} (Lv. {character.Level})");
                        }
                    }
                    else if (element.Value.Count == 1)
                    {
                        Console.WriteLine($"\n{element.Key}: You have 1 character ({element.Value[0].Name}), need 1 more for resonance");
                    }
                }

                // Check which resonances are completely unavailable
                var availableElements = charactersByElement.Keys.ToList();
                var allElements = Enum.GetValues(typeof(ElementType))
                    .Cast<ElementType>()
                    .Where(e => e != ElementType.Unknown && e != ElementType.Physical)
                    .ToList();

                var unavailableElements = allElements.Except(availableElements).ToList();

                if (unavailableElements.Any())
                {
                    Console.WriteLine("\nYou have no characters of these elements:");
                    foreach (var element in unavailableElements)
                    {
                        Console.WriteLine($"• {element}");
                    }
                }
            }
        }

        private static ConsoleColor GetColorForElement(ElementType element)
        {
            return element switch
            {
                ElementType.Pyro => ConsoleColor.Red,
                ElementType.Hydro => ConsoleColor.Blue,
                ElementType.Electro => ConsoleColor.Magenta,
                ElementType.Cryo => ConsoleColor.Cyan,
                ElementType.Anemo => ConsoleColor.Green,
                ElementType.Geo => ConsoleColor.Yellow,
                ElementType.Dendro => ConsoleColor.DarkGreen,
                _ => ConsoleColor.White
            };
        }

        private static void TeamRotationSimulator()
        {
            if (_lastFetchedCharacters.Count < 4)
            {
                Console.WriteLine("Need at least 4 characters for rotation simulation. Please fetch more character data.");
                return;
            }

            // Select team of 4 characters
            List<Character> team = SelectTeamOfFour("Select 4 characters for rotation simulation");
            if (team.Count < 4) return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n=== Team Rotation Simulator ===");
            Console.ResetColor();

            Console.WriteLine("This feature simulates a basic rotation for your team.");
            Console.WriteLine("Note: This is a simplified simulation and doesn't account for all game mechanics.\n");

            // Display team
            Console.WriteLine("Team Composition:");
            for (int i = 0; i < team.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {team[i].Name} ({team[i].Element})");
            }

            // Simple talent information
            Console.WriteLine("\nTalent Information:");
            for (int i = 0; i < team.Count; i++)
            {
                var character = team[i];
                Console.ForegroundColor = GetColorForElement(character.Element);
                Console.WriteLine($"\n{character.Name}:");
                Console.ResetColor();

                // Display available talents
                var normalAttack = character.Talents.FirstOrDefault();
                var elementalSkill = character.Talents.Skip(1).FirstOrDefault();
                var elementalBurst = character.Talents.Skip(2).FirstOrDefault();

                if (normalAttack != null)
                    Console.WriteLine($"• Normal/Charged Attack: {normalAttack.Name} (Lv. {normalAttack.Level})");
                else
                    Console.WriteLine("• Normal/Charged Attack: Unknown");

                if (elementalSkill != null)
                    Console.WriteLine($"• Elemental Skill: {elementalSkill.Name} (Lv. {elementalSkill.Level})");
                else
                    Console.WriteLine("• Elemental Skill: Unknown");

                if (elementalBurst != null)
                    Console.WriteLine($"• Elemental Burst: {elementalBurst.Name} (Lv. {elementalBurst.Level})");
                else
                    Console.WriteLine("• Elemental Burst: Unknown");
            }

            // Run simple rotation simulation
            Console.WriteLine("\nPress enter to start rotation simulation...");
            Console.ReadLine();

            SimulateRotation(team);
        }

        private static List<Character> SelectTeamOfFour(string prompt)
        {
            Console.WriteLine($"\n=== {prompt} ===");

            // Display available characters
            for (int i = 0; i < _lastFetchedCharacters.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_lastFetchedCharacters[i].Name} (Lv. {_lastFetchedCharacters[i].Level}, {_lastFetchedCharacters[i].Element})");
            }

            List<Character> team = new List<Character>();

            // Select 4 characters
            for (int i = 1; i <= 4; i++)
            {
                Console.Write($"\nSelect character {i} (1-{_lastFetchedCharacters.Count}, or 0 to cancel): ");

                if (!int.TryParse(Console.ReadLine(), out int charIndex) || charIndex < 0 || charIndex > _lastFetchedCharacters.Count)
                {
                    Console.WriteLine("Invalid selection. Try again.");
                    i--; // Retry this position
                    continue;
                }

                if (charIndex == 0)
                {
                    Console.WriteLine("Team selection canceled.");
                    return new List<Character>();
                }

                var selectedChar = _lastFetchedCharacters[charIndex - 1];

                if (team.Contains(selectedChar))
                {
                    Console.WriteLine("Character already in team. Try again.");
                    i--; // Retry this position
                    continue;
                }

                team.Add(selectedChar);
            }

            return team;
        }

        private static void SimulateRotation(List<Character> team)
        {
            Console.WriteLine("\n=== Rotation Simulation ===");
            Console.WriteLine("Simulating a standard rotation cycle...\n");

            // Energy levels
            Dictionary<Character, double> energyLevels = team.ToDictionary(c => c, _ => 0.0);
            Dictionary<Character, double> energyMax = team.ToDictionary(c => c, _ => 80.0); // Default energy cost

            // Cooldowns
            Dictionary<Character, int> skillCooldowns = team.ToDictionary(c => c, _ => 0);
            Dictionary<Character, int> burstCooldowns = team.ToDictionary(c => c, _ => 0);

            int currentCharIndex = 0;
            int turn = 1;
            int maxTurns = 12; // 12 turns for a full rotation example

            while (turn <= maxTurns)
            {
                var currentChar = team[currentCharIndex];

                Console.ForegroundColor = GetColorForElement(currentChar.Element);
                Console.WriteLine($"\nTurn {turn}: {currentChar.Name}'s action");
                Console.ResetColor();

                // Reduce cooldowns
                foreach (var charKey in skillCooldowns.Keys.ToList())
                {
                    if (skillCooldowns[charKey] > 0)
                        skillCooldowns[charKey]--;

                    if (burstCooldowns[charKey] > 0)
                        burstCooldowns[charKey]--;
                }

                // Check if burst is ready
                bool burstReady = energyLevels[currentChar] >= energyMax[currentChar] && burstCooldowns[currentChar] == 0;

                // Decide action
                if (burstReady)
                {
                    // Use burst
                    Console.WriteLine($"• {currentChar.Name} uses Elemental Burst!");
                    Console.WriteLine($"  Energy: {energyLevels[currentChar]:F1}/{energyMax[currentChar]:F1} → 0/{energyMax[currentChar]:F1}");

                    // Generate particles for team
                    foreach (var character in team)
                    {
                        double energyGain;
                        if (character == currentChar)
                            energyGain = 4.0; // On-field character gets more energy
                        else if (character.Element == currentChar.Element)
                            energyGain = 3.0; // Same element gets more energy
                        else
                            energyGain = 1.8; // Different element gets less

                        energyLevels[character] = Math.Min(energyLevels[character] + energyGain, energyMax[character]);

                        if (character != currentChar)
                            Console.WriteLine($"  {character.Name} gains {energyGain:F1} energy ({energyLevels[character]:F1}/{energyMax[character]:F1})");
                    }

                    // Apply cooldown
                    burstCooldowns[currentChar] = 15; // Generic burst cooldown
                    energyLevels[currentChar] = 0; // Reset energy
                }
                else if (skillCooldowns[currentChar] == 0)
                {
                    // Use skill
                    Console.WriteLine($"• {currentChar.Name} uses Elemental Skill!");

                    // Generate energy particles
                    double baseParticles = 3.0; // Most skills generate around 3 particles

                    // Energy calculation
                    foreach (var character in team)
                    {
                        double energyGain;
                        if (character == currentChar)
                            energyGain = baseParticles * 1.0; // On-field character
                        else if (character.Element == currentChar.Element)
                            energyGain = baseParticles * 0.6; // Off-field, same element
                        else
                            energyGain = baseParticles * 0.36; // Off-field, different element

                        energyLevels[character] = Math.Min(energyLevels[character] + energyGain, energyMax[character]);
                        Console.WriteLine($"  {character.Name}: Energy {(character == currentChar ? "" : "+")}{energyGain:F1} ({energyLevels[character]:F1}/{energyMax[character]:F1})");
                    }

                    // Apply cooldown
                    skillCooldowns[currentChar] = 8; // Generic skill cooldown
                }
                else
                {
                    // Use normal attacks
                    Console.WriteLine($"• {currentChar.Name} uses Normal Attacks");

                    // Small energy gain for current character
                    energyLevels[currentChar] = Math.Min(energyLevels[currentChar] + 1.0, energyMax[currentChar]);
                    Console.WriteLine($"  Energy: +1.0 ({energyLevels[currentChar]:F1}/{energyMax[currentChar]:F1})");
                }

                // Status of cooldowns if any
                if (skillCooldowns[currentChar] > 0)
                    Console.WriteLine($"  Skill cooldown: {skillCooldowns[currentChar]} turns remaining");
                if (burstCooldowns[currentChar] > 0)
                    Console.WriteLine($"  Burst cooldown: {burstCooldowns[currentChar]} turns remaining");

                // Next character
                currentCharIndex = (currentCharIndex + 1) % team.Count;
                turn++;

                if (turn <= maxTurns)
                {
                    Console.WriteLine("\nPress enter to continue simulation...");
                    Console.ReadLine();
                }
            }

            Console.WriteLine("\nRotation simulation complete!");
        }

        private static async Task ExportCurrentData()
        {
            if (_lastFetchedPlayerInfo == null && _lastFetchedCharacters.Count == 0)
            {
                Console.WriteLine("No data available to export. Please fetch data first.");
                return;
            }

            Console.WriteLine("\n=== Export Current Data ===");
            Console.WriteLine("1. Export as Text");
            Console.WriteLine("2. Export as JSON");
            Console.WriteLine("3. Export as CSV");
            Console.WriteLine("4. Back to Main Menu");
            Console.Write("\nEnter your choice (1-4): ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ExportAsText();
                    break;
                case "2":
                    ExportAsJson();
                    break;
                case "3":
                    ExportAsCsv();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Returning to main menu.");
                    return;
            }
        }

        private static void ExportAsText()
        {
            Console.Write("\nEnter filename to save (default: export.txt): ");
            string? filename = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filename))
                filename = "export.txt";

            if (!filename.EndsWith(".txt"))
                filename += ".txt";

            try
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    // Save in export-ready display mode
                    DisplayMode originalMode = _currentDisplayMode;
                    _currentDisplayMode = DisplayMode.ExportReady;

                    // Export player info if available
                    if (_lastFetchedPlayerInfo != null)
                    {
                        // Redirect console output to string
                        using (StringWriter stringWriter = new StringWriter())
                        {
                            // Save original output
                            TextWriter originalOutput = Console.Out;

                            try
                            {
                                Console.SetOut(stringWriter);
                                DisplayPlayerInfo(_lastFetchedPlayerInfo, _lastFetchedUid); // Pass UID
                                writer.Write(stringWriter.ToString());
                                writer.WriteLine(new string('-', 50));
                            }
                            finally
                            {
                                Console.SetOut(originalOutput);
                            }
                        }
                    }

                    // Export characters if available
                    if (_lastFetchedCharacters.Count > 0)
                    {
                        // Redirect console output to string
                        using (StringWriter stringWriter = new StringWriter())
                        {
                            // Save original output
                            TextWriter originalOutput = Console.Out;

                            try
                            {
                                Console.SetOut(stringWriter);
                                DisplayCharacters(_lastFetchedCharacters);
                                writer.Write(stringWriter.ToString());
                            }
                            finally
                            {
                                Console.SetOut(originalOutput);
                            }
                        }
                    }

                    // Restore original display mode
                    _currentDisplayMode = originalMode;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Data successfully exported to {filename}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error exporting data: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ExportAsJson()
        {
            Console.Write("\nEnter filename to save (default: export.json): ");
            string? filename = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filename))
                filename = "export.json";

            if (!filename.EndsWith(".json"))
                filename += ".json";

            try
            {
                var exportData = new
                {
                    ExportTime = DateTime.Now,
                    Uid = _lastFetchedUid, // Include UID in JSON export
                    PlayerInfo = _lastFetchedPlayerInfo,
                    Characters = _lastFetchedCharacters
                };

                string json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(filename, json);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Data successfully exported to {filename}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error exporting data: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void ExportAsCsv()
        {
            if (_lastFetchedCharacters.Count == 0)
            {
                Console.WriteLine("No character data available for CSV export.");
                return;
            }

            Console.Write("\nEnter filename to save (default: characters.csv): ");
            string? filename = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filename))
                filename = "characters.csv";

            if (!filename.EndsWith(".csv"))
                filename += ".csv";

            try
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    // Write header
                    writer.WriteLine("Name,Level,Ascension,Element,Constellation,Friendship,HP,ATK,DEF,CritRate,CritDMG,EnergyRecharge,ElementalMastery,Weapon,WeaponLevel,WeaponRefinement");

                    // Write character data
                    foreach (var character in _lastFetchedCharacters)
                    {
                        string weaponName = character.Weapon?.Name ?? "None";
                        string weaponLevel = character.Weapon != null ? character.Weapon.Level.ToString() : "0";
                        string weaponRefinement = character.Weapon != null ? character.Weapon.Refinement.ToString() : "0";

                        writer.WriteLine(
                            $"{character.Name}," +
                            $"{character.Level}," +
                            $"{character.Ascension}," +
                            $"{character.Element}," +
                            $"{character.ConstellationLevel}," +
                            $"{character.Friendship}," +
                            $"{character.GetStatValue(StatType.HP):F0}," +
                            $"{character.GetStatValue(StatType.Attack):F0}," +
                            $"{character.GetStatValue(StatType.Defense):F0}," +
                            $"{character.GetStatValue(StatType.CriticalRate) * 100:F1}," +
                            $"{character.GetStatValue(StatType.CriticalDamage) * 100:F1}," +
                            $"{character.GetStatValue(StatType.EnergyRecharge) * 100:F1}," +
                            $"{character.GetStatValue(StatType.ElementalMastery):F0}," +
                            $"{weaponName}," +
                            $"{weaponLevel}," +
                            $"{weaponRefinement}"
                        );
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Data successfully exported to {filename}");
                Console.ResetColor();

                // Offer to export artifacts separately
                Console.Write("\nDo you want to export artifact details separately? (y/n): ");
                string? response = Console.ReadLine()?.ToLower();

                if (response == "y" || response == "yes")
                {
                    string artifactFilename = filename.Replace(".csv", "_artifacts.csv");

                    using (StreamWriter writer = new StreamWriter(artifactFilename))
                    {
                        // Write header
                        writer.WriteLine("Character,ArtifactName,Slot,Rarity,Level,SetName,MainStat,MainStatValue,Substat1,Value1,Substat2,Value2,Substat3,Value3,Substat4,Value4");

                        // Write artifact data
                        foreach (var character in _lastFetchedCharacters)
                        {
                            foreach (var artifact in character.Artifacts)
                            {
                                // Process main stat
                                string mainStatType = artifact.MainStat?.Type.ToString() ?? "None";
                                string mainStatValue = artifact.MainStat?.Value.ToString("F1") ?? "0";

                                // Process substats (up to 4)
                                string[] substatTypes = new string[4];
                                string[] substatValues = new string[4];

                                for (int i = 0; i < 4; i++)
                                {
                                    if (i < artifact.SubStats.Count)
                                    {
                                        substatTypes[i] = artifact.SubStats[i].Type.ToString();
                                        substatValues[i] = artifact.SubStats[i].Value.ToString("F1");
                                    }
                                    else
                                    {
                                        substatTypes[i] = "";
                                        substatValues[i] = "";
                                    }
                                }

                                writer.WriteLine(
                                    $"{character.Name}," +
                                    $"{artifact.Name}," +
                                    $"{artifact.Slot}," +
                                    $"{artifact.Rarity}," +
                                    $"{artifact.Level}," +
                                    $"{artifact.SetName}," +
                                    $"{mainStatType}," +
                                    $"{mainStatValue}," +
                                    $"{substatTypes[0]}," +
                                    $"{substatValues[0]}," +
                                    $"{substatTypes[1]}," +
                                    $"{substatValues[1]}," +
                                    $"{substatTypes[2]}," +
                                    $"{substatValues[2]}," +
                                    $"{substatTypes[3]}," +
                                    $"{substatValues[3]}"
                                );
                            }
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Artifact data exported to {artifactFilename}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error exporting data: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void DisplayDebugInfo()
        {
            Console.WriteLine("\n=== Debug Information ===");
            Console.WriteLine($"Client Initialized: {(_client != null ? "Yes" : "No")}");
            Console.WriteLine($"Current Language: {_currentLanguage}");
            Console.WriteLine($"Assets Path: {_assetsPath}");
            Console.WriteLine($"Current Display Mode: {_currentDisplayMode}");
            Console.WriteLine($"Player Data Available: {(_lastFetchedPlayerInfo != null ? $"Yes (UID: {_lastFetchedUid})" : "No")}"); // Show UID
            Console.WriteLine($"Character Data Available: {(_lastFetchedCharacters.Count > 0 ? $"Yes ({_lastFetchedCharacters.Count} characters)" : "No")}");

            if (_client != null)
            {
                try
                {
                    // Correctly call GetCacheStats and access tuple properties
                    var cacheStats = _client.GetCacheStats();
                    Console.WriteLine($"HTTP Cache Stats: {cacheStats.Count} entries ({cacheStats.ExpiredCount} expired)");
                }
                catch (Exception ex) // Catch potential exceptions if HttpHelper doesn't implement the method yet
                {
                    Console.WriteLine($"Unable to retrieve cache statistics: {ex.Message}");
                }
            }

            Console.WriteLine("\nSystem Information:");
            Console.WriteLine($"OS: {Environment.OSVersion}");
            Console.WriteLine($"64-bit Process: {Environment.Is64BitProcess}");
            Console.WriteLine($"Processor Count: {Environment.ProcessorCount}");
            Console.WriteLine($".NET Version: {Environment.Version}");

            // Memory usage
            long memoryUsed = GC.GetTotalMemory(false) / 1024 / 1024;
            Console.WriteLine($"Approximate Memory Usage: {memoryUsed} MB");

            if (_client != null)
            {
                // Offer some debug actions
                Console.WriteLine("\nDebug Actions:");
                Console.WriteLine("1. Clear HTTP Cache");
                Console.WriteLine("2. Force Garbage Collection");
                Console.WriteLine("3. Back to Main Menu");
                Console.Write("\nEnter your choice (1-3): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        try
                        {
                            // Correctly call ClearCache
                            _client.ClearCache();
                            Console.WriteLine("HTTP Cache cleared");
                        }
                        catch (Exception ex) // Catch potential exceptions
                        {
                            Console.WriteLine($"Error clearing cache: {ex.Message}");
                        }
                        break;
                    case "2":
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        Console.WriteLine("Garbage collection performed");
                        break;
                    case "3":
                    default:
                        return;
                }
            }
        }

        private static async Task<bool> FetchProfile(bool includeCharacters)
        {
            if (_client == null)
            {
                Console.WriteLine("Client not initialized. Please restart the application.");
                return false;
            }

            Console.Write("\nEnter Genshin Impact UID: ");
            string? uidString = Console.ReadLine();

            if (!int.TryParse(uidString, out int uid))
            {
                Console.WriteLine("Invalid UID format. Please enter a numeric UID.");
                return false;
            }

            Console.Write("Bypass cache? (y/n, default: n): ");
            string? bypassInput = Console.ReadLine()?.ToLower();
            bool bypassCache = bypassInput == "y" || bypassInput == "yes";

            Console.WriteLine($"\nFetching player {(includeCharacters ? "profile" : "info")} for UID {uid} (Bypass cache: {bypassCache})...");

            try
            {
                if (includeCharacters)
                {
                    // Fetch full profile (player info + characters)
                    var (playerInfo, characters) = await _client.GetUserProfile(uid, bypassCache);
                    _lastFetchedPlayerInfo = playerInfo;
                    _lastFetchedCharacters = characters;
                    _lastFetchedUid = uid; // Store the successfully fetched UID

                    DisplayPlayerInfo(playerInfo, uid); // Pass UID

                    if (characters.Count > 0)
                    {
                        Console.WriteLine($"\nFound {characters.Count} character(s)");
                        DisplayCharacters(characters);
                    }
                    else
                    {
                        Console.WriteLine("\nNo character details available. Character showcases may be hidden.");
                    }
                }
                else
                {
                    // Fetch player info only
                    var playerInfo = await _client.GetPlayerInfoAsync(uid, bypassCache);
                    _lastFetchedPlayerInfo = playerInfo;
                    _lastFetchedUid = uid; // Store the successfully fetched UID
                    _lastFetchedCharacters.Clear(); // Clear characters if only fetching player info

                    DisplayPlayerInfo(playerInfo, uid); // Pass UID
                }

                return true;
            }
            catch (ProfilePrivateException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Profile is private or character details are hidden.");
                Console.ResetColor();
            }
            catch (PlayerNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Player with UID {uid} not found.");
                Console.ResetColor();
            }
            catch (EnkaNetworkException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"API Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Console.ResetColor();
            }

            return false;
        }

        // Modified to accept UID
        private static void DisplayPlayerInfo(PlayerInfo playerInfo, int uid)
        {
            var originalColor = Console.ForegroundColor;

            if (_currentDisplayMode == DisplayMode.ExportReady)
            {
                Console.WriteLine("PLAYER INFORMATION");
                Console.WriteLine($"UID: {uid}");
                Console.WriteLine($"Nickname: {playerInfo.Nickname}");
                Console.WriteLine($"Adventure Rank: {playerInfo.Level}");
                Console.WriteLine($"World Level: {playerInfo.WorldLevel}");

                if (!string.IsNullOrEmpty(playerInfo.Signature))
                {
                    Console.WriteLine($"Signature: {playerInfo.Signature}");
                }

                if (!string.IsNullOrEmpty(playerInfo.NameCardIcon))
                {
                    Console.WriteLine($"Profile Name Card Icon: {playerInfo.NameCardIcon} (ID: {playerInfo.NameCardId})");
                }
                Console.WriteLine($"Achievements: {playerInfo.FinishedAchievements}");
                Console.WriteLine($"Spiral Abyss: Floor {playerInfo.TowerFloor} Chamber {playerInfo.TowerChamber}");
                Console.WriteLine($"Name Card ID: {playerInfo.NameCardId}");
                Console.WriteLine($"Profile Picture Character ID: {playerInfo.ProfilePictureCharacterId}");

                if (playerInfo.ShowcaseNameCards.Count > 0)
                {
                    Console.WriteLine("Showcase Name Cards:");
                    foreach (var nameCard in playerInfo.ShowcaseNameCards)
                    {
                        Console.WriteLine($" - ID: {nameCard.Id}, Icon: {nameCard.Icon}");
                    }
                }

                return;
            }

            bool isCompact = _currentDisplayMode == DisplayMode.Compact;

            if (isCompact)
            {
                Console.WriteLine($"\nPlayer: {playerInfo.Nickname} (UID: {uid}, AR {playerInfo.Level}, WL {playerInfo.WorldLevel})"); // Show UID
                if (!string.IsNullOrEmpty(playerInfo.Signature))
                    Console.WriteLine($"Signature: \"{playerInfo.Signature}\"");

                Console.WriteLine($"Spiral Abyss: Floor {playerInfo.TowerFloor}-{playerInfo.TowerChamber}, Achievements: {playerInfo.FinishedAchievements}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n=== Player Information ===");
                Console.ResetColor();

                Console.WriteLine($"UID: {uid}"); // Show UID
                Console.WriteLine($"Nickname: {playerInfo.Nickname}");
                Console.WriteLine($"Adventure Rank: {playerInfo.Level}");
                Console.WriteLine($"World Level: {playerInfo.WorldLevel}");

                if (!string.IsNullOrEmpty(playerInfo.Signature))
                    Console.WriteLine($"Signature: \"{playerInfo.Signature}\"");

                Console.WriteLine($"Achievements Completed: {playerInfo.FinishedAchievements}");
                Console.WriteLine($"Spiral Abyss Progress: Floor {playerInfo.TowerFloor}, Chamber {playerInfo.TowerChamber}");

                if (_currentDisplayMode == DisplayMode.Detailed)
                {
                    if (!string.IsNullOrEmpty(playerInfo.NameCardIcon))
                    {
                        Console.WriteLine($"Profile Name Card Icon: {playerInfo.NameCardIcon} (ID: {playerInfo.NameCardId})");
                    }

                    Console.WriteLine($"Profile Picture Character ID: {playerInfo.ProfilePictureCharacterId}");

                    if (playerInfo.ShowcaseCharacterIds.Count > 0)
                    {
                        Console.WriteLine("Showcase Character IDs: " + string.Join(", ", playerInfo.ShowcaseCharacterIds));
                    }

                    if (playerInfo.ShowcaseNameCards.Count > 0)
                    {
                        Console.WriteLine("Showcase Name Cards:");
                        foreach (var nameCard in playerInfo.ShowcaseNameCards)
                        {
                            Console.WriteLine($" - ID: {nameCard.Id}, Icon: {nameCard.Icon}");
                        }
                    }
                }
            }

            Console.ForegroundColor = originalColor;
        }

        private static async Task FetchCharacters()
        {
            if (_client == null)
            {
                Console.WriteLine("Client not initialized. Please restart the application.");
                return;
            }

            Console.Write("\nEnter Genshin Impact UID: ");
            string? uidString = Console.ReadLine();

            if (!int.TryParse(uidString, out int uid))
            {
                Console.WriteLine("Invalid UID format. Please enter a numeric UID.");
                return;
            }

            Console.Write("Bypass cache? (y/n, default: n): ");
            string? bypassInput = Console.ReadLine()?.ToLower();
            bool bypassCache = bypassInput == "y" || bypassInput == "yes";

            Console.WriteLine($"\nFetching character details for UID {uid} (Bypass cache: {bypassCache})...");

            try
            {
                var characters = await _client.GetCharactersAsync(uid, bypassCache);
                _lastFetchedCharacters = characters;
                _lastFetchedUid = uid; // Store UID even if only fetching characters
                _lastFetchedPlayerInfo = null; // Clear player info if only fetching characters

                Console.WriteLine($"Found {characters.Count} character(s)");
                DisplayCharacters(characters);
            }
            catch (ProfilePrivateException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Profile is private or character details are hidden.");
                Console.ResetColor();
            }
            catch (PlayerNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Player with UID {uid} not found.");
                Console.ResetColor();
            }
            catch (EnkaNetworkException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"API Error: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.ResetColor();
            }
        }

        private static void DisplayCharacters(List<Character> characters)
        {
            if (characters.Count == 0)
            {
                Console.WriteLine("No characters to display.");
                return;
            }

            if (_currentDisplayMode == DisplayMode.ExportReady)
            {
                Console.WriteLine("\nCHARACTER INFORMATION");

                foreach (var character in characters)
                {
                    Console.WriteLine(new string('-', 50));
                    Console.WriteLine($"Name: {character.Name} (ID: {character.Id})");
                    Console.WriteLine($"Level: {character.Level}/{GetMaxLevel(character.Ascension)} (Ascension {character.Ascension})");
                    Console.WriteLine($"Element: {character.Element}");
                    Console.WriteLine($"Constellation: C{character.ConstellationLevel}");
                    Console.WriteLine($"Friendship: {character.Friendship}");

                    if (character.Weapon != null)
                    {
                        Console.WriteLine($"Weapon: {character.Weapon.Name} (Lv. {character.Weapon.Level}, R{character.Weapon.Refinement})");
                    }

                    Console.WriteLine("\nSTATS:");
                    Console.WriteLine($"HP: {character.GetStatValue(StatType.HP):F0}");
                    Console.WriteLine($"ATK: {character.GetStatValue(StatType.Attack):F0}");
                    Console.WriteLine($"DEF: {character.GetStatValue(StatType.Defense):F0}");
                    Console.WriteLine($"CRIT Rate: {character.GetStatValue(StatType.CriticalRate):P1}");
                    Console.WriteLine($"CRIT DMG: {character.GetStatValue(StatType.CriticalDamage):P1}");
                    Console.WriteLine($"Energy Recharge: {character.GetStatValue(StatType.EnergyRecharge):P1}");
                    Console.WriteLine($"Elemental Mastery: {character.GetStatValue(StatType.ElementalMastery):F0}");

                    if (character.Artifacts.Count > 0)
                    {
                        Console.WriteLine("\nARTIFACTS:");
                        foreach (var artifact in character.Artifacts)
                        {
                            Console.WriteLine($"{artifact.Slot}: {artifact.Name} ({artifact.Rarity}★, +{artifact.Level}) - {artifact.SetName}");
                            if (artifact.MainStat != null)
                                Console.WriteLine($"  Main: {artifact.MainStat}");

                            if (artifact.SubStats.Count > 0)
                            {
                                Console.WriteLine("  Substats:");
                                foreach (var substat in artifact.SubStats)
                                {
                                    Console.WriteLine($"    {substat}");
                                }
                            }
                        }
                    }

                    if (character.Talents.Count > 0)
                    {
                        Console.WriteLine("\nTALENTS:");
                        foreach (var talent in character.Talents)
                        {
                            Console.WriteLine($"{talent.Name}: Lv. {talent.Level}");
                        }
                    }
                }
                return;
            }

            bool isArtifactsOnly = _currentDisplayMode == DisplayMode.ArtifactsOnly;
            bool isStatsOnly = _currentDisplayMode == DisplayMode.StatsOnly;
            bool isCompact = _currentDisplayMode == DisplayMode.Compact;
            bool isDetailed = _currentDisplayMode == DisplayMode.Detailed;

            foreach (var character in characters)
            {
                // Character header
                Console.ForegroundColor = GetColorForElement(character.Element);
                if (!isArtifactsOnly && !isStatsOnly)
                {
                    Console.WriteLine($"\n=== {character.Name} ===");
                }
                else if (isArtifactsOnly)
                {
                    Console.WriteLine($"\n=== {character.Name}'s Artifacts ===");
                }
                else if (isStatsOnly)
                {
                    Console.WriteLine($"\n=== {character.Name}'s Stats ===");
                }
                Console.ResetColor();

                // Basic character info (unless artifacts or stats only)
                if (!isArtifactsOnly && !isStatsOnly)
                {
                    if (isCompact)
                    {
                        Console.WriteLine($"Lv. {character.Level}/{GetMaxLevel(character.Ascension)} {character.Element} (C{character.ConstellationLevel})");

                        if (character.Weapon != null)
                        {
                            Console.WriteLine($"Weapon: {character.Weapon.Name} R{character.Weapon.Refinement} Lv. {character.Weapon.Level}/{GetWeaponMaxLevel(character.Weapon.Ascension)}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Level: {character.Level}/{GetMaxLevel(character.Ascension)} (Ascension {character.Ascension})");
                        Console.WriteLine($"Element: {character.Element}");
                        Console.WriteLine($"Constellation: C{character.ConstellationLevel}");
                        Console.WriteLine($"Friendship Level: {character.Friendship}");

                        if (character.Weapon != null)
                        {
                            Console.WriteLine($"Weapon: {character.Weapon.Name} ({character.Weapon.Rarity}★) - Lv. {character.Weapon.Level}/{GetWeaponMaxLevel(character.Weapon.Ascension)} R{character.Weapon.Refinement}");

                            if (isDetailed && character.Weapon.SecondaryStat != null)
                            {
                                Console.WriteLine($"• {character.Weapon.SecondaryStat}");
                            }
                        }

                        if (isDetailed && character.Talents.Count > 0)
                        {
                            Console.WriteLine("\nTalents:");
                            foreach (var talent in character.Talents)
                            {
                                Console.WriteLine($"• {talent.Name}: Lv. {talent.Level}" +
                                    (talent.ExtraLevel > 0 ? $" (Base {talent.BaseLevel} + {talent.ExtraLevel})" : ""));
                            }
                        }
                    }
                }

                // Stats (unless artifacts only)
                if (!isArtifactsOnly)
                {
                    if (!isCompact || isStatsOnly)
                    {
                        Console.WriteLine("\nStats:");
                    }

                    if (isCompact && !isStatsOnly)
                    {
                        // Compact mode shows just the most important stats on one line
                        Console.WriteLine($"HP: {character.GetStatValue(StatType.HP):N0} | " +
                                          $"ATK: {character.GetStatValue(StatType.Attack):N0} | " +
                                          $"CR/CD: {character.GetStatValue(StatType.CriticalRate):P1}/{character.GetStatValue(StatType.CriticalDamage):P1} | " +
                                          $"EM: {character.GetStatValue(StatType.ElementalMastery):F0}");
                    }
                    else
                    {
                        // Show full stats
                        DisplayStat(character, StatType.HP, "HP");
                        DisplayStat(character, StatType.Attack, "ATK");
                        DisplayStat(character, StatType.Defense, "DEF");
                        DisplayStat(character, StatType.CriticalRate, "CRIT Rate", true);
                        DisplayStat(character, StatType.CriticalDamage, "CRIT DMG", true);
                        DisplayStat(character, StatType.EnergyRecharge, "Energy Recharge", true);
                        DisplayStat(character, StatType.ElementalMastery, "Elemental Mastery");

                        // Element-specific damage bonus
                        if (character.Element != ElementType.Unknown && character.Element != ElementType.Physical)
                        {
                            StatType elementDamageType = GetElementalDmgType(character.Element);
                            string elementName = character.Element.ToString();
                            DisplayStat(character, elementDamageType, $"{elementName} DMG Bonus", true);
                        }

                        // Additional detailed stats
                        if (isDetailed || isStatsOnly)
                        {
                            DisplayStat(character, StatType.PhysicalDamageBonus, "Physical DMG Bonus", true);
                            DisplayStat(character, StatType.HealingBonus, "Healing Bonus", true);

                            // Crit value
                            double cv = character.GetStatValue(StatType.CriticalRate) * 2 + character.GetStatValue(StatType.CriticalDamage);
                            Console.WriteLine($"Critical Value: {cv * 100:F1} CV");
                        }
                    }
                }

                // Artifacts (unless stats only)
                if (!isStatsOnly && character.Artifacts.Count > 0)
                {
                    // Group by sets
                    var artifactSetCounts = character.Artifacts
                        .GroupBy(a => a.SetName)
                        .ToDictionary(g => g.Key, g => g.Count());

                    // Display set bonuses
                    if (!isCompact || isArtifactsOnly)
                    {
                        Console.WriteLine("\nArtifact Sets:");
                        foreach (var set in artifactSetCounts)
                        {
                            Console.WriteLine($"• {set.Key}: {set.Value} piece(s)");
                        }
                    }

                    // Display individual artifacts
                    if (!isCompact || isArtifactsOnly)
                    {
                        Console.WriteLine("\nEquipped Artifacts:");

                        foreach (var artifact in character.Artifacts.OrderBy(a => (int)a.Slot))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"• {artifact.Slot}: {artifact.Name}");
                            Console.ResetColor();
                            Console.WriteLine($" ({artifact.Rarity}★, +{artifact.Level})");

                            if (artifact.MainStat != null)
                            {
                                Console.WriteLine($"  Main: {artifact.MainStat}");
                            }

                            if (artifact.SubStats.Count > 0)
                            {
                                foreach (var substat in artifact.SubStats)
                                {
                                    Console.WriteLine($"  • {substat}");
                                }
                            }
                        }
                    }
                }
                else if (!isStatsOnly && character.Artifacts.Count == 0)
                {
                    Console.WriteLine("No artifacts equipped.");
                }
            }
        }

        private static void DisplayStat(Character character, StatType statType, string label, bool isPercentage = false)
        {
            double value = character.GetStatValue(statType);

            string formattedValue;
            if (isPercentage)
            {
                formattedValue = $"{value:P1}";
            }
            else if (value == (int)value)
            {
                formattedValue = $"{value:N0}";
            }
            else
            {
                formattedValue = $"{value:N1}";
            }

            Console.WriteLine($"{label}: {formattedValue}");
        }

        private static int GetMaxLevel(int ascension)
        {
            return ascension switch
            {
                0 => 20,
                1 => 40,
                2 => 50,
                3 => 60,
                4 => 70,
                5 => 80,
                6 => 90,
                _ => 90
            };
        }

        private static int GetWeaponMaxLevel(int ascension)
        {
            return ascension switch
            {
                0 => 20,
                1 => 40,
                2 => 50,
                3 => 60,
                4 => 70,
                5 => 90, // Fixed: Max level for ascension 5 is 90
                _ => 90
            };
        }

        private static async Task ChangeLanguage()
        {
            Console.WriteLine("\n=== Available Languages ===");
            Console.WriteLine("1. English (en)");
            Console.WriteLine("2. Simplified Chinese (zh-CN)");
            Console.WriteLine("3. Traditional Chinese (zh-TW)");
            Console.WriteLine("4. German (de)");
            Console.WriteLine("5. Spanish (es)");
            Console.WriteLine("6. French (fr)");
            Console.WriteLine("7. Indonesian (id)");
            Console.WriteLine("8. Italian (it)");
            Console.WriteLine("9. Japanese (ja)");
            Console.WriteLine("10. Korean (ko)");
            Console.WriteLine("11. Portuguese (pt)");
            Console.WriteLine("12. Russian (ru)");
            Console.WriteLine("13. Thai (th)");
            Console.WriteLine("14. Turkish (tr)");
            Console.WriteLine("15. Vietnamese (vi)");
            Console.Write("\nEnter your choice (1-15): ");

            int choice;
            if (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 15)
            {
                Console.WriteLine("Invalid choice. Language unchanged.");
                return;
            }

            string newLanguage = choice switch
            {
                1 => "en",
                2 => "zh-CN",
                3 => "zh-TW",
                4 => "de",
                5 => "es",
                6 => "fr",
                7 => "id",
                8 => "it",
                9 => "ja",
                10 => "ko",
                11 => "pt",
                12 => "ru",
                13 => "th",
                14 => "tr",
                15 => "vi",
                _ => "en"
            };

            if (newLanguage == _currentLanguage)
            {
                Console.WriteLine($"Language is already set to {newLanguage}.");
                return;
            }

            _currentLanguage = newLanguage;
            Console.WriteLine($"Language changed to {_currentLanguage}");

            // Reinitialize client with new language
            Console.WriteLine("Reinitializing client with new language...");
            _client?.Dispose();
            _client = null;
            await InitializeClient();
        }

        private static async Task ChangeAssetsPath()
        {
            Console.Write("\nEnter new assets path (leave blank to keep current): ");
            string? newPath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newPath))
            {
                Console.WriteLine("Assets path unchanged.");
                return;
            }

            _assetsPath = newPath;
            Console.WriteLine($"Assets path changed to: {_assetsPath}");

            // Reinitialize client with new assets path
            Console.WriteLine("Reinitializing client with new assets path...");
            _client?.Dispose();
            _client = null;
            await InitializeClient();
        }
    }
}
