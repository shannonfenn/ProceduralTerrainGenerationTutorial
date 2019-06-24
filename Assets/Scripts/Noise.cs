using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[mapWidth, mapHeight];
        float minNoiseHeight = float.MaxValue;
        float maxNoiseHeight = float.MinValue;

        System.Random prng = new System.Random(seed);
        // to allow each octave to be sampled from different regions
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i=0; i < octaves; i++) {
            octaveOffsets[i] = new Vector2(
                prng.Next(-100000, 100000) + offset.x,
                prng.Next(-100000, 100000) + offset.y);
        }

        // clamp scale
        scale = Mathf.Max(scale, 0.0001f);

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for(int i=0; i<octaves; i++) {
                    // original sampling
                    // float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    // float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    // fix to keep octaves in positional sync
                    float sampleX = ((x - halfWidth) / scale + octaveOffsets[i].x) * frequency;
                    float sampleY = ((y - halfHeight) / scale + octaveOffsets[i].y) * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    // shift to [-1, 1]
                    perlinValue = perlinValue * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                minNoiseHeight = Mathf.Min(minNoiseHeight, noiseHeight);
                maxNoiseHeight = Mathf.Max(maxNoiseHeight, noiseHeight);

                noiseMap[x,y] = noiseHeight;
            }
        }

        // normalise
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
            }
        }

        return noiseMap;
    }
}
