﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Rebus.Extensions;
using Rebus.Injection;
using Rebus.Logging;
using Rebus.Pipeline;

namespace Rebus.Config
{
    /// <summary>
    /// Allows for configuring additional options
    /// </summary>
    public class OptionsConfigurer
    {
        readonly Options _options;
        readonly Injectionist _injectionist;

        internal OptionsConfigurer(Options options, Injectionist injectionist)
        {
            _options = options;
            _injectionist = injectionist;
        }

        /// <summary>
        /// Configures the number of workers to start competing over the input queue
        /// </summary>
        public OptionsConfigurer SetNumberOfWorkers(int numberOfWorkers)
        {
            _options.NumberOfWorkers = numberOfWorkers;
            return this;
        }

        /// <summary>
        /// Configures the degree of parallelism allowed within each worker
        /// </summary>
        public OptionsConfigurer SetMaxParallelism(int maxParallelism)
        {
            _options.MaxParallelism = maxParallelism;
            return this;
        }

        /// <summary>
        /// Registers the given factory function as a resolve of the given <see cref="TService"/> service
        /// </summary>
        public void Register<TService>(Func<IResolutionContext, TService> resolverMethod)
        {
            _injectionist.Register(resolverMethod);
        }

        /// <summary>
        /// Registers the given factory function as a resolve of the given <see cref="TService"/> service
        /// </summary>
        public void Decorate<TService>(Func<IResolutionContext, TService> factoryMethod)
        {
            _injectionist.Register(factoryMethod, isDecorator: true);
        }

        /// <summary>
        /// Outputs the layout of the send and receive pipelines to the log
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public OptionsConfigurer LogPipeline(bool verbose = false)
        {
            var logger = RebusLoggerFactory.Current.GetCurrentClassLogger();

            Decorate(c =>
            {
                var pipeline = c.Get<IPipeline>();

                var receivePipeline = pipeline.ReceivePipeline();
                var sendPipeline = pipeline.SendPipeline();

                logger.Info(@"
------------------------------------------------------------------------------
Message pipelines
------------------------------------------------------------------------------
Receive pipeline:
{0}

Send pipeline:
{1}
------------------------------------------------------------------------------
", Format(receivePipeline, verbose), Format(sendPipeline, verbose));


                return pipeline;
            });

            return this;
        }

        string Format(IEnumerable<IStep> pipeline, bool verbose)
        {
            return string.Join(Environment.NewLine,
                pipeline.Select((step, i) =>
                {
                    var stepType = step.GetType().FullName;
                    var stepString = string.Format("    {0}", stepType);

                    if (verbose)
                    {
                        var docs = GetDocsOrNull(step);

                        if (!string.IsNullOrWhiteSpace(docs))
                        {
                            stepString = string.Concat(stepString, Environment.NewLine, docs.WrappedAt(60).Indented(8),
                                Environment.NewLine);
                        }

                    }

                    return stepString;
                }));
        }

        string GetDocsOrNull(IStep step)
        {
            var docsAttribute = step.GetType()
                .GetCustomAttributes()
                .OfType<StepDocumentationAttribute>()
                .FirstOrDefault();

            return docsAttribute != null
                ? docsAttribute.Text
                : null;
        }
    }
}