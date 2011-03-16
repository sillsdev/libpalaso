using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Migration
{
	public interface IFileVersion
	{
		int GetFileVersion(string pathToFile);
		int StrategyGoodToVersion { get; }
		int StrategyGoodFromVersion { get; }
	}
}
