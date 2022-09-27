using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MTTest1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Команда Get возвращает значение Count");
            Console.WriteLine("Rоманда Add N добавляет число N к значению Count");

            int n = 0;

            while (true)
            {
                string[] input = Console.ReadLine().Split(" ");
                if (input[0].ToLower() == "add")
                {
                    if (input.Length == 2) Server.AddToCount(Convert.ToInt32(input[1]));
                    else Console.WriteLine("Неверная команда");
                }
                else if (input[0].ToLower() == "get")
                {
                    Console.WriteLine(Server.GetCount());
                    n++;
                }
                else Console.WriteLine("Неверная команда");
            }
        }
    }


    /// <summary>
    /// "Сервер" чистый
    /// </summary>
    static class Server
    {
        static int Count = 0;
        static ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Чтение переменной Count
        /// </summary>
        /// <returns></returns>
        public static int GetCount()
        {
            Task<int> Read = new Task<int>(() =>
            {
                Lock.EnterReadLock();
                try
                {
                    return Count;
                }
                finally
                {
                    Lock.ExitReadLock();
                }
            });
            Read.Start();
            return Count;
        }

        /// <summary>
        /// Добавление значения к Count
        /// </summary>
        /// <param name="C"></param>
        public static void AddToCount(int C)
        {
            Thread AddCall = new Thread(obj =>
            {
                Lock.EnterWriteLock();
                try
                {
                    if (obj is int C)
                    {
                        Count += C;
                    }
                }
                finally
                {
                    Lock.ExitWriteLock();
                }

            });
            AddCall.Start(C);
        }
    }

    #region Альтернативный вариант
    /*
    /// <summary>
    /// "Сервер"
    /// </summary>
    static class Server
    {
        static int Count = 0;
        static Semaphore Writer = new Semaphore(1, 1);
        
        static bool IsWriting = false;

        /// <summary>
        /// Серверная функция для чтения переменной Count
        /// </summary>
        /// <param name="n">переменная для хранения номера вызова (для отладки)</param>
        /// <returns></returns>
        public static int GetCount(int n)
        {
            Task<int> Read = new Task<int>(() =>
            {
                while (IsWriting)
                {
                    Thread.Sleep(100);
                }
                Console.WriteLine($"Поток {n}");
                for (int i = 0; i < 10; i++)
                {
                    //Console.WriteLine($"Читаааем");
                    Thread.Sleep(500);
                }
                Console.WriteLine($"вывод: {Count} - Поток {n}");

                return Count;
            });
            Read.Start();
            return Count;
        }

        /// <summary>
        /// Серверная функция добавления значения к Count
        /// </summary>
        /// <param name="C"></param>
        public static void AddToCount(int C)
        {
            Thread AddCall = new Thread(obj =>
            {
                if (obj is int C)
                {
                    IsWriting = true;
                    Writer.WaitOne();

                    Console.WriteLine($"Поток прибавления");
                    Thread.Sleep(1000);

                    for (int i = 0; i < 10; i++)
                    {
                        Console.WriteLine($"Прибавляяяяем");
                        Thread.Sleep(500);
                    }

                    Count += C;

                    Console.WriteLine($"Прибавление успено завершено");
                    Thread.Sleep(1000);

                    Console.WriteLine($"Now Count = {Count}");

                    IsWriting = false;
                    Writer.Release();
                }
            });

            AddCall.Start(C);
            
        }
    }*/

    #endregion
}
