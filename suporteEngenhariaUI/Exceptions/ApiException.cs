using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suporteEngenhariaUI.Exceptions
{
    public class ApiException : Exception
    {
        public string? ApiResponse { get; }

        public ApiException(string message) : base(message) { }
        public ApiException(string message, Exception innerException) : base(message, innerException) { }
        public ApiException(string message, Exception? innerException = null, string? apiResponse = null) : base(message, innerException)
        {
            ApiResponse = apiResponse;
        }
    }
}
