using Coursework.Framework.Components.Common;
using Coursework.Framework.Components.Tasks.Common;
using Coursework.Framework.Components.Modules.Common;

namespace Coursework.Framework.Components.Blueprints.Schemes.Common;

internal abstract class Scheme : Element
{
    private protected Scheme(string identifier) : base(identifier)
    {
    }

    internal abstract Module? GetNextModule(DummyTask task);
}
