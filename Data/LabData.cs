//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;

namespace Data
{
    public partial class LabData
    {
        public int Id { get; set; }
        public int CardholderIndex { get; set; }
        public int LabTypeId { get; set; }
        public byte Source { get; set; }
        public int LabSourceId { get; set; }
        public string Value { get; set; }
        public System.DateTime LabDate { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<System.DateTime> DateAdded { get; set; }
        public string Npi { get; set; }
    }
}