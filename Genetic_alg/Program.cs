using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing;
using static Program;

class Program
{
    public class Chromosome
    {
        public long Fitness { get; set; }
        public List<int> ChromosomePath { get; set; }
    }

    //czytanie z liku INI
    static List<string> fileNameVector = new List<string>();
    static List<long> testCountVector = new List<long>();
    static List<int> solutionVector = new List<int>();
    static List<string> pathVector = new List<string>();
    static string outputFileName;
    //czytanie grafu
    static List<List<int>> matrix = new List<List<int>>(); //reprezentacja grafu w postaci macierzy
    static int N;//liczba wierzchowłów w grafie
    static int population_size = 500;
    static int generations = 1000;
    static int repAllowed = 150; // liczba iteracji bez poprawy wyniku
    static double pK = 0.1; //90%
    static double pM = 0.05; //5%
    static List<int> solution = new List<int>(); //przechowywanie optymalnej ścieżki
    static List<Chromosome> population = new List<Chromosome>();
    static int numberOfFirstVertex = 0;
    static long currentCost = 999999999;


    static void ReadFile(string FileName)
    {
        string line;

        using (StreamReader file = new StreamReader(FileName))//using zwalnia automatycznie zasoby po zakończnieu bloku
        {
            string fileName;
            int testCount;
            int solution;
            string path;

            while ((line = file.ReadLine()) != null)
            {
                string[] columns = line.Split();
                if (columns[0].Contains(".csv"))
                {
                    outputFileName = columns[0];
                    break;
                }
                if (columns.Length >= 4)
                {
                    fileName = columns[0];
                    fileNameVector.Add(fileName);
                    testCount = int.Parse(columns[1]);
                    testCountVector.Add(testCount);
                    solution = int.Parse(columns[2]);
                    solutionVector.Add(solution);
                    path = string.Join(" ", columns, 3, columns.Length - 3);
                    pathVector.Add(path);
                }
            }
        }
    }

    static void ReadMatrix(string FileName)
    {
        string line;
        int value;
        using (StreamReader file = new StreamReader(FileName))
        {
            while ((line = file.ReadLine()) != null)
            {
                string[] oneRow = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (oneRow.Length == 1)
                {
                    N = int.Parse(oneRow[0]);
                    continue;
                }
                if (oneRow.Length >= N)
                {
                    List<int> row = new List<int>();

                    for (int i = 0; i < N; i++)
                    {
                        value = int.Parse(oneRow[i]);
                        row.Add(value);
                    }
                    matrix.Add(row);
                }
            }
        }
    }
    static long CalculateCost(List<int> solve)
    {
        long cost = 0;
        for (int i = 0; i < solve.Count - 1; i++)
        {
            cost += matrix[solve[i]][solve[i + 1]];
        }
        cost += matrix[numberOfFirstVertex][solve[0]];
        cost += matrix[solve[^1]][numberOfFirstVertex];
        return cost;
    }
    static List<int> NeighboorSwap(List<int> oldSolution)
    {
        Random random = new Random();
        int randIndexI = random.Next(0, oldSolution.Count);
        int randIndexJ = random.Next(0, oldSolution.Count);
        if (randIndexI == randIndexJ)
        {
            return oldSolution;
        }

        int temp = oldSolution[randIndexI];
        oldSolution[randIndexI] = oldSolution[randIndexJ];
        oldSolution[randIndexJ] = temp;

        return oldSolution;
    }

    static List<int> Invert(List<int> oldSolution)
    {
        Random rand = new Random();
        int randIndexI = (int)(rand.NextDouble() * 10000) % oldSolution.Count;
        int randIndexJ = (int)(rand.NextDouble() * 10000) % oldSolution.Count;
        if (randIndexI == randIndexJ)
            return oldSolution;

        if (randIndexI < randIndexJ)
        {
            while (randIndexI < randIndexJ)
            {
                int temp = oldSolution[randIndexI];
                oldSolution[randIndexI] = oldSolution[randIndexJ];
                oldSolution[randIndexJ] = temp;

                randIndexI++;
                randIndexJ--;
            }
        }
        else
        {
            while (randIndexJ < randIndexI)
            {
                int temp = oldSolution[randIndexJ];
                oldSolution[randIndexJ] = oldSolution[randIndexI];
                oldSolution[randIndexI] = temp;

                randIndexJ++;
                randIndexI--;
            }
        }
        return oldSolution;
    }

