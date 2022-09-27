using System;
using System.Threading;
using System.Threading.Tasks;

namespace MTTest2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Начало работы программы");
            EventHandler h = new EventHandler(myEventHandler); // адрес метода
            
            // Синхронно
            h.Invoke(null, EventArgs.Empty); // вызов делегата

            // Асинхронно
            var res = h.BeginInvoke(null, EventArgs.Empty, null, null);

            // Полусинхронно
            AsyncCaller ac = new AsyncCaller(h);
            bool completedOK = ac.Invoke(5000, null, EventArgs.Empty);
            
            Console.WriteLine("Конец программы");
        }

        //async static Task<int> Test(int n) { }

        static void myEventHandler(object sender, EventArgs e)
        {
            Console.WriteLine("Делегат выполняет метод");
        }
    }

    /// <summary>
    /// "Полусинхронного" в данном случае означает, что делегат будет вызван,
    /// и вызывающий поток будет ждать, пока вызов не выполнится.
    /// Но если выполнение делегата займет больше 5000 миллисекунд, то
    /// ac.Invoke выйдет и вернет в completedOK значение false.
    /// </summary>
    class AsyncCaller
    {
        static EventHandler Del;
        static bool IsCompleted;

        public AsyncCaller(EventHandler EventHand)
        {
            Del = EventHand;
        }

        /// <summary>
        /// Внешняя функция для вызова делегата с ограничением на время выполнения
        /// </summary>
        /// <param name="Timer">ограничение по времени на выполнение</param>
        /// <param name="obj"></param>
        /// <param name="EventArg"></param>
        /// <returns>Успел ли выполниться делегат</returns>
        internal bool Invoke(int Timer, object? obj, System.EventArgs EventArg)
        {
            return HalfAsync(Timer, obj, EventArg).Result;
        }

        /// <summary>
        /// Запуск двух параллельных потоков. Кто раньше - такой и bool
        /// </summary>
        /// <param name="Timer">ограничение по времени на выполнение</param>
        /// <param name="obj"></param>
        /// <param name="EventArg"></param>
        /// <returns>False - завершился раньше таймер, True - завершился раньше делегат</returns>
        async static Task<bool> HalfAsync(int Timer, object? obj, System.EventArgs EventArg)
        {
            var TimerTask = AsyncCaller.Timer(Timer);
            var Event = AsyncCaller.Event(obj, EventArg);

            IsCompleted = await Task.WhenAny(TimerTask, Event).Result; // кто быстрее - тот и прав
            Console.WriteLine($"Первым завершилось {IsCompleted}");
            return IsCompleted;
        }

        /// <summary>
        /// Таймер, ограничивающий время выполнения делегата
        /// </summary>
        /// <param name="Time"></param>
        /// <returns></returns>
        async static Task<bool> Timer(int Time)
        {
            Console.WriteLine("Запуск таймера");
            await Task.Delay(Time);
            Console.WriteLine("Конец таймера");
            return false;
        }

        /// <summary>
        /// Делегат
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="EventArg"></param>
        /// <returns></returns>
        async static Task<bool> Event(object? obj, System.EventArgs EventArg)
        {
            Console.WriteLine("Запуск делегата");
            await Task.Delay(4000);
            Del.BeginInvoke(obj, EventArg, null, null);
            return true;
        }
    }
}
