﻿using System.Xml.Linq;

namespace Endpoint.Application
{
    public static class Constants
    {
        public static readonly XNamespace MSBUILD = "";

        public static string RootNamespace = nameof(Endpoint);
        public static string Tab = "    ";
        public static string SettingsFileName = "clisettings.json";
        public static class Folders
        {
            public static string Extensions = nameof(Extensions);
            public static string Features = nameof(Features);
            public static string Models = nameof(Models);
            public static string Data = nameof(Data);
            public static string Controllers = nameof(Controllers);
            public static string Core = nameof(Core);
            public static string Behaviors = nameof(Behaviors);
            public static string Properties = nameof(Properties);
            public static string Interfaces = nameof(Interfaces);
            public static string Logs = nameof(Logs);
        }
    }
}
