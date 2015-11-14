using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnIRC.Shared.Helpers
{
    public class SemaphoreSlimUnlocker : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public SemaphoreSlimUnlocker(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore?.Release();
        }
    }

    public static class SemaphoreSlimExtensions
    {
        public static async Task<SemaphoreSlimUnlocker> LockAsync(this SemaphoreSlim semaphore)
        {
            var semaphoreUnlocker = new SemaphoreSlimUnlocker(semaphore);
            await semaphore.WaitAsync();
            return semaphoreUnlocker;
        }
    }
}
