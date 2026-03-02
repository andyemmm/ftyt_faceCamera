using ftyt_faceCamera;
using ftyt_faceCamera.IService;
using MaterialDesignThemes.Wpf;
using System;
using System.Threading.Tasks;

namespace ftyt_faceCamera.Service {
    public class DialogService : IService.IDialogService {
        public async Task<AsyncActionResult> ShowBusyIndicatorAsync(Func<Task<AsyncActionResult>> dialogOpenedAsync, int delay = 0) {
            var result = AsyncActionResult.Succcess;
            await DialogHost.Show(
                null, 
                Identifier.MainDialogHost,
                async (sendr, e) => {
                    if (delay > 0) await Task.Delay(delay);
                    if (dialogOpenedAsync != null) result = await dialogOpenedAsync();
                    if (DialogHost.IsDialogOpen(Identifier.MainDialogHost)) DialogHost.Close(Identifier.MainDialogHost);
                }, 
                null);
            return result;
        }
    }
}
