//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BusinessLogic.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class TimeSheet
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public System.DateTime Date { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public int ProjectId { get; set; }
        public string Details { get; set; }
        public Nullable<bool> Active { get; set; }
    
        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
    }
}