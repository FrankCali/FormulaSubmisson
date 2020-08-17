using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormulaSubmission.API.Models
{
    public class QueueMessage
    {
        public Guid FormulaStatusId { get; set; }

        public string Message { get; set; }
    }
}
