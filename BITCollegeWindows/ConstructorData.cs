using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BITCollege_SY.Models;

namespace BITCollegeWindows
{
    /// <summary>
    /// ConstructorData:  This class is used to capture data to be passed
    /// among the windows forms.
    /// Further code to be added.
    /// </summary>
    public class ConstructorData
    {
        public Student Student { get; set; }

        public Registration Registration { get; set; }
    }
}
