namespace E_Commerce.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tokenOrder")]
    public partial class tokenOrder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid id { get; set; }

        [StringLength(128)]
        public string idUser { get; set; }

        public int? numTokens { get; set; }

        public double? price { get; set; }

        [StringLength(20)]
        public string status { get; set; }

        public virtual User User { get; set; }
    }
}
