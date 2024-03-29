﻿namespace CodinGame.Solo.Puzzles.Easy;

using System;
using System.Collections.Generic;
using System.Globalization;

public static class OneDSpreadsheet
{
    public static void Main(string[] args)
    {
        int cellsCount;
        var cells = new List<string>();

        if (args.Length == 0)
        {
            cellsCount = int.Parse(Console.ReadLine()!, CultureInfo.InvariantCulture);
            for (int i = 0; i < cellsCount; i++)
            {
                cells.Add(Console.ReadLine()!);
            }
        }
        else
        {
            cellsCount = int.Parse(args[0], CultureInfo.InvariantCulture);
            for (int i = 1; i < cellsCount + 1; i++)
            {
                cells.Add(args[i]);
            }
        }

        for (int i = 0; i < cellsCount; i++)
        {
            Console.WriteLine($"{GetValue(cells, i, new int[cellsCount])}");
        }
    }

    private static int GetValue(List<string> cells, int index, int[] cache)
    {
        if (cache[index] == 0)
        {
            var parameters = cells[index].Split(' ');
            var arg1 = ParseArgument(cells, parameters[1], cache);
            var arg2 = ParseArgument(cells, parameters[2], cache);

            cache[index] = parameters[0] switch
            {
                "VALUE" => arg1,
                "ADD" => arg1 + arg2,
                "SUB" => arg1 - arg2,
                "MULT" => arg1 * arg2,
                _ => throw new InvalidOperationException("Invalid Operand"),
            };
        }

        return cache[index];
    }

    private static int ParseArgument(List<string> cells, string parameter, int[] cache)
    {
        if (parameter[0] == '$')
        {
            var index = int.Parse(parameter.Substring(1), CultureInfo.InvariantCulture);
            cache[index] = cache[index] == 0 ? GetValue(cells, index, cache) : cache[index];
            return cache[index];
        }

        if (parameter[0] == '_')
        {
            return 0;
        }

        return int.Parse(parameter, CultureInfo.InvariantCulture);
    }
}

public static class OneDSpreadsheetTestCases
{
    public static readonly string[] SimpleDependency = new string[]
    {
        "2",
        "VALUE 3 _",
        "ADD $0 4",
    };

    public static readonly string[] DeepBirecursion = new string[]
    {
        "92",
        "SUB $33 $64",
        "ADD $60 $60",
        "ADD $61 $61",
        "SUB $76 $80",
        "SUB $25 $59",
        "ADD $58 $28",
        "ADD $88 $59",
        "ADD $32 $32",
        "ADD $83 $21",
        "ADD $69 $39",
        "ADD $57 $64",
        "ADD $26 $26",
        "ADD $1 $1",
        "SUB $62 $68",
        "ADD $73 $1",
        "ADD $50 $27",
        "SUB $24 $2",
        "ADD $14 $12",
        "ADD $10 $89",
        "SUB $67 $35",
        "ADD $58 $58",
        "ADD $7 $7",
        "SUB $0 $89",
        "ADD $20 $20",
        "SUB $43 $61",
        "SUB $53 $11",
        "ADD $37 $37",
        "ADD $82 $47",
        "ADD $90 $2",
        "ADD $89 $89",
        "ADD $85 $85",
        "SUB $91 $47",
        "ADD $69 $69",
        "SUB $46 $86",
        "SUB $42 $20",
        "ADD $12 $12",
        "ADD $56 $8",
        "ADD $72 $72",
        "ADD $9 $32",
        "ADD $30 $77",
        "ADD $80 $48",
        "ADD $79 $81",
        "SUB $16 $58",
        "SUB $44 $56",
        "SUB $63 $21",
        "ADD $20 $5",
        "SUB $49 $81",
        "ADD $54 $54",
        "ADD $29 $18",
        "SUB $34 $23",
        "ADD $47 $47",
        "SUB $74 $32",
        "SUB $17 $72",
        "SUB $71 $26",
        "ADD $59 $59",
        "ADD $15 $68",
        "ADD $21 $21",
        "ADD $86 $41",
        "ADD $2 $2",
        "ADD $11 $11",
        "ADD $80 $80",
        "ADD $56 $56",
        "SUB $31 $50",
        "SUB $51 $7",
        "ADD $86 $86",
        "ADD $72 $35",
        "SUB $75 $30",
        "SUB $70 $12",
        "ADD $50 $50",
        "ADD $30 $30",
        "SUB $84 $1",
        "SUB $52 $37",
        "VALUE 1 _",
        "ADD $40 $60",
        "SUB $66 $69",
        "SUB $13 $85",
        "SUB $22 $29",
        "ADD $55 $85",
        "ADD $37 $65",
        "ADD $23 $45",
        "ADD $29 $29",
        "ADD $23 $23",
        "ADD $54 $6",
        "ADD $38 $7",
        "SUB $3 $60",
        "ADD $68 $68",
        "ADD $81 $81",
        "ADD $78 $26",
        "ADD $87 $11",
        "ADD $64 $64",
        "ADD $61 $36",
        "SUB $4 $54",
    };
}