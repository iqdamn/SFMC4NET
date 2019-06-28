using System;
using System.Collections.Generic;
using System.Text;

namespace SFMC4NET.Entities
{
    public interface IFolder
    {
        long Id { get; }
        string Description { get; }
        long EnterpriseId { get; }
        long MemberId { get; }
        string Name { get; }
        long ParentId { get; }
        string CategoryType { get; }
        List<IFolder> Folders { get; set; }
    }
}
