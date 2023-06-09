using Application.Common.AccessControl.Models;

namespace Application.Common.AccessControl.Base;

public class BaseControlList
{
    /// <summary>
    /// Store a collection of base control items for each resource, using the resource as a hash key.
    /// </summary>
    private readonly Dictionary<ValueType, BaseControlItem> _operations;
    private readonly List<ValueType> _basePrincipalList;

    /// <summary>
    /// Disallow a null dictionary.
    /// </summary>
    public BaseControlList()
    {
        _operations = new Dictionary<ValueType, BaseControlItem>();
        _basePrincipalList = new List<ValueType>();
    } 

    /// <summary>
    /// Returns true if the dictionary contains a collection of operations for the resource; false otherwise.
    /// </summary>
    public bool Contains(ValueType physicalResource)
    {
        return _operations.ContainsKey(physicalResource);
    }

    /// <summary>
    /// Removes the principal from the operation on the resource.
    /// </summary>
    public void Exclude(ValueType physicalResource, Enum operation, ValueType physicalPrincipal)
    {
        var value = GetValue(physicalResource);
        value.Exclude(operation, physicalPrincipal);
    }

    /// <summary>
    /// Returns only those principals already added to the operation on the resource. Given a list of principals, 
    /// you might want to know which one(s) have been included for a specific operation on a specific resource.
    /// </summary>
    public IList<ValueType> FindIncludedPrincipals(ValueType physicalResource, Enum operation, params ValueType[] principals)
    {
        var value = GetValue(physicalResource);
        return value.FindIncludedPrincipals(operation, principals);
    }

    /// <summary>
    /// Adds the principal to the operation on the resource.
    /// </summary>
    public void Include(ValueType physicalResource, Enum operation, ValueType physicalPrincipal)
    {
        var value = GetValue(physicalResource);
        value.Include(operation, physicalPrincipal);
    }

    /// <summary>
    /// Returns true if any one of the principals has been included for the operation on the resource.
    /// </summary>
    public bool IsIncluded(ValueType physicalResource, Enum operation, IEnumerable<ValueType> principals)
    {
        var value = !_operations.ContainsKey(physicalResource) ? null : _operations[physicalResource];
        return value != null && value.IsIncluded(operation, principals);
    }

    /// <summary>
    /// Returns the collection of operations corresponding to the resource.
    /// </summary>
    private BaseControlItem GetValue(ValueType physicalResource)
    {
        BaseControlItem value;
        if (!_operations.ContainsKey(physicalResource))
        {
            value = new BaseControlItem(_basePrincipalList);
            _operations.Add(physicalResource, value);
        }
        else
        {
            value = _operations[physicalResource];
        }

        return value;
    }
}