#include "pch.h"

// Schedule it in the installer with a CustomAction element whose DllEntry is CustomAction1, for example:
//   <Binary Id="CustomActionNameBinary" SourceFile="$(var.CustomActionName.TargetPath)" />
//   <CustomAction Id="CustomAction1" BinaryRef="CustomActionNameBinary" DllEntry="CustomAction1" Execute="immediate" Return="check" />
extern "C" UINT __stdcall CustomAction1(
    __in MSIHANDLE hInstall
    )
{
    HRESULT hr = S_OK;
    DWORD er = ERROR_SUCCESS;

    hr = WcaInitialize(hInstall, "CustomAction1");
    ExitOnFailure(hr, "Failed to initialize");

    WcaLog(LOGMSG_STANDARD, "Initialized.");

    // TODO: Add the custom action's work here.

LExit:
    er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
    return WcaFinalize(er);
}
