using ftyt_faceCamera;
using System;
using System.Threading.Tasks;

namespace ftyt_faceCamera.IService {
    public interface IDialogService {
        Task<AsyncActionResult> ShowBusyIndicatorAsync(Func<Task<AsyncActionResult>> dialogOpenedAsync, int delay = 0);  
    }
}
