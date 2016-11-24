using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autofac;
using Mofichan.Core.Interfaces;
using Mofichan.Library.Response;

namespace Mofichan.Library
{
    public class LibraryModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            /*
             * Libraries.
             */
            builder.RegisterInstance(BuildLibrary("greetings"));
            builder.RegisterInstance(BuildLibrary("emotes"));

            builder.RegisterType<ArticleFilter>().As<IArticleFilter>();
            builder.RegisterType<ArticleResolver>().As<IArticleResolver>();
            builder.RegisterType<ResponseBuilder>().As<IResponseBuilder>();
        }

        private static ILibrary BuildLibrary(string resourceName)
        {
            var assembly = typeof(LibraryModule).GetTypeInfo().Assembly;
            var resourcePath = "Mofichan.Library.Resources.ResponseLib." + resourceName + ".json";
            using (var resourceStream = assembly.GetManifestResourceStream(resourcePath))
            {
                Debug.Assert(resourceStream != null, "The resource should exist");

                return new JsonSourceLibrary(new StreamReader(resourceStream));
            }
        }
    }
}
