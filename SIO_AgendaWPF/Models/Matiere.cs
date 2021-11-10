namespace SIO_AgendaWPF.Models
{
    public class Matiere
    {
        public int Id { get; set; }
        public string Libelle { get; set; }

        public override string ToString()
        {
            return Libelle;
        }
    }
}