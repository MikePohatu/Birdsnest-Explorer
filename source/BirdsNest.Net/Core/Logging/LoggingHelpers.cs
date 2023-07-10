﻿#region license
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
using NLog;
using NLog.Config;
using System.Linq;

namespace Core.Logging
{

    public static class LoggingHelpers
    {
        public static void AddLoggingHandler(NewLog handler)
        {
            if (handler == null) { return; }

            var receivers = NLog.LogManager.Configuration.AllTargets.Where(t => t is UserUITarget).Cast<UserUITarget>();
            foreach (UserUITarget receiver in receivers)
            { receiver.NewLogMessage += handler; }
        }

        public static void InitLogging()
        {
            NLog.LogManager.Setup().SetupExtensions(s =>
                s.RegisterTarget<UserUITarget>("UserUITarget")
                );
            //NLog.Config.ConfigurationItemFactory.Default.Targets.RegisterDefinition("UserUITarget", typeof(UserUITarget));
        }

        public static void SetLoggingLevel(LogLevel loglevel)
        {
            LoggingRule rule = LogManager.Configuration.FindRuleByName("outputPane");
            rule?.SetLoggingLevels(loglevel, LogLevel.Fatal);
            LogManager.ReconfigExistingLoggers();
        }
    }
}
