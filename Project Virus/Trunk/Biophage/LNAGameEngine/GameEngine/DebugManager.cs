/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Text;
using System.Diagnostics;

namespace LNA.GameEngine
{
    /// <summary>
    /// This class provides basic error logging procedures. This class
    /// is thread safe as it inherits the safety policies from the
    /// '.NET' Debug and Trace classes.
    /// </summary>
    /// <remarks>
    /// The WriteLogEntry method will be active only in the DEBUG build
    /// of the project, whereas the Assert method will be active in both
    /// DEBUG and RELEASE builds. This class simply acts as a wrapper to
    /// the .NET 'Debug' and 'Trace' system classes. This is needed to
    /// help with defining a Lua script bind.
    /// </remarks>
    /// <see href="http://www.codeproject.com/KB/trace/debugtreatise.aspx"/>
    public class DebugManager
    {
        #region constants

        //Default log file path
        private const string defLogFilePath = "log.txt";

        #endregion

        #region methods

        #region construction

        //What both constructors must do in common
        private void ConstructHelper()
        {
            //get current date/time
            DateTime date = DateTime.Now;
            //write session's first log entry
            String logEntry = "BEGIN - ";
            logEntry += date.ToString();
            Debug.WriteLine("");
            Debug.WriteLine(logEntry);
            Debug.Flush();
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DebugManager()
        {
            //set debug output to log tex file and console
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(defLogFilePath));
            
            ConstructHelper();
        }

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="logFilePath">
        /// The directory path to the custom log text file.
        /// </param>
        public DebugManager(String logFilePath)
        {
            //check if parameter string is not empty and assign
            //as log file path. Otherwise use default log file
            //path
            Trace.Listeners.Clear();
            if (logFilePath.Length > 0)
                Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
            else
                Trace.Listeners.Add(new TextWriterTraceListener(defLogFilePath));

            ConstructHelper();
        }

        #endregion

        #region log_entry

        /// <summary>
        /// Append a log entry (String) to the log file. This method is
        /// active only in the DEBUG project build.
        /// </summary>
        /// <param name="logEntry">
        /// Log entry (String) to append to the log text file.
        /// </param>
        [Conditional("DEBUG")]
        public void WriteLogEntry(String logEntry)
        {
            //check valid entry
            if (logEntry.Length == 0)
                return;
            //write entry to log file
            String tempString = "DEBUG\t";
            tempString += logEntry;
            Debug.WriteLine(tempString);
            Debug.Flush();
        }

        /// <summary>
        /// If the passed condition is false, the log entry will be appended
        /// to the log text file. This method will be active for both the
        /// DEBUG and RELEASE project builds. Ideally this method will be
        /// used for critical errors and will log an entry when an end user
        /// runs into such a problem.
        /// </summary>
        /// <param name="condtion">
        /// If the assert condition is false, the log entry will be appended
        /// to the log text file.
        /// </param>
        /// <param name="logEntry">
        /// Log entry to be appended to the log text file if the assertion
        /// fails during runtime.
        /// </param>
        [Conditional("TRACE")]
        public void Assert(bool condtion, String logEntry)
        {
            //make log entry if assertion failed
            if (!condtion)
            {
                String sTemp = "FAIL\t";
                sTemp += logEntry;
                Trace.WriteLine(sTemp);
                //close trace and kill process
                Trace.Close();
#if !XBOX
                Process.GetCurrentProcess().Kill();
#endif
            }
        }

        #endregion

        #endregion
    }
}
