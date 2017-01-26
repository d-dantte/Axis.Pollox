﻿using static Axis.Luna.Extensions.ExceptionExtensions;
using static Axis.Luna.Extensions.ObjectExtensions;

using System;
using System.Linq;
using Axis.Luna;
using Axis.Pollux.RBAC.Auth;
using Axis.Pollux.Identity.Principal;
using System.Collections.Generic;

namespace Axis.Pollux.RBAC.Services
{
    public class RoleAuthority : IUserAuthorization
    {
        private Jupiter.IDataContext _context = null;

        public RoleAuthority(Jupiter.IDataContext dataContext)
        {
            ThrowNullArguments(() => dataContext);

            _context = dataContext;
        }

        #region IUSerAuthentication
        public virtual Operation AssignRole(User user, string role)
            => Operation.Run(() =>
            {
                var userRoleStore = _context.Store<UserRole>();
                var roleStore = _context.Store<Role>();
                if(roleStore.Query.Any(r => r.RoleName == role) &&
                   !userRoleStore.Query.Any(ur => ur.RoleName == role && ur.UserId == user.EntityId))
                {
                    var userRole = userRoleStore.NewObject();
                    userRole.RoleName = role;
                    userRole.UserId = user.UserId;
                    userRoleStore.Add(userRole);
                    _context.CommitChanges();
                }
            });

        public virtual IQueryable<Role> UserRoles(User user)
            => from ur in _context.Store<UserRole>().Query
               join r in _context.Store<Role>().Query on ur.RoleName equals r.EntityId
               where ur.UserId == user.EntityId
               select r;

        public virtual Operation<Role> CreateRole(string name)
            => Operation.Run(() => _context.Store<Role>()
                                           .NewObject()
                                           .With(new { RoleName = name }));

        public virtual Operation DeleteUserRole(User user, Role role)
            => Operation.Run(() =>
            {
                var urstore = _context.Store<UserRole>();
                var ur = urstore.Query.FirstOrDefault(_ur => _ur.UserId == user.EntityId && _ur.RoleName == role.RoleName);
                if (ur != null) urstore.Delete(ur, true);
                else throw new Exception("Failed");
            });

        public virtual Operation AddRole(string role)
            => Operation.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(role)) throw new Exception("Invalid Role Name specified");
                var store = _context.Store<Role>();
                if(!store.Query.Any(r => r.RoleName == role))
                {
                    store.Add(new Role { RoleName = role });
                    _context.CommitChanges();
                }
            });

        public virtual Operation DeleteRole(Role role)
            => Operation.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(role?.RoleName)) throw new Exception("Invalid Role Name specified");
                _context.Store<Role>().Delete(role, true);
            });



        public virtual Operation AuthorizeAccess(PermissionProfile authRequest, Action operation = null)
            => Operation.Try(() =>
            {
                ValidatePermissionProfile(authRequest);
                operation?.Invoke();
            });

        public virtual Operation<T> AuthorizeAccess<T>(PermissionProfile authRequest, Func<T> operation = null)
            => Operation.Try(() =>
            {
                ValidatePermissionProfile(authRequest);
                if (operation != null) return operation.Invoke();
                else return default(T);
            });

        public virtual Operation AuthorizeAccess(PermissionProfile authRequest, Func<Operation> operation = null)
            => Operation.Try(() =>
            {
                ValidatePermissionProfile(authRequest);
                return operation?.Invoke() ?? Operation.NoOp();
            });

        public virtual Operation<T> AuthorizeAccess<T>(PermissionProfile authRequest, Func<Operation<T>> operation = null)
            => Operation.Try(() =>
            {
                ValidatePermissionProfile(authRequest);
                return operation?.Invoke() ?? Operation.NoOp<T>();
            });
        
        #endregion

        /// <summary>
        /// Validates the permission profile by checking that the principal has the right to access all the given resources
        /// </summary>
        /// <param name="authRequest"></param>
        protected void ValidatePermissionProfile(PermissionProfile authRequest)
        {
            GetUserPermissions(authRequest.Principal).Values

                //flatten the permissions
                .SelectMany(_rps => _rps)

                //cross multiply the permissions with the resources in the permission profile, then filter out permissions that dont apply to the resource
                .SelectMany(_rp => authRequest.ResourcePaths.Select(_r => new { Matched = _rp.Resource.Match(_r), Resource = _r, Permission = _rp }).Where(_m => _m.Matched))

                //Group by the resource names (from those that matched)
                .GroupBy(_m => _m.Resource)

                //get the combined effect for each group
                .Select(_mgroup => new { Resource = _mgroup.Key, Effect = _mgroup.Select(_m => _m.Permission.Effect).Combine() })

                //if the permission profile requires that permission to all resources must be granted
                .ThrowIf(_mgroups => authRequest.CombinationMethod == CombinationMethod.All &&
                         (_mgroups.Count() != authRequest.ResourcePaths.Count() || //all resources must be represented by groups
                          _mgroups.Select(_mgroup => _mgroup.Effect).Combine() == Effect.Deny), //all effecs must be Granted
                         "Access Denied")

                //if the permission profile requires that permission to ANY of the given resources is adequate
                .ThrowIf(_mgroups => authRequest.CombinationMethod == CombinationMethod.Any &&
                         !_mgroups.Any(_mgroup => _mgroup.Effect == Effect.Grant), "Access Denied");
        }

        protected virtual Dictionary<string, IEnumerable<Permission>> GetUserPermissions(User user)
        {
            var roles = _context.Store<UserRole>().Query
                                .Where(_ur => _ur.UserId == user.UserId)
                                .Select(_ur => _ur.RoleName)
                                .ToArray();

            return _context.Store<Permission>()
                           .QueryWith(_p => _p.Role)
                           .Where(_p => roles.Contains(_p.Role.RoleName))
                           .GroupBy(_pgroup => _pgroup.Role.RoleName)
                           .Select(_pgroup => new { Key = _pgroup.Key, Permissions = (IEnumerable<Permission>)_pgroup.ToArray() })
                           .ToDictionary(_pmap => _pmap.Key, _pmap => _pmap.Permissions);
        }
    }
}
