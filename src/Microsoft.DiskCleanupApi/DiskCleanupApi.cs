using System;
using System.Runtime.InteropServices;

namespace Microsoft.DiskCleanupApi
{
    [Flags]
    public enum EmptyVolumeCacheFlags
    {
        EVCF_HASSETTINGS = 0x1,
        EVCF_ENABLEBYDEFAULT = 0x2,
        EVCF_REMOVEFROMLIST = 0x4,
        EVCF_ENABLEBYDEFAULT_AUTO = 0x8,
        EVCF_DONTSHOWIFZERO = 0x10,
        EVCF_SETTINGSMODE = 0x20,
        EVCF_OUTOFDISKSPACE = 0x40,
        EVCF_USERCONSENTOBTAINED = 0x80,
        EVCF_SYSTEMAUTORUN = 0x100
    }

    [Flags]
    public enum EmptyVolumeCacheCallbackFlags
    {
        EVCCF_LASTNOTIFICATION = 0x1
    }

    [ComImport, Guid("6E793361-73C6-11D0-8469-00AA00442901")]
    public interface IEmptyVolumeCacheCallback
    {
        [PreserveSig]
        int ScanProgress(ulong spaceUsed, EmptyVolumeCacheCallbackFlags flags, [MarshalAs(UnmanagedType.LPWStr)] string status);
        [PreserveSig]
        int PurgeProgress(ulong spaceFreed, ulong spaceToFree, EmptyVolumeCacheCallbackFlags flags, [MarshalAs(UnmanagedType.LPWStr)] string status);
    }

    [ComImport, Guid("8FCE5227-04DA-11d1-A004-00805F8ABE06")]
    public interface IEmptyVolumeCache
    {
        void Initialize(IntPtr hRegKey, [MarshalAs(UnmanagedType.LPWStr)] string volume,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string displayName, [Out, MarshalAs(UnmanagedType.LPWStr)] out string description,
            ref EmptyVolumeCacheFlags flags);

        void GetSpaceUsed(out ulong spaceUsed, IEmptyVolumeCacheCallback callback);
        void Purge(ulong spaceToFree, IEmptyVolumeCacheCallback callback);
        void ShowProperties(IntPtr hWndParent);
        void Deactivate(out EmptyVolumeCacheFlags flags);
    }

    [ComImport, Guid("02b7e3ba-4db3-11d2-b2d9-00c04f8eec8c")]
    public interface IEmptyVolumeCache2 : IEmptyVolumeCache
    {
        void InitializeEx(IntPtr hRegKey, [MarshalAs(UnmanagedType.LPWStr)] string volume, [MarshalAs(UnmanagedType.LPWStr)] string keyName,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string displayName, [Out, MarshalAs(UnmanagedType.LPWStr)] out string description,
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string buttonText, ref EmptyVolumeCacheFlags flags);
    }
}
