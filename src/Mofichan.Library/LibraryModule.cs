using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Mofichan.Core.Interfaces;
using Mofichan.Library.Analysis;
using Mofichan.Library.Response;
using Serilog;

namespace Mofichan.Library
{
    public class LibraryModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            /*
             * Collect analysis libraries to train Mofi with.
             */
            var analysisLibraries = new[]
            {
                BuildAnalysisLibrary("greetings"),
                BuildAnalysisLibrary("emotive")
            };

            double requiredConfidenceRatio = 0.5;
            builder.Register(c =>
            {
                var messageClassifier = new MessageClassifier(c.Resolve<ILogger>());
                messageClassifier.Train(analysisLibraries.SelectMany(it => it.Articles), requiredConfidenceRatio);

                return messageClassifier;
            }).As<IMessageClassifier>().SingleInstance();

            /*
             * Register response libraries directly - used by ResponseBuilder.
             */
            builder.RegisterInstance(BuildResponseLibrary("greetings"));
            builder.RegisterInstance(BuildResponseLibrary("emotes"));

            builder.RegisterType<ArticleFilter>().As<IArticleFilter>();
            builder.RegisterType<ArticleResolver>().As<IArticleResolver>();
            builder.RegisterType<ResponseBuilder>().As<IResponseBuilder>();
        }

        private static ILibrary BuildResponseLibrary(string resourceName)
        {
            return BuildLibrary("ResponseLib", resourceName);
        }

        private static ILibrary BuildAnalysisLibrary(string resourceName)
        {
            return BuildLibrary("AnalysisLib", resourceName);
        }

        private static ILibrary BuildLibrary(string resourceDir, string resourceName)
        {
            var assembly = typeof(LibraryModule).GetTypeInfo().Assembly;
            var resourcePath = string.Format("Mofichan.Library.Resources.{0}.{1}.json",
                resourceDir, resourceName);

            using (var resourceStream = assembly.GetManifestResourceStream(resourcePath))
            {
                Debug.Assert(resourceStream != null, "The resource should exist");

                return new JsonSourceLibrary(new StreamReader(resourceStream));
            }
        }
    }
}
