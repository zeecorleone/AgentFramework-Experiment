using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AF.Shared.Extensions;

public static class UsageDetailsExtensions
{
    extension(UsageDetails? usageDetails)
    {
        public void OutputAsInformation()
        {
            if (usageDetails is null)
                return;
            Console.WriteLine("\n\nUsage:");
            Utils.WriteLineMagenta($"- Input Tokens: {usageDetails.InputTokenCount}");
            Utils.WriteLineMagenta($"- Output Tokens: {usageDetails.InputTokenCount}" +
                                    $" ({usageDetails.ReasoningTokenCount ?? 0} was used for reasoning)");
        }
    }
}
