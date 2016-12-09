﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.DataAccess.Analysis
{
    /// <summary>
    /// An Autofac module that registers classes to aid in message analysis.
    /// </summary>
    /// <seealso cref="Autofac.Module" />
    public class AnalysisModule : Autofac.Module
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
            var analysisLibraries = new[]
            {
                BuildLibrary("greetings"),
                BuildLibrary("emotive")
            };

            Action<MessageClassifier> trainClassifier = classifier =>
            {
                double requiredConfidenceRatio = 0.5;
                classifier.Train(analysisLibraries.SelectMany(it => it.Articles), requiredConfidenceRatio);
            };

            builder.RegisterType<MessageClassifier>()
                .OnActivated(e => trainClassifier(e.Instance))
                .Named<IMessageClassifier>("classifier")
                .SingleInstance();

            builder.RegisterDecorator<IMessageClassifier>(
                (c, inner) => new SentenceFragmentAnalyser(inner, c.Resolve<ILogger>()),
                fromKey: "classifier");
        }

        private static ILibrary BuildLibrary(string resourceName)
        {
            var assembly = typeof(AnalysisModule).GetTypeInfo().Assembly;
            var resourcePath = string.Format("Mofichan.DataAccess.Analysis.Resources.{0}.json", resourceName);

            using (var resourceStream = assembly.GetManifestResourceStream(resourcePath))
            {
                Debug.Assert(resourceStream != null, "The resource should exist");

                return new JsonSourceLibrary(new StreamReader(resourceStream));
            }
        }
    }
}