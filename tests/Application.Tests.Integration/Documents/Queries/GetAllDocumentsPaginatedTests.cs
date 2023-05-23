using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Mappings;
using Application.Documents.Queries.GetAllDocumentsPaginated;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bogus;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Documents.Queries;

public class GetAllDocumentsPaginatedTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllDocumentsPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>());

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnAllDocuments_WhenNoContainersAreDefined()
    {
        // Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };

        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            Room = room,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            Locker = locker,
            NumberOfDocuments = 1,
            IsAvailable = true
        };

        var document = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong",
            Folder = folder
        };

        await AddAsync(document);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(document));
        
        // Cleanup
        Remove(document);
        Remove(folder);
        Remove(locker);
        Remove(room);
    }

    [Fact]
    public async Task ShouldReturnEmptyPaginatedList_WhenNoDocumentsExist()
    {
        // Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };

        await AddAsync(room);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should().BeEmpty();
        
        // Cleanup
        Remove(room);
    }

    [Fact]
    public async Task ShouldReturnDocumentsOfRoom_WhenOnlyRoomIdIsPresent()
    {
        // Arrange
        var document1 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong",
        };
        
        var document2 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong 2",
        };

        var folder1 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };
        folder1.Documents.Add(document1);
        folder1.Documents.Add(document2);

        var locker1 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        locker1.Folders.Add(folder1);
        
        var room1 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        room1.Lockers.Add(locker1);

        var document3 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong",
        };
        
        var document4 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong 2",
        };
        
        
        var folder2 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };
        folder2.Documents.Add(document3);
        folder2.Documents.Add(document4);

        var locker2 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        locker2.Folders.Add(folder2);
        
        var room2 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        room2.Lockers.Add(locker2);
        
        await AddAsync(room1);
        await AddAsync(room2);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room1.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(document1));
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(document2));
        result.Items.Should().NotContainEquivalentOf(_mapper.Map<DocumentItemDto>(document3));
        result.Items.Should().NotContainEquivalentOf(_mapper.Map<DocumentItemDto>(document4));
        
        // Cleanup
        Remove(document1);
        Remove(document2);
        Remove(document3);
        Remove(document4);
        Remove(folder1);
        Remove(folder2);
        Remove(locker1);
        Remove(locker2);
        Remove(room1);
        Remove(room2);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenOnlyRoomIdIsPresentButDoesNotExist()
    {
        // Arrange
        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = Guid.NewGuid()
        };
        
        // Act
        var action = async () => await SendAsync(query);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>("Room does not exist.");
    }

    [Fact]
    public async Task ShouldReturnDocumentsOfLocker_WhenOnlyRoomIdAndLockerIdArePresentAndLockerIsInRoom()
    {
        // Arrange
        var document1 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong",
        };
        
        var document2 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong 2",
        };

        var folder1 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };
        folder1.Documents.Add(document1);
        folder1.Documents.Add(document2);

        var locker1 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        locker1.Folders.Add(folder1);

        var document3 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong",
        };
        
        var document4 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong 2",
        };

        var folder2 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };
        folder2.Documents.Add(document3);
        folder2.Documents.Add(document4);

        var locker2 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        locker2.Folders.Add(folder2);
        
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        room.Lockers.Add(locker1);
        room.Lockers.Add(locker2);

        await AddAsync(room);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room.Id,
            LockerId = locker1.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(document1));
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(document2));
        result.Items.Should().NotContainEquivalentOf(_mapper.Map<DocumentItemDto>(document3));
        result.Items.Should().NotContainEquivalentOf(_mapper.Map<DocumentItemDto>(document4));
        
        // Cleanup
        Remove(document1);
        Remove(document2);
        Remove(document3);
        Remove(document4);
        Remove(folder1);
        Remove(folder2);
        Remove(locker1);
        Remove(locker2);
        Remove(room);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenOnlyRoomIdAndLockerIdArePresentAndLockerDoesNotExist()
    {
        // Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        await AddAsync(room);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room.Id,
            LockerId = Guid.NewGuid()
        };

        // Act
        var action = async () => await SendAsync(query);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>("Locker does not exist.");
        
        // Cleanup
        Remove(room);
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenOnlyRoomIdAndLockerIdArePresentAndLockerIsNotInRoom()
    {
        // Arrange
        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        
        var room1 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        
        var room2 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        
        room2.Lockers.Add(locker);
        await AddAsync(room1);
        await AddAsync(room2);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room1.Id,
            LockerId = locker.Id
        };

        // Act
        var action = async () => await SendAsync(query);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>("Room does not match locker.");
        
        // Cleanup
        Remove(locker);
        Remove(room1);
        Remove(room2);
    }
    
    [Fact]
    public async Task ShouldReturnDocumentsOfFolder_WhenAllIdsArePresentAndFolderIsInBothLockerAndRoom()
    {
        // Arrange
        var document1 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong",
        };
        
        var document2 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong 2",
        };

        var folder1 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };
        folder1.Documents.Add(document1);
        folder1.Documents.Add(document2);


        var document3 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong",
        };
        
        var document4 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong 2",
        };

        var folder2 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };
        folder2.Documents.Add(document3);
        folder2.Documents.Add(document4);
        
        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        locker.Folders.Add(folder1);
        locker.Folders.Add(folder2);
        
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        room.Lockers.Add(locker);

        await AddAsync(room);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room.Id,
            LockerId = locker.Id,
            FolderId = folder1.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(document1));
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(document2));
        result.Items.Should().NotContainEquivalentOf(_mapper.Map<DocumentItemDto>(document3));
        result.Items.Should().NotContainEquivalentOf(_mapper.Map<DocumentItemDto>(document4));
        
        // Cleanup
        Remove(document1);
        Remove(document2);
        Remove(document3);
        Remove(document4);
        Remove(folder1);
        Remove(folder2);
        Remove(locker);
        Remove(room);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenAllIdsArePresentAndValidAndFolderDoesNotExist()
    {
        // Arrange
        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        room.Lockers.Add(locker);
        
        await AddAsync(room);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room.Id,
            LockerId = locker.Id,
            FolderId = Guid.NewGuid()
        };

        // Act
        var action = async () => await SendAsync(query);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>("Folder does not exist.");
        
        // Cleanup
        Remove(locker);
        Remove(room);
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenAllIdsArePresentAndFolderIsNotInLockerOrInRoom()
    {
        // Arrange
        var folder1 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };
        
        var locker1 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        locker1.Folders.Add(folder1);

        var room1 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        room1.Lockers.Add(locker1);
        
        var folder2 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };

        var locker2 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        locker2.Folders.Add(folder2);
        
        var room2 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };

        room2.Lockers.Add(locker2);
        await AddAsync(room1);
        await AddAsync(room2);

        var query1 = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room1.Id,
            LockerId = locker2.Id,
            FolderId = folder1.Id
        };
        
        var query2 = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room1.Id,
            LockerId = locker2.Id,
            FolderId = folder2.Id
        };

        // Act
        var action1 = async () => await SendAsync(query1);
        var action2 = async () => await SendAsync(query2);
        
        // Assert
        await action1.Should().ThrowAsync<ConflictException>("Either locker or room does not match folder.");
        await action1.Should().ThrowAsync<ConflictException>("Either locker or room does not match folder.");
        
        // Cleanup
        Remove(folder1);
        Remove(folder2);
        Remove(locker1);
        Remove(locker2);
        Remove(room1);
        Remove(room2);
    }
    
    [Fact]
    public async Task ShouldReturnSortedByIdPaginatedList_WhenSortByIsNotPresent()
    {
        // Arrange
        var document1 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong",
        };
        
        var document2 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong 2",
        };
        
        
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };
        folder.Documents.Add(document1);
        folder.Documents.Add(document2);

        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        locker.Folders.Add(folder);
        
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        room.Lockers.Add(locker);

        await AddAsync(room);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.First().Should().BeEquivalentTo(_mapper.Map<DocumentItemDto>(document1.Id.CompareTo(document2.Id) <= 0 ? document1 : document2));
        
        // Cleanup
        Remove(document1);
        Remove(document2);
        Remove(folder);
        Remove(locker);
        Remove(room);
    }

    [Theory]
    [InlineData(nameof(DocumentItemDto.Id), "asc")]
    [InlineData(nameof(DocumentItemDto.Title), "asc")]
    [InlineData(nameof(DocumentItemDto.DocumentType), "asc")]
    [InlineData(nameof(DocumentItemDto.Description), "asc")]
    [InlineData(nameof(DocumentItemDto.FolderId), "asc")]
    [InlineData(nameof(DocumentItemDto.DepartmentId), "asc")]
    [InlineData(nameof(DocumentItemDto.ImporterId), "asc")]
    [InlineData(nameof(DocumentItemDto.Id), "desc")]
    [InlineData(nameof(DocumentItemDto.Title), "desc")]
    [InlineData(nameof(DocumentItemDto.DocumentType), "desc")]
    [InlineData(nameof(DocumentItemDto.Description), "desc")]
    [InlineData(nameof(DocumentItemDto.FolderId), "desc")]
    [InlineData(nameof(DocumentItemDto.DepartmentId), "desc")]
    [InlineData(nameof(DocumentItemDto.ImporterId), "desc")]
    public async Task ShouldReturnSortedByPropertyPaginatedList_WhenSortByIsPresent(string sortBy, string sortOrder)
    {
        // Arrange
        var document1 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong",
        };
        
        var document2 = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Commerce.ProductName(),
            DocumentType = "Hop Dong 2",
        };
        
        
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = 1,
            IsAvailable = true
        };
        folder.Documents.Add(document1);
        folder.Documents.Add(document2);

        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        locker.Folders.Add(folder);
        
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
        room.Lockers.Add(locker);

        await AddAsync(room);

        var query = new GetAllDocumentsPaginatedQuery()
        {
            RoomId = room.Id,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        
        var list = new EnumerableQuery<DocumentItemDto>(_mapper.Map<List<DocumentItemDto>>(new List<Document>() { document1, document2 }));
        var list2 = list.OrderByCustom(sortBy, sortOrder);
        var expected = list2.ToList();
        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should().BeEquivalentTo(expected);
        
        // Cleanup
        Remove(document1);
        Remove(document2);
        Remove(folder);
        Remove(locker);
        Remove(room);
    }
}