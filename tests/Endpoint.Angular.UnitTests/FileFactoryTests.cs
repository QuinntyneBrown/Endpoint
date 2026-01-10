// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Angular.UnitTests;

public class FileFactoryTests
{
    private readonly Mock<ILogger<FileFactory>> _mockLogger;
    private readonly MockFileSystem _mockFileSystem;
    private readonly FileFactory _sut;

    public FileFactoryTests()
    {
        _mockLogger = new Mock<ILogger<FileFactory>>();
        _mockFileSystem = new MockFileSystem();
        _sut = new FileFactory(_mockLogger.Object, _mockFileSystem);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileFactory(null!, _mockFileSystem));
    }

    [Fact]
    public void Constructor_WithNullFileSystem_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileFactory(_mockLogger.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new FileFactory(_mockLogger.Object, _mockFileSystem));

        // Assert
        Assert.Null(exception);
    }

    #endregion

    #region IndexCreate TypeScript Tests

    [Fact]
    public async Task IndexCreate_WithTypeScriptFiles_ShouldGenerateExportStatements()
    {
        // Arrange
        var directory = "/src/app/models";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/user.model.ts", new MockFileData("export class User { }"));
        _mockFileSystem.AddFile($"{directory}/product.model.ts", new MockFileData("export class Product { }"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.Equal("index", result[0].Name);
        Assert.Equal(".ts", result[0].Extension);
        Assert.Contains("export * from './user.model';", result[0].Body);
        Assert.Contains("export * from './product.model';", result[0].Body);
    }

    [Fact]
    public async Task IndexCreate_WithTypeScriptFiles_ShouldExcludeSpecFiles()
    {
        // Arrange
        var directory = "/src/app/services";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/auth.service.ts", new MockFileData("export class AuthService { }"));
        _mockFileSystem.AddFile($"{directory}/auth.service.spec.ts", new MockFileData("describe('AuthService', () => { });"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.Contains("export * from './auth.service';", result[0].Body);
        Assert.DoesNotContain("spec", result[0].Body);
    }

    [Fact]
    public async Task IndexCreate_WithTypeScriptFiles_ShouldExcludeIndexFile()
    {
        // Arrange
        var directory = "/src/app/components";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/button.component.ts", new MockFileData("export class Button { }"));
        _mockFileSystem.AddFile($"{directory}/index.ts", new MockFileData("// existing index"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.Contains("export * from './button.component';", result[0].Body);
        Assert.DoesNotContain("export * from './index';", result[0].Body);
    }

    [Fact]
    public async Task IndexCreate_WithSubdirectoriesContainingIndex_ShouldExportFromSubdirectories()
    {
        // Arrange
        var directory = "/src/app/features";
        var subdirectory = $"{directory}/auth";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddDirectory(subdirectory);
        _mockFileSystem.AddFile($"{subdirectory}/index.ts", new MockFileData("export * from './login';"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.Contains("export * from './auth';", result[0].Body);
    }

    [Fact]
    public async Task IndexCreate_WithSubdirectoriesWithoutIndex_ShouldNotExportFromSubdirectories()
    {
        // Arrange
        var directory = "/src/app/features";
        var subdirectory = $"{directory}/dashboard";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddDirectory(subdirectory);
        _mockFileSystem.AddFile($"{subdirectory}/dashboard.component.ts", new MockFileData("export class Dashboard { }"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.DoesNotContain("export * from './dashboard';", result[0].Body);
    }

    [Fact]
    public async Task IndexCreate_TypeScript_ShouldSetCorrectDirectory()
    {
        // Arrange
        var directory = "/src/lib";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/utils.ts", new MockFileData("export const utils = {};"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.Equal(directory, result[0].Directory);
    }

    #endregion

    #region IndexCreate SCSS Tests

    [Fact]
    public async Task IndexCreate_WithScssFiles_ShouldGenerateUseStatements()
    {
        // Arrange
        var directory = "/src/styles";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/variables.scss", new MockFileData("$primary: blue;"));
        _mockFileSystem.AddFile($"{directory}/mixins.scss", new MockFileData("@mixin flex { }"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: true);

        // Assert
        Assert.Single(result);
        Assert.Equal("index", result[0].Name);
        Assert.Equal(".scss", result[0].Extension);
        Assert.Contains("@use './variables';", result[0].Body);
        Assert.Contains("@use './mixins';", result[0].Body);
    }

    [Fact]
    public async Task IndexCreate_WithScssFiles_ShouldExcludeIndexScss()
    {
        // Arrange
        var directory = "/src/scss";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/base.scss", new MockFileData("body { }"));
        _mockFileSystem.AddFile($"{directory}/index.scss", new MockFileData("// existing index"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: true);

        // Assert
        Assert.Single(result);
        Assert.Contains("@use './base';", result[0].Body);
        Assert.DoesNotContain("@use './index';", result[0].Body);
    }

    [Fact]
    public async Task IndexCreate_Scss_ShouldSetCorrectExtension()
    {
        // Arrange
        var directory = "/src/styles";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/theme.scss", new MockFileData("// theme"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: true);

        // Assert
        Assert.Single(result);
        Assert.Equal(".scss", result[0].Extension);
    }

    [Fact]
    public async Task IndexCreate_Scss_ShouldSetCorrectDirectory()
    {
        // Arrange
        var directory = "/src/assets/scss";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/colors.scss", new MockFileData("// colors"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: true);

        // Assert
        Assert.Single(result);
        Assert.Equal(directory, result[0].Directory);
    }

    #endregion

    #region Empty Directory Tests

    [Fact]
    public async Task IndexCreate_WithEmptyDirectory_TypeScript_ShouldReturnEmptyBody()
    {
        // Arrange
        var directory = "/src/empty";
        _mockFileSystem.AddDirectory(directory);

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.Equal(string.Empty, result[0].Body);
    }

    [Fact]
    public async Task IndexCreate_WithEmptyDirectory_Scss_ShouldReturnEmptyBody()
    {
        // Arrange
        var directory = "/src/empty-scss";
        _mockFileSystem.AddDirectory(directory);

        // Act
        var result = await _sut.IndexCreate(directory, scss: true);

        // Assert
        Assert.Single(result);
        Assert.Equal(string.Empty, result[0].Body);
    }

    [Fact]
    public async Task IndexCreate_WithNoSubdirectories_ShouldStillWork()
    {
        // Arrange
        var directory = "/src/flat";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/helper.ts", new MockFileData("export const helper = {};"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.Contains("export * from './helper';", result[0].Body);
    }

    #endregion

    #region FileModel Property Tests

    [Fact]
    public async Task IndexCreate_ShouldReturnFileModelWithCorrectName()
    {
        // Arrange
        var directory = "/src/app";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/app.module.ts", new MockFileData("// module"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.Equal("index", result[0].Name);
    }

    [Fact]
    public async Task IndexCreate_ShouldReturnListWithSingleItem()
    {
        // Arrange
        var directory = "/src/lib";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/file1.ts", new MockFileData("// file1"));
        _mockFileSystem.AddFile($"{directory}/file2.ts", new MockFileData("// file2"));
        _mockFileSystem.AddFile($"{directory}/file3.ts", new MockFileData("// file3"));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].Body.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length);
    }

    #endregion

    #region Multiple Files Tests

    [Fact]
    public async Task IndexCreate_WithMultipleTypeScriptFiles_ShouldIncludeAll()
    {
        // Arrange
        var directory = "/src/app/models";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/user.ts", new MockFileData(""));
        _mockFileSystem.AddFile($"{directory}/product.ts", new MockFileData(""));
        _mockFileSystem.AddFile($"{directory}/order.ts", new MockFileData(""));
        _mockFileSystem.AddFile($"{directory}/customer.ts", new MockFileData(""));

        // Act
        var result = await _sut.IndexCreate(directory, scss: false);

        // Assert
        Assert.Single(result);
        var exports = result[0].Body.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, exports.Length);
    }

    [Fact]
    public async Task IndexCreate_WithMultipleScssFiles_ShouldIncludeAll()
    {
        // Arrange
        var directory = "/src/scss";
        _mockFileSystem.AddDirectory(directory);
        _mockFileSystem.AddFile($"{directory}/variables.scss", new MockFileData(""));
        _mockFileSystem.AddFile($"{directory}/mixins.scss", new MockFileData(""));
        _mockFileSystem.AddFile($"{directory}/base.scss", new MockFileData(""));
        _mockFileSystem.AddFile($"{directory}/components.scss", new MockFileData(""));
        _mockFileSystem.AddFile($"{directory}/utilities.scss", new MockFileData(""));

        // Act
        var result = await _sut.IndexCreate(directory, scss: true);

        // Assert
        Assert.Single(result);
        var uses = result[0].Body.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(5, uses.Length);
    }

    #endregion
}
