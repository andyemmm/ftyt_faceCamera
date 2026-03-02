using ftyt.faceCameraFramework;
using System;
using System.Threading.Tasks;

namespace ftyt.faceCameraFramework.IService {
    public interface IDialogService {
        Task<AsyncActionResult> ShowBusyIndicatorAsync(Func<Task<AsyncActionResult>> dialogOpenedAsync, int delay = 0);  
    }
}
