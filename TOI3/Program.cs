using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TOI3
{
    class Program
    {
        static List<string> document = new List<string>();
        static double[,] frequencyMatrix;
        static int m, n, clustersCount, rowsCount;
        static double[] w;
        static double[,] u, vt, VT;
        static ArrayList[] clusters;

        static void Main(string[] args)
        {
           
            document.Add("Британская полиция знает о местонахождении основателя WikiLeaks");
            document.Add("В суде США начинается процесс против россиянина, рассылавшего спам");
            document.Add("Церемонию вручения Нобелевской премии мира бойкотируют 19 стран");
            document.Add("В Великобритании арестован основатель сайта Wikileaks Джулиан Ассандж");
            document.Add("Украина игнорирует церемонию вручения Нобелевской премии");
            document.Add("Шведский суд отказался рассматривать апелляцию основателя Wikileaks");
            document.Add("НАТО и США разработали планы обороны стран Балтии против России");
            document.Add("Полиция Великобритании нашла основателя WikiLeaks, но, не арестовала");
            document.Add("В Стокгольме и Осло сегодня состоится вручение Нобелевских премий");

            string words = "";

            foreach (string str in document)
            {
                words += str + " ";
            }
            //Удаляем  стоп слова
            words =words.ToLower().Replace(",", string.Empty);
            DelStopWords(ref words);
            
            List<string> WordList = words.Split(' ').ToList<string>();

            
            //Стемминг
            for (int i = 0; i < WordList.Count; i++)
            {
                WordList[i] = Stemming.TransformingWord(WordList[i]);
            }

            //Удаляем слова повторяющиеся менее 1 раза
            DelOneWords(ref WordList);
            ListCreate(ref WordList);
           


            double[,] matrix = new double[WordList.Count, document.Count];

            for (int i = 0; i < WordList.Count; i++)
            {
                for (int j = 0; j < document.Count; j++)
                {
                    matrix[i, j] = CountWords(WordList[i], document[j]);
                }
            }
            frequencyMatrix = matrix;
            
            for (int i = 0; i < WordList.Count; i++)
            {
                Console.Write(WordList[i] + " ");
                for (int j = 0; j < document.Count; j++)
                {
                    Console.Write(matrix[i, j] + " ");
                }
                Console.WriteLine();
            }

            clustersCount = 3;
            rowsCount = 3;
            m = WordList.Count;
            n = document.Count;
            //сингулярное разложение
            SVD();
            //кластеризация
            Clustering();
            //вывод
            Output();
            Console.WriteLine("Результат:");
            Console.ReadKey();

        }
      
        static void DelStopWords(ref string words)
        {
            string StopWordsStr = File.ReadAllText("StopWords.txt");

            List<string> StopWords = StopWordsStr.Split(' ').ToList<string>();
            List<string> AllWords = words.ToLower().Split(' ').ToList<string>();



            for (int i = 0; i < AllWords.Count; i++)
            {
               for (int j = 0; j < StopWords.Count; j++)
                {
                    if (AllWords[i] == StopWords[j])
                        AllWords.Remove(AllWords[i]);
                }
            }
            words = string.Join(" ", AllWords);

        }
        static void DelOneWords(ref List<string> words)
        {

            List<string> temp = new List<string>();

            foreach (string a in words)
            {
                int count = 0;
                foreach (string b in words)
                {
                    if (a == b)
                    {
                        count++;
                        if (count > 1)
                        {
                            temp.Add(a);
                            break;
                        }
                    }
                }
            }
            words = temp;
        }
        //определение кол-ва вхождений
        public static int CountWords(string s, string s0)
        {

            List<string> words = s0.ToLower().Split(' ').ToList<string>();

            int count = 0;
            foreach(var a in words)
            {
                 
                if (a.StartsWith(s))
                {
                   
                    count++;
                }
            }
            return count;
        }
        //Составление списка слов по одному
        public static void ListCreate(ref List<string> words)
        {
            List<string> temp = new List<string>();

            foreach(var a in words)
            {
                if (!temp.Contains(a))
                {
                    temp.Add(a);
                }
            }
            words = temp;
        }
        //
        static void SVD()
        {
            alglib.rmatrixsvd(frequencyMatrix, m, n, 2, 2, 2, out w, out u, out vt);
            VT = new double[rowsCount, n];
            /*где rowsCount задается при старте программы, а n это количество столбцов*/
            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    VT[i, j] = vt[i, j];//2demension matrix, rowsCount eneter when app start
                }
            }
        }
        static void InitializeCentroid(double[,] centroid)
        {
            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < clustersCount; j++)
                {
                    centroid[i, j] = VT[i, j];
                }
            }
        }

        /*First step*/
        static void FindClusters(double[,] centroid1)
        {
            for (int i = 0; i < clustersCount; i++)
                clusters[i].Clear();
            for (int i = 0; i < n; i++)
            {
                double[] distances = new double[clustersCount];
                for (int j = 0; j < clustersCount; j++)
                {
                    distances[j] = 0;
                    for (int k = 0; k < rowsCount; k++)
                    {
                        distances[j] += Math.Pow(centroid1[k, j] - VT[k, i], 2);
                    }
                    distances[j] = Math.Sqrt(distances[j]);

                }
                double min = distances.Min();
                int minIndex = Array.IndexOf(distances, min);
                clusters[minIndex].Add(i);
            }
        }

        static void CopyCentroids(double[,] centroid1, double[,] centroid2)
        {
            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < clustersCount; j++)
                {
                    centroid2[i, j] = centroid1[i, j];
                }
            }
        }


        static void CreateNewCentroid(double[,] centroid1)
        {
            for (int i = 0; i < clustersCount; i++)
            {
                foreach (int c in clusters[i])
                {
                    for (var j = 0; j < rowsCount; j++)
                    {
                        centroid1[j, i] += VT[j, c];
                    }
                }
                for (var j = 0; j < rowsCount; j++)
                {
                    if (clusters[i].Count != 0)
                    {
                        centroid1[j, i] /= clusters[i].Count;
                    }
                }
            }
        }
 
        static double MaxChanging(double[,] centroid1, double[,] centroid2)
        {
            double[] changing = new double[clustersCount];
            for (int i = 0; i < clustersCount; i++)
            {
                for (int j = 0; j < rowsCount; j++)
                {
                    changing[i] += Math.Pow(centroid1[j, i] - centroid2[j, i], 2);
                }

                changing[i] = Math.Sqrt(changing[i]);
            }
            return changing.Max();
        }

        static void Clustering()
        {
            double[,] centroid1 = new double[rowsCount, clustersCount];
            double[,] centroid2 = new double[rowsCount, clustersCount];
            InitializeCentroid(centroid1);
            clusters = new ArrayList[clustersCount];
            for (int i = 0; i < clustersCount; i++)
            {
                clusters[i] = new ArrayList();
            }
            FindClusters(centroid1);
            CopyCentroids(centroid1, centroid2);
            CreateNewCentroid(centroid1);
            while (MaxChanging(centroid1, centroid2) > 0.00001)
            {
                FindClusters(centroid1);
                CopyCentroids(centroid1, centroid2);
                CreateNewCentroid(centroid1);
            }
        }
        static void Output()
        {
            using (StreamWriter result = new StreamWriter("C:/Users/admin/Desktop/TOIres.txt"))
            {
                for (int i = 0; i < clustersCount; i++)
                {
                    foreach (int doc in clusters[i])
                    {
                        result.WriteLine(document[doc]);//запись заголовка под номером
                    }
                    result.WriteLine();
                }
            }
        }

    }
    

}
