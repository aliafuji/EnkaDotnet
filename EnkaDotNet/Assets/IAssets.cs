using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums;

namespace EnkaDotNet.Assets
{
    public interface IAssets
    {
        GameType GameType { get; }
        string Language { get; }
        string GetText(string hash);
    }
}