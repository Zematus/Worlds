using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Layer
{
    public static float MinLayerTemperature = World.MinPossibleTemperature * 3 - 1;
    public static float MaxLayerTemperature = World.MaxPossibleTemperature * 3 + 1;

    public static float MinLayerRainfall = World.MinPossibleRainfall * 3 - 1;
    public static float MaxLayerRainfall = World.MaxPossibleRainfall * 3 + 1;

    public static float MinLayerAltitude = World.MinPossibleAltitude * 3 - 1;
    public static float MaxLayerAltitude = World.MaxPossibleAltitude * 3 + 1;

    public static Dictionary<string, Layer> Layers;

    public string Name;
    public string Id;

    public float NoiseScale;
    public float NoiseMagnitude;

    public float MinAltitude;
    public float MaxAltitude;

    public float MinRainfall;
    public float MaxRainfall;

    public float MinTemperature;
    public float MaxTemperature;

    public static void ResetLayers()
    {
        Layers = new Dictionary<string, Layer>();
    }

    public static void LoadLayersFile(string filename)
    {
        foreach (Layer layer in LayerLoader.Load(filename))
        {
            if (Layers.ContainsKey(layer.Id))
            {
                Layers[layer.Id] = layer;
            }
            else
            {
                Layers.Add(layer.Id, layer);
            }
        }
    }
}
