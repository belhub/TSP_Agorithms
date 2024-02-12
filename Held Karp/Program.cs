using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing;

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
    static int[,] dynamicTable; //tablica dynamicznego programowania
    static int[,] nodeTable; //tablica do przechowywania rodzica każdego wierzchołka
    static List<int> solution = new List<int>(); //przechowywanie optymalnej ścieżki


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

    static int DynamicTableInit() //przygotowanie tablicy do przechowywania wyników pośrednich algorytów
    {
        int power2n = (int)Math.Pow(2, N);

        dynamicTable = new int[power2n, N];
        nodeTable = new int[power2n, N];

        for (int i = 0; i < power2n; i++)
        {
            for (int j = 0; j < N; j++)
            {
                nodeTable[i, j] = -1;
                dynamicTable[i, j] = -1;
            }
        }
        return HeldKarp(1, 0);
    }

    static int HeldKarp(int visitedCities, int currentCity)
    {
        if (visitedCities == (1 << N) - 1) //jeśli zawiera wszystkie miasta
        {
            return matrix[currentCity][0]; //wróć do miasta 0, koniec rekurencji.
        }

        if (dynamicTable[visitedCities, currentCity] != -1)//jeśłi wynik został obliczony i jest przechowywany w dynamic table
        {
            return dynamicTable[visitedCities, currentCity];
        }

        int minDistance = 999999;

        for (int v = 0; v < N; v++) //obliczanie minimalnego kosztu podróży z obecnego miasta do pozostałych używając rekurencji
        {
            if ((visitedCities & (1 << v)) == 0 && v != currentCity)
            {
                int newcity = visitedCities | (1 << v); //dodanie do odwiedzonyhc miast nowego miasta - v
                int distance = matrix[currentCity][v] + HeldKarp(newcity, v);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    nodeTable[visitedCities, currentCity] = v;

                }
                //minDistance = Math.Min(minDistance, distance);
            }
        }
        
        dynamicTable[visitedCities, currentCity] = minDistance;
        return minDistance;
    }
    static void ReconstructPath(int n) //odtowrzenie optymalnej ścieżki
    {
        int visitedCities = 1; //odwiedzone miasta
        int city = 0;
        int[] path = new int[n]; //trasa
        for (int i = 0; i < n; i++)
        {
            path[i] = -1;
        }

        for (int i = 0; i < n; i++)
        {
            if (visitedCities == (1 << n) - 1)
            {
                break;  //zakoncz jak wszystkie wierzchołki zostały odwiedzone
            }

            //Console.WriteLine(mask + " / " + pos);
            int nextNode = nodeTable[visitedCities, city];
            path[i] = nextNode;
            visitedCities ^= (1 << nextNode); //usuwanie bitów aby zaznaczyć że miasto zostało odwiedzone
            city = nextNode;
        }

        //for (int i = 0; i < (1 << n); i++)
        //{
        //    for (int j = 0; j < n; j++)
        //    {
        //        Console.Write(nodeTable[i, j] + " ");
        //    }
        //    Console.WriteLine();
        //}

        foreach (int node in path)
        {
            if (node == -1)
            {
                break;  //koniec jeśli równe -1
            }
            //Console.Write("n:" + node);
            solution.Add(node);

        }
    }
    static void Testing()
    {
        using (StreamWriter outputFile = new StreamWriter(outputFileName))
        {
            double time;
            int minDistance = 9999999;

            for (int i = 0; i < fileNameVector.Count; i++)
            {
                outputFile.Write($"{fileNameVector[i]};{testCountVector[i]};{solutionVector[i]};{pathVector[i]}");
                ReadMatrix(fileNameVector[i]);
                outputFile.WriteLine();
                for (int j = 0; j < testCountVector[i]; j++)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    minDistance = DynamicTableInit();
                    ReconstructPath(N);
                    watch.Stop();
                    time = watch.ElapsedTicks;
                    string path = string.Join(" ", solution);
                    double timeMikroS = (time / Stopwatch.Frequency) * 1000000;
                    outputFile.WriteLine($"{timeMikroS};{minDistance};[ 0 {path} 0 ]");
                    minDistance = 999999;
                    solution.Clear();

                }
                minDistance = 9999999;
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