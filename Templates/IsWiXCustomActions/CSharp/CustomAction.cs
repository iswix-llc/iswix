using WixToolset.Dtf.WindowsInstaller;

namespace CustomActionName
{
    public class CustomActions
    {
        // Schedule it in the installer with a CustomAction element whose DllEntry is CustomAction1, for example:
        //   <Binary Id="CustomActionNameBinary" SourceFile="$(var.CustomActionName.TargetDir)CustomActionName.CA.dll" />
        //   <CustomAction Id="CustomAction1" BinaryRef="CustomActionNameBinary" DllEntry="CustomAction1" Execute="immediate" Return="check" />
        [CustomAction]
        public static ActionResult CustomAction1(Session session)
        {
            session.Log("Begin CustomAction1");

            return ActionResult.Success;
        }
    }
}
