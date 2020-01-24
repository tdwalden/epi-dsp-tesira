using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;

namespace Tesira_DSP_EPI {
    public class ProducerQueue {
        public ThreadCallbackFunction Work { get; set; }
        EventWaitHandle _wh = new AutoResetEvent(false);
        Thread _worker;
        readonly object _locker = new object();
        Queue<string> _tasks = new Queue<string>();

        public ProducerQueue() {
            Work = new 
            Thread SimpleWork = new Thread(new ThreadCallbackFunction(Work()), null);
            _worker = new Thread(Work, null);
            _worker.Start();
        }

        public void EnqueueTask(string task) {
            lock (_locker) _tasks.Enqueue(task);
            _wh.Set();
        }



        public void Dispose() {
            EnqueueTask(null);     // Signal the consumer to exit.
            _worker.Join();         // Wait for the consumer's thread to finish.
            _wh.Close();            // Release any OS resources.
        }

        void WorkFunc() {
            while (true) {
                string task = null;
                lock (_locker)
                    if (_tasks.Count > 0) {
                        task = _tasks.Dequeue();
                        if (task == null) return;
                    }
                if (task != null) {
                    Console.WriteLine("Performing task: " + task);
                    Thread.Sleep(1000);  // simulate work...
                }
                else
                    _wh.WaitOne();         // No more tasks - wait for a signal
            }
        }
    }
}