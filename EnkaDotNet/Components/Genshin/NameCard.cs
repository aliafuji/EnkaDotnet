using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
namespace EnkaDotNet.Components.Genshin
{
    public class NameCard
    {
        public string Id { get; set; }
        public string IconUrl { get; set; }
        public NameCard() { }
        public NameCard(int id, string icon)
        {
            Id = id.ToString();
            IconUrl = icon;
        }
    }
}