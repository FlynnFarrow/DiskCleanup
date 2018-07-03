using System;
using System.Runtime.InteropServices;
using Sunburst.OutOfProcessComServer;
using Microsoft.DiskCleanupApi;

namespace Sunburst.DiskCleanupApp
{
    public abstract class DiskCleanupProvider : OutOfProcessComObject, IEmptyVolumeCache2
    {
        public DiskCleanupProvider() : base(OutOfProcessComObjectFlags.Default) { }

        protected string VolumeName { get; private set; }
        protected bool DiskSpaceCritical { get; private set; }
        
        protected abstract ProviderSettings Initialize();
        protected abstract ulong ComputeSpaceUsed(IComputeFreeSpaceProgress progress);
        protected abstract void FreeSpace(ulong desiredFreeSpace, IFreeSpaceProgress progress);
        protected virtual void ShowSettings(IntPtr hWndOwner) => throw new NotImplementedException();

        #region Nested Classes

        private class ComputeFreeSpaceProgress : IComputeFreeSpaceProgress
        {
            private readonly IEmptyVolumeCacheCallback Callback;
            public ComputeFreeSpaceProgress(IEmptyVolumeCacheCallback callback)
            {
                Callback = callback;
            }

            public void ReportProgress(ulong spaceUsed, string status) => Callback.ScanProgress(spaceUsed, 0, status);
            public void ReportComplete(ulong spaceUsed, string status) => Callback.ScanProgress(spaceUsed, EmptyVolumeCacheCallbackFlags.EVCCF_LASTNOTIFICATION, status);
        }

        private class FreeSpaceProgress : IFreeSpaceProgress
        {
            private readonly IEmptyVolumeCacheCallback Callback;
            public FreeSpaceProgress(IEmptyVolumeCacheCallback callback)
            {
                Callback = callback;
            }

            public void ReportProgress(ulong spaceFreed, ulong spaceToFree, string status) => Callback.PurgeProgress(spaceFreed, spaceToFree, 0, status);
            public void ReportComplete(ulong spaceFreed, ulong spaceToFree, string status) => Callback.PurgeProgress(spaceFreed, spaceToFree, EmptyVolumeCacheCallbackFlags.EVCCF_LASTNOTIFICATION, status);
        }

        #endregion

        #region IEmptyVolumeCache

        public void InitializeEx(IntPtr hRegKey, [MarshalAs(UnmanagedType.LPWStr)] string volume, [MarshalAs(UnmanagedType.LPWStr)] string keyName,
            [MarshalAs(UnmanagedType.LPWStr), Out] out string displayName, [MarshalAs(UnmanagedType.LPWStr), Out] out string description,
            [MarshalAs(UnmanagedType.LPWStr), Out] out string buttonText, ref EmptyVolumeCacheFlags flags)
        {
            VolumeName = volume;
            DiskSpaceCritical = flags.HasFlag(EmptyVolumeCacheFlags.EVCF_OUTOFDISKSPACE);

            ProviderSettings settings = Initialize();
            buttonText = settings.ButtonText;
            description = settings.Description;
            displayName = settings.DisplayName;

            if (settings.DisplayButton) flags |= EmptyVolumeCacheFlags.EVCF_HASSETTINGS;
            if (settings.EnableByDefault) flags |= EmptyVolumeCacheFlags.EVCF_ENABLEBYDEFAULT | EmptyVolumeCacheFlags.EVCF_ENABLEBYDEFAULT_AUTO;
            if (settings.NoShowIfSpaceZero) flags |= EmptyVolumeCacheFlags.EVCF_DONTSHOWIFZERO;
        }

        public void Initialize(IntPtr hRegKey, [MarshalAs(UnmanagedType.LPWStr)] string volume, [MarshalAs(UnmanagedType.LPWStr), Out] out string displayName,
            [MarshalAs(UnmanagedType.LPWStr), Out] out string description, ref EmptyVolumeCacheFlags flags)
        {
            // Deliberately not implemented; this should never be called by Windows, as InitializeEx() is preferred.
            throw new NotImplementedException();
        }

        public void GetSpaceUsed(out ulong spaceUsed, IEmptyVolumeCacheCallback callback)
        {
            spaceUsed = ComputeSpaceUsed(new ComputeFreeSpaceProgress(callback));
        }

        public void Purge(ulong spaceToFree, IEmptyVolumeCacheCallback callback)
        {
            FreeSpace(spaceToFree, new FreeSpaceProgress(callback));
        }

        public void ShowProperties(IntPtr hWndParent)
        {
            ShowSettings(hWndParent);
        }

        public void Deactivate(out EmptyVolumeCacheFlags flags)
        {
            flags = 0;
        }

        #endregion
    }

    public sealed class ProviderSettings
    {
        public string ButtonText { get; set; } = null;
        public string Description { get; set; } = null;
        public string DisplayName { get; set; } = null;
        public bool DisplayButton { get; set; } = false;
        public bool EnableByDefault { get; set; } = false;
        public bool NoShowIfSpaceZero { get; set; } = false;
    }

    public interface IComputeFreeSpaceProgress
    {
        void ReportProgress(ulong spaceUsed, string status);
        void ReportComplete(ulong spaceUsed, string status);
    }

    public interface IFreeSpaceProgress
    {
        void ReportProgress(ulong spaceFreed, ulong spaceToFree, string status);
        void ReportComplete(ulong spaceFreed, ulong spaceToFree, string status);
    }
}
