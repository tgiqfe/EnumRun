using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDelivery.Maps
{
    internal class SubCandidate
    {
        public string Name { get; set; }
        public string[] Candidate { get; set; }

        public SubCandidate(params string[] candidate)
        {
            this.Name = candidate[0];
            this.Candidate = candidate;
        }

        public string Check(string text)
        {
            return this.Candidate.Any(x => x.Equals(text, StringComparison.OrdinalIgnoreCase)) ?
                Name : null;
        }
    }
}
