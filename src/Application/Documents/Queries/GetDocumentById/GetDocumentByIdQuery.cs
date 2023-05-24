using Application.Common.Interfaces;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Documents.Queries.GetDocumentById;

public record GetDocumentByIdQuery : IRequest<DocumentDto>
{
    public Guid Id { get; init; } 
}