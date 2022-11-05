using System;

namespace Scorpian.Maths;

public static class MathExt
{
    private static readonly PerlinNoise Perlin = new();

    public static double PerlinNoise(double x, double y)
    {
        return Perlin.Perlin(x, y, 0);
    }

    public static double Lerp(double a, double b, double t)
    {
        return (1.0 - t) * a + b * t;
    }

    public static double InverseLerp(double a, double b, double v)
    {
        return (v - a) / (b - a);
    }
    
    public static double SumNoise(double x, double y, int octaves, float persistance, float startFrequency)
    {
        var amplitude = 1.0;
        var frequency = startFrequency;
        var noiseSum = 0.0;
        var amplitudeSum = 0.0;
        for (var i = 0; i < octaves; i++)
        {
            noiseSum += amplitude * PerlinNoise(x * frequency, y * frequency);
            amplitudeSum += amplitude;
            amplitude *= persistance;
            frequency *= 2;

        }
            
        return noiseSum / amplitudeSum; // set range back to 0-1
    }
    
    public static double RangeMap(double inputValue, double inMin, double inMax, double outMin, double outMax)
    {
        return outMin + (inputValue - inMin) * (outMax - outMin) / (inMax - inMin);
    }
    
    public static double ToRad(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    public static double ToDegrees(double radians)
    {
        return radians * 180 / Math.PI;
    }

    public static double ToBearing(double radians)
    {
        // convert radians to degrees (as bearing: 0...360)
        return (ToDegrees(radians) + 360) % 360;
    }
}