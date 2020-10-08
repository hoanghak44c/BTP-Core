using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace QLBH.Core.Exceptions
{
    [Serializable]
    public class ManagedException : Exception
    {
        private string message;

        private bool canThrow;
        /// <summary>
        /// Throw message without logging detail.
        /// </summary>
        /// <param name="message"></param>
        public ManagedException(string message)
            : base(message)
        {
            LoggingMessage(message, true);
        }

        /// <summary>
        /// Throw message with optional logging detail.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="nolog"></param>
        public ManagedException(string message, bool nolog)
            : base(message)
        {
            LoggingMessage(message, nolog);
        }

        ///// <summary>
        ///// Throw message without logging detail.
        ///// </summary>
        ///// <param name="message"></param>
        ///// <param name="paras"></param>
        public ManagedException(string message, params object[] paras)
            : base(message)
        {
            LoggingMessage(message, true, paras);
        }

        /// <summary>
        /// Throw message with optional logging detail.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="nolog"></param>
        /// <param name="paras"></param>
        public ManagedException(string message, bool nolog, params object[] paras)
            : base(message)
        {
            LoggingMessage(message, nolog, paras);
        }


        //private void LoggingMessage(string exceptionMessage, bool nolog)
        //{
        //    var objStackTrace = new StackTrace(true);

        //    message = exceptionMessage;

        //    NoLog = nolog;

        //    try
        //    {
        //        File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Error.log", message + "\n\r" + objStackTrace + "\n\r");
        //    }
        //    catch (IOException)
        //    {
        //        File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Error." + Path.GetRandomFileName() + ".log", message + "\n\r" + objStackTrace + "\n\r");
        //    }

        //    if (!nolog)
        //    {
        //        MethodBase methodExecuting = objStackTrace.GetFrame(2).GetMethod();

        //        canThrow = !String.IsNullOrEmpty(objStackTrace.GetFrame(3).GetFileName());

        //        if (objStackTrace.GetFrame(2).GetMethod().Module.Name.StartsWith(GetType().Module.Name.Replace("Exceptions.dll", String.Empty)))
        //        {
        //            this.message = exceptionMessage;

        //            return;
        //        }

        //        string stackTraceContent = String.Format(" at {0}.{1}()",

        //            methodExecuting.ReflectedType.FullName, methodExecuting.Name);

        //        stackTraceContent += String.Format(" at line {0}", objStackTrace.GetFrame(2).GetFileLineNumber());

        //        this.message = String.Format("{0}\n{1}", exceptionMessage, stackTraceContent);
        //    }            
        //}

        private void LoggingMessage(string exceptionMessage, bool nolog, params object[] paras)
        {
            var objStackTrace = new StackTrace(true);

            message = exceptionMessage;

            string tmp = String.Empty;

            foreach (var para in paras)
            {
                tmp += para + ",";
            }

            File.AppendAllText(String.Format("{0}{1}.trace.log", AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("dd-MM-yyyy")), String.Format("[{0}]: {1}\r\n{2}\r\n{3}\r\n", DateTime.Now,  message, objStackTrace, tmp));

            //if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Trace.log"))

            //    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "Trace.log");

            NoLog = nolog;

            //try
            //{
            //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Error.log", message + "\n\r" + objStackTrace + "\n\r");
            //}
            //catch (IOException)
            //{
            //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "Error." + Path.GetRandomFileName() + ".log", message + "\n\r" + objStackTrace + "\n\r");
            //}

            if (!nolog)
            {
                MethodBase methodExecuting = objStackTrace.GetFrame(2).GetMethod();

                canThrow = !String.IsNullOrEmpty(objStackTrace.GetFrame(3).GetFileName());

                if (objStackTrace.GetFrame(2).GetMethod().Module.Name.StartsWith(GetType().Module.Name.Replace("dll", String.Empty)))
                {
                    this.message = message;

                    return;
                }

                string sParams = String.Empty;

                string sFormat = " at {0}.{1}()";

                string stackTraceContent = String.Format(sFormat,

                    methodExecuting.ReflectedType.FullName, methodExecuting.Name);

                if (paras != null && paras.Length > 0)
                {
                    foreach (object para in paras)
                    {
                        sParams += para + ", ";
                    }

                    sParams = sParams.TrimEnd(", ".ToCharArray());

                    sFormat = " at {0}.{1}({2})";

                    stackTraceContent = String.Format(sFormat,

                        methodExecuting.ReflectedType.FullName, methodExecuting.Name, sParams);
                }

                stackTraceContent += String.Format(" at line {0}", objStackTrace.GetFrame(2).GetFileLineNumber());

                this.message = String.Format("{0}\n{1}", message, stackTraceContent);
            }
        }

        public bool CanThrow
        {
            get { return canThrow; }
        }

        public override string Message
        {
            get
            {
                return message;
            }
        }

        public bool NoLog { get; private set; }

        public override string StackTrace
        {
            get
            {
                return String.Empty;
            }
        }
    }
}