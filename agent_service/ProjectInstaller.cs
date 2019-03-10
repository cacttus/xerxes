using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace AgentService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
        protected void GetUserPass(ref string user, ref string pass)
        {
            // ** get u/p from text file.
            string[] lines = System.IO.File.ReadAllLines("installup.txt");
            if (lines.Length != 2)
                throw new Exception("installup.txt file must have only 2 lines user, then password.");

            user = lines[0];
            pass = lines[1];

            user = System.Environment.MachineName + "\\" + user;

            Console.WriteLine("User = '" + user + "' Pass = '" + pass + "'");
        }
        protected void SetUserAccount()
        {
            //push / pop dirs
            string user = string.Empty;
            string pass = string.Empty;
            GetUserPass(ref user, ref pass);

            this.serviceInstaller1.DisplayName = "_Spartan Build Agent";
            this.serviceInstaller1.Description = "Spartan Agent Build Service";

            serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.User;
            serviceProcessInstaller1.Username = user;
            serviceProcessInstaller1.Password = pass;
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            SetUserAccount();

            base.OnBeforeInstall(savedState);
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }
        protected void OtherCrap()
        {
            //string assemblyPath = Context.Parameters["assemblypath"];
            //this.serviceInstaller1.
            //string kvp = string.Empty;
            //foreach (string key in Context.Parameters.Keys)
            //    kvp += key + "=" + Context.Parameters[key] + "\n";

            //if (!Context.Parameters.ContainsKey("args"))
            //    throw new Exception("There were no args. Please use /args=..(,'%) spaces must be commas, double quotes are ' single qyuo, and spaces in string parameters are % signs, without double quotes around them, and without spaces except for single quoted args, for the spartan service. Do not use nested quotes.\nParams:\n" + kvp);

            //string args = Context.Parameters["args"];
            //string[] argArray = args.Split(',');
            //args = "";
            //foreach (string arg in argArray)
            //{
            //    string arg2 = arg.Replace('\'', '\"');
            //    arg2 = arg2.Replace('%', ' ');
            //    args += arg2 + " ";
            //}

            //assemblyPath = "\"" + assemblyPath + "\" " + args + "";

            //Context.Parameters["assemblypath"] = assemblyPath;

        }
    }
}
