using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Documents.Queries;
using AutoMapper;
using Domain.Entities;
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
        var department = CreateDepartment();
        var documents = CreateNDocuments(1);
        documents.First().Department = department;
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var query = new GetAllDocumentsPaginated.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.First().Title.Should().Be(documents.First().Title);
        
        // Cleanup
        Remove(documents.First());
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldReturnEmptyPaginatedList_WhenNoDocumentsExist()
    {
        // Arrange
        var department = CreateDepartment();
        var room = CreateRoom(department);

        await AddAsync(room);

        var query = new GetAllDocumentsPaginated.Query()
        {
            RoomId = room.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should().BeEmpty();
        
        // Cleanup
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldReturnDocumentsOfRoom_WhenOnlyRoomIdIsPresent()
    {
        // Arrange
        var department1 = CreateDepartment();
        var department2 = CreateDepartment();
        var documents1 = CreateNDocuments(2);
        var folder1 = CreateFolder(documents1);
        var locker1 = CreateLocker(folder1);
        var room1 = CreateRoom(department1, locker1);
        var documents2 = CreateNDocuments(2);
        var folder2 = CreateFolder(documents2);
        var locker2 = CreateLocker(folder2);
        var room2 = CreateRoom(department2, locker2);
        await AddAsync(room1);
        await AddAsync(room2);

        var query = new GetAllDocumentsPaginated.Query()
        {
            RoomId = room1.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should()
            .BeEquivalentTo(_mapper.Map<DocumentDto[]>(documents1), x => x.IgnoringCyclicReferences());
        result.Items.Should()
            .NotBeEquivalentTo(_mapper.Map<DocumentDto[]>(documents2), x => x.IgnoringCyclicReferences());

        // Cleanup
        Remove(documents1[0]);
        Remove(documents1[1]);
        Remove(documents2[0]);
        Remove(documents2[1]);
        Remove(folder1);
        Remove(folder2);
        Remove(locker1);
        Remove(locker2);
        Remove(room1);
        Remove(room2);
        Remove(await FindAsync<Department>(department1.Id));
        Remove(await FindAsync<Department>(department2.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenOnlyRoomIdIsPresentButDoesNotExist()
    {
        // Arrange
        var query = new GetAllDocumentsPaginated.Query()
        {
            RoomId = Guid.NewGuid(),
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
        var department = CreateDepartment();
        var documents1 = CreateNDocuments(2);
        var folder1 = CreateFolder(documents1);
        var locker1 = CreateLocker(folder1);
        var documents2 = CreateNDocuments(2);
        var folder2 = CreateFolder(documents2);
        var locker2 = CreateLocker(folder2);
        var room = CreateRoom(department, locker1, locker2);
        await AddAsync(room);

        var query = new GetAllDocumentsPaginated.Query()
        {
            RoomId = room.Id,
            LockerId = locker1.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should()
            .BeEquivalentTo(_mapper.Map<DocumentDto[]>(documents1), x => x.IgnoringCyclicReferences());
        result.Items.Should()
            .NotBeEquivalentTo(_mapper.Map<DocumentDto[]>(documents2), x => x.IgnoringCyclicReferences());

        // Cleanup
        Remove(documents1[0]);
        Remove(documents1[1]);
        Remove(documents2[0]);
        Remove(documents2[1]);
        Remove(folder1);
        Remove(folder2);
        Remove(locker1);
        Remove(locker2);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenOnlyRoomIdAndLockerIdArePresentAndLockerDoesNotExist()
    {
        // Arrange
        var department = CreateDepartment();
        var room = CreateRoom(department);
        await AddAsync(room);

        var query = new GetAllDocumentsPaginated.Query()
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
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenOnlyRoomIdAndLockerIdArePresentAndLockerIsNotInRoom()
    {
        // Arrange
        var department1 = CreateDepartment();
        var department2 = CreateDepartment();
        var locker = CreateLocker();
        var room1 = CreateRoom(department1);
        var room2 = CreateRoom(department2, locker);
        await AddAsync(room1);
        await AddAsync(room2);

        var query = new GetAllDocumentsPaginated.Query()
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
        Remove(await FindAsync<Department>(department1.Id));
        Remove(await FindAsync<Department>(department2.Id));
    }
    
    [Fact]
    public async Task ShouldReturnDocumentsOfFolder_WhenAllIdsArePresentAndFolderIsInBothLockerAndRoom()
    {
        // Arrange
        var department = CreateDepartment();
        var documents1 = CreateNDocuments(2);
        documents1[0].Department = department;
        documents1[1].Department = department;
        var folder1 = CreateFolder(documents1);
        var documents2 = CreateNDocuments(2);
        documents2[0].Department = department;
        documents2[1].Department = department;
        var folder2 = CreateFolder(documents2);
        var locker = CreateLocker(folder1, folder2);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var query = new GetAllDocumentsPaginated.Query()
        {
            RoomId = room.Id,
            LockerId = locker.Id,
            FolderId = folder1.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should()
            .BeEquivalentTo(_mapper.Map<DocumentDto[]>(documents1), x => x.IgnoringCyclicReferences());
        result.Items.Should()
            .NotBeEquivalentTo(_mapper.Map<DocumentDto[]>(documents2), x => x.IgnoringCyclicReferences());
        
        // Cleanup
        Remove(documents1[0]);
        Remove(documents1[1]);
        Remove(documents2[0]);
        Remove(documents2[1]);
        Remove(folder1);
        Remove(folder2);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenAllIdsArePresentAndValidAndFolderDoesNotExist()
    {
        // Arrange
        var department = CreateDepartment();
        var locker = CreateLocker();
        var room = CreateRoom(department, locker);
        
        await AddAsync(room);

        var query = new GetAllDocumentsPaginated.Query()
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
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenAllIdsArePresentAndFolderIsNotInLockerOrInRoom()
    {
        // Arrange
        var department1 = CreateDepartment();
        var department2 = CreateDepartment();
        var folder1 = CreateFolder();
        var locker1 = CreateLocker(folder1);
        var room1 = CreateRoom(department1, locker1);
        var folder2 = CreateFolder();
        var locker2 = CreateLocker(folder2);
        var room2 = CreateRoom(department2, locker2);
        await AddAsync(room1);
        await AddAsync(room2);

        var query1 = new GetAllDocumentsPaginated.Query()
        {
            RoomId = room1.Id,
            LockerId = locker2.Id,
            FolderId = folder1.Id
        };
        
        var query2 = new GetAllDocumentsPaginated.Query()
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
        await action2.Should().ThrowAsync<ConflictException>("Either locker or room does not match folder.");
        
        // Cleanup
        Remove(folder1);
        Remove(folder2);
        Remove(locker1);
        Remove(locker2);
        Remove(room1);
        Remove(room2);
        Remove(await FindAsync<Department>(department1.Id));
        Remove(await FindAsync<Department>(department2.Id));
    }
    
    [Fact]
    public async Task ShouldReturnSortedByIdPaginatedList_WhenSortByIsNotPresent()
    {
        // Arrange
        var department = CreateDepartment();
        var documents = CreateNDocuments(2);
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var query = new GetAllDocumentsPaginated.Query()
        {
            RoomId = room.Id,
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.First().Should()
            .BeEquivalentTo(
                _mapper.Map<DocumentDto[]>(documents)
                    .OrderBy(x => x.Id).First(),
                x => x.IgnoringCyclicReferences());
        
        // Cleanup
        Remove(documents[0]);
        Remove(documents[1]);
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Theory]
    [InlineData(nameof(DocumentDto.Id), "asc")]
    [InlineData(nameof(DocumentDto.Title), "asc")]
    [InlineData(nameof(DocumentDto.DocumentType), "asc")]
    [InlineData(nameof(DocumentDto.Description), "asc")]
    [InlineData(nameof(DocumentDto.Id), "desc")]
    [InlineData(nameof(DocumentDto.Title), "desc")]
    [InlineData(nameof(DocumentDto.DocumentType), "desc")]
    [InlineData(nameof(DocumentDto.Description), "desc")]
    public async Task ShouldReturnSortedByPropertyPaginatedList_WhenSortByIsPresent(string sortBy, string sortOrder)
    {
        // Arrange
        var department = CreateDepartment();
        var documents = CreateNDocuments(2);
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);

        await AddAsync(room);

        var query = new GetAllDocumentsPaginated.Query()
        {
            RoomId = room.Id,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        var list = new EnumerableQuery<DocumentDto>(_mapper.Map<List<DocumentDto>>(documents));
        var list2 = list.OrderByCustom(sortBy, sortOrder);
        var expected = list2.ToList();
        
        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should().BeEquivalentTo(expected, x => x.IgnoringCyclicReferences());
        
        // Cleanup
        Remove(documents[0]);
        Remove(documents[1]);
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
}