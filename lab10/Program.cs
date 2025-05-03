using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        int simulationTime = 1000; // Время симуляции в миллисекундах
        int maxIterations = 100; // Количество проходов
        double serviceRate = 0.95; // Интенсивность обслуживания
        double arrivalRate = 1.0; // Интенсивность прихода

        Queue<int> regularQueue = new Queue<int>();
        Queue<int> poissonQueue = new Queue<int>();
        Queue<int> erlangQueue = new Queue<int>();

        List<int> regularQueueLengths = new List<int>();
        List<int> poissonQueueLengths = new List<int>();
        List<int> erlangQueueLengths = new List<int>();

        Random random = new Random();

        for (int i = 0; i < maxIterations; i++)
        {
            // Регулярный поток
            if (random.NextDouble() < arrivalRate)
            {
                regularQueue.Enqueue(i);
            }
            regularQueueLengths.Add(regularQueue.Count);

            // Пуассоновский поток
            if (random.NextDouble() < arrivalRate)
            {
                poissonQueue.Enqueue(i);
            }
            poissonQueueLengths.Add(poissonQueue.Count);

            // Поток Эрланга (порядка 2)
            if (random.NextDouble() < arrivalRate)
            {
                erlangQueue.Enqueue(i);
                erlangQueue.Enqueue(i); // Два прихода для Эрланга
            }
            erlangQueueLengths.Add(erlangQueue.Count);

            // Обработка требований
            if (regularQueue.Count > 0 && random.NextDouble() < serviceRate)
            {
                regularQueue.Dequeue();
            }

            if (poissonQueue.Count > 0 && random.NextDouble() < serviceRate)
            {
                poissonQueue.Dequeue();
            }

            if (erlangQueue.Count > 0 && random.NextDouble() < serviceRate)
            {
                erlangQueue.Dequeue();
            }

            Thread.Sleep(10); // Задержка для имитации времени
        }

        // Запись результатов в файл для Python
        System.IO.File.WriteAllLines("queue_lengths.txt", new[]
        {
            string.Join(",", regularQueueLengths),
            string.Join(",", poissonQueueLengths),
            string.Join(",", erlangQueueLengths)
        });

        // Запуск Python скрипта для визуализации
        RunPythonScript("plot.py");
    }

    static void RunPythonScript(string scriptName)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "python", // или "python3", в зависимости от вашей системы
                Arguments = scriptName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Console.WriteLine("Python stdout:\n" + output);
        if (!string.IsNullOrWhiteSpace(error))
            Console.WriteLine("Python stderr:\n" + error);
    }
}
