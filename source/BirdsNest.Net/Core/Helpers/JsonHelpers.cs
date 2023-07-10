#region license
// Copyright (c) 2019-2023 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;

namespace Core.Helpers
{
    public static class JsonHelpers
    {
        public static JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Error = OnSerializationError,
            TraceWriter = new JsonLogger(),
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public static T Deserialize<T>(string json)
        {
            T obj = JsonConvert.DeserializeObject<T>(json, SerializerSettings);
            return obj;
        }

        public static string Serialize(object data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented, SerializerSettings);
            return json;
        }

        private static void OnSerializationError(object sender, ErrorEventArgs args)
        {
            // only log an error once
            if (args.CurrentObject == args.ErrorContext.OriginalObject)
            {
                Log.Error(args.ErrorContext.Error, "Error serializing or deserializing json: " + args.ErrorContext.Error.Message);
            }
            args.ErrorContext.Handled = true;
        }

        private class JsonLogger : ITraceWriter
        {
            public TraceLevel LevelFilter { get; } = TraceLevel.Verbose;
            public void Trace(TraceLevel level, string s, Exception e)
            {
                Log.Trace(e, s);
            }
        }
    }
}
