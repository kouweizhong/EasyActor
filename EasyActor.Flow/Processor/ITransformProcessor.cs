﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.Flow.Processor
{
    public interface ITransformProcessor<in TMessage1, TMessage2, out TProgress>
    {
        Task<TMessage2> Transform(TMessage1 message, IProgress<TProgress> progress, CancellationToken cancelationToken);
    }
}
