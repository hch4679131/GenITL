using System.IO;
using System.Runtime.InteropServices;

namespace GenICamCameraCapture.GenTL;

/// <summary>
/// GenTL 1.5 标准 API 封装
/// 通过 LoadLibrary 加载 .cti 文件，使用 GetProcAddress 绑定函数指针
/// </summary>
public class GenTLApi : IDisposable
{
    private IntPtr _hModule;
    private bool _initialized;

    #region Kernel32

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibraryW(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetDllDirectoryW(string lpPathName);

    #endregion

    #region GenTL 委托定义

    // ---- 库级别 ----
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCInitLibDelegate();

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCCloseLibDelegate();

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCGetInfoDelegate(
        int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    // ---- TL 级别 ----
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR TLOpenDelegate(out IntPtr phTL);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR TLCloseDelegate(IntPtr hTL);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR TLGetInfoDelegate(
        IntPtr hTL, int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR TLGetNumInterfacesDelegate(IntPtr hTL, out uint piNumIfaces);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR TLGetInterfaceIDDelegate(
        IntPtr hTL, uint iIndex, IntPtr sID, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR TLGetInterfaceInfoDelegate(
        IntPtr hTL, [MarshalAs(UnmanagedType.LPStr)] string sIfaceID,
        int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR TLOpenInterfaceDelegate(
        IntPtr hTL, [MarshalAs(UnmanagedType.LPStr)] string sIfaceID, out IntPtr phIface);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR TLUpdateInterfaceListDelegate(
        IntPtr hTL, ref byte pbChanged, ulong iTimeout);

    // ---- 接口级别 ----
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR IFCloseDelegate(IntPtr hIface);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR IFGetInfoDelegate(
        IntPtr hIface, int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR IFGetNumDevicesDelegate(IntPtr hIface, out uint piNumDevices);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR IFGetDeviceIDDelegate(
        IntPtr hIface, uint iIndex, IntPtr sID, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR IFUpdateDeviceListDelegate(
        IntPtr hIface, ref byte pbChanged, ulong iTimeout);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR IFGetDeviceInfoDelegate(
        IntPtr hIface, [MarshalAs(UnmanagedType.LPStr)] string sDeviceID,
        int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR IFOpenDeviceDelegate(
        IntPtr hIface, [MarshalAs(UnmanagedType.LPStr)] string sDeviceID,
        uint iOpenFlags, out IntPtr phDevice);

    // ---- 设备级别 ----
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DevCloseDelegate(IntPtr hDevice);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DevGetInfoDelegate(
        IntPtr hDevice, int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DevGetNumDataStreamsDelegate(IntPtr hDevice, out uint piNumDataStreams);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DevGetDataStreamIDDelegate(
        IntPtr hDevice, uint iIndex, IntPtr sDataStreamID, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DevOpenDataStreamDelegate(
        IntPtr hDevice, [MarshalAs(UnmanagedType.LPStr)] string sDataStreamID, out IntPtr phDataStream);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DevGetPortDelegate(IntPtr hDevice, out IntPtr phRemoteDevice);

    // ---- Port 级别 ----
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCReadPortDelegate(
        IntPtr hPort, ulong iAddress, IntPtr pBuffer, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCWritePortDelegate(
        IntPtr hPort, ulong iAddress, IntPtr pBuffer, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCGetPortInfoDelegate(
        IntPtr hPort, int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCGetNumPortURLsDelegate(IntPtr hPort, out uint piNumURLs);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCGetPortURLInfoDelegate(
        IntPtr hPort, uint iURLIndex, int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    // ---- DataStream 级别 ----
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSCloseDelegate(IntPtr hDataStream);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSGetInfoDelegate(
        IntPtr hDataStream, int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSAllocAndAnnounceBufferDelegate(
        IntPtr hDataStream, long iSize, IntPtr pPrivate, out IntPtr phBuffer);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSAnnounceBufferDelegate(
        IntPtr hDataStream, IntPtr pBuffer, long iSize, IntPtr pPrivate, out IntPtr phBuffer);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSRevokeBufferDelegate(
        IntPtr hDataStream, IntPtr hBuffer, out IntPtr pBuffer, out IntPtr pPrivate);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSQueueBufferDelegate(IntPtr hDataStream, IntPtr hBuffer);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSFlushQueueDelegate(IntPtr hDataStream, uint iOperation);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSStartAcquisitionDelegate(
        IntPtr hDataStream, uint iStartFlags, ulong iNumToAcquire);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSStopAcquisitionDelegate(
        IntPtr hDataStream, uint iStopFlags);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR DSGetBufferInfoDelegate(
        IntPtr hDataStream, IntPtr hBuffer, int iInfoCmd, ref int piType, IntPtr pBuffer, ref long piSize);

    // ---- Event 级别 ----
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR EventGetDataDelegate(
        IntPtr hEvent, IntPtr pBuffer, ref long piSize, ulong iTimeout);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCRegisterEventDelegate(
        IntPtr hModule, uint iEventID, out IntPtr phEvent);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR GCUnregisterEventDelegate(
        IntPtr hModule, uint iEventID);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR EventFlushDelegate(IntPtr hEvent);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate GC_ERROR EventKillDelegate(IntPtr hEvent);

    #endregion

    #region 函数实例

    private GCInitLibDelegate? _gcInitLib;
    private GCCloseLibDelegate? _gcCloseLib;
    private GCGetInfoDelegate? _gcGetInfo;
    private TLOpenDelegate? _tlOpen;
    private TLCloseDelegate? _tlClose;
    private TLGetInfoDelegate? _tlGetInfo;
    private TLGetNumInterfacesDelegate? _tlGetNumInterfaces;
    private TLGetInterfaceIDDelegate? _tlGetInterfaceID;
    private TLGetInterfaceInfoDelegate? _tlGetInterfaceInfo;
    private TLOpenInterfaceDelegate? _tlOpenInterface;
    private TLUpdateInterfaceListDelegate? _tlUpdateInterfaceList;
    private IFCloseDelegate? _ifClose;
    private IFGetInfoDelegate? _ifGetInfo;
    private IFGetNumDevicesDelegate? _ifGetNumDevices;
    private IFGetDeviceIDDelegate? _ifGetDeviceID;
    private IFUpdateDeviceListDelegate? _ifUpdateDeviceList;
    private IFGetDeviceInfoDelegate? _ifGetDeviceInfo;
    private IFOpenDeviceDelegate? _ifOpenDevice;
    private DevCloseDelegate? _devClose;
    private DevGetInfoDelegate? _devGetInfo;
    private DevGetNumDataStreamsDelegate? _devGetNumDataStreams;
    private DevGetDataStreamIDDelegate? _devGetDataStreamID;
    private DevOpenDataStreamDelegate? _devOpenDataStream;
    private DevGetPortDelegate? _devGetPort;
    private GCReadPortDelegate? _gcReadPort;
    private GCWritePortDelegate? _gcWritePort;
    private GCGetPortInfoDelegate? _gcGetPortInfo;
    private GCGetNumPortURLsDelegate? _gcGetNumPortURLs;
    private GCGetPortURLInfoDelegate? _gcGetPortURLInfo;
    private DSCloseDelegate? _dsClose;
    private DSGetInfoDelegate? _dsGetInfo;
    private DSAllocAndAnnounceBufferDelegate? _dsAllocAndAnnounceBuffer;
    private DSAnnounceBufferDelegate? _dsAnnounceBuffer;
    private DSRevokeBufferDelegate? _dsRevokeBuffer;
    private DSQueueBufferDelegate? _dsQueueBuffer;
    private DSFlushQueueDelegate? _dsFlushQueue;
    private DSStartAcquisitionDelegate? _dsStartAcquisition;
    private DSStopAcquisitionDelegate? _dsStopAcquisition;
    private DSGetBufferInfoDelegate? _dsGetBufferInfo;
    private EventGetDataDelegate? _eventGetData;
    private GCRegisterEventDelegate? _gcRegisterEvent;
    private GCUnregisterEventDelegate? _gcUnregisterEvent;
    private EventFlushDelegate? _eventFlush;
    private EventKillDelegate? _eventKill;

    #endregion

    #region 加载 .cti

    /// <summary>
    /// 加载 .cti 文件
    /// </summary>
    public void Load(string ctiPath)
    {
        if (!File.Exists(ctiPath))
            throw new FileNotFoundException($"CTI 文件不存在: {ctiPath}");

        // 设置 DLL 搜索目录为 .cti 所在目录，解决依赖问题
        string? dir = Path.GetDirectoryName(ctiPath);
        if (!string.IsNullOrEmpty(dir))
            SetDllDirectoryW(dir);

        _hModule = LoadLibraryW(ctiPath);
        if (_hModule == IntPtr.Zero)
        {
            int err = Marshal.GetLastWin32Error();
            throw new GenTLException(GC_ERROR.GC_ERR_ERROR,
                $"无法加载 CTI 文件: {ctiPath}, Win32 Error: {err}");
        }

        BindFunctions();
    }

    private void BindFunctions()
    {
        _gcInitLib = GetFunc<GCInitLibDelegate>("GCInitLib");
        _gcCloseLib = GetFunc<GCCloseLibDelegate>("GCCloseLib");
        _gcGetInfo = TryGetFunc<GCGetInfoDelegate>("GCGetInfo");
        _tlOpen = GetFunc<TLOpenDelegate>("TLOpen");
        _tlClose = GetFunc<TLCloseDelegate>("TLClose");
        _tlGetInfo = TryGetFunc<TLGetInfoDelegate>("TLGetInfo");
        _tlGetNumInterfaces = GetFunc<TLGetNumInterfacesDelegate>("TLGetNumInterfaces");
        _tlGetInterfaceID = GetFunc<TLGetInterfaceIDDelegate>("TLGetInterfaceID");
        _tlGetInterfaceInfo = TryGetFunc<TLGetInterfaceInfoDelegate>("TLGetInterfaceInfo");
        _tlOpenInterface = GetFunc<TLOpenInterfaceDelegate>("TLOpenInterface");
        _tlUpdateInterfaceList = GetFunc<TLUpdateInterfaceListDelegate>("TLUpdateInterfaceList");
        _ifClose = GetFunc<IFCloseDelegate>("IFClose");
        _ifGetInfo = TryGetFunc<IFGetInfoDelegate>("IFGetInfo");
        _ifGetNumDevices = GetFunc<IFGetNumDevicesDelegate>("IFGetNumDevices");
        _ifGetDeviceID = GetFunc<IFGetDeviceIDDelegate>("IFGetDeviceID");
        _ifUpdateDeviceList = GetFunc<IFUpdateDeviceListDelegate>("IFUpdateDeviceList");
        _ifGetDeviceInfo = TryGetFunc<IFGetDeviceInfoDelegate>("IFGetDeviceInfo");
        _ifOpenDevice = GetFunc<IFOpenDeviceDelegate>("IFOpenDevice");
        _devClose = GetFunc<DevCloseDelegate>("DevClose");
        _devGetInfo = TryGetFunc<DevGetInfoDelegate>("DevGetInfo");
        _devGetNumDataStreams = GetFunc<DevGetNumDataStreamsDelegate>("DevGetNumDataStreams");
        _devGetDataStreamID = GetFunc<DevGetDataStreamIDDelegate>("DevGetDataStreamID");
        _devOpenDataStream = GetFunc<DevOpenDataStreamDelegate>("DevOpenDataStream");
        _devGetPort = GetFunc<DevGetPortDelegate>("DevGetPort");
        _gcReadPort = GetFunc<GCReadPortDelegate>("GCReadPort");
        _gcWritePort = GetFunc<GCWritePortDelegate>("GCWritePort");
        _gcGetPortInfo = TryGetFunc<GCGetPortInfoDelegate>("GCGetPortInfo");
        _gcGetNumPortURLs = TryGetFunc<GCGetNumPortURLsDelegate>("GCGetNumPortURLs");
        _gcGetPortURLInfo = TryGetFunc<GCGetPortURLInfoDelegate>("GCGetPortURLInfo");
        _dsClose = GetFunc<DSCloseDelegate>("DSClose");
        _dsGetInfo = GetFunc<DSGetInfoDelegate>("DSGetInfo");
        _dsAllocAndAnnounceBuffer = TryGetFunc<DSAllocAndAnnounceBufferDelegate>("DSAllocAndAnnounceBuffer");
        _dsAnnounceBuffer = GetFunc<DSAnnounceBufferDelegate>("DSAnnounceBuffer");
        _dsRevokeBuffer = GetFunc<DSRevokeBufferDelegate>("DSRevokeBuffer");
        _dsQueueBuffer = GetFunc<DSQueueBufferDelegate>("DSQueueBuffer");
        _dsFlushQueue = GetFunc<DSFlushQueueDelegate>("DSFlushQueue");
        _dsStartAcquisition = GetFunc<DSStartAcquisitionDelegate>("DSStartAcquisition");
        _dsStopAcquisition = GetFunc<DSStopAcquisitionDelegate>("DSStopAcquisition");
        _dsGetBufferInfo = GetFunc<DSGetBufferInfoDelegate>("DSGetBufferInfo");
        _eventGetData = GetFunc<EventGetDataDelegate>("EventGetData");
        _gcRegisterEvent = GetFunc<GCRegisterEventDelegate>("GCRegisterEvent");
        _gcUnregisterEvent = GetFunc<GCUnregisterEventDelegate>("GCUnregisterEvent");
        _eventFlush = TryGetFunc<EventFlushDelegate>("EventFlush");
        _eventKill = TryGetFunc<EventKillDelegate>("EventKill");
    }

    private T GetFunc<T>(string name) where T : Delegate
    {
        IntPtr ptr = GetProcAddress(_hModule, name);
        if (ptr == IntPtr.Zero)
            throw new GenTLException(GC_ERROR.GC_ERR_NOT_IMPLEMENTED, $"函数未找到: {name}");
        return Marshal.GetDelegateForFunctionPointer<T>(ptr);
    }

    private T? TryGetFunc<T>(string name) where T : Delegate
    {
        IntPtr ptr = GetProcAddress(_hModule, name);
        return ptr == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer<T>(ptr);
    }

    #endregion

    #region 公共 API

    private void Check(GC_ERROR err, string context = "")
    {
        if (err != GC_ERROR.GC_ERR_SUCCESS)
            throw new GenTLException(err, context);
    }

    // ---- 库级别 ----
    public void GCInitLib()
    {
        Check(_gcInitLib!(), "GCInitLib");
        _initialized = true;
    }

    public void GCCloseLib()
    {
        if (_initialized)
        {
            Check(_gcCloseLib!(), "GCCloseLib");
            _initialized = false;
        }
    }

    // ---- TL 级别 ----
    public IntPtr TLOpen()
    {
        Check(_tlOpen!(out IntPtr hTL), "TLOpen");
        return hTL;
    }

    public void TLClose(IntPtr hTL)
    {
        Check(_tlClose!(hTL), "TLClose");
    }

    public bool TLUpdateInterfaceList(IntPtr hTL, ulong timeout = 1000)
    {
        byte changed = 0;
        Check(_tlUpdateInterfaceList!(hTL, ref changed, timeout), "TLUpdateInterfaceList");
        return changed != 0;
    }

    public uint TLGetNumInterfaces(IntPtr hTL)
    {
        Check(_tlGetNumInterfaces!(hTL, out uint num), "TLGetNumInterfaces");
        return num;
    }

    public string TLGetInterfaceID(IntPtr hTL, uint index)
    {
        long size = 0;
        var err = _tlGetInterfaceID!(hTL, index, IntPtr.Zero, ref size);
        if (err != GC_ERROR.GC_ERR_SUCCESS && err != GC_ERROR.GC_ERR_BUFFER_TOO_SMALL)
            throw new GenTLException(err, "TLGetInterfaceID (size query)");

        IntPtr buf = Marshal.AllocHGlobal((int)size);
        try
        {
            Check(_tlGetInterfaceID(hTL, index, buf, ref size), "TLGetInterfaceID");
            return Marshal.PtrToStringAnsi(buf) ?? "";
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    public IntPtr TLOpenInterface(IntPtr hTL, string ifaceID)
    {
        Check(_tlOpenInterface!(hTL, ifaceID, out IntPtr hIface), "TLOpenInterface");
        return hIface;
    }

    // ---- Interface 级别 ----
    public void IFClose(IntPtr hIface)
    {
        Check(_ifClose!(hIface), "IFClose");
    }

    public bool IFUpdateDeviceList(IntPtr hIface, ulong timeout = 1000)
    {
        byte changed = 0;
        Check(_ifUpdateDeviceList!(hIface, ref changed, timeout), "IFUpdateDeviceList");
        return changed != 0;
    }

    public uint IFGetNumDevices(IntPtr hIface)
    {
        Check(_ifGetNumDevices!(hIface, out uint num), "IFGetNumDevices");
        return num;
    }

    public string IFGetDeviceID(IntPtr hIface, uint index)
    {
        long size = 0;
        var err = _ifGetDeviceID!(hIface, index, IntPtr.Zero, ref size);
        if (err != GC_ERROR.GC_ERR_SUCCESS && err != GC_ERROR.GC_ERR_BUFFER_TOO_SMALL)
            throw new GenTLException(err, "IFGetDeviceID (size query)");

        IntPtr buf = Marshal.AllocHGlobal((int)size);
        try
        {
            Check(_ifGetDeviceID(hIface, index, buf, ref size), "IFGetDeviceID");
            return Marshal.PtrToStringAnsi(buf) ?? "";
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    public string IFGetDeviceInfo(IntPtr hIface, string deviceID, DEVICE_INFO_CMD cmd)
    {
        if (_ifGetDeviceInfo == null) return "";
        int type = 0;
        long size = 0;
        var err = _ifGetDeviceInfo(hIface, deviceID, (int)cmd, ref type, IntPtr.Zero, ref size);
        if (err != GC_ERROR.GC_ERR_SUCCESS && err != GC_ERROR.GC_ERR_BUFFER_TOO_SMALL)
            return "";

        IntPtr buf = Marshal.AllocHGlobal((int)size);
        try
        {
            err = _ifGetDeviceInfo(hIface, deviceID, (int)cmd, ref type, buf, ref size);
            if (err != GC_ERROR.GC_ERR_SUCCESS) return "";
            return Marshal.PtrToStringAnsi(buf) ?? "";
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    public IntPtr IFOpenDevice(IntPtr hIface, string deviceID,
        DEVICE_ACCESS_FLAGS flags = DEVICE_ACCESS_FLAGS.DEVICE_ACCESS_CONTROL)
    {
        Check(_ifOpenDevice!(hIface, deviceID, (uint)flags, out IntPtr hDevice), "IFOpenDevice");
        return hDevice;
    }

    // ---- Device 级别 ----
    public void DevClose(IntPtr hDevice)
    {
        Check(_devClose!(hDevice), "DevClose");
    }

    public uint DevGetNumDataStreams(IntPtr hDevice)
    {
        Check(_devGetNumDataStreams!(hDevice, out uint num), "DevGetNumDataStreams");
        return num;
    }

    public string DevGetDataStreamID(IntPtr hDevice, uint index)
    {
        long size = 0;
        var err = _devGetDataStreamID!(hDevice, index, IntPtr.Zero, ref size);
        if (err != GC_ERROR.GC_ERR_SUCCESS && err != GC_ERROR.GC_ERR_BUFFER_TOO_SMALL)
            throw new GenTLException(err, "DevGetDataStreamID (size query)");

        IntPtr buf = Marshal.AllocHGlobal((int)size);
        try
        {
            Check(_devGetDataStreamID(hDevice, index, buf, ref size), "DevGetDataStreamID");
            return Marshal.PtrToStringAnsi(buf) ?? "";
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    public IntPtr DevOpenDataStream(IntPtr hDevice, string streamID)
    {
        Check(_devOpenDataStream!(hDevice, streamID, out IntPtr hStream), "DevOpenDataStream");
        return hStream;
    }

    public IntPtr DevGetPort(IntPtr hDevice)
    {
        Check(_devGetPort!(hDevice, out IntPtr hPort), "DevGetPort");
        return hPort;
    }

    // ---- Port 级别 ----
    public byte[] GCReadPort(IntPtr hPort, ulong address, long size)
    {
        IntPtr buf = Marshal.AllocHGlobal((int)size);
        try
        {
            long readSize = size;
            Check(_gcReadPort!(hPort, address, buf, ref readSize), "GCReadPort");
            byte[] data = new byte[readSize];
            Marshal.Copy(buf, data, 0, (int)readSize);
            return data;
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    public void GCWritePort(IntPtr hPort, ulong address, byte[] data)
    {
        IntPtr buf = Marshal.AllocHGlobal(data.Length);
        try
        {
            Marshal.Copy(data, 0, buf, data.Length);
            long size = data.Length;
            Check(_gcWritePort!(hPort, address, buf, ref size), "GCWritePort");
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    public uint GCGetNumPortURLs(IntPtr hPort)
    {
        if (_gcGetNumPortURLs == null) return 0;
        Check(_gcGetNumPortURLs(hPort, out uint num), "GCGetNumPortURLs");
        return num;
    }

    public string GCGetPortURLInfo(IntPtr hPort, uint urlIndex, URL_INFO_CMD cmd)
    {
        if (_gcGetPortURLInfo == null) return "";
        int type = 0;
        long size = 0;
        var err = _gcGetPortURLInfo(hPort, urlIndex, (int)cmd, ref type, IntPtr.Zero, ref size);
        if (err != GC_ERROR.GC_ERR_SUCCESS && err != GC_ERROR.GC_ERR_BUFFER_TOO_SMALL)
            return "";

        IntPtr buf = Marshal.AllocHGlobal((int)size);
        try
        {
            err = _gcGetPortURLInfo(hPort, urlIndex, (int)cmd, ref type, buf, ref size);
            if (err != GC_ERROR.GC_ERR_SUCCESS) return "";
            return Marshal.PtrToStringAnsi(buf) ?? "";
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    // ---- DataStream 级别 ----
    public void DSClose(IntPtr hDataStream)
    {
        Check(_dsClose!(hDataStream), "DSClose");
    }

    public long DSGetInfoLong(IntPtr hDataStream, STREAM_INFO_CMD cmd)
    {
        int type = 0;
        long size = 8;
        IntPtr buf = Marshal.AllocHGlobal(8);
        try
        {
            Check(_dsGetInfo!(hDataStream, (int)cmd, ref type, buf, ref size), $"DSGetInfo({cmd})");
            return Marshal.ReadInt64(buf);
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    public IntPtr DSAllocAndAnnounceBuffer(IntPtr hDataStream, long bufferSize)
    {
        if (_dsAllocAndAnnounceBuffer != null)
        {
            Check(_dsAllocAndAnnounceBuffer(hDataStream, bufferSize, IntPtr.Zero, out IntPtr hBuffer),
                "DSAllocAndAnnounceBuffer");
            return hBuffer;
        }
        else
        {
            // 手动分配
            IntPtr pBuffer = Marshal.AllocHGlobal((int)bufferSize);
            Check(_dsAnnounceBuffer!(hDataStream, pBuffer, bufferSize, IntPtr.Zero, out IntPtr hBuffer),
                "DSAnnounceBuffer");
            return hBuffer;
        }
    }

    public IntPtr DSAnnounceBuffer(IntPtr hDataStream, IntPtr pBuffer, long size)
    {
        Check(_dsAnnounceBuffer!(hDataStream, pBuffer, size, IntPtr.Zero, out IntPtr hBuffer),
            "DSAnnounceBuffer");
        return hBuffer;
    }

    public void DSRevokeBuffer(IntPtr hDataStream, IntPtr hBuffer)
    {
        Check(_dsRevokeBuffer!(hDataStream, hBuffer, out _, out _), "DSRevokeBuffer");
    }

    public void DSQueueBuffer(IntPtr hDataStream, IntPtr hBuffer)
    {
        Check(_dsQueueBuffer!(hDataStream, hBuffer), "DSQueueBuffer");
    }

    public void DSFlushQueue(IntPtr hDataStream, ACQ_QUEUE_TYPE operation)
    {
        Check(_dsFlushQueue!(hDataStream, (uint)operation), "DSFlushQueue");
    }

    public void DSStartAcquisition(IntPtr hDataStream,
        ACQ_START_FLAGS flags = ACQ_START_FLAGS.ACQ_START_FLAGS_DEFAULT, ulong numToAcquire = 0xFFFFFFFFFFFFFFFF)
    {
        Check(_dsStartAcquisition!(hDataStream, (uint)flags, numToAcquire), "DSStartAcquisition");
    }

    public void DSStopAcquisition(IntPtr hDataStream,
        ACQ_STOP_FLAGS flags = ACQ_STOP_FLAGS.ACQ_STOP_FLAGS_DEFAULT)
    {
        Check(_dsStopAcquisition!(hDataStream, (uint)flags), "DSStopAcquisition");
    }

    public BufferInfo DSGetBufferInfo(IntPtr hDataStream, IntPtr hBuffer)
    {
        var info = new BufferInfo();
        info.Base = GetBufferInfoPtr(hDataStream, hBuffer, BUFFER_INFO_CMD.BUFFER_INFO_BASE);
        info.Size = GetBufferInfoLong(hDataStream, hBuffer, BUFFER_INFO_CMD.BUFFER_INFO_SIZE);
        info.SizeFilled = GetBufferInfoLong(hDataStream, hBuffer, BUFFER_INFO_CMD.BUFFER_INFO_SIZE_FILLED);
        info.Width = (int)GetBufferInfoLong(hDataStream, hBuffer, BUFFER_INFO_CMD.BUFFER_INFO_WIDTH);
        info.Height = (int)GetBufferInfoLong(hDataStream, hBuffer, BUFFER_INFO_CMD.BUFFER_INFO_HEIGHT);
        info.FrameId = GetBufferInfoLong(hDataStream, hBuffer, BUFFER_INFO_CMD.BUFFER_INFO_FRAMEID);
        info.Timestamp = GetBufferInfoLong(hDataStream, hBuffer, BUFFER_INFO_CMD.BUFFER_INFO_TIMESTAMP);
        info.PixelFormat = (uint)GetBufferInfoLong(hDataStream, hBuffer, BUFFER_INFO_CMD.BUFFER_INFO_PIXELFORMAT);

        int type = 0;
        long size = 1;
        IntPtr buf = Marshal.AllocHGlobal(8);
        try
        {
            var err = _dsGetBufferInfo!(hDataStream, hBuffer, (int)BUFFER_INFO_CMD.BUFFER_INFO_IS_INCOMPLETE,
                ref type, buf, ref size);
            info.IsIncomplete = err == GC_ERROR.GC_ERR_SUCCESS && Marshal.ReadByte(buf) != 0;
        }
        finally { Marshal.FreeHGlobal(buf); }

        return info;
    }

    private long GetBufferInfoLong(IntPtr hDS, IntPtr hBuf, BUFFER_INFO_CMD cmd)
    {
        int type = 0;
        long size = 8;
        IntPtr buf = Marshal.AllocHGlobal(8);
        try
        {
            var err = _dsGetBufferInfo!(hDS, hBuf, (int)cmd, ref type, buf, ref size);
            if (err != GC_ERROR.GC_ERR_SUCCESS) return 0;
            return Marshal.ReadInt64(buf);
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    private IntPtr GetBufferInfoPtr(IntPtr hDS, IntPtr hBuf, BUFFER_INFO_CMD cmd)
    {
        int type = 0;
        long size = IntPtr.Size;
        IntPtr buf = Marshal.AllocHGlobal(IntPtr.Size);
        try
        {
            var err = _dsGetBufferInfo!(hDS, hBuf, (int)cmd, ref type, buf, ref size);
            if (err != GC_ERROR.GC_ERR_SUCCESS) return IntPtr.Zero;
            return Marshal.ReadIntPtr(buf);
        }
        finally { Marshal.FreeHGlobal(buf); }
    }

    // ---- Event 级别 ----
    public IntPtr GCRegisterEvent(IntPtr hModule, EVENT_TYPE eventType)
    {
        Check(_gcRegisterEvent!(hModule, (uint)eventType, out IntPtr hEvent), "GCRegisterEvent");
        return hEvent;
    }

    public void GCUnregisterEvent(IntPtr hModule, EVENT_TYPE eventType)
    {
        Check(_gcUnregisterEvent!(hModule, (uint)eventType), "GCUnregisterEvent");
    }

    /// <summary>
    /// 等待事件并返回已填充的 Buffer Handle
    /// </summary>
    public bool EventGetData(IntPtr hEvent, out IntPtr hBuffer, ulong timeout = 5000)
    {
        hBuffer = IntPtr.Zero;
        // EVENT_NEW_BUFFER 数据 = { BufferHandle, pUserPointer } = 2 × IntPtr
        // 分配 256 字节留足余量，避免堆溢出
        const int BufSize = 256;
        long size = BufSize;
        IntPtr pBuffer = Marshal.AllocHGlobal(BufSize);
        try
        {
            var err = _eventGetData!(hEvent, pBuffer, ref size, timeout);
            if (err == GC_ERROR.GC_ERR_SUCCESS)
            {
                hBuffer = Marshal.ReadIntPtr(pBuffer);
                return true;
            }
            return false;
        }
        finally { Marshal.FreeHGlobal(pBuffer); }
    }

    public void EventKill(IntPtr hEvent)
    {
        _eventKill?.Invoke(hEvent);
    }

    #endregion

    public void Dispose()
    {
        try { GCCloseLib(); } catch { }
        if (_hModule != IntPtr.Zero)
        {
            FreeLibrary(_hModule);
            _hModule = IntPtr.Zero;
        }
        SetDllDirectoryW("");
    }
}
