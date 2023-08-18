using System;
using System.Collections.Generic;
using System.Linq;
using AutochaveamentoACCA;

public class Chaveador
{
    // ... (other methods and code)
    private static Random random = new Random();

    public static (List<Atleta>, int) GenerateBracket(string filePath)
    {
        List<Atleta> athletes = ReadAthleteFromCSV(filePath);

        // Sort athletes based on "Agremiação" to apply Rule 1
        athletes = athletes.OrderBy(a => a.agrem).ToList();

        List<Atleta> bracket = new List<Atleta>();

        // Separate athletes into different categories based on belts (Low Belts: Branca, Cinza, Azul, Amarela, Laranja, Verde)
        var lowBelts = athletes.Where(a => a.faixa == "BRANCA" || a.faixa == "CINZA" || a.faixa == "AZUL" || a.faixa == "AMARELA" || a.faixa == "LARANJA" || a.faixa == "VERDE").ToList();

        // Separate athletes into different categories based on belts (High Belts: Roxa, Marrom, Preta)
        var highBelts = athletes.Where(a => a.faixa == "ROXA" || a.faixa == "MARROM" || a.faixa == "PRETA").ToList();

        // Shuffle the athletes within each category
        lowBelts = ShuffleAthletes(lowBelts);
        highBelts = ShuffleAthletes(highBelts);

        // Combine the shuffled lists back into a single list of shuffled athletes
        athletes = lowBelts.Concat(highBelts).ToList();

        int numAthletes = athletes.Count;

        // Determine the number of free pass spots
        int numFreePassSpots = 0;
        int bracketBegin = 0;
        if (numAthletes % 2 == 1)
        {
            numFreePassSpots = 1;
            bracketBegin = 1;
        }
        else if ((numAthletes/2) % 2 == 1) 
        {
            numFreePassSpots = 2;
            bracketBegin = 1;
        }
        Console.WriteLine(numFreePassSpots);

        // Select athletes with the highest belts for the free pass spots
        var athletesWithFreePass = athletes.OrderByDescending(a => GetBeltPriority(a.faixa)).ToList();

        // If only one free pass spot, treat the last position as the spot for the free pass
        if (numFreePassSpots == 1)
        {
            bracket.Add(athletesWithFreePass[0]);
            athletes.Remove(athletesWithFreePass[0]);
        }
        // If two free pass spots, treat the first and last positions as the spots for the free passes
        else if (numFreePassSpots == 2)
        {
            bracket.Insert(0, athletesWithFreePass[1]);
            athletes.Remove(athletesWithFreePass[1]);
            bracket.Add(athletesWithFreePass[0]);
            athletes.Remove(athletesWithFreePass[0]);
        }

        // Create the first round of the bracket while enforcing safeguards against scenarios 1, 2, and 3
        for (int i = 0; i < athletes.Count; i = i + 2)
        {
            var athlete1 = athletes[i];
            var athlete2 = athletes[i + 1];
            // Check if athletes are from the same academy (Rule 1)
            if (athlete1.agrem == athlete2.agrem)
            {
                // Swap one of the athletes with another from a different agrem to enforce Rule 1
                Console.WriteLine("Mesma agrem!");
                for (int j = 0; j < athletes.Count; j++)
                {
                    if (athletes[j].agrem != athlete2.agrem)
                    {
                        (athletes[i + 1], athletes[j]) = (athletes[j], athletes[i + 1]);
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < athletes.Count; i = i + 2)
        {
            var athlete1 = athletes[i];
            var athlete2 = athletes[i + 1];

            Console.WriteLine("Comparação:\n" + athlete1.nome + " Faixa: " + athlete1.faixa + " Agrem: " + athlete1.agrem);
            Console.WriteLine(athlete2.nome + " Faixa: " + athlete2.faixa + " Agrem: " + athlete2.agrem);

            // Check if both athletes are high belts (Rule 2)
            if (IsHighBelt(athlete1.faixa) || IsHighBelt(athlete2.faixa))
            {
                Console.WriteLine("Ambos faixa alta!");
                // If two high belts would face each other, find a low belt vs. low belt case and swap one athlete
                for (int j = 0; j < athletes.Count; j = j + 2)
                {
                    if (!IsHighBelt(athletes[j].faixa))
                    {
                        if (!IsHighBelt(athletes[j+1].faixa)) 
                        {
                            (athletes[i + 1], athletes[j]) = (athletes[j], athletes[i + 1]);
                            break;
                        }
                    }
                }
            }
        }
        bracket.InsertRange(bracketBegin, athletes);

        return (bracket, numFreePassSpots);
    }

    private static List<Atleta> ShuffleAthletes(List<Atleta> athletes)
    {
        // Fisher-Yates shuffle algorithm to randomly shuffle the athletes
        for (int i = athletes.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            var temp = athletes[i];
            athletes[i] = athletes[j];
            athletes[j] = temp;
        }
        return athletes;
    }

    private static bool IsHighBelt(string belt)
    {
        return belt == "ROXA" || belt == "MARROM" || belt == "PRETA";
    }

    private static int GetBeltPriority(string belt)
    {
        switch (belt)
        {
            case "BRANCA":
                return 0;
            case "CINZA":
                return 1;
            case "AZUL":
                return 2;
            case "AMARELA":
                return 3;
            case "LARANJA":
                return 4;
            case "VERDE":
                return 5;
            case "ROXA":
                return 6;
            case "MARROM":
                return 7;
            case "PRETA":
                return 8;
            default:
                return 0;
        }
    }

    public static List<Atleta> ReadAthleteFromCSV(string filePath)
    {
        List<Atleta> athletes = new List<Atleta>();

        try
        {
            using (var reader = new StreamReader(filePath))
            {
                // Skip the header line
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    string nome = values[0];
                    int idade = int.Parse(values[1]);
                    char gen = char.Parse(values[2]);
                    string faixa = values[3];
                    string agrem = values[4];
                    string categ = values[5];

                    Atleta atleta = new Atleta(nome, idade, "NDA", "NDA", gen, faixa, agrem, categ);
                    athletes.Add(atleta);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file '{filePath}': {ex.Message}");
        }

        return athletes;
    }
}
