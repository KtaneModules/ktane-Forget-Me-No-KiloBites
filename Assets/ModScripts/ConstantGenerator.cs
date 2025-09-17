using System;
using System.Collections.Generic;
using System.Linq;
using KModkit;

public struct Constant
{
    public ConstantType Type { get; private set; }
    public int[] ConstantDigits { get; private set; }

    public Constant(ConstantType type, int[] constantDigits)
    {
        Type = type;
        ConstantDigits = constantDigits;
    }
}

public class ConstantGenerator
{
    public List<Constant> Constants { get; private set; }
    private int[] ObtainConstants(ConstantType constant)
    {
        string constantString;

        switch (constant)
        {
            case ConstantType.Pi:
                constantString = "1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679";
                break;
            case ConstantType.Eulers:
                constantString = "7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274";
                break;
            case ConstantType.SquareRoot2:
                constantString = "4142135623730950488016887242096980785696718753769480731766797379907324784621070388503875343276415727";
                break;
            case ConstantType.NaturalLogarithm2:
                constantString = "6931471805599453094172321214581765680755001343602552541206800094933936219696947156058633269964186875";
                break;
            case ConstantType.GoldenRatio:
                constantString = "6180339887498948482045868343656381177203091798057628621354486227052604628189024497072072041893911374";
                break;
            case ConstantType.Euler_Mascheroni:
                constantString = "5772156649015328606065120900824024310421593359399235988057672348848628161332625388471532653213384543";
                break;
            case ConstantType.Plastic:
                constantString = "3247179572447460259609088544780973407344040569017333647511773196849943330457981735351419943227460373";
                break;
            case ConstantType.Feigenbaum:
                constantString = "6692016091029906718532038204662016172581855774757686327456513430046430596155893079608658315461735027";
                break;
            case ConstantType.Conways:
                constantString = "3035772690342963912570991121525518907307025046597086782995734358261341345935722034301986265121843828";
                break;
            case ConstantType.Lambert_W1:
                constantString = "5671432904097840003294836545880241725504396327895736206961023466748606876187663252409701835347037716";
                break;
            default:
                throw new InvalidOperationException("The constant given doesn't exist.");
        }

        return constantString.Select(x => int.Parse(x.ToString())).ToArray();
    }

    private readonly int validIx;

    public ConstantGenerator(KMBombInfo bomb)
    {
        var constants = new List<Constant>();

        var orderConditions = new[]
        {
            bomb.IsPortPresent(Port.Parallel) && bomb.IsIndicatorOn(Indicator.BOB),
            bomb.GetBatteryCount() > 5,
            bomb.GetIndicators().Count() > 3,
            bomb.GetPortCount() == 0,
            bomb.GetBatteryCount() == 0,
            bomb.GetSerialNumberNumbers().Count() == 4,
            bomb.GetModuleNames().Count() > 11 && bomb.GetModuleNames().Count() < 47,
            bomb.GetSerialNumber().Any("AEIOU".Contains),
            bomb.GetPortPlates().Any(x => x.Count() == 0),
            bomb.GetPorts().Distinct().Count() == 6,
            bomb.GetOnIndicators().Count() > bomb.GetOffIndicators().Count(),
            int.Parse(bomb.GetSerialNumber()[5].ToString()) % 2 == 0,
            int.Parse(bomb.GetSerialNumber()[2].ToString()) % 2 != 0,
            true
        };

        var orderPriority = new[]
        {
            Enumerable.Range(0, 4).ToArray(),
            Enumerable.Range(0, 4).Reverse().ToArray(),
            new[] { 0, 3, 2, 1 },
            new[] { 1, 3, 2, 0 },
            new[] { 2, 1, 3, 0 },
            new[] { 1, 0, 3, 2 },
            new[] { 3, 0, 1, 2 },
            new[] { 2, 0, 3, 1 },
            new[] { 1, 2, 0, 3 },
            new[] { 3, 1, 2, 0 },
            new[] { 0, 3, 1, 2 },
            new[] { 2, 1, 0, 3 },
            new[] { 2, 3, 0, 1 },
            new[] { 1, 0, 2, 3 }
        };

        var snValues = bomb.GetSerialNumber().Select(x => char.IsLetter(x) ? "-ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x) : int.Parse(x.ToString())).ToArray();

        var invalidValues = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        for (int i = 0; i < snValues.Length; i++)
        {
            while (!invalidValues.Contains(snValues[i]))
            {
                snValues[i]++;
                snValues[i] %= 10;
            }

            invalidValues.Remove(snValues[i]);

            constants.Add(new Constant((ConstantType)snValues[i], ObtainConstants((ConstantType)snValues[i])));
        }

        validIx = Enumerable.Range(0, orderConditions.Length).First(x => orderConditions[x]);

        foreach (var remainingIx in orderPriority[validIx].ToArray())
            constants.Add(new Constant((ConstantType)invalidValues[remainingIx], ObtainConstants((ConstantType)invalidValues[remainingIx])));

        Constants = constants;
    }

    public override string ToString()
    {
        switch (validIx)
        {
            case 0:
                return "Parallel port and a lit BOB indicator present";
            case 1:
                return "More than 5 batteries present";
            case 2:
                return "More than 3 indicators present";
            case 3:
                return "No ports are present";
            case 4:
                return "No batteries are present";
            case 5:
                return "Exactly 4 digits in the serial number present";
            case 6:
                return "Number of modules present are greater than 11, but less than 47";
            case 7:
                return "Vowel in the serial number present";
            case 8:
                return "Empty port plate present";
            case 9:
                return "All port types are present";
            case 10:
                return "There are more lit than unlit indicators present";
            case 11:
                return "Sixth character of the serial number is even";
            case 12:
                return "Third character of the serial number is odd";
        }

        return "No conditions applied";
    }
}