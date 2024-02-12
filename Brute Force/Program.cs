using System.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

class Program
{
    static List<string> fileNameVector = new List<string>();
    static List<int> testCountVector = new List<int>();
    static List<int> solutionVector = new List<int>();
    static List<string> pathVector = new List<string>();
    static string outputFileName;
    static List<List<int>> matrix = new List<List<int>>(); //reprezentacja grafu w postaci macierzy
    static int N;//liczba wierzchowłów w grafie
    static int minDistance;
    static List<int> solution = new List<int>();

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
        using(StreamReader file = new StreamReader(FileName))
        {
            while((line = file.ReadLine()) != null)
            {
                string[] oneRow = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (oneRow.Length == 1)
                {
                    N = int.Parse(oneRow[0]);
                    continue;
                }
                if (oneRow.Length>=N)
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

    static void BruteForce(int currentN, int distanceSum, List<int> visitedN)
    {
        //Console.Write(visitedN.Count);
        if (visitedN.Count == N)
        {
            visitedN.Add(0); // Dodaj wierzchołek początkowy, aby zamknąć cykl
            distanceSum += matrix[currentN][0];

            if (minDistance > distanceSum)
            {
                minDistance = distanceSum;
                solution = new List<int>(visitedN); // Kopia rozwiązania
            }

            visitedN.RemoveAt(visitedN.Count - 1);
            return;
        }

        for (int i = 0; i < N; i++)
        {
            bool repeat = false;

            if (i == currentN)
            {
                continue;
            }
            if (minDistance < distanceSum + matrix[currentN][i])
            {
                continue;
            }

            for (int j = 0; j < visitedN.Count; j++)
            {
                if (visitedN[j] == i)
                {
                    repeat = true;
                    break;
                }
            }

            if (repeat)
            {
                continue;
            }

            visitedN.Add(i);
            BruteForce(i, distanceSum + matrix[currentN][i], visitedN);
            visitedN.RemoveAt(visitedN.Count - 1);

        }
    }

    static void Testing()
    {
        using (StreamWriter outputFile = new StreamWriter(outputFileName))
        {
            int firstN = 0;
            double time;
            string path;
            minDistance = 9999999;

            List<int> visitedN = new List<int>();
            visitedN.Add(firstN);

            for (int i = 0; i<fileNameVector.Count; i++) {
                outputFile.Write($"{fileNameVector[i]};{testCountVector[i]};{solutionVector[i]};{pathVector[i]}");
                ReadMatrix(fileNameVector[i]);
                outputFile.WriteLine(); 
                for(int j = 0; j < testCountVector[i]; j++)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    BruteForce(firstN, 0, visitedN);
                    watch.Stop();
                    time = watch.ElapsedTicks;
                    path = string.Join(" ", solution);
                    double timeMikroS = (time / Stopwatch.Frequency) * 1000000;
                    outputFile.WriteLine($"{timeMikroS};{minDistance};[{path}]");
                    minDistance = 999999;
                    visitedN.Clear();
                    visitedN.Add(firstN);   
                }

                minDistance = 9999999;
                visitedN.Clear();
                visitedN.Add(firstN);
                matrix.Clear();
                outputFile.WriteLine();
            }
        }
    }
    static void Main()
    {
        ReadFile("tsp.ini");
        Testing();
        //Console.Write("\n"+N);
    }
}