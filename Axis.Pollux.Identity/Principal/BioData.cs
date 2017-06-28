﻿using System;

namespace Axis.Pollux.Identity.Principal
{
    public class BioData: PolluxModel<long>
    {
        #region Names
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        #endregion

        #region Bio
        public DateTime? Dob { get; set; }
        public Gender Gender { get; set; }

        public string Nationality { get; set; }
        public string StateOfOrigin { get; set; }
        #endregion

        #region navigational properties
        public virtual User Owner { get; set; }
        #endregion

        public BioData()
        {
        }
    }

    public enum Gender { Female, Male, Other}
}
