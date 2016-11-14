using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mofichan.Spec.Diagnostics.Feature
{
    public abstract class BaseScenario : Scenario
    {
        protected BaseScenario(string scenarioTitle = null) : base(scenarioTitle)
        {
        }
    }
}
