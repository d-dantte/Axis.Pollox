﻿namespace Axis.Pollux.Identity.Principal
{
    public class AddressData : PolluxModel<long>
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string Country { get; set; }

        #region navigational properties
        public virtual User Owner { get; set; }
        #endregion

        public AddressData()
        {
        }
    }
}
