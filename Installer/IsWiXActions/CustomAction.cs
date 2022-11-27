using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsWiXActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult FixPaths(Session session)
        {
            session["VS2022_IDE_DIR"] = session["VS2022_IDE_DIR"].Replace(@"C:\Program Files\", @"C:\Progra~1\");
            session["VS2022_EXTENSIONS_DIR"] = session["VS2022_EXTENSIONS_DIR"].Replace(@"C:\Program Files\", @"C:\Progra~1\");
            return ActionResult.Success;
        }
    }
}
