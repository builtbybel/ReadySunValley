using System;
using System.Runtime.InteropServices;

namespace ReadySunValley.Assessment
{
    // Detecting DirectX wrapping COM objects
    [Guid("7D0F462F-4064-4862-BC7F-933E5058C10F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDxDiagContainer
    {
        void EnumChildContainerNames(uint dwIndex, string pwszContainer, uint cchContainer);

        void EnumPropNames(uint dwIndex, string pwszPropName, uint cchPropName);

        void GetChildContainer(string pwszContainer, out IDxDiagContainer ppInstance);

        void GetNumberOfChildContainers(out uint pdwCount);

        void GetNumberOfProps(out uint pdwCount);

        void GetProp(string pwszPropName, out object pvarProp);
    }

    [ComImport]
    [Guid("A65B8071-3BFE-4213-9A5B-491DA4461CA7")]
    public class DxDiagProvider { }

    [Guid("9C6B4CB0-23F8-49CC-A3ED-45A55000A6D2")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDxDiagProvider
    {
        void Initialize(ref DXDIAG_INIT_PARAMS pParams);

        void GetRootContainer(out IDxDiagContainer ppInstance);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DXDIAG_INIT_PARAMS
    {
        public int dwSize;
        public uint dwDxDiagHeaderVersion;
        public bool bAllowWHQLChecks;
        public IntPtr pReserved;
    };

    public static class DirectX
    {
    }
}