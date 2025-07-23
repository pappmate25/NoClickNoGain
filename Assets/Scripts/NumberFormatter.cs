using System;
using System.Globalization;
using UnityEngine;

public static class NumberFormatter
{
    private static readonly string[] prefixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

    public static string FormatNumber(double number, int decimals = 2)
    {
        int prefixIndex = 0;

        while (number >= 1000 && prefixIndex < prefixes.Length - 1) //cser�lhet� pl logaritmikusra ha gond lenne a teljes�tm�nnyel
        {
            number /= 1000;
            prefixIndex++;
        }

        // kereketes, hogy ne terjen el a kiirt es a tenyleges összeg
        double faktor = Mathf.Pow(10, decimals);
        number = Math.Floor(number * faktor) / faktor;

        string format = decimals switch
        {
            0 => "0",
            1 => "0.#",
            2 => "0.##",
            //3 => "0.###",
            _ => $"0.{new string('#', decimals)}"
        };


        return $"{number.ToString(format, CultureInfo.InvariantCulture)}{prefixes[prefixIndex]}";
    }

    public static double RoundCalculatedNumber(double number, int decimals = 2)
    {
        int dividedByHundred = 0;

        while (number >= 100)
        {
            number /= 100;
            dividedByHundred++;
        }

        double faktor = Math.Pow(10, decimals);
        number = Math.Floor(number * faktor) / faktor * Math.Pow(faktor, dividedByHundred);


        return number;
    }
}
