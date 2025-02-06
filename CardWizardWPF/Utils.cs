using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardWizardWPF
{
    internal class Utils
    {
        //**********************************************************
        // Function name: CmToDeviceIndependentUnits
        //
        // Purpose: converts centimeters to device independent units
        // 
        // Parameters: centimeters in float form
        //
        // Returns: device independent units
        public double CmToDeviceIndependentUnits(double cm)
        {
            const double cmPerInch = 2.54;
            const double dpi = 96.0; // WPF uses 96 DPI for device-independent units
            return cm * (dpi / cmPerInch);
        }
    }
}
