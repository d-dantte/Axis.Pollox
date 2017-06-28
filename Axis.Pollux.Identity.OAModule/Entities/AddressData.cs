﻿namespace Axis.Pollux.Identity.OAModule.Entities
{
    public class AddressDataEntity : PolluxEntity<long>
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string Country { get; set; }

        #region navigational properties
        public virtual UserEntity Owner { get; set; }
        public string OwnerId { get; set; }
        #endregion

        public AddressDataEntity()
        {
        }
    }
}