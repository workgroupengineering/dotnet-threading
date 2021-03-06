﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

﻿#if !NET40PLUS

// The reference source is missing documentation
#pragma warning disable SA1600 // Elements must be documented

namespace System
{
    using System;
    using System.Reflection;

    internal class ExecutionContextLightup : Lightup
    {
        public static readonly ExecutionContextLightup Instance = new ExecutionContextLightup(null);
        private readonly object _executionContext;
        private Delegate _dispose;
        private Delegate _capture;
        private Delegate _run;
        private Delegate _createCopy;

        private ExecutionContextLightup(object executionContext)
            : base(LightupType.ExecutionContext)
        {
            _executionContext = executionContext;
        }

        protected override object GetInstance()
        {
            return _executionContext;
        }

        public ExecutionContextLightup Capture()
        {
            object executionContext;
            if (TryCall(ref _capture, "Capture", out executionContext) && executionContext != null)
            {
                return new ExecutionContextLightup(executionContext);
            }

            return null;
        }

        public ExecutionContextLightup CreateCopy()
        {
            object copy = Call<object>(ref _createCopy, "CreateCopy");

            return new ExecutionContextLightup(copy);
        }

        public void Run(ExecutionContextLightup executionContext, Action<object> callback, object state)
        {
            if (LightupType.ExecutionContext == null || LightupType.ContextCallback == null)
                throw new PlatformNotSupportedException();

            // Replace the Action<object> with a ContextCallback
            Delegate contextCallback = LightupServices.ReplaceWith(callback, LightupType.ContextCallback);

            Type actionRepresentingSignature = typeof(Action<,,>).MakeGenericType(LightupType.ExecutionContext, LightupType.ContextCallback, typeof(object));

            Delegate d = GetMethodAccessor(ref _run, actionRepresentingSignature, "Run");
            d.DynamicInvoke(executionContext._executionContext, contextCallback, state);
        }

        public void Dispose()
        {
            TryCall(ref _dispose, "Dispose");
        }
    }
}

#endif
