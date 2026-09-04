using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using OpenQA.Selenium;
using System;

namespace Selenium.BaseComponents
{
    // Context : Overriding Retry Attribute to check and try until success the test case.
    // Author : Vishnu vardhan

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CustomRetry : PropertyAttribute, IWrapSetUpTearDown
    {
        private int _count;
        public CustomRetry(int count) : base(count)
        {
            _count = count;
        }
        public TestCommand Wrap(TestCommand command)
        {
            return new RetryCommand(command, _count);
        }
        public class RetryCommand : DelegatingTestCommand
        {
            private int _retryCount;
            public RetryCommand(TestCommand innerCommand, int retryCount)
                          : base(innerCommand)
            {
                _retryCount = retryCount;
            }
            public override TestResult Execute(TestExecutionContext context)
            {
                for (int count = _retryCount; count-- > 0;)
                {
                    try
                    {
                        context.CurrentResult = innerCommand.Execute(context);
                    }
                    catch (WebDriverTimeoutException ex)
                    {
                        if (count == 0)
                            throw;

                        continue;
                    }

                    if (context.CurrentResult.AssertionResults != null && context.CurrentResult.AssertionResults.Count > 0)
                        break;

                    if (context.CurrentResult.ResultState.Status != ResultState.Failure.Status)
                        break;

                    if (count > 0)
                        context.CurrentResult = context.CurrentTest.MakeTestResult();
                }

                return context.CurrentResult;
            }
        }

    }

}
