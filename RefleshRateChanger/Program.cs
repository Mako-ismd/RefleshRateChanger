using System;
using System.Runtime.InteropServices;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Put the target refresh rate in the argument");
            Console.ReadKey();
            return;
        }
        var changer = new DisplayChanger();
        var targetRate = int.Parse(args[0]);
        if (targetRate <= changer.GetMaxRefreshRate()) changer.ChangeRefreshRate(targetRate);
    }
}

public class DisplayChanger
{
    [DllImport("user32.dll")]
    public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;

        public int dmPositionX;
        public int dmPositionY;
        public ScreenOrientation dmDisplayOrientation;
        public int dmDisplayFixedOutput;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    public enum ScreenOrientation : int
    {
        DMDO_DEFAULT = 0,
        DMDO_90 = 1,
        DMDO_180 = 2,
        DMDO_270 = 3
    }

    public void ChangeRefreshRate(int refreshRate)
    {
        DEVMODE devMode = new DEVMODE();
        devMode.dmSize = (short)Marshal.SizeOf(devMode);

        if (0 == EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode))
        {
            throw new InvalidOperationException("Cannot retrieve current display settings.");
        }

        devMode.dmDisplayFrequency = refreshRate;

        int result = ChangeDisplaySettings(ref devMode, CDS_TEST);

        if (result != DISP_CHANGE_SUCCESSFUL)
        {
            throw new InvalidOperationException("Cannot change display settings.");
        }

        ChangeDisplaySettings(ref devMode, CDS_UPDATEREGISTRY);
    }

    public int GetMaxRefreshRate()
    {
        DEVMODE devMode = new DEVMODE();
        devMode.dmSize = (short)Marshal.SizeOf(devMode);

        int modeNum = 0;
        int maxRefreshRate = 0;

        while (EnumDisplaySettings(null, modeNum, ref devMode) != 0)
        {
            if (devMode.dmDisplayFrequency > maxRefreshRate)
            {
                maxRefreshRate = devMode.dmDisplayFrequency;
            }
            modeNum++;
        }

        return maxRefreshRate;
    }


    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int CDS_TEST = 2;
    private const int CDS_UPDATEREGISTRY = 1;
    private const int DISP_CHANGE_SUCCESSFUL = 0;

    [DllImport("user32.dll")]
    private static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
}