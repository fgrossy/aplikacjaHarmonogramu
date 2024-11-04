/*Wielowątkowy harmonogram zadań
Celem zadania jest napisanie aplikacji która pozwala na zdefiniowanie zadań do wykonania o wskazanym czasie.
Użytkownik uruchamia aplikację. Ma możliwość - w dowolnym momencie:
1.Dodania nowego zadania TASK (dla uproszczenia: ścieżka do skryptu ps1) do wykonania o DATE.
2. Wyświetlenia aktualnego statusu (listy zadań z datą i godziną gdy mają się zacząć + WAITING / RUNNING / DONE jako status).
3. Usunięcia zadania (anulowania przyszłego, przerwania zaczętego bądź usunięcia z listy skończonego).
Podczas pracy, gdy nadejdzie DATE jakiegokolwiek zadania TASK program rozpoczyna jego wykonanie. 
w aplikacji konsolwej c#
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TaskSchedulerApp
{
    public enum TaskStatus
    {
        WAITING,
        RUNNING,
        DONE
    }

    public class ScheduledTask
    {
        public string ScriptPath { get; set; }
        public DateTime StartTime { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.WAITING;
        public Timer Timer { get; set; }

        public ScheduledTask(string scriptPath, DateTime startTime)
        {
            ScriptPath = scriptPath;
            StartTime = startTime;
        }

        public void Run()
        {
            Status = TaskStatus.RUNNING;
            Console.WriteLine($"Running task: {ScriptPath}");

            
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = $"-File \"{ScriptPath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            process.WaitForExit(); 
            Status = TaskStatus.DONE;
            Console.WriteLine($"Task {ScriptPath} finished.");
        }
    }

    public class TaskScheduler
    {
        private List<ScheduledTask> tasks = new List<ScheduledTask>();
        public void AddTask(string scriptPath, DateTime startTime)
        {
            var delay = startTime - DateTime.Now;

            if (delay <= TimeSpan.Zero)
            {
                Console.WriteLine("Error: The specified time is in the past. Please enter a future date and time.");
                return;
            }

            var task = new ScheduledTask(scriptPath, startTime);
            task.Timer = new Timer(ExecuteTask, task, delay, Timeout.InfiniteTimeSpan);
            tasks.Add(task);
            Console.WriteLine($"Task added: {scriptPath} at {startTime}");
        }


        private void ExecuteTask(object state)
        {
            if (state is ScheduledTask task)
            {
                task.Run();
            }
        }

        public void ShowTasks()
        {
            Console.WriteLine("\nCurrent tasks:");
            foreach (var task in tasks)
            {
                Console.WriteLine($"{task.ScriptPath} | {task.StartTime} | Status: {task.Status}");
            }
        }

        public void RemoveTask(string scriptPath)
        {
            var task = tasks.Find(t => t.ScriptPath == scriptPath);
            if (task != null)
            {
                task.Timer?.Dispose();

                if (task.Status == TaskStatus.RUNNING)
                {
                    Console.WriteLine("Cannot remove a running task.");
                }
                else
                {
                    tasks.Remove(task);
                    Console.WriteLine($"Task {scriptPath} removed.");
                }
            }
            else
            {
                Console.WriteLine("Task not found.");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var scheduler = new TaskScheduler();

            while (true)
            {
                Console.WriteLine("\nOptions:");
                Console.WriteLine("1. Add new task");
                Console.WriteLine("2. Show tasks");
                Console.WriteLine("3. Remove task");
                Console.WriteLine("0. Exit");
                Console.Write("Choose an option: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter script path: ");
                        var path = Console.ReadLine();
                        Console.Write("Enter date and time (yyyy-MM-dd HH:mm): ");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime date))
                        {
                            scheduler.AddTask(path, date);
                        }
                        else
                        {
                            Console.WriteLine("Invalid date format.");
                        }
                        break;

                    case "2":
                        scheduler.ShowTasks();
                        break;

                    case "3":
                        Console.Write("Enter script path to remove: ");
                        var removePath = Console.ReadLine();
                        scheduler.RemoveTask(removePath);
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }
}
