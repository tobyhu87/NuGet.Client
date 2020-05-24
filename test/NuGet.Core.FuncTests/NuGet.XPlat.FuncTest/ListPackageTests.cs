// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Moq;
using NuGet.CommandLine.XPlat;
using NuGet.Protocol;
using NuGet.Test.Utility;
using Xunit;

namespace NuGet.XPlat.FuncTest
{
    [Collection("NuGet XPlat Test Collection")]
    public class ListPackageTests
    {
        [Fact]
        public void BasicListPackageParsing_Interactive()
        {
            VerifyCommand(
                (projectPath, mockCommandRunner, testApp) =>
                {
                    // Arrange
                    var argList = new List<string> { "list", "--interactive", projectPath };

                    // Act
                    var result = testApp.Execute(argList.ToArray());

                    // Assert
                    mockCommandRunner.Verify();
                    Assert.NotNull(HttpHandlerResourceV3.CredentialService);
                    Assert.Equal(0, result);
                });
        }

        [Fact]
        public void BasicListPackageParsing_InteractiveTakesNoArguments()
        {
            VerifyCommand(
                (projectPath, mockCommandRunner, testApp) =>
                {
                    // Arrange
                    var argList = new List<string>() { "list", "--interactive", "no", projectPath };

                    // Act & Assert
                    Assert.Throws<CommandParsingException>(() => testApp.Execute(argList.ToArray()));
                });
        }

        [Fact]
        public void BasicListPackageParsing_ShowProtocolLogs()
        {
            VerifyCommand(
                (projectPath, mockCommandRunner, testApp) =>
                {
                    // Arrange
                    var argList = new List<string> { "list", projectPath, "--show-protocol-logs" };

                    // Act
                    var result = testApp.Execute(argList.ToArray());

                    // Assert
                    mockCommandRunner.Verify(
                        x => x.ExecuteCommandAsync(It.Is<ListPackageArgs>(a => a.ShowProtocolLogs)),
                        Times.Once);
                    Assert.Equal(0, result);
                });
        }

        [Fact]
        public void BasicListPackageParsing_NoShowProtocolLogs()
        {
            VerifyCommand((projectPath, mockCommandRunner, testApp) =>
                {
                    // Arrange
                    var argList = new List<string> { "list", projectPath };

                    // Act
                    var result = testApp.Execute(argList.ToArray());

                    // Assert
                    mockCommandRunner.Verify(
                        x => x.ExecuteCommandAsync(It.Is<ListPackageArgs>(a => !a.ShowProtocolLogs)),
                        Times.Once);
                    Assert.Equal(0, result);
                });
        }

        private void VerifyCommand(Action<string, Mock<IListPackageCommandRunner>, CommandLineApplication> verify)
        {
            // Arrange
            using (var testDirectory = TestDirectory.Create())
            {
                var projectPath = Path.Combine(testDirectory, "project.csproj");
                File.WriteAllText(projectPath, string.Empty);

                var logger = new TestCommandOutputLogger();
                var testApp = new CommandLineApplication();
                var mockCommandRunner = new Mock<IListPackageCommandRunner>();
                mockCommandRunner
                    .Setup(m => m.ExecuteCommandAsync(It.IsAny<ListPackageArgs>()))
                    .Returns(Task.CompletedTask);

                testApp.Name = "dotnet nuget_test";
                ListPackageCommand.Register(testApp,
                    () => logger,
                    () => mockCommandRunner.Object);

                // Act & Assert
                try
                {
                    verify(projectPath, mockCommandRunner, testApp);
                }
                finally
                {
                    XPlatTestUtils.DisposeTemporaryFile(projectPath);
                }
            }
        }
    }
}
