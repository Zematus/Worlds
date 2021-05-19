using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapB : MonoBehaviour
{
    public Texture2D GenDataText;
    public Texture2D WaterPresence;
    public Texture2D LayerDataText;
    public List<float> BiomeList;
    public int BiomeNumbers;
    public int BiomeLength;
    public int LayerNumber;

    public UnityEngine.UI.RawImage Surface;
    //public Shader BiomeShader;

    public bool PushData;

    void Start()
    {
        LoadData();
        /*
        this.GetComponent<Renderer>().material.SetTexture("_AltTex", GenDataText);
        this.GetComponent<Renderer>().material.SetTexture("_SecondTex", WaterPresence);
        this.GetComponent<Renderer>().material.SetTexture("_LayerTex", LayerDataText);
        this.GetComponent<Renderer>().material.SetFloatArray("Biomes", BiomeList);
        this.GetComponent<Renderer>().material.SetInt("BiomeNumbers", BiomeNumbers);
        this.GetComponent<Renderer>().material.SetInt("BiomeLength", BiomeLength);
        this.GetComponent<Renderer>().material.SetInt("LayerNumber", LayerNumber);*/

        
        Shader.SetGlobalTexture("_AltTex", GenDataText);
        Shader.SetGlobalTexture("_SecondTex", WaterPresence);
        Shader.SetGlobalTexture("_LayerTex", LayerDataText);
        Shader.SetGlobalFloatArray("Biomes", BiomeList);
        /*int i = 0;
        foreach (float f in BiomeList) {
            Shader.SetGlobalFloat("Biomes" + i, f);
            i++;
        }*/
        Shader.SetGlobalInt("BiomeNumbers", BiomeNumbers);
        Shader.SetGlobalInt("BiomeLength", BiomeLength);
        Shader.SetGlobalInt("LayerNumber", LayerNumber);
    }

    // Update is called once per frame
    void Update()
    {
        if (Surface != null) {
            float OffSetSize = Surface.uvRect.width;
            Shader.SetGlobalFloat("_CoastSize", OffSetSize);
        }

    }

    public void LoadData() {
        TerrainCell[][] terrainCells = Manager.CurrentWorld.TerrainCells;
        GenDataText = new Texture2D(terrainCells.Length, terrainCells[0].Length);
        WaterPresence = new Texture2D(terrainCells.Length, terrainCells[0].Length);
        float MaxAltitude = float.MinValue;
        float MinAltitude = float.MaxValue;

        List<Layer> Layers = new List<Layer>();
        foreach (Layer layer in Layer.Layers.Values) {
            Layers.Add(layer);
        }
        Debug.LogWarning(Layers.Count);
        int LayerNeededText = Mathf.CeilToInt(Layers.Count / 4f);
        //Debug.LogError(LayerNeededText);
        if (LayerNeededText > 0)
        {
            LayerDataText = new Texture2D(terrainCells.Length * LayerNeededText, terrainCells[0].Length);
        }
        else {
            LayerDataText = new Texture2D(1, 1);
        }
        LayerNumber = Layers.Count;

        for (int i = 0; i < terrainCells.Length; i++) {
            TerrainCell[] Line = terrainCells[i];
            for (int ii = 0; ii < Line.Length; ii++)
            {
                TerrainCell Cell = Line[ii];
                
                GenDataText.SetPixel(i, ii, new Color((Cell.Altitude / 16000f) +0.5f, (Cell.Temperature /120f)+0.5f, (Manager.GetSlant(Cell)/12000f) + 0.5f));
                WaterPresence.SetPixel(i, ii, new Color((Cell.FlowingWater/80000f)+0.5f, Cell.Rainfall/6000f, Cell.WaterBiomePresence));

                for (int iii = 0; iii < LayerNeededText; iii++) {
                    Color LayerColor = new Color(0, 0, 0, 0);
                    if (Layers.Count > iii * 4 + 0) {
                        LayerColor.r = Cell.GetLayerValue(Layers[iii * 4 + 0].Id);
                    }
                    if (Layers.Count > iii * 4 + 1)
                    {
                        LayerColor.g = Cell.GetLayerValue(Layers[iii * 4 + 1].Id);
                    }
                    if (Layers.Count > iii * 4 + 2)
                    {
                        LayerColor.b = Cell.GetLayerValue(Layers[iii * 4 + 2].Id);
                    }
                    if (Layers.Count > iii * 4 + 3)
                    {
                        LayerColor.a = Cell.GetLayerValue(Layers[iii * 4 + 3].Id);
                    }
                    LayerDataText.SetPixel(i + iii * terrainCells.Length, ii, LayerColor);
                }
                /*
                if (Cell.GetLayerValue(Layers[1].Id) > MaxAltitude) {
                    MaxAltitude = Cell.GetLayerValue(Layers[1].Id);
                }
                if (Cell.GetLayerValue(Layers[1].Id) < MinAltitude) {
                    MinAltitude = Cell.GetLayerValue(Layers[1].Id);
                }*/
            }
        }
        Debug.Log(MinAltitude + "|" + MaxAltitude);
        GenDataText.filterMode = FilterMode.Point;
        WaterPresence.filterMode = FilterMode.Point;
        LayerDataText.filterMode = FilterMode.Point;
        GenDataText.Apply();
        WaterPresence.Apply();
        LayerDataText.Apply();

        BiomeNumbers = 0;
        BiomeList = new List<float>();
        BiomeLength = 0;
        foreach (Biome biome in Biome.Biomes.Values) {
            /*if (biome.Name == "river") {
                continue;
            }
            if (biome.Name == "sea" && false)
            {
                continue;
            }*/

            BiomeNumbers++;

            float IsSea = 0;
            if (biome.Traits.Contains("sea")) {
                IsSea = 1;
            }
            float IsWaterType = 0;
            if (biome.TerrainType == BiomeTerrainType.Water) {
                IsWaterType = 1;
            }

            Debug.Log(biome.Name + "|" + biome.Color.r + "|" + biome.Color.g + "|" + biome.Color.b + "|" + biome.MinAltitude + "|" + biome.MaxAltitude + "|" + biome.MinTemperature + "|" + biome.MaxTemperature + "|" + biome.MinRainfall + "|" + biome.MaxRainfall + "|" + biome.MinFlowingWater + "|" + biome.MaxFlowingWater);
            float[] BiomeData = new float[] { biome.Color.r, biome.Color.g, biome.Color.b, biome.MinAltitude, biome.MaxAltitude, biome.MinTemperature, biome.MaxTemperature, biome.MinRainfall, biome.MaxRainfall, biome.MinFlowingWater, biome.MaxFlowingWater, biome.AltSaturationSlope, biome.WaterSaturationSlope, biome.TempSaturationSlope, IsSea, IsWaterType };
            List<float> FinalBiomeData = BiomeData.ToList<float>();
            foreach (Layer layer in Layers) {
                List<float> LayerData = new List<float>();
                if(biome.LayerConstraints != null) { 
                if (biome.LayerConstraints.ContainsKey(layer.Id))
                {
                    Biome.LayerConstraint layerConstraint = biome.LayerConstraints[layer.Id];
                    //Condition present
                    LayerData.Add(1);
                    //Min
                    LayerData.Add(layerConstraint.MinValue / layer.MaxPossibleValue);
                    //Max
                    LayerData.Add(layerConstraint.MaxValue / layer.MaxPossibleValue);
                    //Saturation
                    LayerData.Add(layerConstraint.SaturationSlope);
                }
                    else
                    {
                        //Condition present
                        LayerData.Add(0);
                        //Min
                        LayerData.Add(0);
                        //Max
                        LayerData.Add(0);
                        //Saturation
                        LayerData.Add(0);
                    }
                }
                else {
                    //Condition present
                    LayerData.Add(0);
                    //Min
                    LayerData.Add(0);
                    //Max
                    LayerData.Add(0);
                    //Saturation
                    LayerData.Add(0);
                }
                FinalBiomeData.AddRange(LayerData);
            }
            if (BiomeLength == 0) {
                BiomeLength = FinalBiomeData.Count;
            }
            BiomeList.AddRange(FinalBiomeData);
        }
    }

}
