using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static Random rand = new Random();
    const double arrivalRate = 1.0;
    const double serviceRateSystem1 = 0.9;
    const double serviceRateSystem2 = 0.3;
    const int deviceCountSystem2 = 3;
    const int simulationSteps = 10000;

    static void Main()
    {
        var queueLengths1 = new List<int>();
        var queueLengths2 = new List<int>();

        var smo1 = new SingleServerSystem(serviceRateSystem1);
        var smo2 = new MultiServerSystem(deviceCountSystem2, serviceRateSystem2);

        for (int step = 0; step < simulationSteps; step++)
        {
            bool newRequest = GenerateEvent(arrivalRate);

            if (newRequest)
            {
                smo1.AddRequest();
                smo2.AddRequest();
            }

            smo1.ProcessStep();
            smo2.ProcessStep();

            queueLengths1.Add(smo1.QueueLength);
            queueLengths2.Add(smo2.QueueLength);
        }
        if (!Directory.Exists("src"))
        {
            Directory.CreateDirectory("src");
        }
        // Сохранение данных в CSV
        File.WriteAllLines("src/queues.csv", new[] { "Step,Queue1,Queue2" }.Concat(
            Enumerable.Range(0, queueLengths1.Count)
                      .Select(i => $"{i},{queueLengths1[i]},{queueLengths2[i]}")));
        Console.WriteLine("CSV-файл сохранён: queues.csv");

        // Вызов Python-скрипта для построения графика
        RunPythonScript("plot_queues.py");
    }

    static bool GenerateEvent(double rate)
    {
        double time = -Math.Log(1.0 - rand.NextDouble()) / rate;
        return time < 1.0; // Имитация дискретного шага
    }

    class SingleServerSystem
    {
        private double serviceRate;
        private Queue<double> queue = new Queue<double>();
        private double? currentServiceTime = null;

        public int QueueLength => queue.Count + (currentServiceTime.HasValue ? 1 : 0);

        public SingleServerSystem(double serviceRate)
        {
            this.serviceRate = serviceRate;
        }

        public void AddRequest()
        {
            if (!currentServiceTime.HasValue)
                currentServiceTime = GenerateServiceTime();
            else
                queue.Enqueue(GenerateServiceTime());
        }

        public void ProcessStep()
        {
            if (currentServiceTime.HasValue)
            {
                currentServiceTime -= 1.0;
                if (currentServiceTime <= 0)
                {
                    currentServiceTime = queue.Count > 0 ? queue.Dequeue() : (double?)null;
                }
            }
        }

        private double GenerateServiceTime()
        {
            return -Math.Log(1.0 - rand.NextDouble()) / serviceRate;
        }
    }

    class MultiServerSystem
    {
        private double serviceRate;
        private Queue<double> waitingQueue = new Queue<double>();
        private double?[] servers;

        public int QueueLength => waitingQueue.Count + Array.FindAll(servers, s => s.HasValue).Length;

        public MultiServerSystem(int serverCount, double serviceRate)
        {
            this.serviceRate = serviceRate;
            servers = new double?[serverCount];
        }

        public void AddRequest()
        {
            for (int i = 0; i < servers.Length; i++)
            {
                if (!servers[i].HasValue)
                {
                    servers[i] = GenerateServiceTime();
                    return;
                }
            }

            waitingQueue.Enqueue(GenerateServiceTime());
        }

        public void ProcessStep()
        {
            for (int i = 0; i < servers.Length; i++)
            {
                if (servers[i].HasValue)
                {
                    servers[i] -= 1.0;
                    if (servers[i] <= 0)
                        servers[i] = waitingQueue.Count > 0 ? waitingQueue.Dequeue() : (double?)null;
                }
            }
        }

        private double GenerateServiceTime()
        {
            return -Math.Log(1.0 - rand.NextDouble()) / serviceRate;
        }
    }

    // Метод для запуска Python-скрипта
    static void RunPythonScript(string scriptName)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "python", // или "python", в зависимости от твоей системы
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
