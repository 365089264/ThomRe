using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace CNE.Scheduler.Extension
{

    public abstract class CtrlBreakHandler
    {
        // Use a List to remember each unique callback function pointer that is registered
        private static List<HandlerRoutine> callbacks = new List<HandlerRoutine>();
        private static bool terminated;
        private static bool initialized;

        private static HandlerRoutine defaultHandlerRoutine = new HandlerRoutine(TermHandlerRoutine);

        public static bool Init()
        {
            return Init(defaultHandlerRoutine);
        }

        public static bool Init(HandlerRoutine callback)
        {
            // Add the callback delegate to the list
            // only if the list does not currently contain it
            // and register the callback delegate with the operating system.
            if (!(callbacks.Contains(callback)))
            {
                callbacks.Add(callback);
                if (!SetConsoleCtrlHandler(callback, true))
                {


                    return false;
                }
            }

            if (initialized)
            {
                return true;
            }

            initialized = true;
            terminated = false;

            return true;
        }

        public static void Exit()
        {
            if (!initialized)
            {
                return;
            }

            // Unregister each callback delegate that the list contains
            for (int i = 0; i < callbacks.Count; i++)
            {
                SetConsoleCtrlHandler(callbacks[i], false);
            }

            callbacks.Clear();

            initialized = false;
            terminated = true;
        }

        public static void ForceExit()
        {
            if (!initialized)
            {
                return;
            }

            // Send CTRL_C_EVENT to all processes that share the console
            // of this process. For more info please see description
            // of the GenerateConsoleCtrlEvent()
            bool retCode = GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);
            //if (!retCode)
            //{
            //    int retError = Marshal.GetLastWin32Error();
            //}
        }

        public static void Sleep(int msec)
        {
            Thread.Sleep(msec);
        }

        public static bool IsTerminated()
        {
            return terminated;
        }

        public static void SetTerminated(bool termi)
        {
            terminated = termi;
        }

        private static bool TermHandlerRoutine(CtrlTypes dwCtrlType)
        {
            switch (dwCtrlType)
            {
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_BREAK_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                case CtrlTypes.CTRL_C_EVENT:
                    terminated = true;
                    return true;
            }
            return false;
        }

        #region unmanaged
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GenerateConsoleCtrlEvent(CtrlTypes sigevent, int dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        public delegate bool HandlerRoutine(CtrlTypes ctrlType);
        #endregion
    }
}
