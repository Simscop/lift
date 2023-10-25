using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lift.Core.Extensions.ImageMetadata;

public static class PropertyExtension
{
    public static double Width(this Common.ImageMetadata meta) => meta.Resolution?.X ?? -1;
}
