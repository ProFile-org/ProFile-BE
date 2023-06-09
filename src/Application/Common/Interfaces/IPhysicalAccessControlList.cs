using Application.Common.AccessControl.Models;

namespace Application.Common.Interfaces;

public interface IPhysicalAccessControlList
{
    bool IsGranted(PhysicalResource resource, Enum operation, params PhysicalPrincipal[] principals);
    bool IsDenied(PhysicalResource resource, Enum operation, params PhysicalPrincipal[] principals);
    void Grant(PhysicalResource resource, Enum operation, PhysicalPrincipal physicalPrincipal);
    void Deny(PhysicalResource resource, Enum operation, PhysicalPrincipal physicalPrincipal);
}