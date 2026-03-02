using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ftyt.faceCameraFramework
{
    public class AsyncActionResult
    {
        public AsyncActionResult(string error)
        {
            Error = error;
            IsSuccess = string.IsNullOrEmpty(error);
        }

        public bool IsSuccess { get; }
        public string Error { get; }

        public static readonly AsyncActionResult Succcess = new AsyncActionResult(string.Empty);
    }
}
