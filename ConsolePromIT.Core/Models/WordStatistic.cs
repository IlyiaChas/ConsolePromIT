
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsolePromIT.Core.Models
{
    public class WordStatistic
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Word { get; set; }

        public int Count { get; set; } = 1;
    }
}
