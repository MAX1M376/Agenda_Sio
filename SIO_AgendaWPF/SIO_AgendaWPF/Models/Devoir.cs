using System;
using System.Collections;
using System.Collections.Generic;

namespace SIO_AgendaWPF.Models
{
    public class Devoir : IComparer<Devoir>
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
        public DateTime Date { get; set; }
        public Matiere Matiere { get; set; }
        public Classe Classe { get; set; }
        public string Description { get; set; }

        public int Compare(Devoir x, Devoir y)
        {
            int res = x.Date.CompareTo(y.Date);

            if (res == 0)
            {
                res = x.Matiere.Libelle.CompareTo(y.Matiere.Libelle);
            }
            return res;
        }
    }
}
