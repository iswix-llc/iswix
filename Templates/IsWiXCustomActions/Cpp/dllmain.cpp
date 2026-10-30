#include "pch.h"

// Sets up and tears down the WiX custom action library for the DLL.
extern "C" BOOL WINAPI DllMain(
    __in HINSTANCE hInstance,
    __in ULONG ulReason,
    __in LPVOID /*pvReserved*/
    )
{
    switch (ulReason)
    {
    case DLL_PROCESS_ATTACH:
        WcaGlobalInitialize(hInstance);
        break;

    case DLL_PROCESS_DETACH:
        WcaGlobalFinalize();
        break;
    }

    return TRUE;
}
