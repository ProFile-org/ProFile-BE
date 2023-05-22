using Application.Documents.Queries.GetDocumentsByTitle;
using Application.Helpers;
using Bogus;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.PhysicalDocuments.GetDocumentsByTitle.Queries;

public class GetDocumentsByTitleTest : BaseClassFixture
{
    public GetDocumentsByTitleTest(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldReturnAllDocuments_WhenUserRoleIsAdmin()
    {
        //Arrange
        var department01 = new Department()
        {
            Id = Guid.NewGuid(),
            Name = "Admin Department"
        };

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


        var department02 = new Department()
        {
            Id = Guid.NewGuid(),
            Name = "HR Department"
        };
        

        var user = new User()
        {
            Id = Guid.NewGuid(),
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            Username = new Faker().Internet.UserName(),
            PasswordHash = SecurityUtil.Hash(new Faker().Internet.Password()),
            Email = new Faker().Internet.Email(),
            Department = department01,
            Role = "Admin",
            Position = new Faker().Name.JobTitle(),
            IsActive = true,
            IsActivated = true
        };
        
       

        var importer = new User()
        {
            Id = Guid.NewGuid(),
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            Username = new Faker().Internet.UserName(),
            PasswordHash = SecurityUtil.Hash(new Faker().Internet.Password()),
            Email = new Faker().Internet.Email(),
            Department = department02,
            Role = new Faker().Name.JobDescriptor(),
            Position = new Faker().Name.JobTitle(),
            IsActive = true,
            IsActivated = true
        };

       
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
       
        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Capacity = 2,
            Room = room,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Capacity = 3,
            Locker = locker,
            NumberOfDocuments = 3,
            IsAvailable = true
        };
        
        var document01 = new Document()
        {
            Id = Guid.NewGuid(),
            Department = null,
            Description = "haha",
            DocumentType = "chienngu",
            Folder = folder,
            Importer = importer,
            Title = "doc1"
        };
        
        var document02 = new Document()
        {
            Id = Guid.NewGuid(),
            Department = department02,
            Description = "haha",
            DocumentType = "chienngu",
            Folder = folder,
            Importer = importer,
            Title = "doc2"
        };
        
        var document03 = new Document()
        {
            Id = Guid.NewGuid(),
            Department =  department02,
            Description = "haha",
            DocumentType = "chienngu",
            Folder = folder,
            Importer = importer,
            Title = "doc3"
        };
        await context.Users.AddAsync(user);
        await context.Users.AddAsync(importer);
        await context.Departments.AddAsync(department01);
        await context.Departments.AddAsync(department02);
        await context.Rooms.AddAsync(room);
        await context.Lockers.AddAsync(locker);
        await context.Folders.AddAsync(folder);
        await context.Documents.AddAsync(document01); 
        await context.Documents.AddAsync(document02);
        await context.Documents.AddAsync(document03);
        await context.SaveChangesAsync();
        
        //Act
        var query = new GetDocumentsByTitleQuery()
        {
            SearchTerm = "doc",
            UserId = user.Id
        };
        var documents = await SendAsync(query);
        
        //Assert
        documents.TotalCount.Should().Be(3);
        documents.Items.First().Department.Should().BeNull();
        
        //Cleanup
        Remove(document01);
        Remove(document02);
        Remove(document03);
        Remove(user);
        Remove(importer);
        Remove(department01);
        Remove(department02);
        Remove(folder);
        Remove(locker);
        Remove(room);

    }

