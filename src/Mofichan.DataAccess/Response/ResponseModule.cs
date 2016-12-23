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
            builder.Register(context =>
            {
                var articles = context.Resolve<IQueryableMemoryManager>().LoadResponses();
                return new ArticleFilter(articles);
            }).As<IArticleFilter>();

            builder.RegisterType<ArticleResolver>().As<IArticleResolver>();
            builder.RegisterType<ResponseBodyBuilder>().As<IResponseBodyBuilder>();
        }
    }
}
