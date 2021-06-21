// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "WorldMap/Biomes"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        //[PerRendererData] _AltTex("Sprite Texture", 2D) = "white" {}
        //[PerRendererData] _SecondTex("Sprite Texture2", 2D) = "white" {}
        //[PerRendererData] _LayerTex("_LayerTex", 2D) = "white" {}
        [HideInInspector] _Cutoff("Alpha Cutoff", Range(0,1)) = 0.0001
        

        [Header(MapMode)]
        [KeywordEnum(Pixel, Linear, Complex)] Height_Mode("Height mode", int) = 1
        [KeywordEnum(Pixel, Linear, Complex)] Temperature_Mode("Temperature mode", int) = 1
        [KeywordEnum(Pixel, Linear, Complex)] Slant_Mode("Slant mode", int) = 1
        [KeywordEnum(Pixel, Linear, Complex)] FlowingWater_Mode("FlowingWater mode", int) = 1
        [KeywordEnum(Pixel, Linear, Complex)] Rainfall_Mode("Rainfall mode", int) = 1
        [KeywordEnum(Pixel, Linear, Complex)] WarterBiomePresence_Mode("WarterBiomePresence mode", int) = 1
        [Toggle] Linear_Distance_mode("Linear use smooth distance", int) = 0
        [Toggle] Complex_Distance_mode("Complex use smooth distance", int) = 0

        [Header(Variable)]
        _BlurAmount("Temp data", Range(0, 2)) = 0.5
        _TempData2("Temp data 2", Range(0, 1)) = 0.1

        [Header(Colors)]
        ice_shelf_Color("Temp", color) = (0.992, 0.956, 0.921,0)
        
        
    }
