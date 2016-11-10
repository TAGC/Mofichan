using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mofichan.Core;
using Serilog;
using Specify.Configuration;
using TinyIoC;

namespace Mofichan.Spec
{
    /// <summary>
    /// The startup class to configure Specify with the default TinyIoc container. 
    /// Make any changes to the default configuration settings in this file.
    /// </summary>
    public class SpecifyBootstrapper : DefaultBootstrapper
    {
        public SpecifyBootstrapper()
        {
            LoggingEnabled = true;
            HtmlReport.ReportHeader = "Mofichan Specifications";
            HtmlReport.ReportDescription = "Tests to make sure that Mofichan behaves properly";
            HtmlReport.OutputPath = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .CreateLogger();
        }

        /// <summary>
        /// Register any additional items into the TinyIoc container or leave it as it is. 
        /// </summary>
        /// <param name="container">The <see cref="TinyIoC.TinyIoCContainer"/> container.</param>
        public override void ConfigureContainer(TinyIoCContainer container)
        {
            container.Register((c, _) => new Kernel(1));
        }
    }
}
