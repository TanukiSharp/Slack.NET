using System;
using System.Collections.Generic;
using System.Text;
using SlackDotNet.WebApi;

namespace SlackDotNet
{
    internal class Utils
    {
        internal static void AppendParseMode(IQueryBuilder queryBuilder, ParseMode parseMode, ParseMode defaultParseMode)
        {
            if (parseMode == defaultParseMode)
                return;

            if (parseMode == ParseMode.None)
                queryBuilder.Append("parse", StringConstants.None);
            else if (parseMode == ParseMode.Client)
                queryBuilder.Append("parse", StringConstants.Client);
            else if (parseMode == ParseMode.Full)
                queryBuilder.Append("parse", StringConstants.Full);
        }
    }
}
