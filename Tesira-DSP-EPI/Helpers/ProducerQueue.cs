﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using PepperDash.Core;

namespace Tesira_DSP_EPI.Helpers {
    public class BiampResponseProcessor : IDisposable, IKeyed {
        TesiraDsp Parent { get; set; }
        CEvent _wh = new CEvent();
        Thread _worker;
        readonly object _locker = new object();
        public CrestronQueue<TesiraDsp.QueuedCommand> _tasks = new CrestronQueue<TesiraDsp.QueuedCommand>();
        public string Key { get; set; }
​
        public BiampResponseProcessor(TesiraDsp DspDevice)
        {
            Parent = DspDevice;
            Key = String.Format("{0}--CommandQueue", Parent.Key);
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

        object  ProcessResponses(object obj) {
            while (!Parent.ProcessCommand) {
                TesiraDsp.QueuedCommand task = null;

                if (_tasks.Count > 0) {
                    task = _tasks.Peek();
                    if (task == null) return task;
                }
                if (task != null) {
                    Parent.ProcessCommand = true;
                    Debug.Console(2, this, "Enqueued Command " + task);
                    Parent.SendLine(task.Command);

                }
                else
                    _wh.Wait();        // No more tasks - wait for a signal
            }
            return null;
        }
    }
}