    static void InitializePopulationGreedy()
    {
        for (int i = 0; i < population_size; i++)
        {
            List<int> chromosome = GreedyAlgorithm();
            chromosome.RemoveAt(0);//usuwamy zerowy wierzchołek ({fitness= NeighboorSwap(chromosome)
            chromosome = Invert(chromosome); // uzywamy invert, żeby nie dawać takich samych wierzchołków po greedy
            population.Add(new Chromosome { Fitness = CalculateCost(chromosome), ChromosomePath = chromosome });
            //currentCost = Math.Min(population[i].Fitness, currentCost);
        }
    }

    static List<int> GreedyAlgorithm()
    {
        List<int> path = new List<int>();

        while (path.Count < N)
        {
            int lastCity = path.LastOrDefault(0); // zaczynamy od zerowego
            int nextCity = ChooseNextCity(lastCity, path);
            path.Add(nextCity);
        }

        return path;
    }
    static int ChooseNextCity(int lastCity, List<int> path)
    {
        // wybór kolejnego sąsiada - greedy
        int nextCity = -1;
        int minDistance = int.MaxValue;

        for (int city = 0; city < N; city++)
        {
            if (!path.Contains(city) && matrix[lastCity][city] < minDistance)
            {
                minDistance = matrix[lastCity][city];
                nextCity = city;
            }
        }

        return nextCity;
    }
    static List<Chromosome> SelectParentsTournament()
    {
        List<Chromosome> selectedParents = new List<Chromosome>();

        Random random = new Random();

        for (int i = 0; i < population_size/2; i++)
        {
            // losowy wybór rodziców w turnieju
            List<Chromosome> tournamentParticipants = new List<Chromosome>();

            for (int j = 0; j < 2; j++)
            {
                int randomIndex = random.Next(population_size);
                tournamentParticipants.Add(population[randomIndex]);
            }

            // najlepszy z turnieju na podstawie turnieju
            Chromosome bestParticipant = GetBestParticipant(tournamentParticipants);
            selectedParents.Add(bestParticipant);
        }
        return selectedParents;
    }
    static Chromosome GetBestParticipant(List<Chromosome> candidates)
    {
        Chromosome bestParticipant = candidates[0];

        for (int i = 1; i < candidates.Count; i++)
        {

            if (candidates[i].Fitness < bestParticipant.Fitness)
            {
                bestParticipant = candidates[i];
            }
        }

        return bestParticipant;
    }
    static List<Chromosome> SelectParentsRanking()
    {
        List<Chromosome> selectedParents = new List<Chromosome>();
        selectedParents = population.OrderByDescending(x => x.Fitness).ToList();
        selectedParents.RemoveRange(0,population_size/2);
        
        return selectedParents;
    }
    static List<Chromosome> OrderCrossover(List<Chromosome> parentsList)
    {
        List<Chromosome> children = new List<Chromosome>();
        int chromosomeLength = parentsList[0].ChromosomePath.Count;
        for (int i = 0; i<parentsList.Count; i++)
        {
            Random random = new Random();
            //prawdopodobieństwo krzyżowania jeśli nie krzyżujemy to zwracamy rodzica jako dziecko
            if (random.NextDouble() < pK) 
            {
                children.Add(parentsList[i]);
                continue;
            }
            // losowe miejsca cięcia
            int cutPoint1 = random.Next(chromosomeLength);
            int cutPoint2 = random.Next(cutPoint1+1, chromosomeLength);
            if (i == parentsList.Count - 1) //jeśli osttani to weź dwoje osttanich rodziców
            {
                children.Add(CreateChild(parentsList[i], parentsList[i-1], cutPoint1, cutPoint2));
                break;
            }

            // utowrzenie dzieci na podstawie wybranych rodziców oraz cięć
            children.Add(CreateChild(parentsList[i], parentsList[i + 1], cutPoint1, cutPoint2));
        }
        children.InsertRange(0, parentsList);
        return children;
    }
 
