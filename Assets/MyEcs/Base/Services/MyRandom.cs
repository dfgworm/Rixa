using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Linq;

public static class MyRandom
{
    public static bool Roll(float chance)
    {
        return Random.value <= chance;
    }
    public static T RollPick<T>(float chance, T val1, T val2)
    {
        return Random.value <= chance ? val1 : val2;
    }
    public static T Pick<T>(IEnumerable<T> array)
    {
        int len = array.Count();
        return array.ElementAt(Random.Range(0, len));
    }
    public static void Shuffle<T>(T[] array)
    {
        var indices = GetShuffledIndices(array.Length);
        for (int i = 1; i < array.Length; i++)
        {
            int index = indices[i];
            var val = array[i];
            array[i] = array[index];
            array[index] = val;
        }
    }
    public static int[] GetShuffledIndices(int len)
    {
        int[] indices = new int[len];
        float[] weights = new float[len];
        for (int i = 0; i < len; i++)
        {
            indices[i] = i;
            weights[i] = Random.value;
        }
        var sorted = from i in indices orderby weights[i] select i;
        return sorted.ToArray();
    }
}