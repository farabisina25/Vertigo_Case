using Vertigo.Wheel.Core.Wheels;
using Vertigo.Wheel.Presentation.Wheel;

namespace Vertigo.Wheel.Controllers.Mapping
{
    public interface IWheelViewDataProvider
    {
        WheelViewData Create(int zone, WheelDefinition wheel);
    }
}
