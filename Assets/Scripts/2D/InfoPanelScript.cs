using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelScript : MonoBehaviour
{
    public Text InfoText;

    private bool _infoTextMinimized = false;

    public void MinimizeInfoText(bool state)
    {
        _infoTextMinimized = state;
    }

    /// <summary>
    /// Updates the upper left corner info panel
    /// </summary>
    public void UpdateInfoPanel()
    {
        World world = Manager.CurrentWorld;

        if (Manager.GameMode == GameMode.Simulator)
        {
            InfoText.text = Manager.GetDateString(world.CurrentDate);
        }
        else if (Manager.GameMode == GameMode.Editor)
        {
            InfoText.text = "Map Editor Mode";
        }

        if (_infoTextMinimized)
            return;

        if (Manager.CurrentWorld.SelectedCell != null)
        {
            AddCellDataToInfoPanel(Manager.CurrentWorld.SelectedCell);
        }

        InfoText.text += "\n";

        if (Manager.CurrentDevMode != DevMode.None)
        {
            InfoText.text += "\n -- Debug Data -- ";

            if ((Manager.CurrentWorld != null) &&
                (Manager.CurrentWorld.SelectedTerritory != null))
            {
                InfoText.text += "\n";
                InfoText.text += "\nSelected Territory's Polity Id: " +
                    Manager.CurrentWorld.SelectedTerritory.Polity.Id;
            }

            InfoText.text += "\n";
            InfoText.text += "\nEvents Evaluated Per Second: " + Manager.LastEventsEvaluatedCount;
            InfoText.text += "\nEvents Triggered Per Second: " + Manager.LastEventsTriggeredCount;

            float successRate = 0;
            if (Manager.LastEventsEvaluatedCount > 0)
            {
                successRate = Manager.LastEventsTriggeredCount / (float)Manager.LastEventsEvaluatedCount;
            }

            InfoText.text += "\nSuccess Rate: " + successRate.ToString("P");

            if ((Manager.CurrentDevMode == DevMode.Advanced) &&
                (Manager.LastEventEvalStatsPerType.Count > 0))
            {
                InfoText.text += "\n\n-- Breakdown by type --\n";

                foreach (KeyValuePair<string, World.EventEvalStats> pair in Manager.LastEventEvalStatsPerType)
                {
                    int evalCount = pair.Value.EvaluationCount;

                    if (evalCount <= 0)
                    {
                        throw new System.Exception("Event of type ("+ pair.Key + ") has 0 evaluations");
                    }

                    float percentOfTotal = evalCount / (float)Manager.LastEventsEvaluatedCount;

                    successRate = pair.Value.TriggerCount / (float)evalCount;

                    InfoText.text += "\n" + pair.Key + ": ";
                    InfoText.text += "\nEvaluated: " + pair.Value.EvaluationCount +
                        " (" + percentOfTotal.ToString("P") + " of all evaluated events)";
                    InfoText.text += "\nTriggered: " + pair.Value.TriggerCount +
                        " (" + successRate.ToString("P") + " success rate)";
                    InfoText.text += "\n";
                }
            }

            InfoText.text += "\nMap Updates Per Second: " + Manager.LastMapUpdateCount;
            InfoText.text += "\nPixel Updates Per Second: " + Manager.LastPixelUpdateCount;

            if (Manager.LastMapUpdateCount > 0)
            {
                float pixelUpdatesPerMapUpdate = Manager.LastPixelUpdateCount / (float)Manager.LastMapUpdateCount;

                InfoText.text +=
                    "\nPixel Updates per Map Update: " + pixelUpdatesPerMapUpdate.ToString("0.00");
            }

            InfoText.text += "\n";
            InfoText.text += "\nSimulated Time per RTS:";
            InfoText.text += "\n" + Manager.GetTimeSpanString(Manager.LastDevModeDateSpan);
            InfoText.text += "\n";
        }
    }

    private void AddCellDataToInfoPanel(int longitude, int latitude)
    {
        TerrainCell cell = Manager.CurrentWorld.GetCell(longitude, latitude);

        if (cell == null) return;

        AddCellDataToInfoPanel(cell);
    }

    private void AddCellDataToInfoPanel_Terrain(TerrainCell cell)
    {
        float cellArea = cell.Area;

        InfoText.text += "\n";
        InfoText.text += "\n -- Cell Terrain Data -- ";
        InfoText.text += "\n";

        InfoText.text += "\nArea: " + cellArea + " Km^2";
        InfoText.text += "\nAltitude: " + cell.Altitude + " meters";
        InfoText.text += "\nRainfall: " + cell.Rainfall + " mm / year";
        InfoText.text += "\nWater Accumulation: " + cell.WaterAccumulation + " mm";
        InfoText.text += "\nTemperature: " + cell.Temperature + " C";
        InfoText.text += "\n";

        if (cell.PresentLayerIds.Count > 0)
        {
            for (int i = 0; i < cell.PresentLayerIds.Count; i++)
            {
                Layer layer = Layer.Layers[cell.PresentLayerIds[i]];
                float value = cell.LayerData[i].Value * layer.MaxPossibleValue;

                if (value <= 0) continue;

                InfoText.text += "\nLayer: " + layer.Name.FirstLetterToUpper();
                InfoText.text += ": " + value + " " + layer.Units;
            }

            InfoText.text += "\n";
        }

        InfoText.text += "\nBiomes:";

        float farmlandPercent = cell.FarmlandPercentage;
        float wildernessPercent = 1 - farmlandPercent;

        for (int i = 0; i < cell.PresentBiomeIds.Count; i++)
        {
            float percentage = cell.BiomePresences[i] * wildernessPercent;

            Biome biome = Biome.Biomes[cell.PresentBiomeIds[i]];

            InfoText.text += "\n\t" + biome.Name.FirstLetterToUpper() + ":";
            InfoText.text += " (" + percentage.ToString("P") + ")";
        }

        if (farmlandPercent > 0)
        {
            InfoText.text += "\n\tFarmland:";
            InfoText.text += " (" + farmlandPercent.ToString("P") + ")";
        }

        InfoText.text += "\n";
        InfoText.text += "\nSurvivability: " + cell.Survivability.ToString("P");
        InfoText.text += "\nForaging Capacity: " + cell.ForagingCapacity.ToString("P");
        InfoText.text += "\nAccessibility: " + cell.Accessibility.ToString("P");
        if (cell.Accessibility != cell.BaseAccessibility)
            InfoText.text += "\nOriginal Accessibility: " + cell.BaseAccessibility.ToString("P");
        InfoText.text += "\nArability: " + cell.Arability.ToString("P");
        if (cell.Arability != cell.BaseArability)
            InfoText.text += "\nOriginal Arability: " + cell.BaseArability.ToString("P");
        InfoText.text += "\nHilliness: " + cell.Hilliness.ToString("P");
        InfoText.text += "\n";

        Region region = cell.Region;

        if (region == null)
        {
            InfoText.text += "\nCell doesn't belong to any known region";
        }
        else
        {
            InfoText.text += "\nCell is part of Region Id: " + region.Id + ", Name: " + region.Name;
        }
    }

    private void AddCellDataToInfoPanel_Region(TerrainCell cell)
    {
        Region region = cell.Region;

        InfoText.text += "\n";
        InfoText.text += "\n -- Region Terrain Data -- ";
        InfoText.text += "\n";

        if (region == null)
        {
            InfoText.text += "\nCell doesn't belong to any known region";

            return;
        }
        else
        {
            InfoText.text += "\nRegion Id: " + region.Id + ", Name: " + region.Name;
        }
        InfoText.text += "\n";
        InfoText.text += "\nAttributes: ";

        bool first = true;
        foreach (RegionAttribute.Instance attr in region.Attributes.Values)
        {
            if (first)
            {
                InfoText.text += attr.Name;
                first = false;
            }
            else
            {
                InfoText.text += ", " + attr.Name;
            }
        }

        InfoText.text += "\n";
        InfoText.text += "\nTotal Area: " + region.TotalArea + " Km^2";

        InfoText.text += "\n";
        InfoText.text += "\nCoast Percentage: " + region.CoastPercentage.ToString("P");
        InfoText.text += "\nWater Percentage: " + region.WaterPercentage.ToString("P");

        InfoText.text += "\n";
        InfoText.text += "\nAverage Altitude: " + region.AverageAltitude + " meters";
        InfoText.text += "\nAverage Rainfall: " + region.AverageRainfall + " mm / year";
        InfoText.text += "\nAverage Flowing Water: " + region.AverageFlowingWater + " mm";
        InfoText.text += "\nAverage Temperature: " + region.AverageTemperature + " C";
        InfoText.text += "\n";

        InfoText.text += "\nMin Region Altitude: " + region.MinAltitude + " meters";
        InfoText.text += "\nMax Region Altitude: " + region.MaxAltitude + " meters";
        InfoText.text += "\nAverage Border Altitude: " + region.AverageOuterBorderAltitude + " meters";
        InfoText.text += "\n";

        for (int i = 0; i < region.PresentBiomeIds.Count; i++)
        {
            float percentage = region.BiomePresences[i];

            InfoText.text += "\nBiome: " + region.PresentBiomeIds[i];
            InfoText.text += " (" + percentage.ToString("P") + ")";
        }

        InfoText.text += "\n";
        InfoText.text += "\nAverage Survivability: " + region.AverageSurvivability.ToString("P");
        InfoText.text += "\nAverage Foraging Capacity: " + region.AverageForagingCapacity.ToString("P");
        InfoText.text += "\nAverage Accessibility: " + region.AverageAccessibility.ToString("P");
        InfoText.text += "\nAverage Arability: " + region.AverageArability.ToString("P");
    }

    private void AddCellDataToInfoPanel_FarmlandDistribution(TerrainCell cell)
    {
        float cellArea = cell.Area;
        float farmlandPercentage = cell.FarmlandPercentage;

        InfoText.text += "\n";
        InfoText.text += "\n -- Cell Farmland Distribution Data -- ";
        InfoText.text += "\n";

        InfoText.text += "\nFarmland Percentage: " + farmlandPercentage.ToString("P");
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        if (farmlandPercentage > 0)
        {
            float farmlandArea = farmlandPercentage * cellArea;

            InfoText.text += "\nFarmland Area per Pop: " + (farmlandArea / (float)population).ToString("0.000") + " Km^2 / Pop";
        }
    }

    private void AddCellDataToInfoPanel_PopDensity(TerrainCell cell)
    {
        float cellArea = cell.Area;

        InfoText.text += "\n";
        InfoText.text += "\n -- Group Population Density Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int optimalPopulation = cell.Group.OptimalPopulation;
        int previousPopulation = cell.Group.PreviousPopulation;

        InfoText.text += "\nPopulation: " + population;
        InfoText.text += "\nPrevious Population: " + previousPopulation;
        InfoText.text += "\nPopulation Change: " + (population - previousPopulation);
        InfoText.text += "\nOptimal Population: " + optimalPopulation;
        InfoText.text += "\nPop Density: " + (population / cellArea).ToString("0.000") + " Pop / Km^2";

        cell.CalculateAdaptation(
            cell.Group.Culture,
            out float modifiedForagingCapacity,
            out float modifiedSurvivability);

        InfoText.text += "\n";
        InfoText.text += "\nSurvivability: " + modifiedSurvivability.ToString("P");
        InfoText.text += "\nForaging Capacity: " + modifiedForagingCapacity.ToString("P");
    }

    private void AddCellDataToInfoPanel_Language(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Group Language Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        Language groupLanguage = cell.Group.Culture.Language;

        if (groupLanguage == null)
        {
            InfoText.text += "\n\tNo major language spoken at location";

            return;
        }

        InfoText.text += "\n\tPredominant language at location: " + groupLanguage;
    }

    private void AddCellDataToInfoPanel_UpdateSpan(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Group Update Span Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        CellGroup group = cell.Group;

        int population = group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        long lastUpdateDate = group.LastUpdateDate;
        long nextUpdateDate = group.NextUpdateDate;

        InfoText.text += $"\nLast Update Date: {Manager.GetDateString(lastUpdateDate)}";
        InfoText.text += $"\nNext Update Date: {Manager.GetDateString(nextUpdateDate)}";
        InfoText.text += $"\nTime between updates: {Manager.GetTimeSpanString(nextUpdateDate - lastUpdateDate)}";

        InfoText.text += $"\n\nMigration Pressure: {group.MigrationPressure:0.000}";

//#if DEBUG
//        float currentPressure = group.CalculateOverallMigrationPressure();
//        InfoText.text += $"\n\n(DEBUG) Current Migration Pressure: {group.MigrationPressure:0.000}";
//#endif
    }

    private void AddCellDataToInfoPanel_PolityProminence(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Group Polity Prominence Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        bool firstPolity = true;

        List<PolityProminence> polityProminences = GetCellSortedListOfProminences(cell);

        foreach (PolityProminence polityProminence in polityProminences)
        {
            Polity polity = polityProminence.Polity;
            float prominenceValue = polityProminence.Value;
            float factionCoreDistance = polityProminence.FactionCoreDistance;
            Identifier closestFactionCoreId = polityProminence.ClosestFactionId;
            float polityCoreDistance = polityProminence.PolityCoreDistance;
            float administrativeCost = polityProminence.AdministrativeCost;

            if (prominenceValue >= 0.001)
            {
                if (firstPolity)
                {
                    InfoText.text += "\nPolities:";

                    firstPolity = false;
                }

                InfoText.text += "\n\tPolity: " + polity.Name.Text +
                    "\n\t\tProminence: " + prominenceValue.ToString("P") +
                    "\n\t\tDistance to Polity Core: " + polityCoreDistance.ToString("0.000") +
                    "\n\t\tClosest Faction Core: " + closestFactionCoreId +
                    "\n\t\tDistance to Faction Core: " + factionCoreDistance.ToString("0.000") +
                    "\n\t\tAdministrative Cost: " + administrativeCost.ToString("0.000");
            }
        }
    }

    private void AddCellDataToInfoPanel_PolityContacts(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Polity Contacts Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            InfoText.text += "\n\tGroup not part of a polity's territory";
            return;
        }

        Polity polity = territory.Polity;

        InfoText.text += "\nTerritory of the " + polity.Name.Text + " " + polity.TypeStr.ToLower();
        InfoText.text += "\nTranslates to: " + polity.Name.Meaning;
        InfoText.text += "\nFormation Date: " + Manager.GetDateString(polity.FormationDate);
        InfoText.text += "\n";

        if (polity.GetContacts().Count <= 0)
        {
            InfoText.text += "\nPolity has no contact with other polities...";
        }
        else
        {
            InfoText.text += "\nPolities in contact:";
        }

        foreach (PolityContact contact in polity.GetContacts())
        {
            Polity contactPolity = contact.NeighborPolity;

            InfoText.text += "\n\n\tPolity: " + contactPolity.Name.Text + " " + contactPolity.TypeStr.ToLower();

            Faction dominantFaction = contactPolity.DominantFaction;

            InfoText.text += "\n\tDominant Faction: " + dominantFaction.Type + " " + dominantFaction.Name;

            Agent leader = contactPolity.CurrentLeader;

            InfoText.text += "\n\tLeader: " + leader.Name.Text;

            InfoText.text += "\n\tContact Strength: " + contact.GroupCount;
        }
    }

    private void AddCellDataToInfoPanel_General(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "Uninhabited land";

            return;
        }

        int cellPopulation = cell.Group.Population;

        if (cellPopulation <= 0)
        {
            InfoText.text += "Group has zero population";
            Debug.LogError("Group has zero or less population: " + cellPopulation);

            return;
        }

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            InfoText.text += "Disorganized bands";
            InfoText.text += "\n";
            InfoText.text += "\n";

            InfoText.text += cellPopulation + " inhabitants in selected cell";

        }
        else
        {
            Polity polity = territory.Polity;

            InfoText.text += "Territory of the " + polity.Name.Text + " " + polity.TypeStr.ToLower();
            InfoText.text += "\nTranslates to: " + polity.Name.Meaning;
            InfoText.text += "\nFormation Date: " + Manager.GetDateString(polity.FormationDate);
            InfoText.text += "\n";

            Agent leader = polity.CurrentLeader;

            InfoText.text += "\nLeader: " + leader.Name.Text;
            InfoText.text += "\nTranslates to: " + leader.Name.Meaning;
            InfoText.text += "\nBirth Date: " + Manager.GetDateString(leader.BirthDate);
            InfoText.text += "\nAge: " + Manager.GetTimeSpanString(leader.Age);
            InfoText.text += "\nGender: " + ((leader.IsFemale) ? "Female" : "Male");
            InfoText.text += "\nCharisma: " + leader.Charisma;
            InfoText.text += "\nWisdom: " + leader.Wisdom;
            InfoText.text += "\n";
            InfoText.text += "\n";

            int polPopulation = (int)polity.TotalPopulation;

            if (polity.TypeStr == Tribe.PolityTypeStr)
            {
                InfoText.text += polPopulation + " tribe members";
            }
            else
            {
                InfoText.text += polPopulation + " polity citizens";
            }
        }
    }

    private void AddCellDataToInfoPanel_PolityTerritory(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Polity Territory Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        Territory territory = cell.EncompassingTerritory;

        if (territory == null)
        {
            InfoText.text += "\n\tGroup not part of a polity's territory";
            return;
        }

        Polity polity = territory.Polity;

        InfoText.text += "\nTerritory of the " + polity.Name.Text + " " + polity.TypeStr.ToLower();
        InfoText.text += "\nTranslates to: " + polity.Name.Meaning;
        InfoText.text += "\nFormation Date: " + Manager.GetDateString(polity.FormationDate);
        InfoText.text += "\n";

        int totalPopulation = (int)polity.TotalPopulation;

        InfoText.text += "\n\tPolity population: " + totalPopulation;
        InfoText.text += "\n";

        float administrativeCost = polity.TotalAdministrativeCost;

        InfoText.text += "\n\tAdministrative Cost: " + administrativeCost;

        Agent leader = polity.CurrentLeader;

        InfoText.text += "\nLeader: " + leader.Name.Text;
        InfoText.text += "\nTranslates to: " + leader.Name.Meaning;
        InfoText.text += "\nBirth Date: " + Manager.GetDateString(leader.BirthDate);
        InfoText.text += "\nGender: " + ((leader.IsFemale) ? "Female" : "Male");
        InfoText.text += "\nCharisma: " + leader.Charisma;
        InfoText.text += "\nWisdom: " + leader.Wisdom;
        InfoText.text += "\n";

        InfoText.text += "\n";
        InfoText.text += "\n -- Polity Factions -- ";
        InfoText.text += "\n";

        List<Faction> factions = new List<Faction>(polity.GetFactions());

        factions.Sort((a, b) =>
        {
            if (a.Influence > b.Influence)
                return -1;
            if (a.Influence < b.Influence)
                return 1;

            return 0;
        });

        foreach (Faction faction in factions)
        {
            InfoText.text += "\n\t" + faction.Type + " " + faction.Name;
            InfoText.text += "\n\t\tCore: " + faction.CoreGroup.Position;
            InfoText.text += "\n\t\tFormation Date: " + Manager.GetDateString(faction.FormationDate);
            InfoText.text += "\n\t\tInfluence: " + faction.Influence.ToString("P");

            Agent factionLeader = faction.CurrentLeader;

            InfoText.text += "\n\t\tLeader: " + factionLeader.Name.Text;
            InfoText.text += "\n\t\tTranslates to: " + factionLeader.Name.Meaning;
            InfoText.text += "\n\t\tBirth Date: " + Manager.GetDateString(factionLeader.BirthDate);
            InfoText.text += "\n\t\tGender: " + ((factionLeader.IsFemale) ? "Female" : "Male");
            InfoText.text += "\n\t\tCharisma: " + factionLeader.Charisma;
            InfoText.text += "\n\t\tWisdom: " + factionLeader.Wisdom;
            InfoText.text += "\n";
        }

        InfoText.text += "\n";
        InfoText.text += "\n -- Selected Group's Polity Data -- ";
        InfoText.text += "\n";

        float percentageOfPopulation = cell.Group.GetPolityProminenceValue(polity);
        int prominencedPopulation = (int)(population * percentageOfPopulation);

        float percentageOfPolity = 1;

        if (totalPopulation > 0)
        {
            percentageOfPolity = prominencedPopulation / (float)totalPopulation;
        }

        InfoText.text += "\n\tProminenced population: " + prominencedPopulation;
        InfoText.text += "\n\tPercentage of polity population: " + percentageOfPolity.ToString("P");
        InfoText.text += "\n";

        PolityProminence pi = cell.Group.GetPolityProminence(polity);

        if (pi != null)
        {
            InfoText.text += "\n\tDistance to polity core: " + pi.PolityCoreDistance.ToString("0.000");
            InfoText.text += "\n\tClosest Faction Core: " + pi.ClosestFactionId;
            InfoText.text += "\n\tDistance to faction core: " + pi.FactionCoreDistance.ToString("0.000");
        }
        else
        {
            InfoText.text += "\n\tNo group present at location";
        }
    }

    private List<PolityProminence> GetCellSortedListOfProminences(TerrainCell cell)
    {
        List<PolityProminence> polityProminences = new List<PolityProminence>(cell.PolityProminences);

        polityProminences.Sort((a, b) =>
        {
            if (a.Value > b.Value) return -1;
            if (a.Value < b.Value) return 1;
            return 0;
        });

        return polityProminences;
    }

    private void AddCellDataToInfoPanel_PolityClusters(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Group Polity Clusters Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        bool firstPolity = true;

        List<PolityProminence> polityProminences = GetCellSortedListOfProminences(cell);

        foreach (PolityProminence polityProminence in polityProminences)
        {
            Polity polity = polityProminence.Polity;
            float prominenceValue = polityProminence.Value;
            PolityProminenceCluster prominenceCluster = polityProminence.Cluster;

            if (prominenceValue >= 0.001)
            {
                if (firstPolity)
                {
                    InfoText.text += "\nPolities:";

                    firstPolity = false;
                }

                InfoText.text += $"\n\tPolity: {polity.Name.Text}" +
                    $"\n\t\tProminence: {prominenceValue:P}";

                if (prominenceCluster != null)
                {
                    InfoText.text += $"\n\t\tCluster: {prominenceCluster}";
                    InfoText.text += $"\n\t\tCluster Admin Cost: " +
                        $"{prominenceCluster.TotalAdministrativeCost:0.000}";
                }
                else
                {
                    InfoText.text += "\n\t\tCluster: null";
                }
            }
        }
    }

    private void AddCellDataToInfoPanel_PolityCulturalPreference(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Polity Preference Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        PolityProminence polityProminence = cell.Group.HighestPolityProminence;

        if (polityProminence == null)
        {
            InfoText.text += "\n\tGroup not part of a polity";

            return;
        }

        bool firstPreference = true;

        foreach (CulturalPreference preference in polityProminence.Polity.Culture.GetPreferences())
        {
            if (firstPreference)
            {
                InfoText.text += "\nPreferences:";

                firstPreference = false;
            }

            InfoText.text += "\n\t" + preference.Name + " Preference: " + preference.Value.ToString("P");
        }
    }

    private void AddCellDataToInfoPanel_PolityCulturalActivity(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Polity Activity Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        PolityProminence polityProminence = cell.Group.HighestPolityProminence;

        if (polityProminence == null)
        {
            InfoText.text += "\n\tGroup not part of a polity";

            return;
        }

        bool firstActivity = true;

        foreach (CulturalActivity activity in polityProminence.Polity.Culture.GetActivities())
        {
            if (firstActivity)
            {
                InfoText.text += "\nActivities:";

                firstActivity = false;
            }

            InfoText.text += "\n\t" + activity.Name + " Contribution: " + activity.Contribution.ToString("P");
        }
    }

    private void AddCellDataToInfoPanel_PopCulturalPreference(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Group Preference Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        bool firstPreference = true;

        foreach (CulturalPreference preference in cell.Group.Culture.GetPreferences())
        {
            if (firstPreference)
            {
                InfoText.text += "\nPreferences:";

                firstPreference = false;
            }

            InfoText.text += "\n\t" + preference.Name + " Preference: " + preference.Value.ToString("P");
        }
    }

    private void AddCellDataToInfoPanel_PopCulturalActivity(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Group Activity Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        bool firstActivity = true;

        foreach (CulturalActivity activity in cell.Group.Culture.GetActivities())
        {
            if (firstActivity)
            {
                InfoText.text += "\nActivities:";

                firstActivity = false;
            }

            InfoText.text += "\n\t" + activity.Name + " Contribution: " + activity.Contribution.ToString("P");
        }
    }

    private void AddCellDataToInfoPanel_PolityCulturalSkill(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Polity Skill Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        PolityProminence polityProminence = cell.Group.HighestPolityProminence;

        if (polityProminence == null)
        {
            InfoText.text += "\n\tGroup not part of a polity";

            return;
        }

        bool firstSkill = true;

        foreach (CulturalSkill skill in polityProminence.Polity.Culture.GetSkills())
        {
            float skillValue = skill.Value;

            if (skillValue >= 0.001)
            {
                if (firstSkill)
                {
                    InfoText.text += "\nSkills:";

                    firstSkill = false;
                }

                InfoText.text += "\n\t" + skill.Name + " Value: " + skill.Value.ToString("0.000");
            }
        }
    }

    private void AddCellDataToInfoPanel_PopCulturalSkill(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Group Skill Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        bool firstSkill = true;

        foreach (CulturalSkill skill in cell.Group.Culture.GetSkills())
        {
            float skillValue = skill.Value;

            if (skillValue >= 0.001)
            {
                if (firstSkill)
                {
                    InfoText.text += "\nSkills:";

                    firstSkill = false;
                }

                InfoText.text += "\n\t" + skill.Name + " Value: " + skill.Value.ToString("0.000");
            }
        }
    }

    private void AddCellDataToInfoPanel_PolityCulturalKnowledge(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Polity Knowledge Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        PolityProminence polityProminence = cell.Group.HighestPolityProminence;

        if (polityProminence == null)
        {
            InfoText.text += "\n\tGroup not part of a polity";

            return;
        }

        bool firstKnowledge = true;

        foreach (CulturalKnowledge knowledge in polityProminence.Polity.Culture.GetKnowledges())
        {
            float knowledgeValue = knowledge.ScaledValue;

            if (firstKnowledge)
            {
                InfoText.text += "\nKnowledges:";

                firstKnowledge = false;
            }

            InfoText.text += "\n\t" + knowledge.Name + " Value: " + knowledgeValue.ToString("0.000");
        }
    }

    private void AddCellDataToInfoPanel_PopCulturalKnowledge(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Group Knowledge Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        bool firstKnowledge = true;

        foreach (CulturalKnowledge knowledge in cell.Group.Culture.GetKnowledges())
        {
            float knowledgeValue = knowledge.ScaledValue;

            if (firstKnowledge)
            {
                InfoText.text += "\nKnowledges:";

                firstKnowledge = false;
            }

            InfoText.text += "\n\t" + knowledge.Name + " Value: " + knowledgeValue.ToString("0.000");
        }
    }

    private void AddCellDataToInfoPanel_PolityCulturalDiscovery(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Polity Discovery Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        PolityProminence polityProminence = cell.Group.HighestPolityProminence;

        if (polityProminence == null)
        {
            InfoText.text += "\n\tGroup not part of a polity";

            return;
        }

        bool firstDiscovery = true;

        foreach (Discovery discovery in polityProminence.Polity.Culture.Discoveries.Values)
        {
            if (firstDiscovery)
            {
                InfoText.text += "\nDiscoveries:";

                firstDiscovery = false;
            }

            InfoText.text += "\n\t" + discovery.Name;
        }
    }

    private void AddCellDataToInfoPanel_PopCulturalDiscovery(TerrainCell cell)
    {
        InfoText.text += "\n";
        InfoText.text += "\n -- Group Discovery Data -- ";
        InfoText.text += "\n";

        if (cell.Group == null)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        int population = cell.Group.Population;

        if (population <= 0)
        {
            InfoText.text += "\n\tNo population at location";

            return;
        }

        bool firstDiscovery = true;

        foreach (Discovery discovery in cell.Group.Culture.Discoveries.Values)
        {
            if (firstDiscovery)
            {
                InfoText.text += "\nDiscoveries:";

                firstDiscovery = false;
            }

            InfoText.text += "\n\t" + discovery.Name;
        }
    }

    private void AddCellDataToInfoPanel(TerrainCell cell)
    {
        int longitude = cell.Longitude;
        int latitude = cell.Latitude;

        InfoText.text += "\n";
        InfoText.text += string.Format("\nPosition: Longitude {0}, Latitude {1}", longitude, latitude);

        if ((Manager.PlanetOverlay == PlanetOverlay.None) ||
            (Manager.PlanetOverlay == PlanetOverlay.Rainfall) ||
            (Manager.PlanetOverlay == PlanetOverlay.DrainageBasins) ||
            (Manager.PlanetOverlay == PlanetOverlay.Arability) ||
            (Manager.PlanetOverlay == PlanetOverlay.Accessibility) ||
            (Manager.PlanetOverlay == PlanetOverlay.Hilliness) ||
            (Manager.PlanetOverlay == PlanetOverlay.BiomeTrait) ||
            (Manager.PlanetOverlay == PlanetOverlay.Layer) ||
            (Manager.PlanetOverlay == PlanetOverlay.Temperature))
        {
            AddCellDataToInfoPanel_Terrain(cell);
        }

        if ((Manager.PlanetOverlay == PlanetOverlay.Region) ||
            (Manager.PlanetOverlay == PlanetOverlay.RegionSelection))
        {
            AddCellDataToInfoPanel_Region(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.Language)
        {
            AddCellDataToInfoPanel_Language(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.FarmlandDistribution)
        {
            AddCellDataToInfoPanel_FarmlandDistribution(cell);
        }


        if (Manager.PlanetOverlay == PlanetOverlay.General)
        {
            AddCellDataToInfoPanel_General(cell);
        }

        if ((Manager.PlanetOverlay == PlanetOverlay.PopDensity) ||
            (Manager.PlanetOverlay == PlanetOverlay.PopChange))
        {
            AddCellDataToInfoPanel_PopDensity(cell);
        }

        if ((Manager.PlanetOverlay == PlanetOverlay.UpdateSpan) ||
            (Manager.PlanetOverlay == PlanetOverlay.Migration) ||
            (Manager.PlanetOverlay == PlanetOverlay.MigrationPressure) ||
            (Manager.PlanetOverlay == PlanetOverlay.PolityMigrationPressure))
        {
            AddCellDataToInfoPanel_UpdateSpan(cell);
        }

        if ((Manager.PlanetOverlay == PlanetOverlay.PolityProminence) ||
            (Manager.PlanetOverlay == PlanetOverlay.FactionCoreDistance))
        {
            AddCellDataToInfoPanel_PolityProminence(cell);
        }

        if ((Manager.PlanetOverlay == PlanetOverlay.PolityContacts) ||
            (Manager.PlanetOverlay == PlanetOverlay.PolitySelection))
        {
            AddCellDataToInfoPanel_PolityContacts(cell);
        }

        if ((Manager.PlanetOverlay == PlanetOverlay.PolityCoreRegions) ||
            (Manager.PlanetOverlay == PlanetOverlay.PolityTerritory) ||
            (Manager.PlanetOverlay == PlanetOverlay.PolityAdminCost))
        {
            AddCellDataToInfoPanel_PolityTerritory(cell);
        }

        if ((Manager.PlanetOverlay == PlanetOverlay.PolityCluster) ||
            (Manager.PlanetOverlay == PlanetOverlay.ClusterAdminCost))
        {
            AddCellDataToInfoPanel_PolityClusters(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalPreference)
        {
            AddCellDataToInfoPanel_PolityCulturalPreference(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PopCulturalPreference)
        {
            AddCellDataToInfoPanel_PopCulturalPreference(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalActivity)
        {
            AddCellDataToInfoPanel_PolityCulturalActivity(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PopCulturalActivity)
        {
            AddCellDataToInfoPanel_PopCulturalActivity(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalSkill)
        {
            AddCellDataToInfoPanel_PolityCulturalSkill(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PopCulturalSkill)
        {
            AddCellDataToInfoPanel_PopCulturalSkill(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalKnowledge)
        {
            AddCellDataToInfoPanel_PolityCulturalKnowledge(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PopCulturalKnowledge)
        {
            AddCellDataToInfoPanel_PopCulturalKnowledge(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalDiscovery)
        {
            AddCellDataToInfoPanel_PolityCulturalDiscovery(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.PopCulturalDiscovery)
        {
            AddCellDataToInfoPanel_PopCulturalDiscovery(cell);
        }

        if (Manager.PlanetOverlay == PlanetOverlay.CellSelection)
        {
            if (Manager.PlanetOverlaySubtype == Manager.GroupProminenceOverlaySubtype)
                AddCellDataToInfoPanel_PolityProminence(cell);
            else
                AddCellDataToInfoPanel_Terrain(cell);
        }
    }
}
