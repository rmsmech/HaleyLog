﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Haley.Abstractions;
using Haley.Enums;
using Haley.Models;
using Haley.Utils;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Haley.Log
{
    public static class LogStore
    {
        private static ConcurrentDictionary<string, IHLogger> _loggers = new ConcurrentDictionary<string, IHLogger>();


        private static ConcurrentDictionary<Type, OutputInfo> _outputPaths = new ConcurrentDictionary<Type, OutputInfo>(); //For each file logger type, we can set a common output path.

        #region GENERIC METHODS
        public static IHLogger CreateLogger(Enum key, IHLogger logger)
        {
            string _key = key.GetKey();
            return CreateLogger(_key, logger);
        }
        public static IHLogger CreateLogger(string key, IHLogger logger)
        {
            if (!_loggers.ContainsKey(key))
            {
                _loggers.TryAdd(key, logger);
            }
            return _loggers[key];
        }
        public static IHLogger GetLogger(Enum key)
        {
            return GetLogger(key.GetKey());
        }
        public static IHLogger GetLogger(string key)
        {
            if (_loggers.ContainsKey(key)) return _loggers[key];
            return null;
        }
        #endregion

        #region FILE LOGGERS
        public static IHLogger GetOrAddFileLogger(Enum key, string loggerName)
        {
            return GetOrAddFileLogger(key.GetKey(), loggerName);
        }
        public static IHLogger GetOrAddFileLogger(string key, string loggerName)
        {
            return GetOrAddFileLogger(key, loggerName, OutputType.Text_simple);
        }
        public static IHLogger GetOrAddFileLogger(Enum key, string loggerName, OutputType outputype)
        {
            return GetOrAddFileLogger(key.GetKey(), loggerName, outputype, LogLevel.Information);
        }
        public static IHLogger GetOrAddFileLogger(string key, string loggerName, OutputType outputype)
        {
            return GetOrAddFileLogger(key, loggerName, outputype,LogLevel.Information);
        }
        public static IHLogger GetOrAddFileLogger(Enum key, string loggerName, OutputType outputype,LogLevel allowedLevel)
        {
            return GetOrAddFileLogger(key.GetKey(), loggerName, outputype,allowedLevel);
        }
        public static IHLogger GetOrAddFileLogger(string key, string loggerName, OutputType outputype, LogLevel allowedLevel)
        {
            return GetOrAddFileLogger(key,loggerName, outputype, allowedLevel, null,null);
        }
        public static IHLogger GetOrAddFileLogger(Enum key, string loggerName, OutputType outputype, LogLevel allowedLevel,string outputDirectory,string fileName)
        {
            return GetOrAddFileLogger(key.GetKey(), loggerName, outputype, allowedLevel,outputDirectory,fileName);
        }
        public static IHLogger GetOrAddFileLogger(string key, string loggerName, OutputType outputype, LogLevel allowedLevel, string outputDirectory, string fileName)
        {
            var _logger = GetLogger(key);
            if (_logger == null)
            {
                var _info = GetOutputInfo<FileLogger>(); //Because we are trying to create a file logger.
                if (string.IsNullOrWhiteSpace(outputDirectory) && _info != null && ! string.IsNullOrWhiteSpace(_info.Directory))
                {
                    outputDirectory = Path.GetDirectoryName(_info.Directory);
                }

                if (string.IsNullOrWhiteSpace(fileName) && _info != null && !string.IsNullOrWhiteSpace(_info.FileName))
                {
                    fileName = Path.GetFileNameWithoutExtension(_info.FileName);
                }

                _logger = CreateLogger(key, new FileLogger(loggerName ?? "HLog", allowedLevel, outputDirectory, fileName, outputype));
            }
            return _logger;
        }
        #endregion

        #region Logger Changes
        public static void ChangeAllLogLevels(LogLevel logLevel)
        {
            //this changes log level of all the internal loggers.
            foreach (var logger in _loggers)
            {
                if (logger.Value is HLoggerBase hbase)
                {
                    hbase.SetAllowedLevel(logLevel);
                }
            }
        }
        public static void ChangeLoggerLevel(Enum key, LogLevel logLevel)
        {
            ChangeLoggerLevel(key.GetKey(), logLevel);
        }
        public static void ChangeLoggerLevel(string key, LogLevel logLevel)
        {
            if (_loggers.ContainsKey(key))
            {
                if (_loggers[key] is HLoggerBase hlog)
                {
                    hlog.SetAllowedLevel(logLevel);
                }
            }
        }
        public static OutputInfo SetOutputInfo<T>(OutputInfo info) where T : Type
        {
            //User can choose to set a common output path for a given type.
            //When other similar types are generated without any output path specified, then we get that default value.
            if (info == null) return null;

            if (!_outputPaths.ContainsKey(typeof(T)))
            {
                _outputPaths.TryAdd(typeof(T), info);
            }
            return _outputPaths[typeof(T)];
        }
        private static OutputInfo GetOutputInfo<T>()
        {
            if (_outputPaths.ContainsKey(typeof(T)))
            {
                return _outputPaths[typeof(T)];
            }
            return null;
        }
        #endregion
    }
}
