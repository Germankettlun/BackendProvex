namespace ProvexApi.Entities.Packinglist
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System;

    public class ViewPLPangea
    {
        [Column("Receiver")]
        public string? Receiver { get; set; }

        [Column("Lot #")]
        public string? LotNumber { get; set; }     // NULL en SQL → string

        [Column("Product Code")]
        public string? ProductCode { get; set; }   // NULL en SQL → string

        [Column("Producto")]
        public string? Producto { get; set; }

        [Column("Variety")]
        public string? Variety { get; set; }

        [Column("Label")]
        public string? Label { get; set; }

        [Column("Dep PORT")]
        public string? DepPORT { get; set; }

        [Column("Arri PORT")]
        public string? ArriPORT { get; set; }

        [Column("Container Nbr.")]
        public string? ContainerNbr { get; set; }

        [Column("Vessel Name")]
        public string? VesselName { get; set; }

        [Column("ETD")]
        public DateTime? ETD { get; set; }        // Puede ser null

        [Column("ETA")]
        public DateTime? ETA { get; set; }        // Puede ser null

        [Column("Vessel  Number")]
        public string? VesselNumber { get; set; }

        [Column("Country of Origin")]
        public string? CountryOfOrigin { get; set; }

        [Column("USDA Seal/ SAG Seal")]
        public string? UsdaSeal { get; set; }      // NULL en SQL → string

        [Column("Size")]
        public string? Size { get; set; }

        [Column("Commodity")]
        public string? Commodity { get; set; }

        [Column("Pallet number")]
        public string? PalletNumber { get; set; }

        [Column("Thermometer No")]
        public string? ThermometerNo { get; set; }

        [Column("Packing Date")]
        public DateTime? PackingDate { get; set; }

        [Column("(21).Carton Weight")]
        public decimal? CartonWeight { get; set; }    // SUM → puede ser null

        [Column("NBoxes")]
        public int? NBoxes { get; set; }              // SUM → puede ser null

        [Column("Pack Style")]
        public string? PackStyle { get; set; }

        [Column("Pack")]
        public string? Pack { get; set; }

        [Column("Category")]
        public string? Category { get; set; }

        [Column("PLU")]
        public string? PLU { get; set; }

        [Column("Grower Name")]
        public string? GrowerName { get; set; }

        [Column("Grower Code")]
        public string? GrowerCode { get; set; }

        [Column("Invoice")]
        public string? Invoice { get; set; } // Si tu vista lo trae, si no, bórralo

        [Column("ShippingCompany")]
        public string? ShippingCompany { get; set; }
    }
}
