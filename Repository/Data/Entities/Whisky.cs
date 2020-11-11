using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Data.Entities
{
    public class Whisky
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OriginalId { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; }
        public string Taste { get; set; }
        public string Content { get; set; }
        public string Country { get; set; }
        public bool InStock { get; set; }
        public string Store { get; set; }
        public DateTime CreateDt { get; set; }
        public DateTime ModifyDt { get; set; }
    }
}
