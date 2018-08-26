namespace E_Commerce.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
 


    [Table("auction")]
    public partial class auction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public auction()
        {
            bid = new HashSet<bid>();
            User = new HashSet<User>();
        }
        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid id { get; set; }

        [StringLength(50)]
        public string title { get; set; }

        public byte[] picture { get; set; }

        public int? duration { get; set; }

        public double? startPrice { get; set; }

        public double? currentPrice { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? openedAt { get; set; }

        public DateTime? closedAt { get; set; }

        [StringLength(10)]
        public string status { get; set; }

        public Guid bidId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<bid> bid { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> User { get; set; }
    }
}
