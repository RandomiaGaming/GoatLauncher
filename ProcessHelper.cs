using System.Runtime.InteropServices;
using System;

namespace GoatLauncher
{
    public static class ProcessHelper
    {
        #region Private PInvoke Bindings
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryFullProcessImageName(IntPtr processHandle, int flags, System.Text.StringBuilder stringBuilder, ref int length);
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumProcesses([Out] IntPtr processIds, uint arraySizeBytes, [MarshalAs(UnmanagedType.U4)] out uint bytesCopied);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint accessFlags, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr processHandle);
        #endregion
        public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        public static bool IsGoatSimulatorOpen()
        {
            uint processIdsByteSize = 2096;
            IntPtr processIdsPtr = Marshal.AllocHGlobal((int)processIdsByteSize);
            uint bytesCopied;

            EnumProcesses(processIdsPtr, processIdsByteSize, out bytesCopied);

            uint[] processIds = new uint[(int)(bytesCopied / sizeof(uint))];
            byte[] processIdsBytes = new byte[processIds.Length * sizeof(uint)];
            Marshal.Copy(processIdsPtr, processIdsBytes, 0, processIdsBytes.Length);
            for (int i = 0; i < processIds.Length; i++)
            {
                processIds[i] = processIdsBytes[i * sizeof(uint)];
                processIds[i] |= (uint)processIdsBytes[(i * sizeof(uint)) + 1] << (8 * 1);
                processIds[i] |= (uint)processIdsBytes[(i * sizeof(uint)) + 2] << (8 * 2);
                processIds[i] |= (uint)processIdsBytes[(i * sizeof(uint)) + 3] << (8 * 3);
            }

            bool goatSimulatorFound = false;

            for (int i = 0; i < processIds.Length; i++)
            {
                IntPtr processHandle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, (int)processIds[i]);

                if (processHandle != IntPtr.Zero)
                {
                    int pathBufferSize = 1024;

                    System.Text.StringBuilder pathStringBuilder = new System.Text.StringBuilder(pathBufferSize);

                    QueryFullProcessImageName(processHandle, 0, pathStringBuilder, ref pathBufferSize);

                    string path = pathStringBuilder.ToString();

                    CloseHandle(processHandle);

                    if (path.EndsWith("Goat2.exe") || path.EndsWith("Goat2-Win64-Shipping.exe") || path.EndsWith("Goat2-Win32-Shipping.exe"))
                    {
                        goatSimulatorFound = true;
                        break;
                    }
                }
            }

            Marshal.FreeHGlobal(processIdsPtr);

            return goatSimulatorFound;
        }
    }
}