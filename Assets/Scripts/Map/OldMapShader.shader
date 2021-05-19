// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "WorldMap/Coastline"
{
    Properties
    {
        [PerRendererData] _MainTex("Texture", 2D) = "white" {}
        //[PerRendererData] _AltTex("Sprite Texture", 2D) = "white" {}
        //[PerRendererData] _SecondTex("Sprite Texture2", 2D) = "white" {}
        //[HideInInspector]_Cutoff("Alpha Cutoff", Range(0,1)) = 0.0001
        
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
        //_CoastSize("Coast Size", Range(0, 1)) = 0.5 //400 for large map view && 150 for zoomed map
        _CoastSeaLand("Coast on sea or on land", Range(0, 1)) = 0.5
        _Slant("Slant", Range(0, 1)) = 0.75

        [Header(Colors)]
        Sea_Color("Sea_Color", color) = (0.984, 0.9019, 0.7725,0)
        Coast_Color("Sea_Color", color) = (0.2901, 0.1529, 0.0509,0)
        Water_Presence_Color("Water_Presence_Colot", color) = (0.6078, 0.4117, 0.2823,0)
        Base_Color("Base_Color", color) = (0.9137, 0.7764, 0.5333,0)
        Montain_Color("Montain_Color", color) = (0.7352, 0.5349, 0.3298,0)
        
    }
        CGINCLUDE

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
        ENDCG

            SubShader
        {

            Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }


            LOD 300
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha


            CGPROGRAM

            #pragma surface surf ToonRamp vertex:vert alpha alphatest:_Cutoff addshadow
            #pragma lighting ToonRamp
            #include "UnityCG.cginc"
            //#pragma fragment frag
            //#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)


            uniform sampler2D _AltTex;
            sampler2D _MainTex;
            sampler2D _SecondTex; 
            int Height_Mode;
            int Temperature_Mode;
            int Slant_Mode;
            int FlowingWater_Mode;
            int Rainfall_Mode;
            int WarterBiomePresence_Mode;
            int Linear_Distance_mode;
            int Complex_Distance_mode;
            float _CoastSize;
            float _CoastSeaLand;
            float _Slant;
            float4 _AltTex_TexelSize;
            half4 Sea_Color;
            half4 Coast_Color;
            half4 Water_Presence_Color;
            half4 Base_Color;
            half4 Montain_Color;


            // for hard double-sided proximity lighting
            inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
            {
                half4 c;
                c.rgb = s.Albedo * 0.5;
                c.a = s.Alpha;
                return c;
            }


            struct Input
            {
                //float4 Temp : POSITION;
                //float2 uv_MainTex: TEXCOORD0;
                float2 uv_AltTex : TEXCOORD0;
                //float2 uv_SecondTex : TEXCOORD0;
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
                while (CorrectUv.x < 0) {
                    CorrectUv.x += 1;
                }
                while (CorrectUv.x >= 1) {
                    CorrectUv.x -= 1;
                }

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
                    if(Temperature_Mode == 0)
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
            

                //Coast
                bool IsCoast = false;
                float CoastSize = 1;
                
                CoastSize = _CoastSize/2;

                float Val00 = HeightTranslator(tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x - CoastSize) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y - CoastSize) / TextHeight)).r);
                float Val10 = HeightTranslator(tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y - CoastSize) / TextHeight)).r);
                float Val20 = HeightTranslator(tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x + CoastSize) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y - CoastSize) / TextHeight)).r);
                float Val01 = HeightTranslator(tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x - CoastSize) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y) / TextHeight)).r);
                float Val11 = HeightTranslator(tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y) / TextHeight)).r);
                float Val21 = HeightTranslator(tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x + CoastSize) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y) / TextHeight)).r);
                float Val02 = HeightTranslator(tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x - CoastSize) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y + CoastSize) / TextHeight)).r);
                float Val12 = HeightTranslator(tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y + CoastSize) / TextHeight)).r);
                float Val22 = HeightTranslator(tex2D(_AltTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x + CoastSize) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y + CoastSize) / TextHeight)).r);
                if (TrueHeight < 0) {
                    if (Val00 > 0 || Val10 > 0 || Val20 > 0 || Val01 > 0 || Val11 > 0 || Val21 > 0 || Val02 > 0 || Val12 > 0 || Val22 > 0) {
                        IsCoast = true;
                    }
                }
                if (TrueHeight > 0) {
                    if (Val00 < 0 || Val10 < 0 || Val20 < 0 || Val01 < 0 || Val11 < 0 || Val21 < 0 || Val02 < 0 || Val12 < 0 || Val22 < 0) {
                        IsCoast = true;
                    }
                }
                
                //Calc the color

                o.Alpha = (1);
                

                if (IsCoast == true) {
                    o.Albedo = Coast_Color;
                }
                else if (TrueHeight < 0 ) {
                    o.Albedo = Sea_Color;
                }
                else if (TrueWarterBiomePresence >= 0.5) {
                    o.Albedo = Water_Presence_Color;
                }
                else {
                    float slantFactor = _Slant * TrueSlant / (HeightTranslator(1));
                    slantFactor = -20 * slantFactor;
                    if (slantFactor > 1) {
                        slantFactor = 1;
                    }
                    if (slantFactor > 0.1f)
                    {
                        o.Albedo = (Montain_Color * slantFactor) + (Base_Color * (1 - slantFactor));
                    }
                    else {
                        o.Albedo = Base_Color;
                    }
                }

                //o.Albedo = tex2D(_MainTex, half2((CaseIndex.x + PositionDecay.x + 0.5 + DistanceFromCenterOfPixel.x - CoastSize) / TextLenght, (CaseIndex.y + PositionDecay.y + 0.5 + +DistanceFromCenterOfPixel.y - CoastSize) / TextHeight));
                
                //o.Albedo = half4(_AltTex_ST.x, _AltTex_ST.y, _AltTex_ST.z,0);

                //o.Albedo = half4(FinalH, FinalH, FinalH,0);
                //River
        
                //o.Albedo = half4(IN.uv_AltTex.x - (CaseIndex.x + 0.5) / TextLenght, (IN.uv_AltTex.y - (CaseIndex.y + 0.5) / TextLenght), 0, 0).rgb;
                //o.Albedo = half4(DistanceFromCenterOfPixel.x +0.5, DistanceFromCenterOfPixel.y+0.5, 0, 0).rgb;
                //o.Albedo = finalOutput.rgb;
            }

        ENDCG
        }

}
