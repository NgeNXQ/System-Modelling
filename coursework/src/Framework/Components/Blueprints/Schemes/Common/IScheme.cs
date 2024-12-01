using CourseWork.Framework.Components.Tasks.Common;
using CourseWork.Framework.Components.Modules.Common;

namespace CourseWork.Framework.Components.Blueprints.Schemes.Common;

internal interface IScheme
{
    public Module? GetNextModule(DummyTask task);
}