using Application.Common.AccessControl.Models;

namespace Application.Common.AccessControl.Base;

public class BaseControlItem
    {
        /// <summary>
        /// Store a collection of principals for each operation, using the operation as a hash key.
        /// </summary>
        private readonly Dictionary<Enum, IList<ValueType>> _principals;

        private readonly List<ValueType> _basePrincipalList;

        /// <summary>
        /// Disallow a null dictionary.
        /// </summary>
        public BaseControlItem(List<ValueType> basePrincipalList)
        {
            _basePrincipalList = basePrincipalList;
            _principals = new Dictionary<Enum, IList<ValueType>>();
        }

        /// <summary>
        /// Returns true if the dictionary contains a collection of principals for the operation; false otherwise.
        /// </summary>
        public bool Contains(Enum operation)
        {
            return _principals.ContainsKey(operation);
        }

        /// <summary>
        /// Removes a principal from an operation.
        /// </summary>
        public void Exclude(Enum operation, ValueType physicalPrincipal)
        {
            if (_basePrincipalList.Contains(physicalPrincipal))
            {
                physicalPrincipal = _basePrincipalList.Single(x => x.Equals(physicalPrincipal));
            }

            var value = GetValue(operation);
            if (value.Contains(physicalPrincipal))
                value.Remove(physicalPrincipal);
        }

        /// <summary>
        /// Returns only those principals already added to an operation. Given a list of principals, you might want to
        /// know which one(s) have been included for a specific operation.
        /// </summary>
        public IList<ValueType> FindIncludedPrincipals(Enum operation, IEnumerable<ValueType> principals)
        {
            var includedPrincipals = new List<ValueType>();

            if (!_principals.ContainsKey(operation))
                return includedPrincipals;

            var value = _principals[operation];
            includedPrincipals.AddRange(principals.Where(x => value.Contains(x)));

            return includedPrincipals;
        }

        /// <summary>
        /// Adds a principal to an operation.
        /// </summary>
        public void Include(Enum operation, ValueType physicalPrincipal)
        {
            if (!_basePrincipalList.Contains(physicalPrincipal))
            {
                _basePrincipalList.Add(physicalPrincipal);
            }

            physicalPrincipal = _basePrincipalList.Single(x => x.Equals(physicalPrincipal));
            
            var value = GetValue(operation);

            if (!value.Contains(physicalPrincipal))
                value.Add(physicalPrincipal);
        }

        /// <summary>
        /// Returns true if any one of the principals has been included for the operation.
        /// </summary>
        public bool IsIncluded(Enum operation, IEnumerable<ValueType> principals)
        {
            var value = !_principals.ContainsKey(operation) ? null : _principals[operation];
            return value != null && principals.Any(principal => value.Contains(principal));
        }

        /// <summary>
        /// Returns the collection of principals corresponding to the operation.
        /// </summary>
        private IList<ValueType> GetValue(Enum operation)
        {
            IList<ValueType> value;
            if (!_principals.ContainsKey(operation))
            {
                value = new List<ValueType>();
                _principals.Add(operation, value);
            }
            else
            {
                value = _principals[operation];
            }

            return value;
        }
    }