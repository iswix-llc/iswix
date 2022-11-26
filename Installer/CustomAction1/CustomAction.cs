using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomAction1
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CustomAction1(Session session)
        {
            session.Log("Begin CustomAction1");

            return ActionResult.Success;
        }
    }
}
