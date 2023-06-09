using Application.Common.AccessControl.Base;
using Application.Common.AccessControl.Models;
using Application.Common.Interfaces;

namespace Infrastructure.Identity.Authorization;

public class PhysicalAccessControlList : IPhysicalAccessControlList
{
    private readonly BaseControlList _grantedList;
    private readonly BaseControlList _deniedList;

    public PhysicalAccessControlList()
    {
        _grantedList = new BaseControlList();
        _deniedList = new BaseControlList();
    }

    public bool IsGranted(PhysicalResource resource, Enum operation, params PhysicalPrincipal[] principals)
    {
        return _grantedList.IsIncluded(resource, operation, principals.Select(x => x as ValueType));
    }

    public bool IsDenied(PhysicalResource resource, Enum operation, params PhysicalPrincipal[] principals)
    {
        return _deniedList.IsIncluded(resource, operation, principals.Select(x => x as ValueType));
    }

    public void Grant(PhysicalResource resource, Enum operation, PhysicalPrincipal physicalPrincipal)
    {
        _grantedList.Include(resource, operation, physicalPrincipal);
    }

    public void Deny(PhysicalResource resource, Enum operation, PhysicalPrincipal physicalPrincipal)
    {
        _deniedList.Include(resource, operation, physicalPrincipal);
    }
}