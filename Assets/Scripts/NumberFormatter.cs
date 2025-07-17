using System;
using UnityEngine;

public static class NumberFormatter
{
    private static readonly string[] prefixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

    public static string FormatNumber(double number, int decimals = 3)
    {
        int prefixIndex = 0;

        while (number >= 1000 && prefixIndex < prefixes.Length - 1) //cser�lhet� pl logaritmikusra ha gond lenne a teljes�tm�nnyel
        {
            number /= 1000;
            prefixIndex++;
        }

        // kerek�t�s, hogy ne t�rjen el a ki�rt �s a t�nyleges �sszeg
        double faktor = Mathf.Pow(10, decimals);
        number = Math.Floor(number * faktor) / faktor;

        string format = decimals switch
        {
            0 => "0",
            1 => "0.#",
            2 => "0.##",
            3 => "0.###",
            _ => $"0.{new string('#', decimals)}"
        };


        return $"{number.ToString(format)}{prefixes[prefixIndex]}";
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
        number = Math.Floor(number * faktor) / faktor * Math.Pow(faktor, dividedByThousand);


        return number;
    }
}
