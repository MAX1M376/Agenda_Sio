using System;

namespace SIO_AgendaWPF.Models
{
    class Devoir
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
        public DateTime Date { get; set; }
        public Matiere Matiere { get; set; }
        public Classe Classe { get; set; }
        public string Description { get; set; }
    }
}
