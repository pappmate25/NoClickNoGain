using System;
using UnityEngine;

public static class NumberFormatter
{
    private static readonly string[] Prefixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

    public static string FormatNumber(double number, int decimals = 3)
    {
        int prefixIndex = 0;

        while(number >= 1000 && prefixIndex < Prefixes.Length-1) //cserélhetõ pl logaritmikusra ha gond lenne a teljesítménnyel
        {
            number /= 1000;
            prefixIndex++;
        }

        // kerekítés, hogy ne térjen el a kiírt és a tényleges összeg
        double faktor = Mathf.Pow(10,decimals);
        number = Math.Floor(number * faktor) / faktor;

        string format = decimals switch
        {
            0 => "0",
            1 => "0.#",
            2 => "0.##",
            3 => "0.###",
            _ => $"0.{new string('#', decimals)}"
        };


        return $"{number.ToString(format)}{Prefixes[prefixIndex]}";
    }

    public static double RoundCalculatedNumber(double number, int decimals = 3)
    {
        int dividedByThousand = 0;

        while (number >= 1000)
        {
            number /= 1000;
            dividedByThousand++;
        }

        double faktor = Math.Pow(10, decimals);
        number = Math.Floor(number * faktor) / faktor * Math.Pow(faktor,dividedByThousand);


        return number;
    }
}
