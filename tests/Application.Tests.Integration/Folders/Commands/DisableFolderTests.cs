using Application.Common.Exceptions;
using Application.Folders.Commands;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class DisableFolderTests : BaseClassFixture
{
    public DisableFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldDisableFolder_WhenFolderHaveNoDocument()
    {
        // Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Capacity = 24,
            IsAvailable = true,
            Name = "Kamito's room",
            NumberOfLockers = 1,
        };

        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Capacity = 47,
            Name = "fqwlkjdb sajdbqwk",
            IsAvailable = true,
            Room = room,
            NumberOfFolders = 1,
        };
        
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Capacity = 12,
            IsAvailable = true,
            Name = "A Random name",
            NumberOfDocuments = 0,
            Locker = locker
        };

        await AddAsync(folder);
        var disableFolderCommand = new DisableFolder.Command()
        {
            FolderId = folder.Id
        };
        
        // Act
        var disabledFolder = await SendAsync(disableFolderCommand);

        // Assert
        disabledFolder.IsAvailable.Should().BeFalse();
        
        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenFolderDoesNotExist()
    {
        // Arrange
        var disableFolderCommand = new DisableFolder.Command()
        {
            FolderId = Guid.NewGuid()
        };
        
        // Act
        var result = async () => await SendAsync(disableFolderCommand);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Folder does not exist.");
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenFolderIsAlreadyDisabled()
    {
        // Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Capacity = 24,
            IsAvailable = true,
            Name = "Kamito's room!",
            NumberOfLockers = 1,
        };

        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Capacity = 47,
            Name = "fqwlkjdb sajdbqwk!",
            IsAvailable = true,
            Room = room,
            NumberOfFolders = 1,
        };
        
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Capacity = 12,
            IsAvailable = false,
            Name = "A Random name!",
            NumberOfDocuments = 0,
            Locker = locker
        };
        await AddAsync(folder);
        var disableFolderCommand = new DisableFolder.Command()
        {
            FolderId = folder.Id
        };
        
        // Act
        var result = async () => await SendAsync(disableFolderCommand);

        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Folder has already been disabled.");
        
        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
    }
    
    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenFolderHasDocuments()
    {
        // Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Capacity = 24,
            IsAvailable = true,
            Name = "Kamito's room!!safqwf!!",
            NumberOfLockers = 1,
        };

        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Capacity = 47,
            Name = "fqwlkjdb sawfqfw wfqwfjdbqwk!",
            IsAvailable = true,
            Room = room,
            NumberOfFolders = 1,
        };
        
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Capacity = 12,
            IsAvailable = true,
            Name = "A Randasfwqfawfqom name!",
            NumberOfDocuments = 1,
            Locker = locker
        };

        var document = new Document()
        {
            DocumentType = "fqwkfwqbfk",
            Id = Guid.NewGuid(),
            Title = "wjqk ljfqwjlf qwkhjf ;qikwf",
            Folder = folder
        };

        await AddAsync(document);
        var disableFolderCommand = new DisableFolder.Command()
        {
            FolderId = folder.Id
        };
        
        // Act
        var result = async () => await SendAsync(disableFolderCommand);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Folder cannot be disabled because it contains documents.");
        
        // Cleanup
        Remove(document);
        Remove(folder);
        Remove(locker);
        Remove(room);
    }
}