using SFMC4NET.Attributes;
using System;

namespace Tests.Entities
{
    [DataExtension]
    internal class SFMC4NET_TestDE
    {
        [KeyColumn]
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [DEColumn(Name = "Birthdate")]
        public DateTime? TestTime { get; set; }
    }
}
