using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Newtonsoft.Json;

namespace Middleware.GlobalExceptionHandler
{
    public class ExceptionHandler
    {
    
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static string HandleException(Exception ex, out object errorResponse)
        {
            _logger.Error(ex, "An error occurred in the application!!");

            errorResponse = new
            {
                Success = false,
                Message = "An error occurred",
                Error = ex.Message
            };
            return JsonConvert.SerializeObject(errorResponse);
        }

        //Method to create a response object without serialization
        public static object CreateErrorResponse(Exception ex)
        {
            _logger.Error(ex, "An error occurred!");
            return new
            {
                Success = false,
                Message = "An error occurred",
                Error = ex.Message
            };

        }
    }
}