using System.Text;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Exceptions
{
    public class ErrorResponseFatNetLibException : FatNetLibException
    {
        public ErrorResponseFatNetLibException(string message, Package response)
            : base(BuildMessage(message, response))
        {
            Response = response;
        }

        public Package Response { get; }

        private static string BuildMessage(string message, Package response)
        {
            var result = new StringBuilder(message);
            if (!(response.Error is EndpointRunFailedView exceptionBody))
                return result.Append(". Error=")
                    .Append(response.Error)
                    .ToString();

            result.Append(". Running remote endpoint failed at route ")
                .Append(response.Route);
            AppendExceptionBody(result, exceptionBody);
            return result.ToString();
        }

        private static void AppendExceptionBody(StringBuilder result, EndpointRunFailedView exceptionView)
        {
            EndpointRunFailedView currentView = exceptionView;
            while (true)
            {
                // The code below builds messages like:
                //  - Type: Kolyhalov.FatNetLib.Core.Exceptions.FatNetLibException, Message: "outer-exception"
                result.Append("\n - ")
                    .Append(nameof(EndpointRunFailedView.Type))
                    .Append(": ")
                    .Append(currentView.Type)
                    .Append(", ")
                    .Append(nameof(EndpointRunFailedView.Message))
                    .Append(": \"")
                    .Append(currentView.Message)
                    .Append("\"");
                if (currentView.InnerException is null) return;
                currentView = currentView.InnerException;
            }
        }
    }
}