    [Fact]
    public async Task ShouldReturnDocumentsOfUsersDepartment()
    {
        //Act
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hrDepartment = new Department()
        {
            Id = Guid.NewGuid(),
            Name = "HR Department"
        };
        
        var saleDepartment = new Department()
        {
            Id = Guid.NewGuid(),
            Name = "Sale Department"
        };

        var accountDepartment = new Department()
        {
            Id = Guid.NewGuid(),
            Name = "Account Department"
        };

        var hrUser = new User()
        {
            Id = Guid.NewGuid(),
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            Username = new Faker().Internet.UserName(),
            PasswordHash = SecurityUtil.Hash(new Faker().Internet.Password()),
            Email = new Faker().Internet.Email(),
            Department = hrDepartment,
            Role = "Hr something something",
            Position = new Faker().Name.JobTitle(),
            IsActive = true,
            IsActivated = true
        };

        var hrUserImporter = new User()
        {
            Id = Guid.NewGuid(),
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            Username = new Faker().Internet.UserName(),
            PasswordHash = SecurityUtil.Hash(new Faker().Internet.Password()),
            Email = new Faker().Internet.Email(),
            Department = hrDepartment,
            Role = "Hr something something",
            Position = new Faker().Name.JobTitle(),
            IsActive = true,
            IsActivated = true
        };

        var saleUser = new User()
        {
            Id = Guid.NewGuid(),
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            Username = new Faker().Internet.UserName(),
            PasswordHash = SecurityUtil.Hash(new Faker().Internet.Password()),
            Email = new Faker().Internet.Email(),
            Department = saleDepartment,
            Role = "Sale something something",
            Position = new Faker().Name.JobTitle(),
            IsActive = true,
            IsActivated = true
        };

        var accountUser = new User()
        {
            Id = Guid.NewGuid(),
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            Username = new Faker().Internet.UserName(),
            PasswordHash = SecurityUtil.Hash(new Faker().Internet.Password()),
            Email = new Faker().Internet.Email(),
            Department = accountDepartment,
            Role = "Account something something",
            Position = new Faker().Name.JobTitle(),
            IsActive = true,
            IsActivated = true
        };
        
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Capacity = 3,
            NumberOfLockers = 1,
            IsAvailable = true
        };
       
        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Capacity = 2,
            Room = room,
            NumberOfFolders = 1,
            IsAvailable = true
        };
        
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Capacity = 3,
            Locker = locker,
            NumberOfDocuments = 3,
            IsAvailable = true
        };
        
        var document01 = new Document()
        {
            Id = Guid.NewGuid(),
            Department = hrDepartment,
            Description = "haha",
            DocumentType = "chienngu",
            Folder = folder,
            Importer = hrUserImporter,
            Title = "doc1"
        };
        
        var document02 = new Document()
        {
            Id = Guid.NewGuid(),
            Department = hrDepartment,
            Description = "haha",
            DocumentType = "chienngu",
            Folder = folder,
            Importer = hrUserImporter,
            Title = "doc2"
        };
        
        var document03 = new Document()
        {
            Id = Guid.NewGuid(),
            Department = saleDepartment,
            Description = "haha",
            DocumentType = "chienngu",
            Folder = folder,
            Importer = saleUser,
            Title = "doc3"
        };

        await context.Departments.AddAsync(hrDepartment);
        await context.Departments.AddAsync(saleDepartment);
        await context.Users.AddAsync(hrUser);
        await context.Users.AddAsync(hrUserImporter);
        await context.Users.AddAsync(saleUser);
        await context.Users.AddAsync(accountUser);
        await context.Rooms.AddAsync(room);
        await context.Lockers.AddAsync(locker);
        await context.Folders.AddAsync(folder);
        await context.Documents.AddAsync(document01);
        await context.Documents.AddAsync(document02);
        await context.Documents.AddAsync(document03);
        await context.SaveChangesAsync();
        
        //Act 
        var hrQuery = new GetDocumentsByTitleQuery()
        {
            SearchTerm = "doc",
            UserId = hrUser.Id
        };
        var hrDocuments = await SendAsync(hrQuery);

        var saleQuery = new GetDocumentsByTitleQuery()
        {
            SearchTerm = "doc",
            UserId = saleUser.Id
        };
        var saleDocuments = await SendAsync(saleQuery);

        var accountQuery = new GetDocumentsByTitleQuery()
        {
            SearchTerm = "doc",
            UserId = accountUser.Id
        };
        var accountDocuments = await SendAsync(accountQuery);
        //Assert
        hrDocuments.TotalCount.Should().Be(2);
        saleDocuments.TotalCount.Should().Be(1);
        accountDocuments.TotalCount.Should().Be(0);
        
        //Cleanup
        Remove(document01);
        Remove(document02);
        Remove(document03);
        Remove(hrUser);
        Remove(hrUserImporter);
        Remove(saleUser);
        Remove(accountUser);
        Remove(hrDepartment);
        Remove(saleDepartment);
        Remove(folder);
        Remove(locker);
        Remove(room);
    }
}