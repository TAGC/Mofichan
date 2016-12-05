using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autofac;
using Mofichan.Core.Interfaces;

namespace Mofichan.DataAccess.Response
{
    /// <summary>
    /// An Autofac module that registers classes to aid in response generation.
    /// </summary>
    /// <seealso cref="Autofac.Module" />
    public class ResponseModule : Autofac.Module
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(BuildLibrary("greetings"));
            builder.RegisterInstance(BuildLibrary("emotes"));

            builder.RegisterType<ArticleFilter>().As<IArticleFilter>();
            builder.RegisterType<ArticleResolver>().As<IArticleResolver>();
            builder.RegisterType<ResponseBuilder>().As<IResponseBuilder>();
        }

        private static ILibrary BuildLibrary(string resourceName)
        {
            var assembly = typeof(ResponseModule).GetTypeInfo().Assembly;
            var resourcePath = string.Format("Mofichan.DataAccess.Response.Resources.{0}.json", resourceName);

            using (var resourceStream = assembly.GetManifestResourceStream(resourcePath))
            {
                Debug.Assert(resourceStream != null, "The resource should exist");

                return new JsonSourceLibrary(new StreamReader(resourceStream));
            }
        }
    }
}
