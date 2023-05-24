    using Application.Common.Exceptions;
    using Application.Common.Extensions;
    using Application.Common.Mappings;
    using Application.Common.Models.Dtos.Physical;
    using Application.Documents.Queries.GetAllDocumentsPaginated;
    using AutoMapper;
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
            var documents = CreateNDocuments(1);
            var folder = CreateFolder(documents);
            var locker = CreateLocker(folder);
            var room = CreateRoom(locker);
            await AddAsync(room);

            var query = new GetAllDocumentsPaginatedQuery();

            // Act
            var result = await SendAsync(query);

            // Assert
            result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(documents.First()));
            
            // Cleanup
            Remove(documents.First());
            Remove(folder);
            Remove(locker);
            Remove(room);
        }

        [Fact]
        public async Task ShouldReturnEmptyPaginatedList_WhenNoDocumentsExist()
        {
            // Arrange
            var room = CreateRoom();

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
            var documents1 = CreateNDocuments(2);

            var folder1 = CreateFolder(documents1);

            var locker1 = CreateLocker(folder1);

            var room1 = CreateRoom(locker1);

            var documents2 = CreateNDocuments(2);

            var folder2 = CreateFolder(documents2);

            var locker2 = CreateLocker(folder2);

            var room2 = CreateRoom(locker2);
            
            await AddAsync(room1);
            await AddAsync(room2);

            var query = new GetAllDocumentsPaginatedQuery()
            {
                RoomId = room1.Id
            };

            // Act
            var result = await SendAsync(query);

            // Assert
            result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(documents1[0]));
            result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentItemDto>(documents1[1]));
            result.Items.Should().NotContainEquivalentOf(_mapper.Map<DocumentItemDto>(documents2[0]));
            result.Items.Should().NotContainEquivalentOf(_mapper.Map<DocumentItemDto>(documents2[1]));
            
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
            var documents1 = CreateNDocuments(2);

            var folder1 = CreateFolder(documents1);

            var locker1 = CreateLocker(folder1);
            
            var documents2 = CreateNDocuments(2);

            var folder2 = CreateFolder(documents2);

            var locker2 = CreateLocker(folder2);

            var room = CreateRoom(locker1, locker2);

            await AddAsync(room);

            var query = new GetAllDocumentsPaginatedQuery()
            {
                RoomId = room.Id,
                LockerId = locker1.Id
            };

            // Act
            var result = await SendAsync(query);

            // Assert
            result.Items.Should().BeEquivalentTo(_mapper.Map<List<DocumentItemDto>>(documents1));
            result.Items.Should().NotContainEquivalentOf(_mapper.Map<List<DocumentItemDto>>(documents2));
            
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
        }

        [Fact]
        public async Task ShouldThrowKeyNotFoundException_WhenOnlyRoomIdAndLockerIdArePresentAndLockerDoesNotExist()
        {
            // Arrange
            var room = CreateRoom();
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
            var locker = CreateLocker();

            var room1 = CreateRoom();

            var room2 = CreateRoom(locker);
            
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
            var documents1 = CreateNDocuments(2);
            var folder1 = CreateFolder(documents1);

            var documents2 = CreateNDocuments(2);
            var folder2 = CreateFolder(documents2);
            
            var locker = CreateLocker(folder1, folder2);
            var room = CreateRoom(locker);
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
            result.Items.Should().BeEquivalentTo(_mapper.Map<DocumentItemDto[]>(documents1));
            result.Items.Should().NotBeEquivalentTo(_mapper.Map<DocumentItemDto[]>(documents2));
            
            // Cleanup
            Remove(documents1[0]);
            Remove(documents1[1]);
            Remove(documents2[0]);
            Remove(documents2[1]);
            Remove(folder1);
            Remove(folder2);
            Remove(locker);
            Remove(room);
        }
        
        [Fact]
        public async Task ShouldThrowKeyNotFoundException_WhenAllIdsArePresentAndValidAndFolderDoesNotExist()
        {
            // Arrange
            var locker = CreateLocker();
            var room = CreateRoom(locker);
            
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
            var folder1 = CreateFolder();
            var locker1 = CreateLocker(folder1);
            var room1 = CreateRoom(locker1);

            var folder2 = CreateFolder();
            var locker2 = CreateLocker(folder2);
            var room2 = CreateRoom(locker2);
            
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
            var documents = CreateNDocuments(2);
            var folder = CreateFolder(documents);
            var locker = CreateLocker(folder);
            var room = CreateRoom(locker);

            await AddAsync(room);

            var query = new GetAllDocumentsPaginatedQuery()
            {
                RoomId = room.Id
            };

            // Act
            var result = await SendAsync(query);

            // Assert
            result.Items.First().Should().BeEquivalentTo(_mapper.Map<DocumentItemDto>(documents[0].Id.CompareTo(documents[1].Id) <= 0 ? documents[0] : documents[1]));
            
            // Cleanup
            Remove(documents[0]);
            Remove(documents[1]);
            Remove(folder);
            Remove(locker);
            Remove(room);
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
            var documents = CreateNDocuments(2);
            var folder = CreateFolder(documents);
            var locker = CreateLocker(folder);
            var room = CreateRoom(locker);

            await AddAsync(room);

            var query = new GetAllDocumentsPaginatedQuery()
            {
                RoomId = room.Id,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var list = new EnumerableQuery<DocumentItemDto>(_mapper.Map<List<DocumentItemDto>>(documents));
            var list2 = list.OrderByCustom(sortBy, sortOrder);
            var expected = list2.ToList();
            
            // Act
            var result = await SendAsync(query);

            // Assert
            result.Items.Should().BeEquivalentTo(expected);
            
            // Cleanup
            Remove(documents[0]);
            Remove(documents[1]);
            Remove(folder);
            Remove(locker);
            Remove(room);
        }

        private Document[] CreateNDocuments(int n)
        {
            var list = new List<Document>();
            for (var i = 0; i < n; i++)
            {
                var document = new Document()
                {
                    Id = Guid.NewGuid(),
                    Title = new Faker().Commerce.ProductName(),
                    DocumentType = "Hop Dong",
                };
                list.Add(document);
            }

            return list.ToArray();
        }

        private Folder CreateFolder(params Document[] documents)
        {
            var folder = new Folder()
            {
                Id = Guid.NewGuid(),
                Name = new Faker().Commerce.ProductName(),
                Capacity = 3,
                NumberOfDocuments = 1,
                IsAvailable = true
            };

            foreach (var document in documents)
            {
                folder.Documents.Add(document);
            }

            return folder;
        }

        private Locker CreateLocker(params Folder[] folders)
        {
            var locker = new Locker()
            {
                Id = Guid.NewGuid(),
                Name = new Faker().Commerce.ProductName(),
                Capacity = 3,
                NumberOfFolders = 1,
                IsAvailable = true
            };

            foreach (var folder in folders)
            {
                locker.Folders.Add(folder);
            }
            
            return locker;
        }
        
        private Room CreateRoom(params Locker[] lockers)
        {
            var room = new Room()
            {
                Id = Guid.NewGuid(),
                Name = new Faker().Commerce.ProductName(),
                Capacity = 3,
                NumberOfLockers = 1,
                IsAvailable = true
            };

            foreach (var locker in lockers)
            {
                room.Lockers.Add(locker);
            }
            
            return room;
        }
    }