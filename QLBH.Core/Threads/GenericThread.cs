using System;
using System.Collections.Generic;
using System.Threading;

namespace QLBH.Core.Threads
{
    public delegate void GenericThreadStart();
    public delegate void PrameterizedGenericThreadStart<T>(T value);
    
    public class GenericThread<T>
    {
        private GenericThreadStart threadStart;
        private PrameterizedGenericThreadStart<T> threadParameterizedStart;
        private Thread thread;

        public GenericThread(GenericThreadStart start)
        {
            threadStart = start;
            thread = new Thread(InnerNoneParameterizedStart);
        }

        public GenericThread(PrameterizedGenericThreadStart<T> start)
        {
            threadParameterizedStart = start;
            thread = new Thread(InnerParameterizedStart);
        }

        public string Name
        {
            get { return thread.Name; }
            set { thread.Name = value; }
        }

        public Thread Thread
        {
            get { return thread; }
        }

        public ThreadState ThreadState
        {
            get { return thread.ThreadState; }
        }

        private void InnerNoneParameterizedStart()
        {
            threadStart.Invoke();
        }

        private void InnerParameterizedStart(object value)
        {
            threadParameterizedStart.Invoke(value is T ? (T) value : default(T));
        }

        public void Start()
        {
            thread.Start();
        }

        public void Start(T parameter)
        {
            thread.Start(parameter);
        }
    }
}