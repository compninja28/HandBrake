﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodeBase.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   A Base Class for the Encode Services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrakeWPF.Services.Encode
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;

    using HandBrake.ApplicationServices.Model;
    using HandBrake.ApplicationServices.Services.Logging;
    using HandBrake.ApplicationServices.Services.Logging.Interfaces;

    using EncodeCompletedEventArgs = HandBrakeWPF.Services.Encode.EventArgs.EncodeCompletedEventArgs;
    using EncodeCompletedStatus = HandBrakeWPF.Services.Encode.Interfaces.EncodeCompletedStatus;
    using EncodeProgessStatus = HandBrakeWPF.Services.Encode.Interfaces.EncodeProgessStatus;
    using EncodeProgressEventArgs = HandBrakeWPF.Services.Encode.EventArgs.EncodeProgressEventArgs;
    using EncodeTask = HandBrakeWPF.Services.Encode.Model.EncodeTask;
    using GeneralApplicationException = HandBrakeWPF.Exceptions.GeneralApplicationException;

    /// <summary>
    /// A Base Class for the Encode Services.
    /// </summary>
    public class EncodeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncodeBase"/> class.
        /// </summary>
        public EncodeBase()
        {    
        }

        #region Events

        /// <summary>
        /// Fires when a new QueueTask starts
        /// </summary>
        public event EventHandler EncodeStarted;

        /// <summary>
        /// Fires when a QueueTask finishes.
        /// </summary>
        public event EncodeCompletedStatus EncodeCompleted;

        /// <summary>
        /// Encode process has progressed
        /// </summary>
        public event EncodeProgessStatus EncodeStatusChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether IsEncoding.
        /// </summary>
        public bool IsEncoding { get; protected set; }

        #endregion

        #region Invoke Events

        /// <summary>
        /// Invoke the Encode Status Changed Event.
        /// </summary>
        /// <param name="e">
        /// The EncodeProgressEventArgs.
        /// </param>
        public void InvokeEncodeStatusChanged(EncodeProgressEventArgs e)
        {
            EncodeProgessStatus handler = this.EncodeStatusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Invoke the Encode Completed Event
        /// </summary>
        /// <param name="e">
        /// The EncodeCompletedEventArgs.
        /// </param>
        public void InvokeEncodeCompleted(EncodeCompletedEventArgs e)
        {
            EncodeCompletedStatus handler = this.EncodeCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Invoke the Encode Started Event
        /// </summary>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        public void InvokeEncodeStarted(System.EventArgs e)
        {
            EventHandler handler = this.EncodeStarted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Save a copy of the log to the users desired location or a default location
        /// if this feature is enabled in options.
        /// </summary>
        /// <param name="destination">
        /// The Destination File Path
        /// </param>
        /// <param name="isPreview">
        /// The is Preview.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        public void ProcessLogs(string destination, bool isPreview, HBConfiguration configuration)
        {
            try
            {
                string logDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\HandBrake\\logs";
                string encodeDestinationPath = Path.GetDirectoryName(destination);
                string destinationFile = Path.GetFileName(destination);
                string encodeLogFile = destinationFile + " " + DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace("/", "-").Replace(":", "-") + ".txt";
                ILog log = LogService.GetLogger();
                string logContent = log.ActivityLog;

                // Make sure the log directory exists.
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                // Copy the Log to HandBrakes log folder in the users applciation data folder.
                this.WriteFile(logContent, Path.Combine(logDir, encodeLogFile));

                // Save a copy of the log file in the same location as the enocde.
                if (configuration.SaveLogWithVideo)
                {
                    this.WriteFile(logContent, Path.Combine(encodeDestinationPath, encodeLogFile));
                }

                // Save a copy of the log file to a user specified location
                if (Directory.Exists(configuration.SaveLogCopyDirectory) && configuration.SaveLogToCopyDirectory)
                {
                    this.WriteFile(logContent, Path.Combine(configuration.SaveLogCopyDirectory, encodeLogFile));
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc); // This exception doesn't warrent user interaction, but it should be logged
            }
        }

        /// <summary>
        /// The write file.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        private void WriteFile(string fileName, string content)
        {
            try
            {
                using (StreamWriter fileWriter = new StreamWriter(fileName) { AutoFlush = true })
                {
                    fileWriter.WriteLineAsync(content);
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
            }
        }

        /// <summary>
        /// Verify the Encode Destination path exists and if not, create it.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        /// <exception cref="Exception">
        /// If the creation fails, an exception is thrown.
        /// </exception>
        protected void VerifyEncodeDestinationPath(EncodeTask task)
        {
            // Make sure the path exists, attempt to create it if it doesn't
            try
            {
                string path = Directory.GetParent(task.Destination).ToString();
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception exc)
            {
                throw new GeneralApplicationException(
                    "Unable to create directory for the encoded output.", "Please verify that you have a valid path.", exc);
            }
        }

        #endregion
    }
}