CGINCLUDE
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
//#pragma exclude_renderers d3d11 gles

    half HeightTranslator(half Value) {
        return (Value - 0.5) * 16000;
    }

    half TemperatureTranslator(half Value) {
        return (Value - 0.5) * 120;
    }

    half FlowingWaterTranslator(half Value) {
        return (Value - 0.5) * 80000;
    }

    half RainfallTranslator(half Value) {
        return Value * 6000;
    }

    half SlantTranslator(half Value) {
        return (Value - 0.5) * 12000;
    }

    half ComplexValueCalculation(half VA1, half VA2, half VA3, half VA4, half VB1, half VB2, half VB3, half VB4, half VC1, half VC2, half VC3, half VC4, half VD1, half VD2, half VD3, half VD4, half2 DistanceFromCenterOfPixel) {

        half2 DB3 = (half2(VB3 - VA3, VB3 - VB4) + half2(VC3 - VB3, VB2 - VB3)) / 2;
        half2 DC3 = (half2(VC3 - VB3, VC3 - VC4) + half2(VD3 - VC3, VC2 - VC3)) / 2;
        half2 DB2 = (half2(VB2 - VA2, VB2 - VB3) + half2(VC2 - VB2, VB1 - VB2)) / 2;
        half2 DC2 = (half2(VC2 - VB2, VC2 - VC3) + half2(VD2 - VC2, VC1 - VC2)) / 2;

        half RB3 = VB3 + DistanceFromCenterOfPixel.x * DB3.x + DistanceFromCenterOfPixel.y * DB3.y;
        half RC3 = VC3 + (1 - DistanceFromCenterOfPixel.x) * DC3.x + DistanceFromCenterOfPixel.y * DC3.y;
        half RB2 = VB2 + DistanceFromCenterOfPixel.x * DB2.x + (1 - DistanceFromCenterOfPixel.y) * DB2.y;
        half RC2 = VC2 + (1 - DistanceFromCenterOfPixel.x) * DC2.x + (1 - DistanceFromCenterOfPixel.y) * DC2.y;

        half FinalValue = RB3 * (1 - DistanceFromCenterOfPixel.x) * (1 - DistanceFromCenterOfPixel.y);
        FinalValue += RC3 * DistanceFromCenterOfPixel.x * (1 - DistanceFromCenterOfPixel.y);
        FinalValue += RB2 * (1 - DistanceFromCenterOfPixel.x) * DistanceFromCenterOfPixel.y;
        FinalValue += RC2 * DistanceFromCenterOfPixel.x * DistanceFromCenterOfPixel.y;

        return FinalValue;
    }
    
    half BiomeAltitude(half PixelHeight, float BiomeMinAltitude, float BiomeMaxAltitude, float BiomeAltSaturation) {
        float altitudeSpan = BiomeMaxAltitude - BiomeMinAltitude;
        float altitudeDiff = PixelHeight - BiomeMinAltitude;
        if (altitudeDiff < 0)
            return 0;

        float altitudeFactor = 0.5f;
        if (altitudeSpan != 0) {
            altitudeFactor = altitudeDiff / altitudeSpan;
        }
        if (altitudeFactor > 1)
            return 0;

        if (altitudeFactor > 0.5f)
            altitudeFactor = 1 - altitudeFactor;

        altitudeFactor *= BiomeAltSaturation;
        if (altitudeFactor == 0)
        {
            altitudeFactor = 0.005; // We don't want to return 0 ever as it messes up with biome asignations
        }

        return altitudeFactor * 2;
    }

    half BiomeLayer(half LayerValue, float BiomeMinLayer, float BiomeMaxLayer, float BiomeLayerSaturation) {
        float altitudeSpan = BiomeMaxLayer - BiomeMinLayer;
        float altitudeDiff = LayerValue - BiomeMinLayer;
        if (altitudeDiff < 0)
            return 0;

        float altitudeFactor = 0.5f;
        if (altitudeSpan != 0) {
            altitudeFactor = altitudeDiff / altitudeSpan;
        }
        if (altitudeFactor > 1)
            return 0;

        if (altitudeFactor > 0.5f)
            altitudeFactor = 1 - altitudeFactor;

        altitudeFactor *= BiomeLayerSaturation;
        if (altitudeFactor == 0)
        {
            altitudeFactor = 0.005; // We don't want to return 0 ever as it messes up with biome asignations
        }

        return altitudeFactor * 2;
    }

    half WaterBiomeWater(half PixelFlowingWater, float BiomeMinFlowingWater, float BiomeMaxFlowingWater, float BiomeWaterSaturation) {
        float flowingWaterSpan = BiomeMaxFlowingWater - BiomeMinFlowingWater;
        float flowingWaterDiff = PixelFlowingWater - BiomeMinFlowingWater;

        if (flowingWaterDiff < 0)
            return 0;

        float waterFactor = flowingWaterDiff / flowingWaterSpan;

        if (waterFactor > 1)
            return 0;

        if (waterFactor > 0.5f)
            waterFactor = 1 - waterFactor;

        waterFactor *= BiomeWaterSaturation;

        return max(0.005f, waterFactor * 2);
    }

    half BiomeWater(half PixelRainfall, half PixelFlowingWater, float BiomeMinRainfall, float BiomeMaxRainfall, float BiomeMinFlowingWater, float BiomeMaxFlowingWater, float BiomeWaterSaturation, float WaterType) {
        if (WaterType == 1) {
            return WaterBiomeWater(PixelFlowingWater, BiomeMinFlowingWater, BiomeMaxFlowingWater, BiomeWaterSaturation);
        }
        float flowingWaterSpan = BiomeMaxFlowingWater - BiomeMinFlowingWater;
        float rainfallSpan = BiomeMaxRainfall - BiomeMinRainfall;
        float flowingWaterDiff = PixelFlowingWater - BiomeMinFlowingWater;
        float rainfallDiff = PixelRainfall - BiomeMinRainfall;
        if ((flowingWaterDiff < 0) && (rainfallDiff < 0))
            return 0;

        float moistureFactor = flowingWaterDiff / flowingWaterSpan;
        float rainfallFactor = rainfallDiff / rainfallSpan;
        float waterFactor = max(moistureFactor, rainfallFactor);

        if (waterFactor > 1)
            return 0;

        if (waterFactor > 0.5f)
            waterFactor = 1 - waterFactor;

        waterFactor *= BiomeWaterSaturation;

        return max(0.005, waterFactor * 2);
    }

    half BiomeTemperature(half PixelTemperature, float BiomeMinTemperature, float BiomeMaxTemperature, float BiomeTemperatureSaturation) {
        float temperatureSpan = BiomeMaxTemperature - BiomeMinTemperature;
        float temperatureDiff = PixelTemperature - BiomeMinTemperature;

        if (temperatureDiff < 0)
            return 0;

        float temperatureFactor = 0.5;
        if (temperatureSpan != 0)
            temperatureFactor = temperatureDiff / temperatureSpan;

        if (temperatureFactor > 1)
            return 0;

        temperatureFactor *= BiomeTemperatureSaturation;

        if (temperatureFactor == 0)
        {
            temperatureFactor = 0.005; // We don't want to return 0 ever as it messes up with biome asignations
        }

        return temperatureFactor * 2;
    }
    
    half BiomePresence(half PixelHeight, half PixelTemperature, half PixelFlowingWater, half PixelRainfall, float BiomeMinAltitude, float BiomeMaxAltitude, float BiomeAltSaturation, float BiomeMinTemperature, float BiomeMaxTemperature, float BiomeTemperatureSaturation, float BiomeMinRainfall, float BiomeMaxRainfall, float BiomeMinFlowingWater, float BiomeMaxFlowingWater, float BiomeWaterSaturation, float IsSea, float WaterType) {
        float presence = 1;
        presence *= BiomeAltitude(PixelHeight, BiomeMinAltitude, BiomeMaxAltitude, BiomeAltSaturation);
        if (presence <= 0)
            return 0;

        if (IsSea == 0) {
            presence *= BiomeWater(PixelRainfall, PixelFlowingWater, BiomeMinRainfall, BiomeMaxRainfall, BiomeMinFlowingWater, BiomeMaxFlowingWater, BiomeWaterSaturation, WaterType);
        }
        if (presence <= 0)
            return 0;

        presence *= BiomeTemperature(PixelTemperature, BiomeMinTemperature, BiomeMaxTemperature, BiomeTemperatureSaturation);
        if (presence <= 0)
            return 0;

        //Add layer
        return presence;
    }

    

