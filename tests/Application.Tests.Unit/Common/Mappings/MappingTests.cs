using System.Runtime.Serialization;
using Application.Common.Mappings;
using Application.Common.Models.Dtos;
using Application.Common.Models.Dtos.Digital;
using Application.Common.Models.Dtos.ImportDocument;
using Application.Common.Models.Dtos.Physical;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Digital;
using Domain.Entities.Physical;
using Xunit;

namespace Application.Tests.Unit.Common.Mappings;

public class MappingTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        _configuration = new MapperConfiguration(config => 
            config.AddProfile<MappingProfile>());

        _mapper = _configuration.CreateMapper();
    }
    
    [Fact]
    public void ShouldHaveValidConfiguration()
    {
        // Assert
        _configuration.AssertConfigurationIsValid();
    }
    
    [Theory]
    [InlineData(typeof(Department), typeof(DepartmentDto))]
    [InlineData(typeof(User), typeof(UserDto))]
    [InlineData(typeof(Staff), typeof(Staff))]    
    [InlineData(typeof(Room), typeof(RoomDto))]
    [InlineData(typeof(Locker), typeof(LockerDto))]
    [InlineData(typeof(Locker), typeof(EmptyLockerDto))]
    [InlineData(typeof(Folder), typeof(FolderDto))]
    [InlineData(typeof(Folder), typeof(EmptyFolderDto))]
    [InlineData(typeof(Document), typeof(DocumentDto))]
    [InlineData(typeof(Document), typeof(DocumentItemDto))]
    [InlineData(typeof(Borrow), typeof(BorrowDto))]
    [InlineData(typeof(RefreshToken), typeof(RefreshTokenDto))]
    [InlineData(typeof(FileEntity), typeof(FileDto))]
    [InlineData(typeof(Entry), typeof(EntryDto))]
    [InlineData(typeof(User), typeof(IssuerDto))]
    [InlineData(typeof(Document), typeof(IssuedDocumentDto))]
    [InlineData(typeof(ImportRequest), typeof(ImportRequestDto))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        // Arrange
        var instance = GetInstanceOf(source);

        // Act
        _mapper.Map(instance, source, destination);
    }
    
    private object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type)!;

        // Type without parameterless constructor
        return FormatterServices.GetUninitializedObject(type);
    }
}