    static Chromosome CreateChild(Chromosome parent1, Chromosome parent2, int cutPoint1, int cutPoint2)
    {
        int chromosomeLength = parent1.ChromosomePath.Count;

        // pobieraie segmentu pierwszego rodzica
        List<int> inheritanceSegment = parent1.ChromosomePath
            .GetRange(cutPoint1, cutPoint2 - cutPoint1)
            .Distinct()
            .ToList();
        //Console.WriteLine("Segment: "+ string.Join(" ", inheritanceSegment) + " | "+ cutPoint1 + " | " + cutPoint2);

        // inicjalizacja potomstwa
        Chromosome child = new Chromosome
        {
            Fitness = 0,
            ChromosomePath = inheritanceSegment
        };
        List<int> segmentBefore = new();
        //List<int> segmentAfter = new();

        int midleOfSubstring = (int)Math.Round((float)(cutPoint1+cutPoint2)/2);
        for (int i = 0; i < chromosomeLength; i++)
        {
            if (i<= midleOfSubstring && !child.ChromosomePath.Contains(parent2.ChromosomePath[i]))
            {
                segmentBefore.Add(parent2.ChromosomePath[i]);
            }
            if(i > midleOfSubstring && !child.ChromosomePath.Contains(parent2.ChromosomePath[i]))
            {
                child.ChromosomePath.Add(parent2.ChromosomePath[i]);
            }
        }
        child.ChromosomePath.InsertRange(0, segmentBefore); //sklejenie trasy

        child.Fitness = CalculateCost(child.ChromosomePath);
        //Console.WriteLine("Parent1: "+parent1.Fitness + " " + string.Join(" ", parent1.ChromosomePath) + " | "+ cutPoint1 + " | " + cutPoint2);
        //Console.WriteLine("Parent2: " + parent2.Fitness + " " + string.Join(" ", parent2.ChromosomePath) );
        //Console.WriteLine("Child: " + child.Fitness + " " + string.Join(" ", child.ChromosomePath));
        return child;
    }
    static void Mutation()
    {
        Random rand = new Random();
        foreach(var chromosome in population)
        {
            //prawdopodobieństwo mutacji
            if (rand.NextDouble() < pM) 
            {
                chromosome.ChromosomePath = Invert(chromosome.ChromosomePath);
                chromosome.Fitness = CalculateCost(chromosome.ChromosomePath);
            }
        }
    }
    static double GeneticAlg(int bestPathCost)
    {
        Console.WriteLine(bestPathCost);
        var currTime = Stopwatch.StartNew();
        population.Clear();
        InitializePopulationGreedy();
        double error=9999.999;
        int loopCount = 0;

        //foreach (var chromosome in population)
        //{
        //    Console.Write(chromosome.Fitness + " | populacja: ");

        //    Console.WriteLine(string.Join(" -> ", chromosome.ChromosomePath));
        //    solution = chromosome.ChromosomePath;
        //}

        for (int i = 0; i < generations; i++)
        {
            long cost = currentCost;
            
            List<Chromosome> parents = SelectParentsTournament();
            //List<Chromosome> parents = SelectParentsRanking();
            //Console.WriteLine(parents.Count + " Wielkość rodzica");
            //List<Chromosome> children = OrderCrossover(parents);
            population = OrderCrossover(parents);
            Mutation();
            foreach (var chromosome in population)
            {
                cost = chromosome.Fitness;
                if (currentCost > cost)
                {
                    currentCost = cost;
                    solution = chromosome.ChromosomePath;
                    Console.WriteLine("Nowa najlepsza: " + currentCost + " iteracji " + i);
                    loopCount = 0;
                }
            }
            if (cost == currentCost)
            {
                loopCount++;
                if (loopCount == repAllowed)
                {
                    Console.WriteLine("Brak poprawy rozwiązania");
                    break;
                }
            }

            error = Math.Abs((currentCost - bestPathCost) / bestPathCost)*100;
            if (bestPathCost == currentCost)
            {
                Console.WriteLine(bestPathCost + " Najlepsze rozwiązanie w: " + loopCount + " iteracji" + " | błąd: "+error);
                break;
            }
            var stop = currTime.Elapsed.TotalMilliseconds;
            if (stop > 600000) // 10 minut
            {
                Console.WriteLine("Przekroczono czas dla instancji");
                break;
            }
        }
        return error;
    }
    static void Testing()
    {
        using (StreamWriter outputFile = new StreamWriter(outputFileName))
        {
            double time;

            for (int i = 0; i < fileNameVector.Count; i++)
            {
                outputFile.Write($"{fileNameVector[i]};{testCountVector[i]};{solutionVector[i]};{pathVector[i]}");
                ReadMatrix(fileNameVector[i]);
                outputFile.WriteLine();
                for (int j = 0; j < testCountVector[i]; j++)
                {
                    currentCost = 999999999;
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    double errorValue = GeneticAlg(solutionVector[i]);
                    watch.Stop();
                    time = watch.Elapsed.TotalMilliseconds;
                    string path = string.Join(" ", solution);
                    double timeMikroS = (time / Stopwatch.Frequency) * 10000;
                    outputFile.WriteLine($"{time};{currentCost};[ 0 {path} 0 ];{errorValue}");
                    solution.Clear();
                    population.Clear();
                }
                matrix.Clear();
                solution.Clear();
                outputFile.WriteLine();
                Console.WriteLine("Zakończono instancje pliku: " + fileNameVector[i]);
            }
        }
    }
    static void Main()
    {
        ReadFile("tsp.ini");
        Testing();
    }
}