ENDCG

        SubShader
        {

            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }


            LOD 300
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha


            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
//#pragma exclude_renderers d3d11 gles
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
//#pragma exclude_renderers d3d11 gles
            #pragma surface surf ToonRamp vertex vert alpha alphatest:_Cutoff addshadow
            #pragma lighting ToonRamp
            //#pragma fragment frag


            sampler2D _AltTex;
            sampler2D _SecondTex;
            sampler2D _LayerTex;
            uniform float Biomes[1000];
            int BiomeNumbers;
            int BiomeLength;
            int LayerNumber;
            int Height_Mode;
            int Temperature_Mode;
            int Slant_Mode;
            int FlowingWater_Mode;
            int Rainfall_Mode;
            int WarterBiomePresence_Mode;
            int Linear_Distance_mode;
            int Complex_Distance_mode;
            float _BlurAmount;
            float _TempData2;
            float4 _AltTex_TexelSize;
            half4 ice_shelf_Color;
            float _StartX;


            // for hard double-sided proximity lighting
            inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
            {
                half4 c;
                c.rgb = s.Albedo * 0.5;
                c.a = s.Alpha;
                return c ;
            }


            struct Input
            {
                float2 uv_AltTex : TEXCOORD0;
                float2 uv_SecondTex : TEXCOORD0;
                float2 uv_LayerTex : TEXCOORD0;
                fixed4 color;
            };

            
            void vert(inout appdata_full v, out Input o)
            {
                UNITY_INITIALIZE_OUTPUT(Input, o);
                o.color = v.color;
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                //Mode initialisation
                bool MustChargePixelMode = (Height_Mode == 0 || Temperature_Mode == 0 || Slant_Mode == 0 || FlowingWater_Mode == 0 || Rainfall_Mode == 0 || WarterBiomePresence_Mode == 0);
                bool MustChargeLinearMode = (Height_Mode == 1 || Temperature_Mode == 1 || Slant_Mode == 1 || FlowingWater_Mode == 1 || Rainfall_Mode == 1 || WarterBiomePresence_Mode == 1);
                bool MustChargeComplexMode = (Height_Mode == 2 || Temperature_Mode == 2 || Slant_Mode == 2 || FlowingWater_Mode == 2 || Rainfall_Mode == 2 || WarterBiomePresence_Mode == 2);
                // Initialisation
                half TextHeight = 200;
                half TextLenght = 400;
                half2 CorrectUv = IN.uv_AltTex;
                int InitialX = _StartX;
                if (_StartX < 0) {
                    InitialX--;
                }
                CorrectUv.x -= InitialX;

                int2 CaseIndex = int2(CorrectUv.x * TextLenght, CorrectUv.y * TextHeight);
                half2 DistanceFromCenterOfPixel = half2(CorrectUv.x * TextLenght - (CaseIndex.x + 0.5), CorrectUv.y * TextHeight - (CaseIndex.y + 0.5));
                

                int2 PositionDecay = int2(0, 0);
                if (DistanceFromCenterOfPixel.x < 0) {
                    PositionDecay.x = -1;
                    DistanceFromCenterOfPixel.x += 1;
                }
                if (DistanceFromCenterOfPixel.y < 0) {
                    PositionDecay.y = -1;
                    DistanceFromCenterOfPixel.y += 1;
                }
                half TrueHeight = 0;
                half TrueTemperature = 0;
                half TrueSlant = 0;
                half TrueFlowingWater = 0;
                half TrueRainfall = 0;
                half TrueWarterBiomePresence = 0;

                float pi = 3.141592;
                half2 SmoothDistanceFromCenterOfPixel = half2(sin((DistanceFromCenterOfPixel.x - 0.5) * pi) / 2 + 0.5, sin((DistanceFromCenterOfPixel.y - 0.5) * pi) / 2 + 0.5);

                //Pixel
                if (MustChargePixelMode) {
                    half4 Data = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight));
                    half4 Data2 = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight));

                    if (Height_Mode == 0)
                        TrueHeight = HeightTranslator(Data.r);
                    if (Temperature_Mode == 0)
                        TrueTemperature = TemperatureTranslator(Data.g);
                    if (Slant_Mode == 0)
                        TrueSlant = SlantTranslator(Data.b);
                    if (FlowingWater_Mode == 0)
                        TrueFlowingWater = FlowingWaterTranslator(Data2.r);
                    if (Rainfall_Mode == 0)
                        TrueRainfall = RainfallTranslator(Data2.g);
                    if (WarterBiomePresence_Mode == 0)
                        TrueWarterBiomePresence = Data2.b;
                }
                //Linear
                if (MustChargeLinearMode) {
                    half2 LocalDistance;
                    if (Linear_Distance_mode == 0) {
                        LocalDistance = DistanceFromCenterOfPixel;
                    }
                    else {
                        LocalDistance = SmoothDistanceFromCenterOfPixel;
                    }
                    half4 finalOutput = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight)) * (1 - LocalDistance.x) * (1 - LocalDistance.y);
                    finalOutput += tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 1 + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight)) * (LocalDistance.x) * (1 - LocalDistance.y);
                    finalOutput += tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 1 + 0.5) / TextHeight)) * (1 - LocalDistance.x) * (LocalDistance.y);
                    finalOutput += tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 1 + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 1 + 0.5) / TextHeight)) * (LocalDistance.x) * (LocalDistance.y);

                    half4 finalOutput2 = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight)) * (1 - LocalDistance.x) * (1 - LocalDistance.y);
                    finalOutput2 += tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 1 + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight)) * (LocalDistance.x) * (1 - LocalDistance.y);
                    finalOutput2 += tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 1 + 0.5) / TextHeight)) * (1 - LocalDistance.x) * (LocalDistance.y);
                    finalOutput2 += tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 1 + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 1 + 0.5) / TextHeight)) * (LocalDistance.x) * (LocalDistance.y);

                    if (Height_Mode == 1)
                        TrueHeight = HeightTranslator(finalOutput.r);
                    if (Temperature_Mode == 1)
                        TrueTemperature = TemperatureTranslator(finalOutput.g);
                    if (Slant_Mode == 1)
                        TrueSlant = SlantTranslator(finalOutput.b);
                    if (FlowingWater_Mode == 1)
                        TrueFlowingWater = FlowingWaterTranslator(finalOutput2.r);
                    if (Rainfall_Mode == 1)
                        TrueRainfall = RainfallTranslator(finalOutput2.g);
                    if (WarterBiomePresence_Mode == 1)
                        TrueWarterBiomePresence = finalOutput2.b;
                }

                //complex
                if (MustChargeComplexMode) {
                    half2 LocalDistance;
                    if (Complex_Distance_mode == 0) {
                        LocalDistance = DistanceFromCenterOfPixel;
                    }
                    else {
                        LocalDistance = SmoothDistanceFromCenterOfPixel;
                    }

                    half4 A1 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 - 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 2) / TextHeight));
                    half4 A2 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 - 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight));
                    half4 A3 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 - 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 0) / TextHeight));
                    half4 A4 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 - 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 - 1) / TextHeight));
                    half4 B1 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 0) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 2) / TextHeight));
                    half4 B2 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 0) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight));
                    half4 B3 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 0) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 0) / TextHeight));
                    half4 B4 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 0) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 - 1) / TextHeight));
                    half4 C1 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 2) / TextHeight));
                    half4 C2 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight));
                    half4 C3 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 0) / TextHeight));
                    half4 C4 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 - 1) / TextHeight));
                    half4 D1 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 2) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 2) / TextHeight));
                    half4 D2 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 2) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight));
                    half4 D3 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 2) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 0) / TextHeight));
                    half4 D4 = tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 2) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 - 1) / TextHeight));

                    half4 A1b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 - 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 2) / TextHeight));
                    half4 A2b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 - 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight));
                    half4 A3b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 - 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 0) / TextHeight));
                    half4 A4b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 - 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 - 1) / TextHeight));
                    half4 B1b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 0) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 2) / TextHeight));
                    half4 B2b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 0) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight));
                    half4 B3b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 0) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 0) / TextHeight));
                    half4 B4b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 0) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 - 1) / TextHeight));
                    half4 C1b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 2) / TextHeight));
                    half4 C2b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight));
                    half4 C3b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 0) / TextHeight));
                    half4 C4b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 - 1) / TextHeight));
                    half4 D1b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 2) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 2) / TextHeight));
                    half4 D2b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 2) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight));
                    half4 D3b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 2) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 0) / TextHeight));
                    half4 D4b = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 2) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 - 1) / TextHeight));

                    if (Height_Mode == 2)
                        TrueHeight = HeightTranslator(ComplexValueCalculation(A1.r, A2.r, A3.r, A4.r, B1.r, B2.r, B3.r, B4.r, C1.r, C2.r, C3.r, C4.r, D1.r, D2.r, D3.r, D4.r, LocalDistance));
                    if (Temperature_Mode == 2)
                        TrueTemperature = TemperatureTranslator(ComplexValueCalculation(A1.g, A2.g, A3.g, A4.g, B1.g, B2.g, B3.g, B4.g, C1.g, C2.g, C3.g, C4.g, D1.g, D2.g, D3.g, D4.g, LocalDistance));
                    if (Slant_Mode == 2)
                        TrueSlant = SlantTranslator(ComplexValueCalculation(A1.b, A2.b, A3.b, A4.b, B1.b, B2.b, B3.b, B4.b, C1.b, C2.b, C3.b, C4.b, D1.b, D2.b, D3.b, D4.b, LocalDistance));
                    if (FlowingWater_Mode == 2)
                        TrueFlowingWater = FlowingWaterTranslator(ComplexValueCalculation(A1b.r, A2b.r, A3b.r, A4b.r, B1b.r, B2b.r, B3b.r, B4b.r, C1b.r, C2b.r, C3b.r, C4b.r, D1b.r, D2b.r, D3b.r, D4b.r, LocalDistance));
                    if (Rainfall_Mode == 2)
                        TrueRainfall = RainfallTranslator(ComplexValueCalculation(A1b.g, A2b.g, A3b.g, A4b.g, B1b.g, B2b.g, B3b.g, B4b.g, C1b.g, C2b.g, C3b.g, C4b.g, D1b.g, D2b.g, D3b.g, D4b.g, LocalDistance));
                    if (WarterBiomePresence_Mode == 2)
                        TrueWarterBiomePresence = ComplexValueCalculation(A1b.b, A2b.b, A3b.b, A4b.b, B1b.b, B2b.b, B3b.b, B4b.b, C1b.b, C2b.b, C3b.b, C4b.b, D1b.b, D2b.b, D3b.b, D4b.b, LocalDistance);
                }


                //Layers
                int LayerTexSize = (LayerNumber / 4);
                if (LayerTexSize * 4 < LayerNumber) {
                    LayerTexSize++;
                }
                float LayerValues[100];
                half2 CasePos00 = half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight);
                half2 CasePos10 = half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight);
                half2 CasePos01 = half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight);
                half2 CasePos11 = half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight);
                if (CasePos00.x < 0) {
                    CasePos00.x = CasePos00.x + 1;
                }if (CasePos00.x > 1) {
                    CasePos00.x = CasePos00.x - 1;
                }
                if (CasePos00.y < 0) {
                    CasePos00.y = 0;
                }if (CasePos00.y > 1) {
                    CasePos00.y = 1;
                }

                if (CasePos10.x < 0) {
                    CasePos10.x = CasePos10.x + 1;
                }if (CasePos10.x > 1) {
                    CasePos10.x = CasePos10.x - 1;
                }
                if (CasePos10.y < 0) {
                    CasePos10.y = 0;
                }if (CasePos10.y > 1) {
                    CasePos10.y = 1;
                }

                if (CasePos01.x < 0) {
                    CasePos01.x = CasePos01.x + 1;
                }if (CasePos01.x > 1) {
                    CasePos01.x = CasePos01.x - 1;
                }
                if (CasePos01.y < 0) {
                    CasePos01.y = 0;
                }if (CasePos01.y > 1) {
                    CasePos01.y = 1;
                }

                if (CasePos11.x < 0) {
                    CasePos11.x = CasePos11.x + 1;
                }if (CasePos11.x > 1) {
                    CasePos11.x = CasePos11.x - 1;
                }
                if (CasePos11.y < 0) {
                    CasePos11.y = 0;
                }if (CasePos11.y > 1) {
                    CasePos11.y = 1;
                }
                
                for (int Val = 0; Val < LayerTexSize; Val++) {
                    
                    half4 finalOutput = tex2D(_LayerTex, half2((CasePos00.x + Val)/ (LayerTexSize *2), CasePos00.y)) * (1 - DistanceFromCenterOfPixel.x) * (1 - DistanceFromCenterOfPixel.y);
                    finalOutput += tex2D(_LayerTex, half2((CasePos10.x + Val) / (LayerTexSize * 2), CasePos10.y)) * (DistanceFromCenterOfPixel.x) * (1 - DistanceFromCenterOfPixel.y);
                    finalOutput += tex2D(_LayerTex, half2((CasePos01.x + Val) / (LayerTexSize * 2), CasePos01.y)) * (1 - DistanceFromCenterOfPixel.x) * (DistanceFromCenterOfPixel.y);
                    finalOutput += tex2D(_LayerTex, half2((CasePos11.x + Val) / (LayerTexSize * 2), CasePos11.y)) * (DistanceFromCenterOfPixel.x) * (DistanceFromCenterOfPixel.y);
                    
                    LayerValues[Val * 4 + 0] = finalOutput.r;
                    LayerValues[Val * 4 + 1] = finalOutput.g;
                    LayerValues[Val * 4 + 2] = finalOutput.b;
                    LayerValues[Val * 4 + 3] = finalOutput.a;

                }
                
                //Calc the color

                o.Alpha = (1);
                
               
                //River
                /*if (TrueHeight > 0) {
                    bool IsOnRiver = false;
                    float RiverExistanceSize = _TempData2;
                    float RiverMaxSize = _BlurAmount;
                    float Water00 = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight)).b;
                    float Water01 = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight)).b;
                    float Water10 = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5) / TextHeight)).b;
                    float Water11 = tex2D(_SecondTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + 1) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + 1) / TextHeight)).b;
                    if (Water00 > RiverExistanceSize && Water01 > RiverExistanceSize && Water10 > RiverExistanceSize && Water11 > RiverExistanceSize && RiverMaxSize != 0) {
                        IsOnRiver = true;
                    }
                    if (Water00 > RiverExistanceSize && IsOnRiver == false) {
                        float Dist00 = sqrt(DistanceFromCenterOfPixel.x * DistanceFromCenterOfPixel.x + DistanceFromCenterOfPixel.y * DistanceFromCenterOfPixel.y);
                        if (Dist00 < (Water00 - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }
                    if (Water10 > RiverExistanceSize && IsOnRiver == false) {
                        float Dist10 = sqrt((1 - DistanceFromCenterOfPixel.x) * (1 - DistanceFromCenterOfPixel.x) + DistanceFromCenterOfPixel.y * DistanceFromCenterOfPixel.y);
                        if (Dist10 < (Water10 - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }
                    if (Water01 > RiverExistanceSize && IsOnRiver == false) {
                        float Dist01 = sqrt(DistanceFromCenterOfPixel.x * DistanceFromCenterOfPixel.x + (1 - DistanceFromCenterOfPixel.y) * (1 - DistanceFromCenterOfPixel.y));
                        if (Dist01 < (Water01 - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }
                    if (Water11 > RiverExistanceSize && IsOnRiver == false) {
                        float Dist11 = sqrt((1 - DistanceFromCenterOfPixel.x) * (1 - DistanceFromCenterOfPixel.x) + (1 - DistanceFromCenterOfPixel.y) * (1 - DistanceFromCenterOfPixel.y));
                        if (Dist11 < (Water11 - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }
                    if (Water00 > RiverExistanceSize && Water10 > RiverExistanceSize && IsOnRiver == false) {
                        float DistX0 = DistanceFromCenterOfPixel.y;
                        float LocalWater = Water00 * (1 - DistanceFromCenterOfPixel.x) + Water10 * DistanceFromCenterOfPixel.x;
                        if (DistX0 < (LocalWater - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }
                    if (Water01 > RiverExistanceSize && Water11 > RiverExistanceSize && IsOnRiver == false) {
                        float DistX1 = 1-DistanceFromCenterOfPixel.y;
                        float LocalWater = Water01 * (1 - DistanceFromCenterOfPixel.x) + Water11 * DistanceFromCenterOfPixel.x;
                        if (DistX1 < (LocalWater - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }
                    if (Water00 > RiverExistanceSize && Water01 > RiverExistanceSize && IsOnRiver == false) {
                        float Dist0X = DistanceFromCenterOfPixel.x;
                        float LocalWater = Water00 * (1 - DistanceFromCenterOfPixel.y) + Water01 * DistanceFromCenterOfPixel.y;
                        if (Dist0X < (LocalWater - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }
                    if (Water10 > RiverExistanceSize && Water11 > RiverExistanceSize && IsOnRiver == false) {
                        float Dist1X =1- DistanceFromCenterOfPixel.x;
                        float LocalWater = Water10 * (1 - DistanceFromCenterOfPixel.y) + Water11 * DistanceFromCenterOfPixel.y;
                        if (Dist1X < (LocalWater - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }
                    if (Water00 > RiverExistanceSize && Water11 > RiverExistanceSize && IsOnRiver == false) {
                        float DistDiagPos = abs((-DistanceFromCenterOfPixel.y)-(-DistanceFromCenterOfPixel.x))/sqrt(2);
                        float LocalWater = Water00 * (1 - (DistanceFromCenterOfPixel.y + DistanceFromCenterOfPixel.x)/2) + Water11 * (DistanceFromCenterOfPixel.y + DistanceFromCenterOfPixel.x) / 2;
                        if (DistDiagPos < (LocalWater - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }
                    if (Water01 > RiverExistanceSize && Water10 > RiverExistanceSize && IsOnRiver == false) {
                        float DistDiagPos = abs((1-DistanceFromCenterOfPixel.y) - (-DistanceFromCenterOfPixel.x)*-1) / sqrt(2);
                        float LocalWater = Water00 * (1 - (DistanceFromCenterOfPixel.y + DistanceFromCenterOfPixel.x) / 2) + Water11 * (DistanceFromCenterOfPixel.y + DistanceFromCenterOfPixel.x) / 2;
                        if (DistDiagPos < (LocalWater - RiverExistanceSize) * RiverMaxSize) {
                            IsOnRiver = true;
                        }
                    }

                    if (IsOnRiver) {
                        o.Albedo = half4(0, 0, 1, 0).rgb;
                        o.Albedo = river_Color.rgb;
                    }
                }
                */
                o.Albedo = half4(1, 1, 0, 0).rgb;
                float TotalBiomePresence = 0;
                half4 Color = half4(0, 0, 0, 0);
                float TotalWaterBiomePresence = 0;
                half4 WaterColor = half4(0, 0, 0, 0);
                for (int i = 0; i < BiomeNumbers; i++) {
                    int base = i * BiomeLength;
                    float BiomeMinAltitude = Biomes[base + 3];
                    float BiomeMaxAltitude = Biomes[base + 4];
                    float BiomeMinTemperature = Biomes[base + 5];
                    float BiomeMaxTemperature = Biomes[base + 6];
                    float BiomeMinRainfall = Biomes[base + 7];
                    float BiomeMaxRainfall = Biomes[base + 8];
                    float BiomeMinFlowingWater = Biomes[base + 9];
                    float BiomeMaxFlowingWater = Biomes[base + 10];
                    float BiomeAltSaturation = Biomes[base + 11];
                    float BiomeWaterSaturation = Biomes[base + 12];
                    float BiomeTemperatureSaturation = Biomes[base + 13];
                    float IsSea = Biomes[base + 14];
                    float IsWaterType = Biomes[base + 15];


                    half BiomeScore = BiomePresence(TrueHeight, TrueTemperature, TrueFlowingWater, TrueRainfall, BiomeMinAltitude, BiomeMaxAltitude, BiomeAltSaturation, BiomeMinTemperature, BiomeMaxTemperature, BiomeTemperatureSaturation, BiomeMinRainfall, BiomeMaxRainfall, BiomeMinFlowingWater, BiomeMaxFlowingWater, BiomeWaterSaturation, IsSea, IsWaterType);
                    if (BiomeScore <= 0) {
                        continue;
                    }
                    else {
                        for (int LayerId = 0; LayerId < LayerNumber; LayerId++) {
                            int StartIndex = base + 16 + LayerId * 4;
                            if (Biomes[StartIndex] == 1) {
                                BiomeScore = BiomeScore * BiomeLayer(LayerValues[LayerId], Biomes[StartIndex + 1], Biomes[StartIndex+2], Biomes[StartIndex+3]);
                            }
                        }
                    }

                    if (IsWaterType == 1) {
                        if (BiomeScore > 0) {
                            WaterColor += half4(Biomes[base + 0], Biomes[base + 1], Biomes[base + 2], 0) * BiomeScore;
                            TotalWaterBiomePresence += BiomeScore;
                        }
                    }
                    else if (BiomeScore > 0) {
                        Color += half4(Biomes[base + 0], Biomes[base + 1], Biomes[base + 2], 0) * BiomeScore;
                        TotalBiomePresence += BiomeScore;
                    }
                }
                if (TotalBiomePresence > 0) {
                    o.Albedo = Color / TotalBiomePresence;
                }
                else if(TotalWaterBiomePresence > 0) {
                    o.Albedo = WaterColor / TotalWaterBiomePresence;
                }
                
                float slantFactor = 0.10 * TrueSlant / (HeightTranslator(1));
                slantFactor = -20 * slantFactor;
                if (slantFactor > 1) {
                    slantFactor = 1;
                }
                slantFactor = min(1, (4 + (10 * slantFactor)) * 0.2);

                float altitudeFactor = min(1, (0.5 + ((TrueHeight - HeightTranslator(0)) / (HeightTranslator(1) - HeightTranslator(0)))) * 0.6);

                o.Albedo *= max(0.2,slantFactor * altitudeFactor);

               
                /*if (CorrectUv.x < 0.0001  || CorrectUv.x > 0.9999) {
                    o.Albedo = half4(CorrectUv.x, CorrectUv.y, 0, 1);
                }*/
                //o.Albedo = half4(CorrectUv.x, CorrectUv.y, 0, 1);
                //CorrectUv = IN.uv_AltTex;
                //int UvCible = IN.uv_AltTex.x;
                /*while (CorrectUv.x < 0) {
                    CorrectUv.x += 1;
                }
                while (CorrectUv.x > 1) {
                    CorrectUv.x -= 1;
                }*/
                //CorrectUv = half2( CorrectUv.x - UvCible, CorrectUv.y) ;
                //o.Albedo = tex2D(_AltTex, uv).rgb;
                //o.Albedo = half4(LayerValues[0], LayerValues[1], 0, 0).rgb;
                //o.Albedo = half4(CorrectUv.x/2, 1- (CorrectUv.x / 2), 0, 0).rgb;
                //o.Albedo = half4(IN.uv_AltTex.x - (CaseIndex.x + 0.5) / TextLenght, (IN.uv_AltTex.y - (CaseIndex.y + 0.5) / TextLenght), 0, 0).rgb;
                //o.Albedo = half4(DistanceFromCenterOfPixel.x +0.5, DistanceFromCenterOfPixel.y+0.5, 0, 0).rgb;
                //o.Albedo = finalOutput.rgb;
            }

            ENDCG
        }
        
            
}
