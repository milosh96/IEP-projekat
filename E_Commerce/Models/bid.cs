namespace E_Commerce.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("bid")]
    public partial class bid
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid id { get; set; }

        [StringLength(128)]
        public string idUser { get; set; }

        [ForeignKey("auction")]
        public Guid idAuction { get; set; }

        public int? numTokens { get; set; }

        public DateTime? placedAt { get; set; }

        public virtual auction auction { get; set; }

        public virtual User User { get; set; }
    }
}
