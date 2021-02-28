﻿using CliFx.Analyzers.Tests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ParameterValidatorsMustBeValidAnalyzerTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ParameterValidatorsMustBeValidAnalyzer();

        [Fact]
        public void Analyzer_reports_an_error_if_one_of_the_specified_parameter_validators_does_not_derive_from_ArgumentValueValidator()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyValidator
{
    public ValidationResult Validate(string value) => ValidationResult.Ok();
}

[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0, Validators = new[] {typeof(MyValidator)})]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().ProduceDiagnostics(code);
        }

        [Fact]
        public void Analyzer_does_not_report_an_error_if_all_specified_parameter_validators_derive_from_ArgumentValueValidator()
        {
            // Arrange
            // language=cs
            const string code = @"
public class MyValidator : ArgumentValueValidator<string>
{
    public ValidationResult Validate(string value) => ValidationResult.Ok();
}

[Command]
public class MyCommand : ICommand
{
    [CommandParameter(0, Validators = new[] {typeof(MyValidator)})]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}";

            // Act & assert
            Analyzer.Should().NotProduceDiagnostics(code);
        }
    }
}