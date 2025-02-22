using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CardWizardWPF
{
    public class Utils
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

        public double GetCanvasPosition(UIElement element, DependencyProperty property)
        {
            double value = (double)element.GetValue(property);
            return double.IsNaN(value) ? 0 : value; // Default to 0 if NaN
        }
    }
}
