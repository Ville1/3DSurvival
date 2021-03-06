﻿using System.Collections.Generic;
using System.Text;

public class RNG
{
    private static RNG instance;

    private System.Random generator;

    private RNG()
    {
        generator = new System.Random();
    }

    public static RNG Instance
    {
        get {
            if (instance == null) {
                instance = new RNG();
            }
            return instance;
        }
    }

    public void Set_Seed(int seed)
    {
        generator = new System.Random(seed);
    }

    public int Next()
    {
        return generator.Next();
    }

    public int Next(int max)
    {
        return generator.Next(max + 1);
    }

    public int Next(int min, int max)
    {
        return generator.Next(min, max + 1);
    }

    public float Next_F()
    {
        return generator.Next(0, 101) * 0.01f;
    }

    public string String(int lenght)
    {
        if (lenght <= 0) {
            lenght = 1;
        }
        string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        StringBuilder result = new StringBuilder();
        while (result.Length < lenght) {
            result.Append(chars[Next(chars.Length - 1)]);
        }
        return result.ToString();
    }

    public T Item<T>(List<T> list)
    {
        return list[Next(list.Count - 1)];
    }

    public List<T> Shuffle<T>(List<T> list)
    {
        if (list.Count <= 1) {
            return list;
        }
        List<T> new_list = new List<T>();
        while (list.Count > 1) {
            T item = Item(list);
            list.Remove(item);
            new_list.Add(item);
        }
        new_list.Add(list[0]);
        return new_list;
    }
}
