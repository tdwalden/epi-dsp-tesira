using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;

namespace Tesira_DSP_EPI.Helpers {
    public class BiampResponseProcessor : IDisposable {
        TesiraDsp Parent { get; set; }
        CEvent _wh = new CEvent();
        Thread _worker;
        readonly object _locker = new object();
        public CrestronQueue<TesiraDsp.QueuedCommand> _tasks = new CrestronQueue<TesiraDsp.QueuedCommand>();
        private TesiraDsp.QueuedCommand lastTask { get; set; }
​
        public BiampResponseProcessor(TesiraDsp DspDevice)
        {
            Parent = DspDevice;
            _worker = new Thread(ProcessResponses, null, Thread.eThreadStartOptions.Running);
            //_worker.Start();
        }

        public void EnqueueTask(TesiraDsp.QueuedCommand task)
        {
            _tasks.Enqueue (task);
            _wh.Set();
        }
​
        public void Dispose()
        {
            EnqueueTask (null);     // Signal the consumer to exit.
            _worker.Join();         // Wait for the consumer's thread to finish.
            _wh.Close();            // Release any OS resources.
        }

        object ProcessResponses(object obj)
        {
            while (true)
            {
                TesiraDsp.QueuedCommand task = null;
​               
                if (_tasks.Count > 0)
                {
                    task = _tasks.Peek();
                    if (task == null) return task;
                }
                if (task != null)
                {
                    if (task.AttributeCode != lastTask.AttributeCode || lastTask == null) {
                        lastTask = task;
                        Console.WriteLine("Performing task: " + task);
                        Parent.SendLine(task.Command);
                    }
                    else
                        _wh.Wait();
                    
                }
                else
                    _wh.Wait();        // No more tasks - wait for a signal
            }
        }
    }
}