using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing;
using System.CodeDom.Compiler;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using System.Text.RegularExpressions;

class Program
{
    //czytanie z liku INI
    static List<string> fileNameVector = new List<string>();
    static List<int> testCountVector = new List<int>();
    static List<int> solutionVector = new List<int>();
    static List<string> pathVector = new List<string>();
    static string outputFileName;
    //czytanie grafu

    static List<List<int>> matrix = new List<List<int>>(); //reprezentacja grafu w postaci macierzy
    static int N;//liczba wierzchowłów w grafie
    static List<int> solution = new();
    static int numberOfFirstVertex = 0;

    static int bestCost;
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
        using ( StreamReader file = new StreamReader(FileName))
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
   
    static int CalculateCost(List<int> solve)
    {
        int cost = 0; 
        for(int i = 0; i < solve.Count - 1; i++)
        {
            cost += matrix[solve[i]][solve[i + 1]];
        }
        cost += matrix[numberOfFirstVertex][solve[0]];
        cost += matrix[solve[^1]][numberOfFirstVertex];
        return cost;
    }
    static List<int> NeighborhoodSolutionInvert(List<int> oldSolution)  //invert polega na wycięciu kawałka trasy i zamianie wierzchołków na tej trasie
    {
        Random random = new Random();
        int randIndexI = random.Next(0, oldSolution.Count);
        int randIndexJ = random.Next(0, oldSolution.Count);

        if (randIndexI == randIndexJ)
        {
            return oldSolution; // Brak zamiany, gdy indeksy są równe
        }

        if (randIndexI < randIndexJ)
        {
            while (randIndexI < randIndexJ)
            {
                // Zamiana miejscami elementów
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
                // Zamiana miejscami elementów
                int temp = oldSolution[randIndexJ];
                oldSolution[randIndexJ] = oldSolution[randIndexI];
                oldSolution[randIndexI] = temp;

                randIndexJ++;
                randIndexI--;
            }
        }

        return oldSolution;
    }

    static List<int> NeighboorSwap(List<int> oldSolution)
    {
        Random random = new Random();
        int randIndexI = random.Next(0, oldSolution.Count);
        int randIndexJ = random.Next(0, oldSolution.Count);
        if(randIndexI == randIndexJ)
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
        else{
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

    static double NewTempGeometric(double oldTemp, double alpha)
    {
        return oldTemp * alpha;
    }

    static double NewTempLog(double oldTemp, int n)
    {
        return oldTemp / Math.Log(n);
    }

    static List<int> First()
    {
        List<int> solution = new List<int>();
        Random rand = new Random();

        for (int i = 1; i < N; i++)
            solution.Add(i);

        for (int i = 0; i < N; i++)
        {
            int randIndexI = rand.Next(0, solution.Count);
            int randIndexJ = rand.Next(0, solution.Count);
            int temp = solution[randIndexI];
            solution[randIndexI] = solution[randIndexJ];
            solution[randIndexJ] = temp;
        }

        return solution;
    }

    static double BeginningTemp(int startSolutionCost, double alpha)
    {
        return startSolutionCost * alpha;
    }

    static int CalculateEraLength(int sizeOfInstance, double alpha )
    {
        return (int)(sizeOfInstance * alpha);
    }

    static void SimulatedAnnealing()
    {
        List<int> oldSolution = new();
        List<int> newSolution = new();
        Random random = new Random();

        double currentTemp;
        int eraLength;
        int eraLengthSum = 0;
        int oldSolutionCost;
        int newSolutionCost;
        int delta;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        double randomVal;
        double time = 0;
        oldSolution = First();
        oldSolutionCost = CalculateCost(oldSolution);
        double alpha = 0.9999;
        double bigginningTemp = oldSolutionCost * N;
        currentTemp = bigginningTemp;
        eraLength = 10;
        //Console.WriteLine("Era: " + eraLength);
        while(currentTemp > Math.Pow(10,-10) && time < 30000)
        {
            for(int i =0; i < eraLength; i++)//dokończ testy
            {
                newSolution = NeighboorSwap(oldSolution);
                //newSolution = Invert(oldSolution);
                newSolutionCost = CalculateCost(newSolution);
                if(newSolutionCost<oldSolutionCost)
                {
                    oldSolution = newSolution;
                    oldSolutionCost = newSolutionCost;
                }
                else
                {
                    randomVal = random.NextDouble();
                    if(randomVal < Math.Exp(oldSolutionCost- newSolutionCost / currentTemp))
                    {
                        //Console.WriteLine("Math: " + Math.Exp(oldSolutionCost - newSolutionCost / currentTemp) + " | Randowm Val: " +randomVal);
                        oldSolution = newSolution;
                        oldSolutionCost = newSolutionCost;
                    }
                }
            }
            eraLengthSum += eraLength;
            currentTemp = NewTempGeometric(currentTemp, alpha);
            //currentTemp = NewTempLog(currentTemp, eraLengthSum);

            watch.Stop();
            time = watch.ElapsedMilliseconds;
        }

        solution.Add(numberOfFirstVertex);
        solution.AddRange(oldSolution);
        solution.Add(numberOfFirstVertex);
        bestCost = oldSolutionCost;
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
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    SimulatedAnnealing();
                    watch.Stop();
                    time = watch.ElapsedMilliseconds;
                    string path = string.Join(" ", solution);
                    //Console.WriteLine(bestCost);
                    //double timeMikroS = (time / Stopwatch.Frequency) * 1000000;
                    outputFile.WriteLine($"{time};{bestCost};[ 0 {path} 0 ]");
                    solution.Clear();
                    Console.WriteLine("Zakończono instancje: " + j);

                }
                matrix.Clear();
                solution.Clear();

                //Array.Clear(solution, 0, solution.Count);
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