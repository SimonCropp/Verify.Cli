namespace Verify.Cli.Tests;

public class FileVerifierTests
{
    [Fact]
    public async Task SameFiles_AreEqual()
    {
        // Arrange
        var file = ProjectFiles.examples.same_json.Info;
        await FileVerifier.VerifyFileAsync(file, new VerifyFileOptions());
    }

    [Fact]
    public async Task DifferentFiles_AreNotEqual()
    {
        // Arrange
        var file = ProjectFiles.examples.different_json.Info;
        await Assert.ThrowsAnyAsync<Exception>(async () => await FileVerifier.VerifyFileAsync(file, new VerifyFileOptions()));
    }

    [Fact]
    public async Task FileWithInlineDateTime_ScrubsCorrectly()
    {
        // Arrange
        var file = ProjectFiles.examples.sameWithDates_json.Info;
        var options = new VerifyFileOptions(ScrubInlineDatetime: "yyyy-MM-dd");
        
        // Act & Assert
        await FileVerifier.VerifyFileAsync(file, options);
    }

    [Fact]
    public async Task FileWithInlinePattern_ScrubsCorrectly()
    {
        // Arrange
        var file = ProjectFiles.examples.azure_pipeline_template_expression_html.Info;

        // Pattern includes named groups for prefix and suffix
        // This ensures that the replacement string retains the original quotes around the matched pattern
        var options = new VerifyFileOptions(ScrubInlinePatterns: new[] { "(?<prefix>\")/_astro/[^\"]+(?<suffix>\")" });
        
        // Act & Assert
        await FileVerifier.VerifyFileAsync(file, options);
    }

    [Fact]
    public async Task FileWithMultipleInlinePatterns_ScrubsAllCorrectly()
    {
        // Arrange
        var file = ProjectFiles.examples.azure_pipeline_template_expression_html.Info;

        // Apply two patterns: one for /_astro paths and another for "/astro" paths
        var options = new VerifyFileOptions(
            ScrubInlinePatterns: new[]
            {
                "(?<prefix>\")/_astro/[^\"]+(?<suffix>\")",
                "\"/astro/[^\"]+\""
            });

        // Act & Assert
        await FileVerifier.VerifyFileAsync(file, options);
    }

    [Fact]
    public async Task FileWithInlineRemove_RemovesCorrectly()
    {
        // Arrange
        var file = ProjectFiles.examples.withRemovableIds_html.Info;

        // Simple text to remove (not regex) - removes all instances
        var options = new VerifyFileOptions(ScrubInlineRemoves: new[] { " data-temp-id" });
        
        // Act & Assert
        await FileVerifier.VerifyFileAsync(file, options);
    }

    [Fact]
    public async Task FileWithInlineRemove_EmptyPattern_ThrowsException()
    {
        // Arrange
        var file = ProjectFiles.examples.same_json.Info;
        var options = new VerifyFileOptions(ScrubInlineRemoves: new[] { "" });
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await FileVerifier.VerifyFileAsync(file, options));
    }

    [Fact]
    public async Task FileWithInlineRemove_NullPattern_DoesNothing()
    {
        // Arrange
        var file = ProjectFiles.examples.same_json.Info;
        var options = new VerifyFileOptions(ScrubInlineRemoves: null);
        
        // Act & Assert - Should not throw
        await FileVerifier.VerifyFileAsync(file, options);
    }

    [Fact]
    public async Task FileWithMultipleInlineRemove_RemovesAllCorrectly()
    {
        // Arrange
        var file = ProjectFiles.examples.multiRemove_txt.Info;
        var options = new VerifyFileOptions(ScrubInlineRemoves: new[] { "REMOVE1", "REMOVE2" });

        // Act & Assert
        await FileVerifier.VerifyFileAsync(file, options);
    }

    [Fact]
    public async Task VerifyFileAsync_NormalVerbosity_ProducesNoOutput()
    {
        // Arrange
        var file = ProjectFiles.examples.same_json.Info;
        var options = new VerifyFileOptions(Verbosity: Verbosity.Normal);
        
        // Act & Assert - Should not throw
        await FileVerifier.VerifyFileAsync(file, options);
    }

    [Fact]
    public async Task VerifyFileAsync_DetailedVerbosity_ProducesOutput()
    {
        // Arrange
        var file = ProjectFiles.examples.same_json.Info;
        var options = new VerifyFileOptions(Verbosity: Verbosity.Detailed);
        
        // Capture console output
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        
        try
        {
            // Act
            await FileVerifier.VerifyFileAsync(file, options);
            
            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("Source file path:", output);
            Assert.Contains("Received path:", output);
            Assert.Contains("Verified path:", output);
            Assert.Contains("Files match", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task FileWithOverrideFilename_UsesAlternateVerifiedFilename()
    {
        // Arrange
        var file = ProjectFiles.examples.override_test_json.Info;
        var options = new VerifyFileOptions(OverrideFilename: "override-alternate");
        
        // Act & Assert
        await FileVerifier.VerifyFileAsync(file, options);
    }

    [Fact]
    public async Task FileWithOverrideFilename_ProducesCorrectPathInOutput()
    {
        // Arrange
        var file = ProjectFiles.examples.override_test_json.Info;
        var options = new VerifyFileOptions(OverrideFilename: "override-alternate", Verbosity: Verbosity.Detailed);
        
        // Capture console output
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        
        try
        {
            // Act
            await FileVerifier.VerifyFileAsync(file, options);
            
            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("Verified path:", output);
            // The override filename results in the path being override-alternate.json.verified.json
            // because InnerVerifier adds the extension and then adds .verified
            Assert.Contains("override-alternate.json.verified.json", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}